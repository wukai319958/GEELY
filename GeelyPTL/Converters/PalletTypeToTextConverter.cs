using System;
using System.Globalization;
using System.Windows.Data;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 箱型编码到其文本的转换器。
    /// </summary>
    class PalletTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string code = (string)value;
            switch (code)
            {
                case "01":
                    return "常规";
                case "02":
                    return "围板箱";
                case "03":
                    return "超长大件";

                default:
                    return code;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}