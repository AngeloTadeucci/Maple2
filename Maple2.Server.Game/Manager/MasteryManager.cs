using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Manager;

public class MasteryManager {

    private readonly GameSession session;
    private readonly Lua.Lua lua;
    private Mastery Mastery => session.Player.Value.Character.Mastery;
    private IDictionary<int, int> gatheringCounts => session.Config.GatheringCounts;

    public MasteryManager(GameSession session, Lua.Lua lua) {
        this.session = session;
        this.lua = lua;
    }

    public int this[MasteryType type] {
        get => type switch {
            MasteryType.Fishing => Mastery.Fishing,
            MasteryType.Music => Mastery.Instrument,
            MasteryType.Mining => Mastery.Mining,
            MasteryType.Gathering => Mastery.Foraging,
            MasteryType.Breeding => Mastery.Ranching,
            MasteryType.Farming => Mastery.Farming,
            MasteryType.Blacksmithing => Mastery.Smithing,
            MasteryType.Engraving => Mastery.Handicrafts,
            MasteryType.Alchemist => Mastery.Alchemy,
            MasteryType.Cooking => Mastery.Cooking,
            MasteryType.PetTaming => Mastery.PetTaming,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid mastery type."),
        };
        set {
            short startLevel = GetLevel(type);
            switch (type) {
                case MasteryType.Fishing:
                    Mastery.Fishing = Math.Clamp(value, Mastery.Fishing, Constant.FishingMasteryMax);
                    break;
                case MasteryType.Music:
                    Mastery.Instrument = Math.Clamp(value, Mastery.Instrument, Constant.PerformanceMasteryMax);
                    break;
                case MasteryType.Mining:
                    Mastery.Mining = Math.Clamp(value, Mastery.Mining, Constant.MiningMasteryMax);
                    break;
                case MasteryType.Gathering:
                    Mastery.Foraging = Math.Clamp(value, Mastery.Foraging, Constant.ForagingMasteryMax);
                    break;
                case MasteryType.Breeding:
                    Mastery.Ranching = Math.Clamp(value, Mastery.Ranching, Constant.RanchingMasteryMax);
                    break;
                case MasteryType.Farming:
                    Mastery.Farming = Math.Clamp(value, Mastery.Farming, Constant.FarmingMasteryMax);
                    break;
                case MasteryType.Blacksmithing:
                    Mastery.Smithing = Math.Clamp(value, Mastery.Smithing, Constant.SmithingMasteryMax);
                    break;
                case MasteryType.Engraving:
                    Mastery.Handicrafts = Math.Clamp(value, Mastery.Handicrafts, Constant.HandicraftsMasteryMax);
                    break;
                case MasteryType.Alchemist:
                    Mastery.Alchemy = Math.Clamp(value, Mastery.Alchemy, Constant.AlchemyMasteryMax);
                    break;
                case MasteryType.Cooking:
                    Mastery.Cooking = Math.Clamp(value, Mastery.Cooking, Constant.CookingMasteryMax);
                    break;
                case MasteryType.PetTaming:
                    Mastery.PetTaming = Math.Clamp(value, Mastery.PetTaming, Constant.PetTamingMasteryMax);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid mastery type.");
            }

            session.Send(MasteryPacket.UpdateMastery(type, session.Mastery[type]));
            if (startLevel < GetLevel(type)) {
                session.ConditionUpdate(ConditionType.mastery_grade, codeLong: (int) type);
                session.ConditionUpdate(ConditionType.set_mastery_grade, codeLong: (int) type);
                if (type == MasteryType.Music) {
                    session.ConditionUpdate(ConditionType.music_play_grade);
                }
            }
        }
    }

    public short GetLevel(MasteryType type) {
        if (!session.TableMetadata.MasteryRewardTable.Entries.TryGetValue(type, out IReadOnlyDictionary<int, MasteryRewardTable.Entry>? masteryRewardEntries)) {
            return 1;
        }

        return (short) Math.Max(1, masteryRewardEntries.OrderByDescending(mastery => mastery.Key).FirstOrDefault(mastery => session.Mastery[type] >= mastery.Value.Value).Key);
    }

