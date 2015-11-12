using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace OSPCGui.Converter
{
    /// <summary>
    /// Inverts a bool
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? !(bool?)value : null;
        }

        public object ConvertBack(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? !(bool?)value : null;
        }
    }
}
