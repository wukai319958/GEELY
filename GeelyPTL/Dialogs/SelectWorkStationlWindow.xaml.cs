using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using DataAccess.Config;
using GeelyPTL.Models;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 铺线巷道选择界面。
    /// </summary>
    public partial class SelectWorkStationlWindow : Window
    {
        /// <summary>
        /// 操作类型（线边铺线，线边清线，线边空料架返回）
        /// </summary>
        readonly string OperateType;

        /// <summary>
        /// 选择的工位ID
        /// </summary>
        public List<int> InitWorkStationIds { get; set; }

        /// <summary>
        /// 工位选择数据集
        /// </summary>
        private List<WorkStationSelModel> workStationSelModels;

        /// <summary>
        /// 初始化铺线巷道选择界面
        /// </summary>
        /// <param name="nOperateType">操作类型（0：线边铺线，1：线边清线）</param>
        public SelectWorkStationlWindow(int nOperateType)
        {
            this.InitializeComponent();

            if (nOperateType == 0)
            {
                this.OperateType = "线边铺线";
            }
            else if (nOperateType == 1)
            {
                this.OperateType = "线边清线";
            }
            else if (nOperateType == 2)
            {
                this.OperateType = "线边空料架返回";
            }
            this.Title = this.OperateType + "工位选择";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitWorkStationIds = new List<int>();

                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    workStationSelModels = new List<WorkStationSelModel>();

                    List<CFG_WorkStation> cfgWorkStations = dbContext.CFG_WorkStations.OrderBy(t => t.Code).ToList();

                    foreach (CFG_WorkStation cfgWorkStation in cfgWorkStations)
                    {
                        WorkStationSelModel workStationSelModel = new WorkStationSelModel();
                        workStationSelModel.Id = cfgWorkStation.Id;
                        workStationSelModel.Code = cfgWorkStation.Code;
                        workStationSelModel.Name = cfgWorkStation.Name;
                        workStationSelModel.IsChecked = false;

                        workStationSelModels.Add(workStationSelModel);
                    }

                    this.dataGrid.ItemsSource = workStationSelModels;
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

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk1 = (CheckBox)sender;

            if (workStationSelModels != null && workStationSelModels.Count > 0)
            {
                foreach (WorkStationSelModel workStationSelModel in workStationSelModels)
                {
                    workStationSelModel.IsChecked = chk1.IsChecked.Value;
                }

                this.dataGrid.ItemsSource = workStationSelModels;
            }
        }

        private void cb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            WorkStationSelModel vd = chk.Tag as WorkStationSelModel;
            vd.IsChecked = chk.IsChecked.Value;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            InitWorkStationIds.Clear();
            if (workStationSelModels != null && workStationSelModels.Count > 0)
            {
                foreach (WorkStationSelModel workStationSelModel in workStationSelModels)
                {
                    if (workStationSelModel.IsChecked)
                    {
                        InitWorkStationIds.Add(workStationSelModel.Id);
                    }
                }
            }

            if (InitWorkStationIds.Count == 0)
            {
                MessageBox.Show("请选择需要" + this.OperateType + "的工位");
                return;
            }
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            InitWorkStationIds.Clear();
            this.Close();
        }
        
    }
}