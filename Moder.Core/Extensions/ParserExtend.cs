using System.ComponentModel;
using Microsoft.FSharp.Collections;
using Moder.Core.Models;
using ParadoxPower.Parser;
using ParadoxPower.Process;
using ParadoxPower.Utilities;

namespace Moder.Core.Extensions;

public static class ParserExtend
{
    public static bool HasNot(this Node node, string key)
    {
        return !node.Has(key);
    }

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

    public static string GetKey(this Child child)
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

        if (child.IsCommentChild || child.IsValueClauseChild)
        {
            throw new InvalidOperationException("这个 child 不存在 key");
        }
        throw new InvalidEnumArgumentException(nameof(child));
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
                keys[i] = Types.Statement.NewValue(Position.Range.Zero, Types.Value.NewString(valueClause.Keys[i]));
            }
            keys[^1] = Types.Statement.NewValue(valueClause.Position, Types.Value.NewClause(valueClause.ToRaw));

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
}
