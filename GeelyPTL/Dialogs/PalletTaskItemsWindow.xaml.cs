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
    /// 按托任务的明细列表界面。
    /// </summary>
    public partial class PalletTaskItemsWindow : Window
    {
        readonly long astPalletTaskId;

        PalletTaskItemsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 初始化按托任务的明细列表界面。
        /// </summary>
        /// <param name="astPalletTaskId">任务的主键。</param>
        public PalletTaskItemsWindow(long astPalletTaskId)
            : this()
        {
            this.astPalletTaskId = astPalletTaskId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    AST_PalletTask astPalletTask = dbContext.AST_PalletTasks
                                                       .First(lt => lt.Id == this.astPalletTaskId);
                    List<AST_PalletTaskItem> astPalletTaskItems = dbContext.AST_PalletTaskItems
                                                                      .Include(lti => lti.CFG_WorkStation)
                                                                      .Where(lti => lti.AST_PalletTaskId == this.astPalletTaskId)
                                                                      .OrderBy(lti => lti.FromPalletPosition)
                                                                      .ToList();

                    this.Title = this.Title + "：" + astPalletTask.CFG_Pallet.Code;
                    this.dataGrid.ItemsSource = astPalletTaskItems;
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