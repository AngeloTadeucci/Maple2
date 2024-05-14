using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager;

public class ItemEnchantManager {
    private const int MAX_RATE = 100;
    private const int MAX_EXP = 10000;
    private const int CHARGE_RATE = 1;

    // ReSharper disable RedundantExplicitArraySize
    private static readonly int[] RequireFodder = new int[15] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 3, 3, 4 };
    private static readonly int[] GainExp = new int[15] { 10000, 10000, 10000, 5000, 5000, 5000, 2500, 2500, 2500, 2000, 3334, 2000, 2000, 1250, 1250 };
    private static readonly int[] SuccessRate = new int[15] { 100, 100, 100, 95, 90, 80, 70, 60, 50, 40, 30, 20, 15, 10, 5 };
    private static readonly int[] FodderRate = new int[15] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 7, 5, 4, 2 };
    private static readonly int[] FailCharge = new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 4, 4, 5 };
    private static readonly float[] StatBonusRate = new float[15] { 0.02f, 0.04f, 0.07f, 0.1f, 0.14f, 0.19f, 0.25f, 0.32f, 0.4f, 0.5f, 0.64f, 0.84f, 1.12f, 1.5f, 2f };
    // ReSharper restore RedundantExplicitArraySize

    private static readonly IngredientInfo[][] OpheliaCost = new IngredientInfo[15][];
    private static readonly IngredientInfo[][] PeachyCost = new IngredientInfo[15][];

    // TODO: Dynamic catalysts
    static ItemEnchantManager() {
        OpheliaCost[0] = Build(802, 4, 20);
        OpheliaCost[1] = Build(802, 4, 20);
        OpheliaCost[2] = Build(802, 4, 20);
        OpheliaCost[3] = Build(1100, 4, 28);
        OpheliaCost[4] = Build(1100, 4, 28);
        OpheliaCost[5] = Build(1396, 6, 34);
        OpheliaCost[6] = Build(1694, 6, 42);
        OpheliaCost[7] = Build(1990, 6, 50);
        OpheliaCost[8] = Build(4576, 6, 58);
        OpheliaCost[9] = Build(4576, 11, 64);
        OpheliaCost[10] = Build(4576, 11, 74);
        OpheliaCost[11] = Build(6864, 18, 74);
        OpheliaCost[12] = Build(6864, 18, 74);
        OpheliaCost[13] = Build(6864, 18, 74);
        OpheliaCost[14] = Build(6864, 18, 74);

        PeachyCost[0] = Build(802, 4, 20); // 1x
        PeachyCost[1] = Build(802, 4, 20); // 1x
        PeachyCost[2] = Build(802, 4, 20); // 1x
        PeachyCost[3] = Build(579, 2, 15); // 2x
        PeachyCost[4] = Build(611, 2, 16); // 2x
        PeachyCost[5] = Build(872, 4, 21); // 2x
        PeachyCost[6] = Build(604, 2, 15); // 4x
        PeachyCost[7] = Build(826, 3, 21); // 4x
        PeachyCost[8] = Build(1133, 3, 29); // 4x
        PeachyCost[9] = Build(2238, 6, 31); // 5x
        PeachyCost[10] = Build(4828, 13, 78); // 3x
        PeachyCost[11] = Build(6148, 16, 66); // 5x
        PeachyCost[12] = Build(7347, 19, 79); // 5x
        PeachyCost[13] = Build(6374, 17, 69); // 8x
        PeachyCost[14] = Build(7784, 20, 84); // 8x

        IngredientInfo[] Build(int onyx, int chaosOnyx, int crystalFragment) {
            return [
                new IngredientInfo(ItemTag.Onix, onyx),
                new IngredientInfo(ItemTag.ChaosOnix, chaosOnyx),
                new IngredientInfo(ItemTag.CrystalPiece, crystalFragment),
            ];
        }
    }

    private readonly GameSession session;
    private readonly Lua.Lua lua;
    private readonly ILogger logger = Log.Logger.ForContext<ItemEnchantManager>();

    public EnchantType Type { get; private set; }

    private Item? upgradeItem;
    private readonly List<IngredientInfo> catalysts;
    private readonly Dictionary<long, Item> fodders;
    private readonly Dictionary<BasicAttribute, BasicOption> attributeDeltas;
    private readonly EnchantRates rates;
    private int fodderWeight;
    private int useCharges;

    public ItemEnchantManager(GameSession session, Lua.Lua lua) {
        this.session = session;
        this.lua = lua;

        catalysts = new List<IngredientInfo>();
        fodders = new Dictionary<long, Item>();
        attributeDeltas = new Dictionary<BasicAttribute, BasicOption>();
        rates = new EnchantRates();
    }

    public void Reset() {
        Type = EnchantType.None;
        upgradeItem = null;
        catalysts.Clear();
        fodders.Clear();
        attributeDeltas.Clear();
        rates.Clear();
        fodderWeight = 0;
        useCharges = 0;
    }

    public bool StageItem(EnchantType enchantType, long itemUid) {
        if (enchantType is not (EnchantType.Ophelia or EnchantType.Peachy)) {
            return false;
        }

        Item? item = session.Item.GetGear(itemUid);
        if (item == null || !item.Metadata.Limit.EnableEnchant) {
            session.Send(ItemEnchantPacket.Error(ItemEnchantError.s_itemenchant_invalid_item));
            return false;
        }

        Reset();
        Type = enchantType;
        upgradeItem = item;

        // Ensure this item has an Enchant field set.
        upgradeItem.Enchant ??= new ItemEnchant();
        int enchants = Math.Clamp(upgradeItem.Enchant.Enchants, 0, 14);
        int minFodder = RequireFodder[enchants];

        foreach (IngredientInfo ingredient in GetRequiredCatalysts()) {
            catalysts.Add(ingredient);
        }
        foreach ((BasicAttribute attribute, BasicOption option) in GetBasicOptions(upgradeItem)) {
            if (upgradeItem.Enchant.BasicOptions.TryGetValue(attribute, out BasicOption existing)) {
                attributeDeltas[attribute] = option - existing;
            } else {
                attributeDeltas[attribute] = option;
            }
        }

        switch (Type) {
            case EnchantType.Ophelia:
                rates.Success = SuccessRate[enchants];
                break;
            case EnchantType.Peachy:
                rates.Success = MAX_RATE;
                break;
        }

        session.Send(ItemEnchantPacket.StageItem(Type, upgradeItem, catalysts, attributeDeltas, rates, minFodder));
        return true;
    }

    public bool UpdateFodder(long itemUid, bool add) {
        if (upgradeItem == null) {
            return false;
        }

        int enchants = Math.Clamp(upgradeItem.Enchant?.Enchants ?? 0, 0, 14);
        if (FodderRate[enchants] <= 0) {
            return false; // If adding fodder does not improve rate, restrict it.
        }

        if (add) {
            // Prevent adding more fodder if it won't help.
            if (Type is EnchantType.Ophelia && rates.Total >= MAX_RATE) {
                return false;
            }
            // Cannot add the same fodder twice.
            if (fodders.ContainsKey(itemUid)) {
                return false;
            }

            Item? item = session.Item.Inventory.Get(itemUid);
            if (item == null) {
                session.Send(ItemEnchantPacket.Error(ItemEnchantError.s_itemenchant_lack_ingredient));
                return false;
            }
            if (!IsValidFodder(upgradeItem, item)) {
                logger.Debug("Can't add fodder, invalid item: {Id}", item.Id);
                return false;
            }

            fodders.Add(itemUid, item);
            fodderWeight += GetFodderWeight(upgradeItem, item);
        } else {
            if (!fodders.Remove(itemUid, out Item? removed)) {
                return false;
            }

            fodderWeight -= GetFodderWeight(upgradeItem, removed);
        }

        switch (Type) {
            case EnchantType.Ophelia:
                int extra = fodderWeight - RequireFodder[enchants];
                rates.Fodder = Math.Clamp(extra * FodderRate[enchants], 0, MAX_RATE);

                // Recompute charges in case we are over max.
                SetCharges(useCharges);
                session.Send(ItemEnchantPacket.UpdateCharges(fodders.Keys, useCharges, fodderWeight, rates));
                break;
            case EnchantType.Peachy:
                session.Send(ItemEnchantPacket.UpdateFodder(fodders.Keys));
                break;
        }
        return true;
    }

    public bool UpdateCharges(int count) {
        if (upgradeItem == null || count < 0 || count > upgradeItem.Enchant?.Charges) {
            return false;
        }

        SetCharges(count);
        session.Send(ItemEnchantPacket.UpdateCharges(fodders.Keys, useCharges, fodderWeight, rates));
        return true;
    }

    public bool Enchant(long itemUid) {
        Item? item = session.Item.GetGear(itemUid);
        if (item == null || upgradeItem == null || upgradeItem.Uid != item.Uid) {
            return false;
        }

        upgradeItem.Enchant ??= new ItemEnchant();
        int enchants = Math.Clamp(upgradeItem.Enchant.Enchants, 0, 14);
        if (fodderWeight < RequireFodder[enchants]) {
            return false;
        }

        if (!ConsumeMaterial()) {
            return false;
        }

        switch (Type) {
            case EnchantType.Ophelia:
                float roll = Random.Shared.NextSingle() * MAX_RATE;
                int totalRate = rates.Total;
                bool success = roll < totalRate;
                logger.Debug("Enchant result: {Roll} / {Total} = {Result}", roll, totalRate, success);

                if (success) {
                    // GetBasicOptions() again to ensure rates match those in table.
                    // This *MUST* be called before incrementing Enchants.
                    foreach ((BasicAttribute attribute, BasicOption option) in GetBasicOptions(upgradeItem)) {
                        upgradeItem.Enchant.BasicOptions[attribute] = option;
                    }
                    upgradeItem.Enchant.Enchants++;

                    session.ConditionUpdate(ConditionType.enchant_result, codeLong: (int) EnchantResult.Success, targetLong: upgradeItem.Enchant.Enchants);
                    session.Send(ItemEnchantPacket.Success(upgradeItem, attributeDeltas));
                } else {
                    session.ConditionUpdate(ConditionType.enchant_result, codeLong: (int) EnchantResult.Fail, targetLong: upgradeItem.Enchant.Enchants);
                    upgradeItem.Enchant.Charges += FailCharge[enchants];
                    session.Send(ItemEnchantPacket.Failure(upgradeItem, FailCharge[enchants]));
                }

                Reset();
                return true;
            case EnchantType.Peachy:
                upgradeItem.Enchant.EnchantExp += GainExp[enchants];
                if (upgradeItem.Enchant.EnchantExp >= MAX_EXP) {
                    upgradeItem.Enchant.EnchantExp = 0;
                    // GetBasicOptions() again to ensure rates match those in table.
                    // This *MUST* be called before incrementing Enchants.
                    foreach ((BasicAttribute attribute, BasicOption option) in GetBasicOptions(upgradeItem)) {
                        upgradeItem.Enchant.BasicOptions[attribute] = option;
                    }
                    upgradeItem.Enchant.Enchants++;

                    session.ConditionUpdate(ConditionType.enchant_result, codeLong: (int) EnchantResult.Success, targetLong: upgradeItem.Enchant.Enchants);
                    session.Send(ItemEnchantPacket.Success(upgradeItem, attributeDeltas));
                    session.Send(ItemEnchantPacket.UpdateExp(itemUid, 0));
                    Reset();
                } else {
                    // Just clear the fodder as the same details are reused.
                    fodders.Clear();
                    fodderWeight = 0;

                    session.Send(ItemEnchantPacket.UpdateExp(itemUid, upgradeItem.Enchant.EnchantExp));
                }

                return true;
            default:
                return false;
        }
    }

    // session.Item must be locked before calling.
    // Consumes materials and charges for this upgrade.
    private bool ConsumeMaterial() {
        if (upgradeItem?.Enchant == null) {
            return false;
        }

        lock (session.Item) {
            // Validate charges, fodder, catalyst
            if (upgradeItem.Enchant.Charges < useCharges) {
                return false;
            }
            foreach ((long fodderUid, _) in fodders) {
                Item? item = session.Item.Inventory.Get(fodderUid);
                if (item == null) {
                    session.Send(ItemEnchantPacket.Error(ItemEnchantError.s_itemenchant_lack_ingredient));
                    return false;
                }
            }

            // Note: All validation should be above this point.
            if (!session.Item.Inventory.Consume(catalysts)) {
                session.Send(ItemEnchantPacket.Error(ItemEnchantError.s_itemenchant_lack_ingredient));
                return false;
            }

            foreach ((long fodderUid, _) in fodders) {
                if (!session.Item.Inventory.Consume(fodderUid, 1)) {
                    logger.Fatal("Failed to consume item {ItemUid}", fodderUid);
                    throw new InvalidOperationException($"Fatal: Consuming item: {fodderUid}");
                }
            }

            upgradeItem.Enchant.Charges -= useCharges;

            return true;
        }
    }

    private IEnumerable<IngredientInfo> GetRequiredCatalysts() {
        if (upgradeItem == null) {
            yield break;
        }

        float ratio = 1f;
        if (upgradeItem.Type.IsWeapon) {
            if (upgradeItem.Type.IsDagger || upgradeItem.Type.IsThrowingStar) {
                ratio = 0.5f;
            } else {
                ratio = 1f; // All other weapons
            }
        } else if (upgradeItem.Type.IsArmor) {
            if (upgradeItem.Type.IsOverall) {
                ratio = 1f;
            }
            if (upgradeItem.Type.IsClothes || upgradeItem.Type.IsPants) {
                ratio = 0.5f;
            } else if (upgradeItem.Type.IsHat) {
                ratio = 0.375f;
            } else if (upgradeItem.Type.IsGloves || upgradeItem.Type.IsShoes) {
                ratio = 0.125f;
            }
        }

        int enchants = upgradeItem.Enchant?.Enchants ?? 0;
        IEnumerable<IngredientInfo> costs = Type == EnchantType.Peachy ? PeachyCost[enchants] : OpheliaCost[enchants];
        foreach (IngredientInfo ingredient in costs) {
            IngredientInfo modified = ingredient * ratio;
            if (modified.Amount > 0) {
                yield return modified;
            }
        }
    }

    // TODO: Dynamic attribute options
    public static IEnumerable<(BasicAttribute, BasicOption)> GetBasicOptions(Item upgradeItem, int target = -1) {
        int enchants = Math.Clamp(target > 0 ? target - 1 : upgradeItem.Enchant?.Enchants ?? 0, 0, 14);
        if (upgradeItem.Type.IsWeapon) {
            yield return (BasicAttribute.MinWeaponAtk, new BasicOption(StatBonusRate[enchants]));
            yield return (BasicAttribute.MaxWeaponAtk, new BasicOption(StatBonusRate[enchants]));
        } else if (upgradeItem.Type.IsArmor) {
            yield return (BasicAttribute.Defense, new BasicOption(StatBonusRate[enchants]));
        }
    }

    // Prevent user from using more charges than needed
    private void SetCharges(int count) {
        // Charges are only relevant to Ophelia.
        if (Type != EnchantType.Ophelia) {
            useCharges = 0;
            return;
        }

        int rateWithoutCharges = Math.Clamp(rates.Total - rates.Charge, 0, MAX_RATE);
        int maxCharges = (int) Math.Ceiling((MAX_RATE - rateWithoutCharges) / (float) CHARGE_RATE);
        useCharges = Math.Clamp(count, 0, maxCharges);
        rates.Charge = Math.Clamp(useCharges * CHARGE_RATE, 0, MAX_RATE);
    }

    private static int GetFodderWeight(Item item, Item fodder) {
        if (!item.Type.IsDagger && !item.Type.IsThrowingStar) {
            return 1;
        }

        // For Dagger/ThrowingStar, Toad's Toolkit is valued as 2 fodder.
        return IsValidToolkit(item, fodder) ? 2 : 1;
    }

    private static bool IsValidFodder(Item item, Item fodder) {
        return fodder.Id == item.Id && fodder.Rarity == item.Rarity || IsValidToolkit(item, fodder);
    }

    private static bool IsValidToolkit(Item item, Item fodder) {
        return fodder.Metadata.Property.Tag switch {
            ItemTag.EnchantJockerItemNormal => item.Rarity == 1,
            ItemTag.EnchantJockerItemRare => item.Rarity == 2,
            ItemTag.EnchantJockerItemElite => item.Rarity == 3,
            ItemTag.EnchantJockerItemExcellent => item.Rarity == 4,
            ItemTag.EnchantJockerItemLegendary => item.Rarity == 5,
            ItemTag.EnchantJockerItemEpic => item.Rarity == 6,
            _ => false,
        };
    }
}
