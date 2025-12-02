using BenchmarkDotNet.Running;
using Logfmt.Benchmarks;

var summary = BenchmarkRunner.Run<LoggerBenchmarks>();
