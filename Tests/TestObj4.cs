using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial record TestObj4
{
    public TestObj4? A { get; set; }
}

public class TestObj4_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj4 { A = new TestObj4() });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x91, 0x91, 0xC0 }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] { 0x91, 0x91, 0xC0 };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj4>(bytes);
        Console.WriteLine(a);
        Assert.That(a, Is.EqualTo(new TestObj4 { A = new TestObj4() }));
    }
}
