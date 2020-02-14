using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeelyPTL.Converters
{
    /// <summary>
    /// 运行状态到其图标的转换器。
    /// </summary>
    class IsRunningToImageSourceConverter : IValueConverter
    {
        static readonly ImageSource RunningImageSource = new BitmapImage(new Uri("../Resources/Running.png", UriKind.Relative));
        static readonly ImageSource StopImageSource = new BitmapImage(new Uri("../Resources/Stop.png", UriKind.Relative));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRunning = (bool)value;
            if (isRunning)
                return IsRunningToImageSourceConverter.RunningImageSource;
            else
                return IsRunningToImageSourceConverter.StopImageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}