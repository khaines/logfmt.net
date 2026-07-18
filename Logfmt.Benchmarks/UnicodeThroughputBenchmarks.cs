using BenchmarkDotNet.Attributes;
using Logfmt;

namespace Logfmt.Benchmarks;

/// <summary>
/// Compares ASCII vs unicode (CJK / emoji / surrogate pairs / combining marks) value throughput and
/// allocations through the core Logger. Each value is the same UTF-16 length (128 code units) so the
/// Ratio/Allocated columns reflect encoding cost, not payload length. ASCII is the baseline (#78).
/// </summary>
[MemoryDiagnoser]
public class UnicodeThroughputBenchmarks
{
    private Logger _logger = null!;
    private string _ascii = null!;
    private string _cjk = null!;
    private string _emoji = null!;
    private string _surrogate = null!;
    private string _combining = null!;

    [GlobalSetup]
    public void Setup()
    {
        _logger = new Logger(Stream.Null, SeverityLevel.Trace);
        _ascii = new string('a', 128);
        _cjk = Repeat("\u4e2d\u6587", 64);       // 128 UTF-16 units (BMP CJK)
        _emoji = Repeat("\U0001F600", 64);        // 64 emoji = 128 UTF-16 units (surrogate pairs)
        _surrogate = Repeat("\U00020BB7", 64);    // supplementary-plane CJK = 128 UTF-16 units
        _combining = Repeat("e\u0301", 64);       // base + combining acute = 128 UTF-16 units
    }

    [Benchmark(Baseline = true)]
    public void Ascii() => Log(_ascii);

    [Benchmark]
    public void Cjk() => Log(_cjk);

    [Benchmark]
    public void Emoji() => Log(_emoji);

    [Benchmark]
    public void SurrogatePairs() => Log(_surrogate);

    [Benchmark]
    public void CombiningMarks() => Log(_combining);

    private void Log(string value)
    {
        for (int i = 0; i < 1000; i++)
        {
            _logger.Info("message", "value", value);
        }
    }

    private static string Repeat(string s, int count)
    {
        var builder = new System.Text.StringBuilder(s.Length * count);
        for (int i = 0; i < count; i++)
        {
            builder.Append(s);
        }

        return builder.ToString();
    }
}
