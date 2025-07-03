using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Context;
using Maple2.Model.Metadata;
using Maple2.Tools;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Storage;

public class ItemMetadataStorage : MetadataStorage<int, ItemMetadata>, ISearchable<ItemMetadata> {
    private const int CACHE_SIZE = 40000; // ~34k total items

    private readonly ConcurrentDictionary<int, ItemMetadata> petToItem = new();
    private readonly ConcurrentMultiDictionary<int, int, PetMetadata> petLookup = new();

    public ItemMetadataStorage(MetadataContext context) : base(context, CACHE_SIZE) {
        IndexPets();

        foreach (PetMetadata metadata in Context.PetMetadata) {
            petLookup.TryAdd(metadata.Id, metadata.NpcId, metadata);
        }
    }

    public bool TryGet(int id, [NotNullWhen(true)] out ItemMetadata? item) {
        if (Cache.TryGet(id, out item)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet(id, out item)) {
                return true;
            }

            item = Context.ItemMetadata.Find(id);

            if (item == null) {
                return false;
            }

            Cache.AddReplace(id, item);
        }

        return true;
    }

    public bool TryGetPet(int petId, [NotNullWhen(true)] out ItemMetadata? item) {
        return petToItem.TryGetValue(petId, out item);
    }

    public bool TryGetPet(int petId, [NotNullWhen(true)] out PetMetadata? pet) {
        return petLookup.TryGetKey1(petId, out pet);
    }

    public bool TryGetPetByNpcId(int npcId, [NotNullWhen(true)] out PetMetadata? pet) {
        return petLookup.TryGetKey2(npcId, out pet);
    }

    public override void InvalidateCache() {
        base.InvalidateCache();
        IndexPets();
    }

    public List<ItemMetadata> Search(string name) {
        lock (Context) {
            return Context.ItemMetadata
                .Where(item => EF.Functions.Like(item.Name!, $"%{name}%"))
                .ToList();
        }
    }

    private void IndexPets() {
        petToItem.Clear();

        lock (Context) {
            List<ItemMetadata> result = Context.ItemMetadata
                .FromSql($"SELECT * FROM `item` WHERE JSON_EXTRACT(Property, '$.PetId') > 0")
                .ToList();

            foreach (ItemMetadata item in result) {
                Cache.AddReplace(item.Id, item);
                petToItem[item.Property.PetId] = item;
            }
        }
    }
}
