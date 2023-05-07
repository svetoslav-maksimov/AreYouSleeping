using System;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AreYouSleeping;

public partial class MainWindowViewModel : ObservableObject
{
    private bool _isTimerRunning = false;

    public ObservableCollection<TimerOption> TimerOptions { get; set; }
    public ObservableCollection<ActionModeOption> ActionModes { get; set; }

    public ObservableCollection<string> CustomBrowserPatterns { get; set; }


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

    public RelayCommand StartTimerCommand { get; init; }
    public RelayCommand StopTimerCommand { get; init; }
    public RelayCommand<string> DeleteCustomBrowserPatternCommand { get; init; }
    public RelayCommand ApplySettingsCommand { get; set; }
    public RelayCommand CancelSettingsCommand { get; set; }

    public MainWindowViewModel()
    {

        TimerOptions = new ObservableCollection<TimerOption>
        {
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

        SelectedTimerOption = TimerOptions.First(t => t.Duration.TotalMinutes == 20);

        ActionModes = new ObservableCollection<ActionModeOption>(Enum.GetValues<ActionMode>().Select(x => ActionModeOption.FromMode(x)));
        _selectedActionMode = ActionModes.First(x => x.ActionMode == ActionMode.CloseBrowserTab);

        CustomBrowserPatterns = new ObservableCollection<string>();

        StartTimerCommand = new RelayCommand(StartTimerExecute, () => { return !_isTimerRunning; });
        StopTimerCommand = new RelayCommand(StopTimerExecute, () => { return _isTimerRunning; });
        DeleteCustomBrowserPatternCommand = new RelayCommand<string>(DeleteCustomBrowserPattern);
        ApplySettingsCommand = new RelayCommand(ApplySettings);
        CancelSettingsCommand = new RelayCommand(CancelSettings);
    }

    private void StartTimerExecute()
    {
        _isTimerRunning = true;
        StartTimerCommand.NotifyCanExecuteChanged();
        StopTimerCommand.NotifyCanExecuteChanged();
    }

    private void StopTimerExecute()
    {
        _isTimerRunning = false;
        StartTimerCommand.NotifyCanExecuteChanged();
        StopTimerCommand.NotifyCanExecuteChanged();
    }

    private void DeleteCustomBrowserPattern(string? pattern)
    {
        if (!string.IsNullOrEmpty(pattern))
        {
            CustomBrowserPatterns.Remove(pattern);
        }
    }

    private void ApplySettings()
    {
        // TODO
    }

    private void CancelSettings()
    {
        // TODO
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