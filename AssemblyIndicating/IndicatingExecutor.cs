using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.AssemblyIndicating;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using DeviceCommunicationHost;
using Distributing;
using Ptl.Device;
using Ptl.Device.Communication.Command;

namespace AssemblyIndicating
{
    /// <summary>
    /// 为各自工位执行装配指示任务。
    /// </summary>
    public class IndicatingExecutor
    {
        /// <summary>
        /// 获取服务于工位的主键。
        /// </summary>
        public int CFG_WorkStationId { get; private set; }

        /// <summary>
        /// 获取当前正在进行的装配任务的主键。
        /// </summary>
        public long? CurrentAsmTaskId { get; private set; }

        /// <summary>
        /// 获取当前正在进行的装配明细的主键。
        /// </summary>
        public long? CurrentAsmTaskItemId { get; private set; }

        readonly Thread thread;

        /// <summary>
        /// 一段时间无法交互，则在此超时时间后自动交互。
        /// </summary>
        readonly TimeSpan autoPressTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 使用工位主键初始化执行器。
        /// </summary>
        /// <param name="cfgWorkStationId">服务于工位的主键。</param>
        public IndicatingExecutor(int cfgWorkStationId)
        {
            this.CFG_WorkStationId = cfgWorkStationId;

            this.Restore();

            this.thread = new Thread(this.threadStart);
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                     .First(ws => ws.Id == this.CFG_WorkStationId);
                this.thread.Name = string.Format(CultureInfo.InvariantCulture, "{0}({1})", this.GetType().FullName, cfgWorkStation.Name);
            }
            this.thread.IsBackground = true;
            this.thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            this.thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

            this.thread.Start();
        }

        /// <summary>
        /// 还原之前的停靠状态。
        /// </summary>
        void Restore()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                     .First(ws => ws.Id == this.CFG_WorkStationId);
                List<CFG_WorkStationCurrentCart> cfgWorkStationCurrentCarts = dbContext.CFG_WorkStationCurrentCarts
                                                                                  .Include(wscc => wscc.CFG_WorkStation)
                                                                                  .Include(wscc => wscc.CFG_Cart)
                                                                                  .Include(wscc => wscc.CFG_Cart.CFG_CartCurrentMaterials)
                                                                                  .Where(wscc => wscc.CFG_WorkStationId == this.CFG_WorkStationId && wscc.CFG_CartId != null)
                                                                                  .ToList();

                foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in cfgWorkStationCurrentCarts)
                {
                    CFG_Cart cfgCart = cfgWorkStationCurrentCart.CFG_Cart;
                    CFG_CartCurrentMaterial firstNotEmptyCfgCartCurrentMaterial = cfgCart.CFG_CartCurrentMaterials
                                                                                      .FirstOrDefault(ccm => ccm.AST_CartTaskItemId != null);

                    CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(cfgCart.Id);
                    Ptl900U cartPtl900UPublisher = cartPtl.GetPtl900UPublisher();

                    Display900UItem cartPublisherDisplay900UItem = new Display900UItem();
                    cartPublisherDisplay900UItem.Name = "抵达工位 " + cfgWorkStation.Name;
                    if (firstNotEmptyCfgCartCurrentMaterial != null)
                    {
                        cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
批次：{2}", firstNotEmptyCfgCartCurrentMaterial.ProjectCode, firstNotEmptyCfgCartCurrentMaterial.ProjectStep, firstNotEmptyCfgCartCurrentMaterial.BatchCode);
                    }
                    cartPublisherDisplay900UItem.Count = (ushort)cfgWorkStationCurrentCart.Position;
                    cartPublisherDisplay900UItem.Unit = "位";

                    cartPtl900UPublisher.Lock();
                    cartPtl900UPublisher.Display(cartPublisherDisplay900UItem, LightColor.Off);
                }

                Logger.Log(this.GetType().Name + "." + cfgWorkStation.Code, DateTime.Now.ToString("HH:mm:ss") + " Restore() 完成" + Environment.NewLine);
            }
        }

        /// <summary>
        /// 启动指定的装配指引任务。
        /// </summary>
        /// <param name="asmTaskId">装配指引任务的主键。</param>
        public void Start(long asmTaskId)
        {
            if (this.CurrentAsmTaskId != null)
                throw new InvalidOperationException();

            this.CurrentAsmTaskId = asmTaskId;
        }

        void threadStart(object notUsed)
        {
            while (true)
            {
                //加载线程推动新明细，按钮交互推进明细的进度
                if (this.CurrentAsmTaskId != null && this.CurrentAsmTaskItemId == null)
                {
                    try
                    {
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                                 .First(ws => ws.Id == this.CFG_WorkStationId);
                            ASM_Task asmTask = dbContext.ASM_Tasks
                                                   .First(t => t.Id == this.CurrentAsmTaskId.Value);

                            //因为 IndicatingExecutorLoader 加载和 IndicatingExecutor 是不同线程，所以再次核对状态
                            if (asmTask.AssembleStatus == AssembleStatus.Finished)
                            {
                                this.CurrentAsmTaskId = null;

                                continue;
                            }

                            ASM_AssembleIndication asmAssembleIndication = asmTask.ASM_AssembleIndication;
                            ASM_TaskItem asmTaskItem = dbContext.ASM_TaskItems
                                                           .OrderBy(ti => ti.Gzz)
                                                           .ThenBy(ti => ti.AssembleSequence)
                                                           .FirstOrDefault(ti => ti.ASM_TaskId == asmTask.Id
                                                                                 && ti.AssembleStatus != AssembleStatus.Finished);
                            if (asmTaskItem == null)
                            {
                                asmTask.AssembleStatus = AssembleStatus.Finished;
                                asmAssembleIndication.AssembleStatus = asmTask.AssembleStatus;

                                #region 当前装配可以提交

                                List<ASM_AssembleIndicationItem> asmAssembleIndicatonItems = asmAssembleIndication.ASM_AssembleIndicationItems
                                                                                                 .ToList();

                                ASM_AssembleResult asmAssembleResult = new ASM_AssembleResult();
                                asmAssembleResult.ASM_AssembleIndicationId = asmAssembleIndication.Id;
                                asmAssembleResult.FactoryCode = asmAssembleIndication.FactoryCode;
                                asmAssembleResult.ProductionLineCode = asmAssembleIndication.ProductionLineCode;
                                asmAssembleResult.CFG_WorkStationId = asmAssembleIndication.CFG_WorkStationId;
                                asmAssembleResult.GzzList = asmAssembleIndication.GzzList;
                                asmAssembleResult.MONumber = asmAssembleIndication.MONumber;
                                asmAssembleResult.ProductSequence = asmAssembleIndication.ProductSequence;
                                asmAssembleResult.BeginTime = asmTask.ASM_TaskItems.OrderBy(ati => ati.AssembledTime).First().AssembledTime.Value;
                                asmAssembleResult.EndTime = asmTask.ASM_TaskItems.OrderBy(ati => ati.AssembledTime).Last().AssembledTime.Value;

                                dbContext.ASM_AssembleResults.Add(asmAssembleResult);

                                ASM_AssembleResultMessage asmAssembleResultMessage = new ASM_AssembleResultMessage();
                                asmAssembleResultMessage.ASM_AssembleResult = asmAssembleResult;
                                asmAssembleResultMessage.SentSuccessful = false;

                                dbContext.ASM_AssembleResultMessages.Add(asmAssembleResultMessage);

                                foreach (ASM_AssembleIndicationItem asmAssembleIndicationItem in asmAssembleIndicatonItems)
                                {
                                    List<ASM_TaskItem> asmTaskItems = asmAssembleIndicationItem.ASM_TaskItems
                                                                          .ToList();
                                    ASM_TaskItem lastAsmTaskItem = asmTaskItems.Last();

                                    ASM_AssembleResultItem asmAssembleResultItem = new ASM_AssembleResultItem();
                                    asmAssembleResultItem.ASM_AssembleResult = asmAssembleResult;
                                    asmAssembleResultItem.CFG_CartId = lastAsmTaskItem.CFG_CartId;
                                    asmAssembleResultItem.CartPosition = lastAsmTaskItem.CartPosition;
                                    asmAssembleResultItem.Gzz = asmAssembleIndicationItem.Gzz;
                                    asmAssembleResultItem.MaterialCode = asmAssembleIndicationItem.MaterialCode;
                                    asmAssembleResultItem.MaterialName = asmAssembleIndicationItem.MaterialName;
                                    asmAssembleResultItem.AssembleSequence = asmAssembleIndicationItem.AssembleSequence;
                                    asmAssembleResultItem.ToAssembleQuantity = asmAssembleIndicationItem.ToAssembleQuantity;
                                    asmAssembleResultItem.AssembledQuantity = asmTaskItems.Sum(ti => ti.AssembledQuantity.Value);
                                    asmAssembleResultItem.PickedTime = lastAsmTaskItem.AssembledTime.Value;
                                    asmAssembleResultItem.ProjectCode = asmAssembleIndicationItem.ProjectCode;
                                    asmAssembleResultItem.ProjectStep = asmAssembleIndicationItem.ProjectStep;

                                    dbContext.ASM_AssembleResultItems.Add(asmAssembleResultItem);
                                }

                                #endregion

                                dbContext.SaveChanges();

                                this.CurrentAsmTaskId = null;

                                Logger.Log(this.GetType().Name + "." + cfgWorkStation.Code, DateTime.Now.ToString("HH:mm:ss") + " 装配任务完成："
                                                                                            + asmAssembleIndication.ProductSequence + ", "
                                                                                            + asmAssembleIndication.GzzList + Environment.NewLine);
                            }
                            else
                            {
                                ASM_AssembleIndicationItem asmAssembleIndicationItem = asmTaskItem.ASM_AssembleIndicationItem;
                                CFG_Cart cfgCart = asmTaskItem.CFG_Cart;

                                //先更新数据库
                                asmTask.AssembleStatus = AssembleStatus.Assembling;
                                asmTaskItem.AssembleStatus = AssembleStatus.Assembling;
                                asmAssembleIndication.AssembleStatus = AssembleStatus.Assembling;
                                asmAssembleIndicationItem.AssembleStatus = AssembleStatus.Assembling;
                                cfgCart.CartStatus = CartStatus.Indicating;

                                dbContext.SaveChanges();

                                //再控制设备
                                CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(asmTaskItem.CFG_CartId);
                                Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();
                                Ptl900U ptl900U = cartPtl.GetPtl900UByPosition(asmTaskItem.CartPosition);
                                Ptl900U ptl900ULight = cartPtl.GetPtl900ULight();

                                Display900UItem publisherDisplay900UItem = new Display900UItem();
                                publisherDisplay900UItem.Name = asmAssembleIndicationItem.MaterialName;
                                publisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}，{1}
车号：{2}
量产工位：{3}", asmAssembleIndicationItem.ProjectCode, asmAssembleIndicationItem.ProjectStep, asmAssembleIndication.ProductSequence, asmAssembleIndicationItem.Gzz);
                                publisherDisplay900UItem.LongSubLocation = asmTaskItem.ToAssembleQuantity.ToString(CultureInfo.InvariantCulture);
                                publisherDisplay900UItem.Count = 0;

                                Display900UItem display900UItem = new Display900UItem();
                                display900UItem.Count = (ushort)asmTaskItem.ToAssembleQuantity;

                                LightMode lightMode = new LightMode();
                                lightMode.Color = LightColor.Green;
                                if (asmAssembleIndicationItem.Qtxbs == "1")
                                {
                                    lightMode.Color = LightColor.Magenta;
                                    lightMode.Ratio = LightOnOffRatio.RatioP1V1;
                                    lightMode.Period = LightOnOffPeriod.Period500;
                                }

                                //ptl900UPublisher.Pressed += this.ptl900UPublisher_Pressed;
                                ptl900UPublisher.Clear(true);
                                //ptl900UPublisher.Unlock();
                                ptl900UPublisher.Lock();
                                ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off, true);

                                ptl900U.Pressed += this.ptl900U_Pressed;
                                ptl900U.Unlock();
                                ptl900U.Display(display900UItem, lightMode, true);

                                ptl900ULight.Clear();
                                ptl900ULight.Display(new Display900UItem(), lightMode, false);

                                this.CurrentAsmTaskItemId = asmTaskItem.Id;

                                Logger.Log(this.GetType().Name + "." + cfgWorkStation.Code, DateTime.Now.ToString("HH:mm:ss") + " 点亮装配明细："
                                                                                            + asmAssembleIndication.ProductSequence + ", "
                                                                                            + asmAssembleIndication.GzzList + ", "
                                                                                            + asmAssembleIndicationItem.MaterialCode + ", "
                                                                                            + asmAssembleIndicationItem.MaterialName + ", "
                                                                                            + cfgCart.Name + ", "
                                                                                            + asmTaskItem.CartPosition + Environment.NewLine);

                                //如果新底盘抵达，则自动完成之前的
                                this.AutoPressByNewCarArrivedAsync(this.CurrentAsmTaskId.Value, this.CurrentAsmTaskItemId.Value, ptl900U, display900UItem);
                                //如果长时间无法交互，则自动交互
                                this.AutoPressByDeviceErrorAsync(this.CurrentAsmTaskItemId.Value, ptl900U, display900UItem);
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

                        Logger.Log("IndicatingExecutor.threadStart", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                                     + message + Environment.NewLine
                                                                     + Environment.NewLine);

                        Thread.Sleep(1000);
                    }
                }

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 如果新底盘抵达，则自动完成之前的。
        /// </summary>
        void AutoPressByNewCarArrivedAsync(long capturedCurrentAsmTaskId, long capturedCurrentAsmTaskItemId, Ptl900U ptl900U, Display900UItem display900UItem)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.CurrentThread.Name = "IndicatingExecutor.AutoPressByNewCarArrivedAsync_" + this.CFG_WorkStationId;

                while (true)
                {
                    if (this.CurrentAsmTaskItemId != capturedCurrentAsmTaskItemId)
                        break;

                    try
                    {
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            bool hasOtherTask = dbContext.ASM_Tasks
                                                    .Any(t => t.ASM_AssembleIndication.CFG_WorkStationId == this.CFG_WorkStationId
                                                              && t.AssembleStatus != AssembleStatus.Finished
                                                              && t.Id != capturedCurrentAsmTaskId);

                            if (hasOtherTask
                                && this.CurrentAsmTaskItemId == capturedCurrentAsmTaskItemId)
                            {
                                CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                                     .First(ws => ws.Id == this.CFG_WorkStationId);
                                ASM_TaskItem asmTaskItem = dbContext.ASM_TaskItems
                                                               .First(ti => ti.Id == capturedCurrentAsmTaskItemId);
                                ASM_AssembleIndicationItem asmAssembleIndicationItem = asmTaskItem.ASM_AssembleIndicationItem;
                                ASM_AssembleIndication asmAssembleIndication = asmAssembleIndicationItem.ASM_AssembleIndication;
                                CFG_Cart cfgCart = asmTaskItem.CFG_Cart;

                                Logger.Log(this.GetType().Name + "." + cfgWorkStation.Code, DateTime.Now.ToString("HH:mm:ss") + " 新底盘抵达，自动完成之前的："
                                                                                            + asmAssembleIndication.ProductSequence + ", "
                                                                                            + asmAssembleIndication.GzzList + ", "
                                                                                            + asmAssembleIndicationItem.MaterialCode + ", "
                                                                                            + asmAssembleIndicationItem.MaterialName + ", "
                                                                                            + cfgCart.Name + ", "
                                                                                            + asmTaskItem.CartPosition + Environment.NewLine);

                                ptl900U.Clear(true);

                                Ptl900UPressedEventArgs ptl900UPressedEventArgs = new Ptl900UPressedEventArgs();
                                ptl900UPressedEventArgs.ResultByItem.Add(display900UItem, display900UItem.Count);

                                this.ptl900U_Pressed(ptl900U, ptl900UPressedEventArgs, true);

                                break;
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

                        Logger.Log("IndicatingExecutor.AutoPressByNewCarArrivedAsync", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                                                       + message + Environment.NewLine
                                                                                       + Environment.NewLine);
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            });
        }

        /// <summary>
        /// 如果长时间无法交互，则自动交互。
        /// </summary>
        void AutoPressByDeviceErrorAsync(long capturedCurrentAsmTaskItemId, Ptl900U ptl900U, Display900UItem display900UItem)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                DateTime beginSleepTime = DateTime.Now;
                while (this.CurrentAsmTaskItemId == capturedCurrentAsmTaskItemId && ptl900U.InError != false && (DateTime.Now - beginSleepTime) < this.autoPressTimeout)
                    Thread.Sleep(1);

                if (this.CurrentAsmTaskItemId == capturedCurrentAsmTaskItemId
                    && ptl900U.InError != false)
                {
                    try
                    {
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                                 .First(ws => ws.Id == this.CFG_WorkStationId);
                            ASM_TaskItem asmTaskItem = dbContext.ASM_TaskItems
                                                           .First(ti => ti.Id == capturedCurrentAsmTaskItemId);
                            ASM_AssembleIndicationItem asmAssembleIndicationItem = asmTaskItem.ASM_AssembleIndicationItem;
                            ASM_AssembleIndication asmAssembleIndication = asmAssembleIndicationItem.ASM_AssembleIndication;
                            CFG_Cart cfgCart = asmTaskItem.CFG_Cart;

                            Logger.Log(this.GetType().Name + "." + cfgWorkStation.Code, DateTime.Now.ToString("HH:mm:ss") + " 长时间无法交互，自动交互："
                                                                                        + asmAssembleIndication.ProductSequence + ", "
                                                                                        + asmAssembleIndication.GzzList + ", "
                                                                                        + asmAssembleIndicationItem.MaterialCode + ", "
                                                                                        + asmAssembleIndicationItem.MaterialName + ", "
                                                                                        + cfgCart.Name + ", "
                                                                                        + asmTaskItem.CartPosition + Environment.NewLine);
                        }
                    }
                    catch { }

                    ptl900U.Clear(true);

                    Ptl900UPressedEventArgs ptl900UPressedEventArgs = new Ptl900UPressedEventArgs();
                    ptl900UPressedEventArgs.ResultByItem.Add(display900UItem, display900UItem.Count);

                    this.ptl900U_Pressed(ptl900U, ptl900UPressedEventArgs, true);
                }
            });
        }

        /// <summary>
        /// 按拣选标签表示拣选一个。
        /// </summary>
        void ptl900U_Pressed(object sender, Ptl900UPressedEventArgs e)
        {
            this.ptl900U_Pressed(sender, e, false);
        }

        void ptl900U_Pressed(object sender, Ptl900UPressedEventArgs e, bool pickAll)
        {
            while (true)
            {
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                             .First(ws => ws.Id == this.CFG_WorkStationId);
                        ASM_TaskItem asmTaskItem = dbContext.ASM_TaskItems
                                                       .First(ti => ti.Id == this.CurrentAsmTaskItemId.Value);
                        ASM_AssembleIndicationItem asmAssembleIndicationItem = asmTaskItem.ASM_AssembleIndicationItem;
                        ASM_AssembleIndication asmAssembleIndication = asmAssembleIndicationItem.ASM_AssembleIndication;
                        List<ASM_TaskItem> otherAsmTaskItems = asmAssembleIndicationItem.ASM_TaskItems
                                                                   .Where(ti => ti.Id != asmTaskItem.Id)
                                                                   .ToList();
                        CFG_Cart cfgCart = asmTaskItem.CFG_Cart;
                        List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                                    .OrderBy(ccm => ccm.Position)
                                                                                    .ToList();
                        CFG_CartCurrentMaterial cfgCartCurrentMaterial = cfgCartCurrentMaterials.First(ccm => ccm.Position == asmTaskItem.CartPosition);
                        CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                                   .FirstOrDefault(wscc => wscc.CFG_WorkStationId == this.CFG_WorkStationId && wscc.CFG_CartId == cfgCart.Id);

                        if (asmTaskItem.AssembledQuantity == null)
                            asmTaskItem.AssembledQuantity = 0;

                        int pickedCount = 1;
                        if (pickAll)
                            pickedCount = asmTaskItem.ToAssembleQuantity - asmTaskItem.AssembledQuantity.Value;

                        asmTaskItem.AssembledQuantity += pickedCount;
                        asmTaskItem.AssembledTime = DateTime.Now;

                        if (asmAssembleIndicationItem.AssembledQuantity == null)
                            asmAssembleIndicationItem.AssembledQuantity = 0;
                        asmAssembleIndicationItem.AssembledQuantity += pickedCount;
                        asmAssembleIndicationItem.AssembledTime = DateTime.Now;

                        bool currentItemFinished = asmTaskItem.AssembledQuantity == asmTaskItem.ToAssembleQuantity;
                        if (currentItemFinished)
                        {
                            asmTaskItem.AssembleStatus = AssembleStatus.Finished;
                            if (otherAsmTaskItems.All(ti => ti.AssembleStatus == AssembleStatus.Finished))
                                asmAssembleIndicationItem.AssembleStatus = AssembleStatus.Finished;
                        }

                        if (cfgCartCurrentMaterial.Quantity != null && cfgCartCurrentMaterial.Quantity > 0)
                        {
                            cfgCartCurrentMaterial.Quantity -= pickedCount;
                        }

                        //料车上物料消耗完则施放料车并通知 AGV 回收，再尝试补充一辆料车
                        if (cfgCartCurrentMaterials.All(ccm => ccm.Quantity == null || ccm.Quantity == 0))
                        {
                            cfgCart.CartStatus = CartStatus.Free;

                            foreach (CFG_CartCurrentMaterial innerCfgCartCurrentMaterial in cfgCartCurrentMaterials)
                            {
                                innerCfgCartCurrentMaterial.AST_CartTaskItemId = null;
                                innerCfgCartCurrentMaterial.ProjectCode = null;
                                innerCfgCartCurrentMaterial.WbsId = null;
                                innerCfgCartCurrentMaterial.ProjectStep = null;
                                innerCfgCartCurrentMaterial.CFG_WorkStationId = null;
                                innerCfgCartCurrentMaterial.BatchCode = null;
                                innerCfgCartCurrentMaterial.CFG_ChannelId = null;
                                innerCfgCartCurrentMaterial.CFG_PalletId = null;
                                innerCfgCartCurrentMaterial.BoxCode = null;
                                innerCfgCartCurrentMaterial.FromPalletPosition = null;
                                innerCfgCartCurrentMaterial.MaterialCode = null;
                                innerCfgCartCurrentMaterial.MaterialName = null;
                                innerCfgCartCurrentMaterial.MaterialBarcode = null;
                                innerCfgCartCurrentMaterial.Quantity = null;
                                innerCfgCartCurrentMaterial.AssortedTime = null;
                                innerCfgCartCurrentMaterial.CFG_EmployeeId = null;
                                if (innerCfgCartCurrentMaterial.Usability != CartPositionUsability.DisableByOffLineDevice)
                                    innerCfgCartCurrentMaterial.Usability = CartPositionUsability.Enable;
                            }

                            if (cfgWorkStationCurrentCart != null)
                            {
                                //通知 AGV 回收当前车，尝试空满切换下一辆车
                                DST_AgvSwitch dstAgvSwitch = dbContext.DST_AgvSwitchs.FirstOrDefault(t => t.isOpen);
                                if (dstAgvSwitch != null)
                                {
                                    int nCurPosition = cfgWorkStationCurrentCart.Position;
                                    string sWorkStationCode = cfgWorkStationCurrentCart.CFG_WorkStation.Code;
                                    if (nCurPosition <= 4) //内侧
                                    {
                                        //如果对应外侧没有正在执行的物料超市配送任务，才生成里侧的线边配送任务
                                        string sOutPosition = sWorkStationCode + "-" + (nCurPosition + 4);
                                        DST_DistributeTask outDistributeTask = dbContext.DST_DistributeTasks.FirstOrDefault(t => t.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute
                                            && t.endPosition.Equals(sOutPosition)
                                            && t.sendErrorCount < 5
                                            && t.arriveTime == null);
                                        if (outDistributeTask == null)
                                        {
                                            CFG_WorkStationCurrentCart cfgOutWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                                                          .FirstOrDefault(wscc => wscc.CFG_WorkStationId == this.CFG_WorkStationId && wscc.Position == nCurPosition + 4 && wscc.CFG_CartId != null);
                                            if (cfgOutWorkStationCurrentCart != null)
                                            {
                                                //生成料架转换任务
                                                List<DST_DistributeTask> distributeTasks = DistributingTaskGenerator.Instance.GenerateProductCartSwitchTask(cfgCart);
                                                foreach (DST_DistributeTask distributeTask in distributeTasks)
                                                {
                                                    dbContext.DST_DistributeTasks.Add(distributeTask);
                                                }
                                            }
                                            else
                                            {
                                                //生成线边自动清线配送任务
                                                List<DST_DistributeTask> distributeTasks = DistributingTaskGenerator.Instance.GenerateProductAreaAutoClearTask(sWorkStationCode, nCurPosition.ToString(), cfgCart.Code);
                                                foreach (DST_DistributeTask distributeTask in distributeTasks)
                                                {
                                                    dbContext.DST_DistributeTasks.Add(distributeTask);
                                                }
                                            }
                                        }
                                    }
                                    else //外侧
                                    {
                                        //如果对应里侧没有正在执行的空满转换任务，才生成外侧的线边配送任务
                                        string sInPosition = sWorkStationCode + "-" + (nCurPosition - 4);
                                        DST_DistributeTask inDistributeTask = dbContext.DST_DistributeTasks.FirstOrDefault(t => t.DistributeReqTypes == DistributeReqTypes.ProductCartSwitch
                                            && t.startPosition.Equals(sInPosition)
                                            && t.sendErrorCount < 5
                                            && t.arriveTime == null);
                                        if (inDistributeTask == null)
                                        {
                                            //生成线边配送任务
                                            string sTaskSendType = "自动";
                                            List<DST_DistributeTask> distributeTasks = DistributingTaskGenerator.Instance.GenerateProductAreaDistributeTask(sWorkStationCode, nCurPosition.ToString(), cfgCart.Code, sTaskSendType, true, dbContext);
                                            foreach (DST_DistributeTask distributeTask in distributeTasks)
                                            {
                                                dbContext.DST_DistributeTasks.Add(distributeTask);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //解除停靠
                                    cfgWorkStationCurrentCart.CFG_CartId = null;
                                    cfgWorkStationCurrentCart.DockedTime = null;
                                }
                            }

                            //尝试发起下一车的拉料任务
                            CFG_Cart nextCfgCart = dbContext.CFG_Carts
                                                       .FirstOrDefault(c => c.CartStatus == CartStatus.ArrivedAtBufferArea
                                                                            && c.CFG_CartCurrentMaterials.Any(ccm => ccm.CFG_WorkStationId == this.CFG_WorkStationId
                                                                                                                     && ccm.Quantity > 0));
                            if (nextCfgCart != null)
                            {
                                nextCfgCart.CartStatus = CartStatus.NeedToWorkStation;

                                CFG_CartCurrentMaterial firstCfgCartFirstCartCurrentMaterial = nextCfgCart.CFG_CartCurrentMaterials
                                                                                                   .First(ccm => ccm.Quantity > 0);

                                FND_Task fndTask = new FND_Task();
                                fndTask.ProjectCode = firstCfgCartFirstCartCurrentMaterial.ProjectCode;
                                fndTask.ProjectStep = firstCfgCartFirstCartCurrentMaterial.ProjectStep;
                                fndTask.BatchCode = firstCfgCartFirstCartCurrentMaterial.BatchCode;
                                fndTask.MaxNeedArrivedTime = DateTime.Now.AddHours(1);
                                fndTask.RequestTime = DateTime.Now;
                                fndTask.CFG_WorkStationId = firstCfgCartFirstCartCurrentMaterial.CFG_WorkStationId.Value;
                                fndTask.CFG_CartId = nextCfgCart.Id;
                                fndTask.LightColor = (byte)LightColor.Off;
                                fndTask.FindingStatus = FindingStatus.New;

                                dbContext.FND_Tasks.Add(fndTask);
                            }
                        }

                        dbContext.SaveChanges();

                        CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(asmTaskItem.CFG_CartId);
                        Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();
                        Ptl900U ptl900U = cartPtl.GetPtl900UByPosition(asmTaskItem.CartPosition);
                        Ptl900U ptl900ULight = cartPtl.GetPtl900ULight();

                        if (currentItemFinished)
                        {
                            Display900UItem cartPublisherDisplay900UItem = new Display900UItem();
                            cartPublisherDisplay900UItem.Name = "抵达工位 " + cfgWorkStation.Name;
                            cartPublisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
批次：{2}", cfgCartCurrentMaterial.ProjectCode, cfgCartCurrentMaterial.ProjectStep, cfgCartCurrentMaterial.BatchCode);
                            if (cfgWorkStationCurrentCart != null)
                                cartPublisherDisplay900UItem.Count = (ushort)cfgWorkStationCurrentCart.Position;
                            cartPublisherDisplay900UItem.Unit = "位";

                            ptl900UPublisher.Pressed -= this.ptl900UPublisher_Pressed;
                            ptl900UPublisher.Clear(true);
                            ptl900UPublisher.Lock();
                            ptl900UPublisher.Display(cartPublisherDisplay900UItem, LightColor.Off);

                            ptl900U.Pressed -= this.ptl900U_Pressed;

                            ptl900ULight.Clear();

                            this.CurrentAsmTaskItemId = null;
                        }
                        else
                        {
                            Display900UItem publisherDisplay900UItem = new Display900UItem();
                            publisherDisplay900UItem.Name = asmAssembleIndicationItem.MaterialName;
                            publisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}，{1}
车号：{2}
量产工位：{3}", asmAssembleIndicationItem.ProjectCode, asmAssembleIndicationItem.ProjectStep, asmAssembleIndication.ProductSequence, asmAssembleIndicationItem.Gzz);
                            publisherDisplay900UItem.LongSubLocation = asmTaskItem.ToAssembleQuantity.ToString(CultureInfo.InvariantCulture);
                            publisherDisplay900UItem.Count = (ushort)asmTaskItem.AssembledQuantity.Value;

                            Display900UItem display900UItem = new Display900UItem();
                            display900UItem.Count = (ushort)(asmTaskItem.ToAssembleQuantity - asmTaskItem.AssembledQuantity.Value);

                            LightMode lightMode = new LightMode();
                            lightMode.Color = LightColor.Green;
                            if (asmAssembleIndicationItem.Qtxbs == "1")
                            {
                                lightMode.Color = LightColor.Magenta;
                                lightMode.Ratio = LightOnOffRatio.RatioP1V1;
                                lightMode.Period = LightOnOffPeriod.Period500;
                            }

                            ptl900UPublisher.Clear(true);
                            //ptl900UPublisher.Unlock();
                            ptl900UPublisher.Lock();
                            ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off, true);

                            ptl900U.Display(display900UItem, lightMode, true);

                            //如果长时间无法交互，则自动交互
                            this.AutoPressByDeviceErrorAsync(this.CurrentAsmTaskItemId.Value, ptl900U, display900UItem);
                        }

                        string logMessage = (pickAll ? " 拣选所有：" : " 拣选一个：")
                                            + asmAssembleIndication.ProductSequence + ", "
                                            + asmAssembleIndication.GzzList + ", "
                                            + asmAssembleIndicationItem.MaterialCode + ", "
                                            + asmAssembleIndicationItem.MaterialName + ", "
                                            + cfgCart.Name + ", "
                                            + asmTaskItem.CartPosition;
                        if (currentItemFinished)
                            logMessage += ", 明细完成";
                        Logger.Log(this.GetType().Name + "." + cfgWorkStation.Code, DateTime.Now.ToString("HH:mm:ss") + logMessage + Environment.NewLine);
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

                    Logger.Log("IndicatingExecutor.ptl900U_Pressed", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                                     + message + Environment.NewLine
                                                                     + Environment.NewLine);

                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 按发布器表示短拣。
        /// </summary>
        void ptl900UPublisher_Pressed(object sender, Ptl900UPressedEventArgs e)
        {
            while (true)
            {
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                             .First(ws => ws.Id == this.CFG_WorkStationId);
                        ASM_TaskItem asmTaskItem = dbContext.ASM_TaskItems
                                                       .First(ti => ti.Id == this.CurrentAsmTaskItemId.Value);
                        ASM_AssembleIndicationItem asmAssembleIndicationItem = asmTaskItem.ASM_AssembleIndicationItem;
                        ASM_AssembleIndication asmAssembleIndication = asmAssembleIndicationItem.ASM_AssembleIndication;
                        List<ASM_TaskItem> otherAsmTaskItems = asmAssembleIndicationItem.ASM_TaskItems
                                                                   .Where(ti => ti.Id != asmTaskItem.Id)
                                                                   .ToList();
                        CFG_Cart cfgCart = asmTaskItem.CFG_Cart;

                        KeyValuePair<Display900UItem, ushort> display900UitemAndResult = e.ResultByItem.First();

                        asmTaskItem.AssembledQuantity = display900UitemAndResult.Value;
                        asmTaskItem.AssembledTime = DateTime.Now;
                        asmTaskItem.AssembleStatus = AssembleStatus.Finished;

                        if (asmAssembleIndicationItem.AssembledQuantity == null)
                            asmAssembleIndicationItem.AssembledQuantity = 0;
                        asmAssembleIndicationItem.AssembledQuantity += display900UitemAndResult.Value - display900UitemAndResult.Key.Count;
                        asmAssembleIndicationItem.AssembledTime = DateTime.Now;

                        if (otherAsmTaskItems.All(ti => ti.AssembleStatus == AssembleStatus.Finished))
                            asmAssembleIndicationItem.AssembleStatus = AssembleStatus.Finished;

                        dbContext.SaveChanges();

                        CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(asmTaskItem.CFG_CartId);
                        Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();
                        Ptl900U ptl900U = cartPtl.GetPtl900UByPosition(asmTaskItem.CartPosition);
                        Ptl900U ptl900ULight = cartPtl.GetPtl900ULight();

                        ptl900U.Pressed -= this.ptl900U_Pressed;
                        ptl900U.Clear(true);

                        ptl900UPublisher.Pressed -= this.ptl900UPublisher_Pressed;

                        ptl900ULight.Clear();

                        this.CurrentAsmTaskItemId = null;

                        Logger.Log(this.GetType().Name + "." + cfgWorkStation.Code, DateTime.Now.ToString("HH:mm:ss") + " 短拣："
                                                                                    + asmAssembleIndication.ProductSequence + ", "
                                                                                    + asmAssembleIndication.GzzList + ", "
                                                                                    + asmAssembleIndicationItem.MaterialCode + ", "
                                                                                    + asmAssembleIndicationItem.MaterialName + ", "
                                                                                    + cfgCart.Name + ", "
                                                                                    + asmTaskItem.CartPosition + Environment.NewLine);
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

                    Logger.Log("IndicatingExecutor.ptl900UPublisher_Pressed", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                                              + message + Environment.NewLine
                                                                              + Environment.NewLine);

                    Thread.Sleep(1000);
                }
            }
        }
    }
}