namespace Coplt.MessagePack;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class MessagePackAttribute : Attribute
{
    /// <summary>
    /// When marked on Property | Field, it indicates the index of the field, similar to enum, later fields will be incremented based on this.
    /// </summary>
    public int Tag { get; set; }
}
