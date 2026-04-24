using System.Globalization;

namespace ClassSched.Converters;

public class BoolToIntConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                var trueValue = int.Parse(parts[0]);
                var falseValue = int.Parse(parts[1]);
                return boolValue ? trueValue : falseValue;
            }
        }
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
