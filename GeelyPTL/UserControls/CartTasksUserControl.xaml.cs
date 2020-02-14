using System;
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
using DataAccess.Config;
using GeelyPTL.Dialogs;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 料车任务的列表界面。
    /// </summary>
    public partial class CartTasksUserControl : UserControl
    {
        readonly string Title = "料车任务";

        /// <summary>
        /// 初始化料车任务的列表界面。
        /// </summary>
        public CartTasksUserControl()
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

                IQueryable<AST_CartTask> queryable = dbContext.AST_CartTasks
                                                         .Include(lt => lt.CFG_Cart)
                                                         .Include(lt => lt.CFG_Channel)
                                                         .Include(lt => lt.CFG_WorkStation)
                                                         .Where(lt => lt.CreateTime > minTime && lt.CreateTime < maxTime)
                                                         .OrderBy(lt => lt.Id);

                if (!string.IsNullOrEmpty(condition))
                {
                    queryable = queryable.Where(lt => lt.ProjectCode.Contains(condition)
                                                      || lt.ProjectStep.Contains(condition)
                                                      || lt.BatchCode.Contains(condition)
                                                      || lt.CFG_Cart.Name.Contains(condition)
                                                      || lt.CFG_Channel.Name.Contains(condition)
                                                      || lt.CFG_WorkStation.Code.Contains(condition));
                }

                this.dataGrid.ItemsSource = queryable.ToList();
            }
        }

        private void buttonDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AST_CartTask astCartTask = (AST_CartTask)this.dataGrid.SelectedItem;
                if (astCartTask != null)
                {
                    CartTaskItemsWindow dialog = new CartTaskItemsWindow(astCartTask.Id);
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonFinish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AST_CartTask uiAstCartTask = (AST_CartTask)this.dataGrid.SelectedItem;
                if (uiAstCartTask != null
                    && MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "确认强制完成 {0}？", uiAstCartTask.CFG_Cart.Name), this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        AST_CartTask dbAstCartTask = dbContext.AST_CartTasks
                                                         .First(lt => lt.Id == uiAstCartTask.Id);

                        dbAstCartTask.AssortingStatus = AssortingStatus.Finished;
                        dbAstCartTask.CFG_Cart.CartStatus = CartStatus.Assorted;

                        dbContext.SaveChanges();
                    }

                    MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "已强制完成 {0}。\r\n \r\n请手动指派 AGV 搬运到物料超市。", uiAstCartTask.CFG_Cart.Name), this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
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