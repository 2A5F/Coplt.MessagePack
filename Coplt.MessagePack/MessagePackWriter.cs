using System.Buffers;
using System.Text;

namespace Coplt.MessagePack;

using static MessagePackTags;

public static class MessagePackWriter
{
    public static MessagePackWriter<TTarget> Create<TTarget>(TTarget Target, bool TargetOwner = true) where TTarget : IWriteTarget, allows ref struct
        => new(Target, TargetOwner);
    public static AsyncMessagePackWriter<TTarget> CreateAsync<TTarget>(TTarget Target, bool TargetOwner = true) where TTarget : IAsyncWriteTarget
        => new(Target, TargetOwner);
}

public ref struct MessagePackWriter<TTarget>(TTarget Target, bool TargetOwner = true) : IDisposable
    where TTarget : IWriteTarget, allows ref struct
{
    public TTarget Target = Target;

    public void Dispose()
    {
        if (TargetOwner) Target.Dispose();
    }

    public void WriteNull() => Target.Write(Nil);
    public void WriteBoolean(bool value) => Target.Write(value ? True : False);
    public void WriteByte(byte value)
    {
        if (value < 128) Target.Write(value);
        else Target.Write(UInt8, value);
    }
    public void WriteUInt16(ushort value)
    {
        switch (value)
        {
            case < 128: Target.Write((byte)value); break;
            case < byte.MaxValue: Target.Write(UInt8, (byte)value); break;
            default: Target.Write(UInt16, value.BE()); break;
        }
    }
    public void WriteUInt32(uint value)
    {
        switch (value)
        {
            case < 128: Target.Write((byte)value); break;
            case < byte.MaxValue: Target.Write(UInt8, (byte)value); break;
            case < ushort.MaxValue: Target.Write(UInt16, ((ushort)value).BE()); break;
            default: Target.Write(UInt32, value.BE()); break;
        }
    }
    public void WriteUInt64(ulong value)
    {
        switch (value)
        {
            case < 128: Target.Write((byte)value); break;
            case < byte.MaxValue: Target.Write(UInt8, (byte)value); break;
            case < ushort.MaxValue: Target.Write(UInt16, ((ushort)value).BE()); break;
            case < uint.MaxValue: Target.Write(UInt32, ((uint)value).BE()); break;
            default: Target.Write(UInt64, value.BE()); break;
        }
    }
    public void WriteSByte(sbyte value)
    {
        switch (value)
        {
            case >= 0: Target.Write((byte)value); break;
            case >= -32: Target.Write(value); break;
            default: Target.Write(Int8, value); break;
        }
    }
    public void WriteInt16(short value)
    {
        switch (value)
        {
            case >= 0 and < 128: Target.Write((byte)value); break;
            case >= 0 and <= byte.MaxValue: Target.Write(UInt8, (byte)value); break;
            case < 0 and >= -32: Target.Write((sbyte)value); break;
            case < 0 and >= sbyte.MinValue: Target.Write(Int8, (sbyte)value); break;
            default: Target.Write(Int16, value.BE()); break;
        }
    }
    public void WriteInt32(int value)
    {
        switch (value)
        {
            case >= 0 and < 128: Target.Write((byte)value); break;
            case >= 0 and <= byte.MaxValue: Target.Write(UInt8, (byte)value); break;
            case >= 0 and <= ushort.MaxValue: Target.Write(UInt16, ((ushort)value).BE()); break;
            case < 0 and >= -32: Target.Write((sbyte)value); break;
            case < 0 and >= sbyte.MinValue: Target.Write(Int8, (sbyte)value); break;
            case < 0 and >= short.MinValue: Target.Write(Int16, ((short)value).BE()); break;
            default: Target.Write(Int32, value.BE()); break;
        }
    }
    public void WriteInt64(long value)
    {
        switch (value)
        {
            case >= 0 and < 128: Target.Write((byte)value); break;
            case >= 0 and <= byte.MaxValue: Target.Write(UInt8, (byte)value); break;
            case >= 0 and <= ushort.MaxValue: Target.Write(UInt16, ((ushort)value).BE()); break;
            case >= 0 and <= uint.MaxValue: Target.Write(UInt32, ((uint)value).BE()); break;
            case < 0 and >= -32: Target.Write((sbyte)value); break;
            case < 0 and >= sbyte.MinValue: Target.Write(Int8, (sbyte)value); break;
            case < 0 and >= short.MinValue: Target.Write(Int16, ((short)value).BE()); break;
            case < 0 and >= int.MinValue: Target.Write(Int32, ((int)value).BE()); break;
            default: Target.Write(Int64, value.BE()); break;
        }
    }
    public void WriteHalf(Half value) => WriteSingle((float)value);
    public void WriteSingle(float value) => Target.Write(Float32, value.BE());
    public void WriteDouble(double value) => Target.Write(Float64, value.BE());
    public void WriteString(ReadOnlySpan<char> value)
    {
        var arr = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(value.Length));
        try
        {
            var len = Encoding.UTF8.GetBytes(value, arr);
            WriteStringUtf8(arr.AsSpan(0, len));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(arr);
        }
    }
    public void WriteStringUtf8(ReadOnlySpan<byte> value)
    {
        var len = value.Length;
        switch (len)
        {
            case < 32: Target.Write((byte)(0b1010_0000 | len)); break;
            case <= byte.MaxValue: Target.Write(String8, (byte)len); break;
            case <= ushort.MaxValue: Target.Write(String16, ((ushort)len).BE()); break;
            default: Target.Write(String32, len.BE()); break;
        }
        Target.Write(value);
    }
    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        var len = value.Length;
        switch (len)
        {
            case <= byte.MaxValue: Target.Write(Bytes8, (byte)len); break;
            case <= ushort.MaxValue: Target.Write(Bytes16, ((ushort)len).BE()); break;
            default: Target.Write(Bytes32, len.BE()); break;
        }
        Target.Write(value);
    }
    public void WriteArrayHead(int length)
    {
        switch (length)
        {
            case <= 16: Target.Write((byte)(0b1001_0000 | length)); break;
            case <= ushort.MaxValue: Target.Write(Array16, ((ushort)length).BE()); break;
            default: Target.Write(Array32, length.BE()); break;
        }
    }
    public void WriteMapHead(int length)
    {
        switch (length)
        {
            case <= 16: Target.Write((byte)(0b1000_0000 | length)); break;
            case <= ushort.MaxValue: Target.Write(Map16, ((ushort)length).BE()); break;
            default: Target.Write(Map32, length.BE()); break;
        }
    }

    public void WriteDateTime(DateTime value) => WriteDateTimeOffset(value);
    public void WriteDateTimeOffset(DateTimeOffset value)
    {
        var ticks = value.UtcTicks - DateTimeOffset.UnixEpoch.UtcTicks;
        var (seconds, nanoseconds100) = Math.DivRem(ticks, TimeSpan.TicksPerSecond);
        var nanoseconds = nanoseconds100 * 100;

        if (seconds >> 34 == 0)
        {
            var data64 = (ulong)(nanoseconds << 34 | seconds);
            if ((data64 & 0xffffffff00000000UL) == 0UL)
            {
                var data32 = (uint)data64;
                Target.Write(FixExt4, (sbyte)-1, data32.BE());
            }
            else
            {
                Target.Write(FixExt8, (sbyte)-1, data64.BE());
            }
        }
        else
        {
            Target.Write(Ext8, (byte)12, (sbyte)-1);
            Target.Write(((uint)nanoseconds).BE(), seconds.BE());
        }
    }

    public void WriteDecimalAsBytes(decimal value) => Target.Write(Bytes8, (byte)16, value.BE());
    public void WriteGuidAsBytes(Guid value) => Target.Write(Bytes8, (byte)16, value.BE());
}

