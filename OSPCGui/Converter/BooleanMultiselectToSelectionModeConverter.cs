using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace OSPCGui.Converter
{
    /// <summary>
    /// Converts a bool to the SelectionMode enum. True = Extended, False = Single
    /// </summary>
    [ValueConversion(typeof(bool), typeof(SelectionMode))]
    public class BooleanMultiselectToSelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {
            return value is bool && (bool)value ? SelectionMode.Extended : SelectionMode.Single;
        }

        public object ConvertBack(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
