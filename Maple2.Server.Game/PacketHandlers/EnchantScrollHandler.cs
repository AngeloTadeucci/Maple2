using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using static Maple2.Model.Error.EnchantScrollError;

namespace Maple2.Server.Game.PacketHandlers;

public class EnchantScrollHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.EnchantScroll;

    private enum Command : byte {
        Preview = 1,
        Enchant = 2,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public TableMetadataStorage TableMetadata { private get; init; } = null!;
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Preview:
                HandlePreview(session, packet);
                return;
            case Command.Enchant:
                HandleEnchant(session, packet);
                return;
        }
    }

    private void HandlePreview(GameSession session, IByteReader packet) {
        long scrollUid = packet.ReadLong();
        long itemUid = packet.ReadLong();

        if (!TryGetMetadata(session, scrollUid, out EnchantScrollMetadata? metadata)) {
            session.Send(EnchantScrollPacket.Error(s_enchantscroll_invalid_scroll));
            return;
        }

        Item? item = session.Item.Inventory.Get(itemUid);
        if (item == null) {
            session.Send(EnchantScrollPacket.Error(s_enchantscroll_invalid_item));
            return;
        }

        EnchantScrollError error = IsCompatibleScroll(item, metadata);
        if (error != s_enchantscroll_ok) {
            session.Send(EnchantScrollPacket.Error(error));
            return;
        }

        int minEnchant = Math.Min(item.Enchant?.Enchants ?? 0, metadata.Enchants.Min());
        int maxEnchant = metadata.Enchants.Max();
        Dictionary<BasicAttribute, BasicOption> minOptions = [];
        Dictionary<BasicAttribute, BasicOption> maxOptions = [];
        for (int i = minEnchant; i < maxEnchant; i++) {
            // add to dictionaries
            int targetEnchant = i - 1;
            ItemEnchant result = ItemEnchantManager.GetEnchant(session, item, i + 1);
            foreach ((BasicAttribute attribute, BasicOption option) in result.BasicOptions) {
                if (minOptions.TryGetValue(attribute, out BasicOption currentOption)) {
                    minOptions[attribute] = currentOption + option;
                } else {
                    minOptions[attribute] = option;
                }
            }

            result = ItemEnchantManager.GetEnchant(session, item, i - 1);
            foreach ((BasicAttribute attribute, BasicOption option) in result.BasicOptions) {
                if (maxOptions.TryGetValue(attribute, out BasicOption currentOption)) {
                    maxOptions[attribute] = currentOption + option;
                } else {
                    maxOptions[attribute] = option;
                }
            }
        }
        session.Send(EnchantScrollPacket.Preview(item, metadata.Type, minOptions, maxOptions));
    }

    private void HandleEnchant(GameSession session, IByteReader packet) {
        long scrollUid = packet.ReadLong();
        long itemUid = packet.ReadLong();

        lock (session.Item) {
            if (!TryGetMetadata(session, scrollUid, out EnchantScrollMetadata? metadata)) {
                session.Send(EnchantScrollPacket.Error(s_enchantscroll_invalid_scroll));
                return;
            }

            Item? item = session.Item.Inventory.Get(itemUid);
            if (item == null) {
                session.Send(EnchantScrollPacket.Error(s_enchantscroll_invalid_item));
                return;
            }

            EnchantScrollError error = IsCompatibleScroll(item, metadata);
            if (error != s_enchantscroll_ok) {
                session.Send(EnchantScrollPacket.Error(error));
                return;
            }

            if (!session.Item.Inventory.Consume(scrollUid, 1)) {
                session.Send(EnchantScrollPacket.Error(s_enchantscroll_invalid_scroll));
                return;
            }

            // Update item enchant stats
            int enchantLevel = metadata.Enchants.Random();
            // Ensure that you cannot randomize an enchant lower than current item.
            item.Enchant ??= new ItemEnchant();
            if (enchantLevel > item.Enchant.Enchants) {
                item.Enchant = ItemEnchantManager.GetEnchant(session, item, enchantLevel);
                item.Enchant.Enchants = enchantLevel;
            }

            session.Send(EnchantScrollPacket.Enchant(item));
        }
    }

    private static EnchantScrollError IsCompatibleScroll(Item item, EnchantScrollMetadata metadata) {
        if (item.Metadata.Limit.Level < metadata.MinLevel || item.Metadata.Limit.Level > metadata.MaxLevel) {
            return s_enchantscroll_invalid_level;
        }
        if (!metadata.ItemTypes.Contains(item.Type.Type)) {
            return s_enchantscroll_invalid_slot;
        }
        if (!metadata.Rarities.Contains(item.Rarity)) {
            return s_enchantscroll_invalid_rank;
        }
        if ((item.Enchant?.Enchants ?? 0) >= metadata.Enchants.Max()) {
            return s_enchantscroll_invalid_grade;
        }
        // No idea what it even means for an item to be unstable, but you can't enchant something to 0.
        if (metadata.Enchants is [0]) {
            return s_enchantscroll_not_breaking_item;
        }

        return s_enchantscroll_ok;
    }

    private bool TryGetMetadata(GameSession session, long scrollUid, [NotNullWhen(true)] out EnchantScrollMetadata? metadata) {
        Item? scroll = session.Item.Inventory.Get(scrollUid);
        if (scroll == null) {
            metadata = null;
            return false;
        }

        if (scroll.Metadata.Function?.Type != ItemFunction.EnchantScroll) {
            metadata = null;
            return false;
        }
        if (!int.TryParse(scroll.Metadata.Function.Parameters, out int enchantId)) {
            metadata = null;
            return false;
        }
        if (!TableMetadata.EnchantScrollTable.Entries.TryGetValue(enchantId, out metadata)) {
            return false;
        }

        // Extra sanity check on enchant scroll data.
        return metadata.Enchants.Length > 0 && metadata.ItemTypes.Length > 0 && metadata.Rarities.Length > 0;
    }
}
