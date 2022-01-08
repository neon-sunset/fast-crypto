using FastCrypto.Extensions;

namespace FastCrypto.Benchmarks;

[MemoryDiagnoser]
public class HashUtilsOneShot
{
    // private static readonly string Big = File.ReadAllText("big.pdf");

    [ParamsSource(nameof(Params))]
    public string Input = string.Empty;

    public IEnumerable<string> Params => new[]
    {
        Constants.String32Char,
        Constants.String128Char,
        Constants.String1024Char,
        Constants.MultiByteChars,
        File.ReadAllText("Input.txt"),
        // null
    };

    [Benchmark(Baseline = true)]
    public string ComputeDigest() => Digest
        .ComputeHex(HashAlgorithm.SHA256, Input/* ?? Big*/, useLowercase: false)
        .ToLowerInvariant();

    [Benchmark]
    public string ComputeDigestToLowerInPlace() => Digest
        .ComputeHex(HashAlgorithm.SHA256, Input/* ?? Big*/, useLowercase: false)
        .ToLowerAlphanumeric();
}