using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace JinoOrder.Presentation.Converters;

/// <summary>
/// Polyfill for ObjectConverters.Equal which was added in Avalonia 11.1
/// </summary>
public class EqualConverter : IValueConverter
{
    public static readonly EqualConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Equals(value, parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
