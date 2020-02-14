using System;
using System.Globalization;
using System.Windows.Data;
using DataAccess.CartFinding;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 料车配送状态到其文本的转换器。
    /// </summary>
    class FindingStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FindingStatus findingStatus = (FindingStatus)value;
            switch (findingStatus)
            {
                case FindingStatus.New:
                    return "新任务";
                case FindingStatus.NeedDisplay:
                    return "需要亮灯指示";
                case FindingStatus.Displaying:
                    return "亮灯指示中";
                case FindingStatus.NeedBlink:
                    return "启动运输";
                case FindingStatus.Blinking:
                    return "运输开始";
                case FindingStatus.NeedClear:
                    return "运输中";
                case FindingStatus.Finished:
                    return "已完成";

                default:
                    return findingStatus.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}