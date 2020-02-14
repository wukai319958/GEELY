using System;
using System.Globalization;
using System.Windows.Data;

namespace AssortingKanban.Converters
{
    /// <summary>
    /// 是否大件到其文本的转换器。
    /// </summary>
    class IsBigToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "√";
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}