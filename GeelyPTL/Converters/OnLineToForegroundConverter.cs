using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 在线状态到图元前景色的转换器。
    /// </summary>
    class OnLineToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Brushes.ForestGreen;
            else
                return Brushes.Firebrick;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}