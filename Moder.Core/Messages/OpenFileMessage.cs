using Moder.Core.Models;

namespace Moder.Core.Messages;

public sealed record OpenFileMessage(SystemFileItem FileItem);