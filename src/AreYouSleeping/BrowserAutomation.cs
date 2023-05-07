using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Automation;

namespace AreYouSleeping;

public class BrowserAutomation
{
    private ILogger<BrowserAutomation> _logger;

    public BrowserAutomation(ILogger<BrowserAutomation> logger)
    {
        _logger = logger;
    }

    public bool CloseChromeTab()
    {
        var result = CloseChromeTabs(new[]
        {
            "Netflix.*",
            "HBO Max.*"
        });

        return result;
    }

    private bool CloseChromeTabs(string[] tabNamePatterns)
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
}
