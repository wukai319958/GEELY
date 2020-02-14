using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Aris.SystemExtension.ComponentModel;
using Aris.SystemExtension.Xml.Serialization;
using AssemblyIndicating;
using Assorting;
using CartFinding;
using DataAccess;
using DataAccess.Config;
using DeviceCommunicationHost;
using Interfaces;
using Interfaces.Business;
using Interfaces.DistributingServices;

namespace GeelyPTL.ViewModels
{
    /// <summary>
    /// 服务控制界面的视图模型。
    /// </summary>
    public class ServiceViewModel : ObservableObject
    {
        /// <summary>
        /// 获取本机 IP 地址的集合。
        /// </summary>
        public ObservableCollection<string> IPCollection { get; private set; }

        string serviceIP;
        int servicePort;
        string lesServiceIP;
        int lesServicePort;
        string mesServiceIP;
        int mesServicePort;
        string agvServiceIP;
        int agvServicePort;
        string connectionStringProviderName;
        string connectionStringFormat;
        string connectionStringPassword;
        int historyHoldingDays;

        bool lesToPtlServiceIsRunning;
        bool mesToPtlServiceIsRunning;
        bool ptlToLesServiceIsRunning;
        bool ptlToMesServiceIsRunning;
        bool ptlToAgvServiceIsRunning;
        bool databaseIsOnline;
        bool channelPtlIsRunning;
        bool cartPtlIsRunning;
        bool assortingTaskLoaderIsRunning;
        bool assortingResultWriteBackIsRunning;
        bool cartFindingTaskLoaderIsRunning;
        bool cartFindingResultWriteBackIsRunning;
        bool assembleTaskLoaderIsRunning;
        bool assembleResultWriteBackIsRunning;
        bool historyRemoverIsRunning;
        bool pickAreaServiceIsRunning;
        bool productAreaServiceIsRunning;
        bool initServiceIsRunning;

        readonly TimeSpan statusRefreshPeriod = TimeSpan.FromSeconds(1);
        Thread runningStatusRefreshThread;
        bool runningStatusRefreshThreadNeedQuit;

        /// <summary>
        /// 获取或设置承载的各服务的 IP。
        /// </summary>
        public string ServiceIP
        {
            get { return this.serviceIP; }
            set { this.SetProperty(() => this.ServiceIP, ref this.serviceIP, value); }
        }

        /// <summary>
        /// 获取或设置承载的各服务的端口。
        /// </summary>
        public int ServicePort
        {
            get { return this.servicePort; }
            set { this.SetProperty(() => this.ServicePort, ref this.servicePort, value); }
        }

        /// <summary>
        /// 获取或设置 LES 服务的 IP。
        /// </summary>
        public string LesServiceIP
        {
            get { return this.lesServiceIP; }
            set { this.SetProperty(() => this.LesServiceIP, ref this.lesServiceIP, value); }
        }

        /// <summary>
        /// 获取或设置 LES 服务的端口。
        /// </summary>
        public int LesServicePort
        {
            get { return this.lesServicePort; }
            set { this.SetProperty(() => this.LesServicePort, ref this.lesServicePort, value); }
        }

        /// <summary>
        /// 获取或设置 MES 服务的 IP。
        /// </summary>
        public string MesServiceIP
        {
            get { return this.mesServiceIP; }
            set { this.SetProperty(() => this.MesServiceIP, ref this.mesServiceIP, value); }
        }

        /// <summary>
        /// 获取或设置 MES 服务的端口。
        /// </summary>
        public int MesServicePort
        {
            get { return this.mesServicePort; }
            set { this.SetProperty(() => this.MesServicePort, ref this.mesServicePort, value); }
        }

        /// <summary>
        /// 获取或设置 AGV 服务的 IP。
        /// </summary>
        public string AgvServiceIP
        {
            get { return this.agvServiceIP; }
            set { this.SetProperty(() => this.AgvServiceIP, ref this.agvServiceIP, value); }
        }

        /// <summary>
        /// 获取或设置 AGV 服务的端口。
        /// </summary>
        public int AgvServicePort
        {
            get { return this.agvServicePort; }
            set { this.SetProperty(() => this.AgvServicePort, ref this.agvServicePort, value); }
        }

