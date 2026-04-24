using System.Globalization;

namespace ClassSched.Converters;

public class IntToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 3)
            {
                var targetValue = int.Parse(parts[0]);
                var trueColor = parts[1];
                var falseColor = parts[2];
                
                return intValue == targetValue ? Color.FromArgb(trueColor) : Color.FromArgb(falseColor);
            }
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
