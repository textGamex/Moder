// ReSharper disable once CheckNamespace
namespace Moder.Core.Models;

public sealed class BuildingInfo(string name, byte? maxLevel)
{
	public string Name { get; } = name;
	public byte? MaxLevel { get; } = maxLevel;
}