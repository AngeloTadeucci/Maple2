namespace Maple2.Model.Enum;

[Flags]
public enum PlayerObjectFlag : byte {
    None = 0,
    Dead = 1,
    Position = 2,
    Level = 4,
    Job = 8,
    Motto = 16,
    GearScore = 32,
    State = 64,
    All = Dead | Position | Level | Job | Motto | GearScore | State,
}
