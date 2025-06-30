using System.Runtime.InteropServices;
using Maple2.Model.Enum;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public readonly record struct BasicOption(int Value) {
    public BasicOption(float percent) : this((int) percent * 100) { }

    public static BasicOption operator +(BasicOption self, BasicOption other) {
        return new BasicOption(self.Value + other.Value);
    }

    public static BasicOption operator -(BasicOption self, BasicOption other) {
        return new BasicOption(Math.Max(self.Value - other.Value, 0));
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
public readonly record struct SpecialOption(float Rate) {
    public static SpecialOption operator +(SpecialOption self, SpecialOption other) {
        return new SpecialOption(self.Rate + other.Rate);
    }

    public static SpecialOption operator -(SpecialOption self, SpecialOption other) {
        return new SpecialOption(Math.Max(self.Rate - other.Rate, 0));
    }
}

public readonly struct LockOption {
    private readonly BasicAttribute? basic;
    private readonly SpecialAttribute? special;
    private readonly bool lockValue;

    public LockOption(BasicAttribute attribute, bool lockValue = false) {
        basic = attribute;
        this.lockValue = lockValue;
    }

    public LockOption(SpecialAttribute attribute, bool lockValue = false) {
        special = attribute;
        this.lockValue = lockValue;
    }

    public bool TryGet(out BasicAttribute attribute, out bool valueLocked) {
        valueLocked = lockValue;
        if (basic == null) {
            attribute = 0;
            return false;
        }

        attribute = (BasicAttribute) basic;
        return true;
    }

    public bool TryGet(out SpecialAttribute attribute, out bool valueLocked) {
        valueLocked = lockValue;
        if (special == null) {
            attribute = 0;
            return false;
        }

        attribute = (SpecialAttribute) special;
        return true;
    }
}
