using Moder.Core.Models;

namespace Moder.Core.Messages;

public sealed record SyncSideWorkSelectedItemMessage(SystemFileItem? TargetItem);