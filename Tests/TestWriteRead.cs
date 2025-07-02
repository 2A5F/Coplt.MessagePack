using Coplt.MessagePack;

namespace Tests;

public class TestWriteRead
{
    [Test]
    public void TestString1()
    {
        using var stream = new MemoryStream();
        using var writer = MessagePackWriter.Create(new StreamWriteTarget(stream));
        writer.WriteString("asd");
        stream.Position = 0;
        using var reader = MessagePackReader.Create(new StreamReadSource(stream));
        var str = reader.ReadString();
        Assert.That(str, Is.EqualTo("asd"));
    }

    [Test]
    public void TestString2()
    {
        var src = new string('a', 3000);
        using var stream = new MemoryStream();
        using var writer = MessagePackWriter.Create(new StreamWriteTarget(stream));
        writer.WriteString(src);
        stream.Position = 0;
        using var reader = MessagePackReader.Create(new StreamReadSource(stream));
        var str = reader.ReadString();
        Assert.That(str, Is.EqualTo(src));
    }
    [Test]
    public void TestString3()
    {
        var src = new string('a', 1020);
        using var stream = new MemoryStream();
        using var writer = MessagePackWriter.Create(new StreamWriteTarget(stream));
        writer.WriteString("asd");
        writer.WriteString(src);
        stream.Position = 0;
        using var reader = MessagePackReader.Create(new StreamReadSource(stream));
        var str1 = reader.ReadString();
        var str2 = reader.ReadString();
        Assert.Multiple(() =>
        {
            Assert.That(str1, Is.EqualTo("asd"));
            Assert.That(str2, Is.EqualTo(src));
        });
    }
}
