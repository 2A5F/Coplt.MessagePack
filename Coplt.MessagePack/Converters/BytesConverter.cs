using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Coplt.MessagePack.Converters;

public readonly record struct BytesArrayConverter : IMessagePackConverter<byte[]>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, byte[] value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteBytes(value);
    }
    public static byte[] Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        return reader.ReadBytesArray() ?? throw new MessagePackException("Expected string but not");
    }
    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, byte[] value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        return writer.WriteBytesAsync(value);
    }
    public static async ValueTask<byte[]> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        return await reader.ReadBytesArrayAsync() ?? throw new MessagePackException("Expected string but not");
    }
}

public readonly record struct BytesListConverter : IMessagePackConverter<List<byte>>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, List<byte> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteBytes(CollectionsMarshal.AsSpan(value));
    }
    public static List<byte> Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var r = reader.ReadBytes(static span =>
        {
            var list = new List<byte>(span.Length);
            CollectionsMarshal.SetCount(list, span.Length);
            span.CopyTo(CollectionsMarshal.AsSpan(list));
            return list;
        }) ?? throw new MessagePackException("Expected string but not");
        return r.Value;
    }
    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, List<byte> value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        var arr = GetListItems(value);
        return writer.WriteBytesAsync(arr.AsMemory(0, value.Count));

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
        static extern ref byte[] GetListItems(List<byte> list);
    }
    public static async ValueTask<List<byte>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var r = await reader.ReadBytesAsync(static memory =>
        {
            var list = new List<byte>(memory.Length);
            CollectionsMarshal.SetCount(list, memory.Length);
            memory.Span.CopyTo(CollectionsMarshal.AsSpan(list));
            return ValueTask.FromResult(list);
        }) ?? throw new MessagePackException("Expected string but not");
        return r.Value;
    }
}
