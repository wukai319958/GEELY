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
    class AssortingStatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AssortingStatus assortingStatus = (AssortingStatus)value;
            if (assortingStatus == AssortingStatus.None)
                return Brushes.Firebrick;
            else if (assortingStatus == AssortingStatus.Assorting)
                return Brushes.DarkGoldenrod;
            else if (assortingStatus == AssortingStatus.WaitingConfirm)
                return Brushes.LightSeaGreen;
            else if (assortingStatus == AssortingStatus.Finished)
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