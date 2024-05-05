using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class NpcTalkPacket {
    private enum Command : byte {
        Close = 0,
        Respond = 1,
        Continue = 2,
        Action = 3,
        Update = 4,
        AllianceTalk = 9,
    }

    public static ByteWriter Close() {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Close);

        return pWriter;
    }

    public static ByteWriter Respond(FieldNpc npc, NpcTalkType type, NpcDialogue dialogue) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Respond);
        pWriter.WriteInt(npc.ObjectId);
        pWriter.Write<NpcTalkType>(type);
        pWriter.Write<NpcDialogue>(dialogue);

        return pWriter;
    }

    public static ByteWriter Continue(NpcTalkType type, NpcDialogue dialogue, int questId = 0) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Continue);
        pWriter.Write<NpcTalkType>(type);
        pWriter.WriteInt(questId);
        pWriter.Write<NpcDialogue>(dialogue);

        return pWriter;
    }

    public static ByteWriter MovePlayer(int portalId) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Action);
        pWriter.Write<NpcTalkAction>(NpcTalkAction.MovePlayer);
        pWriter.WriteInt(portalId);

        return pWriter;
    }

    public static ByteWriter OpenDialog(string name, string tags) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Action);
        pWriter.Write<NpcTalkAction>(NpcTalkAction.OpenDialog);
        pWriter.WriteUnicodeString(name);
        pWriter.WriteUnicodeString(tags);

        return pWriter;
    }

    public static ByteWriter RewardItem(IList<Item> items) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Action);
        pWriter.Write<NpcTalkAction>(NpcTalkAction.RewardItem);
        pWriter.WriteInt(items.Count);
        foreach (Item item in items) {
            pWriter.WriteInt(item.Id);
            pWriter.WriteByte((byte) item.Rarity);
            pWriter.WriteInt(item.Amount);
            pWriter.WriteClass<Item>(item);
        }

        return pWriter;
    }

    public static ByteWriter RewardExp(long exp) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Action);
        pWriter.Write<NpcTalkAction>(NpcTalkAction.RewardExp);
        pWriter.WriteLong(exp);

        return pWriter;
    }

    public static ByteWriter RewardMeso(long mesos) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Action);
        pWriter.Write<NpcTalkAction>(NpcTalkAction.RewardMeso);
        pWriter.WriteLong(mesos);

        return pWriter;
    }

    public static ByteWriter Cutscene(string movieString) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Action);
        pWriter.Write<NpcTalkAction>(NpcTalkAction.Cutscene);
        pWriter.WriteUnicodeString(movieString);

        return pWriter;
    }

    public static ByteWriter Update(string text, string voiceId = "", string illustration = "") {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteUnicodeString(text);
        pWriter.WriteUnicodeString(voiceId);
        pWriter.WriteUnicodeString(illustration);

        return pWriter;
    }

    public static ByteWriter AllianceTalk(NpcTalkType type, NpcDialogue dialogue) {
        var pWriter = Packet.Of(SendOp.NpcTalk);
        pWriter.Write<Command>(Command.AllianceTalk);
        pWriter.Write<NpcTalkType>(type);
        pWriter.Write<NpcDialogue>(dialogue);

        return pWriter;
    }
}
