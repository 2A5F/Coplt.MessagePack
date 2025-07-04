using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack(AsArray = false)]
public partial record struct TestObj5
{
    public int A { get; set; }
    public float B { get; set; }
}

public class TestObj5_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj5 { A = 123, B = 456 });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x82, 0xA1, 0x41, 0x7B, 0xA1, 0x42, 0xCA, 0x43, 0xE4, 0x00, 0x00 }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] { 0x82, 0xA1, 0x41, 0x7B, 0xA1, 0x42, 0xCA, 0x43, 0xE4, 0x00, 0x00 };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj5>(bytes);
        Console.WriteLine(a);
        Assert.That(a, Is.EqualTo(new TestObj5 { A = 123, B = 456 }));
    }
}
