using System.Numerics;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class ProxyObjectPacket {
    private enum Command : byte {
        AddPlayer = 3,
        RemovePlayer = 4,
        UpdatePlayer = 5,
        AddNpc = 6,
        RemoveNpc = 7,
        UpdateNpc = 8,
        AddPet = 9,
        RemovePet = 10,
        UpdatePet = 11,
    }

    public static ByteWriter AddPlayer(FieldPlayer fieldPlayer) {
        Player player = fieldPlayer;

        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.AddPlayer);
        pWriter.WriteInt(fieldPlayer.ObjectId);
        pWriter.WriteLong(player.Character.Id);
        pWriter.WriteLong(player.Account.Id);
        pWriter.WriteUnicodeString(player.Character.Name);
        pWriter.WriteUnicodeString(player.Character.Picture);
        pWriter.WriteUnicodeString(player.Character.Motto);
        pWriter.WriteByte(); // CProxyGameObject+3C
        pWriter.Write<Vector3>(fieldPlayer.Position);
        pWriter.WriteShort(player.Character.Level);
        pWriter.Write<JobCode>(player.Character.Job.Code());
        pWriter.Write<Job>(player.Character.Job);
        pWriter.WriteInt(player.Home.PlotMapId);
        pWriter.WriteInt(player.Home.PlotNumber);
        pWriter.WriteInt(player.Home.ApartmentNumber);
        pWriter.WriteUnicodeString(player.Home.Indoor.Name);
        pWriter.WriteInt(fieldPlayer.Stats.GearScore);
        pWriter.WriteShort((short) fieldPlayer.State);
        pWriter.Write<AchievementInfo>(player.Character.AchievementInfo);

        return pWriter;
    }

    public static ByteWriter RemovePlayer(int objectId) {
        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.RemovePlayer);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter UpdatePlayer(FieldPlayer fieldPlayer, byte flag = 0) {
        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.UpdatePlayer);
        pWriter.WriteInt(fieldPlayer.ObjectId);

        pWriter.WriteByte(flag);
        if ((flag & 1) != 0) {
            pWriter.WriteByte(); // CProxyGameObject+3C
        }
        if ((flag & 2) != 0) {
            pWriter.Write<Vector3>(fieldPlayer.Position);
        }
        if ((flag & 4) != 0) {
            pWriter.WriteShort(fieldPlayer.Value.Character.Level);
        }
        if ((flag & 8) != 0) {
            pWriter.Write<JobCode>(fieldPlayer.Value.Character.Job.Code());
            pWriter.Write<Job>(fieldPlayer.Value.Character.Job);
        }
        if ((flag & 16) != 0) {
            pWriter.WriteUnicodeString(fieldPlayer.Value.Character.Motto);
        }
        if ((flag & 32) != 0) {
            pWriter.WriteInt(fieldPlayer.Stats.GearScore);
        }
        if ((flag & 64) != 0) {
            pWriter.WriteShort((short) fieldPlayer.State);
        }

        return pWriter;
    }

    public static ByteWriter AddNpc(FieldNpc fieldNpc) {
        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.AddNpc);
        pWriter.WriteInt(fieldNpc.ObjectId);
        pWriter.WriteInt(fieldNpc.Value.Id);
        pWriter.WriteByte(); // CProxyGameObject+3C
        pWriter.WriteInt(200); // CProxyGameObject+50, Counter
        pWriter.Write<Vector3>(fieldNpc.Position);

        return pWriter;
    }

    public static ByteWriter RemoveNpc(int objectId) {
        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.RemoveNpc);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter UpdateNpc(FieldNpc fieldNpc) {
        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.UpdateNpc);
        pWriter.WriteInt(fieldNpc.ObjectId);
        pWriter.WriteByte(); // CProxyGameObject+3C
        pWriter.Write<Vector3>(fieldNpc.Position);

        return pWriter;
    }

    public static ByteWriter AddPet(FieldPet fieldPet) {
        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.AddPet);
        pWriter.WriteInt(fieldPet.ObjectId);
        pWriter.WriteInt(fieldPet.SkinId);
        pWriter.WriteInt(fieldPet.Value.Id);
        pWriter.WriteByte(); // CProxyGameObject+3C
        pWriter.Write<Vector3>(fieldPet.Position);

        return pWriter;
    }

    public static ByteWriter RemovePet(int objectId) {
        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.RemovePet);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter UpdatePet(FieldPet fieldPet) {
        var pWriter = Packet.Of(SendOp.ProxyGameObj);
        pWriter.Write<Command>(Command.UpdatePet);
        pWriter.WriteInt(fieldPet.ObjectId);
        pWriter.WriteByte(); // CProxyGameObject+3C
        pWriter.Write<Vector3>(fieldPet.Position);

        return pWriter;
    }
}
