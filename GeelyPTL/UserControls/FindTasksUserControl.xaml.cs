using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using DataAccess;
using DataAccess.CartFinding;
using GeelyPTL.Dialogs;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 料车配送任务的列表界面。
    /// </summary>
    public partial class FindTasksUserControl : UserControl
    {
        readonly string Title = "配送任务";

        /// <summary>
        /// 初始化料车配送任务的列表界面。
        /// </summary>
        public FindTasksUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.datePickerMin.Value = DateTime.Today;
            this.datePickerMax.Value = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try { this.DoSearch(); }
                catch { }
            }), DispatcherPriority.SystemIdle);
        }

        private void buttonToday_Click(object sender, RoutedEventArgs e)
        {
            this.datePickerMin.Value = DateTime.Today;
            this.datePickerMax.Value = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DoSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void DoSearch()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                DateTime minTime = this.datePickerMin.Value ?? DateTime.Today;
                DateTime maxTime = this.datePickerMax.Value ?? DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
                string condition = this.textBoxCondition.Text.Trim();

                IQueryable<FND_Task> queryable = dbContext.FND_Tasks
                                                     .Include(ft => ft.CFG_WorkStation)
                                                     .Include(ft => ft.CFG_Cart)
                                                     .Where(ft => ft.RequestTime > minTime && ft.RequestTime < maxTime)
                                                     .OrderBy(ft => ft.Id);

                if (!string.IsNullOrEmpty(condition))
                {
                    queryable = queryable.Where(ft => ft.ProjectCode.Contains(condition)
                                                      || ft.ProjectStep.Contains(condition)
                                                      || ft.BatchCode.Contains(condition)
                                                      || ft.CFG_WorkStation.Code.Contains(condition)
                                                      || ft.CFG_Cart.Name.Contains(condition));
                }

                this.dataGrid.ItemsSource = queryable.ToList();
            }
        }

        private void buttonDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FND_Task fndTask = (FND_Task)this.dataGrid.SelectedItem;
                if (fndTask != null && fndTask.FindingStatus == FindingStatus.Finished)
                {
                    FndTaskItemsWindow dialog = new FndTaskItemsWindow(fndTask.Id);
                    dialog.ShowDialog();
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
            this.buttonDetail.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, this.buttonDetail));
        }
    }
}