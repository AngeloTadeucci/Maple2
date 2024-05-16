using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Server.Core.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    private readonly record struct TokenEntry(Server Server, long AccountId, long CharacterId, Guid MachineId, int Channel);

    // Duration for which a token remains valid.
    private static readonly TimeSpan AuthExpiry = TimeSpan.FromSeconds(30);

    private readonly IMemoryCache tokenCache;
    // private readonly PlayerChannelLookup playerChannels = new();

    public override Task<MigrateOutResponse> MigrateOut(MigrateOutRequest request, ServerCallContext context) {
        ulong token = UniqueToken();

        switch (request.Server) {
            case Server.Login:
                var longEntry = new TokenEntry(request.Server, request.AccountId, request.CharacterId, new Guid(request.MachineId), 0);
                tokenCache.Set(token, longEntry, AuthExpiry);
                return Task.FromResult(new MigrateOutResponse {
                    IpAddress = Target.LoginIp.ToString(),
                    Port = Target.LoginPort,
                    Token = token,
                });
            case Server.Game: {
                    if (channelClients.Count == 0) {
                        throw new RpcException(new Status(StatusCode.Unavailable, $"No available game channels"));
                    }

                    int channel = request.HasChannel ? request.Channel : channelClients.FirstChannel();
                    if (!channelClients.TryGetActiveEndpoint(channel, out IPEndPoint? endpoint)) {
                        throw new RpcException(new Status(StatusCode.InvalidArgument, $"Migrating to invalid game channel: {channel}"));
                    }

                    var gameEntry = new TokenEntry(request.Server, request.AccountId, request.CharacterId, new Guid(request.MachineId), channel);
                    tokenCache.Set(token, gameEntry, AuthExpiry);
                    return Task.FromResult(new MigrateOutResponse {
                        IpAddress = endpoint.Address.ToString(),
                        Port = endpoint.Port,
                        Token = token,
                        Channel = channel,
                    });
                }
            default:
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid server: {request.Server}"));
        }
    }

    public override Task<MigrateInResponse> MigrateIn(MigrateInRequest request, ServerCallContext context) {
        if (!tokenCache.TryGetValue(request.Token, out TokenEntry data)) {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
        }

        if (data.AccountId != request.AccountId) {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "Invalid token for account"));
        }
        if (data.MachineId != new Guid(request.MachineId)) {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Mismatched machineId for account"));
        }

        tokenCache.Remove(request.Token);
        return Task.FromResult(new MigrateInResponse { CharacterId = data.CharacterId, Channel = data.Channel });
    }

    // Generates a 64-bit token that does not exist in cache.
    private ulong UniqueToken() {
        ulong token;
        do {
            token = BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(8));
        } while (tokenCache.TryGetValue(token, out _));

        return token;
    }
}
