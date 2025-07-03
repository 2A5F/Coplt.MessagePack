namespace Coplt.MessagePack.Converters;

public readonly record struct IListConverter<T, TConverter> : IMessagePackConverter<IList<T>>
    where TConverter : IMessagePackConverter<T>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, IList<T> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(value.Count);
        foreach (var item in value)
        {
            TConverter.Write(ref writer, item, options);
        }
    }
    public static IList<T> Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        return ListConverter<T, TConverter>.Read(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, IList<T> value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(value.Count);
        foreach (var item in value)
        {
            await TConverter.WriteAsync(writer, item, options);
        }
    }
    public static async ValueTask<IList<T>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        return await ListConverter<T, TConverter>.ReadAsync(reader, options);
    }
}
