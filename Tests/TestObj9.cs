using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

public enum TestObj9Enum
{
    A = 1,
    B = 2,
    C = 3
}

[MessagePack]
public partial record struct TestObj9
{
    public TestObj9Enum A { get; set; }
}

public class TestObj9_Tests
{
    [Test]
    public void Test1()
    {
        var a = MessagePackSerializer.Instance.Serialize(new TestObj9 { A = TestObj9Enum.C });
        Console.WriteLine(string.Join(" ", a.Select(b => $"{b:X}")));
        Assert.That(a, Is.EqualTo(new byte[] { 0x91, 0x03 }).AsCollection);
    }
    [Test]
    public void Test2()
    {
        var bytes = new byte[] {0x91, 0x03 };
        var a = MessagePackSerializer.Instance.Deserialize<TestObj9>(bytes);
        Console.WriteLine(a);
        Assert.That(a, Is.EqualTo(new TestObj9 { A = TestObj9Enum.C }));
    }
}
