namespace Coplt.MessagePack.Converters;

public readonly record struct ICollectionConverter<T, TConverter> : IMessagePackConverter<ICollection<T>>
    where TConverter : IMessagePackConverter<T>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, ICollection<T> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(value.Count);
        foreach (var item in value)
        {
            TConverter.Write(ref writer, item, options);
        }
    }
    public static ICollection<T> Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        return ListConverter<T, TConverter>.Read(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, ICollection<T> value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(value.Count);
        foreach (var item in value)
        {
            await TConverter.WriteAsync(writer, item, options);
        }
    }
    public static async ValueTask<ICollection<T>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        return await ListConverter<T, TConverter>.ReadAsync(reader, options);
    }
}
