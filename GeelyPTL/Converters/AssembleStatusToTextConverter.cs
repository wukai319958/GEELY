using System;
using System.Globalization;
using System.Windows.Data;
using DataAccess.AssemblyIndicating;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 装配指引状态到其文本的转换器。
    /// </summary>
    class AssembleStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AssembleStatus assortingStatus = (AssembleStatus)value;
            if (assortingStatus == AssembleStatus.New)
                return "未开始";
            else if (assortingStatus == AssembleStatus.Assembling)
                return "指引中";
            else if (assortingStatus == AssembleStatus.Finished)
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