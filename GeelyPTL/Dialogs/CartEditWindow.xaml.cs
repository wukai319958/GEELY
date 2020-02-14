using System;
using System.Linq;
using System.Windows;
using DataAccess;
using DataAccess.Config;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 料车编辑界面。
    /// </summary>
    public partial class CartEditWindow : Window
    {
        readonly int cfgCartId;

        CartEditWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 初始化料车编辑界面。
        /// </summary>
        /// <param name="cfgCartId">待编辑料车的主键。</param>
        public CartEditWindow(int cfgCartId)
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

                    this.textBoxName.Text = cfgCart.Name;
                    this.textBoxRfid1.Text = cfgCart.Rfid1;
                    this.textBoxXGateMAC.Text = cfgCart.XGateMAC;
                    this.textBoxXGateIP.Text = cfgCart.XGateIP;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    CFG_Cart cfgCart = dbContext.CFG_Carts
                                           .First(c => c.Id == this.cfgCartId);

                    cfgCart.Rfid1 = this.textBoxRfid1.Text.Trim();
                    cfgCart.XGateMAC = this.textBoxXGateMAC.Text.Trim();
                    cfgCart.XGateIP = this.textBoxXGateIP.Text.Trim();

                    dbContext.SaveChanges();
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}