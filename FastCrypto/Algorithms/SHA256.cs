using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

using BuiltInSHA256 = System.Security.Cryptography.SHA256;

namespace FastCrypto.Algorithms;

public static class SHA256
{
    private const int DigestByteCount = 32;
    private const int SupportedByteCountThreshold = 4096;

    private static readonly Vector128<uint> K0 = Vector128.Create(0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5);
    private static readonly Vector128<uint> K1 = Vector128.Create(0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5);
    private static readonly Vector128<uint> K2 = Vector128.Create(0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3);
    private static readonly Vector128<uint> K3 = Vector128.Create(0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174);
    private static readonly Vector128<uint> K4 = Vector128.Create(0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc);
    private static readonly Vector128<uint> K5 = Vector128.Create((uint)0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da);
    private static readonly Vector128<uint> K6 = Vector128.Create(0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7);
    private static readonly Vector128<uint> K7 = Vector128.Create(0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967);
    private static readonly Vector128<uint> K8 = Vector128.Create((uint)0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13);
    private static readonly Vector128<uint> K9 = Vector128.Create(0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85);
    private static readonly Vector128<uint> K10 = Vector128.Create(0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3);
    private static readonly Vector128<uint> K11 = Vector128.Create(0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070);
    private static readonly Vector128<uint> K12 = Vector128.Create((uint)0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5);
    private static readonly Vector128<uint> K13 = Vector128.Create((uint)0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3);
    private static readonly Vector128<uint> K14 = Vector128.Create(0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208);
    private static readonly Vector128<uint> K15 = Vector128.Create(0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int HashData(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        if (!Sha256.IsSupported || !AdvSimd.IsSupported)
        {
            return BuiltInSHA256.HashData(source, destination);
        }

        if (source.Length > SupportedByteCountThreshold) // FIXME: Improve performance on large payloads and fix incorrect output
        {
            return BuiltInSHA256.HashData(source, destination);
        }

        if (destination.Length < DigestByteCount)
        {
            throw new ArgumentException("Destination buffer is smaller than digest size of 32 bytes", nameof(destination));
        }

        Span<byte> padding = stackalloc byte[64]
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        padding[0] = 0x80;
        BinaryPrimitives.WriteUInt64BigEndian(padding[56..], (ulong)source.Length * 8); // FIXME: Invalid behavior when payload size is too big

        Span<uint> state = stackalloc uint[8]
        {
            0x6a09e667,
            0xbb67ae85,
            0x3c6ef372,
            0xa54ff53a,
            0x510e527f,
            0x9b05688c,
            0x1f83d9ab,
            0x5be0cd19,
        };

        Block(state, source);
        Block(state, padding);

        for (var i = 0; i < state.Length; i++)
        {
            state[i] = BinaryPrimitives.ReverseEndianness(state[i]);
        }

        MemoryMarshal.Cast<uint, byte>(state).CopyTo(destination);

        return DigestByteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void Block(Span<uint> state, ReadOnlySpan<byte> data)
    {
        var msg = (stackalloc byte[64]);
        var uint_data = MemoryMarshal.Cast<byte, uint>(data);
        var uint_msg = MemoryMarshal.Cast<byte, uint>(msg);

        var hash_abcd = Unsafe.ReadUnaligned<Vector128<uint>>(ref Unsafe.As<uint, byte>(ref state[0]));
        var hash_efgh = Unsafe.ReadUnaligned<Vector128<uint>>(ref Unsafe.As<uint, byte>(ref state[4]));

        while (uint_data.Length >= 16)
        {
            var save_abcd = hash_abcd;
            var save_efgh = hash_efgh;

            for (int i = 0; i < 16; i += 4)
            {
                uint_msg[i] = BinaryPrimitives.ReverseEndianness(uint_data[i]);
                uint_msg[i + 1] = BinaryPrimitives.ReverseEndianness(uint_data[i + 1]);
                uint_msg[i + 2] = BinaryPrimitives.ReverseEndianness(uint_data[i + 2]);
                uint_msg[i + 3] = BinaryPrimitives.ReverseEndianness(uint_data[i + 3]);
            }

            var msg0 = Unsafe.ReadUnaligned<Vector128<uint>>(ref msg[0]);
            var msg1 = Unsafe.ReadUnaligned<Vector128<uint>>(ref msg[16]);
            var msg2 = Unsafe.ReadUnaligned<Vector128<uint>>(ref msg[32]);
            var msg3 = Unsafe.ReadUnaligned<Vector128<uint>>(ref msg[48]);

            // Rounds 0-3
            Vector128<uint> wk = AdvSimd.Add(msg0, K0);
            msg0 = Sha256.ScheduleUpdate0(msg0, msg1);
            msg0 = Sha256.ScheduleUpdate1(msg0, msg2, msg3);
            Vector128<uint> temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 4-7
            wk = AdvSimd.Add(msg1, K1);
            msg1 = Sha256.ScheduleUpdate0(msg1, msg2);
            msg1 = Sha256.ScheduleUpdate1(msg1, msg3, msg0);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 8-11
            wk = AdvSimd.Add(msg2, K2);
            msg2 = Sha256.ScheduleUpdate0(msg2, msg3);
            msg2 = Sha256.ScheduleUpdate1(msg2, msg0, msg1);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 12-15
            wk = AdvSimd.Add(msg3, K3);
            msg3 = Sha256.ScheduleUpdate0(msg3, msg0);
            msg3 = Sha256.ScheduleUpdate1(msg3, msg1, msg2);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 16-19
            wk = AdvSimd.Add(msg0, K4);
            msg0 = Sha256.ScheduleUpdate0(msg0, msg1);
            msg0 = Sha256.ScheduleUpdate1(msg0, msg2, msg3);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 20-23
            wk = AdvSimd.Add(msg1, K5);
            msg1 = Sha256.ScheduleUpdate0(msg1, msg2);
            msg1 = Sha256.ScheduleUpdate1(msg1, msg3, msg0);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 24-27
            wk = AdvSimd.Add(msg2, K6);
            msg2 = Sha256.ScheduleUpdate0(msg2, msg3);
            msg2 = Sha256.ScheduleUpdate1(msg2, msg0, msg1);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 28-31
            wk = AdvSimd.Add(msg3, K7);
            msg3 = Sha256.ScheduleUpdate0(msg3, msg0);
            msg3 = Sha256.ScheduleUpdate1(msg3, msg1, msg2);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 32-35
            wk = AdvSimd.Add(msg0, K8);
            msg0 = Sha256.ScheduleUpdate0(msg0, msg1);
            msg0 = Sha256.ScheduleUpdate1(msg0, msg2, msg3);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 36-39
            wk = AdvSimd.Add(msg1, K9);
            msg1 = Sha256.ScheduleUpdate0(msg1, msg2);
            msg1 = Sha256.ScheduleUpdate1(msg1, msg3, msg0);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 40-43
            wk = AdvSimd.Add(msg2, K10);
            msg2 = Sha256.ScheduleUpdate0(msg2, msg3);
            msg2 = Sha256.ScheduleUpdate1(msg2, msg0, msg1);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 44-47
            wk = AdvSimd.Add(msg3, K11);
            msg3 = Sha256.ScheduleUpdate0(msg3, msg0);
            msg3 = Sha256.ScheduleUpdate1(msg3, msg1, msg2);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 48-51
            wk = AdvSimd.Add(msg0, K12);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 52-55
            wk = AdvSimd.Add(msg1, K13);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 56-59
            wk = AdvSimd.Add(msg2, K14);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Rounds 60-63
            wk = AdvSimd.Add(msg3, K15);
            temp_abcd = hash_abcd;
            hash_abcd = Sha256.HashUpdate1(hash_abcd, hash_efgh, wk);
            hash_efgh = Sha256.HashUpdate2(hash_efgh, temp_abcd, wk);

            // Combine state
            hash_abcd = AdvSimd.Add(hash_abcd, save_abcd);
            hash_efgh = AdvSimd.Add(hash_efgh, save_efgh);

            uint_data = uint_data[16..];
        }

        Unsafe.WriteUnaligned(ref Unsafe.As<uint, byte>(ref state[0]), hash_abcd);
        Unsafe.WriteUnaligned(ref Unsafe.As<uint, byte>(ref state[4]), hash_efgh);
    }
}