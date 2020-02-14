using System;
using System.Globalization;
using System.Windows.Data;
using DataAccess.Config;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 料车状态到其文本的转换器。
    /// </summary>
    class CartStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CartStatus cartStatus = (CartStatus)value;
            switch (cartStatus)
            {
                case CartStatus.Free:
                    return "空闲";
                case CartStatus.WaitingAssorting:
                    return "已停靠";
                case CartStatus.Assorting:
                    return "分拣中";
                case CartStatus.Assorted:
                    return "分拣完成";
                case CartStatus.WaitingToBufferArea:
                    return "等待发往缓存区";
                case CartStatus.InCarriageToBufferArea:
                    return "向缓存区运输中";
                case CartStatus.ArrivedAtBufferArea:
                    return "缓存区";
                case CartStatus.NeedToWorkStation:
                    return "需要发往生产线";
                case CartStatus.WaitingToWorkStation:
                    return "等待发往生产线";
                case CartStatus.InCarriageToWorkStation:
                    return "向生产线运输中";
                case CartStatus.ArrivedAtWorkStation:
                    return "生产线边";
                case CartStatus.Indicating:
                    return "装配中";

                default:
                    return cartStatus.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}