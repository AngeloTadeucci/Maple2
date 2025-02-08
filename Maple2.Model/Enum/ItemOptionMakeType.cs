namespace Maple2.Model.Enum;

public enum ItemOptionMakeType {
    Base = 0, // uses itemoptionvariation table
    Range = 1, // uses itemoptionvariation_* tables
    Lua = 2, // uses lua  functions
}
