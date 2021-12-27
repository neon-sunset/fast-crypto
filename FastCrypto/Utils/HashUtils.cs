using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Cysharp.Collections;

namespace FastCrypto.Utils;

public enum HashAlgorithm
{
    [Obsolete("MD5 is known to the state of California to cause cancer. Please avoid if possible.")]
    MD5,
    [Obsolete("SHA1 is known to the state of California to cause cancer. Please avoid if possible.")]
    SHA1,
    SHA256,
    SHA384,
    SHA512,
}

[SkipLocalsInit]
public static class HashUtils
{
    public static string ComputeDigest(HashAlgorithm algorithm, string source, bool useLowercase = true)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        return ComputeDigest(algorithm, source.AsSpan(), useLowercase);
    }

    public static string ComputeDigest(HashAlgorithm algorithm, ReadOnlySpan<char> source, bool useLowercase = true)
    {
        const int StackAllocThreshold = 1024;
        const int ArrayPoolThreshold = 1024 * 1024 * 10;

        byte[]? toReturn = null;

        var sourceByteCount = Encoding.UTF8.GetByteCount(source);
        Span<byte> sourceBytes = stackalloc byte[StackAllocThreshold];
        if (sourceByteCount is > StackAllocThreshold and < ArrayPoolThreshold)
        {
            toReturn = ArrayPool<byte>.Shared.Rent(sourceByteCount);
            sourceBytes = toReturn;
        }
        else if (sourceByteCount > ArrayPoolThreshold)
        {
            using var nativeMemoryBuffer = new NativeMemoryArray<byte>(sourceByteCount, skipZeroClear: true);
            sourceBytes = nativeMemoryBuffer.AsSpan();
        }

        sourceBytes = sourceBytes[..sourceByteCount];

        _ = Encoding.UTF8.GetBytes(source, sourceBytes);
        var digest = ComputeDigest(algorithm, sourceBytes, useLowercase);

        if (toReturn is not null)
        {
            ArrayPool<byte>.Shared.Return(toReturn);
        }

        return digest;
    }

#pragma warning disable CS0618
    public static string ComputeDigest(HashAlgorithm algorithm, ReadOnlySpan<byte> source, bool useLowercase = true)
    {
        Span<byte> digestBytes = stackalloc byte[64];
        var digestByteCount = algorithm switch
        {
            HashAlgorithm.MD5 => MD5.HashData(source, digestBytes),
            HashAlgorithm.SHA1 => SHA1.HashData(source, digestBytes),
            HashAlgorithm.SHA256 => SHA256.HashData(source, digestBytes),
            HashAlgorithm.SHA384 => SHA384.HashData(source, digestBytes),
            HashAlgorithm.SHA512 => SHA512.HashData(source, digestBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        };

        var digestHexString = Convert.ToHexString(digestBytes[..digestByteCount]);
        return useLowercase ? digestHexString.ToLowerInvariant() : digestHexString;
    }
#pragma warning restore CS0618
}