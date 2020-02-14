using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using DataAccess.AssemblyIndicating;
using DataAccess.Assorting;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using DataAccess.AssortingPDA;
using DataAccess.Other;

namespace DataAccess
{
    /// <summary>
    /// 吉利 PTL 的数据库上下文。
    /// </summary>
    public class GeelyPtlEntities : DbContext
    {
        /// <summary>
        /// 获取或设置架构名，默认 dbo。
        /// </summary>
        public static string SchemaName { get; set; }

        static GeelyPtlEntities()
        {
            GeelyPtlEntities.SchemaName = "dbo";
        }

        /// <summary>
        /// 使用连接字符串名 GeelyPtlEntities 初始化吉利 PTL 的数据库上下文。
        /// </summary>
        public GeelyPtlEntities()
            : base("name=GeelyPtlEntities")
        { }

        /// <summary>
        /// 重置一些约定。
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(GeelyPtlEntities.SchemaName);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<DecimalPropertyConvention>();
            modelBuilder.Conventions.Add(new DecimalPropertyConvention(18, 4));
        }

        /// <summary>
        /// 获取装配指示的集合。
        /// </summary>
        public virtual DbSet<ASM_AssembleIndication> ASM_AssembleIndications { get; set; }

        /// <summary>
        /// 获取装配指示明细的集合。
        /// </summary>
        public virtual DbSet<ASM_AssembleIndicationItem> ASM_AssembleIndicationItems { get; set; }

        /// <summary>
        /// 获取装配指示的通讯消息的集合。
        /// </summary>
        public virtual DbSet<ASM_AssembleIndicationMessage> ASM_AssembleIndicationMessages { get; set; }

        /// <summary>
        /// 获取装配指示取料结果的集合。
        /// </summary>
        public virtual DbSet<ASM_AssembleResult> ASM_AssembleResults { get; set; }

        /// <summary>
        /// 获取装配结果的明细的集合。
        /// </summary>
        public virtual DbSet<ASM_AssembleResultItem> ASM_AssembleResultItems { get; set; }

        /// <summary>
        /// 获取装配指示拣料结果的通讯消息的集合。
        /// </summary>
        public virtual DbSet<ASM_AssembleResultMessage> ASM_AssembleResultMessages { get; set; }

        /// <summary>
        /// 获取装配指引任务的集合。
        /// </summary>
        public virtual DbSet<ASM_Task> ASM_Tasks { get; set; }

        /// <summary>
        /// 获取装配指引任务的明细的集合。
        /// </summary>
        public virtual DbSet<ASM_TaskItem> ASM_TaskItems { get; set; }

        /// <summary>
        /// 获取单车拣选结果的集合。
        /// </summary>
        public virtual DbSet<AST_CartResult> AST_CartResults { get; set; }

        /// <summary>
        /// 获取单车拣选结果的明细的集合。
        /// </summary>
        public virtual DbSet<AST_CartResultItem> AST_CartResultItems { get; set; }

        /// <summary>
        /// 获取单车拣选结果的通讯消息的集合。
        /// </summary>
        public virtual DbSet<AST_CartResultMessage> AST_CartResultMessages { get; set; }

        /// <summary>
        /// 获取按车的播种任务的集合。
        /// </summary>
        public virtual DbSet<AST_CartTask> AST_CartTasks { get; set; }

        /// <summary>
        /// 获取按车的播种任务的明细的集合。
        /// </summary>
        public virtual DbSet<AST_CartTaskItem> AST_CartTaskItems { get; set; }

        /// <summary>
        /// 获取 LES 原始分拣任务的集合。
        /// </summary>
        public virtual DbSet<AST_LesTask> AST_LesTasks { get; set; }

        /// <summary>
        /// 获取 LES 原始分拣任务的明细的集合。
        /// </summary>
        public virtual DbSet<AST_LesTaskItem> AST_LesTaskItems { get; set; }

        /// <summary>
        /// 获取 LES 原始分拣任务的通讯消息的集合。
        /// </summary>
        public virtual DbSet<AST_LesTaskMessage> AST_LesTaskMessages { get; set; }

        /// <summary>
        /// 获取托盘抵达分拣口的通知的集合。
        /// </summary>
        public virtual DbSet<AST_PalletArrived> AST_PalletArriveds { get; set; }

