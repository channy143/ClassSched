using ClassSched.Models;
using System.Globalization;

namespace ClassSched.Converters;

public class AttendanceStatusToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Attended => "✓",
                AttendanceStatus.Missed => "✕",
                AttendanceStatus.Excused => "⊘",
                AttendanceStatus.Late => "⚠",
                _ => "?"
            };
        }

        return "○"; // No record
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
