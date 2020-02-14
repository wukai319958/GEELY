using System;
using System.Globalization;
using System.Windows.Data;
using DataAccess.Assorting;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 分拣状态到其文本的转换器。
    /// </summary>
    class PickStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PickStatus pickStatus = (PickStatus)value;
            if (pickStatus == PickStatus.New)
                return "待拣选";
            else if (pickStatus == PickStatus.Picking)
                return "分拣中";
            else if (pickStatus == PickStatus.Finished)
                return "已完成";
            else
                return pickStatus.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}