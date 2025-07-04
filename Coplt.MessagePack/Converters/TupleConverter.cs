namespace Coplt.MessagePack.Converters;

public readonly record struct TupleConverter<T, TConverter> : ITupleConverter<Tuple<T>>
    where TConverter : IMessagePackConverter<T>
{
    public static int Arity => 1;
    public static void RestWrite<TTarget>(scoped ref MessagePackWriter<TTarget> writer, Tuple<T> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        TConverter.Write(ref writer, value.Item1, options);
    }
    public static Tuple<T> RestRead<TSource>(scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var item0 = TConverter.Read(ref reader, options);
        return new(item0);
    }
    public static ValueTask RestWriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, Tuple<T> value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        return TConverter.WriteAsync(writer, value.Item1, options);
    }
    public static async ValueTask<Tuple<T>> RestReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var item0 = await TConverter.ReadAsync(reader, options);
        return new(item0);
    }
    public static void Write<TTarget>(scoped ref MessagePackWriter<TTarget> writer, Tuple<T> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(Arity);
        RestWrite(ref writer, value, options);
    }
    public static Tuple<T> Read<TSource>(scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return RestRead(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, Tuple<T> value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(Arity);
        await RestWriteAsync(writer, value, options);
    }
    public static async ValueTask<Tuple<T>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return await RestReadAsync(reader, options);
    }
}

public readonly record struct TupleConverter<T0, T1, TConverter0, TConverter1> : ITupleConverter<Tuple<T0, T1>>
    where TConverter0 : IMessagePackConverter<T0>
    where TConverter1 : IMessagePackConverter<T1>
{
    public static int Arity => 2;
    public static void RestWrite<TTarget>(scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        TConverter0.Write(ref writer, value.Item1, options);
        TConverter1.Write(ref writer, value.Item2, options);
    }
    public static Tuple<T0, T1> RestRead<TSource>(scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var item0 = TConverter0.Read(ref reader, options);
        var item1 = TConverter1.Read(ref reader, options);
        return new(item0, item1);
    }
    public static async ValueTask RestWriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await TConverter0.WriteAsync(writer, value.Item1, options);
        await TConverter1.WriteAsync(writer, value.Item2, options);
    }
    public static async ValueTask<Tuple<T0, T1>> RestReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        T0 item0 = await TConverter0.ReadAsync(reader, options);
        T1 item1 = await TConverter1.ReadAsync(reader, options);
        return new(item0, item1);
    }

    public static void Write<TTarget>(scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(Arity);
        RestWrite(ref writer, value, options);
    }
    public static Tuple<T0, T1> Read<TSource>(scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return RestRead(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1> value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(Arity);
        await RestWriteAsync(writer, value, options);
    }
    public static async ValueTask<Tuple<T0, T1>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return await RestReadAsync(reader, options);
    }
}

public readonly record struct TupleConverter<
    T0, T1, T2,
    TConverter0, TConverter1, TConverter2
