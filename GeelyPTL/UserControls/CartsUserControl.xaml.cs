using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using GeelyPTL.Dialogs;
using GeelyPTL.Models;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 料车列表界面。
    /// </summary>
    public partial class CartsUserControl : UserControl
    {
        readonly string Title = "料车";

        /// <summary>
        /// 初始化料车列表界面。
        /// </summary>
        public CartsUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try { this.viewModel.Refresh(); }
                catch { }
            }), DispatcherPriority.SystemIdle);
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.viewModel.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonBatchAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BatchAddCartWindow dialog = new BatchAddCartWindow();
                if (dialog.ShowDialog() == true)
                    this.viewModel.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CartModel cartModel = (CartModel)this.dataGrid.SelectedItem;
                if (cartModel != null)
                {
                    CartEditWindow dialog = new CartEditWindow(cartModel.CFG_Cart.Id);
                    if (dialog.ShowDialog() == true)
                        this.viewModel.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonMaterials_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CartModel cartModel = (CartModel)this.dataGrid.SelectedItem;
                if (cartModel != null)
                {
                    CartMaterialsWindow dialog = new CartMaterialsWindow(cartModel.CFG_Cart.Id);
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.buttonMaterials.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, this.buttonMaterials));
        }
    }
}