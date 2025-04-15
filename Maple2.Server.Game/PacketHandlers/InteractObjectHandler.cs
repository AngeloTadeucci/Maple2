using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class InteractObjectHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.InteractObject;

    private enum Command : byte {
        Start = 11,
        End = 12,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Start:
                HandleStart(session, packet);
                return;
            case Command.End:
                HandleEnd(session, packet);
                return;
        }
    }

    private void HandleStart(GameSession session, IByteReader packet) {
        string entityId = packet.ReadString();

        if (session.Field?.TryGetInteract(entityId, out FieldInteract? interact) != true) {
            return;
        }
    }

    private void HandleEnd(GameSession session, IByteReader packet) {
        string entityId = packet.ReadString();

        if (session.Field?.TryGetInteract(entityId, out FieldInteract? interact) == true && interact.React()) {
            session.ConditionUpdate(ConditionType.interact_object, codeLong: interact.Object.Id);
            session.ConditionUpdate(ConditionType.interact_object_rep, codeLong: interact.Object.Id);

            switch (interact.Value.Type) {
                case InteractType.Mesh:
                    session.Send(InteractObjectPacket.Interact(interact));
                    break;
                case InteractType.Telescope:
                    session.Send(InteractObjectPacket.Interact(interact));
                    session.Send(InteractObjectPacket.Result(InteractResult.s_interact_find_new_telescope, interact));

                    if (!session.Player.Value.Unlock.InteractedObjects.Contains(interact.Object.Id)) {
                        session.Player.Value.Unlock.InteractedObjects.Add(interact.Object.Id);
                        session.Exp.AddExp(ExpType.telescope);
                    }
                    break;
                case InteractType.Ui:
                    session.Send(InteractObjectPacket.Interact(interact));
                    break;
                case InteractType.Web:
                case InteractType.DisplayImage:
                case InteractType.GuildPoster:
                case InteractType.WatchTower:
                    break;
                case InteractType.BillBoard: // AdBalloon
                    session.Send(PlayerHostPacket.AdBalloonWindow((interact.Object as InteractBillBoardObject)!));
                    break;
                case InteractType.Gathering:
                    session.Mastery.Gather(interact);
                    session.Send(InteractObjectPacket.Result(InteractResult.none, interact));
                    break;
            }

            ICollection<Item> items = new List<Item>();
            if (interact.Value.Drop.IndividualDropBoxIds.Length > 0) {
                foreach (int individualDropBoxId in interact.Value.Drop.IndividualDropBoxIds) {
                    ICollection<Item> individualDropBoxItems = session.Field.ItemDrop.GetIndividualDropItems(individualDropBoxId, interact.Value.Drop.Rarity);
                    if (individualDropBoxItems.Count <= 0) {
                        continue;
                    }
                    items.Add(individualDropBoxItems.ElementAt(Random.Shared.Next(individualDropBoxItems.Count)));
                }
            }

            ICollection<Item> globalDropBoxItems = new List<Item>();
            foreach (int globalDropBoxId in interact.Value.Drop.GlobalDropBoxIds) {
                globalDropBoxItems = session.Field.ItemDrop.GetGlobalDropItems(globalDropBoxId, session.Player.Value.Character.Level);
            }

            foreach (Item item in globalDropBoxItems) {
                FieldItem fieldItem = session.Field.SpawnItem(interact, interact.Position with {
                    Z = interact.Position.Z + interact.Value.Drop.DropHeight,
                }, interact.Rotation, item, session.CharacterId);
                session.Field.Broadcast(FieldPacket.DropItem(fieldItem));
            }

            foreach (Item item in items) {
                FieldItem fieldItem = session.Field.SpawnItem(interact, interact.Position with {
                    Z = interact.Position.Z + interact.Value.Drop.DropHeight,
                }, interact.Rotation, item, session.CharacterId);
                session.Field.Broadcast(FieldPacket.DropItem(fieldItem));
            }

            foreach (InteractObjectMetadataEffect.InvokeEffect invokeEffect in interact.Value.AdditionalEffect.Invoke) {
                if (invokeEffect.Probability < 10000 && invokeEffect.Probability / 100 < Random.Shared.Next(100)) {
                    continue;
                }

                session.Player.Buffs.AddBuff(session.Player, session.Player, invokeEffect.Id, invokeEffect.Level, session.Field.FieldTick, notifyField: true);
            }

            if (interact.Value.AdditionalEffect.ModifyCode > 0) {
                session.ConditionUpdate(ConditionType.interact_object, codeLong: interact.Value.Id);
            }
        }
    }
}
