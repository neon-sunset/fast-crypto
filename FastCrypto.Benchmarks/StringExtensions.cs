using FastCrypto.Extensions;

namespace FastCrypto.Benchmarks;

public class StringExtensions
{
    [Params(
        Constants.String32Char,
        Constants.String128Char,
        Constants.String1024Char)]
    public string Input = string.Empty;

    [Benchmark(Baseline = true)]
    public string ToLowerInvariant() => Input.ToLowerInvariant();

    [Benchmark]
    public string ToLowerAlphanumeric() => Input.ToLowerAlphanumeric();
}