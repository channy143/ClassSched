using System.Globalization;

namespace ClassSched.Converters;

public class ColorFromHexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hexColor)
        {
            try
            {
                return Color.FromArgb(hexColor);
            }
            catch
            {
                return Colors.Gray;
            }
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            return color.ToHex();
        }
        return "#808080";
    }
}
