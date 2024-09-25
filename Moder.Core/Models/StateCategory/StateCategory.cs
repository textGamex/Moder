// ReSharper disable once CheckNamespace
namespace Moder.Core.Models;

public sealed class StateCategory(string typeName, byte? localBuildingSlots)
{
    public string TypeName { get; } = typeName;
    public byte? LocalBuildingSlots { get; } = localBuildingSlots;

    public string LocalBuildingSlotsDescription => LocalBuildingSlots.HasValue ? $"[{LocalBuildingSlots}]" : "[?]";
}
