using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;
using Enum = System.Enum;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<PlayerInfoResponse> PlayerInfo(PlayerInfoRequest request, ServerCallContext context) {
        if (request is { CharacterId: <= 0 }) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"AccountId and CharacterId not specified"));
        }

        if (!playerLookup.TryGet(request.CharacterId, out PlayerInfo? info)) {
            throw new RpcException(new Status(StatusCode.NotFound, $"Invalid character: {request.CharacterId}"));
        }

        return Task.FromResult(PlayerInfoResponse(info));
    }

    public override Task<PlayerInfoResponse> AccountInfo(PlayerInfoRequest request, ServerCallContext context) {
        if (request is { AccountId: <= 0 }) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"AccountId not specified"));
        }

        if (!playerLookup.TryGetByAccountId(request.AccountId, out PlayerInfo? info)) {
            return Task.FromResult(new PlayerInfoResponse());
        }

        return Task.FromResult(PlayerInfoResponse(info));
    }

    public override Task<PlayerUpdateResponse> UpdatePlayer(PlayerUpdateRequest request, ServerCallContext context) {
        if (request is { HasGender: true, Gender: not (0 or 1) }) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Player updated with invalid gender: {request.Gender}"));
        }
        if (request.HasJob && !Enum.IsDefined((Job) request.Job)) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Player updated with invalid job: {request.Job}"));
        }

        // TODO: How should we handle failed updated?
        playerLookup.Update(request);
        return Task.FromResult(new PlayerUpdateResponse());
    }

    public override Task<MailNotificationResponse> MailNotification(MailNotificationRequest request, ServerCallContext context) {
        if (request is { CharacterId: <= 0, AccountId: <= 0 }) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "AccountId and CharacterId not specified"));
        }

        if (!playerLookup.TryGet(request.CharacterId, out PlayerInfo? info)) {
            if (!playerLookup.TryGetByAccountId(request.AccountId, out info)) {
                return Task.FromResult(new MailNotificationResponse());
            }
        }

        int channel = info.Channel;
        if (!channelClients.TryGetClient(channel, out ChannelClient? channelClient)) {
            logger.Error("No registry for channel: {Channel}", channel);
            return Task.FromResult(new MailNotificationResponse());
        }

        if (request.CharacterId <= 0) {
            request.CharacterId = info.CharacterId;
        }

        try {
            return Task.FromResult(channelClient.MailNotification(request));
        } catch (RpcException ex) when (ex.StatusCode is StatusCode.NotFound) {
            logger.Information("{CharacterId} not found...", request.CharacterId);
            return Task.FromResult(new MailNotificationResponse());
        }
    }

    public override Task<DisconnectResponse> Disconnect(DisconnectRequest request, ServerCallContext context) {
        if (request is { CharacterId: <= 0 }) {
            logger.Information("Disconnect request with no character id.");
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"CharacterId not specified"));
        }

        if (!playerLookup.TryGet(request.CharacterId, out PlayerInfo? info) || info is { Online: false }) {
            logger.Error("Unable to find player info for character: {CharacterId}", request.CharacterId);
            throw new RpcException(new Status(StatusCode.NotFound, $"Unable to find: {request.CharacterId}"));
        }

        if (!channelClients.TryGetClient(info.Channel, out ChannelClient? channelClient)) {
            logger.Error("No registry for channel: {Channel}", info.Channel);
            return Task.FromResult(new DisconnectResponse());
        }

        DisconnectResponse disconnectResponse = channelClient.Disconnect(new DisconnectRequest {
            CharacterId = info.CharacterId,
        });

        if (!disconnectResponse.Success) {
            logger.Information("Disconnect failed for character: {CharacterId}", info.CharacterId);
            worldServer.SetOffline(info);
            return Task.FromResult(new DisconnectResponse {
                Success = true,
            });
        }

        logger.Information("Disconnect successful for character: {CharacterId}", info.CharacterId);
        return Task.FromResult(disconnectResponse);
    }

    private static PlayerInfoResponse PlayerInfoResponse(PlayerInfo info) {
        return new PlayerInfoResponse {
            AccountId = info.AccountId,
            CharacterId = info.CharacterId,
            UpdateTime = info.UpdateTime,
            Name = info.Name,
            Motto = info.Motto,
            Picture = info.Picture,
            Gender = (int) info.Gender,
            Job = (int) info.Job,
            Level = info.Level,
            GearScore = info.GearScore,
            PremiumTime = info.PremiumTime,
            LastOnlineTime = info.LastOnlineTime,
            MapId = info.MapId,
            Channel = info.Channel,
            Health = new HealthUpdate {
                CurrentHp = info.CurrentHp,
                TotalHp = info.TotalHp,
            },
            DeathState = (int) info.DeathState,
            MentorRole = (int) info.MentorRole,
            GuildName = info.GuildName,
            GuildId = info.GuildId,
            Home = new HomeUpdate {
                Name = info.HomeName,
                MapId = info.PlotMapId,
                PlotNumber = info.PlotNumber,
                ApartmentNumber = info.ApartmentNumber,
                ExpiryTime = new Timestamp {
                    Seconds = info.PlotExpiryTime,
                },
            },
            Trophy = new TrophyUpdate {
                Combat = info.AchievementInfo.Combat,
                Adventure = info.AchievementInfo.Adventure,
                Lifestyle = info.AchievementInfo.Lifestyle,
            },
            Clubs = {
                info.ClubIds.Select(id => new ClubUpdate {
                    Id = id,
                }),
            },
            DungeonEnterLimits = {
                info.DungeonEnterLimits.Select(dungeon => new DungeonEnterLimitUpdate {
                    DungeonId = dungeon.Key,
                    Limit = (int) dungeon.Value,
                }),
            },
        };
    }
}
