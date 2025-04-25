using DotRecast.Detour.Crowd;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;

namespace Maple2.Server.Game.Model;

public sealed class FieldPet : FieldNpc {
    private readonly FieldPlayer? owner;
    public readonly Item Pet;
    public readonly PetMetadata Metadata;

    public int OwnerId => owner?.ObjectId ?? 0;
    public int SkinId { get; private set; }

    public readonly float Scale = 1f;

    public short TamingRank; // (0-70=green, 70-95=yellow, 95-100=red)
    public int TamingPoint;
    private long tamingTick;

    public FieldPet(FieldManager field, int objectId, DtCrowdAgent agent, Npc npc, Item pet, string aiPath, FieldPlayer? owner = null) : base(field, objectId, agent, npc, aiPath) {
        this.owner = owner;
        Pet = pet;

        if (!field.ItemMetadata.TryGetPet(pet.Metadata.Property.PetId, out PetMetadata? petMetadata)) {
            throw new KeyNotFoundException($"Pet metadata not found for pet id {pet.Id}");
        }
        Metadata = petMetadata;

        // Wild pets need a TamingRank.
        if (owner == null) {
            TamingRank = (short) (Random.Shared.Next(100) + 1);
        }

        // Use Badge for SkinId if equipped.
        if (owner != null && owner.Session.Item.Equips.Badge.TryGetValue(BadgeType.PetSkin, out Item? value)) {
            SkinId = value.Badge?.PetSkinId ?? Value.Id;
        } else {
            SkinId = Value.Id;
        }
    }

    public override void Update(long tickCount) {
        base.Update(tickCount);

        if (TamingPoint > 0 && tamingTick < tickCount) {
            tamingTick = tickCount + (long) TimeSpan.FromSeconds(1).TotalMilliseconds;
            TamingPoint = Math.Max(0, TamingPoint - 100);
            Field.Broadcast(PetPacket.SyncTaming(BattleState.TargetId, this));
        }
    }

    /// <summary>
    /// Applies taming-related damage to the pet based on the attack, handling taming progress, trap effects, and item drops upon successful taming or forced taming.
    /// </summary>
    /// <param name="caster">The actor attempting to tame or damage the pet.</param>
    /// <param name="damage">The damage record to update with the result of the attack.</param>
    /// <param name="attack">The attack metadata, which must include pet taming data.</param>
    public override void ApplyDamage(IActor caster, DamageRecord damage, SkillMetadataAttack attack) {
        if (attack.Pet == null) {
            return;
        }

        if (BattleState.TargetId != 0 && BattleState.TargetId != caster.ObjectId && caster is FieldPlayer player) {
            player.Session.Send(NoticePacket.Notice(NoticePacket.Flags.Message | NoticePacket.Flags.Alert, new InterfaceText("s_err_taming_attack_other_user"))); // No string code for this?
            return;
        }

        var targetRecord = new DamageRecordTarget(this);
        int damageAmount = TamingPoint - Math.Min(TamingPoint + attack.Pet.TamingPoint, Constant.TamingPetMaxPoint);
        TamingPoint -= damageAmount;
        targetRecord.AddDamage(damageAmount == 0 ? DamageType.Miss : DamageType.Normal, damageAmount);

        if (attack.Pet.TrapLevel > 0) {
            if (attack.Pet.ForcedTaming) {
                IsDead = true;
                OnDeath();
                DropItem(caster);
            } else if (TamingPoint >= Constant.TamingPetMaxPoint) { // trap has chance to fail
                IsDead = true;
                OnDeath();
                DropItem(caster);
            }
        }

        damage.Targets.TryAdd(ObjectId, targetRecord);
    }

    /// <summary>
/// Removes the pet from the field after the specified delay.
/// </summary>
/// <param name="delay">The delay in milliseconds before removal.</param>
protected override void Remove(int delay) => Field.RemovePet(ObjectId, delay);

    public void UpdateSkin(int skinId) {
        SkinId = skinId > 0 ? skinId : Value.Id;
    }

    private void DropItem(IActor capturer) {
        // TODO: Determine rarity of pet drop
        int rarity = TamingRank / 25 + 1;

        using GameStorage.Request db = Field.GameStorage.Context();
        Item? pet = Pet.Mutate(Pet.Metadata, rarity);
        pet.Stats = Field.ItemStatsCalc.GetStats(pet);

        pet = db.CreateItem(0, pet);
        if (pet == null) {
            return;
        }

        FieldItem fieldItem = Field.SpawnItem(this, pet);
        if (capturer is FieldPlayer player) {
            player.Session.Send(FieldPacket.DropItem(fieldItem));
        } else {
            Field.Broadcast(FieldPacket.DropItem(fieldItem));
        }
    }
}
