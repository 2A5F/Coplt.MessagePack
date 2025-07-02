namespace Coplt.MessagePack.Converters;

public readonly record struct ArrayConverter<T, TConverter> : IMessagePackConverter<T[]>
    where TConverter : IMessagePackConverter<T>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, T[] value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(value.Length);
        foreach (var item in value)
        {
            TConverter.Write(ref writer, item, options);
        }
    }
    public static T[] Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Expected array but not");
        var array = GC.AllocateUninitializedArray<T>(len);
        for (var i = 0; i < len; i++)
        {
            array[i] = TConverter.Read(ref reader, options);
        }
        return array;
    }
}

public readonly record struct AsyncArrayConverter<T, TConverter> : IAsyncMessagePackConverter<T[]>
    where TConverter : IAsyncMessagePackConverter<T>
{
    public static async ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, T[] value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(value.Length);
        foreach (var item in value)
        {
            await TConverter.WriteAsync(writer, item, options);
        }
    }
    public static async ValueTask<T[]> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Expected array but not");
        var array = GC.AllocateUninitializedArray<T>(len);
        for (var i = 0; i < len; i++)
        {
            array[i] = await TConverter.ReadAsync(reader, options);
        }
        return array;
    }
}
