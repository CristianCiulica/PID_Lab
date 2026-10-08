using System;
using System.Globalization;
using Avalonia.Data.Converters;
namespace Framework.Converters
{
    public sealed class ImagePresenceConverter : IValueConverter
    {
        public object Convert(object value,Type targetType,object parameter,CultureInfo culture) =>
            parameter?.ToString()=="Empty" ? value==null : value!=null;
        public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) => throw new NotSupportedException();
    }
}
