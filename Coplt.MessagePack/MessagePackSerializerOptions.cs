namespace Coplt.MessagePack;

public record MessagePackSerializerOptions
{
    public static MessagePackSerializerOptions Default { get; } = new();

    /// <inheritdoc cref="MessagePackStructMode"/>
    public MessagePackStructMode StructMode { get; init; } = MessagePackStructMode.Auto;
    /// <inheritdoc cref="MessagePackEnumMode"/>
    public MessagePackEnumMode EnumMode { get; init; } = MessagePackEnumMode.Auto;
}

/// <summary>
/// Controls whether the structure is serialized as an array or a map
/// </summary>
public enum MessagePackStructMode : byte
{
    /// <summary>
    /// Let the structure decide for itself
    /// </summary>
    Auto,
    /// <summary>
    /// Serialize to array
    /// </summary>
    AsArray,
    /// <summary>
    /// Serialize to map
    /// </summary>
    AsMap,
}

public enum MessagePackEnumMode : byte
{
    Auto,
    Number,
    String,
}
