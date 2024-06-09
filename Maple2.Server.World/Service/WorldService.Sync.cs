using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;
using Enum = System.Enum;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<PlayerInfoResponse> PlayerInfo(PlayerInfoRequest request, ServerCallContext context) {
        if (request.CharacterId <= 0) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"AccountId and CharacterId not specified"));
        }

        if (!playerLookup.TryGet(request.CharacterId, out PlayerInfo? info)) {
            throw new RpcException(new Status(StatusCode.NotFound, $"Invalid character: {request.CharacterId}"));
        }

        return Task.FromResult(new PlayerInfoResponse {
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
            LoginTime = info.LoginTime,
            MapId = info.MapId,
            Channel = info.Channel,
            Health = new HealthUpdate {
                CurrentHp = info.CurrentHp,
                TotalHp = info.TotalHp,
            },
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
            Clubs = { info.ClubIds.Select(id => new ClubUpdate { Id = id }) },
        });
    }

    public override Task<PlayerUpdateResponse> UpdatePlayer(PlayerUpdateRequest request, ServerCallContext context) {
        if (request.HasGender && request.Gender is not 0 or 1) {
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
        if (!playerLookup.TryGet(request.CharacterId, out PlayerInfo? info)) {
            return Task.FromResult(new MailNotificationResponse());
        }

        int channel = info.Channel;
        if (!channelClients.TryGetClient(channel, out ChannelClient? channelClient)) {
            logger.Error("No registry for channel: {Channel}", channel);
            return Task.FromResult(new MailNotificationResponse());
        }

        try {
            return Task.FromResult(channelClient.MailNotification(request));
        } catch (RpcException ex) when (ex.StatusCode is StatusCode.NotFound) {
            logger.Information("{CharacterId} not found...", request.CharacterId);
            return Task.FromResult(new MailNotificationResponse());
        }
    }
}
