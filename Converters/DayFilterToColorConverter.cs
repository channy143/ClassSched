using System.Globalization;

namespace ClassSched.Converters;

public class DayFilterToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selectedDay = value as DayOfWeek?;
        var buttonDay = parameter as DayOfWeek?;

        // Selected day gets purple background
        if (selectedDay.HasValue && buttonDay.HasValue && selectedDay.Value == buttonDay.Value)
        {
            return Color.FromArgb("#512BD4"); // Purple
        }

        // Unselected days get light gray background
        return Color.FromArgb("#F0F0F0");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