> : ITupleConverter<Tuple<T0, T1, T2>>
    where TConverter0 : IMessagePackConverter<T0>
    where TConverter1 : IMessagePackConverter<T1>
    where TConverter2 : IMessagePackConverter<T2>
{
    public static int Arity => 3;
    public static void RestWrite<TTarget>(scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        TConverter0.Write(ref writer, value.Item1, options);
        TConverter1.Write(ref writer, value.Item2, options);
        TConverter2.Write(ref writer, value.Item3, options);
    }
    public static Tuple<T0, T1, T2> RestRead<TSource>(scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var item0 = TConverter0.Read(ref reader, options);
        var item1 = TConverter1.Read(ref reader, options);
        var item2 = TConverter2.Read(ref reader, options);
        return new(item0, item1, item2);
    }
    public static async ValueTask RestWriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await TConverter0.WriteAsync(writer, value.Item1, options);
        await TConverter1.WriteAsync(writer, value.Item2, options);
        await TConverter2.WriteAsync(writer, value.Item3, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2>> RestReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var item0 = await TConverter0.ReadAsync(reader, options);
        var item1 = await TConverter1.ReadAsync(reader, options);
        var item2 = await TConverter2.ReadAsync(reader, options);
        return new(item0, item1, item2);
    }

    public static void Write<TTarget>(scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2> value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(Arity);
        RestWrite(ref writer, value, options);
    }
    public static Tuple<T0, T1, T2> Read<TSource>(scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return RestRead(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(Arity);
        await RestWriteAsync(writer, value, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2>> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return await RestReadAsync(reader, options);
    }
}

public readonly record struct TupleConverter<
    T0, T1, T2, T3,
    TConverter0, TConverter1, TConverter2, TConverter3
> : ITupleConverter<Tuple<T0, T1, T2, T3>>
    where TConverter0 : IMessagePackConverter<T0>
    where TConverter1 : IMessagePackConverter<T1>
    where TConverter2 : IMessagePackConverter<T2>
    where TConverter3 : IMessagePackConverter<T3>
{
    public static int Arity => 4;
    public static void RestWrite<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        TConverter0.Write(ref writer, value.Item1, options);
        TConverter1.Write(ref writer, value.Item2, options);
        TConverter2.Write(ref writer, value.Item3, options);
        TConverter3.Write(ref writer, value.Item4, options);
    }
    public static Tuple<T0, T1, T2, T3> RestRead<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var item0 = TConverter0.Read(ref reader, options);
        var item1 = TConverter1.Read(ref reader, options);
        var item2 = TConverter2.Read(ref reader, options);
        var item3 = TConverter3.Read(ref reader, options);
        return new(item0, item1, item2, item3);
    }
    public static async ValueTask RestWriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await TConverter0.WriteAsync(writer, value.Item1, options);
        await TConverter1.WriteAsync(writer, value.Item2, options);
        await TConverter2.WriteAsync(writer, value.Item3, options);
        await TConverter3.WriteAsync(writer, value.Item4, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3>> RestReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var item0 = await TConverter0.ReadAsync(reader, options);
        var item1 = await TConverter1.ReadAsync(reader, options);
        var item2 = await TConverter2.ReadAsync(reader, options);
        var item3 = await TConverter3.ReadAsync(reader, options);
        return new(item0, item1, item2, item3);
    }

    public static void Write<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(Arity);
        RestWrite(ref writer, value, options);
    }
    public static Tuple<T0, T1, T2, T3> Read<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return RestRead(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(Arity);
        await RestWriteAsync(writer, value, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3>> ReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return await RestReadAsync(reader, options);
    }
}

public readonly record struct TupleConverter<
    T0, T1, T2, T3, T4,
    TConverter0, TConverter1, TConverter2, TConverter3, TConverter4
