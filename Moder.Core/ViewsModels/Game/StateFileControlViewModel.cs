using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MethodTimer;
using Moder.Core.Extensions;
using Moder.Core.Models;
using Moder.Core.Models.Vo;
using Moder.Core.Parser;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;
using NLog;
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
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private readonly LeafConverterService _leafConverterService;

    public StateFileControlViewModel(
        GlobalResourceService resourceService,
        LeafConverterService leafConverterService
    )
    {
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
        Log.Info("解析时间: {Time} ms", elapsedTime.TotalMilliseconds);

        // 递归遍历所有节点
        ConvertData(rootNode);
    }

    [Time("AST -> Vo")]
    private void ConvertData(Node rootNode)
    {
        Debug.Assert(rootNode.Key == _fileItem.Name);
        NodeConvertToVo(rootNode, _rootNodeVo);
    }

    // TODO: 重构
    private void NodeConvertToVo(Node node, NodeVo nodeVo)
    {
        var children = node.AllArray;
        for (var index = 0; index < children.Length; index++)
        {
            var child = children[index];
            if (child.IsLeafChild)
            {
                var leaf = child.leaf;
                AddCommentInAdvance(ref index, leaf.Position.StartLine);
                var leafVo = _leafConverterService.GetSpecificLeafVo(
                    leaf.Key,
                    leaf.Value.ToRawString(),
                    leaf.Value.ToLocalValueType(),
                    nodeVo
                );
                nodeVo.Add(leafVo);
            }
            else if (child.IsNodeChild)
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
                    AddCommentInAdvance(ref index, childNode.Position.StartLine);
                    nodeVo.Add(childNodeVo);
                    NodeConvertToVo(childNode, childNodeVo);
                }
            }
            else if (child.IsCommentChild)
            {
                var comment = child.comment;
                var nextChildIndex = index + 1;
                if (nextChildIndex < children.Length && children[nextChildIndex].IsCommentChild)
                {
                    // 将连续的注释合并为一个节点
                    // FuncName 合并连续注释
                    var builder = new StringBuilder(comment.Comment, 64 + comment.Comment.Length);
                    builder.Append(Environment.NewLine);
                    while (index + 1 < children.Length && children[index + 1].IsCommentChild)
                    {
                        builder.AppendLine(children[index + 1].comment.Comment);
                        ++index;
                    }

                    // 移除尾部换行符
                    builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
                    nodeVo.Add(new CommentVo(builder.ToString(), nodeVo));
                }
                else
                {
                    nodeVo.Add(new CommentVo(comment.Comment, nodeVo));
                }
            }
        }
        return;

        // 将尾后的注释移动到被注释代码的上一行, 方便我们显示和处理
        // 注意: 这会导致注释的位置发生变化, 例如:
        // #comment1
        // key = value #comment2
        // 转换后:
        // #comment1
        // #comment2
        // key = value
        void AddCommentInAdvance(ref int currentIndex, int statementStartLine)
        {
            var nextChildIndex = currentIndex + 1;

            if (nextChildIndex >= children.Length)
            {
                return;
            }

            var nextChild = children[nextChildIndex];
            if (nextChild.IsCommentChild && nextChild.comment.Position.StartLine == statementStartLine)
            {
                var comment = nextChild.comment;
                nodeVo.Add(new CommentVo(comment.Comment, nodeVo));
                ++currentIndex;
            }
        }
    }

    [RelayCommand]
    private void SaveData()
    {
        var timestamp = Stopwatch.GetTimestamp();
        // TODO: 数值有效性检查, int, float, bool
        var rootNode = VoConvertToNode(_fileItem.Name, _rootNodeVo.Children.ToArray());
        var text = CKPrinter.PrettyPrintStatements(
            Array.ConvertAll(rootNode.AllArray, child => child.GetRawStatement(rootNode.Key))
        );
        var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
        Log.Info("保存成功, 耗时: {Time} ms", elapsedTime.TotalMilliseconds);
        Log.Debug("Content: {Content}", text);
    }

    private static Node VoConvertToNode(string key, ObservableGameValue[] gameVos)
    {
        var node = new Node(key);
        var list = new List<Child>(gameVos.Length);

        foreach (var gameVo in gameVos)
        {
            list.AddRange(gameVo.ToRawChildren());
        }
        node.AllArray = list.ToArray();
        return node;
    }
}
