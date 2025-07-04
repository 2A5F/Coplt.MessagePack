namespace Coplt.MessagePack.Converters;

public interface ITupleConverter<T> : IMessagePackConverter<T>
{
    public static abstract int Arity { get; }
    
    public static abstract void RestWrite<TTarget>(scoped ref MessagePackWriter<TTarget> writer, T value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct;
    public static abstract T RestRead<TSource>(scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct;
    
    public static abstract ValueTask RestWriteAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, T value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget;
    public static abstract ValueTask<T> RestReadAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource;
}
