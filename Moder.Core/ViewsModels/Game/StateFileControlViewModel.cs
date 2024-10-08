using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MethodTimer;
using Microsoft.Extensions.Logging;
using Moder.Core.Extensions;
using Moder.Core.Models;
using Moder.Core.Models.Vo;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;
using ParadoxPower.Parser;
using ParadoxPower.Process;

namespace Moder.Core.ViewsModels.Game;

public sealed partial class StateFileControlViewModel : ObservableObject
{
	public IReadOnlyList<ObservableGameValue> Items => _rootNodeVo.Children;
	public string Title => _fileItem.Name;
	public string FullPath => _fileItem.FullPath;
	public bool IsSuccess { get; }

	private readonly NodeVo _rootNodeVo = new("Root", null);
	private readonly SystemFileItem _fileItem;
	private readonly ILogger<StateFileControlViewModel> _logger;
	private readonly LeafConverterService _leafConverterService;

	public StateFileControlViewModel(
		GlobalResourceService resourceService,
		ILogger<StateFileControlViewModel> logger,
		LeafConverterService leafConverterService
	)
	{
		_logger = logger;
		_leafConverterService = leafConverterService;
		_fileItem = resourceService.PopCurrentSelectFileItem();
		Debug.Assert(_fileItem.IsFile);

		var timestamp = Stopwatch.GetTimestamp();
		var parser = new TextParser(_fileItem.FullPath);
		if (parser.IsFailure)
		{
			IsSuccess = false;
			return;
		}

		IsSuccess = true;

		var rootNode = parser.GetResult();
		var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
		logger.LogInformation("解析时间: {time} ms", elapsedTime.TotalMilliseconds);

		// 递归遍历所有节点
		ConvertData(rootNode);
	}

	[Time("AST -> Vo")]
	private void ConvertData(Node rootNode)
	{
		Debug.Assert(rootNode.Key == _fileItem.Name);
		Convert(rootNode, _rootNodeVo);
	}

	private void Convert(Node node, NodeVo nodeVo)
	{
		foreach (var child in node.AllArray)
		{
			if (child.IsLeafChild)
			{
				var leaf = child.leaf;
				var leafVo = _leafConverterService.GetSpecificLeafVo(
					leaf.Key,
					leaf.Value.ToRawString(),
					leaf.Value.ToLocalValueType(),
					nodeVo
				);
				nodeVo.Add(leafVo);
			}

			if (child.IsNodeChild)
			{
				var childNode = child.node;
				// 当 LeafValues 不为空时，表示该节点是 LeafValues 节点
				if (childNode.LeafValues.Any())
				{
					nodeVo.Add(new LeafValuesVo(childNode.Key, childNode.LeafValues, nodeVo));
				}
				else
				{
					// 是普通节点
					var childNodeVo = new NodeVo(childNode.Key, nodeVo);
					nodeVo.Add(childNodeVo);
					Convert(childNode, childNodeVo);
				}
			}

			// if (child.IsCommentChild)
			// {
			//     var comment = child.comment;
			// }
		}
	}

	[RelayCommand]
	private void SaveData()
	{
		var parser = new TextParser(_fileItem.FullPath);
		if (parser.IsFailure)
		{
			return;
		}

		// 与 Items 相同的层级结构
		var rootNode = parser.GetResult();
		var timestamp = Stopwatch.GetTimestamp();
		// TODO: 数值有效性检查, int, float, bool
		Save(rootNode, _rootNodeVo.Children.ToList());
		var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
		_logger.LogInformation("保存成功, 耗时: {time} ms", elapsedTime.TotalMilliseconds);
		_logger.LogDebug(
			"Content: {content}",
			CKPrinter.PrettyPrintStatements(rootNode.AllArray.Select(child => child.GetRawStatement(rootNode.Key)))
		);
	}

	private static void Save(Node rawNode, List<ObservableGameValue> newItems)
	{
		var rawList = rawNode.AllChildren;
		for (var index = 0; index < rawList.Count; index++)
		{
			var rawChild = rawList[index];
			if (rawChild.IsLeafChild || rawChild.IsNodeChild)
			{
				var newItem = newItems.Find(item => item.Key == rawChild.GetKey());
				if (newItem is null)
				{
					rawList.RemoveAt(index--);
				}
				else
				{
					if (newItem is LeafVo { IsChanged: true } leafVo)
					{
						rawChild.leaf.Value = leafVo.ToRawValue();
					}
					else if (newItem is LeafValuesVo { IsChanged: true } leafValuesVo)
					{
						rawChild.node.AllArray = leafValuesVo.ToLeafValues();
					}
					else if (newItem is NodeVo nodeVo)
					{
						Save(
							rawChild.node,
							nodeVo.Children.ToList()
						);
					}

					newItems.Remove(newItem);
				}
			}
		}
		rawList.AddRange(newItems.Select(item => item.ToRawChild()));
		rawNode.AllArray = rawList.ToArray();
	}
}
