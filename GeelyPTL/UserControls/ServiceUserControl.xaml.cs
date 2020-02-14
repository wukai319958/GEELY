using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Aris.SystemExtension.Xml.Serialization;
using DataAccess;
using DataAccess.Config;
using DataAccess.Distributing;
using Distributing;

namespace GeelyPTL.UserControls
{
    /// <summary>
    /// 服务控制界面。
    /// </summary>
    public partial class ServiceUserControl : UserControl
    {
        /// <summary>
        /// 初始化服务控制界面。
        /// </summary>
        public ServiceUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            //主题切换会引发多次 Loaded 事件，所以改到 Initialized 事件里
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            try
            {
                //开发早期启用重建数据库
                //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<GeelyPtlEntities>());

                //之后使用数据迁移
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<GeelyPtlEntities, DataAccess.Migrations.Configuration>());

                this.passwordBoxConnectionStingPassword.Password = this.viewModel.ConnectionStringPassword;
                this.viewModel.StartServices();

                //初始化AGV开关服务
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    DST_AgvSwitch dstAgvSwitch = dbContext.DST_AgvSwitchs.FirstOrDefault();
                    if (dstAgvSwitch != null)
                    {
                        if (dstAgvSwitch.isOpen)
                        {
                            this.btnOpenAgv.IsEnabled = false;
                            this.btnCloseAgv.IsEnabled = true;
                            this.txtAgvStatusDesc.Text = "● AGV配送PTL服务已开启";
                        }
                        else
                        {
                            this.btnOpenAgv.IsEnabled = true;
                            this.btnCloseAgv.IsEnabled = false;
                            this.txtAgvStatusDesc.Text = "● AGV配送PTL服务已关闭";
                        }
                    }
                }

