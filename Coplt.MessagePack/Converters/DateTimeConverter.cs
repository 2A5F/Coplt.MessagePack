namespace Coplt.MessagePack.Converters;

public readonly record struct DateTimeConverter : IMessagePackConverter<DateTime>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, DateTime value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteDateTime(value);
    }
    public static DateTime Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var r = reader.ReadDateTimeOffset() ?? throw new MessagePackException("Expected DateTime but not");
        return r.LocalDateTime;
    }
    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, DateTime value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        return writer.WriteDateTimeAsync(value);
    }
    public static async ValueTask<DateTime> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var r = await reader.ReadDateTimeOffsetAsync() ?? throw new MessagePackException("Expected DateTime but not");
        return r.LocalDateTime;
    }
}

public readonly record struct DateTimeOffsetConverter : IMessagePackConverter<DateTimeOffset>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, DateTimeOffset value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteDateTimeOffset(value);
    }
    public static DateTimeOffset Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        return reader.ReadDateTimeOffset() ?? throw new MessagePackException("Expected DateTime but not");
    }
    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, DateTimeOffset value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        return writer.WriteDateTimeOffsetAsync(value);
    }
    public static async ValueTask<DateTimeOffset> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        return await reader.ReadDateTimeOffsetAsync() ?? throw new MessagePackException("Expected DateTime but not");
    }
}
