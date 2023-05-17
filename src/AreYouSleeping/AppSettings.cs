using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AreYouSleeping;

public class AppSettings
{
    public string Language { get; set; } = "en-US";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActionMode SelectedActionMode { get; set; } = ActionMode.CloseBrowserTab;

    public TimeSpan TimerDuration { get; set; } = TimeSpan.FromMinutes(20);

    public AppSettingsBrowserTabOptions BrowserTabOptions { get; set; } = new AppSettingsBrowserTabOptions();

    public bool IsShowingElapsedTime { get; set; } = false;
}

public class AppSettingsBrowserTabOptions
{
    public bool Netflix { get; set; } = true;
    public bool Hbo { get; set; } = true;
    public bool Prime { get; set; } = true;
    public bool Custom { get; set; } = false;
    public List<string> CustomBrowserPatterns { get; set; } = new List<string>();
}
