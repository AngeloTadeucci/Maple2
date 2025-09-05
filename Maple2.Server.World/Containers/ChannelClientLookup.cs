﻿using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Maple2.Database.Extensions;
using Maple2.Model.Game;
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

    private WorldServer worldServer = null!;
    private PlayerInfoLookup playerInfoLookup = null!;

    private enum ChannelStatus {
        Active,
        Inactive,
        Pending,
    }

    public void InjectDependencies(WorldServer worldSv, PlayerInfoLookup playerInfo) {
        worldServer = worldSv;
        playerInfoLookup = playerInfo;
    }

    private readonly ConcurrentDictionary<int, Channel> channels = [];

    private readonly ILogger logger = Log.ForContext<ChannelClientLookup>();

    public int Count => channels.Values.Count(ch => ch is { InstancedContent: false, Status: ChannelStatus.Active });

    public IEnumerable<int> Keys {
        get {
            foreach (Channel channel in channels.Values.Where(ch => ch is { InstancedContent: false, Status: ChannelStatus.Active })) {
                yield return channel.Id;
            }
        }
    }

    public (ushort gamePort, int grpcPort, int channel) FindOrCreateChannelByIp(string gameIp, string grpcGameIp, bool instancedContent) {
        // find the first channel that matches the ip, status and instanced content, but only if not Pending
        Channel? activeChannel = channels.Values.FirstOrDefault(channel =>
            channel.Endpoint.Address.ToString() == gameIp &&
            channel.Status is ChannelStatus.Inactive &&
            channel.InstancedContent == instancedContent);

        if (activeChannel is not null) {
            // Mark as pending before returning
            activeChannel.Status = ChannelStatus.Pending;
            return (activeChannel.GamePort, activeChannel.GrpcPort, activeChannel.Id);
        }

        return AddChannel(gameIp, grpcGameIp, instancedContent);
    }

    /// Gets the ID of the first active, non-instanced content channel.
    /// <returns>
    /// The channel ID if an active, non-instanced content channel is found; otherwise returns -1.
    /// </returns>
    public int FirstChannel() {
        foreach (Channel channel in channels.Values.Where(ch => ch.Status is ChannelStatus.Active && !ch.InstancedContent)) {
            return channel.Id;
        }

        return -1;
    }

    public bool TryGetInstancedChannelId(out int channelId) {
        foreach (Channel channel in channels.Values.Where(ch => ch.Status is ChannelStatus.Active && ch.InstancedContent)) {
            channelId = channel.Id;
            return true;
        }

        channelId = -1;
        return false;
    }

    public bool ValidChannel(int channel) {
        return channel >= 0 && channels.ContainsKey(channel);
    }

    public bool TryGetClient(int channel, [NotNullWhen(true)] out ChannelClient? client) {
        if (!ValidChannel(channel)) {
            client = null;
            return false;
        }

        client = channels[channel].Client;
        return true;
    }

    public bool TryGetActiveEndpoint(int channelId, [NotNullWhen(true)] out IPEndPoint? endpoint) {
        if (!ValidChannel(channelId) || !channels.TryGetValue(channelId, out Channel? channel) || channel.Status is ChannelStatus.Inactive) {
            endpoint = null;
            return false;
        }

        endpoint = channel.Endpoint;
        return true;
    }

    private (ushort gamePort, int grpcPort, int channel) AddChannel(string gameIp, string grpcGameIp, bool instancedContent) {
        int candidate = instancedContent ? 0 : 1;
        int attempts = 0;

        while (true) {
            attempts++;
            int channelId = candidate;
            int newGamePort = Target.BaseGamePort + channelId;
            int newGrpcChannelPort = Target.BaseGrpcChannelPort + channelId;

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
            var activeChannel = new Channel(ChannelStatus.Pending, channelId, instancedContent, gameEndpoint, client, healthClient, (ushort)newGamePort, newGrpcChannelPort);

            if (channels.TryAdd(channelId, activeChannel)) {
                var cancel = new CancellationTokenSource();
                Task.Factory.StartNew(() => MonitorChannel(activeChannel, cancel), cancellationToken: cancel.Token);
                return ((ushort)newGamePort, newGrpcChannelPort, channelId);
            }

            // If instanced content, id=0 is unique; no alternative available.
            if (instancedContent) {
                logger.Error("Failed to add instanced channel (ID 0) due to conflict.");
                return (0, 0, -1);
            }

            // Try next available ID to avoid races when multiple game channels start concurrently.
            candidate++;
            if (attempts > 100) {
                logger.Error("Failed to allocate a channel after {Attempts} attempts", attempts);
                return (0, 0, -1);
            }
        }
    }

    private async Task MonitorChannel(Channel channel, CancellationTokenSource cancellationTokenSource) {
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        logger.Information("Begin monitoring game channel: {Channel} for {EndPoint}", channel.Id, channel.Endpoint);

        do {
            try {
                HealthCheckResponse response = await channel.Health.CheckAsync(new HealthCheckRequest(), deadline: DateTime.UtcNow.AddSeconds(5),
                    cancellationToken: cancellationToken);
                switch (response.Status) {
                    case HealthCheckResponse.Types.ServingStatus.Serving:
                        if (channel.Status is ChannelStatus.Inactive || channel.Status is ChannelStatus.Pending) {
                            logger.Information("Channel {Channel} has become active", channel.Id);
                            Active(channel);
                        }
                        break;
                    default:
                        if (channel.Status is ChannelStatus.Active || channel.Status is ChannelStatus.Pending) {
                            Inactive(channel);
                            logger.Information("Channel {Channel} has become inactive due to {Status}", channel.Id, response.Status);
                        }
                        break;
                }
            } catch (RpcException ex) {
                if (ex.Status.StatusCode != StatusCode.Unavailable) {
                    logger.Warning("{Error} monitoring channel {Channel}", ex.Message, channel.Id);
                }
                if (channel.Status is ChannelStatus.Active) {
                    logger.Information("Channel {Channel} has become inactive", channel.Id);
                    Inactive(channel);
                }
                channel.Status = ChannelStatus.Inactive;
            }

            try {
                await Task.Delay(MonitorInterval, cancellationToken);
            } catch (OperationCanceledException) {
                // Handle cancellation gracefully
                break;
            }
        } while (!cancellationToken.IsCancellationRequested);
        logger.Warning("End monitoring game channel: {Channel} for {EndPoint}", channel.Id, channel.Endpoint);
        channels.TryRemove(channel.Id, out _);
    }

    public IEnumerator<(int, ChannelClient)> GetEnumerator() {
        foreach (Channel channel in channels.Values.Where(ch => ch.Status is ChannelStatus.Active)) {
            yield return (channel.Id, channel.Client);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    private void Inactive(Channel channel) {
        channel.Status = ChannelStatus.Inactive;
        UpdateAllPlayersToOffline(channel.Id);
        UpdateChannels(channel.Id);
    }

    private void Active(Channel channel) {
        channel.Status = ChannelStatus.Active;
        UpdateChannels(channel.Id);
        // Load custom string boards
        foreach ((int id, string message) in worldServer.GetCustomStringBoards()) {
            channel.Client.Admin(new AdminRequest {
                AddStringBoard = new AdminRequest.Types.AddStringBoard {
                    Id = id,
                    Message = message,
                },
            });
        }
    }

    private void UpdateAllPlayersToOffline(int channelId) {
        foreach (PlayerInfo playerInfo in playerInfoLookup.GetPlayersOnChannel(channelId)) {
            playerInfo.Channel = -1;
            foreach ((int id, ChannelClient channelClient) in this) {
                if (id == channelId) {
                    continue;
                }
                channelClient.UpdatePlayer(new PlayerUpdateRequest {
                    AccountId = playerInfo.AccountId,
                    CharacterId = playerInfo.CharacterId,
                    LastOnlineTime = DateTime.UtcNow.ToEpochSeconds(),
                    Channel = -1,
                    Async = true,
                });
            }
        }
    }

    private void UpdateChannels(int exclueChannelBroadcast = -1) {
        foreach ((int id, ChannelClient channelClient) in this) {
            if (id == exclueChannelBroadcast) {
                continue;
            }

            List<int> channelList = channels.Values.Where(ch => ch.Status is ChannelStatus.Active && !ch.InstancedContent).Select(ch => ch.Id).ToList();
            channelClient.UpdateChannels(new Maple2.Server.Channel.Service.ChannelsUpdateRequest {
                Channels = {
                    channelList,
                },
            });
        }
    }

    private class Channel {
        public ChannelStatus Status { get; set; }

        public readonly int Id;
        public readonly bool InstancedContent;

        public readonly IPEndPoint Endpoint;
        public readonly ushort GamePort;
        public readonly int GrpcPort;

        public readonly ChannelClient Client;
        public readonly Health.HealthClient Health;

        public Channel(ChannelStatus status, int id, bool instancedContent, IPEndPoint endpoint, ChannelClient client, Health.HealthClient health, ushort gamePort, int grpcPort) {
            Status = status;
            Id = id;
            InstancedContent = instancedContent;
            Endpoint = endpoint;
            Client = client;
            Health = health;
            GamePort = gamePort;
            GrpcPort = grpcPort;
        }
    }
}
