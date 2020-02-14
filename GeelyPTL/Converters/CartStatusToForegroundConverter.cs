using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataAccess.Config;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 料车状态到其文本前景色的转换器。
    /// </summary>
    class CartStatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CartStatus cartStatus = (CartStatus)value;
            switch (cartStatus)
            {
                case CartStatus.Free:
                    return Brushes.ForestGreen;
                case CartStatus.WaitingAssorting:
                    return Brushes.Firebrick;
                case CartStatus.Assorting:
                    return Brushes.DarkGoldenrod;
                case CartStatus.Assorted:
                    return Brushes.ForestGreen;
                case CartStatus.WaitingToBufferArea:
                    return Brushes.Gold;
                case CartStatus.InCarriageToBufferArea:
                    return Brushes.DarkGoldenrod;
                case CartStatus.ArrivedAtBufferArea:
                    return Brushes.ForestGreen;
                case CartStatus.NeedToWorkStation:
                    return Brushes.Firebrick;
                case CartStatus.WaitingToWorkStation:
                    return Brushes.Gold;
                case CartStatus.InCarriageToWorkStation:
                    return Brushes.DarkGoldenrod;
                case CartStatus.ArrivedAtWorkStation:
                    return Brushes.Firebrick;
                case CartStatus.Indicating:
                    return Brushes.DarkGoldenrod;

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