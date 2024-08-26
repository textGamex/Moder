using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moder.Core.Extensions;
using Moder.Core.Models.Vo;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;
using ParadoxPower.Process;

namespace Moder.Core.ViewsModels.Game;

public sealed class StateFileControlViewModel
{
	private readonly ILogger<StateFileControlViewModel> _logger;
	public List<object> Items { get; } = new(2);
    public string Title => _fileItem.Name;
    public bool IsSuccess { get; }

    private readonly SystemFileItem _fileItem;

    public StateFileControlViewModel(GlobalResourceService resourceService, ILogger<StateFileControlViewModel> logger)
    {
	    _logger = logger;
	    _fileItem = resourceService.PopCurrentSelectFileItem();
        Debug.Assert(_fileItem.IsFile);

        var parser = new TextParser(_fileItem.FullPath);
        if (parser.IsFailure)
        {
            IsSuccess = false;
            return;
        }

        IsSuccess = true;
        var timestamp = Stopwatch.GetTimestamp();
        var rootNode = parser.GetResult();
        var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
        logger.LogInformation("解析时间: {time} ms", elapsedTime.TotalMilliseconds);

        // 递归遍历所有节点
        ConvertData(rootNode);
    }

    private void ConvertData(Node rootNode)
    {
	    Debug.Assert(rootNode.Key == _fileItem.Name);

	    var timestamp = Stopwatch.GetTimestamp();

        // 根节点的key为文件名称, 忽略
        foreach (var child in rootNode.AllArray)
        {
	        if (child.IsLeafChild)
	        {
		        var leaf = child.leaf;
		        Items.Add(new LeafVo(leaf.Key, leaf.Value));
	        }

	        if (child.IsNodeChild)
	        {
		        var childNode = child.node;
		        // 当LeafValues不为空时，表示该节点是LeafValues节点
		        if (childNode.LeafValues.Any())
		        {
			        Items.Add(new LeafValuesVo(childNode.Key, childNode.LeafValues));
		        }
		        else
		        {
			        // 是普通节点
			        var childNodeVo = new NodeVo(childNode.Key);
			        Convert(childNode, childNodeVo);
			        Items.Add(childNodeVo);
		        }
	        }
        }

	    var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
	    _logger.LogInformation("转换时间: {time} ms", elapsedTime.TotalMilliseconds);
    }

    private void Convert(Node node, NodeVo nodeVo)
    {
	    foreach (var child in node.AllArray)
	    {
		    if (child.IsLeafChild)
		    {
			    var leaf = child.leaf;
			    nodeVo.AddChild(new LeafVo(leaf.Key, leaf.Value));
		    }

		    if (child.IsNodeChild)
		    {
			    var childNode = child.node;
			    // 当LeafValues不为空时，表示该节点是LeafValues节点
			    if (childNode.LeafValues.Any())
			    {
				    nodeVo.AddChild(new LeafValuesVo(childNode.Key, childNode.LeafValues));
			    }
			    else
			    {
				    if (childNode.Key == _fileItem.Name)
				    {
                        _logger.LogInformation("忽略根节点");
					    continue;
				    }
				    // 是普通节点
				    var childNodeVo = new NodeVo(childNode.Key);
				    nodeVo.AddChild(childNodeVo);
				    Convert(childNode, childNodeVo);
			    }
		    }
	    }
    }

    public void Save()
    {
        var parser = new TextParser(_fileItem.FullPath);
        if (parser.IsFailure)
        {
            return;
        }

        var rootNode = parser.GetResult().GetChild("state");
		
        // TODO: 保存到文件

    }
}
