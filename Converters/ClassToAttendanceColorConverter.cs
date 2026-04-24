using ClassSched.Models;
using System.Globalization;

namespace ClassSched.Converters;

public class ClassToAttendanceColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Simplified - returns gray placeholder since converters can't do async DB calls
        // The actual attendance status is managed by the ViewModel and commands
        return Color.FromArgb("#E0E0E0");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
