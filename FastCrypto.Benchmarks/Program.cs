using BenchmarkDotNet.Running;
using FastCrypto.Benchmarks.Utils;

// new HashUtilsThroughputBenchmark().Execute();

BenchmarkRunner.Run<HashUtilsBenchmark>();