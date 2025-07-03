using System.Diagnostics;

namespace Coplt.MessagePack.Converters;

public readonly record struct EmptyObjectConverter : IMessagePackConverter<object>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, object value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        if (options.StructMode is MessagePackStructMode.AsMap) writer.WriteMapHead(0);
        else writer.WriteArrayHead(0);
    }
    public static object Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var t = reader.PeekType();
        if (t is not (MessagePackType.Array or MessagePackType.Map)) throw new MessagePackException("Expected object but not");
        if (!reader.SkipOnce()) throw new UnreachableException("Should never be false");
        return new();
    }
    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, object value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        if (options.StructMode is MessagePackStructMode.AsMap) return writer.WriteMapHeadAsync(0);
        else return writer.WriteArrayHeadAsync(0);
    }
    public static async ValueTask<object> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var t = await reader.PeekTypeAsync();
        if (t is not (MessagePackType.Array or MessagePackType.Map)) throw new MessagePackException("Expected object but not");
        if (!await reader.SkipOnceAsync()) throw new UnreachableException("Should never be false");
        return new();
    }
}
