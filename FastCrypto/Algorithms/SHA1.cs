using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

using BuiltInSHA1 = System.Security.Cryptography.SHA1;

namespace FastCrypto.Algorithms;

public static class SHA1
{
    private const int DigestByteCount = 20;
    private const int SupportedByteCountThreshold = 4096 * 1024;

    public static int HashData(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        if (!Sha1.IsSupported || !AdvSimd.IsSupported)
        {
            return BuiltInSHA1.HashData(source, destination);
        }

        if (source.Length > SupportedByteCountThreshold)
        {
            return BuiltInSHA1.HashData(source, destination);
        }

        var state = (stackalloc uint[]
        {
            0x67452301,
            0xefcdab89,
            0x98Badcfe,
            0x10325476,
            0xc3d2e1f0
        });

        Block(state, source);

        return DigestByteCount;
    }

    private static void Block(Span<uint> state, ReadOnlySpan<byte> data)
    {
        Vector128<uint> abcd, abcd_saved;
        Vector128<uint> tmp0, tmp1;
        Vector128<uint> msg0, msg1, msg2, msg3;
        uint e0, e0_saved, e1;

        abcd = Unsafe.ReadUnaligned<Vector128<uint>>(ref Unsafe.As<uint, byte>(ref state[0]));
        e0 = state[4];

        while (data.Length >= 64)
        {
            data = data[64..];
        }

        throw new NotImplementedException();
    }
}