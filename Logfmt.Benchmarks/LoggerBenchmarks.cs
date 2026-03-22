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
        _simpleLogLine = "ts=2026-03-22T12:00:00.0000000Z level=info msg=\"test message\"";
        _complexLogLine = "ts=2026-03-22T12:00:00.0000000Z level=info msg=\"User logged in\" user_id=123 service=api env=production request_id=abc-def-123 duration_ms=42";
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

    [Benchmark]
    public void LogTypedFields()
    {
        _logger.Log(SeverityLevel.Info, "test message", "status", 200, "duration_ms", 42);
    }

    [Benchmark]
    public void LogTypedFiltered()
    {
        _logger.Log(SeverityLevel.Debug, "filtered", new object[] { "count", 42 });
    }

    // Parser benchmarks
    private string _simpleLogLine = null!;
    private string _complexLogLine = null!;

    [Benchmark]
    public object ParseSimple()
    {
        return LogfmtParser.Parse(_simpleLogLine);
    }

    [Benchmark]
    public object ParseComplex()
    {
        return LogfmtParser.Parse(_complexLogLine);
    }
}