    public void Gather(FieldInteract fieldInteract) {
        if (!session.TableMetadata.MasteryRecipeTable.Entries.TryGetValue(fieldInteract.Value.Item.RecipeId, out MasteryRecipeTable.Entry? recipeMetadata)) {
            session.Send(MasteryPacket.Error(MasteryError.s_mastery_error_unknown));
            return;
        }

        if (recipeMetadata.RequiredMastery > this[recipeMetadata.Type]) {
            session.Send(MasteryPacket.Error(MasteryError.s_mastery_error_lack_mastery));
            return;
        }

        if (!gatheringCounts.TryGetValue(recipeMetadata.Id, out int currentCount)) {
            gatheringCounts[recipeMetadata.Id] = 0;
        }

        // TODO: Implement the bool on the last parameter of the formula. Is in someone else's home ?
        float successRate = lua.CalcGatheringObjectSuccessRate(currentCount, recipeMetadata.HighRateLimitCount, recipeMetadata.NormalRateLimitCount);

        int gatheringAmount = 0;
        switch (recipeMetadata.Type) {
            case MasteryType.Farming:
            case MasteryType.Breeding:
                session.ConditionUpdate(ConditionType.mastery_harvest_try, codeLong: recipeMetadata.Id);
                session.ConditionUpdate(ConditionType.mastery_farming_try, codeLong: recipeMetadata.Id);
                break;
            case MasteryType.Gathering:
            case MasteryType.Mining:
                session.ConditionUpdate(ConditionType.mastery_gathering_try, codeLong: recipeMetadata.Id);
                break;
        }
        if (Random.Shared.NextDouble() > (successRate / 100)) {
            session.Send(InteractObjectPacket.Interact(fieldInteract, GatherResult.Fail, decreaseAmount: gatheringAmount));
            return;
        }

        foreach (ItemComponent itemReward in recipeMetadata.RewardItems) {
            Item? item = session.Field.ItemDrop.CreateItem(itemReward.ItemId, itemReward.Rarity, itemReward.Amount);
            if (item == null) {
                continue;
            }
            FieldItem fieldItem = session.Field!.SpawnItem(fieldInteract.Position, fieldInteract.Rotation, item, session.CharacterId);
            session.Field.Broadcast(FieldPacket.DropItem(fieldItem));
        }

        gatheringAmount++;
        switch (recipeMetadata.Type) {
            case MasteryType.Breeding:
            case MasteryType.Farming:
                if (session.Field!.Metadata.Property.Type == MapType.Home && session.Field.OwnerId != session.AccountId) {
                    session.ConditionUpdate(ConditionType.mastery_harvest_otherhouse, counter: gatheringAmount, codeLong: recipeMetadata.Id);
                }
                if (recipeMetadata.Type == MasteryType.Farming) {
                    session.ConditionUpdate(ConditionType.mastery_farming, counter: gatheringAmount, codeLong: recipeMetadata.Id);
                }
                session.ConditionUpdate(ConditionType.mastery_harvest, counter: gatheringAmount, codeLong: recipeMetadata.Id);
                break;
            case MasteryType.Gathering:
            case MasteryType.Mining:
                session.ConditionUpdate(ConditionType.mastery_gathering, counter: gatheringAmount, codeLong: recipeMetadata.Id);
                break;
        }

        this[recipeMetadata.Type] += recipeMetadata.RewardMastery;
        if (!recipeMetadata.NoRewardExp) {
            session.Exp.AddExp(ExpType.gathering);
        }
        gatheringCounts[recipeMetadata.Id] += gatheringAmount;
        session.Send(InteractObjectPacket.Interact(fieldInteract, decreaseAmount: gatheringAmount));


    }
}
