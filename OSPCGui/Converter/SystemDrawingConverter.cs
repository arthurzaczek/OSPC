using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace OSPCGui.Converter
{
    /// <summary>
    /// Converts various System.Drawing types to their WPF equivalents
    /// </summary>
    /// <remarks>
    /// In this project, there is no need for this converter. So there is no System.Drawing reference. 
    /// It's just a example
    /// </remarks>
    [ValueConversion(typeof(object), typeof(object))]
    public class SystemDrawingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Binding.DoNothing;
            var type = value.GetType();

            //if (type == typeof(System.Drawing.Point))
            //{
            //    var p = (System.Drawing.Point)value;
            //    return new System.Windows.Thickness(p.X, p.Y, 0, 0);
            //}
            //else if (type == typeof(System.Drawing.PointF))
            //{
            //    var p = (System.Drawing.PointF)value;
            //    return new System.Windows.Thickness(p.X, p.Y, 0, 0);
            //}
            //else if (type == typeof(System.Drawing.Rectangle))
            //{
            //    var p = (System.Drawing.Rectangle)value;
            //    return new System.Windows.Thickness(p.X, p.Y, p.Right, p.Bottom);
            //}
            //else if (type == typeof(System.Drawing.RectangleF))
            //{
            //    var p = (System.Drawing.RectangleF)value;
            //    return new System.Windows.Thickness(p.X, p.Y, p.Right, p.Bottom);
            //}
            //else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