        /// <summary>
        /// 获取托盘抵达分拣口通知的通讯消息的集合。
        /// </summary>
        public virtual DbSet<AST_PalletArrivedMessage> AST_PalletArrivedMessages { get; set; }

        /// <summary>
        /// 获取单托拣选结果的集合。
        /// </summary>
        public virtual DbSet<AST_PalletResult> AST_PalletResults { get; set; }

        /// <summary>
        /// 获取单托拣选结果的明细的集合。
        /// </summary>
        public virtual DbSet<AST_PalletResultItem> AST_PalletResultItems { get; set; }

        /// <summary>
        /// 获取单托拣选结果的通讯消息的集合。
        /// </summary>
        public virtual DbSet<AST_PalletResultMessage> AST_PalletResultMessages { get; set; }

        /// <summary>
        /// 获取按托合并后的分拣任务的集合。
        /// </summary>
        public virtual DbSet<AST_PalletTask> AST_PalletTasks { get; set; }

        /// <summary>
        /// 获取按托合并后的分拣任务的明细的集合。
        /// </summary>
        public virtual DbSet<AST_PalletTaskItem> AST_PalletTaskItems { get; set; }

        /// <summary>
        /// 获取单车发车结果的集合。
        /// </summary>
        public virtual DbSet<FND_DeliveryResult> FND_DeliveryResults { get; set; }

        /// <summary>
        /// 获取单车发车结果的明细的集合。
        /// </summary>
        public virtual DbSet<FND_DeliveryResultItem> FND_DeliveryResultItems { get; set; }

        /// <summary>
        /// 获取发车结果的通讯消息的集合。
        /// </summary>
        public virtual DbSet<FND_DeliveryResultMessage> FND_DeliveryResultMessages { get; set; }

        /// <summary>
        /// 获取单车寻车任务的集合。
        /// </summary>
        public virtual DbSet<FND_Task> FND_Tasks { get; set; }

        /// <summary>
        /// 获取 PTL 小车的集合。
        /// </summary>
        public virtual DbSet<CFG_Cart> CFG_Carts { get; set; }

        /// <summary>
        /// 获取小车上的当前物料的集合。
        /// </summary>
        public virtual DbSet<CFG_CartCurrentMaterial> CFG_CartCurrentMaterials { get; set; }

        /// <summary>
        /// 获取小车上设备的状态的集合。
        /// </summary>
        public virtual DbSet<CFG_CartPtlDevice> CFG_CartPtlDevices { get; set; }

        /// <summary>
        /// 获取分拣巷道的集合。
        /// </summary>
        public virtual DbSet<CFG_Channel> CFG_Channels { get; set; }

        /// <summary>
        /// 获取分拣巷道的当前小车的集合。
        /// </summary>
        public virtual DbSet<CFG_ChannelCurrentCart> CFG_ChannelCurrentCarts { get; set; }

        /// <summary>
        /// 获取分拣巷道的当前托盘的集合。
        /// </summary>
        public virtual DbSet<CFG_ChannelCurrentPallet> CFG_ChannelCurrentPallets { get; set; }

        /// <summary>
        /// 获取分拣转台上的 PTL 设备的集合。
        /// </summary>
        public virtual DbSet<CFG_ChannelPtlDevice> CFG_ChannelPtlDevices { get; set; }

        /// <summary>
        /// 获取操作员的集合。
        /// </summary>
        public virtual DbSet<CFG_Employee> CFG_Employees { get; set; }

        /// <summary>
        /// 获取托盘的集合。
        /// </summary>
        public virtual DbSet<CFG_Pallet> CFG_Pallets { get; set; }

        /// <summary>
        /// 获取装配工位的集合。
        /// </summary>
        public virtual DbSet<CFG_WorkStation> CFG_WorkStations { get; set; }

        /// <summary>
        /// 获取装配工位上车位的集合。
        /// </summary>
        public virtual DbSet<CFG_WorkStationCurrentCart> CFG_WorkStationCurrentCarts { get; set; }

        /// <summary>
        /// 获取配送任务的集合
        /// </summary>
        public virtual DbSet<DST_DistributeTask> DST_DistributeTasks { get; set; }

