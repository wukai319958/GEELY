using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataAccess.Assorting;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 分拣状态到其文本前景色的转换器。
    /// </summary>
    class PickStatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PickStatus pickStatus = (PickStatus)value;
            if (pickStatus == PickStatus.New)
                return Brushes.Firebrick;
            else if (pickStatus == PickStatus.Picking)
                return Brushes.DarkGoldenrod;
            else if (pickStatus == PickStatus.Finished)
                return Brushes.ForestGreen;
            else
                return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}