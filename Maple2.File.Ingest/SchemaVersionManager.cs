using System.Text;
using Force.Crc32;
using Maple2.Database.Model.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Maple2.File.Ingest;

public static class SchemaVersionManager {
    public static bool ShouldRecreateDatabase(DbContext context) {
        string currentSchemaHash = GenerateSchemaHash(context);

        try {
            SchemaVersion? schemaVersion = context.Set<SchemaVersion>().FirstOrDefault();

            if (schemaVersion == null) {
                Console.WriteLine("No schema version found.");
                return true;
            }

            if (schemaVersion.SchemaHash != currentSchemaHash) {
                Console.WriteLine("Schema has changed, database will be recreated.");
                return true;
            }

            return false;
        } catch {
            // Table doesn't exist or there was an error
            return true;
        }
    }

    public static void StoreSchemaVersion(DbContext context) {
        string hash = GenerateSchemaHash(context);

        // Remove any existing schema versions
        foreach (SchemaVersion existing in context.Set<SchemaVersion>().ToList()) {
            context.Remove(existing);
        }

        // Add new schema version
        context.Add(new SchemaVersion {
            SchemaHash = hash,
            UpdatedAt = DateTime.UtcNow,
        });

        context.SaveChanges();
    }

    private static string GenerateSchemaHash(DbContext context) {
        IModel model = context.Model;
        IEnumerable<IEntityType> entityTypes = model.GetEntityTypes();

        var schemaDefinition = new StringBuilder();

        foreach (IEntityType entityType in entityTypes.OrderBy(e => e.Name)) {
            schemaDefinition.AppendLine(entityType.Name);

            // Add properties
            foreach (IProperty property in entityType.GetProperties().OrderBy(p => p.Name)) {
                schemaDefinition.AppendLine($"  {property.Name}:{property.ClrType.Name}:{property.GetColumnName()}");
            }

            // Add relationships
            foreach (INavigation navigation in entityType.GetNavigations().OrderBy(n => n.Name)) {
                schemaDefinition.AppendLine($"  Nav:{navigation.Name}:{navigation.TargetEntityType.Name}");
            }
        }

        // Compute hash using the same CRC32C algorithm used elsewhere
        return Crc32CAlgorithm.Compute(Encoding.UTF8.GetBytes(schemaDefinition.ToString())).ToString();
    }
}
