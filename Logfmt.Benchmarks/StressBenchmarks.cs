using BenchmarkDotNet.Attributes;
using Logfmt;

namespace Logfmt.Benchmarks;

/// <summary>
/// Stress and load benchmarks: high throughput, large payloads, concurrent writers,
/// logger churn, WithData chaining under concurrency, and severity-filter overhead.
/// GC impact is reported per benchmark by the memory diagnoser (Gen0/Gen1/Gen2 + Allocated).
/// </summary>
[MemoryDiagnoser]
public class StressBenchmarks
{
    private Logger _logger = null!;
    private Logger _filteredLogger = null!;
    private string _largeValue = null!;
    private string _largeLine = null!;

    [GlobalSetup]
    public void Setup()
    {
        _logger = new Logger(Stream.Null, SeverityLevel.Trace);
        _filteredLogger = new Logger(Stream.Null, SeverityLevel.Warn);
        _largeValue = new string('x', 10_000);

        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < 50; i++)
        {
            builder.Append("key").Append(i).Append("=value").Append(i).Append(' ');
        }

        _largeLine = builder.ToString();
    }

    [Benchmark]
    public void HighThroughputBatch()
    {
        for (int i = 0; i < 1000; i++)
        {
            _logger.Info("throughput message", "writer", "bench");
        }
    }

    [Benchmark]
    public void LogLargeValue()
    {
        _logger.Info("large payload", "data", _largeValue);
    }

    [Benchmark]
    public object ParseLargeLine()
    {
        return LogfmtParser.Parse(_largeLine);
    }

    [Benchmark]
    public void ConcurrentWriters()
    {
        using var barrier = new System.Threading.Barrier(32);
        var threads = new System.Threading.Thread[32];
        for (int t = 0; t < threads.Length; t++)
        {
            threads[t] = new System.Threading.Thread(() =>
            {
                barrier.SignalAndWait();
                for (int j = 0; j < 50; j++)
                {
                    _logger.Info("concurrent message", "writer", "w");
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
    }

    [Benchmark]
    public void CreateAndDisposeLoggers()
    {
        for (int i = 0; i < 100; i++)
        {
            using var logger = new Logger(Stream.Null);
        }
    }

    [Benchmark]
    public void WithDataChainUnderConcurrency()
    {
        using var barrier = new System.Threading.Barrier(16);
        var threads = new System.Threading.Thread[16];
        for (int t = 0; t < threads.Length; t++)
        {
            threads[t] = new System.Threading.Thread(() =>
            {
                var chained = _logger.WithData("thread", "t").WithData("stage", "chain");
                barrier.SignalAndWait();
                for (int j = 0; j < 50; j++)
                {
                    chained.Info("chained message");
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
    }

    [Benchmark]
    public void LogEmitted()
    {
        for (int i = 0; i < 1000; i++)
        {
            _logger.Info("emitted message");
        }
    }

    [Benchmark]
    public void LogFilteredOut()
    {
        // 1000 iterations so the filtered path measures real, non-elidable work: each Info is a
        // cross-assembly call that hits the severity gate and returns. The A/B vs LogEmitted (same
        // count) shows the filter's saved formatting cost and its zero allocation.
        for (int i = 0; i < 1000; i++)
        {
            _filteredLogger.Info("filtered message");
        }
    }
}
