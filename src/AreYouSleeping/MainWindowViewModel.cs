using System;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Windows.Threading;
using System.Diagnostics;
using AreYouSleeping.Automation;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AreYouSleeping;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<TimerOption> TimerOptions { get; set; }
    public ObservableCollection<ActionModeOption> ActionModes { get; set; }

    public ObservableCollection<string> CustomBrowserPatterns { get; set; }

    [ObservableProperty]
    private bool _isTimerRunning = false;

    [ObservableProperty]
    private TimerOption _selectedTimerOption;

    [ObservableProperty]
    private ActionModeOption _selectedActionMode;

    [ObservableProperty]
    private bool _browserOptionsVisibility = true;

    [ObservableProperty]
    private bool _browserOptionsNetflix = true;

    [ObservableProperty]
    private bool _browserOptionsHbo = true;

    [ObservableProperty]
    private bool _browserOptionsPrime = true;

    [ObservableProperty]
    private bool _browserOptionsCustom = false;

    [ObservableProperty]
    private string _customBrowserNewPattern = "";

    [ObservableProperty]
    private TimeSpan _elapsedTime;

    [ObservableProperty]
    private string _timeOfSleep;

    public RelayCommand StartTimerCommand { get; init; }
    public RelayCommand StopTimerCommand { get; init; }
    public RelayCommand<string> DeleteCustomBrowserPatternCommand { get; init; }

    private readonly AppSettings _appSettings;
    private readonly DebounceDispatcher _debounceTimer = new DebounceDispatcher();

    private readonly DispatcherTimer _sleepTimer = new DispatcherTimer();
    private readonly DispatcherTimer _displayTimer = new DispatcherTimer();
    private readonly Stopwatch _sleepStopwatch = new Stopwatch();

    private readonly AwakePromptFactory _awakePromptFactory;

    private readonly ShutdownAutomation _shutdownAutomation;
    private readonly BrowserAutomation _browserAutomation;

    public MainWindowViewModel(IOptions<AppSettings> options, AwakePromptFactory awakePromptFactory,
        ShutdownAutomation shutdownAutomation, BrowserAutomation browserAutomation)
    {
        _appSettings = options.Value;
        _shutdownAutomation = shutdownAutomation;
        _browserAutomation = browserAutomation;
        _awakePromptFactory = awakePromptFactory;

        TimerOptions = new ObservableCollection<TimerOption>
        {
            TimerOption.FromDuration(TimeSpan.FromSeconds(10)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(1)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(10)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(15)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(20)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(30)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(40)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(50)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(60)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(90)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(120)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(180)),
            TimerOption.FromDuration(TimeSpan.FromMinutes(240))
        };


        ActionModes = new ObservableCollection<ActionModeOption>(Enum.GetValues<ActionMode>().Select(x => ActionModeOption.FromMode(x)));

        CustomBrowserPatterns = new ObservableCollection<string>();

        StartTimerCommand = new RelayCommand(StartTimerExecute, () => { return !IsTimerRunning; });
        StopTimerCommand = new RelayCommand(StopTimerExecute, () => { return IsTimerRunning; });
        DeleteCustomBrowserPatternCommand = new RelayCommand<string>(DeleteCustomBrowserPattern);

        _selectedActionMode = ActionModes.First(x => x.ActionMode == ActionMode.CloseBrowserTab);
        _selectedTimerOption = TimerOptions.First(t => t.Duration.TotalMinutes == 20);

        _timeOfSleep = "";
        _sleepTimer.Tick += SleepTimer_Tick;
        _displayTimer.Interval = TimeSpan.FromSeconds(1);
        _displayTimer.Tick += DisplayTimer_Tick;

        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        SelectedActionMode =
            ActionModes.FirstOrDefault(x => x.ActionMode == _appSettings.SelectedActionMode)
            ?? ActionModes.First(x => x.ActionMode == ActionMode.CloseBrowserTab);

        SelectedTimerOption = TimerOptions.FirstOrDefault(x => x.Duration == _appSettings.TimerDuration)
            ?? TimerOptions.OrderBy(t => Math.Abs(t.Duration.TotalMilliseconds - _appSettings.TimerDuration.TotalMilliseconds)).First();

        BrowserOptionsNetflix = _appSettings.BrowserTabOptions.Netflix;
        BrowserOptionsHbo = _appSettings.BrowserTabOptions.Hbo;
        BrowserOptionsPrime = _appSettings.BrowserTabOptions.Prime;
        BrowserOptionsCustom = _appSettings.BrowserTabOptions.Custom;

        CustomBrowserPatterns = new ObservableCollection<string>(_appSettings.BrowserTabOptions.CustomBrowserPatterns);
    }

    private async Task SaveToSettings()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        var inputJson = await File.ReadAllTextAsync(path);
        var jNode = JsonNode.Parse(inputJson)!;
        var settings = jNode!["AppSettings"]!.Deserialize<AppSettings>()!;

        settings.SelectedActionMode = SelectedActionMode.ActionMode;
        settings.TimerDuration = SelectedTimerOption.Duration;
        settings.BrowserTabOptions.Netflix = BrowserOptionsNetflix;
        settings.BrowserTabOptions.Hbo = BrowserOptionsHbo;
        settings.BrowserTabOptions.Prime = BrowserOptionsPrime;
        settings.BrowserTabOptions.Custom = BrowserOptionsCustom;
        settings.BrowserTabOptions.CustomBrowserPatterns = CustomBrowserPatterns.ToList();

        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        var updatedJsonString = JsonSerializer.Serialize(settings, serializerOptions);
        jNode["AppSettings"] = JsonNode.Parse(updatedJsonString);

        var outputJson = jNode.ToJsonString(serializerOptions);

        if (inputJson != outputJson)
        {
            await File.WriteAllTextAsync(path, outputJson);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        string[] persistedPropertyNames = new string[]
        {
            nameof(SelectedActionMode),
            nameof(SelectedTimerOption),
            nameof(BrowserOptionsNetflix),
            nameof(BrowserOptionsPrime),
            nameof(BrowserOptionsHbo),
            nameof(BrowserOptionsCustom),
            nameof(CustomBrowserPatterns),
        };

        if (persistedPropertyNames.Contains(e.PropertyName))
        {
            _debounceTimer.Debounce(100, async (p) =>
            {
                await SaveToSettings();
            });
        }
    }

    private void StartTimerExecute()
    {
        IsTimerRunning = true;
        StartTimerCommand.NotifyCanExecuteChanged();
        StopTimerCommand.NotifyCanExecuteChanged();

        ElapsedTime = TimeSpan.Zero;

        TimeOfSleep = "";

        _sleepTimer.Interval = SelectedTimerOption.Duration;
        _sleepTimer.Stop();
        _sleepTimer.Start();

        _displayTimer.Start();

        _sleepStopwatch.Restart();
    }

    private void StopTimerExecute()
    {
        IsTimerRunning = false;
        StartTimerCommand.NotifyCanExecuteChanged();
        StopTimerCommand.NotifyCanExecuteChanged();

        _sleepTimer.Stop();
        _displayTimer.Stop();
        _sleepStopwatch.Stop();
    }

    private void DeleteCustomBrowserPattern(string? pattern)
    {
        if (!string.IsNullOrEmpty(pattern))
        {
            CustomBrowserPatterns.Remove(pattern);
        }
    }

    partial void OnSelectedActionModeChanged(ActionModeOption value)
    {
        switch (value.ActionMode)
        {
            case ActionMode.CloseBrowserTab: BrowserOptionsVisibility = true; break;
            default: BrowserOptionsVisibility = false; break;
        }
    }

    partial void OnCustomBrowserNewPatternChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            CustomBrowserPatterns.Add(value);
            CustomBrowserNewPattern = string.Empty;
        }
    }

    private void DisplayTimer_Tick(object? sender, EventArgs e)
    {
        ElapsedTime = _sleepStopwatch.Elapsed;
    }

    private async void SleepTimer_Tick(object? sender, EventArgs e)
    {
        ElapsedTime = _sleepStopwatch.Elapsed;
        TimeOfSleep = DateTime.Now.ToShortTimeString();
        StopTimerExecute();

        var promptResult = await _awakePromptFactory.AskForConfirmation();
        if (promptResult == true)
        {
            // yes, sleeping
            switch (SelectedActionMode.ActionMode)
            {
                case ActionMode.CloseBrowserTab:
                    var patterns = new List<string>();
                    if (BrowserOptionsNetflix) patterns.Add("Netflix.*");
                    if (BrowserOptionsHbo) patterns.Add("HBO Max.*");
                    if (BrowserOptionsPrime) patterns.Add("Prime Video.*");
                    if (BrowserOptionsCustom) patterns.AddRange(CustomBrowserPatterns);

                    await Task.Run(() =>
                    {
                        _browserAutomation.CloseChromeTabs(patterns.ToArray());
                    });
                    break;

                case ActionMode.CloseBrowserProcess:
                    _browserAutomation.CloseBrowserProcesses(new[] { "chrome" });
                    break;

                case ActionMode.PutInSleep:
                    _shutdownAutomation.Sleep();
                    break;

                case ActionMode.Shutdown:
                    _shutdownAutomation.Shutdown();
                    break;

                default:
                    break;
            }
        }
        else
        {
            // no, restart the timer
            StartTimerExecute();
        }
    }
}

public class TimerOption
{
    public string Label { get; set; } = "";
    public TimeSpan Duration { get; set; }

    public static TimerOption FromDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1440)
        {
            return new TimerOption { Duration = duration, Label = $"{duration.TotalMinutes} minutes" };
        }

        return new TimerOption { Duration = duration, Label = $"{duration.TotalHours} hours" };
    }
}

public enum ActionMode
{
    CloseBrowserTab,
    CloseBrowserProcess,
    PutInSleep,
    Shutdown
}

public class ActionModeOption
{
    public ActionMode ActionMode { get; set; }
    public string Label { get; set; } = "";

    public static ActionModeOption FromMode(ActionMode mode)
    {
        var result = new ActionModeOption { ActionMode = mode };
        switch (mode)
        {
            case ActionMode.CloseBrowserTab: result.Label = "Close browser tab"; break;
            case ActionMode.CloseBrowserProcess: result.Label = "Close all browser processes"; break;
            case ActionMode.PutInSleep: result.Label = "Put computer in sleep mode"; break;
            case ActionMode.Shutdown: result.Label = "Shutdown computer"; break;
        }
        return result;
    }
}