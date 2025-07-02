using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Coplt.MessagePack;

using static MessagePackTags;

public ref struct MessagePackReader<TSource>(TSource Source) where TSource : IReadSource, allows ref struct
{
    #region Fields

    public TSource Source = Source;

    #endregion

    #region PeekType

    public MessagePackType PeekType() => Source.Peek<MessagePackTags>() switch
    {
        Nil => MessagePackType.Nil,
        False or True => MessagePackType.Boolean,
        UInt8 or UInt16 or UInt32 or UInt64 or Int8 or Int16 or Int32 or Int64 => MessagePackType.Integer,
        Float32 or Float64 => MessagePackType.Float,
        String8 or String16 or String32 => MessagePackType.String,
        Bytes8 or Bytes16 or Bytes32 => MessagePackType.Binary,
        Array16 or Array32 => MessagePackType.Array,
        Map16 or Map32 => MessagePackType.Map,
        FixExt1 or FixExt2 or FixExt4 or FixExt8 or FixExt16 or Ext8 or Ext16 or Ext32 => MessagePackType.Extension,
        { } t =>
            ((byte)t & 0b1000_0000) == 0
                ? MessagePackType.Integer
                : ((byte)t & 0b1110_0000) switch
                {
                    0b1110_0000 => MessagePackType.Integer,
                    0b1010_0000 => MessagePackType.String,
                    _ => ((byte)t & 0b1111_0000)switch
                    {
                        0b1001_0000 => MessagePackType.Array,
                        0b1000_0000 => MessagePackType.Map,
                        _ => MessagePackType.EOF,
                    },
                },
        null => MessagePackType.EOF,
    };

    #endregion

    #region ReadNull

    public bool ReadNull()
    {
        if (Source.Peek<MessagePackTags>() is not Nil) return false;
        Source.Read<MessagePackTags>();
        return true;
    }

    #endregion

    #region ReadBool

    public bool? ReadBool()
    {
        var t = Source.Peek<MessagePackTags>();
        if (t is not (True or False)) return null;
        Source.Read<MessagePackTags>();
        return t switch
        {
            True => true,
            False => false,
            _ => throw new UnreachableException(),
        };
    }

    #endregion

    #region ReadInteger

    public MessagePackInteger ReadInteger()
    {
        var bytes = Source.Peek(9);
        if (bytes.IsEmpty) return MessagePackInteger.None;
        var t = bytes[0];
        if (t < 128)
        {
            Source.Read(1);
            return t;
        }
        else if (unchecked((sbyte)t) is < 0 and >= -32)
        {
            Source.Read(1);
            return unchecked((sbyte)t);
        }
        switch ((MessagePackTags)t)
        {
            case UInt8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackFormatException();
                var v = bytes[1];
                Source.Read(2);
                return v;
            }
            case Int8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackFormatException();
                var v = bytes[1];
                Source.Read(2);
                return (sbyte)v;
            }
            case UInt16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                return v;
            }
            case Int16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, short>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                return v;
            }
            case UInt32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(5);
                return v;
            }
            case Int32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, int>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(5);
                return v;
            }
            case UInt64:
            {
                if (bytes.Length < 9) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(9);
                return v;
            }
            case Int64:
            {
                if (bytes.Length < 9) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, long>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(9);
                return v;
            }
        }
        return MessagePackInteger.None;
    }

    public byte? ReadByte() => ReadInteger().TryToByte();

    public ushort? ReadUInt16() => ReadInteger().TryToUInt16();

    public uint? ReadUInt32() => ReadInteger().TryToUInt32();

    public ulong? ReadUInt64() => ReadInteger().TryToUInt64();

    public sbyte? ReadSByte() => ReadInteger().TryToSByte();

    public short? ReadInt16() => ReadInteger().TryToInt16();

    public int? ReadInt32() => ReadInteger().TryToInt32();

    public long? ReadInt64() => ReadInteger().TryToInt64();

    #endregion

    #region ReadFloat

    public MessagePackFloat ReadFloat()
    {
        var bytes = Source.Peek(9);
        if (bytes.IsEmpty) return MessagePackFloat.None;
        var t = bytes[0];
        if ((MessagePackTags)t is Float32)
        {
            if (bytes.Length < 5) throw new InvalidMessagePackFormatException();
            var v = Unsafe.As<byte, float>(ref Unsafe.AsRef(in bytes[1])).BE();
            Source.Read(5);
            return v;
        }
        else if ((MessagePackTags)t is Float64)
        {
            if (bytes.Length < 9) throw new InvalidMessagePackFormatException();
            var v = Unsafe.As<byte, double>(ref Unsafe.AsRef(in bytes[1])).BE();
            Source.Read(9);
            return v;
        }
        return MessagePackFloat.None;
    }

    #endregion

    #region ReadString

    public int? PeekStringUtf8Length() => PeekStringUtf8LengthWithDataStart()?.Length;
    public (int Length, int DataStart)? PeekStringUtf8LengthWithDataStart()
    {
        var bytes = Source.Peek(5);
        if (bytes.IsEmpty) return null;
        var t = bytes[0];
        if ((t & 0b1010_0000) == 0b1010_0000)
        {
            var len = t & 0b0001_1111;
            return (len, 1);
        }
        switch ((MessagePackTags)t)
        {
            case String8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackFormatException();
                var v = bytes[1];
                return (v, 2);
            }
            case String16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                return (v, 3);
            }
            case String32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[1])).BE();
                return ((int)v, 5);
            }
        }
        return null;
    }

    /// <exception cref="ArgumentOutOfRangeException">Will throw if the buffer is not large enough. Please use <see cref="PeekStringUtf8Length"/> to query the length first.</exception>
    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public int? ReadStringUtf8(Span<byte> buffer)
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        if (buffer.Length < length) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer too small");
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackFormatException();
        bytes[data_start..].CopyTo(buffer);
        Source.Read(len);
        return length;
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public string? ReadString()
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackFormatException();
        var str = Encoding.UTF8.GetString(bytes[data_start..]);
        Source.Read(len);
        return str;
    }

    #endregion

    #region ReadBytes

    public int? PeekBytesLength() => PeekBytesLengthWithDataStart()?.Length;
    public (int Length, int DataStart)? PeekBytesLengthWithDataStart()
    {
        var bytes = Source.Peek(5);
        if (bytes.IsEmpty) return null;
        var t = bytes[0];
        switch ((MessagePackTags)t)
        {
            case Bytes8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackFormatException();
                var v = bytes[1];
                return (v, 2);
            }
            case Bytes16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                return (v, 3);
            }
            case Bytes32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[1])).BE();
                return ((int)v, 5);
            }
        }
        return null;
    }

    /// <exception cref="ArgumentOutOfRangeException">Will throw if the buffer is not large enough. Please use <see cref="PeekBytesLength"/> to query the length first.</exception>
    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public int? ReadBytes(Span<byte> buffer)
    {
        var lds = PeekBytesLengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        if (buffer.Length < length) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer too small");
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackFormatException();
        bytes[data_start..].CopyTo(buffer);
        Source.Read(len);
        return length;
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public byte[]? ReadBytesArray()
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackFormatException();
        var arr = bytes[data_start..].ToArray();
        Source.Read(len);
        return arr;
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public int? ReadBytes(Action<ReadOnlySpan<byte>> handler)
    {
        var lds = PeekBytesLengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackFormatException();
        handler(bytes[data_start..]);
        Source.Read(len);
        return length;
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public int? ReadBytes<A>(A arg, Action<A, ReadOnlySpan<byte>> handler) where A : allows ref struct
    {
        var lds = PeekBytesLengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackFormatException();
        handler(arg, bytes[data_start..]);
        Source.Read(len);
        return length;
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public (int Length, R Value)? ReadBytes<R>(Func<ReadOnlySpan<byte>, R> handler)
    {
        var lds = PeekBytesLengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackFormatException();
        var r = handler(bytes[data_start..]);
        Source.Read(len);
        return (length, r);
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public (int Length, R Value)? ReadBytes<A, R>(A arg, Func<A, ReadOnlySpan<byte>, R> handler) where A : allows ref struct
    {
        var lds = PeekBytesLengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackFormatException();
        var r = handler(arg, bytes[data_start..]);
        Source.Read(len);
        return (length, r);
    }

    #endregion

    #region ArrayHead

    public int? ReadArrayHead()
    {
        var bytes = Source.Peek(5);
        if (bytes.IsEmpty) return null;
        var t = bytes[0];
        if ((t & 0b1001_0000) == 0b1001_0000)
        {
            var len = t & 0b0000_1111;
            Source.Read(1);
            return len;
        }
        switch ((MessagePackTags)t)
        {
            case Array16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                return v;
            }
            case Array32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(5);
                return (int)v;
            }
        }
        return null;
    }

    #endregion

    #region MapHead

    public int? ReadMapHead()
    {
        var bytes = Source.Peek(5);
        if (bytes.IsEmpty) return null;
        var t = bytes[0];
        if ((t & 0b1000_0000) == 0b1000_0000)
        {
            var len = t & 0b0000_1111;
            Source.Read(1);
            return len;
        }
        switch ((MessagePackTags)t)
        {
            case Map16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                return v;
            }
            case Map32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackFormatException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(5);
                return (int)v;
            }
        }
        return null;
    }

    #endregion

    #region ReadDateTimeOffset

    public DateTimeOffset? ReadDateTimeOffset()
    {
        var bytes = Source.Peek(15);
        if (bytes.IsEmpty) return null;
        var t = (MessagePackTags)bytes[0];
        switch (t)
        {
            case FixExt4:
            {
                if (bytes.Length < 6) throw new InvalidMessagePackFormatException();
                if (bytes[1] != unchecked((byte)-1)) return null;
                var seconds = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[2])).BE();
                Source.Read(6);
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
            }
            case FixExt8:
            {
                if (bytes.Length < 10) throw new InvalidMessagePackFormatException();
                if (bytes[1] != unchecked((byte)-1)) return null;
                var data64 = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in bytes[2])).BE();
                Source.Read(10);
                var nanoseconds = data64 >> 34;
                var seconds = data64 & 0x00000003ffffffffUL;
                var nanoseconds100 = nanoseconds / 100;
                var ticks = (long)(seconds * TimeSpan.TicksPerSecond + nanoseconds100);
                ticks += DateTimeOffset.UnixEpoch.UtcTicks;
                return new DateTimeOffset(ticks, TimeSpan.Zero);
            }
            case Ext8:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackFormatException();
                if (bytes[2] != unchecked((byte)-1)) return null;
                if (bytes[1] != 12) throw new InvalidMessagePackFormatException();
                if (bytes.Length < 15) throw new InvalidMessagePackFormatException();
                var nanoseconds = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[3])).BE();
                var seconds = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in bytes[7])).BE();
                Source.Read(15);
                var nanoseconds100 = nanoseconds / 100;
                var ticks = (long)(seconds * TimeSpan.TicksPerSecond + nanoseconds100);
                ticks += DateTimeOffset.UnixEpoch.UtcTicks;
                return new DateTimeOffset(ticks, TimeSpan.Zero);
            }
        }
        return null;
    }

    #endregion

    #region ReadDecimal

    public decimal? ReadDecimalFromBytes()
    {
        var pr = Source.Peek<MessagePackTags, byte, decimal>();
        if (pr is null) return null;
        var (tag, len, val) = pr.GetValueOrDefault();
        if (tag == Bytes8 || len != 16) return null;
        return val.BE();
    }

    #endregion

    #region ReadGuid

    public Guid? ReadGuidFromBytes()
    {
        var pr = Source.Peek<MessagePackTags, byte, Guid>();
        if (pr is null) return null;
        var (tag, len, val) = pr.GetValueOrDefault();
        if (tag == Bytes8 || len != 16) return null;
        return val.BE();
    }

    #endregion
}
