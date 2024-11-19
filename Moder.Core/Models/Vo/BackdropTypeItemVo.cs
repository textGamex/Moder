namespace Moder.Core.Models.Vo;

    public sealed class BackdropTypeItemVo(string text, WindowBackdropType backdrop)
    {
        public string Text { get; } = text;
        public WindowBackdropType Backdrop { get; } = backdrop;
    }