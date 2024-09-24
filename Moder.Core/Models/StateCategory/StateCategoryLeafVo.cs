using ParadoxPower.Parser;
using Moder.Core.Services;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Moder.Core.Models.Vo;

public sealed partial class StateCategoryLeafVo(string key, Types.Value value, NodeVo? parent)
	: LeafVo(key, value, parent)
{
	public override string Value
	{
		get => _value;
		set
		{
			SetProperty(ref _value, value);
			OnPropertyChanged(nameof(StateCategoryUiDescription));
		}
	}

	public string StateCategoryUiDescription => GetStateCategoryUiDescription();

	public static IEnumerable<StateCategory> StateCategory { get; }
	private static readonly StateCategoryService StateCategoryService;

	static StateCategoryLeafVo()
	{
		StateCategoryService = App.Current.Services.GetRequiredService<GameResourcesService>().StateCategory;
		StateCategory = StateCategoryService.StateCategories;
	}

	private string GetStateCategoryUiDescription()
	{
		return StateCategoryService.TryGetValue(Value, out var stateCategory)
			? $"建筑槽位 [{stateCategory.LocalBuildingSlots}]"
			: $"未知的 {Key}";
	}
}