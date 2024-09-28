using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Moder.Core.Messages;
using Moder.Core.Services;
using Moder.Core.ViewsModels.Menus;

namespace Moder.Core.Views.Menus;

public sealed partial class SideWorkSpaceControlView : UserControl
{
	private readonly ILogger<SideWorkSpaceControlView> _logger;
	private readonly GlobalResourceService _resourceService;

	public SideWorkSpaceControlView(
		SideWorkSpaceControlViewModel model,
		ILogger<SideWorkSpaceControlView> logger,
		GlobalResourceService resourceService
	)
	{
		_logger = logger;
		_resourceService = resourceService;
		InitializeComponent();

		DataContext = model;
	}

	private void FileTreeView_OnSelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs args)
	{
		if (args.AddedItems.Count != 1)
		{
			_logger.LogDebug("未选中文件");
			return;
		}

		if (args.AddedItems[0] is SystemFileItem { IsFile: true } file)
		{
			_logger.LogInformation("文件: {File}", file.Name);
			// TODO: 这样做只能打开一个文件
			_resourceService.SetCurrentSelectFileItem(file);

			WeakReferenceMessenger.Default.Send(new OpenFileMessage(file));
		}
	}
}