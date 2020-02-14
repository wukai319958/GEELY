using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using DeviceCommunicationHost;
using Distributing;
using Ptl.Device;
using Ptl.Device.Communication.Command;

namespace Assorting
{
    /// <summary>
    /// 为各自分拣口执行分拣任务，加载按托任务，播种后生成按车任务及明细；一辆小车只可以摆放同一批次同一目标工位的物料，同时只作业一辆车。
    /// </summary>
    public class AssortingExecutor
    {
        /// <summary>
        /// 获取服务于巷道的主键。
        /// </summary>
        public int CFG_ChannelId { get; private set; }

        /// <summary>
        /// 获取当前正在进行的分拣任务的主键。
        /// </summary>
        public long? CurrentAstPalletTaskId { get; private set; }

        /// <summary>
        /// 获取当前正在进行的分拣明细的主键。
        /// </summary>
        public long? CurrentAstPalletTaskItemId { get; private set; }

        /// <summary>
        /// 获取当前正在进行作业的料车的主键。
        /// </summary>
        public int? CurrentCfgCartId { get; private set; }

        /// <summary>
        /// 获取当前正在进行的播种任务的主键。
        /// </summary>
        public long? CurrentAstCartTaskId { get; private set; }

        /// <summary>
        /// 获取当前正在进行的播种明细的主键。
        /// </summary>
        public long? CurrentAstCartTaskItemId { get; private set; }

        readonly TimeSpan rotationTimeSpan = TimeSpan.FromSeconds(35);
        readonly Thread thread;

        /// <summary>
        /// 一段时间不旋转，则在此超时时间后自动旋转。
        /// </summary>
        readonly TimeSpan autoRotationTimeout = TimeSpan.FromSeconds(3);
        /// <summary>
        /// 用于判断自动旋转的等待起始时间。
        /// </summary>
        DateTime? needAutoRotationBeginTime;

        /// <summary>
        /// 使用巷道主键初始化执行器。
        /// </summary>
        /// <param name="cfgChannelId">服务于巷道的主键。</param>
        public AssortingExecutor(int cfgChannelId)
        {
            this.CFG_ChannelId = cfgChannelId;

            this.Restore();

            this.thread = new Thread(this.threadStart);
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Channel cfgChannel = dbContext.CFG_Channels
                                             .First(c => c.Id == this.CFG_ChannelId);
                this.thread.Name = string.Format(CultureInfo.InvariantCulture, "{0}({1})", this.GetType().FullName, cfgChannel.Name);
            }
            this.thread.IsBackground = true;
            this.thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            this.thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

            this.thread.Start();
        }

        /// <summary>
        /// 旋转当前停靠的托盘。
        /// </summary>
        void StartRotationPallet()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.CurrentThread.Name = "AssortingExecutor.rotationOpc_RotationStart_" + this.CFG_ChannelId;

                int? currentPalletId = null;

