namespace Coplt.MessagePack;

public class OutOfCapacityException : Exception
{
    public OutOfCapacityException() { }
    public OutOfCapacityException(string message) : base(message) { }
    public OutOfCapacityException(string message, Exception inner) : base(message, inner) { }
}
