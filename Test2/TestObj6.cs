using System.Collections.Frozen;
using System.Text;
using Coplt.MessagePack;

namespace Tests;

[MessagePack]
public partial record struct TestObj6
{
    public (int, int) A { get; set; }
}
