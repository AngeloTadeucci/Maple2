using Maple2.Database.Context;
using Maple2.Model.Game.Field;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace Maple2.Database.Storage.Metadata;

public class MapDataStorage(MetadataContext context) : MetadataStorage<string, FieldAccelerationStructure>(context, CACHE_SIZE) {
    private const int CACHE_SIZE = 500; // ~500 total items

    public bool TryGet(string xblock, [NotNullWhen(true)] out FieldAccelerationStructure? mapData) {
        if (Cache.TryGet(xblock, out mapData)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet(xblock, out mapData)) {
                return true;
            }

            MapDataMetadata? data = Context.MapDataMetadata.Find(xblock);

            if (data == null) {
                return false;
            }

            MemoryStream input = new MemoryStream(data.Data);
            MemoryStream output = new MemoryStream();

            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress)) {
                dstream.CopyTo(output);
            }

            ByteReader reader = new ByteReader(output.ToArray());

            mapData = reader.ReadClass<FieldAccelerationStructure>();

            Cache.AddReplace(xblock, mapData);
        }

        return true;
    }
}
