using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class SkillBookPacket {
    private enum Command : byte {
        Load = 0,
        Save = 1,
        Rename = 2,
        Expand = 4,
    }

    public static ByteWriter Load(SkillBook skillBook) {
        var pWriter = Packet.Of(SendOp.SkillBookTree);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(skillBook.MaxSkillTabs);
        pWriter.WriteLong(skillBook.ActiveSkillTabId);

        pWriter.WriteInt(skillBook.SkillTabs.Count);
        foreach (SkillTab skillTab in skillBook.SkillTabs) {
            pWriter.WriteClass<SkillTab>(skillTab);
        }

        return pWriter;
    }

    public static ByteWriter Save(SkillBook skillBook, long savedTabId, SkillRank ranksSaved) {
        var pWriter = Packet.Of(SendOp.SkillBookTree);
        pWriter.Write<Command>(Command.Save);
        pWriter.WriteLong(skillBook.ActiveSkillTabId);
        pWriter.WriteLong(savedTabId);
        pWriter.WriteInt((int) ranksSaved);

        return pWriter;
    }

    public static ByteWriter Rename(SkillTab skillTab, bool error = false) {
        var pWriter = Packet.Of(SendOp.SkillBookTree);
        pWriter.Write<Command>(Command.Rename);
        pWriter.WriteLong(skillTab.Id);
        pWriter.WriteUnicodeString(skillTab.Name);
        pWriter.WriteBool(error); // true -> s_ban_check_err_any_word

        return pWriter;
    }

    public static ByteWriter Expand(SkillBook skillBook) {
        var pWriter = Packet.Of(SendOp.SkillBookTree);
        pWriter.Write<Command>(Command.Expand);
        pWriter.WriteInt(skillBook.MaxSkillTabs);

        return pWriter;
    }
}
