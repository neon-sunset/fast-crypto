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

        var inChars = MemoryMarshal.Cast<char, ushort>(input);
        var outChars = MemoryMarshal.Cast<char, ushort>(output);

        var unrollFactor = vectorSize * 4;
        Debug.Assert(outChars.Length % unrollFactor == 0);

        for (var i = 0; i < outChars.Length; i += unrollFactor)
        {
            var start1 = i;
            var start2 = i + vectorSize;
            var start3 = i + (vectorSize * 2);
            var start4 = i + (vectorSize * 3);

            var vec1 = Unsafe.ReadUnaligned<Vector<ushort>>(
                ref Unsafe.As<ushort, byte>(ref Unsafe.AsRef(inChars[start1]))) | mask;

            var vec2 = Unsafe.ReadUnaligned<Vector<ushort>>(
                ref Unsafe.As<ushort, byte>(ref Unsafe.AsRef(inChars[start2]))) | mask;

            var vec3 = Unsafe.ReadUnaligned<Vector<ushort>>(
                ref Unsafe.As<ushort, byte>(ref Unsafe.AsRef(inChars[start3]))) | mask;

            var vec4 = Unsafe.ReadUnaligned<Vector<ushort>>(
                ref Unsafe.As<ushort, byte>(ref Unsafe.AsRef(inChars[start4]))) | mask;

            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref outChars[start1]), vec1);
            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref outChars[start2]), vec2);
            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref outChars[start3]), vec3);
            Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref outChars[start4]), vec4);
        }

        return output.ToString();
    }
}