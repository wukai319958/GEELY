using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Aris.SystemExtension.ComponentModel;
using Aris.SystemExtension.Xml.Serialization;
using AssortingKanban.ForAssortingKanban;

namespace AssortingKanban
{
    /// <summary>
    /// 主界面的视图模型。
    /// </summary>
    public class MainWindowViewModel : ObservableObject
    {
        int? channelId;
        string channelName;
        AssortingKanbanTodayStatistics todayStatistics;
        AssortingKanbanTaskInfo taskInfo;
        bool serviceError;

        /// <summary>
        /// 获取或设置所属分拣口的名称。
        /// </summary>
        public string ChannelName
        {
            get { return this.channelName; }
            set { this.SetProperty(() => this.ChannelName, ref this.channelName, value); }
        }

        /// <summary>
        /// 获取或设置今日统计。
        /// </summary>
        public AssortingKanbanTodayStatistics TodayStatistics
        {
            get { return this.todayStatistics; }
            set { this.SetProperty(() => this.TodayStatistics, ref this.todayStatistics, value); }
        }

        /// <summary>
        /// 获取或设置当前任务信息。
        /// </summary>
        public AssortingKanbanTaskInfo TaskInfo
        {
            get { return this.taskInfo; }
            set { this.SetProperty(() => this.TaskInfo, ref this.taskInfo, value); }
        }

        /// <summary>
        /// 获取或设置服务器是否掉线。
        /// </summary>
        public bool ServiceError
        {
            get { return this.serviceError; }
            set { this.SetProperty(() => this.ServiceError, ref this.serviceError, value); }
        }

        readonly string serviceUrl;
        Thread thread;
        readonly TimeSpan pollingPeriod = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 初始化主界面的视图模型。
        /// </summary>
        public MainWindowViewModel()
        {
            LocalSettings localSettings = new XmlSerializerWrapper<LocalSettings>().Entity;
            this.serviceUrl = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/ForAssortingKanbanService/", localSettings.ServerIP, localSettings.ServicePort);

            this.SetupDesignTimeDatas();
        }

        /// <summary>
        /// 初始化设计时数据。
        /// </summary>
        public void SetupDesignTimeDatas()
        {
            this.ChannelName = "巷道一";
            this.TodayStatistics = new AssortingKanbanTodayStatistics
            {
                FinishedBatchCount = 1,
                TotalBatchCount = 2,
                FinishedPalletCount = 3,
                TotalPalletCount = 4,
                FinishedMaterialCount = 567,
                TotalMaterialCount = 890
            };
            this.TaskInfo = new AssortingKanbanTaskInfo
            {
                CurrentBatchInfo = new AssortingKanbanCurrentBatchInfo
                {
                    PickType = "P",
                    ProjectCode = "项目名称",
                    ProjectStep = "阶段",
                    BatchCode = "批次",
                    FinishedPalletCount = 11,
                    TotalPalletCount = 12,
                    FinishedMaterialTypeCount = 13,
                    TotalMaterialTypeCount = 14,
                    FinishedMaterialCount = 15,
                    TotalMaterialCount = 16
                },
                CurrentPalletTask = new AST_PalletTaskDto
                {
                    PalletCode = "托盘编号",
                    PalletType = "01",
                    PalletRotationStatus = PalletRotationStatus.Normal,
                    Items = new AST_PalletTaskItemDto[]
                    {
                        new AST_PalletTaskItemDto {
                            FromPalletPosition = 1,
                            WorkStationCode = "SP01",
                            MaterialCode = "Code1",
                            MaterialName = "Name1",
                            ToPickQuantity = 21,
                            IsSpecial = true,
                            IsBig = false,
                            PickStatus = PickStatus.New,
                            PickedQuantity = null
                        },
                        new AST_PalletTaskItemDto {
                            FromPalletPosition = 3,
                            WorkStationCode = "SP03",
                            MaterialCode = "Code3",
                            MaterialName = "Name3",
                            MaterialBarcode = "Special3Special3Special3",
                            ToPickQuantity = 23,
                            IsSpecial = true,
                            IsBig = false,
                            PickStatus = PickStatus.Picking,
                            PickedQuantity = 13
                        },
                        new AST_PalletTaskItemDto {
                            FromPalletPosition = 5,
                            WorkStationCode = "SP05",
                            MaterialCode = "Code5",
                            MaterialName = "Name5",
                            ToPickQuantity = 25,
                            IsSpecial = false,
                            IsBig = true,
                            PickStatus = PickStatus.Finished,
                            PickedQuantity = 25
                        }
                    }
                },
                CurrentCartTask = new AST_CartTaskDto
                {
                    CartCode = "料车编码",
                    CartName = "料车名称",
                    Items = new AST_CartTaskItemDto[]
                    {
                        new AST_CartTaskItemDto {
                            CartPosition = 1,
                            WorkStationCode = "SP01",
                            MaterialCode = "Code1",
                            MaterialName = "Name1",
                            MaxQuantityInSingleCartPosition = 31,
                            IsSpecial = true,
                            IsBig = false,
                            AssortingStatus = AssortingStatus.None,
                            PickedQuantity = null
                        },
                        new AST_CartTaskItemDto {
                            CartPosition = 4,
                            WorkStationCode = "SP04",
                            MaterialCode = "Code4",
                            MaterialName = "Name4",
                            MaxQuantityInSingleCartPosition = 34,
                            IsSpecial = true,
                            IsBig = true,
                            AssortingStatus = AssortingStatus.Assorting,
                            PickedQuantity = 24
                        },
                        new AST_CartTaskItemDto {
                            CartPosition = 6,
                            WorkStationCode = "SP06",
                            MaterialCode = "Code6",
                            MaterialName = "Name6",
                            MaxQuantityInSingleCartPosition = 36,
                            IsSpecial = false,
                            IsBig = false,
                            AssortingStatus = AssortingStatus.WaitingConfirm,
                            PickedQuantity = 36
                        },
                        new AST_CartTaskItemDto {
                            CartPosition = 7,
                            WorkStationCode = "SP07",
                            MaterialCode = "Code7",
                            MaterialName = "Name7",
                            MaxQuantityInSingleCartPosition = 37,
                            IsSpecial = false,
                            IsBig = true,
                            AssortingStatus = AssortingStatus.Finished,
                            PickedQuantity = 37
                        },
                    }
                }
            };
            this.ServiceError = true;
        }

