using BenchmarkDotNet.Running;

using FastCrypto.Benchmarks.Utils;

new HashUtilsThroughputBenchmark().Execute();

// BenchmarkRunner.Run<FastCrypto.Benchmarks.SHA256.OneShot>();