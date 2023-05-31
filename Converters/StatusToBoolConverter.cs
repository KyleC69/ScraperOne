// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// StatusToBoolConverter.csStatusToBoolConverter.cs032320233:28 AM


using System.ComponentModel;
using System.Globalization;
using Avalonia.Data.Converters;
using ScraperOne.Properties;

namespace ScraperOne.Converters;

[TypeConverter(typeof(bool))]
public class StatusToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        return boolValue
            ? string.Format(CultureInfo.CurrentCulture, Resources.Online)
            : string.Format(CultureInfo.CurrentCulture, Resources.Offline);
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}