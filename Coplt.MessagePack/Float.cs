using Coplt.Union;

namespace Coplt.MessagePack;

[Union]
public readonly partial struct MessagePackFloat
{
    #region Define

    [UnionTemplate]
    private interface Template
    {
        void None();
        float Single();
        double Double();
    }

    public static readonly MessagePackFloat None = MakeNone();

    #endregion

    #region Convert

    public static implicit operator MessagePackFloat(float value) => MakeSingle(value);

    public static implicit operator MessagePackFloat(double value) => MakeDouble(value);

    #endregion

    #region Get

    public float ToSingle() => Tag switch
    {
        Tags.Single => Single,
        Tags.Double => (float)Double,
        _ => throw new ArgumentOutOfRangeException()
    };

    public double ToDouble() => Tag switch
    {
        Tags.Single => Single,
        Tags.Double => Double,
        _ => throw new ArgumentOutOfRangeException()
    };

    #endregion

    #region TryGet

    public float? TryToSingle() => Tag switch
    {
        Tags.None => null,
        Tags.Single => Single,
        Tags.Double => (float)Double,
        _ => throw new ArgumentOutOfRangeException()
    };

    public double? TryToDouble() => Tag switch
    {
        Tags.None => null,
        Tags.Single => Single,
        Tags.Double => Double,
        _ => throw new ArgumentOutOfRangeException()
    };

    #endregion
}
