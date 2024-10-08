using Microsoft.Extensions.DependencyInjection;
using Moder.Core.Services.GameResources;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models;

public sealed class StateCategory(string typeName, byte? localBuildingSlots)
{
    public string TypeName { get; } = typeName;
    public byte? LocalBuildingSlots { get; } = localBuildingSlots;

    public string TypeNameDescription => GameResourcesService.Localisation.GetValue(TypeName);
    public string LocalBuildingSlotsDescription => LocalBuildingSlots.HasValue ? $"[{LocalBuildingSlots}]" : "[?]";

    private static readonly GameResourcesService GameResourcesService = App
        .Current.Services.GetRequiredService<GameResourcesService>();
}
