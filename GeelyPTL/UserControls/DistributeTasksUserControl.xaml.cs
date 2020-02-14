using DataAccess;
using DataAccess.Distributing;
using GeelyPTL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// AGV配送任务的列表界面
    /// </summary>
    public partial class DistributeTasksUserControl : UserControl
    {
        readonly string Title = "AGV配送任务";

        public DistributeTasksUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.datePickerMin.Value = DateTime.Today;
            this.datePickerMax.Value = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
            this.cmbReqType.Text = "";

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try { this.DoSearch(); }
                catch { }
            }), DispatcherPriority.SystemIdle);
        }

        private void buttonToday_Click(object sender, RoutedEventArgs e)
        {
            this.datePickerMin.Value = DateTime.Today;
            this.datePickerMax.Value = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DoSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void DoSearch()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                DateTime minTime = this.datePickerMin.Value ?? DateTime.Today;
                DateTime maxTime = this.datePickerMax.Value ?? DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);

                IQueryable<DST_DistributeTask> queryable = dbContext.DST_DistributeTasks.Where(lt => lt.reqTime > minTime && lt.reqTime < maxTime);

                string sReqType = this.cmbReqType.Text;
                if (!string.IsNullOrEmpty(sReqType))
                {
                    DistributeReqTypes distributeReqTypes = ReqTextToTypeConvert(sReqType);
                    queryable = queryable.Where(lt => lt.DistributeReqTypes == distributeReqTypes);
                }

                string sResponse = this.cmbIsResponse.Text;
                if (!string.IsNullOrEmpty(sResponse))
                {
                    bool IsResponse = false;
                    if (sResponse.Equals("是"))
                    {
                        IsResponse = true;
                    }
                    else if (sResponse.Equals("否"))
                    {
                        IsResponse = false;
                    }
                    queryable = queryable.Where(lt => lt.isResponse == IsResponse);
                }

                string sArrive = this.cmbIsArrive.Text;
                if (!string.IsNullOrEmpty(sArrive))
                {
                    if (sArrive.Equals("是"))
                    {
                        queryable = queryable.Where(lt => lt.arriveTime != null);
                    }
                    else if (sArrive.Equals("否"))
                    {
                        queryable = queryable.Where(lt => lt.arriveTime == null);
                    }
                }

                string sSendType = this.cmbSendType.Text;
                if (!string.IsNullOrEmpty(sSendType))
                {
                    queryable = queryable.Where(lt => lt.podDir.Equals(sSendType));
                }
                
                List<DistributeTaskModel> distributeTaskModels = new List<DistributeTaskModel>();
                foreach (DST_DistributeTask dstDistributeTask in queryable)
                {
                    DistributeTaskModel distributeTaskModel = new DistributeTaskModel();
                    distributeTaskModel.ID = dstDistributeTask.ID;
                    distributeTaskModel.reqCode = dstDistributeTask.reqCode;
                    distributeTaskModel.reqTime = dstDistributeTask.reqTime;
                    distributeTaskModel.clientCode = dstDistributeTask.clientCode;
                    distributeTaskModel.tokenCode = dstDistributeTask.tokenCode;
                    distributeTaskModel.taskTyp = dstDistributeTask.taskTyp;
                    distributeTaskModel.userCallCode = dstDistributeTask.userCallCode;
                    distributeTaskModel.taskGroupCode = dstDistributeTask.taskGroupCode;
                    distributeTaskModel.startPosition = dstDistributeTask.startPosition;
                    distributeTaskModel.endPosition = dstDistributeTask.endPosition;
                    distributeTaskModel.podCode = dstDistributeTask.podCode;
                    distributeTaskModel.podDir = dstDistributeTask.podDir;
                    distributeTaskModel.priority = dstDistributeTask.priority;
                    distributeTaskModel.robotCode = dstDistributeTask.robotCode;
                    distributeTaskModel.taskCode = dstDistributeTask.taskCode;
                    distributeTaskModel.data = dstDistributeTask.data;
                    distributeTaskModel.DistributeReqTypes = dstDistributeTask.DistributeReqTypes;
                    distributeTaskModel.isResponse = dstDistributeTask.isResponse;
                    distributeTaskModel.arriveTime = dstDistributeTask.arriveTime;
                    distributeTaskModel.sendErrorCount = dstDistributeTask.sendErrorCount;
                    distributeTaskModel.IsChecked = false;

                    distributeTaskModels.Add(distributeTaskModel);
                }

                //this.dataGrid.ItemsSource = queryable.ToList();
                this.dataGrid.ItemsSource = distributeTaskModels;
            }
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private DistributeReqTypes ReqTextToTypeConvert(string sReqText)
        {
            switch (sReqText)
            {
                case "拣料区铺线":
                    return DistributeReqTypes.PickAreaInit;
                case "生产线边铺线":
                    return DistributeReqTypes.ProductAreaInit;
                case "拣料区配送":
                    return DistributeReqTypes.PickAreaDistribute;
                case "物料超市配送":
                    return DistributeReqTypes.MaterialMarketDistribute;
                case "空料架缓冲区配送":
                    return DistributeReqTypes.NullCartAreaDistribute;
                case "生产线边料架转换":
                    return DistributeReqTypes.ProductCartSwitch;
                case "生产线边空料架返回":
                    return DistributeReqTypes.ProductNullCartBack;
                case "生产线边外侧到里侧":
                    return DistributeReqTypes.ProductOutToIn;
                case "生产线边里侧到外侧":
                    return DistributeReqTypes.ProductInToOut;
                case "绑定货架":
                    return DistributeReqTypes.BindPod;
                case "解绑货架":
                    return DistributeReqTypes.UnBindPod;
                case "点对点配送":
                    return DistributeReqTypes.PointToPointDistribute;
                default:
                    return DistributeReqTypes.PickAreaInit;
        
            }
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk1 = (CheckBox)sender;

            List<DistributeTaskModel> distributeTaskModels = this.dataGrid.ItemsSource as List<DistributeTaskModel>;
            if (distributeTaskModels != null && distributeTaskModels.Count > 0)
            {
                foreach (DistributeTaskModel distributeTaskModel in distributeTaskModels)
                {
                    distributeTaskModel.IsChecked = chk1.IsChecked.Value;
                }

                this.dataGrid.ItemsSource = distributeTaskModels;
            }
        }

        private void cb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            DistributeTaskModel vd = chk.Tag as DistributeTaskModel;
            vd.IsChecked = chk.IsChecked.Value;
        }

        private void btnReSendTask_Click(object sender, RoutedEventArgs e)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<long> SelDistributeTaskIds = new List<long>();
                List<DistributeTaskModel> distributeTaskModels = this.dataGrid.ItemsSource as List<DistributeTaskModel>;
                if (distributeTaskModels != null && distributeTaskModels.Count > 0)
                {
                    foreach (DistributeTaskModel distributeTaskModel in distributeTaskModels)
                    {
                        if (distributeTaskModel.IsChecked)
                        {
                            SelDistributeTaskIds.Add(distributeTaskModel.ID);
                        }
                    }
                }

                if (SelDistributeTaskIds.Count == 0)
                {
                    MessageBox.Show("请先选择未响应的配送任务");
                    return; 
                }

                List<DST_DistributeTask> dstDistributeTasks = dbContext.DST_DistributeTasks.Where(t => SelDistributeTaskIds.Contains(t.ID) && !t.isResponse && t.sendErrorCount >= 5).ToList();
                if (dstDistributeTasks.Count == 0)
                {
                    MessageBox.Show("选择的配送任务中没有未响应且发送错误次数大于5的任务");
                    return; 
                }
                foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                {
                    dstDistributeTask.sendErrorCount = 0;
                }

                if (dbContext.SaveChanges() > 0)
                {
                    DoSearch();
                    MessageBox.Show("重发成功");
                }
                else
                {
                    MessageBox.Show("重发失败");
                }
            }
        }

        private void btnStopTask_Click(object sender, RoutedEventArgs e)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<long> SelDistributeTaskIds = new List<long>();
                List<DistributeTaskModel> distributeTaskModels = this.dataGrid.ItemsSource as List<DistributeTaskModel>;
                if (distributeTaskModels != null && distributeTaskModels.Count > 0)
                {
                    foreach (DistributeTaskModel distributeTaskModel in distributeTaskModels)
                    {
                        if (distributeTaskModel.IsChecked)
                        {
                            //if (distributeTaskModel.DistributeReqTypes != DistributeReqTypes.ProductAreaInit && distributeTaskModel.DistributeReqTypes != DistributeReqTypes.MaterialMarketDistribute) 
                            //{
                            //    MessageBox.Show("选择的配送任务中存在不为生产线边铺线和物料超市配送的任务，不能强制结束");
                            //    return;
                            //}
                            SelDistributeTaskIds.Add(distributeTaskModel.ID);
                        }
                    }
                }

                if (SelDistributeTaskIds.Count == 0)
                {
                    MessageBox.Show("请先选择需要强制结束的配送任务");
                    return;
                }

                List<DST_DistributeTask> dstDistributeTasks = dbContext.DST_DistributeTasks.Where(t => SelDistributeTaskIds.Contains(t.ID)).ToList();
                if (dstDistributeTasks.Count >0)
                {
                    foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                    {
                        dstDistributeTask.sendErrorCount = 5;
                    }
                }
                
                if (dbContext.SaveChanges() > 0)
                {
                    DoSearch();
                    MessageBox.Show("强制结束配送任务成功");
                }
                else
                {
                    MessageBox.Show("强制结束配送任务失败");
                }
            }
        }
    }
}