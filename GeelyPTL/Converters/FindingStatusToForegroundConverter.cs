using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataAccess.CartFinding;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 料车配送状态到其文本前景色的转换器。
    /// </summary>
    class FindingStatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FindingStatus findingStatus = (FindingStatus)value;
            switch (findingStatus)
            {
                case FindingStatus.New:
                    return Brushes.Firebrick;
                case FindingStatus.NeedDisplay:
                    return Brushes.Gold;
                case FindingStatus.Displaying:
                    return Brushes.Gold;
                case FindingStatus.NeedBlink:
                    return Brushes.DarkGoldenrod;
                case FindingStatus.Blinking:
                    return Brushes.DarkGoldenrod;
                case FindingStatus.NeedClear:
                    return Brushes.DarkGoldenrod;
                case FindingStatus.Finished:
                    return Brushes.ForestGreen;

                default:
                    return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}