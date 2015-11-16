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
