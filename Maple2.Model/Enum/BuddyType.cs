namespace Maple2.Model.Enum;

[Flags]
public enum BuddyType : byte {
    Default = 0,
    InRequest = 1,
    OutRequest = 2,
    Blocked = 4,
}
