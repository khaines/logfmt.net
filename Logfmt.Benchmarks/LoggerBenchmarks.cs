using BenchmarkDotNet.Attributes;
using Logfmt;

namespace Logfmt.Benchmarks;

[MemoryDiagnoser]
public class LoggerBenchmarks
{
    private Logger _logger = null!;
    private Logger _loggerWithData = null!;
    private Stream _nullStream = null!;

    [GlobalSetup]
    public void Setup()
    {
        _nullStream = Stream.Null;
        _logger = new Logger(_nullStream);
        _loggerWithData = new Logger(_nullStream).WithData("service", "benchmark", "env", "test");
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

    [Benchmark]
    public void LogFiltered()
    {
        _logger.Debug("this should be filtered out");
    }

    [Benchmark]
    public void LogWithDefaultData()
    {
        _loggerWithData.Info("test message");
    }

    [Benchmark]
    public void LogManyFields()
    {
        _logger.Log(SeverityLevel.Info, "test message",
            "field1", "value1", "field2", "value2", "field3", "value3",
            "field4", "value4", "field5", "value5", "field6", "value6",
            "field7", "value7", "field8", "value8");
    }
}