> : ITupleConverter<Tuple<T0, T1, T2, T3, T4>>
    where TConverter0 : IMessagePackConverter<T0>
    where TConverter1 : IMessagePackConverter<T1>
    where TConverter2 : IMessagePackConverter<T2>
    where TConverter3 : IMessagePackConverter<T3>
    where TConverter4 : IMessagePackConverter<T4>
{
    public static int Arity => 5;
    public static void RestWrite<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        TConverter0.Write(ref writer, value.Item1, options);
        TConverter1.Write(ref writer, value.Item2, options);
        TConverter2.Write(ref writer, value.Item3, options);
        TConverter3.Write(ref writer, value.Item4, options);
        TConverter4.Write(ref writer, value.Item5, options);
    }
    public static Tuple<T0, T1, T2, T3, T4> RestRead<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var item0 = TConverter0.Read(ref reader, options);
        var item1 = TConverter1.Read(ref reader, options);
        var item2 = TConverter2.Read(ref reader, options);
        var item3 = TConverter3.Read(ref reader, options);
        var item4 = TConverter4.Read(ref reader, options);
        return new(item0, item1, item2, item3, item4);
    }
    public static async ValueTask RestWriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await TConverter0.WriteAsync(writer, value.Item1, options);
        await TConverter1.WriteAsync(writer, value.Item2, options);
        await TConverter2.WriteAsync(writer, value.Item3, options);
        await TConverter3.WriteAsync(writer, value.Item4, options);
        await TConverter4.WriteAsync(writer, value.Item5, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3, T4>> RestReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var item0 = await TConverter0.ReadAsync(reader, options);
        var item1 = await TConverter1.ReadAsync(reader, options);
        var item2 = await TConverter2.ReadAsync(reader, options);
        var item3 = await TConverter3.ReadAsync(reader, options);
        var item4 = await TConverter4.ReadAsync(reader, options);
        return new(item0, item1, item2, item3, item4);
    }

    public static void Write<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(Arity);
        RestWrite(ref writer, value, options);
    }
    public static Tuple<T0, T1, T2, T3, T4> Read<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return RestRead(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(Arity);
        await RestWriteAsync(writer, value, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3, T4>> ReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return await RestReadAsync(reader, options);
    }
}

public readonly record struct TupleConverter<
    T0, T1, T2, T3, T4, T5,
    TConverter0, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5
> : ITupleConverter<Tuple<T0, T1, T2, T3, T4, T5>>
    where TConverter0 : IMessagePackConverter<T0>
    where TConverter1 : IMessagePackConverter<T1>
    where TConverter2 : IMessagePackConverter<T2>
    where TConverter3 : IMessagePackConverter<T3>
    where TConverter4 : IMessagePackConverter<T4>
    where TConverter5 : IMessagePackConverter<T5>
{
    public static int Arity => 6;
    public static void RestWrite<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        TConverter0.Write(ref writer, value.Item1, options);
        TConverter1.Write(ref writer, value.Item2, options);
        TConverter2.Write(ref writer, value.Item3, options);
        TConverter3.Write(ref writer, value.Item4, options);
        TConverter4.Write(ref writer, value.Item5, options);
        TConverter5.Write(ref writer, value.Item6, options);
    }
    public static Tuple<T0, T1, T2, T3, T4, T5> RestRead<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var item0 = TConverter0.Read(ref reader, options);
        var item1 = TConverter1.Read(ref reader, options);
        var item2 = TConverter2.Read(ref reader, options);
        var item3 = TConverter3.Read(ref reader, options);
        var item4 = TConverter4.Read(ref reader, options);
        var item5 = TConverter5.Read(ref reader, options);
        return new(item0, item1, item2, item3, item4, item5);
    }
    public static async ValueTask RestWriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await TConverter0.WriteAsync(writer, value.Item1, options);
        await TConverter1.WriteAsync(writer, value.Item2, options);
        await TConverter2.WriteAsync(writer, value.Item3, options);
        await TConverter3.WriteAsync(writer, value.Item4, options);
        await TConverter4.WriteAsync(writer, value.Item5, options);
        await TConverter5.WriteAsync(writer, value.Item6, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3, T4, T5>> RestReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var item0 = await TConverter0.ReadAsync(reader, options);
        var item1 = await TConverter1.ReadAsync(reader, options);
        var item2 = await TConverter2.ReadAsync(reader, options);
        var item3 = await TConverter3.ReadAsync(reader, options);
        var item4 = await TConverter4.ReadAsync(reader, options);
        var item5 = await TConverter5.ReadAsync(reader, options);
        return new(item0, item1, item2, item3, item4, item5);
    }

    public static void Write<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(Arity);
        RestWrite(ref writer, value, options);
    }
    public static Tuple<T0, T1, T2, T3, T4, T5> Read<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return RestRead(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(Arity);
        await RestWriteAsync(writer, value, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3, T4, T5>> ReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return await RestReadAsync(reader, options);
    }
}

public readonly record struct TupleConverter<
    T0, T1, T2, T3, T4, T5, T6,
    TConverter0, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6
