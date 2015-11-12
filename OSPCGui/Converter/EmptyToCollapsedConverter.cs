using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace OSPCGui.Converter
{
    /// <summary>
    /// Return Visibility.Collapsed when a IEnumerable or string is empty
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class EmptyToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var enumerable = value as IEnumerable;
            var str = value as string;
            if (enumerable != null)
            {
                return enumerable.GetEnumerator().MoveNext()
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            else if (str != null)
            {
                return string.IsNullOrEmpty(value as string)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
