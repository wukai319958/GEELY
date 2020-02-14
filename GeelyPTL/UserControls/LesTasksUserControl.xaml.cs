using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
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
    /// LES 原始任务的列表界面。
    /// </summary>
    public partial class LesTasksUserControl : UserControl
    {
        readonly string Title = "LES 任务";

        /// <summary>
        /// 初始化 LES 原始任务的列表界面。
        /// </summary>
        public LesTasksUserControl()
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

                IQueryable<AST_LesTask> queryable = dbContext.AST_LesTasks
                                                        .Include(lt => lt.CFG_WorkStation)
                                                        .Include(lt => lt.CFG_Channel)
                                                        .Include(lt => lt.CFG_Pallet)
                                                        .Where(lt => lt.RequestTime > minTime && lt.RequestTime < maxTime)
                                                        .OrderBy(lt => lt.Id);

                if (!string.IsNullOrEmpty(condition))
                {
                    queryable = queryable.Where(lt => lt.ProjectCode.Contains(condition)
                                                      || lt.ProjectStep.Contains(condition)
                                                      || lt.BatchCode.Contains(condition)
                                                      || lt.CFG_WorkStation.Code.Contains(condition)
                                                      || lt.GzzList.Contains(condition)
                                                      || lt.CFG_Channel.Name.Contains(condition)
                                                      || lt.CFG_Pallet.Code.Contains(condition)
                                                      || lt.BoxCode.Contains(condition));
                }

                this.dataGrid.ItemsSource = queryable.ToList();
            }
        }

        private void buttonDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AST_LesTask astLesTask = (AST_LesTask)this.dataGrid.SelectedItem;
                if (astLesTask != null)
                {
                    LesTaskItemsWindow dialog = new LesTaskItemsWindow(astLesTask.Id);
                    dialog.ShowDialog();
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
                List<AST_LesTask> uiAstLesTasks = new List<AST_LesTask>();
                foreach (AST_LesTask uiAstLesTask in this.dataGrid.SelectedItems)
                    uiAstLesTasks.Add(uiAstLesTask);

                if (uiAstLesTasks.Count > 0
                    && MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "确认删除所选的 {0} 条 LES 任务？", uiAstLesTasks.Count), this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        foreach (AST_LesTask uiAstLesTask in uiAstLesTasks)
                        {
                            AST_LesTask dbAstLesTask = dbContext.AST_LesTasks
                                                           .FirstOrDefault(lt => lt.Id == uiAstLesTask.Id);
                            if (dbAstLesTask != null)
                            {
                                dbContext.AST_LesTaskItems.RemoveRange(dbAstLesTask.AST_LesTaskItems);
                                dbContext.AST_LesTaskMessages.Remove(dbAstLesTask.AST_LesTaskMessage);
                                dbContext.AST_LesTasks.Remove(dbAstLesTask);
                            }

                            dbContext.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                try { this.DoSearch(); }
                catch { }
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