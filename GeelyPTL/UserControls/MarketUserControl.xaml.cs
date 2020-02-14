using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 物料超市列表界面。
    /// </summary>
    public partial class MarketUserControl : UserControl
    {
        readonly string Title = "物料超市";

        /// <summary>
        /// 初始化物料超市列表界面。
        /// </summary>
        public MarketUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try { this.viewModel.Refresh(); }
                catch { }
            }), DispatcherPriority.SystemIdle);
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.viewModel.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}