        /// <summary>
        /// 移除设计时数据。
        /// </summary>
        public void ClearDesignTimeDatas()
        {
            this.ChannelName = null;
            this.TodayStatistics = null;
            this.TaskInfo = null;
        }

        /// <summary>
        /// 启动定时轮询线程。
        /// </summary>
        public void StartPollingThread()
        {
            this.thread = new Thread(this.ThreadStart);
            this.thread.Name = this.GetType().FullName;
            this.thread.IsBackground = true;
            this.thread.CurrentCulture = CultureInfo.CurrentCulture;
            this.thread.CurrentUICulture = CultureInfo.CurrentUICulture;
            this.thread.Start();
        }

        void ThreadStart()
        {
            while (true)
            {
                try
                {
                    //第一次需要额外获取分拣口名称
                    if (this.channelId == null)
                    {
                        LocalSettings localSettings = new XmlSerializerWrapper<LocalSettings>().Entity;

                        using (ForAssortingKanbanService proxy = new ForAssortingKanbanService())
                        {
                            proxy.Url = this.serviceUrl;

                            CFG_ChannelDto[] cfgChannels = proxy.QueryChannels();
                            CFG_ChannelDto cfgChannel = cfgChannels
                                                            .First(c => c.Code == localSettings.CfgChannelCode);

                            this.channelId = cfgChannel.Id;
                            this.ChannelName = cfgChannel.Name;
                            this.TaskInfo = proxy.QueryCurrentTaskInfo(this.channelId.Value);

                            if (this.TaskInfo.CurrentBatchInfo.PickType == "P") //PTL料架拣料
                            {
                                this.TodayStatistics = proxy.QueryTodayStatistics(this.channelId.Value);
                            }
                            else
                            {
                                this.TodayStatistics = proxy.QueryPDATodayStatistics(this.channelId.Value);
                            }
                        }
                    }

                    using (ForAssortingKanbanService proxy = new ForAssortingKanbanService())
                    {
                        proxy.Url = this.serviceUrl;

                        this.TaskInfo = proxy.QueryCurrentTaskInfo(this.channelId.Value);

                        //每 10 秒一次汇总
                        if (DateTime.Now.Second % 10 == 0)
                        {
                            if (this.TaskInfo.CurrentBatchInfo.PickType == "P") //PTL料架拣料
                            {
                                this.TodayStatistics = proxy.QueryTodayStatistics(this.channelId.Value);
                            }
                            else
                            {
                                this.TodayStatistics = proxy.QueryPDATodayStatistics(this.channelId.Value);
                            }
                        }
                    }

                    this.ServiceError = false;
                }
                catch
                {
                    this.ServiceError = true;
                }
                finally
                {
                    Thread.Sleep(this.pollingPeriod);
                }
            }
        }
    }
}