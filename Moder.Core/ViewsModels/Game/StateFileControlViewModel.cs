using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Collections;
using Moder.Core.Extensions;
using Moder.Core.Models;
using Moder.Core.Models.Vo;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;
using ParadoxPower.Parser;
using ParadoxPower.Process;
using ParadoxPower.Utilities;

namespace Moder.Core.ViewsModels.Game;

public sealed partial class StateFileControlViewModel : ObservableObject
{
    public List<ObservableGameValue> Items { get; } = new(2);
    public string Title => _fileItem.Name;
    public bool IsSuccess { get; }

    private readonly ILogger<StateFileControlViewModel> _logger;
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
        var timestamp = Stopwatch.GetTimestamp();
        var rootNode = parser.GetResult();
        var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
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
        Save(rootNode, Items);
        _logger.LogInformation("保存成功");
        _logger.LogInformation(
            "Content: {content}",
            CKPrinter.PrettyPrintStatements(rootNode.AllArray.Select(child => child.GetRawStatement(rootNode.Key)))
        );
    }

    private void Save(Node node, IEnumerable<ObservableGameValue> items)
    {
        foreach (var item in items)
        {
            if (item is LeafVo { IsChanged: true } leafVo)
            {
                if (!node.TryGetLeaf(leafVo.Key, out var rawLeaf))
                {
                    _logger.LogWarning("找不到Leaf: {key}", leafVo.Key);
                    continue;
                }

                rawLeaf.Value = leafVo.ToRawValue();
            }

            if (item is LeafValuesVo { IsChanged: true } leafValuesVo)
            {
                if (!node.TryGetChild(leafValuesVo.Key, out var rawLeafVales))
                {
                    _logger.LogWarning("找不到LeafValues: {key}", leafValuesVo.Key);
                    continue;
                }

                rawLeafVales.AllArray = leafValuesVo.ToLeafValues();
            }

            if (item is NodeVo nodeVo)
            {
                Save(Array.Find(node.AllArray, i => i.IsNodeChild && i.node.Key == nodeVo.Key).node, nodeVo.Children);
            }
        }
    }
}
