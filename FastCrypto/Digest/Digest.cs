using System.Runtime.CompilerServices;
using System.Security.Cryptography;

using Cysharp.Collections;

using FastCrypto.Extensions;

using SHA256 = FastCrypto.Algorithms.SHA256;

namespace FastCrypto;

[SkipLocalsInit]
public static partial class Digest
{
    public static string ComputeHex(HashAlgorithm algorithm, string source, bool useLowercase = true)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ComputeHex(algorithm, (ReadOnlySpan<char>)source, useLowercase);
    }

    public static string ComputeHex(HashAlgorithm algorithm, ReadOnlySpan<char> source, bool useLowercase = true)
    {
        var digestBytes = (stackalloc byte[64]);
        var digestByteCount = ComputeBytes(algorithm, source, digestBytes);

        return useLowercase
            ? Convert.ToHexString(digestBytes[..digestByteCount]).ToLowerAlphanumeric()
            : Convert.ToHexString(digestBytes[..digestByteCount]);
    }

    public static string ComputeHex(HashAlgorithm algorithm, ReadOnlySpan<byte> source, bool useLowercase = true)
    {
        var digestBytes = (stackalloc byte[64]);
        var digestByteCount = ComputeBytes(algorithm, source, digestBytes);

        return useLowercase
            ? Convert.ToHexString(digestBytes[..digestByteCount]).ToLowerAlphanumeric()
            : Convert.ToHexString(digestBytes[..digestByteCount]);
    }

    public static byte[] ComputeBytes(HashAlgorithm algorithm, string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ComputeBytes(algorithm, (ReadOnlySpan<char>)source);
    }

    public static byte[] ComputeBytes(HashAlgorithm algorithm, ReadOnlySpan<char> source)
    {
        var digestBytes = (stackalloc byte[64]);
        var digestByteCount = ComputeBytes(algorithm, source, digestBytes);

        return digestBytes[..digestByteCount].ToArray();
    }

    public static byte[] ComputeBytes(HashAlgorithm algorithm, ReadOnlySpan<byte> source)
    {
        var digestBytes = (stackalloc byte[64]);
        var digestByteCount = ComputeBytes(algorithm, source, digestBytes);

        return digestBytes[..digestByteCount].ToArray();
    }

    public static int ComputeBytes(HashAlgorithm algorithm, string source, Span<byte> destination)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ComputeBytes(algorithm, (ReadOnlySpan<char>)source, destination);
    }

    public static int ComputeBytes(HashAlgorithm algorithm, ReadOnlySpan<char> source, Span<byte> destination)
    {
        byte[]? pooledArray = null;
        NativeMemoryArray<byte>? nativeMemoryArray = null;

        var sourceByteCount = Encoding.UTF8.GetByteCount(source);
        var sourceBytes = sourceByteCount switch
        {
            <= Constants.StackAllocLimitBytes => stackalloc byte[Constants.StackAllocLimitBytes],
            <= Constants.ArrayPoolLimitBytes => pooledArray = ArrayExtensions.RentPooled(sourceByteCount),
            _ => (nativeMemoryArray = ArrayExtensions.AllocNative(sourceByteCount)).AsSpan()
        };

        sourceBytes = sourceBytes[..sourceByteCount];

        _ = Encoding.UTF8.GetBytes(source, sourceBytes);
        var digestByteCount = ComputeBytes(algorithm, sourceBytes, destination);

        pooledArray?.Return();
        nativeMemoryArray?.Free();

        return digestByteCount;
    }

#pragma warning disable CS0618
    public static int ComputeBytes(HashAlgorithm algorithm, ReadOnlySpan<byte> source, Span<byte> destination)
    {
        return algorithm switch
        {
            HashAlgorithm.MD5 => MD5.HashData(source, destination),
            HashAlgorithm.SHA1 => SHA1.HashData(source, destination),
            HashAlgorithm.SHA256 => SHA256.HashData(source, destination),
            HashAlgorithm.SHA384 => SHA384.HashData(source, destination),
            HashAlgorithm.SHA512 => SHA512.HashData(source, destination),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        };
    }
#pragma warning restore CS0618
}