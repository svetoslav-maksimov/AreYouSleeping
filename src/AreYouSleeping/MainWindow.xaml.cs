using Microsoft.Extensions.Logging;
using System;
using System.Windows;

namespace AreYouSleeping;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    public MainWindow(ILogger<MainWindow> logger)
    {
        MainWindowHandler.Instance = this;

        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        MainWindowHandler.Instance = null;
        base.OnClosed(e);
    }
}
