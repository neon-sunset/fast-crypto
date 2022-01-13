using System.Runtime.Intrinsics;

namespace FastCrypto.Algorithms;

public static class BLAKE3
{
    public static int HashData(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        throw new NotImplementedException();
    }

    private static void Round(Span<uint> v_payload64, Span<Vector128<uint>> msg_payload64, nuint r)
    {
        throw new NotImplementedException();
    }

    private static void TransposeVectors(
        Vector128<uint> a,
        Vector128<uint> b,
        Vector128<uint> c,
        Vector128<uint> d)
    {
        throw new NotImplementedException();
    }

    private static void TransposeMessageVectors(
        ReadOnlySpan<byte> source,
        nuint offset,
        Span<uint> dst_payload64)
    {
        throw new NotImplementedException();
    }
}