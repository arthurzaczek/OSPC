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
