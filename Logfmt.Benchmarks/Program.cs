using BenchmarkDotNet.Running;

var switcher = BenchmarkSwitcher.FromAssembly(typeof(Logfmt.Benchmarks.LoggerBenchmarks).Assembly);
if (args.Length == 0)
{
    switcher.RunAll();
}
else
{
    switcher.Run(args);
}
