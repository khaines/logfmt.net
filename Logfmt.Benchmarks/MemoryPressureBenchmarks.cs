using BenchmarkDotNet.Attributes;
using Logfmt;

namespace Logfmt.Benchmarks;

/// <summary>
/// Exercises logging while ~256MB of live heap is retained, covering the "memory pressure scenarios
/// with 256MB+ heap usage" criterion (#60). Every OS page of the retained heap is faulted in during
/// setup so the ~256MB is genuinely resident (committed), not merely reserved; this forces the GC to
/// track a large live set. The MemoryDiagnoser columns show whether per-call logging cost or
/// allocations degrade under that pressure (they should not -- the logger's per-call allocation is
/// independent of heap size).
/// </summary>
[MemoryDiagnoser]
public class MemoryPressureBenchmarks
{
    private const int BlockCount = 256;
    private const int BlockSize = 1024 * 1024; // 1 MB per block -> ~256 MB retained
    private const int PageSize = 4096;          // touch every OS page so the heap is resident

    private Logger _logger = null!;
    private byte[][] _retainedHeap = null!;

    [GlobalSetup]
    public void Setup()
    {
        _logger = new Logger(Stream.Null, SeverityLevel.Trace);

        _retainedHeap = new byte[BlockCount][];
        for (int i = 0; i < BlockCount; i++)
        {
            _retainedHeap[i] = new byte[BlockSize];

            // Write to every OS page so the whole block is committed and resident, not just reserved
            // address space (a demand-paged host would otherwise leave most of it unfaulted).
            for (int offset = 0; offset < BlockSize; offset += PageSize)
            {
                _retainedHeap[i][offset] = (byte)i;
            }
        }
    }

    [Benchmark]
    public void LogUnderMemoryPressure()
    {
        for (int i = 0; i < 1000; i++)
        {
            _logger.Info("under memory pressure", "iter", "x");
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _retainedHeap = null!;
    }
}
