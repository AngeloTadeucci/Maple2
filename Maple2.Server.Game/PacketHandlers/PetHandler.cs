using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Manager.Config;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class PetHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestPet;

    private enum Command : byte {
        Summon = 0,
        UnSummon = 1,
        Switch = 3,
        Rename = 4,
        UpdatePotionConfig = 5,
        UpdateLootConfig = 6,
        Fusion = 12, // OnPetEnchant, s_msg_transfer_bind_pet_compose
        Attack = 15, // OnPetAttack, setPetAttackState
        Unknown16 = 16,
        Evolve = 17,
        EvolvePoints = 18,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        switch (command) {
            case Command.Summon:
                HandleSummon(session, packet);
                return;
            case Command.UnSummon:
                HandleUnSummon(session, packet);
                return;
            case Command.Switch:
                HandleSwitch(session, packet);
                return;
            case Command.Rename:
                HandleRename(session, packet);
                return;
            case Command.UpdatePotionConfig:
                HandleUpdatePotionConfig(session, packet);
                return;
            case Command.UpdateLootConfig:
                HandleUpdateLootConfig(session, packet);
                return;
            case Command.Fusion:
                HandleFusion(session, packet);
                return;
            case Command.Attack:
                HandleAttack(session, packet);
                return;
            case Command.Unknown16:
                HandleUnknown16(session, packet);
                return;
            case Command.Evolve:
                HandleEvolve(session, packet);
                return;
            case Command.EvolvePoints:
                HandleEvolvePoints(session, packet);
                return;
        }
    }

    private void HandleSummon(GameSession session, IByteReader packet) {
        long petUid = packet.ReadLong();
        SummonPet(session, petUid);
    }

    private void HandleUnSummon(GameSession session, IByteReader packet) {
        long petUid = packet.ReadLong();
        if (session.Pet?.Pet.Uid != petUid) {
            return;
        }

        session.Pet.Dispose();
    }

    private void HandleSwitch(GameSession session, IByteReader packet) {
        if (session.Pet == null) {
            return;
        }

        long petUid = packet.ReadLong();
        session.Pet.Dispose();
        SummonPet(session, petUid);
    }

    private void HandleRename(GameSession session, IByteReader packet) {
        if (session.Pet == null) {
            return;
        }

        string name = packet.ReadUnicodeString();
        session.Pet.Rename(name);
    }

    private void HandleUpdatePotionConfig(GameSession session, IByteReader packet) {
        if (session.Pet == null) {
            return;
        }

        byte count = packet.ReadByte();

        var config = new PetPotionConfig[count];
        for (int i = 0; i < count; i++) {
            config[i] = packet.Read<PetPotionConfig>();
        }

        session.Pet.UpdatePotionConfig(config);
    }

    private void HandleUpdateLootConfig(GameSession session, IByteReader packet) {
        if (session.Pet == null) {
            return;
        }

        var config = packet.Read<PetLootConfig>();
        session.Pet.UpdateLootConfig(config);
    }

    private void HandleFusion(GameSession session, IByteReader packet) {
        long petUid = packet.ReadLong();
        short count = packet.ReadShort();
        for (int i = 0; i < count; i++) {
            packet.ReadLong(); // fodder uid
            packet.ReadInt(); // count
        }
    }

    private void HandleAttack(GameSession session, IByteReader packet) {
        packet.ReadBool();
    }

    private void HandleUnknown16(GameSession session, IByteReader packet) {
        packet.ReadLong();
        packet.ReadLong();
    }

    private void HandleEvolve(GameSession session, IByteReader packet) {
        long petUid = packet.ReadLong();
    }

    private void HandleEvolvePoints(GameSession session, IByteReader packet) {
        long petUid = packet.ReadLong();
        short count = packet.ReadShort();
        for (int i = 0; i < count; i++) {
            packet.ReadLong(); // fodder uid
        }
    }

    private static void SummonPet(GameSession session, long petUid) {
        if (session.Field == null || session.Pet != null) {
            return;
        }

        lock (session.Item) {
            Item? pet = session.Item.Inventory.Get(petUid, InventoryType.Pets);
            if (pet == null) {
                return;
            }

            FieldPet? fieldPet = session.Field.SpawnPet(pet, session.Player.Position, session.Player.Rotation, player: session.Player);
            if (fieldPet == null) {
                return;
            }

            session.Field.Broadcast(FieldPacket.AddPet(fieldPet));
            session.Field.Broadcast(ProxyObjectPacket.AddPet(fieldPet));
            session.Pet = new PetManager(session, fieldPet);
            session.Pet.Load();
        }
    }
}