                //启动旋转
                while (true)
                {
                    try
                    {
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            //注意 PickStatus 从 New 到 Picking 的并发
                            CFG_Pallet cfgPallet = dbContext.AST_PalletTasks
                                                       .Where(pt => pt.CFG_ChannelId == this.CFG_ChannelId && pt.PickStatus == PickStatus.Picking)
                                                       .Select(pt => pt.CFG_Pallet)
                                                       .FirstOrDefault();
                            if (cfgPallet == null && this.CurrentAstPalletTaskId != null)
                            {
                                cfgPallet = dbContext.AST_PalletTasks
                                                .Where(pt => pt.Id == this.CurrentAstPalletTaskId)
                                                .Select(pt => pt.CFG_Pallet)
                                                .FirstOrDefault();
                            }
                            if (cfgPallet == null)
                            {
                                cfgPallet = dbContext.AST_PalletTasks
                                                .Where(pt => pt.CFG_ChannelId == this.CFG_ChannelId && pt.PickStatus == PickStatus.New)
                                                .Select(pt => pt.CFG_Pallet)
                                                .FirstOrDefault();
                            }

                            if (cfgPallet == null || cfgPallet.PalletRotationStatus != PalletRotationStatus.Normal)
                                return;

                            cfgPallet.PalletRotationStatus = PalletRotationStatus.BeginRotation;

                            dbContext.SaveChanges();

                            currentPalletId = cfgPallet.Id;

                            Logger.Log(this.GetType().Name + "." + this.CFG_ChannelId, DateTime.Now.ToString("HH:mm:ss") + " 开始旋转托盘：" + cfgPallet.Code + Environment.NewLine);
                        }

                        break;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                    }
                }

                //旋转中
                Thread.Sleep(this.rotationTimeSpan);

                //旋转完成
                while (true)
                {
                    try
                    {
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            if (currentPalletId != null)
                            {
                                CFG_Pallet cfgPallet = dbContext.CFG_Pallets
                                                           .First(p => p.Id == currentPalletId.Value);

                                if (cfgPallet.PalletRotationStatus == PalletRotationStatus.BeginRotation)
                                {
                                    cfgPallet.PalletRotationStatus = PalletRotationStatus.Reversed;

                                    dbContext.SaveChanges();

                                    Logger.Log(this.GetType().Name + "." + this.CFG_ChannelId, DateTime.Now.ToString("HH:mm:ss") + " 旋转托盘成功：" + cfgPallet.Code + Environment.NewLine);
                                }
                            }
                        }

                        break;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                    }
                }
            });
        }

        /// <summary>
        /// 还原之前的停靠状态和未完成的任务。
        /// </summary>
        void Restore()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                //旋转成目标位
                List<CFG_Pallet> rotatingPallets = dbContext.CFG_Pallets
                                                       .Where(p => p.PalletRotationStatus == PalletRotationStatus.BeginRotation
                                                                   || p.PalletRotationStatus == PalletRotationStatus.BeginReverseRotation)
                                                       .ToList();
                foreach (CFG_Pallet rotatingPallet in rotatingPallets)
                {
                    if (rotatingPallet.PalletRotationStatus == PalletRotationStatus.BeginRotation)
                        rotatingPallet.PalletRotationStatus = PalletRotationStatus.Reversed;
                    else if (rotatingPallet.PalletRotationStatus == PalletRotationStatus.BeginReverseRotation)
                        rotatingPallet.PalletRotationStatus = PalletRotationStatus.Normal;
                }

                dbContext.SaveChanges();

                //停靠的料车：刚停靠、部分作业但已完成、全部作业完成
                List<CFG_ChannelCurrentCart> cfgChannelCurrentCarts = dbContext.CFG_ChannelCurrentCarts
                                                                          .Where(ccc => ccc.CFG_ChannelId == this.CFG_ChannelId && ccc.CFG_CartId != null)
                                                                          .ToList();
                foreach (CFG_ChannelCurrentCart cfgChannelCurrentCart in cfgChannelCurrentCarts)
                {
                    CFG_Channel cfgChannel = cfgChannelCurrentCart.CFG_Channel;
                    CFG_Cart cfgCart = cfgChannelCurrentCart.CFG_Cart;
                    List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                                .OrderBy(ccm => ccm.Position)
                                                                                .ToList();
                    CFG_CartCurrentMaterial firstNotEmptyCfgCartCurrentMaterial = cfgCartCurrentMaterials
                                                                                      .FirstOrDefault(ccm => ccm.AST_CartTaskItemId != null);
                    CFG_WorkStation cfgWorkStation = null;
                    AST_CartTask astCartTask = null;
                    if (firstNotEmptyCfgCartCurrentMaterial != null)
                    {
                        cfgWorkStation = firstNotEmptyCfgCartCurrentMaterial.CFG_WorkStation;
                        astCartTask = firstNotEmptyCfgCartCurrentMaterial.AST_CartTaskItem.AST_CartTask;
                    }

                    CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(cfgCart.Id);
                    Ptl900U cartPtl900UPublisher = cartPtl.GetPtl900UPublisher();
                    Ptl900U cartPtl900ULight = cartPtl.GetPtl900ULight();

                    Display900UItem cartPublisherDisplay900UItem = new Display900UItem();
                    if (cfgCart.CartStatus == CartStatus.WaitingAssorting)
                    {
                        cartPublisherDisplay900UItem.Name = "停靠成功";
                        cartPublisherDisplay900UItem.Description = cfgChannel.Name;
                        cartPublisherDisplay900UItem.Count = (ushort)cfgChannelCurrentCart.Position;
                        cartPublisherDisplay900UItem.Unit = "位";
                    }
                    else if (cfgCartCurrentMaterials.All(ccm => ccm.AST_CartTaskItem == null
                                                                || ccm.AST_CartTaskItem.AssortingStatus == AssortingStatus.Finished))
                    {
                        if (astCartTask != null)
                        {
                            cartPublisherDisplay900UItem.Name = "批次：" + astCartTask.BatchCode;
                            cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
工位：{2}", astCartTask.ProjectCode, astCartTask.ProjectStep, cfgWorkStation.Code);
                        }
                        cartPublisherDisplay900UItem.Count = (ushort)cfgCartCurrentMaterials
                                                                         .Where(ccm => ccm.Quantity != null)
                                                                         .Select(ccm => ccm.Quantity.Value)
                                                                         .Sum();
                        cartPublisherDisplay900UItem.Unit = "个";
                    }
                    else if (cfgCart.CartStatus == CartStatus.Assorted)
                    {
                        cartPublisherDisplay900UItem.Name = "已分拣完成";
                        if (astCartTask != null)
                        {
                            cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
工位：{2}", astCartTask.ProjectCode, astCartTask.ProjectStep, cfgWorkStation.Code);
                        }
                        cartPublisherDisplay900UItem.Count = (ushort)cfgCartCurrentMaterials
                                                                         .Where(ccm => ccm.Quantity != null)
                                                                         .Select(ccm => ccm.Quantity.Value)
                                                                         .Sum();
                        cartPublisherDisplay900UItem.Unit = "个";

                        cartPtl900ULight.Clear();
                        cartPtl900ULight.Display(new Display900UItem(), LightColor.Cyan);
                    }

                    cartPtl900UPublisher.Clear(true);
                    cartPtl900UPublisher.Lock();
                    cartPtl900UPublisher.Display(cartPublisherDisplay900UItem, LightColor.Off);
                }

                //有作业中任务的料车
                AST_CartTaskItem astCartTaskItem = dbContext.AST_CartTaskItems
                                                       .FirstOrDefault(cti => cti.AST_CartTask.CFG_ChannelId == this.CFG_ChannelId
                                                                              && (cti.AssortingStatus == AssortingStatus.Assorting || cti.AssortingStatus == AssortingStatus.WaitingConfirm));
                if (astCartTaskItem != null)
                {
                    //先准备好数据
                    AST_CartTask astCartTask = astCartTaskItem.AST_CartTask;
                    AST_PalletTaskItem astPalletTaskItem = astCartTaskItem.AST_PalletTaskItem;
                    AST_PalletTask astPalletTask = astPalletTaskItem.AST_PalletTask;
                    CFG_WorkStation cfgWorkStation = astPalletTaskItem.CFG_WorkStation;
                    CFG_Cart cfgCart = astCartTask.CFG_Cart;
                    List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                                .OrderBy(ccm => ccm.Position)
                                                                                .ToList();
                    List<AST_CartTaskItem> astCartTaskItems = new List<AST_CartTaskItem>();
                    foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                        astCartTaskItems.Add(cfgCartCurrentMaterial.AST_CartTaskItem);

                    //再控制设备
                    //点亮托盘库位指引
                    ChannelPtl channelPtl = ChannelPtlHost.Instance.GetChannelPtl(astPalletTask.CFG_ChannelId);
                    int fromPalletPosition = astPalletTaskItem.FromPalletPosition;
                    if (fromPalletPosition > 5)
                        fromPalletPosition = 3;
                    Ptl900U channelPtl900U = channelPtl.GetPtl900UByPosition(fromPalletPosition);

                    Display900UItem channelDisplay900UItem = new Display900UItem();
                    channelDisplay900UItem.Count = (ushort)astPalletTaskItem.ToPickQuantity;

                    LightMode channelLightMode = new LightMode();
                    channelLightMode.Color = LightColor.Green;
                    if (astPalletTaskItem.IsBig)
                    {
                        channelLightMode.Color = LightColor.Magenta;
                    }
                    if (astPalletTaskItem.IsSpecial)
                    {
                        channelLightMode.Color = LightColor.Magenta;
                        channelLightMode.Ratio = LightOnOffRatio.RatioP1V1;
                        channelLightMode.Period = LightOnOffPeriod.Period500;
                    }

                    channelPtl900U.Display(channelDisplay900UItem, channelLightMode, false);

                    //点亮小车
                    CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(cfgCart.Id);
                    Ptl900U cartPtl900UPublisher = cartPtl.GetPtl900UPublisher();
                    Ptl900U cartPtl900ULight = cartPtl.GetPtl900ULight();

                    foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                    {
                        Ptl900U cartPtl900U = cartPtl.GetPtl900UByPosition(cfgCartCurrentMaterial.Position);

                        Display900UItem cartDisplay900UItem = new Display900UItem();
                        cartDisplay900UItem.Count = 0;
                        if (cfgCartCurrentMaterial.Quantity != null)
                            cartDisplay900UItem.Count = (ushort)cfgCartCurrentMaterial.Quantity.Value;

                        //多种状态：播种中、等待确认、已满不显示
                        LightColor cartLightColor = LightColor.Off;

                        if (cfgCartCurrentMaterial.AST_CartTaskItemId != null
                            && cfgCartCurrentMaterial.AST_CartTaskItem.AssortingStatus == AssortingStatus.WaitingConfirm)
                        {
                            //当前库位等待确认
                            cartLightColor = LightColor.Yellow;

                            cartPtl900U.Lock();
                            cartPtl900U.Display(cartDisplay900UItem, cartLightColor);
                        }
                        else if (cfgCartCurrentMaterial.AST_CartTaskItemId != null
                                 && cfgCartCurrentMaterial.AST_CartTaskItem.AssortingStatus == AssortingStatus.Assorting)
                        {
                            //当前库位播种中
                            cartLightColor = LightColor.Green;

                            cartPtl900U.Pressed += this.cartPtl900U_Pressed;
                            cartPtl900U.Display(cartDisplay900UItem, cartLightColor, true);
                        }
                    }

                    //刷新信息屏和灯塔
                    Display900UItem cartPublisherDisplay900UItem = new Display900UItem();
                    cartPublisherDisplay900UItem.Name = astPalletTaskItem.MaterialName;
                    cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
工位：{2}", astPalletTask.ProjectCode, astPalletTask.ProjectStep, cfgWorkStation.Code);
                    cartPublisherDisplay900UItem.LongSubLocation = astPalletTaskItem.ToPickQuantity.ToString(CultureInfo.InvariantCulture);
                    cartPublisherDisplay900UItem.Count = 0;
                    if (astPalletTaskItem.PickedQuantity != null)
                        cartPublisherDisplay900UItem.Count = (ushort)astPalletTaskItem.PickedQuantity.Value;

                    LightColor cartPublisherLightColor = LightColor.Off;
                    LightColor cartLighthouseLightColor = LightColor.Off;

                    //整车已满 = 所有库位完成 或者 该批次该工位没有未完成的任务
                    bool wholeCartIsFull = cfgCartCurrentMaterials.All(ccm => ccm.AST_CartTaskItemId != null
                                                                              && ccm.AST_CartTaskItem.AssortingStatus == AssortingStatus.Finished)
                                           || dbContext.AST_LesTaskItems.Count(lti => lti.AST_LesTask.BatchCode == astCartTask.BatchCode
                                                                                      && lti.AST_LesTask.CFG_ChannelId == astCartTask.CFG_ChannelId
                                                                                      && lti.AST_LesTask.CFG_WorkStationId == astCartTask.CFG_WorkStationId
                                                                                      && (lti.AST_PalletTaskItem == null
                                                                                          || (lti.AST_PalletTaskItem.PickStatus != PickStatus.Finished
                                                                                              && lti.AST_PalletTaskItemId != astPalletTaskItem.Id))) == 0;

                    bool waitingConfirm = cfgCartCurrentMaterials.Any(ccm => ccm.AST_CartTaskItemId != null && ccm.AST_CartTaskItem.AssortingStatus == AssortingStatus.WaitingConfirm);

                    if (wholeCartIsFull)
                    {
                        cartPublisherDisplay900UItem.Name = "已分拣完成";
                        cartPublisherDisplay900UItem.LongSubLocation = string.Empty;
                        cartPublisherDisplay900UItem.Count = (ushort)cfgCartCurrentMaterials
                                                                         .Where(ccm => ccm.CFG_CartId == cfgCart.Id && ccm.Quantity != null)
                                                                         .Select(ccm => ccm.Quantity.Value)
                                                                         .Sum();

                        cartPublisherLightColor = LightColor.Cyan;
                        cartLighthouseLightColor = LightColor.Cyan;
                    }
                    else if (waitingConfirm)
                    {
                        cartPublisherLightColor = LightColor.Green;
                    }

                    if (wholeCartIsFull)
                    {
                        cartPtl900UPublisher.Clear(true);
                        cartPtl900UPublisher.Lock();
                        cartPtl900UPublisher.Display(cartPublisherDisplay900UItem, cartPublisherLightColor);
                    }
                    else
                    {
                        cartPtl900UPublisher.Clear(true);
                        cartPtl900UPublisher.Unlock();
                        cartPtl900UPublisher.Pressed += this.cartPtl900UPublisher_Pressed;
                        cartPtl900UPublisher.Display(cartPublisherDisplay900UItem, cartPublisherLightColor, true);
                    }

                    cartPtl900ULight.Display(new Display900UItem(), cartLighthouseLightColor);

                    //标记任务已启动
                    this.CurrentAstCartTaskItemId = astCartTaskItem.Id;
                    this.CurrentAstCartTaskId = astCartTaskItem.AST_CartTaskId;
                    this.CurrentCfgCartId = cfgCart.Id;
                    this.CurrentAstPalletTaskItemId = astPalletTaskItem.Id;
                    this.CurrentAstPalletTaskId = astPalletTask.Id;
                }
            }

            Logger.Log(this.GetType().Name + "." + this.CFG_ChannelId, DateTime.Now.ToString("HH:mm:ss") + " Restore() 完成" + Environment.NewLine);
        }

        /// <summary>
        /// 启动指定的分拣任务。
        /// </summary>
        /// <param name="astPalletTaskId">分拣任务的主键。</param>
        public void Start(long astPalletTaskId)
        {
            if (this.CurrentAstPalletTaskId != null)
                throw new InvalidOperationException();

            this.CurrentAstPalletTaskId = astPalletTaskId;
        }

        void threadStart(object notUsed)
        {
            while (true)
            {
                //加载线程推动新任务，按钮交互推进明细的进度
                if (this.CurrentAstPalletTaskId != null && this.CurrentAstPalletTaskItemId == null)
                {
                    try
                    {
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            AST_PalletTask astPalletTask = dbContext.AST_PalletTasks
                                                               .First(t => t.Id == this.CurrentAstPalletTaskId.Value);

                            //因为 AssortingExecutorLoader 加载和 AssortingExecutor 是不同线程，所以再次核对状态
                            if (astPalletTask.PickStatus == PickStatus.Finished)
                            {
                                this.CurrentAstPalletTaskId = null;

                                continue;
                            }

                            List<AST_PalletTaskItem> astPalletTaskItems = astPalletTask.AST_PalletTaskItems
                                                                              .ToList();
                            astPalletTaskItems.Sort(new PalletPositionPickSortComparer());
                            AST_PalletTaskItem currentAstPalletTaskItem = astPalletTaskItems
                                                                              .FirstOrDefault(ti => ti.PickStatus != PickStatus.Finished);
                            if (currentAstPalletTaskItem != null)
                            {
                                CFG_Pallet cfgPallet = astPalletTask.CFG_Pallet;

                                // 五料盒的托盘，1、2 库位在旋转后开始分拣
                                if (cfgPallet.PalletType == "01"
                                    && (currentAstPalletTaskItem.FromPalletPosition == 1
                                        || currentAstPalletTaskItem.FromPalletPosition == 2))
                                {
                                    //如果长时间不按转台或转台故障，则自动旋转
                                    if (this.needAutoRotationBeginTime == null)
                                        this.needAutoRotationBeginTime = DateTime.Now;

                                    if ((DateTime.Now - this.needAutoRotationBeginTime.Value > this.autoRotationTimeout)
                                        && cfgPallet.PalletRotationStatus == PalletRotationStatus.Normal)
                                    {
                                        this.StartRotationPallet();
                                    }

                                    if (cfgPallet.PalletRotationStatus != PalletRotationStatus.Reversed)
                                        continue;

                                    this.needAutoRotationBeginTime = null;
                                }

                                CFG_WorkStation cfgWorkStation = currentAstPalletTaskItem.CFG_WorkStation;
                                List<CFG_Cart> dockedCfgCarts = dbContext.CFG_ChannelCurrentCarts
                                                                    .Where(ccc => ccc.CFG_ChannelId == astPalletTask.CFG_ChannelId && ccc.CFG_CartId != null)
                                                                    .OrderBy(ccc => ccc.DockedTime)
                                                                    .ThenBy(ccc => ccc.Position)
                                                                    .Select(ccc => ccc.CFG_Cart)
                                                                    .ToList();
                                List<CFG_Cart> usableCfgCarts = dockedCfgCarts
                                                                    .Where(c => c.CFG_CartCurrentMaterials.Any(ccm => ccm.Quantity == null)
                                                                                && c.CFG_CartCurrentMaterials.All(ccm => ccm.Quantity == null
                                                                                                                         || (ccm.BatchCode == astPalletTask.BatchCode
                                                                                                                             && ccm.CFG_WorkStationId == cfgWorkStation.Id)))
                                                                    .ToList();

                                //播种中的排在最前
                                for (int i = 0; i < usableCfgCarts.Count; i++)
                                {
                                    CFG_Cart usableCfgCart = usableCfgCarts[i];
                                    if (usableCfgCart.CartStatus == CartStatus.Assorting)
                                    {
                                        usableCfgCarts.RemoveAt(i);
                                        usableCfgCarts.Insert(0, usableCfgCart);

                                        break;
                                    }
                                }

                                //整层大件需要同层的两个空库位
                                if (currentAstPalletTaskItem.IsBig)
                                {
                                    List<CFG_Cart> bigMaterialUsableCfgCarts = new List<CFG_Cart>();

                                    foreach (CFG_Cart temporaryCfgCart in usableCfgCarts)
                                    {
                                        List<CFG_CartCurrentMaterial> temporaryCfgCartCurrentMaterials = temporaryCfgCart.CFG_CartCurrentMaterials
                                                                                                             .OrderBy(ccm => ccm.Position)
                                                                                                             .ToList();
                                        if ((temporaryCfgCartCurrentMaterials[0].Usability == CartPositionUsability.Enable && temporaryCfgCartCurrentMaterials[1].Usability == CartPositionUsability.Enable)
                                            || (temporaryCfgCartCurrentMaterials[2].Usability == CartPositionUsability.Enable && temporaryCfgCartCurrentMaterials[3].Usability == CartPositionUsability.Enable)
                                            || (temporaryCfgCartCurrentMaterials[4].Usability == CartPositionUsability.Enable && temporaryCfgCartCurrentMaterials[5].Usability == CartPositionUsability.Enable)
                                            || (temporaryCfgCartCurrentMaterials[6].Usability == CartPositionUsability.Enable && temporaryCfgCartCurrentMaterials[7].Usability == CartPositionUsability.Enable))
                                        {
                                            bigMaterialUsableCfgCarts.Add(temporaryCfgCart);
                                        }
                                    }

                                    for (int i = usableCfgCarts.Count - 1; i >= 0; i--)
                                    {
                                        if (!bigMaterialUsableCfgCarts.Contains(usableCfgCarts[i]))
                                            usableCfgCarts.RemoveAt(i);
                                    }
                                }

                                //Empty Cart Must Online
                                for (int i = 0; i < usableCfgCarts.Count; i++)
                                {
                                    CFG_Cart usableCfgCart = usableCfgCarts[i];
                                    if (usableCfgCart.CartStatus == CartStatus.WaitingAssorting && !usableCfgCart.OnLine)
                                        usableCfgCarts.RemoveAt(i);
                                }

                                //没有空闲且符合当前批次当前工位的小车
                                if (usableCfgCarts.Count == 0)
                                    continue;

                                //同时只作业一辆车
                                CFG_Cart cfgCart = usableCfgCarts.First();

                                //提前加载小车的物料，以便控制设备时不用访问数据库
                                List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                                            .OrderBy(ccm => ccm.Position)
                                                                                            .ToList();

                                List<AST_CartTask> astCartTasks = new List<AST_CartTask>();
                                List<AST_CartTaskItem> astCartTaskItems = new List<AST_CartTaskItem>();
                                foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                                {
                                    if (cfgCartCurrentMaterial.AST_CartTaskItemId != null)
                                    {
                                        AST_CartTaskItem astCartTaskItem = cfgCartCurrentMaterial.AST_CartTaskItem;

                                        astCartTaskItems.Add(astCartTaskItem);

                                        if (astCartTasks.All(ct => ct.Id != astCartTaskItem.AST_CartTaskId))
                                            astCartTasks.Add(astCartTaskItem.AST_CartTask);
                                    }
                                }

                                //先更新数据库
                                astPalletTask.PickStatus = PickStatus.Picking;
                                currentAstPalletTaskItem.PickStatus = PickStatus.Picking;

                                dbContext.SaveChanges();

                                //再控制设备
                                ChannelPtl channelPtl = ChannelPtlHost.Instance.GetChannelPtl(astPalletTask.CFG_ChannelId);
                                int fromPalletPosition = currentAstPalletTaskItem.FromPalletPosition;
                                if (fromPalletPosition > 5)
                                    fromPalletPosition = 3;
                                Ptl900U channelPtl900U = channelPtl.GetPtl900UByPosition(fromPalletPosition);

                                Display900UItem channelDisplay900UItem = new Display900UItem();
                                channelDisplay900UItem.Count = (ushort)currentAstPalletTaskItem.ToPickQuantity;

                                LightMode channelLightMode = new LightMode();
                                channelLightMode.Color = LightColor.Green;
                                if (currentAstPalletTaskItem.IsBig)
                                {
                                    channelLightMode.Color = LightColor.Magenta;
                                }
                                if (currentAstPalletTaskItem.IsSpecial)
                                {
                                    channelLightMode.Color = LightColor.Magenta;
                                    channelLightMode.Ratio = LightOnOffRatio.RatioP1V1;
                                    channelLightMode.Period = LightOnOffPeriod.Period500;
                                }

                                channelPtl900U.Display(channelDisplay900UItem, channelLightMode, false);

                                CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(cfgCart.Id);
                                Ptl900U cartPtl900UPublisher = cartPtl.GetPtl900UPublisher();
                                Ptl900U cartPtl900ULight = cartPtl.GetPtl900ULight();

                                //刷新小车的空闲库位
                                foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                                {
                                    if (cfgCartCurrentMaterial.AST_CartTaskItemId == null)
                                    {
                                        //整层大件需要同层的两个空库位
                                        if (currentAstPalletTaskItem.IsBig)
                                        {
                                            if ((cfgCartCurrentMaterial.Position == 1 && cfgCartCurrentMaterials[1].Usability != CartPositionUsability.Enable)
                                                || (cfgCartCurrentMaterial.Position == 2 && cfgCartCurrentMaterials[0].Usability != CartPositionUsability.Enable)
                                                || (cfgCartCurrentMaterial.Position == 3 && cfgCartCurrentMaterials[3].Usability != CartPositionUsability.Enable)
                                                || (cfgCartCurrentMaterial.Position == 4 && cfgCartCurrentMaterials[2].Usability != CartPositionUsability.Enable)
                                                || (cfgCartCurrentMaterial.Position == 5 && cfgCartCurrentMaterials[5].Usability != CartPositionUsability.Enable)
                                                || (cfgCartCurrentMaterial.Position == 6 && cfgCartCurrentMaterials[4].Usability != CartPositionUsability.Enable)
                                                || (cfgCartCurrentMaterial.Position == 7 && cfgCartCurrentMaterials[7].Usability != CartPositionUsability.Enable)
                                                || (cfgCartCurrentMaterial.Position == 8 && cfgCartCurrentMaterials[6].Usability != CartPositionUsability.Enable))
                                            {
                                                continue;
                                            }
                                        }

                                        Ptl900U cartPtl900U = cartPtl.GetPtl900UByPosition(cfgCartCurrentMaterial.Position);

                                        Display900UItem cartDisplay900UItem = new Display900UItem();
                                        cartDisplay900UItem.Count = 0;

                                        LightColor cartLightColor = LightColor.Green;

                                        cartPtl900U.Pressed -= this.cartPtl900U_Pressed;
                                        cartPtl900U.Pressed += this.cartPtl900U_Pressed;
                                        cartPtl900U.Clear(true);
                                        cartPtl900U.Unlock();
                                        cartPtl900U.Display(cartDisplay900UItem, cartLightColor, true);
                                    }
                                }

                                //刷新信息屏和灯塔
                                Display900UItem cartPublisherDisplay900UItem = new Display900UItem();
                                cartPublisherDisplay900UItem.Name = currentAstPalletTaskItem.MaterialName;
                                cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
工位：{2}", astPalletTask.ProjectCode, astPalletTask.ProjectStep, cfgWorkStation.Code);
                                cartPublisherDisplay900UItem.LongSubLocation = currentAstPalletTaskItem.ToPickQuantity.ToString(CultureInfo.InvariantCulture);
                                cartPublisherDisplay900UItem.Count = 0;
                                if (currentAstPalletTaskItem.PickedQuantity != null)
                                    cartPublisherDisplay900UItem.Count = (ushort)currentAstPalletTaskItem.PickedQuantity.Value;

                                LightColor cartPublisherLightColor = LightColor.Off;

                                cartPtl900UPublisher.Pressed -= this.cartPtl900UPublisher_Pressed;
                                cartPtl900UPublisher.Pressed += this.cartPtl900UPublisher_Pressed;
                                cartPtl900UPublisher.Clear(true);
                                cartPtl900UPublisher.Lock();
                                cartPtl900UPublisher.Display(cartPublisherDisplay900UItem, cartPublisherLightColor, true);

                                cartPtl900ULight.Clear();

                                this.CurrentAstPalletTaskItemId = currentAstPalletTaskItem.Id;
                                this.CurrentCfgCartId = cfgCart.Id;

                                Logger.Log(this.GetType().Name + "." + this.CFG_ChannelId, DateTime.Now.ToString("HH:mm:ss") + " 点亮按托明细："
                                                                                           + astPalletTask.BatchCode + ", "
                                                                                           + cfgWorkStation.Code + ", "
                                                                                           + cfgPallet.Code + ", "
                                                                                           + currentAstPalletTaskItem.FromPalletPosition + ", "
                                                                                           + currentAstPalletTaskItem.MaterialCode + ", "
                                                                                           + currentAstPalletTaskItem.MaterialName + Environment.NewLine);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        DbEntityValidationException dbEntityValidationException = ex as DbEntityValidationException;
                        if (dbEntityValidationException != null)
                        {
                            foreach (DbEntityValidationResult validationResult in dbEntityValidationException.EntityValidationErrors)
                            {
                                foreach (DbValidationError validationError in validationResult.ValidationErrors)
                                    message += Environment.NewLine + validationError.ErrorMessage;
                            }
                        }
                        message += Environment.NewLine + ex.StackTrace;

                        Logger.Log("AssortingExecutor.threadStart", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                                    + message + Environment.NewLine
                                                                    + Environment.NewLine);
                        Thread.Sleep(1000);
                    }
                }

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 从外部手动触发拣选。
        /// </summary>
        /// <param name="cartPosition"></param>
        public void TryRaisePtl900UPressed(int cartPosition)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                if (this.CurrentAstPalletTaskItemId != null && this.CurrentCfgCartId != null)
                {
                    CFG_Cart cfgCart = dbContext.CFG_Carts
                                           .First(c => c.Id == this.CurrentCfgCartId.Value);
                    AST_PalletTaskItem astPalletTaskItem = dbContext.AST_PalletTaskItems
                                                               .First(pti => pti.Id == this.CurrentAstPalletTaskItemId.Value);

                    CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(this.CurrentCfgCartId.Value);
                    Ptl900U ptl900U = null;

                    Ptl900UPressedEventArgs ptl900UPressedEventArgs = new Ptl900UPressedEventArgs();
                    Display900UItem display900UItem = new Display900UItem();
                    display900UItem.Count = (ushort)astPalletTaskItem.ToPickQuantity;
                    ptl900UPressedEventArgs.ResultByItem.Add(display900UItem, display900UItem.Count);

                    if (this.CurrentAstCartTaskItemId == null)
                    {
                        ptl900U = cartPtl.GetPtl900UByPosition(cartPosition);
                    }
                    else
                    {
                        AST_CartTaskItem astCartTaskItem = dbContext.AST_CartTaskItems
                                                               .First(cti => cti.Id == this.CurrentAstCartTaskItemId.Value);

                        if (cartPosition == astCartTaskItem.CartPosition)
                            ptl900U = cartPtl.GetPtl900UByPosition(astCartTaskItem.CartPosition);
                    }

                    if (ptl900U != null)
                    {
                        Logger.Log(this.GetType().Name + "." + this.CFG_ChannelId, DateTime.Now.ToString("HH:mm:ss") + " 模拟按灭储位指示灯：" + cfgCart.Name + ", " + cartPosition + Environment.NewLine);

                        ptl900U.Clear(true);

                        this.cartPtl900U_Pressed(ptl900U, ptl900UPressedEventArgs);
                    }
                }
            }
        }

        /// <summary>
        /// 按拣选标签表示拣选一个，小车任务是动态生成的。
        /// </summary>
        void cartPtl900U_Pressed(object sender, Ptl900UPressedEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this.CurrentAstPalletTaskItemId == null)
                        return;

                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        Ptl900U cartPtl900U = (Ptl900U)sender;
                        CartPtl cartPtl = CartPtlHost.Instance.GetCartPtlByPtlDevice(cartPtl900U);
                        Ptl900U cartPtl900UPublisher = cartPtl.GetPtl900UPublisher();

                        AST_PalletTaskItem currentAstPalletTaskItem = dbContext.AST_PalletTaskItems
                                                                          .First(ti => ti.Id == this.CurrentAstPalletTaskItemId.Value);
                        AST_PalletTask astPalletTask = currentAstPalletTaskItem.AST_PalletTask;
                        CFG_WorkStation cfgWorkStation = currentAstPalletTaskItem.CFG_WorkStation;
                        CFG_Cart cfgCart = dbContext.CFG_Carts
                                               .First(c => c.Id == cartPtl.CFG_CartId);
                        int toCartPosition = cartPtl900U.Address;

                        //采用并发处理，不允许在未结束目标库位的情况下开始新库位
                        if (this.CurrentAstCartTaskItemId != null)
                        {
                            AST_CartTaskItem astCartTaskItem = dbContext.AST_CartTaskItems
                                                                   .First(cti => cti.Id == this.CurrentAstCartTaskItemId.Value);
                            if (astCartTaskItem.CartPosition != toCartPosition)
                            {
                                cartPtl900U.Display(new Display900UItem(), LightColor.Off, true);

                                return;
                            }
                        }

                        //小车任务是动态生成的
                        if (cfgCart.CartStatus == CartStatus.WaitingAssorting)
                            cfgCart.CartStatus = CartStatus.Assorting;

                        AST_CartTask astCartTask = null;
                        if (this.CurrentAstCartTaskId != null)
                        {
                            astCartTask = dbContext.AST_CartTasks
                                              .First(ct => ct.Id == this.CurrentAstCartTaskId.Value);
                        }
                        else
                        {
                            astCartTask = dbContext.AST_CartTasks
                                              .FirstOrDefault(ct => ct.CFG_CartId == cfgCart.Id && ct.CFG_WorkStationId == cfgWorkStation.Id && ct.BatchCode == astPalletTask.BatchCode);
                        }

                        if (astCartTask == null)
                        {
                            astCartTask = new AST_CartTask();
                            astCartTask.CFG_CartId = cartPtl.CFG_CartId;
                            astCartTask.CFG_WorkStationId = currentAstPalletTaskItem.CFG_WorkStationId;
                            astCartTask.BatchCode = astPalletTask.BatchCode;
                            astCartTask.ProjectCode = astPalletTask.ProjectCode;
                            astCartTask.WbsId = astPalletTask.WbsId;
                            astCartTask.ProjectStep = astPalletTask.ProjectStep;
                            astCartTask.CFG_ChannelId = astPalletTask.CFG_ChannelId;
                            astCartTask.AssortingStatus = AssortingStatus.Assorting;
                            astCartTask.CreateTime = DateTime.Now;

                            dbContext.AST_CartTasks.Add(astCartTask);
                        }

                        AST_CartTaskItem currentAstCartTaskItem = null;
                        if (this.CurrentAstCartTaskItemId != null)
                        {
                            currentAstCartTaskItem = dbContext.AST_CartTaskItems
                                                         .First(cti => cti.Id == this.CurrentAstCartTaskItemId.Value);
                        }

                        if (currentAstCartTaskItem == null)
                        {
                            currentAstCartTaskItem = new AST_CartTaskItem();
                            if (astCartTask.Id == 0)
                                currentAstCartTaskItem.AST_CartTask = astCartTask;
                            else
                                currentAstCartTaskItem.AST_CartTaskId = astCartTask.Id;
                            currentAstCartTaskItem.CartPosition = toCartPosition;
                            currentAstCartTaskItem.AST_PalletTaskItemId = currentAstPalletTaskItem.Id;
                            currentAstCartTaskItem.AssortingStatus = AssortingStatus.Assorting;
                            currentAstCartTaskItem.AssortedQuantity = 0;

                            dbContext.AST_CartTaskItems.Add(currentAstCartTaskItem);
                        }

                        currentAstCartTaskItem.AssortedQuantity += 1;
                        currentAstCartTaskItem.AssortedTime = DateTime.Now;

                        if (currentAstPalletTaskItem.PickedQuantity == null)
                            currentAstPalletTaskItem.PickedQuantity = 0;
                        currentAstPalletTaskItem.PickedQuantity += 1;
                        currentAstPalletTaskItem.PickedTime = DateTime.Now;
                        currentAstPalletTaskItem.PickStatus = PickStatus.Picking;

                        //可能会满或分拣完成，但需要 按灭信息屏 来确认
                        bool waitingConfirm = currentAstCartTaskItem.AssortedQuantity == currentAstPalletTaskItem.MaxQuantityInSingleCartPosition
                                              || currentAstPalletTaskItem.ToPickQuantity == currentAstPalletTaskItem.PickedQuantity;

                        if (waitingConfirm)
                        {
                            currentAstCartTaskItem.AssortingStatus = AssortingStatus.WaitingConfirm;
                        }

                        //放入小车
                        List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                                    .OrderBy(ccm => ccm.Position)
                                                                                    .ToList();
                        CFG_CartCurrentMaterial cfgCartCurrentMaterial = cfgCartCurrentMaterials
                                                                             .First(ccm => ccm.CFG_CartId == astCartTask.CFG_CartId
                                                                                           && ccm.Position == currentAstCartTaskItem.CartPosition);
                        if (currentAstCartTaskItem.Id == 0)
                            cfgCartCurrentMaterial.AST_CartTaskItem = currentAstCartTaskItem;
                        else
                            cfgCartCurrentMaterial.AST_CartTaskItemId = currentAstCartTaskItem.Id;
                        cfgCartCurrentMaterial.ProjectCode = astPalletTask.ProjectCode;
                        cfgCartCurrentMaterial.WbsId = astPalletTask.WbsId;
                        cfgCartCurrentMaterial.ProjectStep = astPalletTask.ProjectStep;
                        cfgCartCurrentMaterial.CFG_WorkStationId = currentAstPalletTaskItem.CFG_WorkStationId;
                        cfgCartCurrentMaterial.BatchCode = astPalletTask.BatchCode;
                        cfgCartCurrentMaterial.CFG_ChannelId = astPalletTask.CFG_ChannelId;
                        cfgCartCurrentMaterial.CFG_PalletId = astPalletTask.CFG_PalletId;
                        cfgCartCurrentMaterial.BoxCode = currentAstPalletTaskItem.BoxCode;
                        cfgCartCurrentMaterial.FromPalletPosition = currentAstPalletTaskItem.FromPalletPosition;
                        cfgCartCurrentMaterial.MaterialCode = currentAstPalletTaskItem.MaterialCode;
                        cfgCartCurrentMaterial.MaterialName = currentAstPalletTaskItem.MaterialName;
                        cfgCartCurrentMaterial.MaterialBarcode = currentAstPalletTaskItem.MaterialBarcode;
                        if (cfgCartCurrentMaterial.Quantity == null)
                            cfgCartCurrentMaterial.Quantity = 0;
                        cfgCartCurrentMaterial.Quantity += 1;
                        cfgCartCurrentMaterial.AssortedTime = currentAstPalletTaskItem.PickedTime;

                        //大件占有旁边的储位
                        if (currentAstPalletTaskItem.IsBig)
                        {
                            CFG_CartCurrentMaterial neighborCfgCartCurrentMaterial = null;
                            if (cfgCartCurrentMaterial.Position == 1)
                                neighborCfgCartCurrentMaterial = cfgCartCurrentMaterials[1];
                            else if (cfgCartCurrentMaterial.Position == 2)
                                neighborCfgCartCurrentMaterial = cfgCartCurrentMaterials[0];
                            else if (cfgCartCurrentMaterial.Position == 3)
                                neighborCfgCartCurrentMaterial = cfgCartCurrentMaterials[3];
                            else if (cfgCartCurrentMaterial.Position == 4)
                                neighborCfgCartCurrentMaterial = cfgCartCurrentMaterials[2];
                            else if (cfgCartCurrentMaterial.Position == 5)
                                neighborCfgCartCurrentMaterial = cfgCartCurrentMaterials[5];
                            else if (cfgCartCurrentMaterial.Position == 6)
                                neighborCfgCartCurrentMaterial = cfgCartCurrentMaterials[4];
                            else if (cfgCartCurrentMaterial.Position == 7)
                                neighborCfgCartCurrentMaterial = cfgCartCurrentMaterials[7];
                            else if (cfgCartCurrentMaterial.Position == 8)
                                neighborCfgCartCurrentMaterial = cfgCartCurrentMaterials[6];

                            if (neighborCfgCartCurrentMaterial != null)
                            {
                                if (currentAstCartTaskItem.Id == 0)
                                    neighborCfgCartCurrentMaterial.AST_CartTaskItem = currentAstCartTaskItem;
                                else
                                    neighborCfgCartCurrentMaterial.AST_CartTaskItemId = currentAstCartTaskItem.Id;
                                neighborCfgCartCurrentMaterial.ProjectCode = astPalletTask.ProjectCode;
                                neighborCfgCartCurrentMaterial.WbsId = astPalletTask.WbsId;
                                neighborCfgCartCurrentMaterial.ProjectStep = astPalletTask.ProjectStep;
                                neighborCfgCartCurrentMaterial.CFG_WorkStationId = currentAstPalletTaskItem.CFG_WorkStationId;
                                neighborCfgCartCurrentMaterial.BatchCode = astPalletTask.BatchCode;
                                neighborCfgCartCurrentMaterial.CFG_ChannelId = astPalletTask.CFG_ChannelId;
                                neighborCfgCartCurrentMaterial.CFG_PalletId = astPalletTask.CFG_PalletId;
                                neighborCfgCartCurrentMaterial.BoxCode = currentAstPalletTaskItem.BoxCode;
                                neighborCfgCartCurrentMaterial.FromPalletPosition = currentAstPalletTaskItem.FromPalletPosition;
                                neighborCfgCartCurrentMaterial.MaterialCode = currentAstPalletTaskItem.MaterialCode;
                                neighborCfgCartCurrentMaterial.MaterialName = currentAstPalletTaskItem.MaterialName;
                                neighborCfgCartCurrentMaterial.MaterialBarcode = currentAstPalletTaskItem.MaterialBarcode;
                                if (neighborCfgCartCurrentMaterial.Quantity == null)
                                    neighborCfgCartCurrentMaterial.Quantity = 0;
                                neighborCfgCartCurrentMaterial.AssortedTime = currentAstPalletTaskItem.PickedTime;
                                neighborCfgCartCurrentMaterial.Usability = CartPositionUsability.DisableByNeighborBigMaterial;
                            }
                        }

                        dbContext.SaveChanges();

                        Display900UItem cartPublisherDisplay900UItem = new Display900UItem();
                        cartPublisherDisplay900UItem.Name = currentAstPalletTaskItem.MaterialName;
                        cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
工位：{2}", astPalletTask.ProjectCode, astPalletTask.ProjectStep, cfgWorkStation.Code);
                        cartPublisherDisplay900UItem.LongSubLocation = currentAstPalletTaskItem.ToPickQuantity.ToString(CultureInfo.InvariantCulture);
                        cartPublisherDisplay900UItem.Count = (ushort)currentAstPalletTaskItem.PickedQuantity.Value;

                        Display900UItem cartDisplay900UItem = new Display900UItem();
                        cartDisplay900UItem.Count = (ushort)cfgCartCurrentMaterial.Quantity.Value;

                        LightColor cartPublisherLightColor = LightColor.Off;
                        LightColor lightColor = LightColor.Green;

                        if (waitingConfirm)
                        {
                            cartPublisherLightColor = LightColor.Green;
                            lightColor = LightColor.Yellow;
                        }

                        //小车指示
                        if (waitingConfirm)
                        {
                            cartPtl900U.Pressed -= this.cartPtl900U_Pressed;
                            cartPtl900U.Lock();
                            cartPtl900U.Display(cartDisplay900UItem, lightColor);

                            cartPtl900UPublisher.Clear(true);
                            cartPtl900UPublisher.Unlock();
                            cartPtl900UPublisher.Display(cartPublisherDisplay900UItem, cartPublisherLightColor, true);
                        }
                        else
                        {
                            cartPtl900U.Display(cartDisplay900UItem, lightColor, true);

                            cartPtl900UPublisher.Clear(true);
                            cartPtl900UPublisher.Lock();
                            cartPtl900UPublisher.Display(cartPublisherDisplay900UItem, LightColor.Off, true);
                        }

                        //熄灭其他未采用的库位
                        foreach (CFG_CartCurrentMaterial emptyCfgCartCurrentMaterial in cfgCartCurrentMaterials)
                        {
                            if (emptyCfgCartCurrentMaterial.Quantity == null || emptyCfgCartCurrentMaterial.Quantity == 0)
                            {
                                Ptl900U emptyCartPtl900U = cartPtl.GetPtl900UByPosition(emptyCfgCartCurrentMaterial.Position);
                                emptyCartPtl900U.Clear(true);
                            }
                        }

                        this.CurrentAstCartTaskItemId = currentAstCartTaskItem.Id;
                        this.CurrentAstCartTaskId = astCartTask.Id;

                        string logMessage = " 按灭储位指示灯：" + cfgCart.Name + ", " + toCartPosition;
                        if (waitingConfirm)
                            logMessage += ", 等待确认";
                        Logger.Log(this.GetType().Name + "." + this.CFG_ChannelId, DateTime.Now.ToString("HH:mm:ss") + logMessage + Environment.NewLine);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    DbEntityValidationException dbEntityValidationException = ex as DbEntityValidationException;
                    if (dbEntityValidationException != null)
                    {
                        foreach (DbEntityValidationResult validationResult in dbEntityValidationException.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationResult.ValidationErrors)
                                message += Environment.NewLine + validationError.ErrorMessage;
                        }
                    }
                    message += Environment.NewLine + ex.StackTrace;

                    Logger.Log("AssortingExecutor.cartPtl900U_Pressed", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                                        + message + Environment.NewLine
                                                                        + Environment.NewLine);

                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 从外部手动触发确认。
        /// </summary>
        public void TryRaisePtlPublisherPressed()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                if (this.CurrentAstCartTaskItemId != null && this.CurrentCfgCartId != null)
                {
                    CFG_Cart cfgCart = dbContext.CFG_Carts
                                           .First(c => c.Id == this.CurrentCfgCartId.Value);
                    AST_CartTaskItem astCartTaskItem = dbContext.AST_CartTaskItems
                                                           .First(cti => cti.Id == this.CurrentAstCartTaskItemId.Value);

                    CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(this.CurrentCfgCartId.Value);
                    Ptl900U ptlPublisher = cartPtl.GetPtl900UPublisher();

                    Ptl900UPressedEventArgs ptl900UPressedEventArgs = new Ptl900UPressedEventArgs();
                    Display900UItem display900UItem = new Display900UItem();
                    display900UItem.Count = (ushort)astCartTaskItem.AssortedQuantity.Value;
                    ptl900UPressedEventArgs.ResultByItem.Add(display900UItem, display900UItem.Count);

                    Logger.Log(this.GetType().Name + "." + this.CFG_ChannelId, DateTime.Now.ToString("HH:mm:ss") + " 模拟按灭信息显式屏：" + cfgCart.Name + Environment.NewLine);

                    ptlPublisher.Clear(true);
                    ptlPublisher.Unlock();

                    this.cartPtl900UPublisher_Pressed(ptlPublisher, ptl900UPressedEventArgs);
                }
            }
        }

        /// <summary>
        /// 按发布器表示确认和短拣零拣，但只可以从客户端短拣零拣。
        /// </summary>
        void cartPtl900UPublisher_Pressed(object sender, Ptl900UPressedEventArgs e)
        {
            while (true)
            {
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        Ptl900U cartPtl900UPublisher = (Ptl900U)sender;
                        CartPtl cartPtl = CartPtlHost.Instance.GetCartPtlByPtlDevice(cartPtl900UPublisher);

                        AST_PalletTaskItem currentAstPalletTaskItem = dbContext.AST_PalletTaskItems
                                                                          .First(pi => pi.Id == this.CurrentAstPalletTaskItemId);
                        AST_PalletTask astPalletTask = currentAstPalletTaskItem.AST_PalletTask;
                        List<AST_PalletTaskItem> astPalletTaskItems = astPalletTask.AST_PalletTaskItems
                                                                          .ToList();
                        CFG_WorkStation cfgWorkStation = currentAstPalletTaskItem.CFG_WorkStation;
                        CFG_Cart cfgCart = dbContext.CFG_Carts
                                               .First(c => c.Id == cartPtl.CFG_CartId);
                        List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                                    .OrderBy(ccm => ccm.Position)
                                                                                    .ToList();

                        KeyValuePair<Display900UItem, ushort> display900UitemAndResult = e.ResultByItem.First();

                        if (currentAstPalletTaskItem.PickedQuantity == null)
                            currentAstPalletTaskItem.PickedQuantity = 0;
                        if (currentAstPalletTaskItem.PickedTime == null)
                            currentAstPalletTaskItem.PickedTime = DateTime.Now;

                        //可能为空是因为跳过库位标签直接按信息发布屏
                        AST_CartTaskItem nullableCurrentAstCartTaskItem = null;
                        AST_CartTask nullableAstCartTask = null;
                        if (this.CurrentAstCartTaskItemId != null)
                        {
                            nullableCurrentAstCartTaskItem = dbContext.AST_CartTaskItems
                                                                 .First(cti => cti.Id == this.CurrentAstCartTaskItemId.Value);
                            nullableAstCartTask = nullableCurrentAstCartTaskItem.AST_CartTask;

                            //小车明细完成
                            nullableCurrentAstCartTaskItem.AssortingStatus = AssortingStatus.Finished;

                            //库位已满
                            foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in nullableCurrentAstCartTaskItem.CFG_CartCurrentMaterials)
                            {
                                if (cfgCartCurrentMaterial.Usability == CartPositionUsability.Enable)
                                    cfgCartCurrentMaterial.Usability = CartPositionUsability.DisableByFull;
                            }
                        }

                        //托盘明细完成 = 数量拣足 或者 当前小车储位数量小于容量的零拣短拣
                        bool currentAstPalletTaskItemFinished = false;
                        if (currentAstPalletTaskItem.ToPickQuantity == currentAstPalletTaskItem.PickedQuantity
                            || display900UitemAndResult.Value < currentAstPalletTaskItem.MaxQuantityInSingleCartPosition)
                        {
                            currentAstPalletTaskItem.PickStatus = PickStatus.Finished;
                            currentAstPalletTaskItemFinished = true;
                        }

                        //整车已满 = 所有库位完成 或者 该批次该工位没有未完成的任务
                        bool wholeCartIsFull = cfgCartCurrentMaterials.All(ccm => ccm.AST_CartTaskItemId != null
                                                                                  && ccm.AST_CartTaskItem.AssortingStatus == AssortingStatus.Finished)
                                               || dbContext.AST_LesTaskItems.Count(lti => lti.AST_LesTask.BatchCode == astPalletTask.BatchCode
                                                                                          && lti.AST_LesTask.CFG_ChannelId == astPalletTask.CFG_ChannelId
                                                                                          && lti.AST_LesTask.CFG_WorkStationId == currentAstPalletTaskItem.CFG_WorkStationId
                                                                                          && (lti.AST_PalletTaskItem == null
                                                                                              || (lti.AST_PalletTaskItemId != currentAstPalletTaskItem.Id
                                                                                                  && lti.AST_PalletTaskItem.PickStatus != PickStatus.Finished)
                                                                                              || (lti.AST_PalletTaskItemId == currentAstPalletTaskItem.Id
                                                                                                  && !currentAstPalletTaskItemFinished))) == 0;

                        //如果存在大件而小车没有整层空位，也满
                        if (!wholeCartIsFull
                            && !(cfgCartCurrentMaterials[0].Usability == CartPositionUsability.Enable && cfgCartCurrentMaterials[1].Usability == CartPositionUsability.Enable)
                            && !(cfgCartCurrentMaterials[2].Usability == CartPositionUsability.Enable && cfgCartCurrentMaterials[3].Usability == CartPositionUsability.Enable)
                            && !(cfgCartCurrentMaterials[4].Usability == CartPositionUsability.Enable && cfgCartCurrentMaterials[5].Usability == CartPositionUsability.Enable)
                            && !(cfgCartCurrentMaterials[6].Usability == CartPositionUsability.Enable && cfgCartCurrentMaterials[7].Usability == CartPositionUsability.Enable)
                            && dbContext.AST_LesTaskItems.Count(lti => lti.AST_LesTask.BatchCode == astPalletTask.BatchCode
                                                                       && lti.AST_LesTask.CFG_ChannelId == astPalletTask.CFG_ChannelId
                                                                       && lti.AST_LesTask.CFG_WorkStationId == currentAstPalletTaskItem.CFG_WorkStationId
                                                                       && lti.IsBig
                                                                       && (lti.AST_PalletTaskItem == null
                                                                           || (lti.AST_PalletTaskItemId != currentAstPalletTaskItem.Id
                                                                               && lti.AST_PalletTaskItem.PickStatus != PickStatus.Finished)
                                                                           || (lti.AST_PalletTaskItemId == currentAstPalletTaskItem.Id
                                                                               && !currentAstPalletTaskItemFinished))) > 0)
                        {
                            wholeCartIsFull = true;
                        }

                        if (wholeCartIsFull)
                        {
                            if (nullableAstCartTask != null)
                                nullableAstCartTask.AssortingStatus = AssortingStatus.Finished;
                            cfgCart.CartStatus = CartStatus.Assorted;

                            #region 当前小车提交

                            List<CFG_CartCurrentMaterial> notNullCartCfgCartCurrentMaterials = cfgCartCurrentMaterials
                                                                                                   .Where(ccm => ccm.Quantity != null)
                                                                                                   .ToList();
                            CFG_CartCurrentMaterial firstFinishedCfgCartCurrentMaterial = notNullCartCfgCartCurrentMaterials
                                                                                              .OrderBy(ccm => ccm.AssortedTime)
                                                                                              .First();
                            CFG_CartCurrentMaterial lastFinishedCfgCartCurrentMaterial = notNullCartCfgCartCurrentMaterials
                                                                                             .OrderByDescending(ccm => ccm.AssortedTime)
                                                                                             .First();

                            AST_CartResult astCartResult = new AST_CartResult();
                            astCartResult.ProjectCode = astPalletTask.ProjectCode;
                            astCartResult.WbsId = astPalletTask.WbsId;
                            astCartResult.ProjectStep = astPalletTask.ProjectStep;
                            astCartResult.CFG_WorkStationId = cfgWorkStation.Id;
                            astCartResult.BatchCode = astPalletTask.BatchCode;
                            astCartResult.CFG_ChannelId = astPalletTask.CFG_ChannelId;
                            astCartResult.CFG_CartId = cfgCart.Id;
                            astCartResult.BeginPickTime = firstFinishedCfgCartCurrentMaterial.AssortedTime.Value;
                            astCartResult.EndPickTime = lastFinishedCfgCartCurrentMaterial.AssortedTime.Value;
                            astCartResult.CFG_EmployeeId = lastFinishedCfgCartCurrentMaterial.CFG_EmployeeId;

                            dbContext.AST_CartResults.Add(astCartResult);

                            AST_CartResultMessage astCartResultMessage = new AST_CartResultMessage();
                            astCartResultMessage.AST_CartResult = astCartResult;
                            astCartResultMessage.SentSuccessful = false;

                            dbContext.AST_CartResultMessages.Add(astCartResultMessage);

                            foreach (CFG_CartCurrentMaterial temporaryCfgCartCurrentMaterial in notNullCartCfgCartCurrentMaterials)
                            {
                                AST_CartResultItem astCartResultItem = new AST_CartResultItem();
                                astCartResultItem.AST_CartResult = astCartResult;
                                astCartResultItem.CartPosition = temporaryCfgCartCurrentMaterial.Position;
                                astCartResultItem.MaterialCode = temporaryCfgCartCurrentMaterial.MaterialCode;
                                astCartResultItem.MaterialName = temporaryCfgCartCurrentMaterial.MaterialName;
                                astCartResultItem.MaterialBarcode = temporaryCfgCartCurrentMaterial.MaterialBarcode;
                                astCartResultItem.Quantity = temporaryCfgCartCurrentMaterial.Quantity.Value;

                                dbContext.AST_CartResultItems.Add(astCartResultItem);
                            }

                            #endregion

                            //生成拣料区配送任务
                            DST_AgvSwitch dstAgvSwitch = dbContext.DST_AgvSwitchs.FirstOrDefault(t => t.isOpen);
                            if (dstAgvSwitch != null)
                            {
                                List<DST_DistributeTask> distributeTasks = DistributingTaskGenerator.Instance.GeneratePickAreaDistributeTask(cfgCart);
                                foreach (DST_DistributeTask distributeTask in distributeTasks)
                                {
                                    dbContext.DST_DistributeTasks.Add(distributeTask);
                                }
                            }
                            else
                            {
                                //自动移出并拉到物料超市
                                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                                   .First(ccc => ccc.CFG_ChannelId == this.CFG_ChannelId && ccc.CFG_CartId == cfgCart.Id);
                                cfgChannelCurrentCart.CFG_CartId = null;
                                cfgChannelCurrentCart.DockedTime = null;
                            }
                            cfgCart.CartStatus = CartStatus.WaitingToBufferArea;
                            cfgCart.CartStatus = CartStatus.InCarriageToBufferArea;
                            cfgCart.CartStatus = CartStatus.ArrivedAtBufferArea;

                            //尝试发起补充到生产线的搬运任务，需判断有没有在途的
                            int workStationCurrenCartCount = dbContext.CFG_WorkStationCurrentCarts
                                                                 .Count(wscc => wscc.CFG_WorkStationId == cfgWorkStation.Id
                                                                                && wscc.CFG_CartId != null)
                                                             + dbContext.FND_Tasks
                                                                   .Where(t => t.CFG_WorkStationId == cfgWorkStation.Id
                                                                               && t.CFG_Cart.CFG_CartCurrentMaterials.FirstOrDefault(m => m.CFG_WorkStationId == cfgWorkStation.Id) != null
                                                                               && (t.CFG_Cart.CartStatus == CartStatus.NeedToWorkStation
                                                                                   || t.CFG_Cart.CartStatus == CartStatus.WaitingToWorkStation
                                                                                   || t.CFG_Cart.CartStatus == CartStatus.InCarriageToWorkStation))
                                                                   .Select(t => t.CFG_CartId).Distinct().Count();
                            if (workStationCurrenCartCount < 8) //8：生产线边有 8 个车位
                            {
                                CFG_WorkStationCurrentCart firstEmptyCfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                                                     .FirstOrDefault(wscc => wscc.CFG_WorkStationId == cfgWorkStation.Id
                                                                                                                             && wscc.CFG_CartId == null);
                                if (firstEmptyCfgWorkStationCurrentCart != null)
                                {
                                    cfgCart.CartStatus = CartStatus.NeedToWorkStation;

                                    CFG_CartCurrentMaterial firstCfgCartFirstCartCurrentMaterial = cfgCart.CFG_CartCurrentMaterials
                                                                                                       .First(ccm => ccm.Quantity > 0);

                                    FND_Task fndTask = new FND_Task();
                                    fndTask.ProjectCode = firstCfgCartFirstCartCurrentMaterial.ProjectCode;
                                    fndTask.ProjectStep = firstCfgCartFirstCartCurrentMaterial.ProjectStep;
                                    fndTask.BatchCode = firstCfgCartFirstCartCurrentMaterial.BatchCode;
                                    fndTask.MaxNeedArrivedTime = DateTime.Now.AddHours(1);
                                    fndTask.RequestTime = DateTime.Now;
                                    fndTask.CFG_WorkStationId = firstCfgCartFirstCartCurrentMaterial.CFG_WorkStationId.Value;
                                    fndTask.CFG_CartId = cfgCart.Id;
                                    fndTask.LightColor = (byte)LightColor.Off;
                                    fndTask.FindingStatus = FindingStatus.New;

                                    dbContext.FND_Tasks.Add(fndTask);
                                }
                            }
                        }

                        //整托完成
                        bool wholePalletFinished = astPalletTask.AST_PalletTaskItems
                                                       .All(pti => pti.PickStatus == PickStatus.Finished);
                        if (wholePalletFinished)
                        {
                            astPalletTask.PickStatus = PickStatus.Finished;

                            #region 当前托盘提交

                            AST_PalletTaskItem firstFinishedAstPalletTaskItem = astPalletTaskItems
                                                                                    .OrderBy(ti => ti.PickedTime)
                                                                                    .First();
                            AST_PalletTaskItem lastFinishedAstPalletTaskItem = astPalletTaskItems
                                                                                   .OrderByDescending(ti => ti.PickedTime)
                                                                                   .First();

                            AST_PalletResult astPalletResult = new AST_PalletResult();
                            astPalletResult.ProjectCode = astPalletTask.ProjectCode;
                            astPalletResult.WbsId = astPalletTask.WbsId;
                            astPalletResult.ProjectStep = astPalletTask.ProjectStep;
                            astPalletResult.BatchCode = astPalletTask.BatchCode;
                            astPalletResult.CFG_ChannelId = astPalletTask.CFG_ChannelId;
                            astPalletResult.CFG_PalletId = astPalletTask.CFG_PalletId;
                            astPalletResult.BeginPickTime = firstFinishedAstPalletTaskItem.PickedTime.Value;
                            astPalletResult.EndPickTime = lastFinishedAstPalletTaskItem.PickedTime.Value;
                            astPalletResult.CFG_EmployeeId = lastFinishedAstPalletTaskItem.CFG_EmployeeId;

                            dbContext.AST_PalletResults.Add(astPalletResult);

                            AST_PalletResultMessage astPalletResultMessage = new AST_PalletResultMessage();
                            astPalletResultMessage.AST_PalletResult = astPalletResult;
                            astPalletResultMessage.SentSuccessful = false;

                            dbContext.AST_PalletResultMessages.Add(astPalletResultMessage);

                            //提交每个原始明细，需要考虑在料车与储位上的拆分
                            foreach (AST_PalletTaskItem temporaryAstPalletTaskItem in astPalletTaskItems)
                            {
                                List<AST_LesTaskItem> splittedAstLesTaskItems = temporaryAstPalletTaskItem.AST_LesTaskItems
                                                                                    .OrderBy(lti => lti.BillDetailId)
                                                                                    .ToList();
                                List<AST_CartTaskItem> splittedAstCartTaskItems = temporaryAstPalletTaskItem.AST_CartTaskItems
                                                                                      .ToList();

                                //用于辅助托盘明细到原始明细的还原
                                int uncorrelatedPalletTaskItemPickedQuantity = temporaryAstPalletTaskItem.PickedQuantity.Value;

                                //用于辅助料车明细到原始明细的还原
                                int uncorrelatedLesTaskItemToPickQuantity;
                                Dictionary<AST_CartTaskItem, int> correlatedPickedQuantityByCartTaskItem = new Dictionary<AST_CartTaskItem, int>();
                                foreach (AST_CartTaskItem astCartTaskItem in splittedAstCartTaskItems)
                                    correlatedPickedQuantityByCartTaskItem.Add(astCartTaskItem, 0);

                                foreach (AST_LesTaskItem splittedAstLesTaskItem in splittedAstLesTaskItems)
                                {
                                    AST_LesTask splittedAstLesTask = splittedAstLesTaskItem.AST_LesTask;

                                    uncorrelatedLesTaskItemToPickQuantity = splittedAstLesTaskItem.ToPickQuantity;

                                    foreach (AST_CartTaskItem splittedAstCartTaskItem in splittedAstCartTaskItems)
                                    {
                                        int correlatedPickedQuantity = correlatedPickedQuantityByCartTaskItem[splittedAstCartTaskItem];
                                        if (uncorrelatedLesTaskItemToPickQuantity > 0 && splittedAstCartTaskItem.AssortedQuantity > correlatedPickedQuantity)
                                        {
                                            AST_PalletResultItem astPalletResultItem = new AST_PalletResultItem();
                                            astPalletResultItem.AST_PalletResult = astPalletResult;
                                            astPalletResultItem.CFG_WorkStationId = splittedAstLesTask.CFG_WorkStationId;
                                            astPalletResultItem.GzzList = splittedAstLesTask.GzzList;
                                            astPalletResultItem.BillDetailId = splittedAstLesTaskItem.BillDetailId;
                                            astPalletResultItem.BoxCode = splittedAstLesTask.BoxCode;
                                            astPalletResultItem.PalletPosition = splittedAstLesTask.FromPalletPosition;
                                            astPalletResultItem.MaterialCode = splittedAstLesTaskItem.MaterialCode;
                                            astPalletResultItem.MaterialName = splittedAstLesTaskItem.MaterialName;
                                            astPalletResultItem.MaterialBarcode = splittedAstLesTaskItem.MaterialBarcode;
                                            astPalletResultItem.ToPickQuantity = splittedAstLesTaskItem.ToPickQuantity;
                                            astPalletResultItem.PickedQuantity = Math.Min(uncorrelatedLesTaskItemToPickQuantity, splittedAstCartTaskItem.AssortedQuantity.Value - correlatedPickedQuantity);
                                            astPalletResultItem.CFG_CartId = splittedAstCartTaskItem.AST_CartTask.CFG_CartId;
                                            astPalletResultItem.CartPosition = splittedAstCartTaskItem.CartPosition;

                                            dbContext.AST_PalletResultItems.Add(astPalletResultItem);

                                            uncorrelatedPalletTaskItemPickedQuantity -= astPalletResultItem.PickedQuantity;
                                            uncorrelatedLesTaskItemToPickQuantity -= astPalletResultItem.PickedQuantity;
                                            correlatedPickedQuantityByCartTaskItem[splittedAstCartTaskItem] += astPalletResultItem.PickedQuantity;
                                        }
                                    }

                                    if (uncorrelatedLesTaskItemToPickQuantity > 0)
                                    {
                                        AST_PalletResultItem astPalletResultItem = new AST_PalletResultItem();
                                        astPalletResultItem.AST_PalletResult = astPalletResult;
                                        astPalletResultItem.CFG_WorkStationId = splittedAstLesTask.CFG_WorkStationId;
                                        astPalletResultItem.GzzList = splittedAstLesTask.GzzList;
                                        astPalletResultItem.BillDetailId = splittedAstLesTaskItem.BillDetailId;
                                        astPalletResultItem.BoxCode = splittedAstLesTask.BoxCode;
                                        astPalletResultItem.PalletPosition = splittedAstLesTask.FromPalletPosition;
                                        astPalletResultItem.MaterialCode = splittedAstLesTaskItem.MaterialCode;
                                        astPalletResultItem.MaterialName = splittedAstLesTaskItem.MaterialName;
                                        astPalletResultItem.MaterialBarcode = splittedAstLesTaskItem.MaterialBarcode;
                                        astPalletResultItem.ToPickQuantity = splittedAstLesTaskItem.ToPickQuantity;
                                        astPalletResultItem.PickedQuantity = 0;
                                        astPalletResultItem.CFG_CartId = null;
                                        astPalletResultItem.CartPosition = null;

                                        dbContext.AST_PalletResultItems.Add(astPalletResultItem);
                                    }
                                }
                            }

                            #endregion

                            //当前任务的托盘移出
                            CFG_ChannelCurrentPallet cfgChannelCurrentPallet = dbContext.CFG_ChannelCurrentPallets
                                                                                   .FirstOrDefault(ccp => ccp.CFG_ChannelId == astPalletTask.CFG_ChannelId && ccp.CFG_PalletId == astPalletTask.CFG_PalletId);
                            if (cfgChannelCurrentPallet != null)
                            {
                                cfgChannelCurrentPallet.CFG_PalletId = null;
                                cfgChannelCurrentPallet.ArrivedTime = null;
                            }
                        }

                        string gzzList = this.GetGzzListFromCfgCartMaterial(cfgCart);

                        dbContext.SaveChanges();

                        ChannelPtl channelPtl = ChannelPtlHost.Instance.GetChannelPtl(astPalletTask.CFG_ChannelId);
                        int fromPalletPosition = currentAstPalletTaskItem.FromPalletPosition;
                        if (fromPalletPosition > 5)
                            fromPalletPosition = 3;
                        Ptl900U channelPtl900U = channelPtl.GetPtl900UByPosition(fromPalletPosition);
                        Ptl900U cartPtl900ULight = cartPtl.GetPtl900ULight();

                        Display900UItem cartPublisherDisplay900UItem = new Display900UItem();
                        cartPublisherDisplay900UItem.Name = "批次：" + astPalletTask.BatchCode;
                        cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
工位：{2}", astPalletTask.ProjectCode, astPalletTask.ProjectStep, cfgWorkStation.Code);
                        cartPublisherDisplay900UItem.LongSubLocation = string.Empty;
                        cartPublisherDisplay900UItem.Count = (ushort)cfgCartCurrentMaterials
                                                                         .Where(ccm => ccm.Quantity != null)
                                                                         .Select(ccm => ccm.Quantity.Value)
                                                                         .Sum();
                        cartPublisherDisplay900UItem.Unit = "个";

                        if (nullableCurrentAstCartTaskItem != null)
                        {
                            Ptl900U cartPtl900U = cartPtl.GetPtl900UByPosition(nullableCurrentAstCartTaskItem.CartPosition);

                            cartPtl900U.Pressed -= this.cartPtl900U_Pressed;
                            cartPtl900U.Clear(true);
                        }

                        LightColor cartPublisherLightColor = LightColor.Off;
                        LightColor cartLighthouseLightColor = LightColor.Off;

                        if (wholeCartIsFull)
                        {
                            cartPublisherDisplay900UItem.Name = "已分拣完成";
                            cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}，{1}
批次：{2}
工位：{3}，{4}", astPalletTask.ProjectCode, astPalletTask.ProjectStep, astPalletTask.BatchCode, cfgWorkStation.Code, gzzList);

                            cartLighthouseLightColor = LightColor.Cyan;
                        }

                        if (currentAstPalletTaskItem.PickStatus == PickStatus.Finished)
                            channelPtl900U.Clear();

                        //空闲库位清空
                        if (wholeCartIsFull || nullableCurrentAstCartTaskItem == null)
                        {
                            foreach (CFG_CartCurrentMaterial temporaryCfgCartCurrentMaterial in cfgCartCurrentMaterials)
                            {
                                if (temporaryCfgCartCurrentMaterial.Quantity == null)
                                {
                                    Ptl900U cartPtl900UFree = cartPtl.GetPtl900UByPosition(temporaryCfgCartCurrentMaterial.Position);
                                    cartPtl900UFree.Pressed -= this.cartPtl900U_Pressed;
                                    cartPtl900UFree.Clear(true);
                                }
                            }
                        }

                        cartPtl900UPublisher.Pressed -= this.cartPtl900UPublisher_Pressed;
                        cartPtl900UPublisher.Lock();
                        cartPtl900UPublisher.Display(cartPublisherDisplay900UItem, cartPublisherLightColor);

                        cartPtl900ULight.Clear();
                        cartPtl900ULight.Display(new Display900UItem(), cartLighthouseLightColor);

                        if (wholePalletFinished)
                            this.CurrentAstPalletTaskId = null;
                        this.CurrentAstPalletTaskItemId = null;
                        this.CurrentCfgCartId = null;
                        this.CurrentAstCartTaskId = null;
                        this.CurrentAstCartTaskItemId = null;

                        string logMessage = " 按灭信息显式屏：" + cfgCart.Name;
                        if (wholeCartIsFull)
                            logMessage += ", 整车完成";
                        if (wholePalletFinished)
                            logMessage += ", 整托完成";
                        Logger.Log(this.GetType().Name + "." + this.CFG_ChannelId, DateTime.Now.ToString("HH:mm:ss") + logMessage + Environment.NewLine);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    DbEntityValidationException dbEntityValidationException = ex as DbEntityValidationException;
                    if (dbEntityValidationException != null)
                    {
                        foreach (DbEntityValidationResult validationResult in dbEntityValidationException.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationResult.ValidationErrors)
                                message += Environment.NewLine + validationError.ErrorMessage;
                        }
                    }
                    message += Environment.NewLine + ex.StackTrace;

                    Logger.Log("AssortingExecutor.cartPtl900UPublisher_Pressed", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                                                 + message + Environment.NewLine
                                                                                 + Environment.NewLine);

                    Thread.Sleep(1000);
                }
            }
        }

        string GetGzzListFromCfgCartMaterial(CFG_Cart cfgCart)
        {
            List<string> gzzList = new List<string>();
            foreach (CFG_CartCurrentMaterial cfgCartCurrentMateiral in cfgCart.CFG_CartCurrentMaterials)
            {
                if (cfgCartCurrentMateiral.AST_CartTaskItem != null)
                {
                    foreach (AST_LesTaskItem astLesTaskItem in cfgCartCurrentMateiral.AST_CartTaskItem.AST_PalletTaskItem.AST_LesTaskItems)
                    {
                        string[] gzzArray = astLesTaskItem.AST_LesTask.GzzList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string gzz in gzzArray)
                        {
                            if (!gzzList.Contains(gzz))
                                gzzList.Add(gzz);
                        }
                    }
                }
            }
            gzzList.Sort();

            return string.Join(",", gzzList);
        }
    }
}