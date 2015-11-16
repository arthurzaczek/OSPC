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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace OSPCGui.Converter
{
    public abstract class AbstractShadeConverter : IValueConverter
    {
        protected abstract Color ShadeColor(Color value, float amount = 0.5f);

        public object Convert(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Binding.DoNothing;
            var type = value.GetType();
            Color color;
            float amount;

            if (!(parameter is string) || !float.TryParse((string)parameter, NumberStyles.Any, CultureInfo.GetCultureInfo("en-us"), out amount))
            {
                amount = 0.5f;
            }

            if (type == typeof(string))
            {
                var str = (string)value;
                if (string.IsNullOrWhiteSpace(str)) return Binding.DoNothing;
                color = (Color)ColorConverter.ConvertFromString(str);
            }
            else if (type == typeof(Color))
            {
                color = (Color)value;
            }
            else
            {
                return value;
            }

            color = ShadeColor(color, amount);
            if (typeof(Color).IsAssignableFrom(targetType))
            {
                return color;
            }
            else if (typeof(Brush).IsAssignableFrom(targetType))
            {
                return new SolidColorBrush(color);
            }
            else
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType,
                            object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Makes a color lighter
    /// </summary>
    [ValueConversion(typeof(Color), typeof(Color))]
    public class LighterShadeConverter : AbstractShadeConverter
    {
        public static Color MakeLighter(Color value, float lighter = 0.5f)
        {
            return Color.FromScRgb(value.ScA,
                (1.0f - lighter) * value.ScR + lighter,
                (1.0f - lighter) * value.ScG + lighter,
                (1.0f - lighter) * value.ScB + lighter);
        }

        protected override Color ShadeColor(Color value, float lighter = 0.5f)
        {
            return MakeLighter(value, lighter);
        }
    }

    /// <summary>
    /// Makes a color darker
    /// </summary>
    [ValueConversion(typeof(Color), typeof(Color))]
    public class DarkerShadeConverter : AbstractShadeConverter
    {
        public static Color MakeDarker(Color value, float darker = 0.5f)
        {
            return Color.FromScRgb(value.ScA,
                (1.0f - darker) * value.ScR,
                (1.0f - darker) * value.ScG,
                (1.0f - darker) * value.ScB);
        }

        protected override Color ShadeColor(Color value, float lighter = 0.5f)
        {
            return MakeDarker(value, lighter);
        }
    }
}
