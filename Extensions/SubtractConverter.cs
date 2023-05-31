using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace ScraperOne.Extensions;

public class SubtractConverter : MarkupExtension, IValueConverter
{
    public SubtractConverter(string baseValue)
    {
    }

    public double Value { get; set; }

    public object Convert(object baseValue, Type targetType, object parameter, CultureInfo culture)
    {
        var val = System.Convert.ToDouble(baseValue);
        // Change here if you want other operations
        return val - Value;
    }

    public object ConvertBack(object baseValue, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}