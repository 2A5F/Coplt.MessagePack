namespace Coplt.MessagePack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class MessagePackAttribute : Attribute
{
    /// <summary>
    /// Config generated converter type name. {0} is TypeName
    /// <para>Valid use: <see cref="AttributeTargets.Class"/> | <see cref="AttributeTargets.Struct"/></para>
    /// </summary>
    public string ConverterName { get; set; } = "MessagePackConverter";
    /// <summary>
    /// Serialize to an array without retaining field names. Set to false to serialize to a map, similar to a json object.<br/>
    /// <para>Valid use: <see cref="AttributeTargets.Class"/> | <see cref="AttributeTargets.Struct"/></para>
    /// </summary>
    public bool AsArray { get; set; } = true;
    /// <summary>
    /// <para>Valid use: <see cref="AttributeTargets.Class"/> | <see cref="AttributeTargets.Struct"/></para>
    /// </summary>
    public bool IncludeField { get; set; } = false;
    /// <summary>
    /// <para>Valid use: <see cref="AttributeTargets.Class"/> | <see cref="AttributeTargets.Struct"/></para>
    /// </summary>
    public bool IncludePrivate { get; set; } = false;
    /// <summary>
    /// The index of the field, similar to enum, later fields will be incremented based on this.<br/>
    /// Less than 1 means auto increment;<br/>
    /// <para>Valid use: <see cref="AttributeTargets.Property"/> | <see cref="AttributeTargets.Field"/></para>
    /// </summary>
    public int Index { get; set; } = -1;
    /// <summary>
    /// Do not de/serialize this field.<br/>
    /// <para>Valid use: <see cref="AttributeTargets.Property"/> | <see cref="AttributeTargets.Field"/></para>
    /// </summary>
    public bool Skip { get; set; } = false;
    /// <summary>
    /// Manually specifying the converter type.<br/>
    /// <para>Valid use: <see cref="AttributeTargets.Property"/> | <see cref="AttributeTargets.Field"/></para>
    /// </summary>
    public Type? Converter { get; set; } = null!;
    /// <summary>
    /// Serialize to binary instead of array if type is <see cref="T:byte[]"/> or <see cref="T:List{byte}"/><br/>
    /// <para>Valid use: <see cref="AttributeTargets.Property"/> | <see cref="AttributeTargets.Field"/></para>
    /// </summary>
    public bool AsBytes { get; set; } = true;

    public MessagePackAttribute() { }
    /// <inheritdoc cref="Index"/>
    public MessagePackAttribute(int Index)
    {
        this.Index = Index;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class MessagePackConverterSourceAttribute : Attribute;
