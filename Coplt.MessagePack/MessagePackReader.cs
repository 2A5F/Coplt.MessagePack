using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Coplt.MessagePack;

using static MessagePackTags;

public static class MessagePackReader
{
    public static MessagePackReader<TSource> Create<TSource>(TSource Source, bool SourceOwner = true) where TSource : IReadSource, allows ref struct
        => new(Source, SourceOwner);
    public static AsyncMessagePackReader<TSource> CreateAsync<TSource>(TSource Source, bool SourceOwner = true) where TSource : IAsyncReadSource
        => new(Source, SourceOwner);
}

public ref struct MessagePackReader<TSource>(TSource Source, bool SourceOwner = true) : IDisposable
    where TSource : IReadSource, allows ref struct
{
    #region Fields

    public TSource Source = Source;

    #endregion

    #region Dispose

    public void Dispose()
    {
        if (SourceOwner) Source.Dispose();
    }

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

    #region SkipOnce

    /// <returns>Is skipped</returns>
    public bool SkipOnce()
    {
        var tt = Source.Peek<byte>();
        if (tt is null) return false;
        var t = tt.GetValueOrDefault();
        switch ((MessagePackTags)t)
        {
            case Nil or False or True:
                Source.Read(1);
                return true;
            case UInt8 or Int8:
                if (Source.Peek(2).Length != 2) throw new InvalidMessagePackDataException();
                Source.Read(2);
                return true;
            case UInt16 or Int16 or FixExt1:
                if (Source.Peek(3).Length != 3) throw new InvalidMessagePackDataException();
                Source.Read(3);
                return true;
            case FixExt2:
                if (Source.Peek(4).Length != 4) throw new InvalidMessagePackDataException();
                Source.Read(4);
                return true;
            case UInt32 or Int32 or Float32 or Float64:
                if (Source.Peek(5).Length != 5) throw new InvalidMessagePackDataException();
                Source.Read(5);
                return true;
            case FixExt4:
                if (Source.Peek(6).Length != 6) throw new InvalidMessagePackDataException();
                Source.Read(6);
                return true;
            case UInt64 or Int64:
                if (Source.Peek(9).Length != 9) throw new InvalidMessagePackDataException();
                Source.Read(9);
                return true;
            case FixExt8:
                if (Source.Peek(10).Length != 10) throw new InvalidMessagePackDataException();
                Source.Read(10);
                return true;
            case FixExt16:
                if (Source.Peek(18).Length != 18) throw new InvalidMessagePackDataException();
                Source.Read(18);
                return true;
            case String8 or Bytes8:
            {
                var bytes = Source.Peek(2);
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var len = bytes[1] + 2;
                if (Source.Peek(len).Length != len) throw new InvalidMessagePackDataException();
                Source.Read(len);
                return true;
            }
            case String16 or Bytes16:
            {
                var bytes = Source.Peek(4);
                if (bytes.Length < 4) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE() + 3;
                if (Source.Peek(len).Length != len) throw new InvalidMessagePackDataException();
                Source.Read(len);
                return true;
            }
            case String32 or Bytes32:
            {
                var bytes = Source.Peek(6);
                if (bytes.Length < 6) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, int>(ref Unsafe.AsRef(in bytes[1])).BE() + 5;
                if (Source.Peek(len).Length != len) throw new InvalidMessagePackDataException();
                Source.Read(len);
                return true;
            }
            case Array16:
            {
                var bytes = Source.Peek(3);
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                for (var i = 0; i < len; i++)
                {
                    if (!SkipOnce()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case Array32:
            {
                var bytes = Source.Peek(5);
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(5);
                for (var i = 0u; i < len; i++)
                {
                    if (!SkipOnce()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case Map16:
            {
                var bytes = Source.Peek(3);
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                for (var i = 0; i < len; i++)
                {
                    if (!SkipOnce() || !SkipOnce()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case Map32:
            {
                var bytes = Source.Peek(5);
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(5);
                for (var i = 0u; i < len; i++)
                {
                    if (!SkipOnce() || !SkipOnce()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case Ext8:
            {
                var bytes = Source.Peek(3);
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var len = bytes[1] + 3;
                if (Source.Peek(len).Length != len) throw new InvalidMessagePackDataException();
                Source.Read(len);
                return true;
            }
            case Ext16:
            {
                var bytes = Source.Peek(4);
                if (bytes.Length < 4) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE() + 4;
                if (Source.Peek(len).Length != len) throw new InvalidMessagePackDataException();
                Source.Read(len);
                return true;
            }
            case Ext32:
            {
                var bytes = Source.Peek(6);
                if (bytes.Length < 6) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, int>(ref Unsafe.AsRef(in bytes[1])).BE() + 6;
                if (Source.Peek(len).Length != len) throw new InvalidMessagePackDataException();
                Source.Read(len);
                return true;
            }
        }
        if ((t & 0b1000_0000) == 0 || (t & 0b1110_0000) == 0b1110_0000)
        {
            Source.Read(1);
            return true;
        }
        if ((t & 0b1110_0000) == 0b1010_0000)
        {
            var len = 1 + t & 0b0001_1111;
            if (Source.Peek(len).Length != len) throw new InvalidMessagePackDataException();
            Source.Read(len);
            return true;
        }
        switch (t & 0b1111_0000)
        {
            case 0b1001_0000:
            {
                var len = t & 0b0000_1111;
                Source.Read(1);
                for (var i = 0; i < len; i++)
                {
                    if (!SkipOnce()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case 0b1000_0000:
            {
                var len = t & 0b0000_1111;
                Source.Read(1);
                for (var i = 0; i < len; i++)
                {
                    if (!SkipOnce() || !SkipOnce()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            default:
                return false;
        }
    }

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
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var v = bytes[1];
                Source.Read(2);
                return v;
            }
            case Int8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var v = bytes[1];
                Source.Read(2);
                return (sbyte)v;
            }
            case UInt16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                return v;
            }
            case Int16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, short>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                return v;
            }
            case UInt32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(5);
                return v;
            }
            case Int32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, int>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(5);
                return v;
            }
            case UInt64:
            {
                if (bytes.Length < 9) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(9);
                return v;
            }
            case Int64:
            {
                if (bytes.Length < 9) throw new InvalidMessagePackDataException();
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
            if (bytes.Length < 5) throw new InvalidMessagePackDataException();
            var v = Unsafe.As<byte, float>(ref Unsafe.AsRef(in bytes[1])).BE();
            Source.Read(5);
            return v;
        }
        else if ((MessagePackTags)t is Float64)
        {
            if (bytes.Length < 9) throw new InvalidMessagePackDataException();
            var v = Unsafe.As<byte, double>(ref Unsafe.AsRef(in bytes[1])).BE();
            Source.Read(9);
            return v;
        }
        return MessagePackFloat.None;
    }

    public float? ReadSingle() => ReadFloat().TryToSingle();

    public double? ReadDouble() => ReadFloat().TryToDouble();

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
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var v = bytes[1];
                return (v, 2);
            }
            case String16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                return (v, 3);
            }
            case String32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
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
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
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
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var str = Encoding.UTF8.GetString(bytes[data_start..]);
        Source.Read(len);
        return str;
    }

    #region ReadStringUtf8 With handler

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public int? ReadStringUtf8(Action<ReadOnlySpan<byte>> handler)
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        handler(bytes[data_start..]);
        Source.Read(len);
        return len;
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public int? ReadStringUtf8<A>(A arg, Action<A, ReadOnlySpan<byte>> handler) where A : allows ref struct
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        handler(arg, bytes[data_start..]);
        Source.Read(len);
        return len;
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public (int Length, R Value)? ReadStringUtf8<R>(Func<ReadOnlySpan<byte>, R> handler)
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var r = handler(bytes[data_start..]);
        Source.Read(len);
        return (len, r);
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public (int Length, R Value)? ReadStringUtf8<A, R>(A arg, Func<A, ReadOnlySpan<byte>, R> handler) where A : allows ref struct
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var r = handler(arg, bytes[data_start..]);
        Source.Read(len);
        return (len, r);
    }

    #endregion

    #region ReadString With handler

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public int? ReadString(Action<ReadOnlySpan<char>> handler)
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var utf8 = bytes[data_start..];
        var arr = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(length));
        try
        {
            var char_count = Encoding.UTF8.GetChars(utf8, arr);
            handler(arr.AsSpan(0, char_count));
            Source.Read(len);
            return len;
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arr);
        }
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public int? ReadString<A>(A arg, Action<A, ReadOnlySpan<char>> handler) where A : allows ref struct
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var utf8 = bytes[data_start..];
        var arr = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(length));
        try
        {
            var char_count = Encoding.UTF8.GetChars(utf8, arr);
            handler(arg, arr.AsSpan(0, char_count));
            Source.Read(len);
            return len;
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arr);
        }
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public (int Length, R Value)? ReadString<R>(Func<ReadOnlySpan<char>, R> handler)
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var utf8 = bytes[data_start..];
        var arr = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(length));
        try
        {
            var char_count = Encoding.UTF8.GetChars(utf8, arr);
            var r = handler(arr.AsSpan(0, char_count));
            Source.Read(len);
            return (len, r);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arr);
        }
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public (int Length, R Value)? ReadString<A, R>(A arg, Func<A, ReadOnlySpan<char>, R> handler) where A : allows ref struct
    {
        var lds = PeekStringUtf8LengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var utf8 = bytes[data_start..];
        var arr = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(length));
        try
        {
            var char_count = Encoding.UTF8.GetChars(utf8, arr);
            var r = handler(arg, arr.AsSpan(0, char_count));
            Source.Read(len);
            return (len, r);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arr);
        }
    }

    #endregion

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
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var v = bytes[1];
                return (v, 2);
            }
            case Bytes16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                return (v, 3);
            }
            case Bytes32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
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
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
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
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var arr = bytes[data_start..].ToArray();
        Source.Read(len);
        return arr;
    }

    #region ReadBytes with handler

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public int? ReadBytes(Action<ReadOnlySpan<byte>> handler)
    {
        var lds = PeekBytesLengthWithDataStart();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = Source.Peek(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
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
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
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
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
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
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var r = handler(arg, bytes[data_start..]);
        Source.Read(len);
        return (length, r);
    }

    #endregion

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
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                return v;
            }
            case Array32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
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
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes[1])).BE();
                Source.Read(3);
                return v;
            }
            case Map32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
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
                if (bytes.Length < 6) throw new InvalidMessagePackDataException();
                if (bytes[1] != unchecked((byte)-1)) return null;
                var seconds = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes[2])).BE();
                Source.Read(6);
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
            }
            case FixExt8:
            {
                if (bytes.Length < 10) throw new InvalidMessagePackDataException();
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
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                if (bytes[2] != unchecked((byte)-1)) return null;
                if (bytes[1] != 12) throw new InvalidMessagePackDataException();
                if (bytes.Length < 15) throw new InvalidMessagePackDataException();
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

    #region Int128

    public UInt128? ReadUInt128FromBytes()
    {
        var pr = Source.Peek<MessagePackTags, byte, UInt128>();
        if (pr is null) return null;
        var (tag, len, val) = pr.GetValueOrDefault();
        if (tag == Bytes8 || len != 16) return null;
        return val.BE();
    }

    public Int128? ReadInt128FromBytes()
    {
        var pr = Source.Peek<MessagePackTags, byte, Int128>();
        if (pr is null) return null;
        var (tag, len, val) = pr.GetValueOrDefault();
        if (tag == Bytes8 || len != 16) return null;
        return val.BE();
    }

    #endregion
}

