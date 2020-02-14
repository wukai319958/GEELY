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
using DataAccess.AssemblyIndicating;
using GeelyPTL.Dialogs;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 装配指引任务的列表界面。
    /// </summary>
    public partial class AssembleTasksUserControl : UserControl
    {
        readonly string Title = "装配指引任务";

        /// <summary>
        /// 初始化装配指引任务的列表界面。
        /// </summary>
        public AssembleTasksUserControl()
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

                IQueryable<ASM_Task> queryable = dbContext.ASM_Tasks
                                                     .Include(at => at.ASM_AssembleIndication)
                                                     .Include(at => at.ASM_AssembleIndication.CFG_WorkStation)
                                                     .Where(at => at.ASM_AssembleIndication.CarArrivedTime > minTime && at.ASM_AssembleIndication.CarArrivedTime < maxTime)
                                                     .OrderBy(at => at.Id);

                if (!string.IsNullOrEmpty(condition))
                {
                    queryable = queryable.Where(at => at.ASM_AssembleIndication.FactoryCode.Contains(condition)
                                                      || at.ASM_AssembleIndication.ProductionLineCode.Contains(condition)
                                                      || at.ASM_AssembleIndication.CFG_WorkStation.Code.Contains(condition)
                                                      || at.ASM_AssembleIndication.GzzList.Contains(condition)
                                                      || at.ASM_AssembleIndication.MONumber.Contains(condition)
                                                      || at.ASM_AssembleIndication.ProductSequence.Contains(condition));
                }

                this.dataGrid.ItemsSource = queryable.ToList();
            }
        }

        private void buttonDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ASM_Task asmTask = (ASM_Task)this.dataGrid.SelectedItem;
                if (asmTask != null)
                {
                    AssembleTaskItemsWindow dialog = new AssembleTaskItemsWindow(asmTask.Id);
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