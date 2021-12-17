namespace FastCrypto.Benchmarks.Utils;

[MemoryDiagnoser]
public class HashUtilsBenchmark
{
    [Params(
        Constants.String32Char,
        Constants.String128Char,
        Constants.String1024Char,
        Constants.MultiByteChars)]
    public string Input = string.Empty;

    [Benchmark(Baseline = true)]
    public string Sha256() => HashUtils.ComputeDigest(HashAlgorithm.SHA256, Input, false);

    [Benchmark]
    public string Sha256Intrinsics() => HashUtils.ComputeDigest(HashAlgorithm.SHA256Arm64, Input, false);
    
    /*
    [Benchmark]
    public string Sha384() => HashUtils.ComputeDigest(HashAlgorithm.SHA384, Input);

    [Benchmark]
    public string Sha512() => HashUtils.ComputeDigest(HashAlgorithm.SHA512, Input);

#pragma warning disable CS0618
    [Benchmark]
    public string Md5() => HashUtils.ComputeDigest(HashAlgorithm.MD5, Input);

    [Benchmark]
    public string Sha1() => HashUtils.ComputeDigest(HashAlgorithm.SHA1, Input);
#pragma warning restore CS0618
    */
}