public sealed class AsyncMessagePackReader<TSource>(TSource Source, bool SourceOwner = true) : IDisposable, IAsyncDisposable
    where TSource : IAsyncReadSource
{
    #region Fields

    public TSource Source = Source;

    #endregion

    #region Dispose

    public void Dispose()
    {
        if (SourceOwner) Source.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (SourceOwner) await Source.DisposeAsync();
    }

    #endregion

    #region PeekAsyncType

    public async ValueTask<MessagePackType> PeekTypeAsync() => await Source.PeekAsync<MessagePackTags>() switch
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

    #region SkipOnce

    /// <returns>Is skipped</returns>
    public async ValueTask<bool> SkipOnceAsync()
    {
        var tt = await Source.PeekAsync<byte>();
        if (tt is null) return false;
        var t = tt.GetValueOrDefault();
        switch ((MessagePackTags)t)
        {
            case Nil or False or True:
                await Source.ReadAsync(1);
                return true;
            case UInt8 or Int8:
                if ((await Source.PeekAsync(2)).Length != 2) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(2);
                return true;
            case UInt16 or Int16 or FixExt1:
                if ((await Source.PeekAsync(3)).Length != 3) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(3);
                return true;
            case FixExt2:
                if ((await Source.PeekAsync(4)).Length != 4) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(4);
                return true;
            case UInt32 or Int32 or Float32 or Float64:
                if ((await Source.PeekAsync(5)).Length != 5) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(5);
                return true;
            case FixExt4:
                if ((await Source.PeekAsync(6)).Length != 6) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(6);
                return true;
            case UInt64 or Int64:
                if ((await Source.PeekAsync(9)).Length != 9) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(9);
                return true;
            case FixExt8:
                if ((await Source.PeekAsync(10)).Length != 10) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(10);
                return true;
            case FixExt16:
                if ((await Source.PeekAsync(18)).Length != 18) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(18);
                return true;
            case String8 or Bytes8:
            {
                var bytes = await Source.PeekAsync(2);
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var len = bytes.Span[1] + 2;
                if ((await Source.PeekAsync(len)).Length != len) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(len);
                return true;
            }
            case String16 or Bytes16:
            {
                var bytes = await Source.PeekAsync(4);
                if (bytes.Length < 4) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE() + 3;
                if ((await Source.PeekAsync(len)).Length != len) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(len);
                return true;
            }
            case String32 or Bytes32:
            {
                var bytes = await Source.PeekAsync(6);
                if (bytes.Length < 6) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, int>(ref Unsafe.AsRef(in bytes.Span[1])).BE() + 5;
                if ((await Source.PeekAsync(len)).Length != len) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(len);
                return true;
            }
            case Array16:
            {
                var bytes = await Source.PeekAsync(3);
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(3);
                for (var i = 0; i < len; i++)
                {
                    if (!await SkipOnceAsync()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case Array32:
            {
                var bytes = await Source.PeekAsync(5);
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(5);
                for (var i = 0u; i < len; i++)
                {
                    if (!await SkipOnceAsync()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case Map16:
            {
                var bytes = await Source.PeekAsync(3);
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(3);
                for (var i = 0; i < len; i++)
                {
                    if (!await SkipOnceAsync() || !await SkipOnceAsync()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case Map32:
            {
                var bytes = await Source.PeekAsync(5);
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(5);
                for (var i = 0u; i < len; i++)
                {
                    if (!await SkipOnceAsync() || !await SkipOnceAsync()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case Ext8:
            {
                var bytes = await Source.PeekAsync(3);
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var len = bytes.Span[1] + 3;
                if ((await Source.PeekAsync(len)).Length != len) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(len);
                return true;
            }
            case Ext16:
            {
                var bytes = await Source.PeekAsync(4);
                if (bytes.Length < 4) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE() + 4;
                if ((await Source.PeekAsync(len)).Length != len) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(len);
                return true;
            }
            case Ext32:
            {
                var bytes = await Source.PeekAsync(6);
                if (bytes.Length < 6) throw new InvalidMessagePackDataException();
                var len = Unsafe.As<byte, int>(ref Unsafe.AsRef(in bytes.Span[1])).BE() + 6;
                if ((await Source.PeekAsync(len)).Length != len) throw new InvalidMessagePackDataException();
                await Source.ReadAsync(len);
                return true;
            }
        }
        if ((t & 0b1000_0000) == 0 || (t & 0b1110_0000) == 0b1110_0000)
        {
            await Source.ReadAsync(1);
            return true;
        }
        if ((t & 0b1110_0000) == 0b1010_0000)
        {
            var len = 1 + t & 0b0001_1111;
            if ((await Source.PeekAsync(len)).Length != len) throw new InvalidMessagePackDataException();
            await Source.ReadAsync(len);
            return true;
        }
        switch (t & 0b1111_0000)
        {
            case 0b1001_0000:
            {
                var len = t & 0b0000_1111;
                await Source.ReadAsync(1);
                for (var i = 0; i < len; i++)
                {
                    if (!await SkipOnceAsync()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            case 0b1000_0000:
            {
                var len = t & 0b0000_1111;
                await Source.ReadAsync(1);
                for (var i = 0; i < len; i++)
                {
                    if (!await SkipOnceAsync() || !await SkipOnceAsync()) throw new InvalidMessagePackDataException();
                }
                return true;
            }
            default:
                return false;
        }
    }

    #endregion

    #region ReadNull

    public async ValueTask<bool> ReadNullAsync()
    {
        if (await Source.PeekAsync<MessagePackTags>() is not Nil) return false;
        await Source.ReadAsync<MessagePackTags>();
        return true;
    }

    #endregion

    #region ReadBool

    public async ValueTask<bool?> ReadBoolAsync()
    {
        var t = await Source.PeekAsync<MessagePackTags>();
        if (t is not (True or False)) return null;
        await Source.ReadAsync<MessagePackTags>();
        return t switch
        {
            True => true,
            False => false,
            _ => throw new UnreachableException(),
        };
    }

    #endregion

    #region ReadInteger

    public async ValueTask<MessagePackInteger> ReadIntegerAsync()
    {
        var bytes = await Source.PeekAsync(9);
        if (bytes.IsEmpty) return MessagePackInteger.None;
        var t = bytes.Span[0];
        if (t < 128)
        {
            await Source.ReadAsync(1);
            return t;
        }
        else if (unchecked((sbyte)t) is < 0 and >= -32)
        {
            await Source.ReadAsync(1);
            return unchecked((sbyte)t);
        }
        switch ((MessagePackTags)t)
        {
            case UInt8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var v = bytes.Span[1];
                await Source.ReadAsync(2);
                return v;
            }
            case Int8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var v = bytes.Span[1];
                await Source.ReadAsync(2);
                return (sbyte)v;
            }
            case UInt16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(3);
                return v;
            }
            case Int16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, short>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(3);
                return v;
            }
            case UInt32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(5);
                return v;
            }
            case Int32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, int>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(5);
                return v;
            }
            case UInt64:
            {
                if (bytes.Length < 9) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(9);
                return v;
            }
            case Int64:
            {
                if (bytes.Length < 9) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, long>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(9);
                return v;
            }
        }
        return MessagePackInteger.None;
    }

    public async ValueTask<byte?> ReadByteAsync() => (await ReadIntegerAsync()).TryToByte();

    public async ValueTask<ushort?> ReadUInt16Async() => (await ReadIntegerAsync()).TryToUInt16();

    public async ValueTask<uint?> ReadUInt32Async() => (await ReadIntegerAsync()).TryToUInt32();

    public async ValueTask<ulong?> ReadUInt64Async() => (await ReadIntegerAsync()).TryToUInt64();

    public async ValueTask<sbyte?> ReadSByteAsync() => (await ReadIntegerAsync()).TryToSByte();

    public async ValueTask<short?> ReadInt16Async() => (await ReadIntegerAsync()).TryToInt16();

    public async ValueTask<int?> ReadInt32Async() => (await ReadIntegerAsync()).TryToInt32();

    public async ValueTask<long?> ReadInt64Async() => (await ReadIntegerAsync()).TryToInt64();

    #endregion

    #region ReadFloat

    public async ValueTask<MessagePackFloat> ReadFloatAsync()
    {
        var bytes = await Source.PeekAsync(9);
        if (bytes.IsEmpty) return MessagePackFloat.None;
        var t = bytes.Span[0];
        if ((MessagePackTags)t is Float32)
        {
            if (bytes.Length < 5) throw new InvalidMessagePackDataException();
            var v = Unsafe.As<byte, float>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
            await Source.ReadAsync(5);
            return v;
        }
        else if ((MessagePackTags)t is Float64)
        {
            if (bytes.Length < 9) throw new InvalidMessagePackDataException();
            var v = Unsafe.As<byte, double>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
            await Source.ReadAsync(9);
            return v;
        }
        return MessagePackFloat.None;
    }

    public async ValueTask<float?> ReadSingleAsync() => (await ReadFloatAsync()).TryToSingle();

    public async ValueTask<double?> ReadDoubleAsync() => (await ReadFloatAsync()).TryToDouble();

    #endregion

    #region ReadString

    public async ValueTask<int?> PeekStringUtf8LengthAsync() => (await PeekStringUtf8LengthWithDataStartAsync())?.Length;
    public async ValueTask<(int Length, int DataStart)?> PeekStringUtf8LengthWithDataStartAsync()
    {
        var bytes = await Source.PeekAsync(5);
        if (bytes.IsEmpty) return null;
        var t = bytes.Span[0];
        if ((t & 0b1010_0000) == 0b1010_0000)
        {
            var len = t & 0b0001_1111;
            return (len, 1);
        }
        switch ((MessagePackTags)t)
        {
            case String8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var v = bytes.Span[1];
                return (v, 2);
            }
            case String16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                return (v, 3);
            }
            case String32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                return ((int)v, 5);
            }
        }
        return null;
    }

    /// <exception cref="ArgumentOutOfRangeException">Will throw if the buffer is not large enough. Please use <see cref="PeekStringUtf8LengthAsync"/> to query the length first.</exception>
    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<int?> ReadStringUtf8Async(Memory<byte> buffer)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        if (buffer.Length < length) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer too small");
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        bytes[data_start..].CopyTo(buffer);
        await Source.ReadAsync(len);
        return length;
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<string?> ReadStringAsync()
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var str = Encoding.UTF8.GetString(bytes.Span[data_start..]);
        await Source.ReadAsync(len);
        return str;
    }

    #region ReadStringUtf8 With handler

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<int?> ReadStringUtf8Async(Func<ReadOnlyMemory<byte>, ValueTask> handler)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        await handler(bytes[data_start..]);
        await Source.ReadAsync(len);
        return len;
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<int?> ReadStringUtf8Async<A>(A arg, Func<A, ReadOnlyMemory<byte>, ValueTask> handler)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        await handler(arg, bytes[data_start..]);
        await Source.ReadAsync(len);
        return len;
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<(int Length, R Value)?> ReadStringUtf8Async<R>(Func<ReadOnlyMemory<byte>, ValueTask<R>> handler)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var r = await handler(bytes[data_start..]);
        await Source.ReadAsync(len);
        return (len, r);
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<(int Length, R Value)?> ReadStringUtf8Async<A, R>(A arg, Func<A, ReadOnlyMemory<byte>, ValueTask<R>> handler)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var r = await handler(arg, bytes[data_start..]);
        await Source.ReadAsync(len);
        return (len, r);
    }

    #endregion

    #region ReadString With handler

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<int?> ReadStringAsync(Func<ReadOnlyMemory<char>, ValueTask> handler)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var utf8 = bytes[data_start..];
        var arr = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(length));
        try
        {
            var char_count = Encoding.UTF8.GetChars(utf8.Span, arr);
            await handler(arr.AsMemory(0, char_count));
            await Source.ReadAsync(len);
            return len;
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arr);
        }
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<int?> ReadStringAsync<A>(A arg, Func<A, ReadOnlyMemory<char>, ValueTask> handler)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var utf8 = bytes[data_start..];
        var arr = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(length));
        try
        {
            var char_count = Encoding.UTF8.GetChars(utf8.Span, arr);
            await handler(arg, arr.AsMemory(0, char_count));
            await Source.ReadAsync(len);
            return len;
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arr);
        }
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<(int Length, R Value)?> ReadStringAsync<R>(Func<ReadOnlyMemory<char>, ValueTask<R>> handler)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var utf8 = bytes[data_start..];
        var arr = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(length));
        try
        {
            var char_count = Encoding.UTF8.GetChars(utf8.Span, arr);
            var r = await handler(arr.AsMemory(0, char_count));
            await Source.ReadAsync(len);
            return (len, r);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arr);
        }
    }

    ///<exception cref="OutOfMemoryException">If the string is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<(int Length, R Value)?> ReadStringAsync<A, R>(A arg, Func<A, ReadOnlyMemory<char>, ValueTask<R>> handler)
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var utf8 = bytes[data_start..];
        var arr = ArrayPool<char>.Shared.Rent(Encoding.UTF8.GetMaxCharCount(length));
        try
        {
            var char_count = Encoding.UTF8.GetChars(utf8.Span, arr);
            var r = await handler(arg, arr.AsMemory(0, char_count));
            await Source.ReadAsync(len);
            return (len, r);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(arr);
        }
    }

    #endregion

    #endregion

    #region ReadBytes

    public async ValueTask<int?> PeekBytesLengthAsync() => (await PeekBytesLengthWithDataStartAsync())?.Length;
    public async ValueTask<(int Length, int DataStart)?> PeekBytesLengthWithDataStartAsync()
    {
        var bytes = await Source.PeekAsync(5);
        if (bytes.IsEmpty) return null;
        var t = bytes.Span[0];
        switch ((MessagePackTags)t)
        {
            case Bytes8:
            {
                if (bytes.Length < 2) throw new InvalidMessagePackDataException();
                var v = bytes.Span[1];
                return (v, 2);
            }
            case Bytes16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                return (v, 3);
            }
            case Bytes32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                return ((int)v, 5);
            }
        }
        return null;
    }

    /// <exception cref="ArgumentOutOfRangeException">Will throw if the buffer is not large enough. Please use <see cref="PeekBytesLengthAsync"/> to query the length first.</exception>
    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<int?> ReadBytesAsync(Memory<byte> buffer)
    {
        var lds = await PeekBytesLengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        if (buffer.Length < length) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer too small");
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        bytes[data_start..].CopyTo(buffer);
        await Source.ReadAsync(len);
        return length;
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<byte[]?> ReadBytesArrayAsync()
    {
        var lds = await PeekStringUtf8LengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var arr = bytes[data_start..].ToArray();
        await Source.ReadAsync(len);
        return arr;
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<int?> ReadBytesAsync(Func<ReadOnlyMemory<byte>, ValueTask> handler)
    {
        var lds = await PeekBytesLengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        await handler(bytes[data_start..]);
        await Source.ReadAsync(len);
        return length;
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<int?> ReadBytesAsync<A>(A arg, Func<A, ReadOnlyMemory<byte>, ValueTask> handler)
    {
        var lds = await PeekBytesLengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        await handler(arg, bytes[data_start..]);
        await Source.ReadAsync(len);
        return length;
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<(int Length, R Value)?> ReadBytesAsync<R>(Func<ReadOnlyMemory<byte>, ValueTask<R>> handler)
    {
        var lds = await PeekBytesLengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var r = await handler(bytes[data_start..]);
        await Source.ReadAsync(len);
        return (length, r);
    }

    ///<exception cref="OutOfMemoryException">If the array is larger than the maximum size allowed by the clr</exception>
    public async ValueTask<(int Length, R Value)?> ReadBytesAsync<A, R>(A arg, Func<A, ReadOnlyMemory<byte>, ValueTask<R>> handler)
    {
        var lds = await PeekBytesLengthWithDataStartAsync();
        if (lds is null) return null;
        var (length, data_start) = lds.Value;
        if (length < 0 || length > Array.MaxLength) throw new OutOfMemoryException();
        var len = length + data_start;
        var bytes = await Source.PeekAsync(len);
        if (bytes.Length < len) throw new InvalidMessagePackDataException();
        var r = await handler(arg, bytes[data_start..]);
        await Source.ReadAsync(len);
        return (length, r);
    }

    #endregion

    #region ArrayHead

    public async ValueTask<int?> ReadArrayHeadAsync()
    {
        var bytes = await Source.PeekAsync(5);
        if (bytes.IsEmpty) return null;
        var t = bytes.Span[0];
        if ((t & 0b1001_0000) == 0b1001_0000)
        {
            var len = t & 0b0000_1111;
            await Source.ReadAsync(1);
            return len;
        }
        switch ((MessagePackTags)t)
        {
            case Array16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(3);
                return v;
            }
            case Array32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(5);
                return (int)v;
            }
        }
        return null;
    }

    #endregion

    #region MapHead

    public async ValueTask<int?> ReadMapHeadAsync()
    {
        var bytes = await Source.PeekAsync(5);
        if (bytes.IsEmpty) return null;
        var t = bytes.Span[0];
        if ((t & 0b1000_0000) == 0b1000_0000)
        {
            var len = t & 0b0000_1111;
            await Source.ReadAsync(1);
            return len;
        }
        switch ((MessagePackTags)t)
        {
            case Map16:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, ushort>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(3);
                return v;
            }
            case Map32:
            {
                if (bytes.Length < 5) throw new InvalidMessagePackDataException();
                var v = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[1])).BE();
                await Source.ReadAsync(5);
                return (int)v;
            }
        }
        return null;
    }

    #endregion

    #region ReadDateTimeOffset

    public async ValueTask<DateTimeOffset?> ReadDateTimeOffsetAsync()
    {
        var bytes = await Source.PeekAsync(15);
        if (bytes.IsEmpty) return null;
        var t = (MessagePackTags)bytes.Span[0];
        switch (t)
        {
            case FixExt4:
            {
                if (bytes.Length < 6) throw new InvalidMessagePackDataException();
                if (bytes.Span[1] != unchecked((byte)-1)) return null;
                var seconds = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[2])).BE();
                await Source.ReadAsync(6);
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
            }
            case FixExt8:
            {
                if (bytes.Length < 10) throw new InvalidMessagePackDataException();
                if (bytes.Span[1] != unchecked((byte)-1)) return null;
                var data64 = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in bytes.Span[2])).BE();
                await Source.ReadAsync(10);
                var nanoseconds = data64 >> 34;
                var seconds = data64 & 0x00000003ffffffffUL;
                var nanoseconds100 = nanoseconds / 100;
                var ticks = (long)(seconds * TimeSpan.TicksPerSecond + nanoseconds100);
                ticks += DateTimeOffset.UnixEpoch.UtcTicks;
                return new DateTimeOffset(ticks, TimeSpan.Zero);
            }
            case Ext8:
            {
                if (bytes.Length < 3) throw new InvalidMessagePackDataException();
                if (bytes.Span[2] != unchecked((byte)-1)) return null;
                if (bytes.Span[1] != 12) throw new InvalidMessagePackDataException();
                if (bytes.Length < 15) throw new InvalidMessagePackDataException();
                var nanoseconds = Unsafe.As<byte, uint>(ref Unsafe.AsRef(in bytes.Span[3])).BE();
                var seconds = Unsafe.As<byte, ulong>(ref Unsafe.AsRef(in bytes.Span[7])).BE();
                await Source.ReadAsync(15);
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

    public async ValueTask<decimal?> ReadDecimalFromBytesAsync()
    {
        var pr = await Source.PeekAsync<MessagePackTags, byte, decimal>();
        if (pr is null) return null;
        var (tag, len, val) = pr.GetValueOrDefault();
        if (tag == Bytes8 || len != 16) return null;
        return val.BE();
    }

    #endregion

    #region ReadGuid

    public async ValueTask<Guid?> ReadGuidFromBytesAsync()
    {
        var pr = await Source.PeekAsync<MessagePackTags, byte, Guid>();
        if (pr is null) return null;
        var (tag, len, val) = pr.GetValueOrDefault();
        if (tag == Bytes8 || len != 16) return null;
        return val.BE();
    }

    #endregion

    #region Int128

    public async ValueTask<UInt128?> ReadUInt128FromBytes()
    {
        var pr = await Source.PeekAsync<MessagePackTags, byte, UInt128>();
        if (pr is null) return null;
        var (tag, len, val) = pr.GetValueOrDefault();
        if (tag == Bytes8 || len != 16) return null;
        return val.BE();
    }

    public async ValueTask<Int128?> ReadInt128FromBytes()
    {
        var pr = await Source.PeekAsync<MessagePackTags, byte, Int128>();
        if (pr is null) return null;
        var (tag, len, val) = pr.GetValueOrDefault();
        if (tag == Bytes8 || len != 16) return null;
        return val.BE();
    }

    #endregion
}
