using Maple2.Database.Extensions;
using Maple2.Model;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Core.Packets.Helper;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class PlayerInfoPacket {
    public static ByteWriter NotFound(long characterId) {
        var pWriter = Packet.Of(SendOp.CharacterInfo);
        pWriter.WriteLong(characterId);
        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter Load(GameSession session) {
        var pWriter = Packet.Of(SendOp.CharacterInfo);
        pWriter.WriteLong(session.Player.Value.Character.Id);
        pWriter.WriteBool(true);
        pWriter.WriteLong(); // Unknown (AccountId probably, but why is it not set?)
        pWriter.WriteLong(session.Player.Value.Character.Id);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        using (var buffer = new PoolByteWriter()) {
            buffer.WriteDetailsBuffer(session.Player);

            pWriter.WriteInt(buffer.Length);
            pWriter.WriteBytes(buffer.Buffer, 0, buffer.Length);
        }

        using (var buffer = new PoolByteWriter()) {
            int count = session.Item.Equips.Gear.Count + session.Item.Equips.Outfit.Count;
            buffer.WriteByte((byte) count);
            foreach (Item item in session.Item.Equips.Gear.Values) {
                buffer.WriteEquip(item);
            }
            foreach (Item item in session.Item.Equips.Outfit.Values) {
                buffer.WriteEquip(item);
            }

            // Outfit 2 from KMS2
            buffer.WriteBool(true); //has Skin
            buffer.WriteLong();
            buffer.WriteLong();
            // Outfit2
            buffer.WriteByte(); // Skin Count
            for (int i = 0; i < 0; i++) {
                // buffer.WriteEquip();
            }

            pWriter.WriteInt(buffer.Length);
            pWriter.WriteBytes(buffer.Buffer, 0, buffer.Length);
        }

        using (var buffer = new PoolByteWriter()) {
            buffer.WriteByte((byte) session.Item.Equips.Badge.Count);
            foreach (Item item in session.Item.Equips.Badge.Values) {
                buffer.WriteBadge(item);
            }

            pWriter.WriteInt(buffer.Length);
            pWriter.WriteBytes(buffer.Buffer, 0, buffer.Length);
        }

        return pWriter;
    }

    private static void WriteDetailsBuffer(this PoolByteWriter buffer, FieldPlayer fieldPlayer) {
        Player player = fieldPlayer;
        buffer.WriteLong(player.Account.Id);
        buffer.WriteLong(player.Character.Id);
        buffer.WriteUnicodeString(player.Character.Name);
        buffer.WriteShort(player.Character.Level);
        buffer.WriteInt((int) player.Character.Job.Code());
        buffer.Write<Job>(player.Character.Job);
        buffer.WriteInt((int) player.Character.Gender);
        buffer.WriteInt(player.Account.PrestigeLevel);
        buffer.WriteByte();

        #region Stats
        for (int i = 0; i < Stat.TOTAL; i++) {
            for (int j = 0; j < Stats.BASIC_TOTAL; j++) {
                buffer.WriteLong(fieldPlayer.Stats.Values[(BasicAttribute) j][i]);
            }
        }
        for (int i = 0; i < Stats.BASIC_TOTAL; i++) {
            buffer.WriteFloat(fieldPlayer.Stats.Values[(BasicAttribute) i].Rate);
        }

        for (int i = 0; i < Stats.SPECIAL_TOTAL; i++) {
            buffer.WriteFloat(fieldPlayer.Stats.Values[(SpecialAttribute) i].Rate);
        }

        for (int i = 0; i < Stats.SPECIAL_TOTAL; i++) {
            buffer.WriteFloat(fieldPlayer.Stats.Values[(SpecialAttribute) i].Base);
        }
        #endregion

        buffer.WriteUnicodeString(player.Character.Picture);
        buffer.WriteUnicodeString(player.Character.Motto);
        buffer.WriteUnicodeString(player.Character.GuildName);
        buffer.WriteUnicodeString(); // GuildRank

        buffer.WriteUnicodeString(player.Home.Name);
        buffer.WriteInt(player.Home.PlotMapId);
        buffer.WriteInt(player.Home.PlotNumber);
        buffer.WriteInt(player.Home.ApartmentNumber);

        buffer.WriteInt(player.Character.Title);
        buffer.WriteInt(player.Unlock.Titles.Count);
        foreach (int title in player.Unlock.Titles) {
            buffer.WriteInt(title);
        }

        buffer.WriteInt(player.Character.AchievementInfo.Total);
        buffer.WriteInt(fieldPlayer.Stats.Values.GearScore);
        buffer.WriteLong(player.Character.LastModified.ToEpochSeconds()); // Time entered map/logged in
        buffer.WriteInt();
        buffer.WriteInt();
        buffer.Write<SkinColor>(player.Character.SkinColor);
        buffer.WriteClass<MarriageInfo>(player.Character.MarriageInfo);
    }
}
