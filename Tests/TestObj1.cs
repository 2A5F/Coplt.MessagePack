using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial record struct TestObj1
{
    public int A { get; set; }
}

public class TestObj1_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj1 { A = 123 });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x91, 0x7B }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] { 0x91, 0x7B };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj1>(bytes);
        Console.WriteLine(a);
        Assert.That(a, Is.EqualTo(new TestObj1 { A = 123 }));
    }
}
