using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 工位列表界面。
    /// </summary>
    public partial class WorkStationsUserControl : UserControl
    {
        readonly string Title = "工位";

        /// <summary>
        /// 初始化工位列表界面。
        /// </summary>
        public WorkStationsUserControl()
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