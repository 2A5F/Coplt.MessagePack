namespace Coplt.MessagePack;

public class InvalidMessagePackFormatException : Exception
{
    public InvalidMessagePackFormatException() { }
    public InvalidMessagePackFormatException(string message) : base(message) { }
    public InvalidMessagePackFormatException(string message, Exception inner) : base(message, inner) { }
}
