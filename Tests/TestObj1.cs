using System.Collections.Frozen;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial struct TestObj1<T>
{
    public int A { get; set; }
}
