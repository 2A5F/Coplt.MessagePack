using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial record struct TestObj11
{
    public string A { get; set; }
}

public class TestObj11_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj11 { A = "asd" });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x91, 0xA3, 0x61, 0x73, 0x64 }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] { 0x91, 0xA3, 0x61, 0x73, 0x64 };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj11>(bytes);
        Console.WriteLine(a);
        Assert.That(a, Is.EqualTo(new TestObj11 { A = "asd" }));
    }
}
