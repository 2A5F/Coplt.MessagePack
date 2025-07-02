namespace Coplt.MessagePack;

public class InvalidMessagePackDataException : Exception
{
    public InvalidMessagePackDataException() { }
    public InvalidMessagePackDataException(string message) : base(message) { }
    public InvalidMessagePackDataException(string message, Exception inner) : base(message, inner) { }
}
