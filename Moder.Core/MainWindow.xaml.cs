using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Moder.Core.Config;
using Moder.Core.Views.Menus;
using Windows.Media;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Moder.Core;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
	public MainWindowViewModel ViewModel { get; }
	public MainWindow(MainWindowViewModel model, GlobalSettings settings, IServiceProvider serviceProvider)
	{
		this.InitializeComponent();

		ExtendsContentIntoTitleBar = true;
		ViewModel = model;
		if (string.IsNullOrEmpty(settings.WorkRootFolderPath))
		{
			SideContentControl.Content = serviceProvider.GetRequiredService<OpenFolderControlView>();
		}
	}
}