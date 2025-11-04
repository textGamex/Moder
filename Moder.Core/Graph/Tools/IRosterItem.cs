namespace Moder.Core.Graph.Tools;

public interface IRosterItem<out TSignature>
    where TSignature : notnull
{
    public TSignature Signature { get; }
}
