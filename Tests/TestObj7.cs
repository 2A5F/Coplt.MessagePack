using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial record struct TestObj7
{
    public (int, int, int, int, int, int, int, int, int) A { get; set; }
}

public class TestObj7_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj7 { A = (1, 2, 3, 4, 5, 6, 7, 8, 9) });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x91, 0x99, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] { 0x91, 0x99, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj7>(bytes);
        Console.WriteLine(a);
        Assert.That(a, Is.EqualTo(new TestObj7 { A = (1, 2, 3, 4, 5, 6, 7, 8, 9) }));
    }
}
