using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using DataAccess.Config;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 批量添加料车界面。
    /// </summary>
    public partial class BatchAddCartWindow : Window
    {
        /// <summary>
        /// 初始化批量添加料车界面。
        /// </summary>
        public BatchAddCartWindow()
        {
            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    int existCount = dbContext.CFG_Carts
                                         .Count();

                    this.textBoxFrom.Text = (existCount + 1).ToString(CultureInfo.InvariantCulture);
                }
            }
            catch { }
        }

        private void textBoxFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.TryCalculateTo();
        }

        private void textBoxCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.TryCalculateTo();
        }

        void TryCalculateTo()
        {
            try
            {
                this.textBoxTo.Text = string.Empty;

                int from = int.Parse(this.textBoxFrom.Text, CultureInfo.InvariantCulture);
                int count = int.Parse(this.textBoxCount.Text, CultureInfo.InvariantCulture);

                this.textBoxTo.Text = (from + count - 1).ToString(CultureInfo.InvariantCulture);
            }
            catch { }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    int from = int.Parse(this.textBoxFrom.Text, CultureInfo.InvariantCulture);
                    int count = int.Parse(this.textBoxCount.Text, CultureInfo.InvariantCulture);

                    for (int cartNumber = from; cartNumber < from + count; cartNumber++)
                    {
                        CFG_Cart cfgCart = new CFG_Cart();
                        cfgCart.Code = string.Format(CultureInfo.InvariantCulture, "{0:000000}", 100000 + cartNumber);
                        cfgCart.Name = string.Format(CultureInfo.InvariantCulture, "料车 {0}", cartNumber);
                        cfgCart.Rfid1 = "AABBCCDDEEFF";
                        cfgCart.XGateIP = "192.168.0.10";
                        cfgCart.CartStatus = CartStatus.Free;

                        dbContext.CFG_Carts.Add(cfgCart);

                        //小车上的 8 个库位
                        for (int position = 1; position <= 8; position++)
                        {
                            CFG_CartCurrentMaterial cfgCartCurrentMaterial = new CFG_CartCurrentMaterial();
                            cfgCartCurrentMaterial.CFG_Cart = cfgCart;
                            cfgCartCurrentMaterial.Position = position;
                            cfgCartCurrentMaterial.Usability = CartPositionUsability.Enable;

                            dbContext.CFG_CartCurrentMaterials.Add(cfgCartCurrentMaterial);
                        }

                        //小车上的 10 个标签
                        for (byte deviceAddress = 1; deviceAddress <= 10; deviceAddress++)
                        {
                            CFG_CartPtlDevice cfgCartPtlDevice = new CFG_CartPtlDevice();
                            cfgCartPtlDevice.CFG_Cart = cfgCart;
                            cfgCartPtlDevice.DeviceAddress = deviceAddress;

                            dbContext.CFG_CartPtlDevices.Add(cfgCartPtlDevice);
                        }
                    }

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