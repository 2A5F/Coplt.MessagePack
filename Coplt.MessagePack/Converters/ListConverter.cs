namespace Coplt.MessagePack.Converters;

public readonly record struct ListConverter<T, TConverter> : IMessagePackConverter<List<T>>
    where TConverter : IMessagePackConverter<T>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, List<T> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(value.Count);
        foreach (var item in value)
        {
            TConverter.Write(ref writer, item, options);
        }
    }
    public static List<T> Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Expected array but not");
        var list = new List<T>(len);
        for (var i = 0; i < len; i++)
        {
            list.Add(TConverter.Read(ref reader, options));
        }
        return list;
    }
}

public readonly record struct AsyncListConverter<T, TConverter> : IAsyncMessagePackConverter<List<T>>
    where TConverter : IAsyncMessagePackConverter<T>
{
    public static async ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, List<T> value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(value.Count);
        foreach (var item in value)
        {
            await TConverter.WriteAsync(writer, item, options);
        }
    }
    public static async ValueTask<List<T>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Expected array but not");
        var list = new List<T>(len);
        for (var i = 0; i < len; i++)
        {
            list.Add(await TConverter.ReadAsync(reader, options));
        }
        return list;
    }
}
