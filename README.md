# Coplt.MessagePack

[![Nuget](https://img.shields.io/nuget/v/Coplt.MessagePack)](https://www.nuget.org/packages/Coplt.MessagePack/)

Memory-optimized message pack

## Example

```cs
[MessagePack]
public partial record struct TestObj1
{
    public int A { get; set; }
}

var a = new TestObj1 { A = 123 };
List<byte> bytes = MessagePackSerializer.Instance.Serialize(a);
var b = MessagePackSerializer.Instance.Deserialize<TestObj1>(bytes);

Assert.That(b, Is.EqualTo(a));
```

## Todo

- [ ] Generators support generics
- [ ] Support `non ()` constructor
- [ ] Support for more system library types
- [ ] Wait .Net 10 extensions, `MessagePackSerializer.Instance` replace to `MessagePackSerializer`
