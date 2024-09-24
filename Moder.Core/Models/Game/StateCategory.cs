namespace Moder.Core.Models.Game;

public sealed class StateCategory(string typeName, byte? localBuildingSlots)
{
    public string TypeName { get; } = typeName;
    public byte? LocalBuildingSlots { get; } = localBuildingSlots;

    public string Description => $"{TypeName} [{LocalBuildingSlots}]";
}