        /// <summary>
        /// 获取或设置数据库提供者。
        /// </summary>
        public string ConnectionStringProviderName
        {
            get { return this.connectionStringProviderName; }
            set { this.SetProperty(() => this.ConnectionStringProviderName, ref this.connectionStringProviderName, value); }
        }

        /// <summary>
        /// 获取或设置数据库连接字符串的模板，密码使用 {0} 占位符。
        /// </summary>
        public string ConnectionStringFormat
        {
            get { return this.connectionStringFormat; }
            set { this.SetProperty(() => this.ConnectionStringFormat, ref this.connectionStringFormat, value); }
        }

        /// <summary>
        /// 获取或设置数据库连接字符串的密码。
        /// </summary>
        public string ConnectionStringPassword
        {
            get { return this.connectionStringPassword; }
            set { this.SetProperty(() => this.ConnectionStringPassword, ref this.connectionStringPassword, value); }
        }

        /// <summary>
        /// 获取或设置日志保存的天数。
        /// </summary>
        public int HistoryHoldingDays
        {
            get { return this.historyHoldingDays; }
            set { this.SetProperty(() => this.HistoryHoldingDays, ref this.historyHoldingDays, value); }
        }

        /// <summary>
        /// 获取或设置 LES 到 PTL 的服务是否运行中。
        /// </summary>
        public bool LesToPtlServiceIsRunning
        {
            get { return this.lesToPtlServiceIsRunning; }
            set { this.SetProperty(() => this.LesToPtlServiceIsRunning, ref this.lesToPtlServiceIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置 MES 到 PTL 的服务是否运行中。
        /// </summary>
        public bool MesToPtlServiceIsRunning
        {
            get { return this.mesToPtlServiceIsRunning; }
            set { this.SetProperty(() => this.MesToPtlServiceIsRunning, ref this.mesToPtlServiceIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置 PTL 到 LES 的服务是否可用。
        /// </summary>
        public bool PtlToLesServiceIsRunning
        {
            get { return this.ptlToLesServiceIsRunning; }
            set { this.SetProperty(() => this.PtlToLesServiceIsRunning, ref this.ptlToLesServiceIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置 PTL 到 MES 的服务是否可用。
        /// </summary>
        public bool PtlToMesServiceIsRunning
        {
            get { return this.ptlToMesServiceIsRunning; }
            set { this.SetProperty(() => this.PtlToMesServiceIsRunning, ref this.ptlToMesServiceIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置 PTL 到 AGV 的服务是否可用。
        /// </summary>
        public bool PtlToAgvServiceIsRunning
        {
            get { return this.ptlToAgvServiceIsRunning; }
            set { this.SetProperty(() => this.PtlToAgvServiceIsRunning, ref this.ptlToAgvServiceIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置数据库是否在线。
        /// </summary>
        public bool DatabaseIsOnline
        {
            get { return this.databaseIsOnline; }
            set { this.SetProperty(() => this.DatabaseIsOnline, ref this.databaseIsOnline, value); }
        }

        /// <summary>
        /// 获取或设置分拣口指示灯是否运行中。
        /// </summary>
        public bool ChannelPtlIsRunning
        {
            get { return this.channelPtlIsRunning; }
            set { this.SetProperty(() => this.ChannelPtlIsRunning, ref this.channelPtlIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置料车指示灯是否运行中。
        /// </summary>
        public bool CartPtlIsRunning
        {
            get { return this.cartPtlIsRunning; }
            set { this.SetProperty(() => this.CartPtlIsRunning, ref this.cartPtlIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置分拣任务加载器是否运行中。
        /// </summary>
        public bool AssortingTaskLoaderIsRunning
        {
            get { return this.assortingTaskLoaderIsRunning; }
            set { this.SetProperty(() => this.AssortingTaskLoaderIsRunning, ref this.assortingTaskLoaderIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置分拣结果回传业务是否运行中。
        /// </summary>
        public bool AssortingResultWriteBackIsRunning
        {
            get { return this.assortingResultWriteBackIsRunning; }
            set { this.SetProperty(() => this.AssortingResultWriteBackIsRunning, ref this.assortingResultWriteBackIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置料车配送任务加载器是否运行中。
        /// </summary>
        public bool CartFindingTaskLoaderIsRunning
        {
            get { return this.cartFindingTaskLoaderIsRunning; }
            set { this.SetProperty(() => this.CartFindingTaskLoaderIsRunning, ref this.cartFindingTaskLoaderIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置料车配送结果回传业务是否运行中。
        /// </summary>
        public bool CartFindingResultWriteBackIsRunning
        {
            get { return this.cartFindingResultWriteBackIsRunning; }
            set { this.SetProperty(() => this.CartFindingResultWriteBackIsRunning, ref this.cartFindingResultWriteBackIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置装配指引任务加载器是否运行中。
        /// </summary>
        public bool AssembleTaskLoaderIsRunning
        {
            get { return this.assembleTaskLoaderIsRunning; }
            set { this.SetProperty(() => this.AssembleTaskLoaderIsRunning, ref this.assembleTaskLoaderIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置装配结果回传业务是否运行中。
        /// </summary>
        public bool AssembleResultWriteBackIsRunning
        {
            get { return this.assembleResultWriteBackIsRunning; }
            set { this.SetProperty(() => this.AssembleResultWriteBackIsRunning, ref this.assembleResultWriteBackIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置历史数据删除业务是否运行中。
        /// </summary>
        public bool HistoryRemoverIsRunning
        {
            get { return this.historyRemoverIsRunning; }
            set { this.SetProperty(() => this.HistoryRemoverIsRunning, ref this.historyRemoverIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置拣料区配送任务发送业务是否运行中
        /// </summary>
        public bool PickAreaServiceIsRunning
        {
            get { return this.pickAreaServiceIsRunning; }
            set { this.SetProperty(() => this.PickAreaServiceIsRunning, ref this.pickAreaServiceIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置线边配送任务发送业务是否运行中
        /// </summary>
        public bool ProductAreaServiceIsRunning
        {
            get { return this.productAreaServiceIsRunning; }
            set { this.SetProperty(() => this.ProductAreaServiceIsRunning, ref this.productAreaServiceIsRunning, value); }
        }

        /// <summary>
        /// 获取或设置初始化配送任务发送业务是否运行中
        /// </summary>
        public bool InitServiceIsRunning
        {
            get { return this.initServiceIsRunning; }
            set { this.SetProperty(() => this.InitServiceIsRunning, ref this.initServiceIsRunning, value); }
        }

        /// <summary>
        /// 初始化视图模型。
        /// </summary>
        public ServiceViewModel()
        {
            this.IPCollection = new ObservableCollection<string>();

            string hostName = Dns.GetHostName();
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            this.IPCollection.Add("localhost");
            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    this.IPCollection.Add(ipAddress.ToString());
            }

            this.InitializeSettings();
        }

        void InitializeSettings()
        {
            LocalSettings localSettings = new XmlSerializerWrapper<LocalSettings>().Entity;
            Uri ptlToLesServiceUrl = new Uri(localSettings.PtlToLesServiceUrl);
            Uri ptlToMesServiceUrl = new Uri(localSettings.PtlToMesServiceUrl);
            Uri ptlToAgvServiceUrl = new Uri(localSettings.PtlToAgvServiceUrl);

            this.ServiceIP = localSettings.ServiceIP;
            this.ServicePort = localSettings.ServicePort;
            this.LesServiceIP = ptlToLesServiceUrl.Host;
            this.LesServicePort = ptlToLesServiceUrl.Port;
            this.MesServiceIP = ptlToMesServiceUrl.Host;
            this.MesServicePort = ptlToMesServiceUrl.Port;
            this.AgvServiceIP = ptlToAgvServiceUrl.Host;
            this.AgvServicePort = ptlToAgvServiceUrl.Port;
            this.ConnectionStringProviderName = localSettings.ConnectionStringProviderName;
            this.ConnectionStringFormat = localSettings.ConnectionStringFormat;
            this.ConnectionStringPassword = localSettings.ConnectionStringPassword;
            this.HistoryHoldingDays = localSettings.HistoryHoldingDays;
        }

        /// <summary>
        /// 启动所有服务。
        /// </summary>
        public void StartServices()
        {
            LocalSettings localSettings = new XmlSerializerWrapper<LocalSettings>().Entity;

            //使用 LocalSettings 里的数据库连接信息覆盖 App.config 里的
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringSettings connectionStringSettings = configuration.ConnectionStrings.ConnectionStrings["GeelyPtlEntities"];
            connectionStringSettings.ProviderName = localSettings.ConnectionStringProviderName;
            connectionStringSettings.ConnectionString = string.Format(CultureInfo.InvariantCulture, localSettings.ConnectionStringFormat, localSettings.ConnectionStringPassword);
            configuration.Save();
            ConfigurationManager.RefreshSection("connectionStrings");

            //确保基础数据已存在于数据库
            BaseDatasInitializer.EnsureInitialized();

            //启动通讯之前重值所有设备的在线状态
            this.DeviceOnLineStatusReset();

            //承载所有接口服务
            ServiceHosts.Open(localSettings.ServiceIP, localSettings.ServicePort);

            //启动设备通讯
            CartPtlHost.Instance.Start();
            ChannelPtlHost.Instance.Start();

            //启动PickZone通讯
            PickZoneHost.Instance.Start();
            //启动FeedZone通讯（分装工位）
            FeedZoneHost.Instance.Start();
            CacheRegionHost.Instance.Start();
            AssemblySectionHost.Instance.Start();

            //启动MarketZone通讯
            MarketZoneHost.Instance.Start();

            //启动任务加载业务
            AssortingExecutorLoader.Instance.Start();
            CartFindingExecutor.Instance.Start();
            IndicatingExecutorLoader.Instance.Start();

            //启动结果回写业务
            AssortResultWriteBack.Instance.Start(localSettings.PtlToLesServiceUrl);
            CartFindingDeliveryResultWriteBack.Instance.Start(localSettings.PtlToLesServiceUrl);
            AssembleResultWriteBack.Instance.Start(localSettings.PtlToMesServiceUrl);

            //启动历史记录清除器
            Ptl.Device.Log.Logger.HoldingPeriodInDays = localSettings.HistoryHoldingDays;
            HistoryRecordsRemover.Instance.Start(localSettings.HistoryHoldingDays);

            //启动AGV配送任务发送业务
            PickAreaService.Instance.Start(localSettings.PtlToAgvServiceUrl);
            ProductAreaService.Instance.Start(localSettings.PtlToAgvServiceUrl);
            InitService.Instance.Start(localSettings.PtlToAgvServiceUrl);

            //启动状态监控
            this.StartRunningStatusRefreshThread();
        }

        /// <summary>
        /// 停止所有服务。
        /// </summary>
        public void StopServices()
        {
            this.StopRunningStatusRefreshThread();

            PickAreaService.Instance.Stop();
            ProductAreaService.Instance.Stop();
            InitService.Instance.Stop();

            Ptl.Device.Log.Logger.HoldingPeriodInDays = int.MaxValue;
            HistoryRecordsRemover.Instance.Stop();

            AssortResultWriteBack.Instance.Stop();
            CartFindingDeliveryResultWriteBack.Instance.Stop();
            AssembleResultWriteBack.Instance.Stop();

            AssortingExecutorLoader.Instance.Stop();
            CartFindingExecutor.Instance.Stop();
            IndicatingExecutorLoader.Instance.Stop();

            CartPtlHost.Instance.Stop();
            ChannelPtlHost.Instance.Stop();

            //关闭PickZone通讯
            PickZoneHost.Instance.Stop();
            //关闭FeedZone通讯（分装工位）
            FeedZoneHost.Instance.Stop();        
            CacheRegionHost.Instance.Stop();
            AssemblySectionHost.Instance.Stop();

            //关闭MarketZone通讯
            MarketZoneHost.Instance.Stop();

            ServiceHosts.Close();

            this.DeviceOnLineStatusReset();
        }

        /// <summary>
        /// 重新启动所有可临时中断的服务。
        /// </summary>
        public void RestartSafelyServices()
        {
            LocalSettings localSettings = new XmlSerializerWrapper<LocalSettings>().Entity;

            //停止所有可临时中断的服务
            this.StopRunningStatusRefreshThread();

            PickAreaService.Instance.Stop();
            ProductAreaService.Instance.Stop();
            InitService.Instance.Stop();

            HistoryRecordsRemover.Instance.Stop();

            AssortResultWriteBack.Instance.Stop();
            CartFindingDeliveryResultWriteBack.Instance.Stop();
            AssembleResultWriteBack.Instance.Stop();

            AssortingExecutorLoader.Instance.Stop();
            CartFindingExecutor.Instance.Stop();
            IndicatingExecutorLoader.Instance.Stop();

            ServiceHosts.Close();

            //启动所有可临时中断的服务
            ServiceHosts.Open(localSettings.ServiceIP, localSettings.ServicePort);

            AssortingExecutorLoader.Instance.Start();
            CartFindingExecutor.Instance.Start();
            IndicatingExecutorLoader.Instance.Start();

            AssortResultWriteBack.Instance.Start(localSettings.PtlToLesServiceUrl);
            CartFindingDeliveryResultWriteBack.Instance.Start(localSettings.PtlToLesServiceUrl);
            AssembleResultWriteBack.Instance.Start(localSettings.PtlToMesServiceUrl);

            HistoryRecordsRemover.Instance.Start(localSettings.HistoryHoldingDays);

            //启动AGV配送任务发送业务
            PickAreaService.Instance.Start(localSettings.PtlToAgvServiceUrl);
            ProductAreaService.Instance.Start(localSettings.PtlToAgvServiceUrl);
            InitService.Instance.Start(localSettings.PtlToAgvServiceUrl);

            this.StartRunningStatusRefreshThread();
        }

        /// <summary>
        /// 开启AGV服务
        /// </summary>
        public void OpenAgvServices()
        {
            LocalSettings localSettings = new XmlSerializerWrapper<LocalSettings>().Entity;

            PickAreaService.Instance.Stop();
            ProductAreaService.Instance.Stop();
            InitService.Instance.Stop();

            PickAreaService.Instance.Start(localSettings.PtlToAgvServiceUrl);
            ProductAreaService.Instance.Start(localSettings.PtlToAgvServiceUrl);
            InitService.Instance.Start(localSettings.PtlToAgvServiceUrl);
        }

        /// <summary>
        /// 关闭AGV服务
        /// </summary>
        public void CloseAgvService()
        {
            PickAreaService.Instance.Stop();
            ProductAreaService.Instance.Stop();
            InitService.Instance.Stop();
        }

        /// <summary>
        /// 启动通讯之前重值所有设备的在线状态。
        /// </summary>
        void DeviceOnLineStatusReset()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<CFG_Cart> cfgCarts = dbContext.CFG_Carts
                                              .ToList();
                foreach (CFG_Cart cfgCart in cfgCarts)
                {
                    cfgCart.OnLine = false;

                    foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                        cfgCartPtlDevice.OnLine = false;
                }

                List<CFG_Channel> cfgChannels = dbContext.CFG_Channels
                                                    .ToList();
                foreach (CFG_Channel cfgChannel in cfgChannels)
                {
                    foreach (CFG_ChannelPtlDevice cfgChannelPtlDevice in cfgChannel.CFG_ChannelPtlDevices)
                        cfgChannelPtlDevice.OnLine = false;
                }

                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 启动运行时状态监视线程。
        /// </summary>
        void StartRunningStatusRefreshThread()
        {
            this.runningStatusRefreshThread = new Thread(this.runningStatusRefreshThread_Start);
            this.runningStatusRefreshThread.Name = this.GetType().FullName;
            this.runningStatusRefreshThread.IsBackground = true;

            this.runningStatusRefreshThreadNeedQuit = false;
            this.runningStatusRefreshThread.Start();
        }

        /// <summary>
        /// 停止运行时状态监视线程。
        /// </summary>
        void StopRunningStatusRefreshThread()
        {
            if (this.runningStatusRefreshThread != null)
            {
                this.runningStatusRefreshThreadNeedQuit = true;
                try { this.runningStatusRefreshThread.Join(TimeSpan.FromSeconds(1)); }
                catch
                {
                    try { this.runningStatusRefreshThread.Abort(); }
                    catch { }
                }

                this.runningStatusRefreshThread = null;
            }
        }

        void runningStatusRefreshThread_Start()
        {
            while (!this.runningStatusRefreshThreadNeedQuit)
            {
                try
                {
                    try
                    {
                        string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/LesToPtlService/", this.ServiceIP, this.ServicePort);
                        WebClient webClient = new WebClient();
                        webClient.DownloadString(url);

                        this.LesToPtlServiceIsRunning = true;
                    }
                    catch
                    {
                        this.LesToPtlServiceIsRunning = false;
                    }

                    try
                    {
                        string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/MesToPtlService/", this.ServiceIP, this.ServicePort);
                        WebClient webClient = new WebClient();
                        webClient.DownloadString(url);

                        this.MesToPtlServiceIsRunning = true;
                    }
                    catch
                    {
                        this.MesToPtlServiceIsRunning = false;
                    }

                    try
                    {
                        string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/Service/PtlToLesService?wsdl", this.LesServiceIP, this.LesServicePort);
                        WebClient webClient = new WebClient();
                        webClient.DownloadString(url);

                        this.PtlToLesServiceIsRunning = true;
                    }
                    catch
                    {
                        this.PtlToLesServiceIsRunning = false;
                    }

                    try
                    {
                        string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/mes-interface/remote/toMes?wsdl", this.MesServiceIP, this.MesServicePort);
                        WebClient webClient = new WebClient();
                        webClient.DownloadString(url);

                        this.PtlToMesServiceIsRunning = true;
                    }
                    catch
                    {
                        this.PtlToMesServiceIsRunning = false;
                    }

                    try
                    {
                        //string url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/rcs/services/rest/HikRpcService", this.AgvServiceIP, this.AgvServicePort);
                        //WebClient webClient = new WebClient();
                        //webClient.DownloadString(url);
                        this.PtlToAgvServiceIsRunning = true;
                    }
                    catch
                    {
                        this.PtlToAgvServiceIsRunning = false;
                    }

                    try
                    {
                        DbProviderFactory factory = DbProviderFactories.GetFactory(this.ConnectionStringProviderName);
                        using (DbConnection connection = factory.CreateConnection())
                        {
                            connection.ConnectionString = string.Format(CultureInfo.InvariantCulture, this.ConnectionStringFormat, this.ConnectionStringPassword);
                            connection.Open();
                        }

                        this.DatabaseIsOnline = true;
                    }
                    catch
                    {
                        this.DatabaseIsOnline = false;
                    }

                    this.ChannelPtlIsRunning = ChannelPtlHost.Instance.IsRunning;
                    this.CartPtlIsRunning = CartPtlHost.Instance.IsRunning;
                    this.AssortingTaskLoaderIsRunning = AssortingExecutorLoader.Instance.IsRunning;
                    this.AssortingResultWriteBackIsRunning = AssortResultWriteBack.Instance.IsRunning;
                    this.CartFindingTaskLoaderIsRunning = CartFindingExecutor.Instance.IsRunning;
                    this.CartFindingResultWriteBackIsRunning = CartFindingDeliveryResultWriteBack.Instance.IsRunning;
                    this.AssembleTaskLoaderIsRunning = IndicatingExecutorLoader.Instance.IsRunning;
                    this.AssembleResultWriteBackIsRunning = AssembleResultWriteBack.Instance.IsRunning;

                    this.PickAreaServiceIsRunning = PickAreaService.Instance.IsRunning;
                    this.ProductAreaServiceIsRunning = ProductAreaService.Instance.IsRunning;
                    this.initServiceIsRunning = InitService.Instance.IsRunning;

                    this.HistoryRemoverIsRunning = HistoryRecordsRemover.Instance.IsRunning;
                }
                catch { }
                finally
                {
                    Thread.Sleep(this.statusRefreshPeriod);
                }
            }
        }
    }
}