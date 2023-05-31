// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// IntToDisplayValueConverter.csIntToDisplayValueConverter.cs032320233:28 AM


using System.Globalization;
using Avalonia.Data.Converters;

namespace ScraperOne.Converters;

public class IntToDisplayValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var number = (int)value;
        return number != 0 ? number : string.Empty;
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var displayValue = value as string;
        return string.IsNullOrEmpty(displayValue) ? 0 : int.Parse(displayValue, CultureInfo.CurrentCulture);
    }
}