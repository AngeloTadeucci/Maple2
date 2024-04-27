using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Maple2.Database.Extensions;

public static class DbContextExtensions {
    public static string? GetTableName<T>(this DbContext context) where T : class {
        IEntityType entityType = context.Model.GetEntityTypes().First(type => type.ClrType == typeof(T));

        IAnnotation tableNameAnnotation = entityType.GetAnnotation("Relational:TableName");
        return tableNameAnnotation.Value?.ToString();
    }

    public static bool TrySaveChanges(this DbContext context, bool autoAccept = true) {
        try {
            context.SaveChanges(autoAccept);
            return true;
        } catch (Exception ex) {
            Console.WriteLine($"> Failed {context.ContextId}");
            Console.WriteLine(ex);
            return false;
        }
    }

    internal static void DisplayStates(this IEnumerable<EntityEntry> entries) {
        foreach (EntityEntry entry in entries) {
            Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State.ToString()} ");
        }
    }

    public static void Overwrite<T>(this DbContext context, T entity) where T : class {
        context.Entry(entity).State = EntityState.Modified;
    }
}
