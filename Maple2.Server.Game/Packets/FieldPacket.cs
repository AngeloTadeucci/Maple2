using System.Numerics;
using Maple2.Database.Extensions;
using Maple2.Model;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Core.Packets.Helper;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class FieldPacket {
    public static ByteWriter AddPlayer(GameSession session) {
        Player player = session.Player;

        var pWriter = Packet.Of(SendOp.FieldAddUser);
        pWriter.WriteInt(session.Player.ObjectId);
        pWriter.WriteHexString("71 28 0E A2 F8 27 18 24 D9 00 A4 A7 F8 27 19 24 04 00 76 00 30 00 31 00 64 00 00 01 71 28 0E A2 F8 27 18 24 01 00 00 00 D4 84 1E 00 D4 84 1E 00 00 00 00 00 0C 00 01 00 50 00 00 00 20 03 00 00 8A 01 00 00 6F 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 DB 00 00 00 99 B3 F5 FF 99 B3 F5 FF 41 FB 35 66 00 00 00 00 04 00 00 00 0B 00 00 00 08 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 38 00 70 00 72 00 6F 00 66 00 69 00 6C 00 65 00 2F 00 35 00 33 00 2F 00 36 00 34 00 2F 00 32 00 36 00 30 00 31 00 31 00 35 00 34 00 32 00 30 00 38 00 37 00 30 00 31 00 30 00 38 00 37 00 39 00 36 00 31 00 2F 00 36 00 33 00 38 00 35 00 30 00 34 00 34 00 35 00 33 00 39 00 31 00 32 00 31 00 38 00 30 00 36 00 33 00 36 00 2E 00 70 00 6E 00 67 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0C 00 33 00 35 00 32 00 38 00 38 00 31 00 36 00 34 00 33 00 40 00 6E 00 78 00 EB 8B 08 15 00 00 00 00 00 00 00 00 04 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 BF 01 36 66 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 50 00 00 00 25 A9 CB A4 00 01 00 00 00 00 00 00 81 CB A4 00 01 00 00 00 00 01 00 B3 CB A4 00 01 00 00 00 00 00 00 8B CB A4 00 02 00 00 00 00 01 00 BD CB A4 00 01 00 00 00 00 00 00 68 CC A4 00 01 00 00 00 00 00 00 95 CB A4 00 02 00 00 00 00 01 00 9F CB A4 00 01 00 00 00 00 00 00 B4 CB A4 00 01 00 00 00 00 00 00 C7 CB A4 00 01 00 00 00 00 01 00 D1 CB A4 00 01 00 00 00 00 01 00 DB CB A4 00 01 00 00 00 00 00 00 E5 CB A4 00 01 00 00 00 00 00 00 EF CB A4 00 01 00 00 00 00 00 00 F9 CB A4 00 01 00 00 00 00 00 00 03 CC A4 00 01 00 00 00 00 00 00 0D CC A4 00 01 00 00 00 00 00 00 17 CC A4 00 01 00 00 00 00 00 00 21 CC A4 00 01 00 00 00 00 00 00 A3 CC A4 00 01 00 00 00 00 00 00 23 CC A4 00 01 00 00 00 00 00 00 2B CC A4 00 01 00 00 00 00 00 00 35 CC A4 00 01 00 00 00 00 00 00 3F CC A4 00 01 00 00 00 00 00 00 49 CC A4 00 01 00 00 00 00 00 00 4B CC A4 00 01 00 00 00 00 00 00 53 CC A4 00 01 00 00 00 00 00 00 5D CC A4 00 01 00 00 00 00 00 00 24 CC A4 00 01 00 00 00 00 00 00 A5 CC A4 00 01 00 00 00 00 00 00 65 CC A4 00 01 00 00 00 00 00 00 67 CC A4 00 01 00 00 00 00 00 00 71 CC A4 00 01 00 00 00 00 00 00 7B CC A4 00 01 00 00 00 00 00 00 85 CC A4 00 01 00 00 00 00 00 00 8F CC A4 00 01 00 00 00 00 00 00 99 CC A4 00 01 00 00 00 00 00 00 00 C0 73 45 00 00 E1 C4 00 C0 F3 44 00 00 00 00 00 00 00 00 00 00 34 43 01 23 8A 01 00 00 00 00 00 00 64 00 00 00 64 00 00 00 64 00 00 00 64 00 00 00 8A 01 00 00 00 00 00 00 64 00 00 00 64 00 00 00 64 00 00 00 64 00 00 00 6F 01 00 00 00 00 00 00 64 00 00 00 64 00 00 00 64 00 00 00 64 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 99 B3 F5 FF 99 B3 F5 FF 38 00 70 00 72 00 6F 00 66 00 69 00 6C 00 65 00 2F 00 35 00 33 00 2F 00 36 00 34 00 2F 00 32 00 36 00 30 00 31 00 31 00 35 00 34 00 32 00 30 00 38 00 37 00 30 00 31 00 30 00 38 00 37 00 39 00 36 00 31 00 2F 00 36 00 33 00 38 00 35 00 30 00 34 00 34 00 35 00 33 00 39 00 31 00 32 00 31 00 38 00 30 00 36 00 33 00 36 00 2E 00 70 00 6E 00 67 00 00 00 00 00 00 08 12 36 66 00 00 00 00 00 00 00 00 00 00 00 00 01 36 02 00 00 00 00 0D 19 78 9C E3 4B A9 38 C3 F0 D6 DA F4 F0 8F E5 32 2A 2C 2C 0C 0C 0C 8C 40 FC 1F 08 38 38 CD D2 18 48 00 2E 93 CB FF DF 6E F5 F9 2F 67 AC F0 9F 19 C8 E7 02 62 56 06 41 06 36 20 CD CC 00 12 11 07 9B 2D CD A0 0C 24 65 18 B4 48 31 1C 02 18 31 44 38 50 44 19 19 6F 32 2C 59 FE 43 5D 52 85 85 A1 8C C1 80 C1 90 21 05 AB 39 20 3F BF 83 FA 99 95 12 3F 6F CD F4 F9 BF 22 D1 F9 BF BF 86 F8 7F 90 39 83 D8 CF 1F 72 D6 30 B4 E4 28 1E 01 F9 99 8D 91 01 E1 E7 3A 12 FD FC EC D9 B3 FF B6 B7 0E FF 9F 36 6D DA 7F 61 06 88 9F 49 07 0D F6 8C 0C 22 0C DC A4 6B C4 0C 0F 16 02 F2 20 50 F5 6D 2D 83 83 4D 36 38 C6 39 98 19 10 BE 57 26 D1 F7 FA FA AB C0 A9 5C 58 38 E1 3F 88 CF 05 36 4B 04 18 E7 24 03 6A C5 EC DE 9A F5 0C 8E 50 BF 71 D2 C2 6F 02 A4 18 02 75 3B 86 08 79 7E B3 64 D9 C8 70 7A D5 E9 A3 20 BF 71 21 FB 2D 98 8B 34 BF 19 5B DB FF 97 56 50 FE CF 2F 24 F2 9F 93 01 E1 37 36 52 0C 81 BA 1D 43 84 3C BF C9 76 6F 62 F0 7D 17 70 E2 C7 72 7D 15 6E 64 BF 4D E3 26 CD 6F C8 00 A4 1F E2 48 11 06 76 D2 B5 53 CB 6F 0B CD 17 30 6C AC 5A 0A 56 29 84 5C DA 38 FC 36 A5 D8 6F 24 03 EC 65 02 21 79 DE 25 B3 19 56 43 FD C0 48 89 1F 4E AF F8 FF 7F 76 E2 8D FF 59 61 67 FF 83 CA 09 26 B0 68 83 3D 6E 1D F8 E4 48 04 E4 F9 3D 55 6B 2E C3 1A A8 DF 99 28 F1 BB A6 CA D6 FF 8F 1F 7F FF CF C4 2E 02 8E 3B 66 52 34 E3 77 23 21 79 8E 8D F3 18 D6 42 FD C0 4C ED 34 C8 B7 60 B9 DD 3A F7 87 B6 31 FD 4E 76 AC 41 8F 6C 48 36 88 3C 3F AD FF BA 96 61 1D D4 4F 1C 94 F8 09 5B 79 48 32 C0 5E 5A 20 C9 63 29 2D 98 B0 98 F3 A3 7A 3D C3 7A A8 AF 38 07 61 69 41 9E AF EE 77 6E 62 D8 00 F5 15 F7 B0 F1 15 E3 3E 46 D4 DA 89 B1 E7 E2 16 06 FB 55 DF F7 81 EA 31 76 64 7F BE 64 83 A8 7C 39 39 96 64 FF 52 D9 9F F8 6B 30 6C FE 04 00 05 1A 37 52 00 01 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF 7F 00 00 00");
        return pWriter;
        pWriter.WriteCharacter(session);
        pWriter.WriteClass<SkillInfo>(session.Config.Skill.SkillInfo);
        pWriter.Write<Vector3>(session.Player.Position);
        pWriter.Write<Vector3>(session.Player.Rotation);
        pWriter.WriteByte();
        pWriter.WritePlayerStats(session.Player.Stats);
        pWriter.WriteBool(session.Player.InBattle);

        #region Unknown Cube Section
        pWriter.WriteByte();
        pWriter.WriteClass<HeldCube>(session.HeldCube ?? HeldCube.Default);
        pWriter.WriteInt();
        #endregion

        pWriter.Write<SkinColor>(player.Character.SkinColor);
        pWriter.WriteUnicodeString(player.Character.Picture);
        pWriter.WriteBool(session.Ride != null);
        if (session.Ride != null) {
            pWriter.WriteClass<RideOnAction>(session.Ride.Action);

            pWriter.WriteByte(); // Unknown Count for Loop
        }

        pWriter.WriteInt();
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); // ???
        pWriter.WriteInt(player.Home.CurrentArchitectScore);
        pWriter.WriteInt(player.Home.ArchitectScore);

        using (var buffer = new PoolByteWriter()) {
            int count = session.Item.Equips.Gear.Count + session.Item.Equips.Outfit.Count;
            buffer.WriteByte((byte) count);
            foreach (Item item in session.Item.Equips.Gear.Values) {
                buffer.WriteEquip(item);
            }
            foreach (Item item in session.Item.Equips.Outfit.Values) {
                buffer.WriteEquip(item);
            }
            // Don't know...
            buffer.WriteBool(true);
            buffer.WriteLong();
            buffer.WriteLong();
            // Outfit2
            buffer.WriteByte();

            pWriter.WriteDeflated(buffer.Buffer, 0, buffer.Length);
        }

        using (var buffer = new PoolByteWriter()) {
            buffer.WriteByte(); // Unknown

            pWriter.WriteDeflated(buffer.Buffer, 0, buffer.Length);
        }

        using (var buffer = new PoolByteWriter()) {
            buffer.WriteByte((byte) session.Item.Equips.Badge.Count);
            foreach (Item item in session.Item.Equips.Badge.Values) {
                buffer.WriteBadge(item);
            }

            pWriter.WriteDeflated(buffer.Buffer, 0, buffer.Length);
        }

        pWriter.WriteShort((short) session.Player.Buffs.Buffs.Count);
        foreach (Buff buff in session.Player.Buffs.Buffs.Values) {
            pWriter.WriteInt(buff.Owner.ObjectId);
            pWriter.WriteInt(buff.ObjectId);
            pWriter.WriteInt(buff.Caster.ObjectId);
            pWriter.WriteClass<Buff>(buff);
        }

        #region sub_BF6440
        pWriter.WriteInt();
        pWriter.WriteInt();
        #endregion

        pWriter.WriteByte();

        #region sub_5F1C30
        pWriter.WriteInt();
        pWriter.WriteByte();
        pWriter.WriteByte();
        #endregion

        pWriter.WriteInt(player.Character.Title);
        pWriter.WriteShort(player.Character.Insignia);
        pWriter.WriteByte(); // InsigniaValue

        pWriter.WriteInt();
        pWriter.WriteBool(session.Pet != null);
        if (session.Pet != null) {
            pWriter.WriteInt(session.Pet.Pet.Id);
            pWriter.WriteLong(session.Pet.Pet.Uid);
            pWriter.WriteInt(session.Pet.Pet.Rarity);
            pWriter.WriteClass<Item>(session.Pet.Pet);
        }
        pWriter.WriteLong(player.Account.PremiumTime);
        pWriter.WriteInt();
        pWriter.WriteByte();
        pWriter.WriteInt(); // Tail
        pWriter.WriteInt();
        pWriter.WriteShort();

        return pWriter;
    }

    public static ByteWriter RemovePlayer(int objectId) {
        var pWriter = Packet.Of(SendOp.FieldRemoveUser);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter AddNpc(FieldNpc npc) {
        var pWriter = Packet.Of(SendOp.FieldAddNpc);
        pWriter.WriteInt(npc.ObjectId);
        pWriter.WriteInt(npc.Value.Id);
        pWriter.Write<Vector3>(npc.Position);
        pWriter.Write<Vector3>(npc.Rotation);
        // If NPC is not valid, the packet seems to stop here

        if (npc.Value.IsBoss) {
            pWriter.WriteString(npc.Value.Metadata.Model.Name);
        }

        pWriter.WriteNpcStats(npc.Stats);
        pWriter.WriteBool(npc.IsDead);

        pWriter.WriteShort((short) npc.Buffs.Buffs.Count);
        foreach (Buff buff in npc.Buffs.Buffs.Values) {
            pWriter.WriteInt(buff.Owner.ObjectId);
            pWriter.WriteInt(buff.ObjectId);
            pWriter.WriteInt(buff.Caster.ObjectId);
            pWriter.WriteClass<Buff>(buff);
        }

        pWriter.WriteLong(); // uid for PetNpc
        pWriter.WriteByte();
        pWriter.WriteInt(npc.Value.Metadata.Basic.Level);
        pWriter.WriteInt();

        if (npc.Value.IsBoss) {
            pWriter.WriteUnicodeString(); // EffectStr
            pWriter.WriteInt(npc.Buffs.Buffs.Count);
            foreach (Buff buff in npc.Buffs.Buffs.Values) {
                pWriter.WriteInt(buff.Id);
                pWriter.WriteShort(buff.Level);
            }

            pWriter.WriteInt();
        }

        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter RemoveNpc(int objectId) {
        var pWriter = Packet.Of(SendOp.FieldRemoveNpc);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter DropItem(FieldItem fieldItem) {
        Item item = fieldItem;

        var pWriter = Packet.Of(SendOp.FieldAddItem);
        pWriter.WriteInt(fieldItem.ObjectId);
        pWriter.WriteInt(item.Id);
        pWriter.WriteInt(item.Amount);

        pWriter.WriteBool(fieldItem.ReceiverId >= 0);
        if (fieldItem.ReceiverId >= 0) {
            pWriter.WriteLong(fieldItem.ReceiverId);
        }

        pWriter.Write<Vector3>(fieldItem.Position);
        pWriter.WriteInt(fieldItem.Owner?.ObjectId ?? 0);
        pWriter.WriteInt();
        pWriter.Write<DropType>(fieldItem.Type);
        pWriter.WriteInt(item.Rarity);
        pWriter.WriteShort();
        pWriter.WriteBool(fieldItem.FixedPosition);
        pWriter.WriteBool(false);

        if (!item.IsMeso()) {
            pWriter.WriteClass<Item>(item);
        }

        return pWriter;
    }

    #region debug
    // This was used for rapid fire placement & repositioning of field items for debug visualization purposes without requiring allocating a whole new FieldItem
    // It was used for debugging npc movement to display important parameters that weren't being replicated properly.
    // Currently there is no easy to use system in place for that, though I do want to make one later
    public static ByteWriter DropDebugItem(FieldItem fieldItem, int objectId, Vector3 position, int unkInt, short unkShort, bool unkBool) {
        Item item = fieldItem;

        var pWriter = Packet.Of(SendOp.FieldAddItem);
        pWriter.WriteInt(objectId);
        pWriter.WriteInt(item.Id);
        pWriter.WriteInt(item.Amount);

        pWriter.WriteBool(fieldItem.ReceiverId >= 0);
        if (fieldItem.ReceiverId >= 0) {
            pWriter.WriteLong(fieldItem.ReceiverId);
        }

        pWriter.Write<Vector3>(position);
        pWriter.WriteInt(fieldItem.Owner?.ObjectId ?? 0);
        pWriter.WriteInt(unkInt);
        pWriter.Write<DropType>(fieldItem.Type);
        pWriter.WriteInt(item.Rarity);
        pWriter.WriteShort(unkShort);
        pWriter.WriteBool(fieldItem.FixedPosition);
        pWriter.WriteBool(unkBool);

        if (!item.IsMeso()) {
            pWriter.WriteClass<Item>(item);
        }

        return pWriter;
    }
    #endregion

    public static ByteWriter RemoveItem(int objectId) {
        var pWriter = Packet.Of(SendOp.FieldRemoveItem);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter AddPet(FieldPet pet) {
        var pWriter = Packet.Of(SendOp.FieldAddPet);
        pWriter.WriteInt(pet.ObjectId);
        pWriter.WriteInt(pet.SkinId);
        pWriter.WriteInt(pet.Value.Id);
        pWriter.Write<Vector3>(pet.Position);
        pWriter.Write<Vector3>(pet.Rotation);
        pWriter.WriteFloat(pet.Scale);
        pWriter.WriteInt(pet.OwnerId);
        pWriter.WriteNpcStats(pet.Stats);
        pWriter.WriteLong(pet.Pet.Uid);
        pWriter.WriteByte();
        pWriter.WriteShort(pet.Value.Metadata.Basic.Level);
        pWriter.WriteShort(pet.TamingRank);
        pWriter.WriteInt();
        pWriter.WriteUnicodeString(pet.Pet.Pet?.Name ?? "");

        return pWriter;
    }

    public static ByteWriter RemovePet(int objectId) {
        var pWriter = Packet.Of(SendOp.FieldRemovePet);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    private static void WriteCharacter(this IByteWriter writer, GameSession session) {
        Account account = session.Player.Value.Account;
        Character character = session.Player.Value.Character;
        writer.WriteLong(account.Id);
        writer.WriteLong(character.Id);
        writer.WriteUnicodeString(character.Name);
        writer.Write<Gender>(character.Gender);
        writer.WriteByte(1);
        writer.WriteLong(character.AccountId);
        writer.WriteInt();
        writer.WriteInt(character.ReturnMapId);
        writer.WriteInt(character.MapId);
        writer.WriteInt(character.InstanceId);
        writer.WriteShort(character.Level);
        writer.WriteShort(character.Channel);
        writer.WriteInt((int) character.Job.Code());
        writer.Write<Job>(character.Job);
        writer.WriteInt((int) session.Stats.Values[BasicAttribute.Health].Current);
        writer.WriteInt((int) session.Stats.Values[BasicAttribute.Health].Total);
        writer.WriteShort();
        writer.WriteLong();
        writer.WriteLong(character.StorageCooldown);
        writer.WriteLong(character.DoctorCooldown);
        writer.WriteInt(character.ReturnMapId);
        writer.Write<Vector3>(character.ReturnPosition);
        writer.WriteInt(session.Stats.Values.GearScore);
        writer.Write<SkinColor>(character.SkinColor);
        writer.WriteLong(character.CreationTime);
        writer.Write<AchievementInfo>(character.AchievementInfo);
        writer.WriteLong(character.GuildId);
        writer.WriteUnicodeString(character.GuildName);
        writer.WriteUnicodeString(character.Motto);
        writer.WriteUnicodeString(character.Picture);
        writer.WriteByte(); // Club Count
        writer.WriteByte(); // PCBang related?
        writer.WriteClass<Mastery>(character.Mastery);
        #region Unknown
        writer.WriteUnicodeString(); // Login username
        writer.WriteLong();
        writer.WriteLong();
        writer.WriteLong();
        #endregion
        writer.WriteInt(); // Unknown Count
        writer.WriteByte();
        writer.WriteBool(false);
        writer.WriteLong(); // Birthday
        writer.WriteInt(session.SuperChatId);
        writer.WriteInt();
        writer.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        writer.WriteInt(account.PrestigeLevel); // PrestigeLevel
        writer.WriteLong(character.LastModified.ToEpochSeconds());
        writer.WriteInt(); // Unknown Count
        writer.WriteInt(); // Unknown Count
        writer.WriteShort(); // Survival related?
        writer.WriteLong();
    }
}