        /// <summary>
        /// 获取配送任务结果的集合
        /// </summary>
        public virtual DbSet<DST_DistributeTaskResult> DST_DistributeTaskResults { get; set; }

        /// <summary>
        /// 获取配送到达任务的集合
        /// </summary>
        public virtual DbSet<DST_DistributeArriveTask> DST_DistributeArriveTasks { get; set; }

        /// <summary>
        /// 获取配送到达任务结果的集合
        /// </summary>
        public virtual DbSet<DST_DistributeArriveTaskResult> DST_DistributeArriveTaskResults { get; set; }

        /// <summary>
        /// 获取配送到达任务结果明细的集合
        /// </summary>
        public virtual DbSet<DST_DistributeArriveResult> DST_DistributeArriveResults { get; set; }

        /// <summary>
        /// 获取AGV开关信息的集合
        /// </summary>
        public virtual DbSet<DST_AgvSwitch> DST_AgvSwitchs { get; set; }

        /// <summary>
        /// 获取 LES 原始分拣任务的集合。
        /// </summary>
        public virtual DbSet<AST_LesTask_PDA> AST_LesTask_PDAs { get; set; }

        /// <summary>
        /// 获取 LES 原始分拣任务的明细的集合。
        /// </summary>
        public virtual DbSet<AST_LesTaskItem_PDA> AST_LesTaskItem_PDAs { get; set; }

        /// <summary>
        /// 获取 LES 原始分拣任务的通讯消息的集合。
        /// </summary>
        public virtual DbSet<AST_LesTaskMessage_PDA> AST_LesTaskMessage_PDAs { get; set; }

        /// <summary>
        /// 获取托盘抵达分拣口的通知的集合。
        /// </summary>
        public virtual DbSet<AST_PalletArrived_PDA> AST_PalletArrived_PDAs { get; set; }

        /// <summary>
        /// 获取托盘抵达分拣口通知的通讯消息的集合。
        /// </summary>
        public virtual DbSet<AST_PalletArrivedMessage_PDA> AST_PalletArrivedMessage_PDAs { get; set; }

        /// <summary>
        /// 获取按托合并后的分拣任务的集合。
        /// </summary>
        public virtual DbSet<AST_PalletTask_PDA> AST_PalletTask_PDAs { get; set; }

        /// <summary>
        /// 获取按托合并后的分拣任务的明细的集合。
        /// </summary>
        public virtual DbSet<AST_PalletTaskItem_PDA> AST_PalletTaskItem_PDAs { get; set; }

        /// <summary>
        /// 获取物料超市工位所停靠的小车的集合。
        /// </summary>
        public virtual DbSet<CFG_MarketWorkStationCurrentCart> CFG_MarketWorkStationCurrentCarts { get; set; }
        /// <summary>
        /// 获取托盘分拣结果的通知的集合。
        /// </summary>
        public virtual DbSet<AST_PalletPickResult_PDA> AST_PalletPickResult_PDAs { get; set; }

        /// <summary>
        /// 获取托盘分拣结果通知的通讯消息的集合。
        /// </summary>
        public virtual DbSet<AST_PalletPickResultMessage_PDA> AST_PalletPickResultMessage_PDAs { get; set; }
        /// <summary>
        /// 拣选通道的柱灯配置集合
        /// </summary>
        public virtual DbSet<PickZone> PickZones { get; set; }

        /// <summary>
        /// 投料区,即分装总成缓存区集合
        /// </summary>
        public virtual DbSet<FeedZone> FeedZones { get; set; }

        public virtual DbSet<FeedRecord> FeedRecords { get; set; }

        public virtual DbSet<MarketZone> MarketZones { get; set; }

        /// <summary>
        /// 缓存区
        /// </summary>
        public virtual DbSet<CacheRegion> CacheRegions { get; set; }

        public virtual DbSet<CacheRegionLightOrder> CacheRegionLightOrders { get; set; }
        /// <summary>
        /// 装配区
        /// </summary>
        public virtual DbSet<Assembling> Assemblings { get; set; }

        public virtual DbSet<AssemblyLightOrder> AssemblyLightOrders { get; set; }
    }
}