using BenchmarkDotNet.Attributes;
using Logfmt;
using Logfmt.ExtensionLogging;
using Microsoft.Extensions.Logging;

namespace Logfmt.Benchmarks;

/// <summary>
/// Measures ExtensionLogger throughput and allocations as the state property count scales
/// (10 -> 1,000 -> 10,000), documenting the degradation curve (#77). The library emits every
/// property (no count cap); the Mean and Allocated columns show the (linear) cost per property.
/// </summary>
[MemoryDiagnoser]
public class StatePropertyCountBenchmarks
{
    [Params(10, 1000, 10000)]
    public int PropertyCount { get; set; }

    private ILogger _logger = null!;
    private Dictionary<string, object> _state = null!;

    [GlobalSetup]
    public void Setup()
    {
        _logger = new ExtensionLogger(new Logger(Stream.Null, SeverityLevel.Trace), GetConfig, "bench");
        _state = new Dictionary<string, object>(PropertyCount);
        for (int i = 0; i < PropertyCount; i++)
        {
            _state[$"key{i}"] = $"val{i}";
        }
    }

    [Benchmark]
    public void LogState()
    {
        _logger.Log(LogLevel.Information, new EventId(0), _state, null, null!);
    }

    private static ExtensionLoggerConfiguration GetConfig()
    {
        var config = new ExtensionLoggerConfiguration();
        config.LogLevel["Default"] = LogLevel.Trace;
        return config;
    }
}
