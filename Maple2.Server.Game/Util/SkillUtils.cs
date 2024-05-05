using System.Numerics;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Tools.Collision;

namespace Maple2.Server.Game.Util;

public static class SkillUtils {
    public static Prism GetPrism(this SkillMetadataRange range, in Vector3 position, float angle) {
        if (range.Type == SkillRegion.None) {
            return new Prism(IPolygon.Null, 0, 0);
        }

        var origin = new Vector2(position.X, position.Y);
        IPolygon polygon = range.Type switch {
            SkillRegion.Box => new Rectangle(origin, range.Width + range.RangeAdd.X, range.Distance + range.RangeAdd.Y, angle),
            SkillRegion.Cylinder => new Circle(origin, range.Distance),
            SkillRegion.Frustum => new Trapezoid(origin, range.Width, range.EndWidth, range.Distance, angle),
            SkillRegion.HoleCylinder => new HoleCircle(origin, range.Width, range.EndWidth),
            _ => throw new ArgumentOutOfRangeException($"Invalid range type: {range.Type}"),
        };

        return new Prism(polygon, position.Z, range.Height + range.RangeAdd.Z);
    }

    public static IEnumerable<T> Filter<T>(this Prism prism, IEnumerable<T> entities, int limit = 10) where T : IActor {
        foreach (T entity in entities) {
            if (limit <= 0) {
                yield break;
            }

            if (entity.IsDead) {
                continue;
            }

            IPrism shape = entity.Shape;
            if (prism.Intersects(shape)) {
                limit--;
                yield return entity;
            }
        }
    }

    public static IEnumerable<T> Filter<T>(this Prism[] prisms, IEnumerable<T> entities, int limit = 10, ICollection<IActor>? ignore = null) where T : IActor {
        foreach (T entity in entities) {
            if (limit <= 0) {
                yield break;
            }

            if (entity.IsDead) {
                continue;
            }
            if (ignore != null && ignore.Contains(entity)) {
                continue;
            }

            IPrism shape = entity.Shape;
            foreach (Prism prism in prisms) {
                if (prism.Intersects(shape)) {
                    limit--;
                    yield return entity;
                }
            }
        }
    }

    public static bool Check(this BeginCondition condition, IActor caster, IActor owner, IActor target) {
        if (caster is FieldPlayer player) {
            if (condition is not { Probability: 1 } && condition.Probability < Random.Shared.NextDouble()) {
                return false;
            }
            if (condition.OnlyShadowWorld && caster.Field.Metadata.Property.Type != MapType.Shadow) {
                return false;
            }
            if (condition.OnlyFlyableMap && !caster.Field.Metadata.Property.CanFly) {
                return false;
            }
            if (player.Value.Character.Level < condition.Level) {
                return false;
            }
            if (player.Session.Currency.Meso < condition.Mesos) {
                return false;
            }
            if (condition.Gender != Gender.All && player.Value.Character.Gender != condition.Gender) {
                return false;
            }
            if (condition.JobCode.Length > 0 && !condition.JobCode.Contains(player.Value.Character.Job.Code())) {
                return false;
            }
            if (condition.Weapon?.Length > 0) {
                if (!condition.Weapon.Any(weapon => weapon.Check(player))) {
                    return false;
                }
            }
            foreach ((BasicAttribute stat, long value) in condition.Stat) {
                if (player.Stats[stat].Total < value) {
                    return false;
                }
            }
        }

        return condition.Caster.Check(caster) && condition.Owner.Check(owner) && condition.Target.Check(target);
    }

    private static bool Check(this BeginConditionTarget? condition, IActor target) {
        if (condition == null) {
            return true;
        }

        foreach ((int id, short level, bool owned, int count, CompareType compare) in condition.Buff) {
            if (!target.Buffs.Buffs.TryGetValue(id, out Buff? buff)) {
                return false;
            }
            if (buff.Level < level) {
                return false;
            }
            if (owned && buff.Owner.ObjectId == 0) {
                return false;
            }

            bool compareResult = compare switch {
                CompareType.Equals => buff.Stacks == count,
                CompareType.Less => buff.Stacks < count,
                CompareType.LessEquals => buff.Stacks <= count,
                CompareType.Greater => buff.Stacks > count,
                CompareType.GreaterEquals => buff.Stacks >= count,
                _ => true,
            };
            if (!compareResult) {
                return false;
            }
        }

        return true;
    }

    private static bool Check(this BeginConditionWeapon weapon, FieldPlayer player) {
        return IsValid(weapon.LeftHand, EquipSlot.LH) && IsValid(weapon.RightHand, EquipSlot.RH);

        bool IsValid(ItemType itemType, EquipSlot slot) {
            if (itemType.Type == 0) return true;
            Item? handItem = player.Session.Item.Equips.Get(slot);
            return handItem != null && handItem.Type.Type == itemType.Type;
        }
    }
}
