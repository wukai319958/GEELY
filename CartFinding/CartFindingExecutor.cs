using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using DeviceCommunicationHost;
using Ptl.Device;
using Ptl.Device.Communication.Command;

namespace CartFinding
{
    /// <summary>
    /// 使用灯色区分送料批次，轮询数据库状态即可。
    /// </summary>
    public class CartFindingExecutor
    {
        static readonly CartFindingExecutor instance = new CartFindingExecutor();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static CartFindingExecutor Instance
        {
            get { return CartFindingExecutor.instance; }
        }

        /// <summary>
        /// 搬运一段时间后关闭指示灯。
        /// </summary>
        readonly TimeSpan autoClearPeriod = TimeSpan.FromSeconds(10);
        /// <summary>
        /// 搬运一段时间后自动抵达。
        /// </summary>
        readonly TimeSpan autoReachedPeriod = TimeSpan.FromMinutes(1);

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(1);
        Thread thread;
        bool threadNeedQuit;

        readonly object refreshSyncRoot = new object();

        /// <summary>
        /// 获取是否运行中。
        /// </summary>
        public bool IsRunning { get; private set; }

        CartFindingExecutor()
        { }

        /// <summary>
        /// 启动定时加载线程。
        /// </summary>
        public void Start()
        {
            this.Restore();

            this.thread = new Thread(this.threadStart);
            this.thread.Name = this.GetType().FullName;
            this.thread.IsBackground = true;
            this.thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            this.thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

            this.threadNeedQuit = false;
            this.thread.Start();

            this.IsRunning = true;
        }

        /// <summary>
        /// 停止定时加载线程。
        /// </summary>
        public void Stop()
        {
            if (this.thread != null)
            {
                this.threadNeedQuit = true;
                this.thread.Join();

                this.thread = null;
            }

            this.IsRunning = false;
        }

        /// <summary>
        /// 还原系统启动前的状态。
        /// </summary>
        void Restore()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<FND_Task> ingFndTasks = dbContext.FND_Tasks
                                                 .Where(t => t.FindingStatus == FindingStatus.Displaying
                                                             || t.FindingStatus == FindingStatus.Blinking)
                                                 .ToList();
                foreach (FND_Task fndTask in ingFndTasks)
                {
                    if (fndTask.FindingStatus == FindingStatus.Displaying)
                        fndTask.FindingStatus = FindingStatus.NeedDisplay;
                    else if (fndTask.FindingStatus == FindingStatus.Blinking)
                        fndTask.FindingStatus = FindingStatus.NeedBlink;
                }

                dbContext.SaveChanges();

                List<FND_Task> unfinishedFndTasks = dbContext.FND_Tasks
                                                        .Where(t => t.FindingStatus != FindingStatus.Finished
                                                                    || t.CFG_Cart.CartStatus == CartStatus.InCarriageToWorkStation)
                                                        .ToList();
                foreach (FND_Task fndTask in unfinishedFndTasks)
                {
                    CFG_WorkStation cfgWorkStation = fndTask.CFG_WorkStation;
                    CFG_Cart cfgCart = fndTask.CFG_Cart;

                    string gzzList = this.GetGzzListFromCfgCartMaterial(cfgCart);

                    CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(fndTask.CFG_CartId);
                    Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();
                    Ptl900U ptl900ULight = cartPtl.GetPtl900ULight();

                    Display900UItem publisherDisplay900UItem = new Display900UItem();
                    publisherDisplay900UItem.Name = string.Format(CultureInfo.InvariantCulture, "{0}：{1}", cfgWorkStation.Name, gzzList);
                    publisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}，{1}
批次：{2}
最迟抵达：{3:HH:mm:ss}", fndTask.ProjectCode, fndTask.ProjectStep, fndTask.BatchCode, fndTask.MaxNeedArrivedTime);
                    publisherDisplay900UItem.Count = (ushort)cfgCart.CFG_CartCurrentMaterials
                                                                 .Where(ccm => ccm.Quantity != null)
                                                                 .Select(ccm => ccm.Quantity.Value)
                                                                 .Sum();
                    publisherDisplay900UItem.Unit = "个";

                    ptl900UPublisher.Clear(true);
                    ptl900UPublisher.Lock();
                    ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off);

