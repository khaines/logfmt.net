using BenchmarkDotNet.Attributes;
using Logfmt;

namespace Logfmt.Benchmarks;

[MemoryDiagnoser]
public class LoggerBenchmarks
{
    private Logger _logger;
    private Stream _nullStream;

    [GlobalSetup]
    public void Setup()
    {
        _nullStream = Stream.Null;
        _logger = new Logger(_nullStream);
    }

    [Benchmark]
    public void LogSimple()
    {
        _logger.Log(SeverityLevel.Info, "test message");
    }

    [Benchmark]
    public void LogWithFields()
    {
        _logger.Log(SeverityLevel.Info, "test message", "foo", "bar", "baz", "qux");
    }

    [Benchmark]
    public void LogWithSpecialChars()
    {
        _logger.Log(SeverityLevel.Info, "test message", "json", "{\"foo\":\"bar\"}");
    }
}
