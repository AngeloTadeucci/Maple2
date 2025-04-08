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
        pWriter.WriteCharacter(session);
        pWriter.WriteClass<SkillInfo>(session.Config.Skill.SkillInfo);
        pWriter.Write<Vector3>(session.Player.Position);
        pWriter.Write<Vector3>(session.Player.Rotation);
        pWriter.WriteByte();
        pWriter.WritePlayerStats(session.Player.Stats.Values);
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

        pWriter.WriteNpcStats(npc.Stats.Values);
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
        pWriter.WriteNpcStats(pet.Stats.Values);
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
        writer.WriteInt(character.RoomId);
        writer.WriteShort(character.Level);
        writer.WriteShort(character.Channel);
        writer.WriteInt((int) character.Job.Code());
        writer.Write<Job>(character.Job);
        writer.WriteInt((int) session.Stats.Values[BasicAttribute.Health].Total);
        writer.WriteInt((int) session.Stats.Values[BasicAttribute.Health].Current);
        writer.WriteShort(character.DeathCount);
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
        writer.WriteByte((byte) character.ClubIds.Count);
        foreach (long clubId in character.ClubIds) {
            bool unk = true;
            writer.WriteBool(unk);
            if (unk) {
                writer.WriteLong(clubId);
                writer.WriteUnicodeString(); // Club Name
            }
        }
        writer.WriteByte(); // PCBang related?
        writer.WriteClass<Mastery>(character.Mastery);
        #region Unknown
        writer.WriteUnicodeString(); // Login username
        writer.WriteLong(); // Session Id
        writer.WriteLong();
        writer.WriteLong();
        #endregion
        writer.WriteInt(); // Unknown Count
        writer.WriteByte(); // Mentor User Type
        writer.WriteBool(false);
        writer.WriteLong(); // Birthday
        writer.WriteInt(session.SuperChatId);
        writer.WriteInt();
        writer.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        writer.WriteInt(account.PrestigeLevel); // PrestigeLevel
        writer.WriteLong(character.LastModified.ToEpochSeconds());
        writer.WriteInt(1); // Unknown Count
        writer.WriteLong(session.CharacterId);
        writer.WriteInt(); // Unknown Count
        writer.WriteShort(); // Survival related?
        writer.WriteLong();
    }
}
