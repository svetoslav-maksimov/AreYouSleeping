using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Automation;

namespace AreYouSleeping.Automation;

public class BrowserAutomation
{
    private ILogger<ShutdownAutomation> _logger;

    public BrowserAutomation(ILogger<ShutdownAutomation> logger)
    {
        _logger = logger;
    }

    public bool CloseChromeTabs(string[] tabNamePatterns)
    {
        var tabNameRegexes = tabNamePatterns.Select(s =>
            new Regex(s, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(50)));

        var uiRoot = AutomationElement.RootElement;
        if (uiRoot == null) return false;

        _logger.LogDebug($"Found the root desktop element.");

        var chromeClassNamePropCondition = new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1");
        var chromeMainWindows = uiRoot.FindAll(TreeScope.Children, chromeClassNamePropCondition);

        if (chromeMainWindows.Count == 0)
        {
            _logger.LogWarning("Did not find any Chrome windows.");
            return false;
        }

        _logger.LogDebug($"Found {chromeMainWindows.Count} Chrome windows.");

        var countClosed = 0;

        foreach (AutomationElement chromeWindow in chromeMainWindows)
        {
            _logger.LogDebug($"Chrome window title: {chromeWindow.Current.Name}");

            var allTabs = chromeWindow.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));

            if (allTabs.Count == 0)
            {
                _logger.LogWarning($"Did not find any Chrome tabs in window {chromeWindow.Current.Name} (pid: {chromeWindow.Current.ProcessId}).");

                // maybe it's fullscreen?
                if (tabNameRegexes.Any(regex => regex.IsMatch(chromeWindow.Current.Name)))
                {
                    var windowPattern = chromeWindow.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern;
                    if (windowPattern == null)
                    {
                        _logger.LogWarning($"Unable to close window {chromeWindow.Current.Name}.");
                        continue;
                    }
                    else
                    {
                        windowPattern.Close();
                        _logger.LogInformation($"Closed window {chromeWindow.Current.Name}");
                        countClosed++;
                    }
                }

                continue;
            }

            _logger.LogDebug($"Found {allTabs.Count} tabs in window {chromeWindow.Current.Name} (pid: {chromeWindow.Current.ProcessId}).");

            foreach (AutomationElement tab in allTabs)
            {
                _logger.LogDebug($"Chrome tab title: {tab.Current.Name}");

                if (tabNameRegexes.Any(regex => regex.IsMatch(tab.Current.Name)))
                {
                    var closeButton = tab.FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, "Close"));

                    if (closeButton == null)
                    {
                        _logger.LogWarning($"Could not find the close button for tab {tab.Current.Name}.");
                        continue;
                    }

                    var invokePattern = closeButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                    if (invokePattern == null)
                    {
                        _logger.LogWarning($"Unable to invoke close button for tab {tab.Current.Name}.");
                        continue;
                    }
                    invokePattern.Invoke();
                    _logger.LogInformation($"Invoked close button for tab {tab.Current.Name}");
                    countClosed++;
                }
                else
                {
                    _logger.LogDebug($"Tab title: {tab.Current.Name} did not match any pattern");
                }
            }
        }

        return countClosed > 0;
    }

    public bool CloseBrowserProcesses(string[] processNames)
    {
        var allProcesses = processNames.SelectMany(p => Process.GetProcessesByName(p)).ToArray();
        _logger.LogDebug($"Got {allProcesses.Length} processes to kill");

        var allOk = true;

        foreach (var process in allProcesses)
        {
            try
            {
                process.Kill();
                _logger.LogInformation($"Killed process {process.Id}");
            }
            catch (Exception exc)
            {
                allOk = false;
                _logger.LogError(exc, "Could not kill process {pid}", process.Id);
            }
        }

        return allOk;
    }
}
