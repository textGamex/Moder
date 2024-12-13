using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.FSharp.Collections;
using Moder.Core.Models.Game;
using ParadoxPower.Parser;
using ParadoxPower.Process;
using ParadoxPower.Utilities;

namespace Moder.Core.Extensions;

public static class ParserExtensions
{
    public static GameValueType ToLocalValueType(this Types.Value value)
    {
        if (value.IsBool)
        {
            return GameValueType.Bool;
        }

        if (value.IsFloat)
        {
            return GameValueType.Float;
        }

        if (value.IsInt)
        {
            return GameValueType.Int;
        }

        if (value.IsString)
        {
            return GameValueType.String;
        }

        if (value.IsQString)
        {
            return GameValueType.StringWithQuotation;
        }

        // if (value.IsClause)
        // {
        //     return GameValueType.Clause;
        // }
        throw new InvalidEnumArgumentException(nameof(value));
    }

    public static string? GetKeyOrNull(this Child child)
    {
        if (child.IsLeafChild)
        {
            return child.leaf.Key;
        }

        if (child.IsNodeChild)
        {
            return child.node.Key;
        }

        if (child.IsLeafValueChild)
        {
            return child.leafvalue.Key;
        }

        return null;
    }

    public static Types.Statement GetRawStatement(this Child child, string key)
    {
        if (child.IsNodeChild)
        {
            return child.node.ToRaw;
        }

        if (child.IsLeafChild)
        {
            return child.leaf.ToRaw;
        }

        if (child.IsLeafValueChild)
        {
            return child.leafvalue.ToRaw;
        }

        if (child.IsCommentChild)
        {
            return Types.Statement.NewCommentStatement(child.comment);
        }

        if (child.IsValueClauseChild)
        {
            var valueClause = child.valueclause;
            var keys = new Types.Statement[valueClause.Keys.Length + 1];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = Types.Statement.NewValue(
                    Position.Range.Zero,
                    Types.Value.NewString(valueClause.Keys[i])
                );
            }
            keys[^1] = Types.Statement.NewValue(
                valueClause.Position,
                Types.Value.NewClause(valueClause.ToRaw)
            );

            return Types.Statement.NewKeyValue(
                Types.PosKeyValue.NewPosKeyValue(
                    valueClause.Position,
                    Types.KeyValueItem.NewKeyValueItem(
                        Types.Key.NewKey(key),
                        Types.Value.NewClause(ListModule.OfArray(keys)),
                        Types.Operator.Equals
                    )
                )
            );
        }

        throw new InvalidEnumArgumentException(nameof(child));
    }

    /// <summary>
    /// 添加一个<see cref="Node"/>子节点
    /// </summary>
    /// <param name="node"></param>
    /// <param name="key">添加的<see cref="Node"/>的 key</param>
    /// <returns>添加的<see cref="Node"/></returns>
    public static Node AddNodeChild(this Node node, string key)
    {
        var nodeChild = new Node(key);
        node.AddChild(Child.NewNodeChild(nodeChild));
        return nodeChild;
    }

    public static string PrintChildren(this Node node)
    {
        return CKPrinter.PrettyPrintStatements(
            Array.ConvertAll(node.AllArray, child => child.GetRawStatement(node.Key))
        );
    }

    public static string PrintRaw(this Node node)
    {
        return CKPrinter.PrettyPrintStatement(node.ToRaw);
    }

    /// <summary>
    /// 判断一个 <see cref="Child"/> 是否为带指定<c>key</c>的 <see cref="Node"/>
    /// </summary>
    /// <param name="child"></param>
    /// <param name="key">键, 不区分大小写</param>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool IsNodeWithKey(this Child child, string key, [NotNullWhen(true)] out Node? node)
    {
        if (child.IsNodeChild && StringComparer.OrdinalIgnoreCase.Equals(child.node.Key, key))
        {
            node = child.node;
            return true;
        }

        node = null;
        return false;
    }
}
