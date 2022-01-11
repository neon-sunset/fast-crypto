using Cysharp.Collections;

namespace FastCrypto.Extensions;

internal static class ArrayExtensions
{
    public static byte[] RentPooled(int minimumLength) => ArrayPool<byte>.Shared.Rent(minimumLength);

    public static void Return(this byte[] pooledArray) => ArrayPool<byte>.Shared.Return(pooledArray);

    public static NativeMemoryArray<byte> AllocNative(int length) => new(length, skipZeroClear: true, addMemoryPressure: true);

    public static void Free(this NativeMemoryArray<byte> nativeArray) => nativeArray.Dispose();
}