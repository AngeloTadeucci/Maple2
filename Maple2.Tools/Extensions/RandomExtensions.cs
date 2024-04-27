using System;

namespace Maple2.Tools.Extensions;

public static class RandomExtensions {
    public static void Shuffle<T>(this Random rng, T[] array) {
        int length = array.Length;
        while (length > 1) {
            int k = rng.Next(length--);
            (array[length], array[k]) = (array[k], array[length]);
        }
    }
}
