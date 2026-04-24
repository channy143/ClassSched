using System.Globalization;

namespace ClassSched.Converters;

public class FilterDayToTitleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selectedDay = value as DayOfWeek?;

        if (selectedDay.HasValue)
        {
            return $"{selectedDay.Value} Classes";
        }

        return "Today's Classes";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
