using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AreYouSleeping;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(ILogger<MainWindow> logger, MainWindowViewModel mainWindowViewModel)
    {
        MainWindowHandler.Instance = this;

        DataContext = mainWindowViewModel;

        InitializeComponent();

        var imageSource = Imaging.CreateBitmapSourceFromHIcon(
           ProjectResources.SysTrayIcon.Handle,
           Int32Rect.Empty,
           BitmapSizeOptions.FromEmptyOptions());
        Icon = imageSource;
    }

    protected override void OnClosed(EventArgs e)
    {
        MainWindowHandler.Instance = null;
        base.OnClosed(e);
    }
}
