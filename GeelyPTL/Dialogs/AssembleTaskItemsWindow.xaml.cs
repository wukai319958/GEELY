using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using DataAccess.AssemblyIndicating;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 装配指引任务的明细列表界面。
    /// </summary>
    public partial class AssembleTaskItemsWindow : Window
    {
        readonly long asmTaskId;

        AssembleTaskItemsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 初始化装配指引任务的明细列表界面。
        /// </summary>
        /// <param name="asmTaskId">任务的主键。</param>
        public AssembleTaskItemsWindow(long asmTaskId)
            : this()
        {
            this.asmTaskId = asmTaskId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ASM_Task asmTask = dbContext.ASM_Tasks
                                           .Include(at => at.ASM_AssembleIndication)
                                           .First(at => at.Id == this.asmTaskId);
                    //List<ASM_TaskItem> asmTaskItems = dbContext.ASM_TaskItems
                    //                                      .Include(ati => ati.ASM_AssembleIndicationItem)
                    //                                      .Include(ati => ati.CFG_Cart)
                    //                                      .Where(ati => ati.ASM_TaskId == this.asmTaskId)
                    //                                      .OrderBy(ati => ati.CFG_CartId)
                    //                                      .ThenBy(ati => ati.CartPosition)
                    //                                      .ToList();
                    List<ASM_TaskItem> asmTaskItems = dbContext.ASM_TaskItems
                                                          .Include(ati => ati.ASM_AssembleIndicationItem)
                                                          .Include(ati => ati.CFG_Cart)
                                                          .Where(ati => ati.ASM_TaskId == this.asmTaskId)
                                                          .OrderBy(ati => ati.AssembleSequence)
                                                          .ToList();

                    this.Title = this.Title + "：" + asmTask.ASM_AssembleIndication.ProductSequence;
                    this.dataGrid.ItemsSource = asmTaskItems;
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