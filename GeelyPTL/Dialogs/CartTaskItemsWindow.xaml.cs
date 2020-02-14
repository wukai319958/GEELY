using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using DataAccess.Assorting;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 料车任务的明细列表界面。
    /// </summary>
    public partial class CartTaskItemsWindow : Window
    {
        readonly long astCartTaskId;

        CartTaskItemsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 初始化料车任务的明细列表界面。
        /// </summary>
        /// <param name="astCartTaskId">任务的主键。</param>
        public CartTaskItemsWindow(long astCartTaskId)
            : this()
        {
            this.astCartTaskId = astCartTaskId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    AST_CartTask astCartTask = dbContext.AST_CartTasks
                                                   .First(lt => lt.Id == this.astCartTaskId);
                    List<AST_CartTaskItem> astCartTaskItems = dbContext.AST_CartTaskItems
                                                                  .Include(lti => lti.AST_PalletTaskItem)
                                                                  .Include(lti => lti.AST_PalletTaskItem.AST_PalletTask.CFG_Pallet)
                                                                  .Where(lti => lti.AST_CartTaskId == this.astCartTaskId)
                                                                  .OrderBy(lti => lti.CartPosition)
                                                                  .ToList();

                    this.Title = this.Title + "：" + astCartTask.CFG_Cart.Name;
                    this.dataGrid.ItemsSource = astCartTaskItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}