> : ITupleConverter<Tuple<T0, T1, T2, T3, T4, T5, T6>>
    where TConverter0 : IMessagePackConverter<T0>
    where TConverter1 : IMessagePackConverter<T1>
    where TConverter2 : IMessagePackConverter<T2>
    where TConverter3 : IMessagePackConverter<T3>
    where TConverter4 : IMessagePackConverter<T4>
    where TConverter5 : IMessagePackConverter<T5>
    where TConverter6 : IMessagePackConverter<T6>
{
    public static int Arity => 7;
    public static void RestWrite<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5, T6> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        TConverter0.Write(ref writer, value.Item1, options);
        TConverter1.Write(ref writer, value.Item2, options);
        TConverter2.Write(ref writer, value.Item3, options);
        TConverter3.Write(ref writer, value.Item4, options);
        TConverter4.Write(ref writer, value.Item5, options);
        TConverter5.Write(ref writer, value.Item6, options);
        TConverter6.Write(ref writer, value.Item7, options);
    }
    public static Tuple<T0, T1, T2, T3, T4, T5, T6> RestRead<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var item0 = TConverter0.Read(ref reader, options);
        var item1 = TConverter1.Read(ref reader, options);
        var item2 = TConverter2.Read(ref reader, options);
        var item3 = TConverter3.Read(ref reader, options);
        var item4 = TConverter4.Read(ref reader, options);
        var item5 = TConverter5.Read(ref reader, options);
        var item6 = TConverter6.Read(ref reader, options);
        return new(item0, item1, item2, item3, item4, item5, item6);
    }
    public static async ValueTask RestWriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5, T6> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await TConverter0.WriteAsync(writer, value.Item1, options);
        await TConverter1.WriteAsync(writer, value.Item2, options);
        await TConverter2.WriteAsync(writer, value.Item3, options);
        await TConverter3.WriteAsync(writer, value.Item4, options);
        await TConverter4.WriteAsync(writer, value.Item5, options);
        await TConverter5.WriteAsync(writer, value.Item6, options);
        await TConverter6.WriteAsync(writer, value.Item7, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3, T4, T5, T6>> RestReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var item0 = await TConverter0.ReadAsync(reader, options);
        var item1 = await TConverter1.ReadAsync(reader, options);
        var item2 = await TConverter2.ReadAsync(reader, options);
        var item3 = await TConverter3.ReadAsync(reader, options);
        var item4 = await TConverter4.ReadAsync(reader, options);
        var item5 = await TConverter5.ReadAsync(reader, options);
        var item6 = await TConverter6.ReadAsync(reader, options);
        return new(item0, item1, item2, item3, item4, item5, item6);
    }

    public static void Write<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5, T6> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(Arity);
        RestWrite(ref writer, value, options);
    }
    public static Tuple<T0, T1, T2, T3, T4, T5, T6> Read<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return RestRead(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5, T6> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(Arity);
        await RestWriteAsync(writer, value, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3, T4, T5, T6>> ReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return await RestReadAsync(reader, options);
    }
}

public readonly record struct TupleConverter<
    T0, T1, T2, T3, T4, T5, T6, TRest,
    TConverter0, TConverter1, TConverter2, TConverter3, TConverter4, TConverter5, TConverter6, TConverterRest
