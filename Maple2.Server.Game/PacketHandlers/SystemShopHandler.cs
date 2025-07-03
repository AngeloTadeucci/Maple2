﻿using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class SystemShopHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.SystemShop;

    private enum Command : byte {
        Arena = 3,
        Fishing = 4,
        Mentee = 6,
        Mentor = 7,
        Item = 10,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Arena:
                HandleArena(session, packet);
                break;
            case Command.Fishing:
                HandleFishing(session, packet);
                break;
            case Command.Mentee:
                HandleMentee(session, packet);
                break;
            case Command.Mentor:
                HandleMentor(session, packet);
                break;
            case Command.Item:
                HandleItem(session, packet);
                break;
        }
    }

    private void HandleArena(GameSession session, IByteReader packet) {
        bool openShop = packet.ReadBool();
        if (!openShop) {
            session.Shop.ClearActiveShop();
            return;
        }

        if (!session.NpcMetadata.TryGet(Constant.SystemShopNPCIDHonorToken, out NpcMetadata? npc)) {
            return;
        }

        session.Shop.Load(npc.Basic.ShopId, Constant.SystemShopNPCIDHonorToken);
        session.Send(SystemShopPacket.Arena());
    }

    private void HandleFishing(GameSession session, IByteReader packet) {
        bool openShop = packet.ReadBool();
        if (!openShop) {
            session.Shop.ClearActiveShop();
            return;
        }
        if (!session.NpcMetadata.TryGet(Constant.SystemShopNPCIDFishing, out NpcMetadata? npc)) {
            return;
        }

        session.Shop.Load(npc.Basic.ShopId, npc.Id);
        session.Send(SystemShopPacket.Fishing());
    }

    private void HandleMentee(GameSession session, IByteReader packet) {
        bool openShop = packet.ReadBool();
        if (!openShop) {
            session.Shop.ClearActiveShop();
            return;
        }

        if (!session.NpcMetadata.TryGet(Constant.SystemShopNPCIDMentee, out NpcMetadata? npc)) {
            return;
        }

        session.Shop.Load(npc.Basic.ShopId, npc.Id);
        session.Send(SystemShopPacket.Mentee());
    }

    private void HandleMentor(GameSession session, IByteReader packet) {
        bool openShop = packet.ReadBool();
        if (!openShop) {
            session.Shop.ClearActiveShop();
            return;
        }

        if (!session.NpcMetadata.TryGet(Constant.SystemShopNPCIDMentor, out NpcMetadata? npc)) {
            return;
        }

        session.Shop.Load(npc.Basic.ShopId, npc.Id);
        session.Send(SystemShopPacket.Mentor());
    }

    private void HandleItem(GameSession session, IByteReader packet) {
        bool openShop = packet.ReadBool();
        if (!openShop) {
            session.Shop.ClearActiveShop();
            return;
        }

        int itemId = packet.ReadInt();
        Item? item = session.Item.Inventory.Find(itemId).FirstOrDefault();
        if (item == null || item.Metadata.Property.ShopId == 0) {
            return;
        }

        session.Shop.Load(item.Metadata.Property.ShopId);
        session.Send(SystemShopPacket.Item());
    }
}
