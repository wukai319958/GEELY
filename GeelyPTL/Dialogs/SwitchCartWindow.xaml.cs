using System;
using System.Linq;
using System.Windows;
using DataAccess;
using DataAccess.Config;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 空满交换料车界面。
    /// </summary>
    public partial class SwitchCartWindow : Window
    {
        public string SwitchCartCode { get; set; }

        public SwitchCartWindow()
        {
            this.InitializeComponent();
            this.SwitchCartCode = "";
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SwitchCartCode = txtCartCode.Text.Trim();

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