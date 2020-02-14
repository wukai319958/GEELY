using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GeelyPTL
{
    /// <summary>
    /// 窗体控制条。
    /// </summary>
    public partial class WindowControlBarUserControl
    {
        /// <summary>
        /// 初始化窗体控制条。
        /// </summary>
        public WindowControlBarUserControl()
        {
            this.InitializeComponent();
        }

        private void borderMin_MouseEnter(object sender, MouseEventArgs e)
        {
            this.pathMin.Fill = Brushes.Blue;
        }

        private void borderMax_MouseEnter(object sender, MouseEventArgs e)
        {
            this.pathMax.Fill = Brushes.Blue;
        }

        private void borderClose_MouseEnter(object sender, MouseEventArgs e)
        {
            this.pathClose.Fill = Brushes.Red;
        }

        private void borderMin_MouseLeave(object sender, MouseEventArgs e)
        {
            this.pathMin.Fill = Brushes.Black;
        }

        private void borderMax_MouseLeave(object sender, MouseEventArgs e)
        {
            this.pathMax.Fill = Brushes.Black;
        }

        private void borderClose_MouseLeave(object sender, MouseEventArgs e)
        {
            this.pathClose.Fill = Brushes.Black;
        }

        private void borderMin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.WindowState = WindowState.Minimized;
        }

        private void borderMax_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void borderClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }
    }
}