using System;
using System.Globalization;
using System.Windows.Data;

namespace AreYouSleeping.Converters;

[ValueConversion(typeof(TimeSpan), typeof(string))]
internal class TimeSpanToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(string)) throw new InvalidOperationException("The target must be a string");
        if (value == null) return string.Empty;
        if (value.GetType() != typeof(TimeSpan)) throw new InvalidOperationException("The source type must be TimeSpan");

        var duration = (TimeSpan)value;

        if (duration.TotalMinutes < 1440)
        {
            return $"{duration.TotalMinutes} {App.Current.TryFindResource("Main_Duration_Minutes")}";
        }

        return $"{duration.TotalHours} {App.Current.TryFindResource("Main_Duration_Hours")}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
