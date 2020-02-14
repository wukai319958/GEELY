using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace GeelyPTL
{
    /// <summary>
    /// 主界面。
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 初始化主界面。
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void gridHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            }
            else if (e.ClickCount == 2)
            {
                this.WindowState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
        }

        private void menuItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.menuItemService.IsActived = false;
            this.menuItemChannelList.IsActived = false;
            this.menuItemCartList.IsActived = false;
            this.menuItemMarketList.IsActived = false;
            this.menuItemWorkStationList.IsActived = false;
            this.menuItemLesTaskList.IsActived = false;
            this.menuItemPalletTaskList.IsActived = false;
            this.menuItemCartTaskList.IsActived = false;
            this.menuItemFindTaskList.IsActived = false;
            this.menuItemDistributeTaskList.IsActived = false;
            this.menuItemAssembleTaskList.IsActived = false;
            this.menuItemEmployeeList.IsActived = false;

            ((MenuItemUserControl)sender).IsActived = true;

            this.serviceUserControl.Visibility = Visibility.Hidden;
            this.channelsUserControl.Visibility = Visibility.Hidden;
            this.cartsUserControl.Visibility = Visibility.Hidden;
            this.marketUserControl.Visibility = Visibility.Hidden;
            this.workStationsUserControl.Visibility = Visibility.Hidden;
            this.lesTasksUserControl.Visibility = Visibility.Hidden;
            this.palletTasksUserControl.Visibility = Visibility.Hidden;
            this.cartTasksUserControl.Visibility = Visibility.Hidden;
            this.findTasksUserControl.Visibility = Visibility.Hidden;
            this.distributeTasksUserControl.Visibility = Visibility.Hidden;
            this.assembleTasksUserControl.Visibility = Visibility.Hidden;
            this.employeesUserControl.Visibility = Visibility.Hidden;

            if (sender == this.menuItemService)
                this.serviceUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemChannelList)
                this.channelsUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemCartList)
                this.cartsUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemMarketList)
                this.marketUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemWorkStationList)
                this.workStationsUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemLesTaskList)
                this.lesTasksUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemPalletTaskList)
                this.palletTasksUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemCartTaskList)
                this.cartTasksUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemFindTaskList)
                this.findTasksUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemDistributeTaskList)
                this.distributeTasksUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemAssembleTaskList)
                this.assembleTasksUserControl.Visibility = Visibility.Visible;
            else if (sender == this.menuItemEmployeeList)
                this.employeesUserControl.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show(this, @"注意：

确定要退出应用程序？

请先确保没有正在进行的作业。
", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }
}