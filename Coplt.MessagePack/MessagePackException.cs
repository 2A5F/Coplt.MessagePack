namespace Coplt.MessagePack;

public class MessagePackException : Exception
{
    public MessagePackException() { }
    public MessagePackException(string message) : base(message) { }
    public MessagePackException(string message, Exception inner) : base(message, inner) { }
}
