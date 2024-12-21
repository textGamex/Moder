﻿using Moder.Core.Extensions;
using Moder.Core.Infrastructure.Parser;
using ParadoxPower.Process;

namespace Moder.Core.Services.GameResources.Base;

public abstract class CommonResourcesService<TType, TContent> : ResourcesService<TType, TContent, Node>
    where TType : CommonResourcesService<TType, TContent>
{
    /// <inheritdoc />
    protected CommonResourcesService(
        string folderOrFileRelativePath,
        WatcherFilter filter,
        PathType pathType = PathType.Folder
    )
        : base(folderOrFileRelativePath, filter, pathType) { }

    ///<inheritdoc />
    protected abstract override TContent? ParseFileToContent(Node rootNode);

    protected override Node? GetParseResult(string filePath)
    {
        if (!TextParser.TryParse(filePath, out var rootNode, out var error))
        {
            Log.LogParseError(error);
            return null;
        }
        return rootNode;
    }
}