                    if (fndTask.FindingStatus == FindingStatus.New)
                    {
                        ptl900ULight.Clear();
                        ptl900ULight.Display(new Display900UItem(), LightColor.Cyan);
                    }
                }
            }
        }

        void threadStart(object notUsed)
        {
            while (!this.threadNeedQuit)
            {
                try
                {
                    this.Refresh();
                }
                catch { }
                finally
                {
                    DateTime beginTime = DateTime.Now;

                    while (!this.threadNeedQuit && (DateTime.Now - beginTime) < this.threadPeriod)
                        Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// 显式或定时刷新。
        /// </summary>
        public void Refresh()
        {
            lock (this.refreshSyncRoot)
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    List<FND_Task> needToDoFndTasks = dbContext.FND_Tasks
                                                          .Where(t => t.FindingStatus == FindingStatus.NeedDisplay
                                                                      || t.FindingStatus == FindingStatus.NeedBlink
                                                                      || t.FindingStatus == FindingStatus.NeedClear)
                                                          .ToList();

                    //获取是否开启AGV配送PTL料架
                    DST_AgvSwitch dstAgvSwitch = dbContext.DST_AgvSwitchs.FirstOrDefault(t => t.isOpen);

                    foreach (FND_Task fndTask in needToDoFndTasks)
                    {
                        CFG_WorkStation cfgWorkStation = fndTask.CFG_WorkStation;
                        CFG_Cart cfgCart = fndTask.CFG_Cart;

                        string gzzList = this.GetGzzListFromCfgCartMaterial(cfgCart);

                        CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(fndTask.CFG_CartId);
                        Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();
                        Ptl900U ptl900ULight = cartPtl.GetPtl900ULight();

                        Display900UItem publisherDisplay900UItem = new Display900UItem();
                        publisherDisplay900UItem.Name = string.Format(CultureInfo.InvariantCulture, "{0}：{1}", cfgWorkStation.Name, gzzList);
                        publisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}，{1}
批次：{2}
最迟抵达：{3:HH:mm:ss}", fndTask.ProjectCode, fndTask.ProjectStep, fndTask.BatchCode, fndTask.MaxNeedArrivedTime);
                        publisherDisplay900UItem.Count = (ushort)cfgCart.CFG_CartCurrentMaterials
                                                                     .Where(ccm => ccm.Quantity != null)
                                                                     .Select(ccm => ccm.Quantity.Value)
                                                                     .Sum();
                        publisherDisplay900UItem.Unit = "个";

                        Display900UItem lightDisplay900UItem = new Display900UItem();

                        LightColor lightColor = (LightColor)fndTask.LightColor;

                        LightMode lightMode = new LightMode();
                        lightMode.Color = lightColor;
                        lightMode.Period = LightOnOffPeriod.Period200;
                        lightMode.Ratio = LightOnOffRatio.RatioP1V1;

                        if (fndTask.FindingStatus == FindingStatus.NeedDisplay)
                        {
                            //先保证数据库更新成功
                            fndTask.DisplayTime = DateTime.Now;
                            fndTask.FindingStatus = FindingStatus.Displaying;

                            cfgCart.CartStatus = CartStatus.WaitingToWorkStation;

                            dbContext.SaveChanges();

                            //再控制设备
                            ptl900UPublisher.Clear(true);
                            ptl900UPublisher.Lock();
                            ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off);

                            ptl900ULight.Clear();
                            ptl900ULight.Display(lightDisplay900UItem, lightColor);
                        }
                        else if (fndTask.FindingStatus == FindingStatus.NeedBlink)
                        {
                            if (fndTask.DisplayTime == null)
                            {
                                fndTask.CFG_EmployeeId = 1;
                                fndTask.DisplayTime = DateTime.Now;
                            }
                            fndTask.DepartedTime = DateTime.Now;
                            fndTask.FindingStatus = FindingStatus.Blinking;

                            if (dstAgvSwitch == null)
                            {
                                cfgCart.CartStatus = CartStatus.InCarriageToWorkStation;
                            }

                            //当前请求可以分小车提交
                            FND_DeliveryResult fndDeliveryResult = new FND_DeliveryResult();
                            fndDeliveryResult.FND_TaskId = fndTask.Id;
                            fndDeliveryResult.ProjectCode = fndTask.ProjectCode;
                            fndDeliveryResult.ProjectStep = fndTask.ProjectStep;
                            fndDeliveryResult.CFG_WorkStationId = fndTask.CFG_WorkStationId;
                            fndDeliveryResult.BatchCode = fndTask.BatchCode;
                            fndDeliveryResult.MaxNeedArrivedTime = fndTask.MaxNeedArrivedTime;
                            fndDeliveryResult.CFG_CartId = fndTask.CFG_CartId;
                            fndDeliveryResult.DepartedTime = DateTime.Now;
                            fndDeliveryResult.CFG_EmployeeId = fndTask.CFG_EmployeeId.Value;

                            dbContext.FND_DeliveryResults.Add(fndDeliveryResult);

                            FND_DeliveryResultMessage fndDeliveryResultMessage = new FND_DeliveryResultMessage();
                            fndDeliveryResultMessage.FND_DeliveryResult = fndDeliveryResult;
                            fndDeliveryResultMessage.SentSuccessful = false;

                            dbContext.FND_DeliveryResultMessages.Add(fndDeliveryResultMessage);

                            List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = fndTask.CFG_Cart.CFG_CartCurrentMaterials
                                                                                        .Where(ccm => ccm.Quantity > 0)
                                                                                        .ToList();
                            foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                            {
                                FND_DeliveryResultItem fndDeliveryResultItem = new FND_DeliveryResultItem();
                                fndDeliveryResultItem.FND_DeliveryResult = fndDeliveryResult;
                                fndDeliveryResultItem.CartPosition = cfgCartCurrentMaterial.Position;
                                fndDeliveryResultItem.MaterialCode = cfgCartCurrentMaterial.MaterialCode;
                                fndDeliveryResultItem.MaterialName = cfgCartCurrentMaterial.MaterialName;
                                fndDeliveryResultItem.MaterialBarcode = cfgCartCurrentMaterial.MaterialBarcode;
                                fndDeliveryResultItem.Quantity = cfgCartCurrentMaterial.Quantity.Value;

                                dbContext.FND_DeliveryResultItems.Add(fndDeliveryResultItem);
                            }

                            dbContext.SaveChanges();

                            ptl900ULight.Clear();
                            ptl900ULight.Display(lightDisplay900UItem, lightMode, false);
                        }
                        else if (fndTask.FindingStatus == FindingStatus.NeedClear)
                        {
                            fndTask.FindingStatus = FindingStatus.Finished;

                            dbContext.SaveChanges();

                            ptl900ULight.Clear();
                        }
                    }

                    //运输一段时间后自动进入 NeedClear 状态
                    DateTime maxBlinkingTime = DateTime.Now.Subtract(this.autoClearPeriod);
                    List<FND_Task> autoClearFndTasks = dbContext.FND_Tasks
                                                           .Where(t => t.FindingStatus == FindingStatus.Blinking
                                                                       && t.DisplayTime < maxBlinkingTime)
                                                           .ToList();

                    foreach (FND_Task fndTask in autoClearFndTasks)
                    {
                        fndTask.FindingStatus = FindingStatus.NeedClear;
                        dbContext.SaveChanges();
                    }

                    if (dstAgvSwitch == null)
                    {
                        //运输一段时间后自动抵达生产线
                        DateTime maxCarriageTime = DateTime.Now.Subtract(this.autoReachedPeriod);
                        List<FND_Task> autoReachedFndTasks = dbContext.FND_Tasks
                                                                 .Where(t => t.CFG_Cart.CartStatus == CartStatus.InCarriageToWorkStation
                                                                             && t.FindingStatus == FindingStatus.Finished
                                                                             && t.DepartedTime < maxCarriageTime)
                                                                 .ToList();

                        foreach (FND_Task fndTask in autoReachedFndTasks)
                        {
                            CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                                       .Where(wscc => wscc.CFG_WorkStationId == fndTask.CFG_WorkStationId
                                                                                                      && wscc.CFG_CartId == null)
                                                                                       .OrderBy(wscc => wscc.Position)
                                                                                       .FirstOrDefault();
                            if (cfgWorkStationCurrentCart != null)
                            {
                                CFG_Cart cfgCart = fndTask.CFG_Cart;

                                cfgWorkStationCurrentCart.CFG_CartId = cfgCart.Id;
                                cfgWorkStationCurrentCart.DockedTime = DateTime.Now;

                                cfgCart.CartStatus = CartStatus.ArrivedAtWorkStation;
                            }

                            dbContext.SaveChanges();
                        }
                    }
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