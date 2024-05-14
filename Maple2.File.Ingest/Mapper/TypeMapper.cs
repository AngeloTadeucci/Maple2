using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Force.Crc32;

namespace Maple2.File.Ingest.Mapper;

public abstract class TypeMapper<T> where T : class {
    private readonly Stopwatch stopwatch;
    private readonly List<T> results;

    public bool Complete { get; private set; }

    public IReadOnlyCollection<T> Results => Complete ? results : Array.Empty<T>();

    public long ElapsedMilliseconds => stopwatch.ElapsedMilliseconds;

    protected TypeMapper() {
        stopwatch = new Stopwatch();
        results = [];
    }

    public uint Process() {
        if (Complete) {
            throw new InvalidOperationException($"{typeof(T)} has already been mapped.");
        }

        uint crc32C = 0;
        stopwatch.Start();
        foreach (T result in Map()) {
            crc32C = Crc32CAlgorithm.Append(crc32C, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)));
            results.Add(result);
        }
        stopwatch.Stop();
        Complete = true;

        return crc32C;
    }

    protected abstract IEnumerable<T> Map();
}
