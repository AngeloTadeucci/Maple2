using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Formulas;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class ItemMergeManager {

    private readonly GameSession session;

    private readonly ILogger logger = Log.Logger.ForContext<ItemMergeManager>();

    private Item? upgradeItem;
    private Item? catalystItem;

    public ItemMergeManager(GameSession session) {
        this.session = session;
    }

    private void Clear() {
        upgradeItem = null;
        catalystItem = null;
    }

    public void Stage(long itemUid) {
        Clear();

        Item? item = session.Item.Inventory.Get(itemUid);
        if (item == null) {
            item = session.Item.Equips.Get(itemUid);
            if (item == null) {
                // TODO: Error
                return;
            }
        }
        int type = item.Type.Type;
        upgradeItem = item;

        List<long> catalystUids = [];
        foreach ((int id, Dictionary<int, ItemMergeSlot> options) in session.ServerTableMetadata.ItemMergeTable.Entries) {
            Item? catalyst = session.Item.Inventory.Find(id).FirstOrDefault();
            if (catalyst == null) {
                continue;
            }

            if (catalyst.Metadata.Limit.Level < item.Metadata.Limit.Level) {
                continue;
            }

            if (!options.ContainsKey(type)) {
                continue;
            }

            catalystUids.Add(catalyst.Uid);
        }

        session.Send(ItemMergePacket.Stage(catalystUids));
    }

    public void SelectCrystal(long itemUid, long crystalUid) {
        if (upgradeItem?.Uid != itemUid) {
            logger.Error("Item Merge upgrade item UID mismatch: {UpgradeItemUid} != {ItemUid}", upgradeItem?.Uid, itemUid);
            return;
        }

        Item? catalyst = session.Item.Inventory.Get(crystalUid);
        if (catalyst == null) {
            return;
        }

        catalystItem = catalyst;

        if (!session.ServerTableMetadata.ItemMergeTable.Entries.TryGetValue(catalyst.Id, out Dictionary<int, ItemMergeSlot>? options) ||
            (!options.TryGetValue(upgradeItem.Type.Type, out ItemMergeSlot? mergeSlot))) {
            return;
        }

        session.Send(ItemMergePacket.Select(mergeSlot, ItemMerge.CostMultiplier(upgradeItem.Rarity)));

        if (!session.NpcMetadata.TryGet(Constant.EmpowermentNpc, out NpcMetadata? npcMetadata) ||
            !session.ScriptMetadata.TryGet(Constant.EmpowermentNpc, out ScriptMetadata? script) ||
            !script.States.TryGetValue(Constant.MergeSmithScriptID, out ScriptState? state)) {
            return;
        }

        CinematicEventScript[] eventScripts = state.Contents.First().Events;
        session.NpcScript = new NpcScriptManager(session, eventScripts);
        session.NpcScript.Event();
    }

    public void Empower(long itemUid, long catalystUid) {
        if (upgradeItem?.Uid != itemUid) {
            logger.Error("Item Merge upgrade item UID mismatch: {UpgradeItemUid} != {ItemUid}", upgradeItem?.Uid, itemUid);
            return;
        }

        if (catalystItem?.Uid != catalystUid) {
            logger.Error("Item Merge catalyst item UID mismatch: {CatalystItemUid} != {CatalystUid}", catalystItem?.Uid, catalystUid);
            return;
        }

        if (!session.ServerTableMetadata.ItemMergeTable.Entries.TryGetValue(catalystItem.Id, out Dictionary<int, ItemMergeSlot>? options) ||
            (!options.TryGetValue(upgradeItem.Type.Type, out ItemMergeSlot? mergeSlot))) {
            return;
        }

        int multiplier = ItemMerge.CostMultiplier(upgradeItem.Rarity);
        long totalMesoCost = mergeSlot.MesoCost * multiplier;
        if (session.Currency.CanAddMeso(-totalMesoCost) != -totalMesoCost) {
            session.Send(ItemMergePacket.Error(ItemMergeError.s_item_merge_option_error_lack_meso));
            return;
        }

        if (!session.Item.Inventory.ConsumeItemComponents(mergeSlot.Materials, multiplier)) {
            session.Send(ItemMergePacket.Error(ItemMergeError.s_item_merge_option_error_lack_material));
            return;
        }

        if (!session.Item.Inventory.Consume(catalystItem.Uid)) {
            session.Send(ItemMergePacket.Error(ItemMergeError.s_item_merge_option_error_invalid_merge_scroll));
            return;
        }

        session.Currency.Meso += -(mergeSlot.MesoCost * multiplier);

        int selectedIndex = Random.Shared.Next(mergeSlot.BasicOptions.Count + mergeSlot.SpecialOptions.Count);

        if (selectedIndex <= mergeSlot.BasicOptions.Count - 1) {
            (BasicAttribute attribute, ItemMergeOption mergeOption) = mergeSlot.BasicOptions.ElementAt(selectedIndex);

            (int value, float rate) = Roll(mergeOption);
            upgradeItem.Stats![ItemStats.Type.Empowerment1] = new ItemStats.Option {
                Basic = {
                    [attribute] = new BasicOption(value, rate),
                },
            };
        } else {
            (SpecialAttribute attribute, ItemMergeOption mergeOption) = mergeSlot.SpecialOptions.ElementAt(selectedIndex - mergeSlot.BasicOptions.Count - 1);

            (int value, float rate) = Roll(mergeOption);
            upgradeItem.Stats![ItemStats.Type.Empowerment1] = new ItemStats.Option {
                Special = {
                    [attribute] = new SpecialOption(rate, value),
                },
            };
        }

        session.Send(ItemMergePacket.Empower(upgradeItem));
        session.NpcScript?.Event();
        session.ConditionUpdate(ConditionType.adventure_level);

        return;

        (int, float) Roll(ItemMergeOption mergeOption) {
            WeightedSet<(int, float)> weightedSet = new();

            for (int i = 0; i < mergeOption.Values.Length; i++) {
                weightedSet.Add((mergeOption.Values[i], mergeOption.Rates[i]), mergeOption.Weights[i]);
            }

            return weightedSet.Get();
        }
    }
}
