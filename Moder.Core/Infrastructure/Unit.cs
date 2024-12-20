namespace Moder.Core.Infrastructure;

public sealed class Unit
{
    private Unit() { }

    public static readonly Unit Value = new();
}
