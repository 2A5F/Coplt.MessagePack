namespace Coplt.MessagePack.Converters;

public readonly record struct StringConverter : IMessagePackConverter<string>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, string value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteString(value);
    }
    public static string Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        return reader.ReadString() ?? throw new MessagePackException("Expected string but not");
    }
    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, string value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        return writer.WriteStringAsync(value);
    }
    public static async ValueTask<string> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        return await reader.ReadStringAsync() ?? throw new MessagePackException("Expected string but not");
    }
}
