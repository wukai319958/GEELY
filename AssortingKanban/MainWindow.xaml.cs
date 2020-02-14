using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AssortingKanban
{
    /// <summary>
    /// 按巷道进行任务计数，监视托盘及其任务的实时状态。
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.Timer timerShow = null;

        /// <summary>
        /// 初始化主界面。
        /// </summary>
        public MainWindow()
        {
            LogOnWindow logOnWindow = new LogOnWindow();
            if (logOnWindow.ShowDialog() != true)
            {
                Application.Current.Shutdown();
                return;
            }

            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.viewModel.ClearDesignTimeDatas();

            this.viewModel.StartPollingThread();

            timerShow = new System.Windows.Forms.Timer();
            timerShow.Tick += timerShow_Tick;
            timerShow.Interval = 1000;
            timerShow.Start();
        }

        void timerShow_Tick(object sender, System.EventArgs e)
        {
            txtCurDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void rectangleClose_MouseEnter(object sender, MouseEventArgs e)
        {
            this.rectangleClose.Fill = Brushes.Red;
        }

        private void rectangleClose_MouseLeave(object sender, MouseEventArgs e)
        {
            this.rectangleClose.Fill = Brushes.Transparent;
        }

        private void rectangleClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}