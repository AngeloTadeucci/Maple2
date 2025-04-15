using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Core.Sync;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util.Sync;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class RideManager {
    private readonly GameSession session;
    public Ride? Ride { get; set; }
    public RideOnType RideType => Ride?.Action.Type ?? RideOnType.None;


    private readonly ILogger logger = Log.Logger.ForContext<RideManager>();

    public RideManager(GameSession session) {
        this.session = session;
    }

    public bool Mount(Item item) {
        if (!session.RideMetadata.TryGet(item.Metadata.Property.Ride, out RideMetadata? metadata)) {
            session.Send(NoticePacket.MessageBox(StringCode.s_item_invalid_function_item));
            return false;
        }

        if (item.Metadata.Limit.TransferType == TransferType.BindOnUse) {
            session.Item.Bind(item);
        }

        int objectId = FieldManager.NextGlobalId();
        var action = new RideOnActionDefault(item.Metadata.Property.Ride, objectId, item);
        Ride = new Ride(session.Player.ObjectId, metadata, action);
        session.Field.Broadcast(RidePacket.Start(Ride));
        return true;
    }

    public bool Mount(AdditionalEffectMetadata additionalEffectMetadata) {
        if (Ride != null) {
            return false;
        }

        if (!session.Field.RideMetadata.TryGet(additionalEffectMetadata.Property.RideId, out RideMetadata? metadata)) {
            return false;
        }

        var action = new RideOnActionBattle(metadata.Id, FieldManager.NextGlobalId(), additionalEffectMetadata.Id, additionalEffectMetadata.Level);

        Ride = new Ride(session.Player.ObjectId, metadata, action);
        session.Field.Broadcast(RidePacket.Start(Ride));
        if (Ride.Metadata.Stats.Count > 0) {
            session.Stats.SetBattleMountStats(Ride.Metadata.Stats);
            session.Field.Broadcast(StatsPacket.Init(session.Player));
        }
        return true;

    }

    public void Dismount(RideOffType type, bool forced = false) {
        if (Ride == null) {
            return;
        }

        RideOffAction rideOffAction = GetRideOffAction(type, forced);
        session.Field.Broadcast(RidePacket.Stop(Ride.OwnerId, rideOffAction));

        Ride ride = Ride;
        Ride = null;

        if (ride.Action is RideOnActionBattle battle) {
            session.Buffs.Remove(battle.SkillId, session.Player.ObjectId);
        }
        if (ride.Metadata.Stats.Count > 0) {
            session.Stats.Refresh();
        }
        session.Config.LoadKeyTable();
    }

    private RideOffAction GetRideOffAction(RideOffType type, bool forced = false) {
        if (forced) {
            return new RideOffAction(forced);
        }

        return type switch {
            RideOffType.Default => new RideOffAction(forced),
            RideOffType.UseSkill => new RideOffActionUseSkill(),
            RideOffType.Interact => new RideOffActionInteract(),
            RideOffType.Taxi => new RideOffActionTaxi(),
            RideOffType.CashCall => new RideOffActionCashCall(),
            RideOffType.BeautyShop => new RideOffActionBeautyShop(),
            RideOffType.TakeLr => new RideOffActionTakeLr(),
            RideOffType.Hold => new RideOffActionHold(),
            RideOffType.Recall => new RideOffActionRecall(),
            RideOffType.SummonPetOn => new RideOffActionSummonPetOn(),
            RideOffType.SummonPetTransfer => new RideOffActionSummonPetTransfer(),
            RideOffType.HomeConvenient => new RideOffActionHomeConvenient(),
            RideOffType.DisableField => new RideOffActionDisableField(),
            RideOffType.Dead => new RideOffActionDead(),
            RideOffType.AdditionalEffect => new RideOffActionAdditionalEffect(),
            RideOffType.RidingUi => new RideOffActionRidingUi(),
            RideOffType.Homemade => new RideOffActionHomemade(),
            RideOffType.AutoInteraction => new RideOffActionAutoInteraction(),
            RideOffType.AutoClimb => new RideOffActionAutoClimb(),
            RideOffType.CoupleEmotion => new RideOffActionCoupleEmotion(),
            //RideOffType.React => new RideOffActionReact(),
            RideOffType.UseFunctionItem => new RideOffActionUseFunctionItem(),
            RideOffType.Nurturing => new RideOffActionNurturing(),
            RideOffType.Groggy => new RideOffActionGroggy(),
            RideOffType.UnRideSkill => new RideOffActionUnRideSkill(),
            RideOffType.UseGlideItem => new RideOffActionUseGlideItem(),
            RideOffType.HideAndSeek => new RideOffActionHideAndSeek(),
            _ => new RideOffAction(forced),
        };
    }
}
