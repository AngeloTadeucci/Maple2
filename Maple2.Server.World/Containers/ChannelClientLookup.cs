using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Maple2.Server.Core.Constants;
using Serilog;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;

namespace Maple2.Server.World.Containers;

public class ChannelClientLookup : IEnumerable<(int, ChannelClient)> {
#if DEBUG
    private static readonly TimeSpan MonitorInterval = TimeSpan.FromSeconds(1);
#else
    private static readonly TimeSpan MonitorInterval = TimeSpan.FromSeconds(5);
#endif

    private record Entry(IPEndPoint Endpoint, ChannelClient Client, Health.HealthClient Health);

    private enum ChannelStatus {
        Active,
        Pending,
        Inactive
    }

    private readonly List<Entry> channels = [];
    private readonly List<ChannelStatus> activeChannels = [];

    private readonly ILogger logger = Log.ForContext<ChannelClientLookup>();

    public int Count => channels.Count;

    public IEnumerable<int> Keys {
        get {
            for (int i = 0; i < activeChannels.Count; i++) {
                if (activeChannels[i] is ChannelStatus.Active) {
                    yield return i + 1;
                }
            }
        }
    }

    public (ushort gamePort, int grpcPort, int channel) FindOrCreateChannelByIp(string gameIp, string grpcGameIp) {
        for (int i = 0; i < channels.Count; i++) {
            if (channels[i].Endpoint.Address.ToString() == gameIp && activeChannels[i] is ChannelStatus.Inactive) {
                return ((ushort) (Target.BaseGamePort + i + 1), Target.BaseGrpcChannelPort + i + 1, i + 1);
            }
        }

        return AddChannel(gameIp, grpcGameIp);
    }

    public int FirstChannel() {
        for (int i = 0; i < activeChannels.Count; i++) {
            if (activeChannels[i] is ChannelStatus.Active) {
                return i + 1;
            }
        }

        return 0;
    }

    public bool Contains(int channel) {
        return ValidChannel(channel) && activeChannels[channel - 1] is ChannelStatus.Active;
    }

    public bool ValidChannel(int channel) {
        return channel > 0 && channel <= activeChannels.Count;
    }

    public bool TryGetClient(int channel, [NotNullWhen(true)] out ChannelClient? client) {
        if (!ValidChannel(channel)) {
            client = null;
            return false;
        }

        client = channels[channel - 1].Client;
        return true;
    }

    public bool TryGetActiveEndpoint(int channel, [NotNullWhen(true)] out IPEndPoint? endpoint) {
        int i = channel - 1;
        if (!ValidChannel(channel) || activeChannels[i] is ChannelStatus.Inactive) {
            endpoint = null;
            return false;
        }

        endpoint = channels[i].Endpoint;
        return true;
    }

    private (ushort gamePort, int grpcPort, int channel) AddChannel(string gameIp, string grpcGameIp) {
        int channel = channels.Count + 1;

        int newGamePort = Target.BaseGamePort + channel;
        int newGrpcChannelPort = Target.BaseGrpcChannelPort + channel;

        IPAddress ipAddress = IPAddress.Parse(gameIp);
        IPEndPoint gameEndpoint = new IPEndPoint(ipAddress, newGamePort);

        Uri grpcUri;

        bool isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        if (isDocker) {
            // When running in Docker, use the provided grpcGameIp as hostnames
            grpcUri = new Uri($"http://{grpcGameIp}:{newGrpcChannelPort}");
        } else {
            // Outside of Docker, parse the IP addresses normally
            IPAddress grpcIpAddress = IPAddress.Parse(grpcGameIp);
            grpcUri = new Uri($"http://{grpcIpAddress}:{newGrpcChannelPort}");
        }

        GrpcChannel grpcChannel = GrpcChannel.ForAddress(grpcUri);
        var client = new ChannelClient(grpcChannel);
        var healthClient = new Health.HealthClient(grpcChannel);
        channels.Add(new Entry(gameEndpoint, client, healthClient));
        activeChannels.Add(ChannelStatus.Pending);

        var cancel = new CancellationToken();
        Task.Factory.StartNew(() => MonitorChannel(channel, cancel), cancellationToken: cancel);

        return ((ushort) newGamePort, newGrpcChannelPort, channel);
    }

    private async Task MonitorChannel(int channel, CancellationToken cancellationToken) {
        int i = channel - 1;
        logger.Information("Begin monitoring game channel: {Channel} for {EndPoint}", channel, channels[i].Endpoint);
        do {
            try {
                HealthCheckResponse response = await channels[i].Health.CheckAsync(new HealthCheckRequest(), deadline: DateTime.UtcNow.AddSeconds(5),
                    cancellationToken: cancellationToken);
                switch (response.Status) {
                    case HealthCheckResponse.Types.ServingStatus.Serving:
                        if (activeChannels[i] is ChannelStatus.Inactive or ChannelStatus.Pending) {
                            logger.Information("Channel {Channel} has become active", channel);
                            activeChannels[i] = ChannelStatus.Active;
                        }
                        break;
                    default:
                        if (activeChannels[i] is ChannelStatus.Active or ChannelStatus.Pending) {
                            logger.Information("Channel {Channel} has become inactive due to {Status}", channel, response.Status);
                            activeChannels[i] = ChannelStatus.Inactive;
                        }
                        break;
                }
            } catch (RpcException ex) {
                if (ex.Status.StatusCode != StatusCode.Unavailable) {
                    logger.Warning("{Error} monitoring channel {Channel}", ex.Message, channel);
                }
                if (activeChannels[i] is ChannelStatus.Active) {
                    logger.Information("Channel {Channel} has become inactive", channel);
                }
                activeChannels[i] = ChannelStatus.Inactive;
            }

            await Task.Delay(MonitorInterval, cancellationToken);
        } while (!cancellationToken.IsCancellationRequested);
        logger.Error("End monitoring game channel: {Channel} for {EndPoint}", channel, channels[i].Endpoint);
    }

    public IEnumerator<(int, ChannelClient)> GetEnumerator() {
        for (int i = 0; i < channels.Count; i++) {
            if (activeChannels[i] is ChannelStatus.Inactive) {
                continue;
            }

            yield return (i + 1, channels[i].Client);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
