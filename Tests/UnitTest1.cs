using Coplt.MessagePack;

namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void TestString1()
    {
        var buf = new ListWriteTarget(new List<byte>());
        var writer = MessagePackWriter.Create(buf);
        writer.WriteString("asd");
        foreach (var b in buf.List)
        {
            Console.Write($"{b:X} ");
        }
        Console.WriteLine();
        Assert.That(buf.List, Is.EqualTo(new byte[] { 0xa3, 0x61, 0x73, 0x64 }).AsCollection);
    }

    [Test]
    public void TestGuid()
    {
        var guid = Guid.Parse("5af5c532-4c91-4cd0-b541-15a405395fc5");
        var buf = new ListWriteTarget(new List<byte>());
        var writer = MessagePackWriter.Create(buf);
        writer.WriteGuidAsBytes(guid);
        foreach (var b in buf.List)
        {
            Console.Write($"{b:X} ");
        }
        Console.WriteLine();
        Assert.That(buf.List,
            Is.EqualTo(new byte[] { 0xC4, 0x10, 0x5A, 0xF5, 0xC5, 0x32, 0x4C, 0x91, 0x4C, 0xD0, 0xB5, 0x41, 0x15, 0xA4, 0x05, 0x39, 0x5F, 0xC5 }).AsCollection);
    }

    [Test]
    public void TestArray1()
    {
        var buf = new ListWriteTarget(new List<byte>());
        var writer = MessagePackWriter.Create(buf);
        writer.WriteArrayHead(3);
        writer.WriteInt32(1);
        writer.WriteInt32(2);
        writer.WriteInt32(3);
        foreach (var b in buf.List)
        {
            Console.Write($"{b:X} ");
        }
        Console.WriteLine();
        Assert.That(buf.List, Is.EqualTo(new byte[] { 0x93, 0x01, 0x02, 0x03 }).AsCollection);
    }

    [Test]
    public void TestInt1()
    {
        var buf = new ListWriteTarget(new List<byte>());
        var writer = MessagePackWriter.Create(buf);
        writer.WriteUInt32(123456);
        foreach (var b in buf.List)
        {
            Console.Write($"{b:X} ");
        }
        Console.WriteLine();
        Assert.That(buf.List, Is.EqualTo(new byte[] { 0xCE, 0x00, 0x01, 0xE2, 0x40 }).AsCollection);
    }

    [Test]
    public void TestInt2()
    {
        var buf = new ListWriteTarget(new List<byte>());
        var writer = MessagePackWriter.Create(buf);
        writer.WriteInt32(-123456);
        foreach (var b in buf.List)
        {
            Console.Write($"{b:X} ");
        }
        Console.WriteLine();
        Assert.That(buf.List, Is.EqualTo(new byte[] { 0xD2, 0xFF, 0xFE, 0x1D, 0xC0 }).AsCollection);
    }

    [Test]
    public void TestFloat1()
    {
        var buf = new ListWriteTarget(new List<byte>());
        var writer = MessagePackWriter.Create(buf);
        writer.WriteDouble(123.456);
        foreach (var b in buf.List)
        {
            Console.Write($"{b:X} ");
        }
        Console.WriteLine();
        Assert.That(buf.List, Is.EqualTo(new byte[] { 0xCB, 0x40, 0x5E, 0xDD, 0x2F, 0x1A, 0x9F, 0xBE, 0x77 }).AsCollection);
    }
}
