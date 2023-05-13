using System;
using System.Globalization;
using System.Windows.Data;

namespace AreYouSleeping.Converters;

[ValueConversion(typeof(ActionMode), typeof(string))]

internal class ActionModeToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(string)) throw new InvalidOperationException("The target must be a string");
        if (value == null) return string.Empty;
        if (value.GetType() != typeof(ActionMode)) throw new InvalidOperationException("The source type must be ActionMode");

        var actionMode = (ActionMode)value;
        switch (actionMode)
        {
            case ActionMode.CloseBrowserTab: return App.Current.TryFindResource("Main_ActionMode_CloseBrowserTab").ToString()!;
            case ActionMode.CloseBrowserProcess: return App.Current.TryFindResource("Main_ActionMode_CloseBrowserProcess").ToString()!;
            case ActionMode.PutInSleep: return App.Current.TryFindResource("Main_ActionMode_PutInSleep").ToString()!;
            case ActionMode.Shutdown: return App.Current.TryFindResource("Main_ActionMode_Shutdown").ToString()!;
        }

        return Enum.GetName(actionMode)!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
