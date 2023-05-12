using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AreYouSleeping
{
    /// <summary>
    /// Interaction logic for AwakePrompt.xaml
    /// </summary>
    public partial class AwakePrompt : Window
    {
        private readonly DispatcherTimer _promptTimer = new DispatcherTimer();
        private readonly Stopwatch _promptStopwatch = new Stopwatch();
        private readonly AwakePromptViewModel _viewModel;
        private readonly TimeSpan TotalDuration = TimeSpan.FromSeconds(30);
        private readonly AppSettings _appSettings;

        public bool? PromptResult = null;

        public AwakePrompt(IOptions<AppSettings> options)
        {
            InitializeComponent();
            _appSettings = options.Value;

            _viewModel = new AwakePromptViewModel();
            DataContext = _viewModel;

            _promptTimer.Interval = TimeSpan.FromMilliseconds(100);
            _promptTimer.Tick += PromptTimer_Tick;

            Loaded += AwakePrompt_Loaded;
        }

        private void AwakePrompt_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Prompt = _appSettings.SleepPrompt;

            _viewModel.Remaining = $"{TotalDuration.Seconds} seconds";

            _promptTimer.Stop();
            _promptTimer.Start();
            _promptStopwatch.Restart();
        }

        private void PromptTimer_Tick(object? sender, EventArgs e)
        {
            if (_promptStopwatch.Elapsed > TotalDuration)
            {
                _promptTimer.Stop();
                _promptStopwatch.Stop();
           
                PromptResult = true;
                Close();
            }

            _viewModel.Remaining = $"{(TotalDuration - _promptStopwatch.Elapsed).Seconds} seconds";
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            PromptResult = true;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            PromptResult = false;
            Close();
        }
    }

    public class AwakePromptFactory
    {
        private readonly IOptions<AppSettings> _options;
        private AwakePrompt? _awakePrompt = null;

        public AwakePromptFactory(IOptions<AppSettings> options)
        {
            _options = options;
        }

        public Task<bool?> AskForConfirmation()
        {
            _awakePrompt = new AwakePrompt(_options);

            var promise = new TaskCompletionSource<bool?>();

            _awakePrompt.Show();
            _awakePrompt.Closed += (s, e) =>
            {
                promise.SetResult(_awakePrompt.PromptResult);
            };

            return promise.Task;
        }
    }

    public partial class AwakePromptViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _prompt = "Are you sleeping?";

        [ObservableProperty]
        private string _remaining = "30s";
    }
}
