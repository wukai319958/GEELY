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
    /// LES 原始任务的明细列表界面。
    /// </summary>
    public partial class LesTaskItemsWindow : Window
    {
        readonly long astLesTaskId;

        LesTaskItemsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 初始化 LES 原始任务的明细列表界面。
        /// </summary>
        /// <param name="astLesTaskId">任务的主键。</param>
        public LesTaskItemsWindow(long astLesTaskId)
            : this()
        {
            this.astLesTaskId = astLesTaskId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    AST_LesTask astLesTask = dbContext.AST_LesTasks
                                                 .First(lt => lt.Id == this.astLesTaskId);
                    List<AST_LesTaskItem> astLesTaskItems = dbContext.AST_LesTaskItems
                                                                .Include(lti => lti.AST_PalletTaskItem)
                                                                .Where(lti => lti.AST_LesTaskId == this.astLesTaskId)
                                                                .OrderBy(lti => lti.BillDetailId)
                                                                .ToList();

                    this.Title = this.Title + "：" + astLesTask.BillCode;
                    this.dataGrid.ItemsSource = astLesTaskItems;
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