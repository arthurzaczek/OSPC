// OSPC - Open Software Plagiarism Checker
// Copyright(C) 2015 Arthur Zaczek at the UAS Technikum Wien


// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

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
