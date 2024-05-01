namespace Maple2.Model.Enum;

// These could be flags but not sure
public enum EnchantResult : byte {
    None = 0,
    Success = 1,
    Fail = 2,
    Unknown3 = 3, // Considered failure as well
    Destabilize = 4,
    Unknown5 = 5, // Considered failture as well
}
