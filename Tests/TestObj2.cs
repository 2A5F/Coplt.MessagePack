using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial record struct TestObj2
{
    public Dictionary<int, int> A { get; set; }
}

public class TestObj2_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj2 { A = new() { { 1, 2 } } });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x91, 0x81, 0x01, 0x02 }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] { 0x91, 0x81, 0x01, 0x02 };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj2>(bytes);
        Assert.That(a.A, Is.EqualTo(new Dictionary<int, int> { { 1, 2 } }).AsCollection);
    }
}
