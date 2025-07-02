using Coplt.Union;

namespace Coplt.MessagePack;

[Union]
public readonly partial struct MessagePackInteger
{
    #region Define

    [UnionTemplate]
    private interface Template
    {
        void None();
        byte Byte();
        ushort UInt16();
        uint UInt32();
        ulong UInt64();
        sbyte SByte();
        short Int16();
        int Int32();
        long Int64();
    }

    public static readonly MessagePackInteger None = MakeNone();

    #endregion

    #region Convert

    public static implicit operator MessagePackInteger(byte value) => MakeByte(value);

    public static implicit operator MessagePackInteger(ushort value) => MakeUInt16(value);

    public static implicit operator MessagePackInteger(uint value) => MakeUInt32(value);

    public static implicit operator MessagePackInteger(ulong value) => MakeUInt64(value);

    public static implicit operator MessagePackInteger(sbyte value) => MakeSByte(value);

    public static implicit operator MessagePackInteger(short value) => MakeInt16(value);

    public static implicit operator MessagePackInteger(int value) => MakeInt32(value);

    public static implicit operator MessagePackInteger(long value) => MakeInt64(value);

    #endregion

    #region Get

    public byte ToByte() => Tag switch
    {
        Tags.Byte => Byte,
        Tags.UInt16 => (byte)UInt16,
        Tags.UInt32 => (byte)UInt32,
        Tags.UInt64 => (byte)UInt64,
        Tags.SByte => (byte)SByte,
        Tags.Int16 => (byte)Int16,
        Tags.Int32 => (byte)Int32,
        Tags.Int64 => (byte)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public ushort ToUInt16() => Tag switch
    {
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => (ushort)UInt32,
        Tags.UInt64 => (ushort)UInt64,
        Tags.SByte => (ushort)SByte,
        Tags.Int16 => (ushort)Int16,
        Tags.Int32 => (ushort)Int32,
        Tags.Int64 => (ushort)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public uint ToUInt32() => Tag switch
    {
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => UInt32,
        Tags.UInt64 => (uint)UInt64,
        Tags.SByte => (uint)SByte,
        Tags.Int16 => (uint)Int16,
        Tags.Int32 => (uint)Int32,
        Tags.Int64 => (uint)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public ulong ToUInt64() => Tag switch
    {
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => UInt32,
        Tags.UInt64 => UInt64,
        Tags.SByte => (ulong)SByte,
        Tags.Int16 => (ulong)Int16,
        Tags.Int32 => (ulong)Int32,
        Tags.Int64 => (ulong)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public sbyte ToSByte() => Tag switch
    {
        Tags.Byte => (sbyte)Byte,
        Tags.UInt16 => (sbyte)UInt16,
        Tags.UInt32 => (sbyte)UInt32,
        Tags.UInt64 => (sbyte)UInt64,
        Tags.SByte => SByte,
        Tags.Int16 => (sbyte)Int16,
        Tags.Int32 => (sbyte)Int32,
        Tags.Int64 => (sbyte)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public short ToInt16() => Tag switch
    {
        Tags.Byte => Byte,
        Tags.UInt16 => (short)UInt16,
        Tags.UInt32 => (short)UInt32,
        Tags.UInt64 => (short)UInt64,
        Tags.SByte => SByte,
        Tags.Int16 => Int16,
        Tags.Int32 => (short)Int32,
        Tags.Int64 => (short)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public int ToInt32() => Tag switch
    {
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => (int)UInt32,
        Tags.UInt64 => (int)UInt64,
        Tags.SByte => SByte,
        Tags.Int16 => Int16,
        Tags.Int32 => Int32,
        Tags.Int64 => (int)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public long ToInt64() => Tag switch
    {
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => UInt32,
        Tags.UInt64 => (long)UInt64,
        Tags.SByte => SByte,
        Tags.Int16 => Int16,
        Tags.Int32 => Int32,
        Tags.Int64 => Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    #endregion

    #region TryGet

    public byte? TryToByte() => Tag switch
    {
        Tags.None => null,
        Tags.Byte => Byte,
        Tags.UInt16 => (byte)UInt16,
        Tags.UInt32 => (byte)UInt32,
        Tags.UInt64 => (byte)UInt64,
        Tags.SByte => (byte)SByte,
        Tags.Int16 => (byte)Int16,
        Tags.Int32 => (byte)Int32,
        Tags.Int64 => (byte)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public ushort? TryToUInt16() => Tag switch
    {
        Tags.None => null,
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => (ushort)UInt32,
        Tags.UInt64 => (ushort)UInt64,
        Tags.SByte => (ushort)SByte,
        Tags.Int16 => (ushort)Int16,
        Tags.Int32 => (ushort)Int32,
        Tags.Int64 => (ushort)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public uint? TryToUInt32() => Tag switch
    {
        Tags.None => null,
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => UInt32,
        Tags.UInt64 => (uint)UInt64,
        Tags.SByte => (uint)SByte,
        Tags.Int16 => (uint)Int16,
        Tags.Int32 => (uint)Int32,
        Tags.Int64 => (uint)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public ulong? TryToUInt64() => Tag switch
    {
        Tags.None => null,
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => UInt32,
        Tags.UInt64 => UInt64,
        Tags.SByte => (ulong)SByte,
        Tags.Int16 => (ulong)Int16,
        Tags.Int32 => (ulong)Int32,
        Tags.Int64 => (ulong)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public sbyte? TryToSByte() => Tag switch
    {
        Tags.None => null,
        Tags.Byte => (sbyte)Byte,
        Tags.UInt16 => (sbyte)UInt16,
        Tags.UInt32 => (sbyte)UInt32,
        Tags.UInt64 => (sbyte)UInt64,
        Tags.SByte => SByte,
        Tags.Int16 => (sbyte)Int16,
        Tags.Int32 => (sbyte)Int32,
        Tags.Int64 => (sbyte)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public short? TryToInt16() => Tag switch
    {
        Tags.None => null,
        Tags.Byte => Byte,
        Tags.UInt16 => (short)UInt16,
        Tags.UInt32 => (short)UInt32,
        Tags.UInt64 => (short)UInt64,
        Tags.SByte => SByte,
        Tags.Int16 => Int16,
        Tags.Int32 => (short)Int32,
        Tags.Int64 => (short)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public int? TryToInt32() => Tag switch
    {
        Tags.None => null,
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => (int)UInt32,
        Tags.UInt64 => (int)UInt64,
        Tags.SByte => SByte,
        Tags.Int16 => Int16,
        Tags.Int32 => Int32,
        Tags.Int64 => (int)Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    public long? TryToInt64() => Tag switch
    {
        Tags.None => null,
        Tags.Byte => Byte,
        Tags.UInt16 => UInt16,
        Tags.UInt32 => UInt32,
        Tags.UInt64 => (long)UInt64,
        Tags.SByte => SByte,
        Tags.Int16 => Int16,
        Tags.Int32 => Int32,
        Tags.Int64 => Int64,
        _ => throw new ArgumentOutOfRangeException()
    };

    #endregion
}
