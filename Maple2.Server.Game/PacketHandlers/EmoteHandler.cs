using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class EmoteHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Emote;

    private enum Command : byte {
        Learn = 1,
        Use = 2,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Learn:
                HandleLearn(session, packet);
                return;
            case Command.Use:
                HandleUse(session, packet);
                return;
        }
    }

    private void HandleLearn(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();

        Item? item = session.Item.Inventory.Get(itemUid);
        if (item?.Metadata.Skill == null || !item.IsEmote()) {
            session.Send(EmotePacket.Error(EmoteError.s_dynamic_action_item_invalid));
            return;
        }

        int emoteId = item.Metadata.Skill.Id;
        if (session.Player.Value.Unlock.Emotes.Contains(emoteId)) {
            session.Send(EmotePacket.Error(EmoteError.s_dynamic_action_already_learn));
            return;
        }

        // Now that we know this item is valid and is an unlearned emote, try to consume it.
        if (!session.Item.Inventory.Consume(itemUid, 1)) {
            session.Send(EmotePacket.Error(EmoteError.s_dynamic_action_item_invalid));
            return;
        }

        session.Player.Value.Unlock.Emotes.Add(emoteId);
        session.Send(EmotePacket.Learn(new Emote(emoteId)));
    }

    private void HandleUse(GameSession session, IByteReader packet) {
        int emoteId = packet.ReadInt();
        string aniKey = packet.ReadUnicodeString();

        if (!session.Player.Value.Unlock.Emotes.Contains(emoteId)) {
            // Not sure if there is a way to prevent an emote from being used.
            return;
        }

        session.ConditionUpdate(ConditionType.emotion, codeString: aniKey);
    }
}
