namespace Coplt.MessagePack.Converters;

public readonly record struct IEnumerableConverter<T, TConverter> : IMessagePackConverter<IEnumerable<T>>
    where TConverter : IMessagePackConverter<T>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, IEnumerable<T> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        var list = value.ToList();
        ListConverter<T, TConverter>.Write(ref writer, list, options);
    }
    public static IEnumerable<T> Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        return ListConverter<T, TConverter>.Read(ref reader, options);
    }
    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, IEnumerable<T> value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        var list = value.ToList();
        return ListConverter<T, TConverter>.WriteAsync(writer, list, options);
    }
    public static async ValueTask<IEnumerable<T>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        return await ListConverter<T, TConverter>.ReadAsync(reader, options);
    }
}
