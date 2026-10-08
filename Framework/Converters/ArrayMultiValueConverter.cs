using System;
using Avalonia.Data.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Framework.Converters
{
    public class ArrayMultiValueConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToArray();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return (object[])value;
        }
    }
}
