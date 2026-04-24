using ClassSched.Models;
using System.Globalization;

namespace ClassSched.Converters;

public class ClassToAttendanceIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Simplified - returns plus sign to indicate attendance can be marked
        // The actual attendance status is managed by the ViewModel and commands
        return "+";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