public struct AsyncMessagePackWriter<TTarget>(TTarget Target, bool TargetOwner = true) : IDisposable, IAsyncDisposable
    where TTarget : IAsyncWriteTarget
{
    public TTarget Target = Target;

    public void Dispose()
    {
        if (TargetOwner) Target.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (TargetOwner) await Target.DisposeAsync();
    }

    public ValueTask WriteNullAsync() => Target.WriteAsync(Nil);
    public ValueTask WriteBooleanAsync(bool value) => Target.WriteAsync(value ? True : False);
    public ValueTask WriteByteAsync(byte value) => value < 128 ? Target.WriteAsync(value) : Target.WriteAsync(UInt8, value);
    public ValueTask WriteUInt16Async(ushort value) => value switch
    {
        < 128 => Target.WriteAsync((byte)value),
        < byte.MaxValue => Target.WriteAsync(UInt8, (byte)value),
        _ => Target.WriteAsync(UInt16, value.BE()),
    };
    public ValueTask WriteUInt32Async(uint value) => value switch
    {
        < 128 => Target.WriteAsync((byte)value),
        < byte.MaxValue => Target.WriteAsync(UInt8, (byte)value),
        < ushort.MaxValue => Target.WriteAsync(UInt16, ((ushort)value).BE()),
        _ => Target.WriteAsync(UInt32, value.BE()),
    };
    public ValueTask WriteUInt64Async(ulong value) => value switch
    {
        < 128 => Target.WriteAsync((byte)value),
        < byte.MaxValue => Target.WriteAsync(UInt8, (byte)value),
        < ushort.MaxValue => Target.WriteAsync(UInt16, ((ushort)value).BE()),
        < uint.MaxValue => Target.WriteAsync(UInt32, ((uint)value).BE()),
        _ => Target.WriteAsync(UInt64, value.BE()),
    };
    public ValueTask WriteSByteAsync(sbyte value) => value switch
    {
        >= 0 => Target.WriteAsync((byte)value),
        >= -32 => Target.WriteAsync(value),
        _ => Target.WriteAsync(Int8, value),
    };
    public ValueTask WriteInt16Async(short value) => value switch
    {
        >= 0 and < 128 => Target.WriteAsync((byte)value),
        >= 0 and <= byte.MaxValue => Target.WriteAsync(UInt8, (byte)value),
        < 0 and >= -32 => Target.WriteAsync((sbyte)value),
        < 0 and >= sbyte.MinValue => Target.WriteAsync(Int8, (sbyte)value),
        _ => Target.WriteAsync(Int16, value.BE())
    };
    public ValueTask WriteInt32Async(int value) => value switch
    {
        >= 0 and < 128 => Target.WriteAsync((byte)value),
        >= 0 and <= byte.MaxValue => Target.WriteAsync(UInt8, (byte)value),
        >= 0 and <= ushort.MaxValue => Target.WriteAsync(UInt16, ((ushort)value).BE()),
        < 0 and >= -32 => Target.WriteAsync((sbyte)value),
        < 0 and >= sbyte.MinValue => Target.WriteAsync(Int8, (sbyte)value),
        < 0 and >= short.MinValue => Target.WriteAsync(Int16, ((short)value).BE()),
        _ => Target.WriteAsync(Int32, value.BE())
    };
    public ValueTask WriteInt64Async(long value) => value switch
    {
        >= 0 and < 128 => Target.WriteAsync((byte)value),
        >= 0 and <= byte.MaxValue => Target.WriteAsync(UInt8, (byte)value),
        >= 0 and <= ushort.MaxValue => Target.WriteAsync(UInt16, ((ushort)value).BE()),
        >= 0 and <= uint.MaxValue => Target.WriteAsync(UInt32, ((uint)value).BE()),
        < 0 and >= -32 => Target.WriteAsync((sbyte)value),
        < 0 and >= sbyte.MinValue => Target.WriteAsync(Int8, (sbyte)value),
        < 0 and >= short.MinValue => Target.WriteAsync(Int16, ((short)value).BE()),
        < 0 and >= int.MinValue => Target.WriteAsync(Int32, ((int)value).BE()),
        _ => Target.WriteAsync(Int64, value.BE())
    };
    public ValueTask WriteHalfAsync(Half value) => WriteSingleAsync((float)value);
    public ValueTask WriteSingleAsync(float value) => Target.WriteAsync(Float32, value.BE());
    public ValueTask WriteDoubleAsync(double value) => Target.WriteAsync(Float64, value.BE());
    public async ValueTask WriteStringAsync(string value)
    {
        var arr = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(value.Length));
        try
        {
            var len = Encoding.UTF8.GetBytes(value.AsSpan(), arr);
            await WriteStringUtf8Async(arr.AsMemory(0, len));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(arr);
        }
    }
    public async ValueTask WriteStringAsync(ReadOnlyMemory<char> value)
    {
        var arr = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(value.Length));
        try
        {
            var len = Encoding.UTF8.GetBytes(value.Span, arr);
            await WriteStringUtf8Async(arr.AsMemory(0, len));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(arr);
        }
    }
    public async ValueTask WriteStringUtf8Async(ReadOnlyMemory<byte> value)
    {
        var len = value.Length;
        await (len switch
        {
            < 32 => Target.WriteAsync((byte)(0b1010_0000 | len)),
            <= byte.MaxValue => Target.WriteAsync(String8, (byte)len),
            <= ushort.MaxValue => Target.WriteAsync(String16, ((ushort)len).BE()),
            _ => Target.WriteAsync(String32, len.BE()),
        });
        await Target.WriteAsync(value);
    }
    public async ValueTask WriteBytesAsync(ReadOnlyMemory<byte> value)
    {
        var len = value.Length;
        await (len switch
        {
            <= byte.MaxValue => Target.WriteAsync(Bytes8, (byte)len),
            <= ushort.MaxValue => Target.WriteAsync(Bytes16, ((ushort)len).BE()),
            _ => Target.WriteAsync(Bytes32, len.BE()),
        });
        await Target.WriteAsync(value);
    }
    public ValueTask WriteArrayHeadAsync(int length) => length switch
    {
        <= 16 => Target.WriteAsync((byte)(0b1001_0000 | length)),
        <= ushort.MaxValue => Target.WriteAsync(Array16, ((ushort)length).BE()),
        _ => Target.WriteAsync(Array32, length.BE())
    };
    public ValueTask WriteMapHeadAsync(int length) => length switch
    {
        <= 16 => Target.WriteAsync((byte)(0b1000_0000 | length)),
        <= ushort.MaxValue => Target.WriteAsync(Map16, ((ushort)length).BE()),
        _ => Target.WriteAsync(Map32, length.BE())
    };

    public ValueTask WriteDateTimeAsync(DateTime value) => WriteDateTimeOffsetAsync(value);
    public async ValueTask WriteDateTimeOffsetAsync(DateTimeOffset value)
    {
        var ticks = value.UtcTicks - DateTimeOffset.UnixEpoch.UtcTicks;
        var (seconds, nanoseconds100) = Math.DivRem(ticks, TimeSpan.TicksPerSecond);
        var nanoseconds = nanoseconds100 * 100;

        if (seconds >> 34 == 0)
        {
            var data64 = (ulong)(nanoseconds << 34 | seconds);
            if ((data64 & 0xffffffff00000000UL) == 0UL)
            {
                var data32 = (uint)data64;
                await Target.WriteAsync(FixExt4, (sbyte)-1, data32.BE());
            }
            else
            {
                await Target.WriteAsync(FixExt8, (sbyte)-1, data64.BE());
            }
        }
        else
        {
            await Target.WriteAsync(Ext8, (byte)12, (sbyte)-1);
            await Target.WriteAsync(((uint)nanoseconds).BE(), seconds.BE());
        }
    }

    public ValueTask WriteGuidAsync(Guid value) => Target.WriteAsync(Bytes8, (byte)16, value.BE());
}
