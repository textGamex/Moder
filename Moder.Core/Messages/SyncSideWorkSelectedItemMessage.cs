using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.Messages;

public sealed record SyncSideWorkSelectedItemMessage(SystemFileItem? TargetItem);