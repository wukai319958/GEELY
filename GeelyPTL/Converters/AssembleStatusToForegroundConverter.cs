using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataAccess.AssemblyIndicating;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 装配指引状态到其文本前景色的转换器。
    /// </summary>
    class AssembleStatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AssembleStatus assortingStatus = (AssembleStatus)value;
            if (assortingStatus == AssembleStatus.New)
                return Brushes.Firebrick;
            else if (assortingStatus == AssembleStatus.Assembling)
                return Brushes.DarkGoldenrod;
            else if (assortingStatus == AssembleStatus.Finished)
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