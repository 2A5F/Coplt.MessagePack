namespace Coplt.MessagePack.Converters;

public readonly record struct EnumStringConverter<TEnum> : IMessagePackConverter<TEnum>
    where TEnum : struct, Enum
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, TEnum value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        var name = Enum.GetName(value);
        name ??= $"{value}";
        writer.WriteString(name);
    }
    public static TEnum Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var t = reader.PeekType();
        if (t is MessagePackType.Nil)
        {
            reader.ReadNull();
            return default;
        }
        var r = reader.ReadString(static name => Enum.Parse<TEnum>(name))
                ?? throw new MessagePackException("Expected string but not");
        return r.Value;
    }
    public static async ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, TEnum value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        var name = Enum.GetName(value);
        name ??= $"{value}";
        await writer.WriteStringAsync(name);
    }
    public static async ValueTask<TEnum> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        if (await reader.PeekTypeAsync() is MessagePackType.Nil)
        {
            await reader.ReadNullAsync();
            return default;
        }
        var r = await reader.ReadStringAsync(static name => ValueTask.FromResult(Enum.Parse<TEnum>(name.Span)))
                ?? throw new MessagePackException("Expected string but not");
        return r.Value;
    }
}
