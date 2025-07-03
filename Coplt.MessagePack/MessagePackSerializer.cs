using System.Runtime.InteropServices;

namespace Coplt.MessagePack;

public readonly record struct MessagePackSerializer
{
    public static MessagePackSerializer Instance = default;

    #region TConverter

    #region Serialize

    public static List<byte> Serialize<T, TConverter>(T value, MessagePackSerializerOptions? options = null)
        where T : allows ref struct
        where TConverter : IMessagePackWritable<T>, allows ref struct
    {
        var target = new ListWriteTarget(new List<byte>());
        var writer = MessagePackWriter.Create(target);
        TConverter.Write(ref writer, value, options ?? MessagePackSerializerOptions.Default);
        return target.List;
    }

    public static void Serialize<T, TConverter>(T value, Span<byte> buffer, MessagePackSerializerOptions? options = null)
        where T : allows ref struct
        where TConverter : IMessagePackWritable<T>, allows ref struct
    {
        var target = new SpanWriteTarget(buffer);
        var writer = MessagePackWriter.Create(target);
        TConverter.Write(ref writer, value, options ?? MessagePackSerializerOptions.Default);
    }

    public static void Serialize<T, TConverter>(T value, Stream stream, MessagePackSerializerOptions? options = null, bool StreamOwner = false)
        where T : allows ref struct
        where TConverter : IMessagePackWritable<T>, allows ref struct
    {
        var target = new StreamWriteTarget(stream, StreamOwner);
        var writer = MessagePackWriter.Create(target);
        try
        {
            TConverter.Write(ref writer, value, options ?? MessagePackSerializerOptions.Default);
        }
        finally
        {
            writer.Dispose();
        }
    }

    public static async ValueTask SerializeAsync<T, TConverter>(T value, Stream stream, MessagePackSerializerOptions? options = null, bool StreamOwner = false)
        where TConverter : IAsyncMessagePackWritable<T>
    {
        var target = new AsyncStreamWriteTarget(stream, StreamOwner);
        await using var writer = MessagePackWriter.CreateAsync(target);
        await TConverter.WriteAsync(writer, value, options ?? MessagePackSerializerOptions.Default);
    }

    #endregion

    #region Deserialize

    public static T Deserialize<T, TConverter>(List<byte> bytes, MessagePackSerializerOptions? options = null)
        where T : allows ref struct
        where TConverter : IMessagePackReadable<T>, allows ref struct
        => Deserialize<T, TConverter>(CollectionsMarshal.AsSpan(bytes), options);

    public static T Deserialize<T, TConverter>(ReadOnlySpan<byte> bytes, MessagePackSerializerOptions? options = null)
        where T : allows ref struct
        where TConverter : IMessagePackReadable<T>, allows ref struct
    {
        var source = new SpanReadSource(bytes);
        var reader = MessagePackReader.Create(source);
        return TConverter.Read(ref reader, options ?? MessagePackSerializerOptions.Default);
    }

    public static T Deserialize<T, TConverter>(Stream stream, MessagePackSerializerOptions? options = null, bool StreamOwner = false)
        where T : allows ref struct
        where TConverter : IMessagePackReadable<T>, allows ref struct
    {
        var source = new StreamReadSource(stream, StreamOwner);
        var reader = MessagePackReader.Create(source);
        try
        {
            return TConverter.Read(ref reader, options ?? MessagePackSerializerOptions.Default);
        }
        finally
        {
            reader.Dispose();
        }
    }

    public static async ValueTask<T> DeserializeAsync<T, TConverter>(Stream stream, MessagePackSerializerOptions? options = null, bool StreamOwner = false)
        where TConverter : IAsyncMessagePackReadable<T>
    {
        var source = new AsyncStreamReadSource(stream, StreamOwner);
        await using var reader = MessagePackReader.CreateAsync(source);
        return await TConverter.ReadAsync(reader, options ?? MessagePackSerializerOptions.Default);
    }

    #endregion

    #endregion

    #region T

    #region Serialize

    public List<byte> Serialize<T>(T value, MessagePackSerializerOptions? options = null)
        where T : IMessagePackSerializable<T>, allows ref struct
    {
        var target = new ListWriteTarget(new List<byte>());
        var writer = MessagePackWriter.Create(target);
        T.Serialize(ref writer, value, options ?? MessagePackSerializerOptions.Default);
        return target.List;
    }

    public void Serialize<T>(T value, Span<byte> buffer, MessagePackSerializerOptions? options = null)
        where T : IMessagePackSerializable<T>, allows ref struct
    {
        var target = new SpanWriteTarget(buffer);
        var writer = MessagePackWriter.Create(target);
        T.Serialize(ref writer, value, options ?? MessagePackSerializerOptions.Default);
    }

    public void Serialize<T>(T value, Stream stream, MessagePackSerializerOptions? options = null, bool StreamOwner = false)
        where T : IMessagePackSerializable<T>, allows ref struct
    {
        var target = new StreamWriteTarget(stream, StreamOwner);
        var writer = MessagePackWriter.Create(target);
        try
        {
            T.Serialize(ref writer, value, options ?? MessagePackSerializerOptions.Default);
        }
        finally
        {
            writer.Dispose();
        }
    }

    public async ValueTask SerializeAsync<T>(T value, Stream stream, MessagePackSerializerOptions? options = null, bool StreamOwner = false)
        where T : IMessagePackAsyncSerializable<T>
    {
        var target = new AsyncStreamWriteTarget(stream, StreamOwner);
        await using var writer = MessagePackWriter.CreateAsync(target);
        await T.SerializeAsync(writer, value, options ?? MessagePackSerializerOptions.Default);
    }

    #endregion

    #region Deserialize

    public T Deserialize<T>(List<byte> bytes, MessagePackSerializerOptions? options = null)
        where T : IMessagePackDeserializable<T>, allows ref struct
        => Deserialize<T>(CollectionsMarshal.AsSpan(bytes), options);

    public T Deserialize<T>(ReadOnlySpan<byte> bytes, MessagePackSerializerOptions? options = null)
        where T : IMessagePackDeserializable<T>, allows ref struct
    {
        var source = new SpanReadSource(bytes);
        var reader = MessagePackReader.Create(source);
        return T.Deserialize(ref reader, options ?? MessagePackSerializerOptions.Default);
    }

    public T Deserialize<T>(Stream stream, MessagePackSerializerOptions? options = null, bool StreamOwner = false)
        where T : IMessagePackDeserializable<T>, allows ref struct
    {
        var source = new StreamReadSource(stream, StreamOwner);
        var reader = MessagePackReader.Create(source);
        try
        {
            return T.Deserialize(ref reader, options ?? MessagePackSerializerOptions.Default);
        }
        finally
        {
            reader.Dispose();
        }
    }

    public async ValueTask<T> DeserializeAsync<T>(Stream stream, MessagePackSerializerOptions? options = null, bool StreamOwner = false)
        where T : IMessagePackAsyncDeserializable<T>
    {
        var source = new AsyncStreamReadSource(stream, StreamOwner);
        await using var reader = MessagePackReader.CreateAsync(source);
        return await T.DeserializeAsync(reader, options ?? MessagePackSerializerOptions.Default);
    }

    #endregion

    #endregion
}
