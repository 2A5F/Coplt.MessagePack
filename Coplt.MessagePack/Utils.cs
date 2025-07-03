using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Coplt.MessagePack;

internal static class Utils
{
    [MethodImpl(256 | 512)]
    public static Guid BE(this Guid guid)
    {
        if (!BitConverter.IsLittleEndian) return guid;
        var vec = Unsafe.BitCast<Guid, Vector128<byte>>(guid);
        vec = Vector128.Shuffle(vec, Vector128.Create((byte)3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15));
        return Unsafe.BitCast<Vector128<byte>, Guid>(vec);
    }

    public static ushort BE(this ushort value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    public static uint BE(this uint value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    public static ulong BE(this ulong value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    public static short BE(this short value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    public static int BE(this int value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    public static long BE(this long value) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    public static float BE(this float value) => BitConverter.IsLittleEndian
        ? Unsafe.BitCast<uint, float>(BinaryPrimitives.ReverseEndianness(Unsafe.BitCast<float, uint>(value)))
        : value;
    public static double BE(this double value) => BitConverter.IsLittleEndian
        ? Unsafe.BitCast<ulong, double>(BinaryPrimitives.ReverseEndianness(Unsafe.BitCast<double, ulong>(value)))
        : value;

    public static decimal BE(this decimal value)
    {
        if (!BitConverter.IsLittleEndian) return value;
        var vec = Unsafe.BitCast<decimal, Vector128<byte>>(value);
        vec = Vector128.Shuffle(vec, Vector128.Create((byte)3, 2, 1, 0, 7, 6, 5, 4, 15, 14, 13, 12, 11, 10, 9, 8));
        return Unsafe.BitCast<Vector128<byte>, decimal>(vec);
    }

    [MethodImpl(256 | 512)]
    public static UInt128 BE(this UInt128 value)
    {
        if (!BitConverter.IsLittleEndian) return value;
        var vec = Unsafe.BitCast<UInt128, Vector128<byte>>(value);
        vec = Vector128.Shuffle(vec, Vector128.Create((byte)15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));
        return Unsafe.BitCast<Vector128<byte>, UInt128>(vec);
    }
    [MethodImpl(256 | 512)]
    public static Int128 BE(this Int128 value)
    {
        if (!BitConverter.IsLittleEndian) return value;
        var vec = Unsafe.BitCast<Int128, Vector128<byte>>(value);
        vec = Vector128.Shuffle(vec, Vector128.Create((byte)15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0));
        return Unsafe.BitCast<Vector128<byte>, Int128>(vec);
    }
}