                //初始化AGV物料超市配送服务
                string StartMaterialMarketDistributeFlag = System.Configuration.ConfigurationManager.AppSettings["StartMaterialMarketDistribute"];
                if (StartMaterialMarketDistributeFlag.Equals("yes"))
                {
                    this.btnOpenMaterialMarketDistribute.IsEnabled = false;
                    this.btnCloseMaterialMarketDistribute.IsEnabled = true;
                    this.txtMaterialMarketDistributeDesc.Text = "● AGV物料超市配送服务已开启";
                }
                else
                {
                    this.btnOpenMaterialMarketDistribute.IsEnabled = true;
                    this.btnCloseMaterialMarketDistribute.IsEnabled = false;
                    this.txtMaterialMarketDistributeDesc.Text = "● AGV物料超市配送服务已关闭";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "服务设置", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonBrowseLesToPtlService_Click(object sender, RoutedEventArgs e)
        {
            string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/LesToPtlService/", this.viewModel.ServiceIP, this.viewModel.ServicePort);
            this.BrowseUrl(url);
        }

        private void buttonBrowseMesToPtlService_Click(object sender, RoutedEventArgs e)
        {
            string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/MesToPtlService/", this.viewModel.ServiceIP, this.viewModel.ServicePort);
            this.BrowseUrl(url);
        }

        private void buttonBrowsePtlToLesService_Click(object sender, RoutedEventArgs e)
        {
            string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/Service/PtlToLesService?wsdl", this.viewModel.LesServiceIP, this.viewModel.LesServicePort);
            this.BrowseUrl(url);
        }

        private void buttonBrowsePtlToMesService_Click(object sender, RoutedEventArgs e)
        {
            string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/mes-interface/remote/toMes?wsdl", this.viewModel.MesServiceIP, this.viewModel.MesServicePort);
            this.BrowseUrl(url);
        }

        private void buttonBrowsePtlToAgvService_Click(object sender, RoutedEventArgs e)
        {
            string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/rcs/services/rest/hikRpcService", this.viewModel.AgvServiceIP, this.viewModel.AgvServicePort);
            this.BrowseUrl(url);
        }

        void BrowseUrl(string url)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(url);

                Process process = Process.Start(startInfo);
                if (process != null)
                    process.Close();
            }
            catch { }
        }

        private void passwordBoxConnectionStingPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.viewModel.ConnectionStringPassword = this.passwordBoxConnectionStingPassword.Password;
        }

        private void buttonTestConnectionString_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(this.viewModel.ConnectionStringProviderName);
                using (DbConnection connection = factory.CreateConnection())
                {
                    connection.ConnectionString = string.Format(CultureInfo.InvariantCulture, this.viewModel.ConnectionStringFormat, this.viewModel.ConnectionStringPassword);
                    connection.Open();
                }

                MessageBox.Show("连接成功。", "服务设置", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "服务设置", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XmlSerializerWrapper<LocalSettings> xmlSerializer = new XmlSerializerWrapper<LocalSettings>();
                LocalSettings localSettings = xmlSerializer.Entity;

                bool connectionStringChanged = false;
                if (localSettings.ConnectionStringProviderName != this.viewModel.ConnectionStringProviderName
                    || localSettings.ConnectionStringFormat != this.viewModel.ConnectionStringFormat
                    || localSettings.ConnectionStringPassword != this.viewModel.ConnectionStringPassword)
                {
                    if (MessageBox.Show(@"注意：

修改数据库连接信息需要重新启动应用程序，确定修改并重启？

请先确保没有正在进行的作业。
", "服务设置", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        connectionStringChanged = true;
                    }
                }

                localSettings.ServiceIP = this.viewModel.ServiceIP;
                localSettings.ServicePort = this.viewModel.ServicePort;
                localSettings.PtlToLesServiceUrl = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/Service/PtlToLesService", this.viewModel.LesServiceIP, this.viewModel.LesServicePort);
                localSettings.PtlToMesServiceUrl = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/mes-interface/remote/toMes", this.viewModel.MesServiceIP, this.viewModel.MesServicePort);
                localSettings.PtlToAgvServiceUrl = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/rcs/services/rest/hikRpcService", this.viewModel.AgvServiceIP, this.viewModel.AgvServicePort);
                localSettings.ConnectionStringProviderName = this.viewModel.ConnectionStringProviderName;
                localSettings.ConnectionStringFormat = this.viewModel.ConnectionStringFormat;
                localSettings.ConnectionStringPassword = this.viewModel.ConnectionStringPassword;
                localSettings.HistoryHoldingDays = this.viewModel.HistoryHoldingDays;

                xmlSerializer.Save();

                if (connectionStringChanged)
                {
                    Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();

                    return;
                }
                else
                {
                    this.viewModel.RestartSafelyServices();

                    MessageBox.Show("保存成功并重新启动各业务服务。", "服务设置", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "服务设置", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 拣料区铺线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPickAreaInit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否要开始拣料区铺线？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                GeelyPTL.Dialogs.SelectInitChannelWindow dlg = new GeelyPTL.Dialogs.SelectInitChannelWindow();
                dlg.ShowDialog();
                if (dlg.InitChannelIds.Count > 0)
                {
                    bool isSuccess = DistributingTaskGenerator.Instance.GeneratePickAreaInitTask(dlg.InitChannelIds, true);
                    if (!isSuccess)
                    {
                        MessageBox.Show("拣料区铺线失败");
                        return;
                    }
                    MessageBox.Show("拣料区铺线成功");
                }

                #region 注释
                //                string xml = string.Format(@"<?xml version='1.0' encoding='UTF-8'?>
//<Service>
//  <Route>
//    <ServiceResponse/>
//    <ServiceID>03024000000002</ServiceID>
//    <SerialNO>2018092502008733808</SerialNO>
//    <ServiceTime>20180925140415</ServiceTime>
//    <SourceSysID>02008</SourceSysID>
//  </Route>
//  <Data>
//    <Control/>
//    <Request>
//      <ASSEMBLE p_type='G' loop_num='1'>
//        <ProjectCode>SX12</ProjectCode>
//        <PS_POSID></PS_POSID>
//        <StageCode>VP2</StageCode>
//        <PickNO>85603461</PickNO>
//        <Bill_Date>2018-09-25 11:08:33.0</Bill_Date>
//        <STATIONCODE></STATIONCODE>
//        <GZZLIST></GZZLIST>
//        <BatchCode>X</BatchCode>
//        <ChannelCode>3</ChannelCode>
//        <PalletCode>P020983</PalletCode>
//        <BoxCode>WB180912139269D</BoxCode>
//        <FromPalletPosition>4</FromPalletPosition>
//        <TOTALNUM>1</TOTALNUM>
//      </ASSEMBLE>
//      <ASSEMBLEITEM p_type='G' loop_num='1'>
//        <BillDtlID>85603496</BillDtlID>
//        <MaterialCode>6600006154</MaterialCode>
//        <MaterialName>倒车雷达探头</MaterialName>
//        <MaterialBarcode>B6600006154_18091800001</MaterialBarcode>
//        <NEED_PICK_NUM>2</NEED_PICK_NUM>
//        <MaxQuantityInSingleCartPosition>0</MaxQuantityInSingleCartPosition>
//        <IsSpecial>0</IsSpecial>
//        <STORETYPE>03</STORETYPE>
//      </ASSEMBLEITEM>
//    </Request>
//    <Response/>
//  </Data>
//</Service>");
                //                string result = Interfaces.Services.LesToPtlService.Instance.LesStockPickPDA(xml);
                #endregion
            }
        }

        /// <summary>
        /// 物料超市配送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMarketDistribute_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否要开始物料超市配送？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                GeelyPTL.Dialogs.SelectWorkStationlWindow dlg = new GeelyPTL.Dialogs.SelectWorkStationlWindow(0);
                dlg.ShowDialog();
                if (dlg.InitWorkStationIds.Count > 0)
                {
                    string result = DistributingTaskGenerator.Instance.GenerateBatchMarketDistributeTask(dlg.InitWorkStationIds, true);
                    MessageBox.Show(result);
                }
            }
        }

        /// <summary>
        /// 生产线边清线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProductAreaClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否要开始生产线边清线？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                GeelyPTL.Dialogs.SelectWorkStationlWindow dlg = new GeelyPTL.Dialogs.SelectWorkStationlWindow(1);
                dlg.ShowDialog();
                if (dlg.InitWorkStationIds.Count > 0)
                {
                    string result = DistributingTaskGenerator.Instance.GenerateBatchProductInToOutTask(dlg.InitWorkStationIds, true);
                    MessageBox.Show(result);
                }
            }
        }

        /// <summary>
        /// 生产线边空满交换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCartSwitch_Click(object sender, RoutedEventArgs e)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                GeelyPTL.Dialogs.SwitchCartWindow dlg = new GeelyPTL.Dialogs.SwitchCartWindow();
                dlg.ShowDialog();
                if (dlg.DialogResult == false)
                {
                    return;
                }
                string sSwitchCartCode = dlg.SwitchCartCode;
                if (string.IsNullOrEmpty(sSwitchCartCode))
                {
                    MessageBox.Show("请先填写要进行空满交换的料架编码");
                    return;
                }
                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Code.Equals(sSwitchCartCode));
                if (cfgCart == null)
                {
                    MessageBox.Show("没有找到对应的料架");
                    return;
                }
                if (MessageBox.Show("是否要开始生产线边空满交换？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    ////解除停靠
                    //CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                    //                                                           .FirstOrDefault(wscc => wscc.CFG_CartId == cfgCart.Id);
                    //if (cfgWorkStationCurrentCart != null)
                    //{
                    //    cfgWorkStationCurrentCart.CFG_CartId = null;
                    //    cfgWorkStationCurrentCart.DockedTime = null;
                    //}
                    List<DST_DistributeTask> distributeTasks = DistributingTaskGenerator.Instance.GenerateProductCartSwitchTask(cfgCart);
                    foreach (DST_DistributeTask distributeTask in distributeTasks)
                    {
                        dbContext.DST_DistributeTasks.Add(distributeTask);
                    }
                    string result = dbContext.SaveChanges() > 0 ? "生成空满交换任务成功" : "生成空满交换任务失败";
                    MessageBox.Show(result);
                }
            }
        }

        /// <summary>
        /// 生产线边空料架返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNullCartBack_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否要开始生产线边空料架返回？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                GeelyPTL.Dialogs.SelectWorkStationlWindow dlg = new GeelyPTL.Dialogs.SelectWorkStationlWindow(2);
                dlg.ShowDialog();
                if (dlg.InitWorkStationIds.Count > 0)
                {
                    string result = DistributingTaskGenerator.Instance.GeneratePTLNullCartBackTask(dlg.InitWorkStationIds);
                    MessageBox.Show(result);
                }
            }
        }

        /// <summary>
        /// 开启AGV配送PTL服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenAgv_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否要开启AGV配送PTL服务？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    DST_AgvSwitch dstAgvSwitch = dbContext.DST_AgvSwitchs.FirstOrDefault();
                    if (dstAgvSwitch != null)
                    {
                        dstAgvSwitch.isOpen = true;
                        dstAgvSwitch.lastOpenTime = DateTime.Now;

                        int result = dbContext.SaveChanges();
                        if (result <= 0)
                        {
                            MessageBox.Show("开启失败");
                            return;
                        }
                        //this.viewModel.OpenAgvServices();
                        MessageBox.Show("开启成功");
                        this.btnOpenAgv.IsEnabled = false;
                        this.btnCloseAgv.IsEnabled = true;
                        this.txtAgvStatusDesc.Text = "● AGV配送PTL服务已开启";
                    }
                }
            }
        }

        /// <summary>
        /// 关闭AGV配送PTL服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseAgv_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否要关闭AGV配送PTL服务？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    DST_AgvSwitch dstAgvSwitch = dbContext.DST_AgvSwitchs.FirstOrDefault();
                    if (dstAgvSwitch != null)
                    {
                        dstAgvSwitch.isOpen = false;
                        dstAgvSwitch.lastCloseTime = DateTime.Now;

                        int result = dbContext.SaveChanges();
                        if (result <= 0)
                        {
                            MessageBox.Show("关闭失败");
                            return;
                        }
                        //this.viewModel.CloseAgvService();
                        MessageBox.Show("关闭成功");
                        this.btnOpenAgv.IsEnabled = true;
                        this.btnCloseAgv.IsEnabled = false;
                        this.txtAgvStatusDesc.Text = "● AGV配送PTL服务已关闭";
                    }
                }
            }
        }

        /// <summary>
        /// 开启物料超市配送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenMaterialMarketDistribute_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否要开启AGV物料超市配送服务？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                string key = "StartMaterialMarketDistribute";
                string value = "yes";
                this.SaveAppSettings(key, value);

                this.btnOpenMaterialMarketDistribute.IsEnabled = false;
                this.btnCloseMaterialMarketDistribute.IsEnabled = true;
                this.txtMaterialMarketDistributeDesc.Text = "● AGV物料超市配送服务已开启";
            }
        }

        /// <summary>
        /// 关闭物料超市配送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseMaterialMarketDistribute_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否要关闭AGV物料超市配送服务？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                string key = "StartMaterialMarketDistribute";
                string value = "no";
                this.SaveAppSettings(key, value);

                this.btnOpenMaterialMarketDistribute.IsEnabled = true;
                this.btnCloseMaterialMarketDistribute.IsEnabled = false;
                this.txtMaterialMarketDistributeDesc.Text = "● AGV物料超市配送服务已关闭";
            }
        }

        private void SaveAppSettings(string key, string value)
        {
            // 创建配置文件对象
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] != null)
            {
                // 修改
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                // 添加
                AppSettingsSection ass = (AppSettingsSection)config.GetSection("appSettings");
                ass.Settings.Add(key, value);
            }

            // 保存修改
            config.Save(ConfigurationSaveMode.Modified);

            // 强制重新载入配置文件的连接配置节
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}