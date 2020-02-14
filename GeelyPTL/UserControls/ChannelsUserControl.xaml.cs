using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 分拣口列表界面。
    /// </summary>
    public partial class ChannelsUserControl : UserControl
    {
        readonly string Title = "巷道";

        /// <summary>
        /// 初始化分拣口列表界面。
        /// </summary>
        public ChannelsUserControl()
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