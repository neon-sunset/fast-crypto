using BuiltInSHA256 = System.Security.Cryptography.SHA256;

namespace FastCrypto.Benchmarks;

[MemoryDiagnoser]
public class HashOneShot
{
    [ParamsSource(nameof(Params))]
    public byte[] Input = Array.Empty<byte>();

    public IEnumerable<byte[]> Params => new[]
    {
        Encoding.UTF8.GetBytes(Constants.String32Char),
        Encoding.UTF8.GetBytes(Constants.String128Char),
        Encoding.UTF8.GetBytes(Constants.String1024Char),
        Encoding.UTF8.GetBytes(Constants.MultiByteChars),
        Encoding.UTF8.GetBytes(File.ReadAllText("Input.txt"))
    };

    [Benchmark(Baseline = true)]
    public byte SHA256CoreLib()
    {
        Span<byte> destination = stackalloc byte[32];
        _ = BuiltInSHA256.HashData(Input, destination);

        return destination[^1];
    }

    [Benchmark]
    public byte SHA256New()
    {
        Span<byte> destination = stackalloc byte[32];
        _ = SHA256.HashData(Input, destination);

        return destination[^1];
    }
}