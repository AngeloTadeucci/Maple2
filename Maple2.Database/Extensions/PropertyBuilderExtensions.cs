using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Extensions;

internal static class PropertyBuilderExtensions {
    private static readonly JsonSerializerOptions Options = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    static PropertyBuilderExtensions() {
        Options.Converters.Add(new Vector3Converter());
    }

    public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> builder) {
        return builder.HasConversion(
            property => JsonSerializer.Serialize(property, Options),
            value => JsonSerializer.Deserialize<TProperty>(value, Options)!
        ).HasColumnType("json");
    }

    public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> OneToOne<TEntity, TRelatedEntity>(
            this EntityTypeBuilder<TEntity> builder) where TEntity : class where TRelatedEntity : class {
        return builder.HasOne<TRelatedEntity>().WithOne();
    }

    public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> OneToOne<TEntity, TRelatedEntity>(
            this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression)
            where TEntity : class where TRelatedEntity : class {
        return builder.HasOne<TRelatedEntity>(navigationExpression).WithOne();
    }
}
