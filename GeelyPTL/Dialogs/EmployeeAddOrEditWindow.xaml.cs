using System;
using System.Linq;
using System.Windows;
using DataAccess;
using DataAccess.Config;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 操作员新增与编辑界面。
    /// </summary>
    public partial class EmployeeAddOrEditWindow : Window
    {
        readonly int? cfgEmployeeId;

        /// <summary>
        /// 用于新增。
        /// </summary>
        public EmployeeAddOrEditWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 用于修改。
        /// </summary>
        /// <param name="cfgEmployeeId">待修改操作员的主键。</param>
        public EmployeeAddOrEditWindow(int cfgEmployeeId)
            : this()
        {
            this.cfgEmployeeId = cfgEmployeeId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.cfgEmployeeId == null)
                {
                    this.checkBoxIsEnable.IsChecked = true;
                }
                else
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        CFG_Employee cfgEmployee = dbContext.CFG_Employees
                                                       .First(c => c.Id == this.cfgEmployeeId);

                        this.textBoxCode.Text = cfgEmployee.Code;
                        this.textBoxName.Text = cfgEmployee.Name;
                        this.textBoxLoginName.Text = cfgEmployee.LoginName;
                        this.passwordBoxPassword.Password = cfgEmployee.Password;
                        this.checkBoxIsEnable.IsChecked = cfgEmployee.IsEnabled;

                        if (cfgEmployee.Code == "Administrator")
                            this.checkBoxIsEnable.IsEnabled = false;
                    }
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
                    CFG_Employee cfgEmployee;

                    if (this.cfgEmployeeId == null)
                    {
                        cfgEmployee = new CFG_Employee();

                        dbContext.CFG_Employees.Add(cfgEmployee);
                    }
                    else
                    {
                        cfgEmployee = dbContext.CFG_Employees
                                          .First(c => c.Id == this.cfgEmployeeId);
                    }

                    cfgEmployee.Code = this.textBoxCode.Text.Trim();
                    cfgEmployee.Name = this.textBoxName.Text.Trim();
                    cfgEmployee.LoginName = this.textBoxLoginName.Text.Trim();
                    cfgEmployee.Password = this.passwordBoxPassword.Password.Trim();
                    cfgEmployee.IsEnabled = this.checkBoxIsEnable.IsChecked == true;

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