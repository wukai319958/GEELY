using System;
using System.Globalization;
using System.Windows.Data;
using DataAccess.Assorting;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 分拣状态到其文本的转换器。
    /// </summary>
    class AssortingStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AssortingStatus assortingStatus = (AssortingStatus)value;
            if (assortingStatus == AssortingStatus.None)
                return string.Empty;
            else if (assortingStatus == AssortingStatus.Assorting)
                return "分拣中";
            else if (assortingStatus == AssortingStatus.WaitingConfirm)
                return "等待确认";
            else if (assortingStatus == AssortingStatus.Finished)
                return "已完成";
            else
                return assortingStatus.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}