using System.Diagnostics;

namespace Coplt.MessagePack.Converters;

public readonly record struct NullableClassConverter<T, TConverter> : IMessagePackConverter<T?>
    where T : class
    where TConverter : IMessagePackConverter<T>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, T? value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        if (value is { } v) TConverter.Write(ref writer, v, options);
        else writer.WriteNull();
    }
    public static T? Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var t = reader.PeekType();
        if (t is MessagePackType.Nil)
        {
            if (!reader.ReadNull()) throw new UnreachableException("Should never be false");
            return null;
        }
        return TConverter.Read(ref reader, options);
    }
    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, T? value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        if (value is { } v) return TConverter.WriteAsync(writer, v, options);
        else return writer.WriteNullAsync();
    }
    public static async ValueTask<T?> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var t = await reader.PeekTypeAsync();
        if (t is MessagePackType.Nil)
        {
            if (!await reader.ReadNullAsync()) throw new UnreachableException("Should never be false");
            return null;
        }
        return await TConverter.ReadAsync(reader, options);
    }
}
