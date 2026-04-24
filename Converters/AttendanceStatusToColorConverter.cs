using ClassSched.Models;
using System.Globalization;

namespace ClassSched.Converters;

public class AttendanceStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Attended => Color.FromArgb("#4CAF50"), // Green
                AttendanceStatus.Missed => Color.FromArgb("#F44336"), // Red
                AttendanceStatus.Excused => Color.FromArgb("#FF9800"), // Orange
                AttendanceStatus.Late => Color.FromArgb("#FFC107"), // Yellow
                _ => Color.FromArgb("#9E9E9E") // Gray
            };
        }

        // No attendance record - transparent/gray
        return Color.FromArgb("#E0E0E0");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
