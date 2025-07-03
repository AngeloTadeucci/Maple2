using System;
using System.Collections.Generic;

namespace Maple2.Tools;

public class LimitedStack<T> {
    private readonly int limit;
    private readonly Queue<T> queue;

    public LimitedStack(int limit) {
        this.limit = limit;
        queue = new Queue<T>(limit);
    }

    public LimitedStack(int limit, T startingValue) : this(limit) {
        Push(startingValue);
    }

    public void Push(T item) {
        if (queue.Count == limit) {
            queue.Dequeue(); // Remove oldest
        }
        queue.Enqueue(item);
    }

    public T Pop() {
        if (queue.Count == 0) throw new InvalidOperationException("Stack is empty");
        // To behave like a stack, remove the newest (last) item
        T[] items = queue.ToArray();
        T last = items[^1];
        queue.Clear();
        for (int i = 0; i < items.Length - 1; i++) {
            queue.Enqueue(items[i]);
        }
        return last;
    }

    public T Peek() {
        if (queue.Count == 0) throw new InvalidOperationException("Stack is empty");
        // To behave like a stack, return the newest (last) item
        T[] items = queue.ToArray();
        return items[^1];
    }

    public int Count => queue.Count;

    public bool IsEmpty => queue.Count == 0;

    public bool Contains(T item) => queue.Contains(item);
}
