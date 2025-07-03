namespace Coplt.MessagePack.Converters;

public readonly record struct BooleanConvert : IMessagePackConverter<bool>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, bool value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteBoolean(value);
    public static bool Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadBool() ?? throw new MessagePackException("Expected bool but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, bool value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteBooleanAsync(value);
    public static async ValueTask<bool> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadBoolAsync() ?? throw new MessagePackException("Expected bool but not");
}

public readonly record struct ByteConvert : IMessagePackConverter<byte>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, byte value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteByte(value);
    public static byte Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadByte() ?? throw new MessagePackException($"Expected {nameof(Byte)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, byte value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteByteAsync(value);
    public static async ValueTask<byte> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadByteAsync() ?? throw new MessagePackException($"Expected {nameof(Byte)} but not");
}

public readonly record struct UInt16Convert : IMessagePackConverter<ushort>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, ushort value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteUInt16(value);
    public static ushort Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadUInt16() ?? throw new MessagePackException($"Expected {nameof(UInt16)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, ushort value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteUInt16Async(value);
    public static async ValueTask<ushort> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadUInt16Async() ?? throw new MessagePackException($"Expected {nameof(UInt16)} but not");
}

public readonly record struct UInt32Convert : IMessagePackConverter<uint>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, uint value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteUInt32(value);
    public static uint Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadUInt32() ?? throw new MessagePackException($"Expected {nameof(UInt32)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, uint value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteUInt32Async(value);
    public static async ValueTask<uint> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadUInt32Async() ?? throw new MessagePackException($"Expected {nameof(UInt32)} but not");
}

public readonly record struct UInt64Convert : IMessagePackConverter<ulong>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, ulong value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteUInt64(value);
    public static ulong Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadUInt64() ?? throw new MessagePackException($"Expected {nameof(UInt64)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, ulong value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteUInt64Async(value);
    public static async ValueTask<ulong> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadUInt64Async() ?? throw new MessagePackException($"Expected {nameof(UInt64)} but not");
}

public readonly record struct UInt128Convert : IMessagePackConverter<UInt128>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, UInt128 value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteUInt128AsBytes(value);
    public static UInt128 Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadUInt128FromBytes() ?? throw new MessagePackException($"Expected {nameof(UInt128)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, UInt128 value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteUInt128AsBytes(value);
    public static async ValueTask<UInt128> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadUInt128FromBytes() ?? throw new MessagePackException($"Expected {nameof(UInt128)} but not");
}

public readonly record struct SByteConvert : IMessagePackConverter<sbyte>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, sbyte value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteSByte(value);
    public static sbyte Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadSByte() ?? throw new MessagePackException($"Expected {nameof(SByte)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, sbyte value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteSByteAsync(value);
    public static async ValueTask<sbyte> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadSByteAsync() ?? throw new MessagePackException($"Expected {nameof(SByte)} but not");
}

public readonly record struct Int16Convert : IMessagePackConverter<short>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, short value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteInt16(value);
    public static short Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadInt16() ?? throw new MessagePackException($"Expected {nameof(Int16)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, short value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteInt16Async(value);
    public static async ValueTask<short> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadInt16Async() ?? throw new MessagePackException($"Expected {nameof(Int16)} but not");
}

public readonly record struct Int32Convert : IMessagePackConverter<int>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, int value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteInt32(value);
    public static int Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadInt32() ?? throw new MessagePackException($"Expected {nameof(Int32)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, int value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteInt32Async(value);
    public static async ValueTask<int> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadInt32Async() ?? throw new MessagePackException($"Expected {nameof(Int32)} but not");
}

public readonly record struct Int64Convert : IMessagePackConverter<long>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, long value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteInt64(value);
    public static long Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadInt64() ?? throw new MessagePackException($"Expected {nameof(Int64)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, long value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteInt64Async(value);
    public static async ValueTask<long> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadInt64Async() ?? throw new MessagePackException($"Expected {nameof(Int64)} but not");
}

public readonly record struct Int128Convert : IMessagePackConverter<Int128>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, Int128 value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
        => writer.WriteInt128AsBytes(value);
    public static Int128 Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
        => reader.ReadInt128FromBytes() ?? throw new MessagePackException($"Expected {nameof(Int128)} but not");

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, Int128 value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
        => writer.WriteInt128AsBytes(value);
    public static async ValueTask<Int128> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
        => await reader.ReadInt128FromBytes() ?? throw new MessagePackException($"Expected {nameof(Int128)} but not");
}
