namespace Coplt.MessagePack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class MessagePackAttribute : Attribute
{
    /// <summary>
    /// Serialize to an array without retaining field names. Set to false to serialize to a map, similar to a json object.<br/>
    /// <para>Valid use: <see cref="AttributeTargets.Class"/> | <see cref="AttributeTargets.Struct"/></para>
    /// </summary>
    public bool AsArray { get; set; } = true;
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
    public bool Skip { get; set; }

    public MessagePackAttribute() { }
    /// <inheritdoc cref="Index"/>
    public MessagePackAttribute(int Index)
    {
        this.Index = Index;
    }
}
