using System.Collections.Concurrent;

namespace Coplt.MessagePack.Converters;

public readonly record struct ConcurrentDictionaryConverter<TKey, TValue, TKeyConverter, TValueConverter>
    : IMessagePackConverter<ConcurrentDictionary<TKey, TValue>>
    where TKey : notnull
    where TKeyConverter : IMessagePackConverter<TKey>
    where TValueConverter : IMessagePackConverter<TValue>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, ConcurrentDictionary<TKey, TValue> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteMapHead(value.Count);
        foreach (var (k, v) in value)
        {
            TKeyConverter.Write(ref writer, k, options);
            TValueConverter.Write(ref writer, v, options);
        }
    }
    public static ConcurrentDictionary<TKey, TValue> Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadMapHead() ?? throw new MessagePackException("Expected map but not");
        var dict = new ConcurrentDictionary<TKey, TValue>();
        for (var i = 0; i < len; i++)
        {
            var k = TKeyConverter.Read(ref reader, options);
            var v = TValueConverter.Read(ref reader, options);
            dict.TryAdd(k, v);
        }
        return dict;
    }

    public static async ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, ConcurrentDictionary<TKey, TValue> value,
        MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        await writer.WriteMapHeadAsync(value.Count);
        foreach (var (k, v) in value)
        {
            await TKeyConverter.WriteAsync(writer, k, options);
            await TValueConverter.WriteAsync(writer, v, options);
        }
    }
    public static async ValueTask<ConcurrentDictionary<TKey, TValue>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader,
        MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var len = await reader.ReadMapHeadAsync() ?? throw new MessagePackException("Expected map but not");
        var dict = new ConcurrentDictionary<TKey, TValue>();
        for (var i = 0; i < len; i++)
        {
            var k = await TKeyConverter.ReadAsync(reader, options);
            var v = await TValueConverter.ReadAsync(reader, options);
            dict.TryAdd(k, v);
        }
        return dict;
    }
}
