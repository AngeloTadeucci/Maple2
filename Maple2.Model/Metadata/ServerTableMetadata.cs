using System;
using System.Text.Json.Serialization;

namespace Maple2.Model.Metadata;

public class ServerTableMetadata {
    public required string Name { get; set; }
    public required ServerTable Table { get; set; }

    protected bool Equals(ServerTableMetadata other) {
        return Name == other.Name && Table.Equals(other.Table);
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;

        return Equals((ServerTableMetadata) obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Name, Table);
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
[JsonDerivedType(typeof(InstanceFieldTable), typeDiscriminator: "instancefield")]
[JsonDerivedType(typeof(ScriptConditionTable), typeDiscriminator: "*scriptCondition")]
[JsonDerivedType(typeof(ScriptFunctionTable), typeDiscriminator: "*scriptFunction")]
[JsonDerivedType(typeof(JobConditionTable), typeDiscriminator: "jobConditionTable")]
[JsonDerivedType(typeof(BonusGameTable), typeDiscriminator: "bonusGame")]
[JsonDerivedType(typeof(GlobalDropItemBoxTable), typeDiscriminator: "globalItemDrop")]

public abstract record ServerTable;
