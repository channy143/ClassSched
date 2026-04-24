using System.Globalization;

namespace ClassSched.Converters;

public class DayFilterToTextColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selectedDay = value as DayOfWeek?;
        var buttonDay = parameter as DayOfWeek?;

        // Selected day gets white text
        if (selectedDay.HasValue && buttonDay.HasValue && selectedDay.Value == buttonDay.Value)
        {
            return Colors.White;
        }

        // Unselected days get dark text
        return Color.FromArgb("#333333");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
