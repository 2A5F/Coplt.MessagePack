namespace Coplt.MessagePack;

public interface IMessagePackSerializable<in TSelf> where TSelf : allows ref struct
{
    public static abstract void Serialize<TTarget>(scoped ref MessagePackWriter<TTarget> writer, TSelf value, MessagePackSerializerOptions options)
        where TTarget : IWriteTarget, allows ref struct;
}

public interface IMessagePackAsyncSerializable<in TSelf>
{
    public static abstract ValueTask SerializeAsync<TTarget>(AsyncMessagePackWriter<TTarget> writer, TSelf value, MessagePackSerializerOptions options)
        where TTarget : IAsyncWriteTarget;
}

public interface IMessagePackDeserializable<out TSelf> where TSelf : allows ref struct
{
    public static abstract TSelf Deserialize<TSource>(scoped ref MessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IReadSource, allows ref struct;
}

public interface IMessagePackAsyncDeserializable<TSelf>
{
    public static abstract ValueTask<TSelf> DeserializeAsync<TSource>(AsyncMessagePackReader<TSource> reader, MessagePackSerializerOptions options)
        where TSource : IAsyncReadSource;
}
