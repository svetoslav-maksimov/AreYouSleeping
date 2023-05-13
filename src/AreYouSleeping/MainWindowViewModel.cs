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
using System.Globalization;

namespace AreYouSleeping;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<CultureInfo> AvailableLanguages { get; set; }

    public ObservableCollection<string> CustomBrowserPatterns { get; set; }

    [ObservableProperty]
    private ObservableCollection<TimeSpan> _timerOptions;

    [ObservableProperty]
    private ObservableCollection<ActionMode> _actionModes;

    [ObservableProperty]
    private bool _isTimerRunning = false;

    [ObservableProperty]
    private TimeSpan _selectedTimerOption;

    [ObservableProperty]
    private ActionMode _selectedActionMode;

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

    [ObservableProperty]
    private CultureInfo _selectedCulture;

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

        TimerOptions = new ObservableCollection<TimeSpan>();
        ActionModes = new ObservableCollection<ActionMode>();

        SetupActionModesAndTimerOptions();

        AvailableLanguages = new ObservableCollection<CultureInfo>()
        {
            CultureInfo.GetCultureInfo("en-US"),
            CultureInfo.GetCultureInfo("bg-BG"),
        };

        CustomBrowserPatterns = new ObservableCollection<string>();

        StartTimerCommand = new RelayCommand(StartTimerExecute, () => { return !IsTimerRunning; });
        StopTimerCommand = new RelayCommand(StopTimerExecute, () => { return IsTimerRunning; });
        DeleteCustomBrowserPatternCommand = new RelayCommand<string>(DeleteCustomBrowserPattern);

        _selectedActionMode = ActionMode.CloseBrowserTab;
        _selectedTimerOption = TimerOptions.First(t => t.TotalMinutes == 20);
        _selectedCulture = AvailableLanguages[0];

        _timeOfSleep = "";
        _sleepTimer.Tick += SleepTimer_Tick;
        _displayTimer.Interval = TimeSpan.FromSeconds(1);
        _displayTimer.Tick += DisplayTimer_Tick;

        LoadFromSettings();
    }

    private void SetupActionModesAndTimerOptions()
    {
        TimerOptions = new ObservableCollection<TimeSpan>
        {
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(20),
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(40),
            TimeSpan.FromMinutes(50),
            TimeSpan.FromMinutes(60),
            TimeSpan.FromMinutes(90),
            TimeSpan.FromMinutes(120),
            TimeSpan.FromMinutes(180),
            TimeSpan.FromMinutes(240)
        };

        ActionModes = new ObservableCollection<ActionMode>(Enum.GetValues<ActionMode>());
    }

    private void LoadFromSettings()
    {
        SelectedActionMode = _appSettings.SelectedActionMode;

        SelectedTimerOption = TimerOptions.FirstOrDefault(x => x == _appSettings.TimerDuration);
        if (SelectedTimerOption == default(TimeSpan))
            SelectedTimerOption = TimerOptions.OrderBy(t => Math.Abs(t.TotalMilliseconds - _appSettings.TimerDuration.TotalMilliseconds)).First();

        BrowserOptionsNetflix = _appSettings.BrowserTabOptions.Netflix;
        BrowserOptionsHbo = _appSettings.BrowserTabOptions.Hbo;
        BrowserOptionsPrime = _appSettings.BrowserTabOptions.Prime;
        BrowserOptionsCustom = _appSettings.BrowserTabOptions.Custom;

        SelectedCulture = AvailableLanguages.FirstOrDefault(l => l.Name == _appSettings.Language) ?? AvailableLanguages[0];

        CustomBrowserPatterns = new ObservableCollection<string>(_appSettings.BrowserTabOptions.CustomBrowserPatterns);
    }

    private async Task SaveToSettings()
    {
        System.Diagnostics.Debug.WriteLine("SaveToSettings()");

        var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        var inputJson = await File.ReadAllTextAsync(path);
        var jNode = JsonNode.Parse(inputJson)!;
        var settings = jNode!["AppSettings"]!.Deserialize<AppSettings>()!;

        settings.Language = SelectedCulture.Name;
        settings.SelectedActionMode = SelectedActionMode;
        settings.TimerDuration = SelectedTimerOption;
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
            nameof(SelectedCulture),
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

        if (e.PropertyName == nameof(SelectedCulture))
            ChangeLanguage();
    }

    private void ChangeLanguage()
    {
        ((App)System.Windows.Application.Current).SetupLocalization(SelectedCulture.Name);
        SetupActionModesAndTimerOptions();

        var tempActionMode = SelectedActionMode;
        SelectedActionMode = ActionModes.First(x => x != tempActionMode);
        SelectedActionMode = tempActionMode;

        var tempTimerOption = SelectedTimerOption;
        SelectedTimerOption = TimerOptions.First(x => x != tempTimerOption);
        SelectedTimerOption = tempTimerOption;
    }

    private void StartTimerExecute()
    {
        IsTimerRunning = true;
        StartTimerCommand.NotifyCanExecuteChanged();
        StopTimerCommand.NotifyCanExecuteChanged();

        ElapsedTime = TimeSpan.Zero;

        TimeOfSleep = "";

        _sleepTimer.Interval = SelectedTimerOption;
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

    partial void OnSelectedActionModeChanged(ActionMode value)
    {
        switch (value)
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
            switch (SelectedActionMode)
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

public enum ActionMode
{
    CloseBrowserTab,
    CloseBrowserProcess,
    PutInSleep,
    Shutdown
}
