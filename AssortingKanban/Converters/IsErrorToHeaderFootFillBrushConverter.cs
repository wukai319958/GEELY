using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AssortingKanban.Converters
{
    /// <summary>
    /// 与服务器连接状态到页眉页脚颜色的转换器。
    /// </summary>
    class IsErrorToHeaderFootFillBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Brushes.Firebrick;
            else
                return new SolidColorBrush(Color.FromRgb(0x04, 0x58, 0x9E));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}