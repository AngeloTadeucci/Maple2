using System;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PathEngine;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;

namespace Maple2.Server.Game.Model;

public sealed class FieldPet : FieldNpc {
    private readonly FieldPlayer? owner;
    public readonly Item Pet;

    public int OwnerId => owner?.ObjectId ?? 0;
    public int SkinId { get; private set; }

    public readonly float Scale = 1f;

    public short TamingRank; // (0-70=green, 70-95=yellow, 95-100=red)
    public int TamingPoint = 1000;

    public FieldPet(FieldManager field, int objectId, Agent agent, Npc npc, Item pet, FieldPlayer? owner = null) : base(field, objectId, agent, npc) {
        this.owner = owner;
        Pet = pet;

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

    public override void ApplyDamage(IActor caster, DamageRecord damage, SkillMetadataAttack attack) {
        if (attack.Pet == null) {
            return;
        }

        var targetRecord = new DamageRecordTarget {
            ObjectId = ObjectId,
        };
        int damageAmount = TamingPoint - Math.Min(TamingPoint + attack.Pet.TamingPoint, Constant.TamingPetMaxPoint);
        TamingPoint -= damageAmount;
        targetRecord.AddDamage(damageAmount == 0 ? DamageType.Miss : DamageType.Normal, damageAmount);
        if (damageAmount != 0) {
            Field.Broadcast(PetPacket.SyncTaming(caster.ObjectId, this));
        }

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

        damage.Targets.Add(targetRecord);
    }

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
