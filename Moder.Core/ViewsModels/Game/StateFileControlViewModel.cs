using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moder.Core.Extensions;
using Moder.Core.Models.Vo;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.ViewsModels.Game;

public sealed class StateFileControlViewModel
{
    public List<object> Items { get; } = new(16);
    public string Title => _fileItem.Name;
    public bool IsSuccess { get; }

    private readonly SystemFileItem _fileItem;

    public StateFileControlViewModel(GlobalResourceService resourceService, ILogger<StateFileControlViewModel> logger)
    {
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
        var rootNode = parser.GetResult().GetChild("state");
        var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
        logger.LogInformation("解析时间: {time}", elapsedTime.TotalMilliseconds);

        // TODO: 递归遍历所有节点
        foreach (var child in rootNode.AllArray)
        {
            if (child.IsLeafChild)
            {
                var leaf = child.leaf;
                Items.Add(new LeafVo(leaf.Key, leaf.Value));
            }

            if (child.IsNodeChild)
            {
                var node = child.node;
                // 当LeafValues不为空时，表示该节点是LeafValues节点
                if (node.LeafValues.Any())
                {
                    Items.Add(new LeafValuesVo(node.Key, node.LeafValues));
                }
                else
                {
                    // 是普通节点
                }
            }
        }
    }

    public void Save()
    {
        // Debug.Assert(Leaves != null);
        var parser = new TextParser(_fileItem.FullPath);
        if (parser.IsFailure)
        {
            return;
        }

        var rootNode = parser.GetResult().GetChild("state");

        // foreach (var leaf in Leaves.Where(item => item.IsChanged))
        // {
        //     foreach (var child in rootNode.Leaves)
        //     {
        //         if (child.Key == leaf.Key)
        //         {
        //             child.Value = leaf.ToRawValue();
        //             break;
        //         }
        //     }
        // }
        // TODO: 保存到文件
    }
}
