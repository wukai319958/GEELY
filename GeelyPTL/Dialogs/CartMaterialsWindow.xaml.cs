using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DataAccess;
using DataAccess.Config;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 料车上存放物料的列表界面。
    /// </summary>
    public partial class CartMaterialsWindow : Window
    {
        readonly int cfgCartId;

        CartMaterialsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 初始化料车上存放物料的列表界面。
        /// </summary>
        /// <param name="cfgCartId">待查看料车的主键。</param>
        public CartMaterialsWindow(int cfgCartId)
            : this()
        {
            this.cfgCartId = cfgCartId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    CFG_Cart cfgCart = dbContext.CFG_Carts
                                           .First(c => c.Id == this.cfgCartId);
                    List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = dbContext.CFG_CartCurrentMaterials
                                                                                .Include(ccm => ccm.CFG_Pallet)
                                                                                .Where(ccm => ccm.CFG_CartId == this.cfgCartId)
                                                                                .OrderBy(ccm => ccm.Position)
                                                                                .ToList();

                    this.textBlockCartName.Text = cfgCart.Name;

                    CFG_CartCurrentMaterial firstNotEmptyCfgCartCurrentMaterial = cfgCartCurrentMaterials
                                                                                      .FirstOrDefault(ccm => ccm.Quantity != null);
                    if (firstNotEmptyCfgCartCurrentMaterial != null)
                    {
                        this.textBoxProjectCode.Text = firstNotEmptyCfgCartCurrentMaterial.ProjectCode;
                        this.textBoxProjectStep.Text = firstNotEmptyCfgCartCurrentMaterial.ProjectStep;
                        this.textBoxBatchCode.Text = firstNotEmptyCfgCartCurrentMaterial.BatchCode;
                        this.textBoxWorkStationCode.Text = firstNotEmptyCfgCartCurrentMaterial.CFG_WorkStation.Code;
                        this.textBoxChannelName.Text = firstNotEmptyCfgCartCurrentMaterial.CFG_Channel.Name;
                    }

                    this.dataGrid.ItemsSource = cfgCartCurrentMaterials;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}