> : ITupleConverter<Tuple<T0, T1, T2, T3, T4, T5, T6, TRest>>
    where TRest : notnull
    where TConverter0 : IMessagePackConverter<T0>
    where TConverter1 : IMessagePackConverter<T1>
    where TConverter2 : IMessagePackConverter<T2>
    where TConverter3 : IMessagePackConverter<T3>
    where TConverter4 : IMessagePackConverter<T4>
    where TConverter5 : IMessagePackConverter<T5>
    where TConverter6 : IMessagePackConverter<T6>
    where TConverterRest : ITupleConverter<TRest>
{
    // ReSharper disable once StaticMemberInGenericType
    public static int Arity { get; } = 7 + TConverterRest.Arity;
    public static void RestWrite<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5, T6, TRest> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        TConverter0.Write(ref writer, value.Item1, options);
        TConverter1.Write(ref writer, value.Item2, options);
        TConverter2.Write(ref writer, value.Item3, options);
        TConverter3.Write(ref writer, value.Item4, options);
        TConverter4.Write(ref writer, value.Item5, options);
        TConverter5.Write(ref writer, value.Item6, options);
        TConverter6.Write(ref writer, value.Item7, options);
        TConverterRest.Write(ref writer, value.Rest, options);
    }
    public static Tuple<T0, T1, T2, T3, T4, T5, T6, TRest> RestRead<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var item0 = TConverter0.Read(ref reader, options);
        var item1 = TConverter1.Read(ref reader, options);
        var item2 = TConverter2.Read(ref reader, options);
        var item3 = TConverter3.Read(ref reader, options);
        var item4 = TConverter4.Read(ref reader, options);
        var item5 = TConverter5.Read(ref reader, options);
        var item6 = TConverter6.Read(ref reader, options);
        var rest = TConverterRest.Read(ref reader, options);
        return new(item0, item1, item2, item3, item4, item5, item6, rest);
    }
    public static async ValueTask RestWriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5, T6, TRest> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await TConverter0.WriteAsync(writer, value.Item1, options);
        await TConverter1.WriteAsync(writer, value.Item2, options);
        await TConverter2.WriteAsync(writer, value.Item3, options);
        await TConverter3.WriteAsync(writer, value.Item4, options);
        await TConverter4.WriteAsync(writer, value.Item5, options);
        await TConverter5.WriteAsync(writer, value.Item6, options);
        await TConverter6.WriteAsync(writer, value.Item7, options);
        await TConverterRest.WriteAsync(writer, value.Rest, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3, T4, T5, T6, TRest>> RestReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var item0 = await TConverter0.ReadAsync(reader, options);
        var item1 = await TConverter1.ReadAsync(reader, options);
        var item2 = await TConverter2.ReadAsync(reader, options);
        var item3 = await TConverter3.ReadAsync(reader, options);
        var item4 = await TConverter4.ReadAsync(reader, options);
        var item5 = await TConverter5.ReadAsync(reader, options);
        var item6 = await TConverter6.ReadAsync(reader, options);
        var rest = await TConverterRest.ReadAsync(reader, options);
        return new(item0, item1, item2, item3, item4, item5, item6, rest);
    }

    public static void Write<TTarget>(
        scoped ref MessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5, T6, TRest> value, MessagePackSerializerOptions options
    ) where TTarget : IWriteTarget, allows ref struct
    {
        writer.WriteArrayHead(Arity);
        RestWrite(ref writer, value, options);
    }
    public static Tuple<T0, T1, T2, T3, T4, T5, T6, TRest> Read<TSource>(
        scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IReadSource, allows ref struct
    {
        var len = reader.ReadArrayHead() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return RestRead(ref reader, options);
    }
    public static async ValueTask WriteAsync<TTarget>(
        AsyncMessagePackWriter<TTarget> writer, Tuple<T0, T1, T2, T3, T4, T5, T6, TRest> value, MessagePackSerializerOptions options
    ) where TTarget : IAsyncWriteTarget
    {
        await writer.WriteArrayHeadAsync(Arity);
        await RestWriteAsync(writer, value, options);
    }
    public static async ValueTask<Tuple<T0, T1, T2, T3, T4, T5, T6, TRest>> ReadAsync<TSource>(
        AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options
    ) where TSource : IAsyncReadSource
    {
        var len = await reader.ReadArrayHeadAsync() ?? throw new MessagePackException("Excepted array but not");
        if (len != Arity) throw new MessagePackException($"Excepted {Arity} tuple but got {len} tuple");
        return await RestReadAsync(reader, options);
    }
}
