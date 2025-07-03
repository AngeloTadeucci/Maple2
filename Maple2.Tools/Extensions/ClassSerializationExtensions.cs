using System.Collections.Generic;
using Maple2.PacketLib.Tools;

namespace Maple2.Tools.Extensions;

public static class ClassSerializationExtensions {
    public static byte[] Serialize<T>(this T value) where T : IByteSerializable {
        using var writer = new PoolByteWriter();
        writer.WriteClass<T>(value);
        return writer.ToArray();
    }

    public static T Deserialize<T>(this byte[] bytes) where T : IByteDeserializable {
        var reader = new ByteReader(bytes);
        return reader.ReadClass<T>();
    }

    public static byte[] SerializeCollection<T>(this ICollection<T> collection) where T : struct {
        using var writer = new PoolByteWriter();
        writer.WriteCollection(collection);
        return writer.ToArray();
    }

    public static ICollection<T> DeserializeCollection<T>(this byte[] bytes) where T : struct {
        var reader = new ByteReader(bytes);
        return reader.ReadCollection<T>();
    }

    public static TCollection DeserializeCollection<TCollection, T>(this byte[] bytes) where TCollection : ICollection<T>, new() where T : struct {
        var reader = new ByteReader(bytes);
        return reader.ReadCollection<TCollection, T>();
    }

    public static byte[] SerializeDictionary<TK, TV>(this IDictionary<TK, TV> dictionary) where TK : struct where TV : struct {
        using var writer = new PoolByteWriter();
        writer.WriteInt(dictionary.Count);
        foreach ((TK key, TV value) in dictionary) {
            writer.Write<TK>(key);
            writer.Write<TV>(value);
        }
        return writer.ToArray();
    }

    public static IDictionary<TK, TV> DeserializeDictionary<TK, TV>(this byte[] bytes) where TK : struct where TV : struct {
        var reader = new ByteReader(bytes);
        Dictionary<TK, TV> dictionary = new Dictionary<TK, TV>();
        int count = reader.ReadInt();
        for (int i = 0; i < count; i++) {
            var key = reader.Read<TK>();
            var value = reader.Read<TV>();
            dictionary[key] = value;
        }
        return dictionary;
    }
}
