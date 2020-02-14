using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using DataAccess.CartFinding;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 配送任务的明细列表界面。
    /// </summary>
    public partial class FndTaskItemsWindow : Window
    {
        readonly long fndTaskId;

        FndTaskItemsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 初始化配送任务的明细列表界面。
        /// </summary>
        /// <param name="fndTaskId">任务的主键。</param>
        public FndTaskItemsWindow(long fndTaskId)
            : this()
        {
            this.fndTaskId = fndTaskId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    FND_DeliveryResult fndDeliveryResult = dbContext.FND_DeliveryResults
                                                               .First(dr => dr.FND_TaskId == this.fndTaskId);
                    List<FND_DeliveryResultItem> fndDeliveryResultItems = fndDeliveryResult.FND_DeliveryResultItems
                                                                              .OrderBy(dri => dri.CartPosition)
                                                                              .ToList();

                    this.Title = this.Title + "：" + fndDeliveryResult.CFG_Cart.Name;
                    this.dataGrid.ItemsSource = fndDeliveryResultItems;
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