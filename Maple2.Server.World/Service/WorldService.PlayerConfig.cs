using Grpc.Core;


namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<PlayerConfigResponse> PlayerConfig(PlayerConfigRequest request, ServerCallContext context) {
        switch (request.BuffCase) {
            case PlayerConfigRequest.BuffOneofCase.Get:
                return Task.FromResult(Get(request.Get, request.RequesterId));
            case PlayerConfigRequest.BuffOneofCase.Save:
                return Task.FromResult(Save(request.Save, request.RequesterId));
            default:
                return Task.FromResult(new PlayerConfigResponse());
        }
    }

    private PlayerConfigResponse Get(PlayerConfigRequest.Types.Get get, long requesterId) {
        (List<BuffInfo> buffs, List<SkillCooldownInfo> skillCooldowns) = playerConfigLookUp.Retrieve(requesterId);
        return new PlayerConfigResponse {
            Buffs = {
                buffs.Select(b => new BuffInfo {
                    Id = b.Id,
                    Stacks = b.Stacks,
                    Enabled = b.Enabled,
                    Level = b.Level,
                    MsRemaining = b.MsRemaining,
                    StopTime = b.StopTime,
                }),
            },
            SkillCooldowns = {
                skillCooldowns.Select(c => new SkillCooldownInfo {
                    SkillId = c.SkillId,
                    SkillLevel = c.SkillLevel,
                    GroupId = c.GroupId,
                    MsRemaining = c.MsRemaining,
                    StopTime = c.StopTime,
                    Charges = c.Charges,
                }),
            },
        };
    }

    private PlayerConfigResponse Save(PlayerConfigRequest.Types.Save save, long requesterId) {
        playerConfigLookUp.Save(save.Buffs.ToList(), save.SkillCooldowns.ToList(), requesterId);
        return new PlayerConfigResponse();
    }
}
