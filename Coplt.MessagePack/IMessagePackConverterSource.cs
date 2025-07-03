namespace Coplt.MessagePack;

public interface IMessagePackConverterSource<TTarget, TConverter>
    where TTarget : allows ref struct
    where TConverter : allows ref struct;
