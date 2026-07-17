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
        System.Threading.Tasks.Parallel.For(0, 32, i =>
        {
            for (int j = 0; j < 10; j++)
            {
                _logger.Info("concurrent message", "writer", "w");
            }
        });
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
        System.Threading.Tasks.Parallel.For(0, 16, i =>
        {
            var chained = _logger.WithData("thread", "t").WithData("stage", "chain");
            chained.Info("chained message");
        });
    }

    [Benchmark]
    public void LogEmitted()
    {
        _logger.Info("emitted message");
    }

    [Benchmark]
    public void LogFilteredOut()
    {
        _filteredLogger.Info("filtered message");
    }
}
