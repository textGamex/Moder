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
    public bool IsSuccess { get; }

    private readonly NodeVo _rootNodeVo = new("Root", null);
    private readonly SystemFileItem _fileItem;
    private readonly ILogger<StateFileControlViewModel> _logger;

    public StateFileControlViewModel(GlobalResourceService resourceService, ILogger<StateFileControlViewModel> logger)
    {
        _logger = logger;
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

    private static void Convert(Node node, NodeVo nodeVo)
    {
        foreach (var child in node.AllArray)
        {
            if (child.IsLeafChild)
            {
                var leaf = child.leaf;
                nodeVo.Add(new LeafVo(leaf.Key, leaf.Value, nodeVo));
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
        Save(rootNode, _rootNodeVo.Children.ToArray());
        // var rootNode = SaveToNode(Items);
        var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
        _logger.LogInformation("保存成功, 耗时: {time} ms", elapsedTime.TotalMilliseconds);
        _logger.LogDebug(
            "Content: {content}",
            CKPrinter.PrettyPrintStatements(rootNode.AllArray.Select(child => child.GetRawStatement(rootNode.Key)))
        );
    }

    private void Save(Node node, ObservableGameValue[] items)
    {
        var list = node.AllChildren;
        for (var index = 0; index < list.Count; index++)
        {
            var child = list[index];
            if (child.IsLeafChild || child.IsNodeChild || child.IsLeafValueChild)
            {
                var item = Array.Find(items, i => i.Key == child.GetKey());
                if (item is not null)
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
                        Save(
                            // Array.Find(node.AllArray, child => child.IsNodeChild && child.node.Key == nodeVo.Key).node,
                            list.Find(c => c.IsNodeChild && c.node.Key == nodeVo.Key).node,
                            nodeVo.Children.ToArray()
                        );
                    }
                }
                else
                {
                    list.RemoveAt(index--);
                }
            }
        }

        node.AllArray = list.ToArray();
    }

    // private Node SaveToNode(IEnumerable<ObservableGameValue> items)
    // {
    //     var rootNode = new Node(_fileItem.Name);
    //     foreach (var item in items)
    //     {
    //
    //     }
    // }
}
