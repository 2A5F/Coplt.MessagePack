using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial record struct TestObj6
{
    public (int, int) A { get; set; }
}

public class TestObj6_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj6 { A = (123, 456) });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x91, 0x92, 0x7B, 0xCD, 0x01, 0xC8 }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] { 0x91, 0x92, 0x7B, 0xCD, 0x01, 0xC8 };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj6>(bytes);
        Console.WriteLine(a);
        Assert.That(a, Is.EqualTo(new TestObj6 { A = (123, 456) }));
    }
}
