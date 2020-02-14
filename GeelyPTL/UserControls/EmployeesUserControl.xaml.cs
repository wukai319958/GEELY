using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using DataAccess;
using DataAccess.Config;
using GeelyPTL.Dialogs;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 操作员列表界面。
    /// </summary>
    public partial class EmployeesUserControl : UserControl
    {
        readonly string Title = "操作员";

        /// <summary>
        /// 初始化操作员列表界面。
        /// </summary>
        public EmployeesUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try { this.DoRefresh(); }
                catch { }
            }), DispatcherPriority.SystemIdle);
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DoRefresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void DoRefresh()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                this.dataGrid.ItemsSource = dbContext.CFG_Employees
                                                .OrderBy(e => e.Code)
                                                .ToList();
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EmployeeAddOrEditWindow dialog = new EmployeeAddOrEditWindow();
                if (dialog.ShowDialog() == true)
                    this.DoRefresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CFG_Employee cfgEmployee = (CFG_Employee)this.dataGrid.SelectedItem;
                if (cfgEmployee != null)
                {
                    EmployeeAddOrEditWindow dialog = new EmployeeAddOrEditWindow(cfgEmployee.Id);
                    if (dialog.ShowDialog() == true)
                        this.DoRefresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CFG_Employee cfgEmployee = (CFG_Employee)this.dataGrid.SelectedItem;
                if (cfgEmployee != null && cfgEmployee.Code != "Administrator")
                {
                    if (MessageBox.Show("确认删除 " + cfgEmployee.Name + "？", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            CFG_Employee cfgEmployeeFromDb = dbContext.CFG_Employees
                                                                 .First(emp => emp.Id == cfgEmployee.Id);

                            dbContext.CFG_Employees.Remove(cfgEmployeeFromDb);

                            dbContext.SaveChanges();
                        }

                        this.DoRefresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.buttonEdit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, this.buttonEdit));
        }
    }
}