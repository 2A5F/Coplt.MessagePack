using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial record struct TestObj3
{
    public int A { get; set; }
    public TestObj1 B { get; set; }
}

public class TestObj3_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj3 { A = 123, B = new() { A = 456 } });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x92, 0x7B, 0x91, 0xCD, 0x01, 0xC8 }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] { 0x92, 0x7B, 0x91, 0xCD, 0x01, 0xC8 };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj3>(bytes);
        Console.WriteLine(a);
        Assert.That(a, Is.EqualTo(new TestObj3 { A = 123, B = new() { A = 456 } }));
    }
}
