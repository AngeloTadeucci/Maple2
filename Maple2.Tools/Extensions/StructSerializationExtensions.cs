using System;
using System.Runtime.CompilerServices;
using Maple2.PacketLib.Tools;

namespace Maple2.Tools.Extensions;

// Separate class to avoid name conflicts
public static class StructSerializationExtensions {
    public static unsafe byte[] Serialize<T>(this T value) where T : struct {
        int size = Unsafe.SizeOf<T>();
        byte[] bytes = new byte[size];
        fixed (byte* ptr = bytes) {
            Unsafe.Write<T>(ptr, value);
        }

        return bytes;
    }

    public static unsafe T Deserialize<T>(this byte[] bytes) where T : struct {
        int size = Unsafe.SizeOf<T>();
        if (bytes.Length != size) {
            throw new ArgumentException($"Cannot convert {bytes.ToHexString(' ')} to {nameof(T)}");
        }

        fixed (byte* ptr = bytes) {
            return Unsafe.Read<T>(ptr);
        }
    }
}
