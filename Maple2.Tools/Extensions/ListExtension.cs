using System;
using System.Collections.Generic;
using System.Linq;

namespace Maple2.Tools.Extensions;

public static class ListExtension {
    public static int AddSorted<T>(this List<T> @this, T item) where T: IComparable<T> {
        return @this.AddSorted(item, Comparer<T>.Create((item, item2) => item.CompareTo(item2)));
    }

    public static int AddSorted<T>(this List<T> @this, T item, Comparer<T> comparer) {
        if (@this.Count == 0) {
            @this.Add(item);

            return @this.Count;
        }

        if (comparer.Compare(@this.Last(), item) <= 0) {
            @this.Add(item);

            return @this.Count;
        }

        if (comparer.Compare(@this.First(), item) >= 0) {
            @this.Insert(0, item);

            return 0;
        }

        int index = @this.BinarySearch(item, comparer);

        if (index < 0) {
            index = ~index;
        }

        @this.Insert(index, item);

        return index;
    }

    public static void RemoveSorted<T>(this List<T> @this, T item) where T : IComparable<T> {
        @this.RemoveSorted(item, Comparer<T>.Create((item, item2) => item.CompareTo(item2)));
    }

    public static void RemoveSorted<T>(this List<T> @this, T item, Comparer<T> comparer) {
        if (@this.Count == 0) {
            return;
        }

        if (comparer.Compare(@this.Last(), item) > 0) {
            return;
        }

        if (comparer.Compare(@this.First(), item) < 0) {
            return;
        }

        int index = @this.BinarySearch(item, comparer);

        if (index >= 0 && index < @this.Count && (@this[index]?.Equals(item) ?? false)) {
            @this.RemoveAt(index);
        }
    }
}

