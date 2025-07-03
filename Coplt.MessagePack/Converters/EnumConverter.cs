namespace Coplt.MessagePack.Converters;

public readonly record struct EnumConverter<TEnum, TUnderlying, TConverter> : IMessagePackConverter<TEnum>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
    where TConverter : IMessagePackConverter<TUnderlying>
{
    public static void Write<TTarget>(ref MessagePackWriter<TTarget> writer, TEnum value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        if (options.EnumMode is MessagePackEnumMode.String)
        {
            EnumStringConverter<TEnum>.Write(ref writer, value, options);
            return;
        }
        var underlying = (TUnderlying)(object)value;
        TConverter.Write(ref writer, underlying, options);
    }
    public static TEnum Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        if (options.EnumMode is MessagePackEnumMode.String)
        {
            return EnumStringConverter<TEnum>.Read(ref reader, options);
        }
        var underlying = TConverter.Read(ref reader, options);
        return (TEnum)(object)underlying;
    }

    public static ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, TEnum value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        if (options.EnumMode is MessagePackEnumMode.String)
        {
            return EnumStringConverter<TEnum>.WriteAsync(writer, value, options);
        }
        var underlying = (TUnderlying)(object)value;
        return TConverter.WriteAsync(writer, underlying, options);
    }
    public static async ValueTask<TEnum> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        if (options.EnumMode is MessagePackEnumMode.String)
        {
            return await EnumStringConverter<TEnum>.ReadAsync(reader, options);
        }
        var underlying = await TConverter.ReadAsync(reader, options);
        return (TEnum)(object)underlying;
    }
}
