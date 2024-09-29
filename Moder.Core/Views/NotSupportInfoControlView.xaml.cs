using Moder.Core.Services;

namespace Moder.Core.Views;

public sealed partial class NotSupportInfoControlView : IFileView
{
	public NotSupportInfoControlView(GlobalResourceService resourceService)
	{
		this.InitializeComponent();

		FullPath = resourceService.PopCurrentSelectFileItem().FullPath;
	}

	public string Title => "²»Ö§³Ö";
	public string FullPath { get; }
}