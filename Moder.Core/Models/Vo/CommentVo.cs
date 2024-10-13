using System.Threading;
using ParadoxPower.Process;
using ParadoxPower.Utilities;

namespace Moder.Core.Models.Vo;

public sealed partial class CommentVo : ObservableGameValue
{
    public string Comment { get; set; }

    public CommentVo(string comment, NodeVo? parent)
        : base(string.Empty, parent)
    {
        Comment = comment;
        Type = GameValueType.Comment;
    }

    private static int _commentLine;

    /// <summary>
    /// 将注释转换为多个 <see cref="Child"/>, 以换行符做分隔符, 每个 <see cref="Child"/> 对应文件中的一行注释
    /// </summary>
    /// <returns><see cref="Child"/> 数组, 每个元素对应一行注释</returns>
    public override Child[] ToRawChildren()
    {
        // 两种情况: 单行注释, 多行注释
        // 单行注释直接返回一个Child数组
        // 多行注释按换行符分割, 每个 Child 对应一个注释行

        // 当TextBox未被选中时, 换行符为\r\n, 否则为\r, 并且按下回车键时添加的也是\r, 所以这里统一用\r分割
        if (Comment.Contains('\r'))
        {
            var comments = Comment.Split('\r');
            return Array.ConvertAll(comments, comment => ToRawChild(comment.Trim('\n')));
        }
        return [ToRawChild(Comment)];
    }

    private static Child ToRawChild(string comment)
    {
        // 赋值Line是为了保证多行注释重新写入时，不会重叠在一起, 例如
        // #victory_points = {
        // #	11386 1
        // #}
        // 如果不赋值Line，则会导致多行注释被合并成一行, 变成
        // #victory_points = { # 11386 1 # }
        // 控制 Comment 的格式化代码在 Prints.fs printKeyValue 方法中
        var line = Interlocked.Increment(ref _commentLine);
        return Child.NewCommentChild(
            new Comment(new Position.Range(0, new Position.pos(line, 0), new Position.pos(line, 0)), comment)
        );
    }
}
