using System.Diagnostics;
using CWTools.Process;
using Moder.Core.Extensions;
using Moder.Core.Models.Vo;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.ViewsModels.Game;

public sealed class StateFileControlViewModel
{
	public List<LeafVo>? Leaves { get; }
	public string Title => _fileItem.Name;
	public bool IsSuccess { get; }

	private readonly SystemFileItem _fileItem;

	public StateFileControlViewModel(GlobalResourceService resourceService)
	{
		_fileItem = resourceService.PopCurrentSelectFileItem();
		Debug.Assert(_fileItem.IsFile);
		var parser = new CwToolsParser(_fileItem.FullPath);
		if (parser.IsFailure)
		{
			IsSuccess = false;
			return;
		}

		IsSuccess = true;

		var rootNode = parser.GetResult().GetChild("state");
		var leaves = new List<LeafVo>();

		foreach (var leaf in rootNode.Leaves)
		{
			leaves.Add(new LeafVo(leaf.Key, leaf.Value.ToRawString()));
		}

		Leaves = leaves;
	}

	public void Save()
	{
		Debug.Assert(Leaves != null);
		var parser = new CwToolsParser(_fileItem.FullPath);
		if (parser.IsFailure)
		{
			return;
		}

		var rootNode = parser.GetResult().GetChild("state");
		foreach (var leaf in Leaves)
		{
			// rootNode.SetTag(leaf.Key, new Leaf());
		}
	}
}