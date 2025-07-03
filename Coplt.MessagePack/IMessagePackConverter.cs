namespace Coplt.MessagePack;

public interface IMessagePackWritable<in T> where T : allows ref struct
{
    public static abstract void Write<TTarget>(ref MessagePackWriter<TTarget> writer, T value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct;
}

public interface IAsyncMessagePackWritable<in T>
{
    public static abstract ValueTask WriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, T value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget;
}

public interface IMessagePackReadable<out T> where T : allows ref struct
{
    public static abstract T Read<TSource>(ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct;
}

public interface IAsyncMessagePackReadable<T>
{
    public static abstract ValueTask<T> ReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource;
}

public interface ISyncMessagePackConverter<T> : IMessagePackWritable<T>, IMessagePackReadable<T>
    where T : allows ref struct;

public interface IAsyncMessagePackConverter<T> : IAsyncMessagePackWritable<T>, IAsyncMessagePackReadable<T>;

public interface IMessagePackConverter<T> : ISyncMessagePackConverter<T>, IAsyncMessagePackConverter<T>;
