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
using DataAccess.Assorting;
using GeelyPTL.Dialogs;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 按托任务列表界面。
    /// </summary>
    public partial class PalletTasksUserControl : UserControl
    {
        readonly string Title = "按托任务";

        /// <summary>
        /// 初始化按托任务列表界面。
        /// </summary>
        public PalletTasksUserControl()
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

                IQueryable<AST_PalletTask> queryable = dbContext.AST_PalletTasks
                                                           .Include(lt => lt.CFG_Channel)
                                                           .Include(lt => lt.CFG_Pallet)
                                                           .Where(lt => lt.CreateTime > minTime && lt.CreateTime < maxTime)
                                                           .OrderBy(lt => lt.Id);

                if (!string.IsNullOrEmpty(condition))
                {
                    queryable = queryable.Where(lt => lt.ProjectCode.Contains(condition)
                                                      || lt.ProjectStep.Contains(condition)
                                                      || lt.BatchCode.Contains(condition)
                                                      || lt.CFG_Channel.Name.Contains(condition)
                                                      || lt.CFG_Pallet.Code.Contains(condition));
                }

                this.dataGrid.ItemsSource = queryable.ToList();
            }
        }

        private void buttonDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AST_PalletTask astPalletTask = (AST_PalletTask)this.dataGrid.SelectedItem;
                if (astPalletTask != null)
                {
                    PalletTaskItemsWindow dialog = new PalletTaskItemsWindow(astPalletTask.Id);
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