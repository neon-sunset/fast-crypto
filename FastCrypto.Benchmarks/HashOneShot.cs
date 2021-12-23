using BuiltInSHA256 = System.Security.Cryptography.SHA256;

namespace FastCrypto.Benchmarks;

[MemoryDiagnoser]
public class HashOneShot
{
    [ParamsSource(nameof(Params))]
    public byte[] Input;

    public IEnumerable<byte[]> Params => new[]
    {
        Encoding.UTF8.GetBytes(Constants.String32Char),
        Encoding.UTF8.GetBytes(Constants.String128Char),
        Encoding.UTF8.GetBytes(Constants.String1024Char),
        Encoding.UTF8.GetBytes(Constants.MultiByteChars)
    };

    [Benchmark(Baseline = true)]
    public byte[] SHA256CoreLib()
    {
        var destination = new byte[32];
        _ = BuiltInSHA256.HashData(Input, destination);

        return destination;
    }

    [Benchmark]
    public byte[] SHA256New()
    {
        var destination = new byte[32];
        _ = FastCrypto.SHA256.HashData(Input, destination);

        return destination;
    }
}