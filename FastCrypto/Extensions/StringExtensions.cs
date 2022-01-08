using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;

[assembly: InternalsVisibleToAttribute("FastCrypto.Benchmarks")]
namespace FastCrypto.Extensions;

internal static class StringExtensions
{
    public static string ToLowerAlphanumeric(this string input)
    {
        if (!ArmBase.Arm64.IsSupported)
        {
            return input.ToLowerInvariant();
        }

        var output = (stackalloc char[input.Length]);
        var mask = new Vector<ushort>(32);
        var vectorSize = Vector<ushort>.Count;

        var inBytes = MemoryMarshal.Cast<char, ushort>(input);
        var outBytes = MemoryMarshal.Cast<char, ushort>(output);

        var unrollFactor = vectorSize * 4;
        Debug.Assert(outBytes.Length % unrollFactor == 0);

        for (var i = 0; i < outBytes.Length; i += unrollFactor)
        {
            var start1 = i;
            var start2 = i + vectorSize;
            var start3 = i + (vectorSize * 2);
            var start4 = i + (vectorSize * 3);
            var end4 = start4 + vectorSize;

            var chars1 = inBytes[start1..start2];
            var chars2 = inBytes[start2..start3];
            var chars3 = inBytes[start3..start4];
            var chars4 = inBytes[start4..end4];

            var vec1 = Unsafe.ReadUnaligned<Vector<ushort>>(
                ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(chars1))) | mask;

            var vec2 = Unsafe.ReadUnaligned<Vector<ushort>>(
                ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(chars2))) | mask;

            var vec3 = Unsafe.ReadUnaligned<Vector<ushort>>(
                ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(chars3))) | mask;

            var vec4 = Unsafe.ReadUnaligned<Vector<ushort>>(
                ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(chars4))) | mask;

            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref outBytes[start1]), vec1);
            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref outBytes[start2]), vec2);
            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref outBytes[start3]), vec3);
            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref outBytes[start4]), vec4);
        }

        return output.ToString();
    }
}