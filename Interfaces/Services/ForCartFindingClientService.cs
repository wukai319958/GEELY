using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Assorting;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.CartFinding;
using DataAccess.Config;
using DeviceCommunicationHost;
using Interfaces.Entities;
using Ptl.Device;

namespace Interfaces.Services
{
    /// <summary>
    /// 为手持机提供服务。
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class ForCartFindingClientService : IForCartFindingClientService
    {
        static readonly ForCartFindingClientService instance = new ForCartFindingClientService();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static ForCartFindingClientService Instance
        {
            get { return ForCartFindingClientService.instance; }
        }

        ForCartFindingClientService()
        { }

        /// <summary>
        /// 查询所有操作员的登录名。
        /// </summary>
        public List<string> QueryCfgEmployeeLoginNames()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                return dbContext.CFG_Employees
                           .Where(e => e.IsEnabled)
                           .Select(e => e.LoginName)
                           .ToList();
            }
        }

        /// <summary>
        /// 查询所有的巷道。
        /// </summary>
        public List<CFG_ChannelDto> QueryCfgChannels()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                return (from ws in dbContext.CFG_Channels
                        select new CFG_ChannelDto
                        {
                            Id = ws.Id,
                            Code = ws.Code,
                            Name = ws.Name
                        })
                           .ToList();
            }
        }

        /// <summary>
        /// 查询所有的工位。
        /// </summary>
        public List<CFG_WorkStationDto> QueryCfgWorkStations()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                return (from ws in dbContext.CFG_WorkStations
                        select new CFG_WorkStationDto
                        {
                            Id = ws.Id,
                            Code = ws.Code,
                            Name = ws.Name
                        })
                           .ToList();
            }
        }

        /// <summary>
        /// 按编码或 RFID 标签查询料车。
        /// </summary>
        /// <param name="cfgCartCodeOrRfid">料车编码或 RFID 标签。</param>
        public CFG_CartDto QueryCfgCart(string cfgCartCodeOrRfid)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                return (from c in dbContext.CFG_Carts
                        where c.Code == cfgCartCodeOrRfid || c.Rfid1 == cfgCartCodeOrRfid || c.Rfid2 == cfgCartCodeOrRfid
                        select new CFG_CartDto
                        {
                            Id = c.Id,
                            Code = c.Code,
                            Name = c.Name
                        })
                           .FirstOrDefault();
            }
        }

        /// <summary>
        /// 登录。
        /// </summary>
        /// <param name="loginName">用户名。</param>
        /// <param name="password">密码。</param>
        public LoginResult Login(string loginName, string password)
        {
            LoginResult loginResult = new LoginResult();

            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Employee cfgEmployee = dbContext.CFG_Employees
                                               .FirstOrDefault(e => e.IsEnabled && e.LoginName == loginName);

                if (cfgEmployee != null && cfgEmployee.Password == password)
                {
                    loginResult.Successful = true;
                    loginResult.CFG_EmployeeId = cfgEmployee.Id;
                }
            }

            return loginResult;
        }

        /// <summary>
        /// 查询料车配送任务。
        /// </summary>
        /// <param name="cfgWorkStationIds">按工位过滤的工位主键集合。</param>
        public List<FND_TaskDto> QueryFndTasks(List<int> cfgWorkStationIds)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<FND_Task> unfinishedFndTasks;
                if (cfgWorkStationIds == null || cfgWorkStationIds.Count == 0)
                {
                    unfinishedFndTasks = dbContext.FND_Tasks
                                             .Where(t => t.FindingStatus != FindingStatus.Finished)
                                             .OrderBy(t => t.BatchCode)
                                             .ThenBy(t => t.CFG_WorkStation.Code)
                                             .ToList();
                }
                else
                {
                    unfinishedFndTasks = dbContext.FND_Tasks
                                             .Where(t => t.FindingStatus != FindingStatus.Finished
                                                         && cfgWorkStationIds.Contains(t.CFG_WorkStationId))
                                             .OrderBy(t => t.BatchCode)
                                             .ThenBy(t => t.CFG_WorkStation.Code)
                                             .ToList();
                }

                DateTime recentlyTime = DateTime.Now.AddMinutes(-1);
                List<FND_Task> recentlyFinishedFndTasks = dbContext.FND_Tasks
                                                              .Where(t => t.FindingStatus == FindingStatus.Finished
                                                                          && t.RequestTime > recentlyTime)
                                                              .OrderBy(t => t.BatchCode)
                                                              .ThenBy(t => t.CFG_WorkStation.Code)
                                                              .Take(10)
                                                              .ToList();

                List<FND_Task> fndTasks = new List<FND_Task>();
                fndTasks.AddRange(unfinishedFndTasks);
                fndTasks.AddRange(recentlyFinishedFndTasks);
                fndTasks = fndTasks.Distinct().ToList();

                List<FND_TaskDto> fndTaskDtos = new List<FND_TaskDto>();
                foreach (FND_Task fndTask in fndTasks)
                {
                    FND_TaskDto fndTaskDto = new FND_TaskDto();
                    fndTaskDto.FND_TaskId = fndTask.Id;
                    fndTaskDto.ProjectCode = fndTask.ProjectCode;
                    fndTaskDto.ProjectStep = fndTask.ProjectStep;
                    fndTaskDto.WorkStationCode = fndTask.CFG_WorkStation.Code;
                    fndTaskDto.CartName = fndTask.CFG_Cart.Name;
                    fndTaskDto.BatchCode = fndTask.BatchCode;
                    fndTaskDto.MaxNeedArrivedTime = fndTask.MaxNeedArrivedTime;
                    fndTaskDto.LightColor = fndTask.LightColor;
                    fndTaskDto.FindingStatus = fndTask.FindingStatus;
                    fndTaskDto.DisplayTime = fndTask.DisplayTime;
                    fndTaskDto.DepartedTime = fndTask.DepartedTime;

                    fndTaskDtos.Add(fndTaskDto);
                }

                return fndTaskDtos;
            }
        }

        /// <summary>
        /// 点亮需配送的料车。
        /// </summary>
        /// <param name="fndTaskId">料车配送任务的主键。</param>
        /// <param name="lightColor">灯色。</param>
        /// <param name="cfgEmployeeId">操作员的主键。</param>
        public void FindCart(long fndTaskId, byte lightColor, int cfgEmployeeId)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                FND_Task fndTask = dbContext.FND_Tasks
                                       .First(t => t.Id == fndTaskId);

                fndTask.LightColor = lightColor;
                fndTask.FindingStatus = FindingStatus.NeedDisplay;
                fndTask.CFG_EmployeeId = cfgEmployeeId;

                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 发出需配送的料车。
        /// </summary>
        /// <param name="fndTaskId">料车配送任务的主键。</param>
        public void DepartCart(long fndTaskId)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                FND_Task fndTask = dbContext.FND_Tasks
                                       .First(t => t.Id == fndTaskId);

                fndTask.FindingStatus = FindingStatus.NeedBlink;

                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 停靠料车到巷道。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        /// <param name="cfgCartCodeOrRfid">料车编码或 RFID 标签。</param>
        public void DockCartToChannel(int cfgChannelId, string cfgCartCodeOrRfid)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Cart cfgCart = dbContext.CFG_Carts
                                       .FirstOrDefault(c => c.Code == cfgCartCodeOrRfid
                                                            || c.Rfid1 == cfgCartCodeOrRfid
                                                            || c.Rfid2 == cfgCartCodeOrRfid);

                if (cfgCart == null)
                    throw new ArgumentException("没有找到小车：" + cfgCartCodeOrRfid, "cfgCartCodeOrRfid");

                int? position = ChannelCurrentCartBinder.Instance.GetFirstEmptyPosition(cfgChannelId);
                if (position == null)
                    throw new ArgumentException("车位已满。", "cfgChannelId");

                ChannelCurrentCartBinder.Instance.Dock(cfgChannelId, position.Value, cfgCart.Id);
            }
        }

        /// <summary>
        /// 从巷道解除料车的停靠。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        /// <param name="cfgCartCodeOrRfid">料车编码或 RFID 标签。</param>
        public void UnDockCartFromChannel(int cfgChannelId, string cfgCartCodeOrRfid)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Cart cfgCart = dbContext.CFG_Carts
                                       .FirstOrDefault(c => c.Code == cfgCartCodeOrRfid
                                                            || c.Rfid1 == cfgCartCodeOrRfid
                                                            || c.Rfid2 == cfgCartCodeOrRfid);

                if (cfgCart == null)
                    throw new ArgumentException("没有找到小车：" + cfgCartCodeOrRfid, "cfgCartCodeOrRfid");

                ChannelCurrentCartBinder.Instance.UnDock(cfgChannelId, cfgCart.Id);
            }
        }

        /// <summary>
        /// 绑定料车的 RFID 标签。
        /// </summary>
        /// <param name="cfgCartCode">料车编码。</param>
        /// <param name="rfid1">RFID 标签一。</param>
        /// <param name="rfid2">RFID 标签二。</param>
        public void BindCartRfid(string cfgCartCode, string rfid1, string rfid2)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Cart cfgCart = dbContext.CFG_Carts
                                       .FirstOrDefault(c => c.Code == cfgCartCode);

                if (cfgCart == null)
                    throw new ArgumentException("没有找到小车：" + cfgCartCode, "cfgCartCode");

                cfgCart.Rfid1 = rfid1;
                cfgCart.Rfid2 = rfid2;

                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 查询当前任务。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        public AndroidPdaTaskInfo QueryCurrentTaskInfo(int cfgChannelId)
        {
            AssortingExecutor assortingExecutor = AssortingExecutorLoader.Instance.GetByChannelId(cfgChannelId);

            AndroidPdaTaskInfo result = new AndroidPdaTaskInfo();
            result.CFG_ChannelId = cfgChannelId;

            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                if (assortingExecutor.CurrentCfgCartId != null)
                {
                    CFG_Cart cfgCart = dbContext.CFG_Carts
                                           .First(c => c.Id == assortingExecutor.CurrentCfgCartId.Value);
                    result.CFG_CartId = cfgCart.Id;
                    result.CartName = cfgCart.Name;
                    result.CartOnLine = cfgCart.OnLine;
                }

                if (assortingExecutor.CurrentAstCartTaskItemId == null)
                {
                    if (assortingExecutor.CurrentAstPalletTaskItemId != null)
                    {
                        AST_PalletTaskItem currentAstPalletTaskItem = dbContext.AST_PalletTaskItems
                                                                          .First(pti => pti.Id == assortingExecutor.CurrentAstPalletTaskItemId.Value);
                        List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = dbContext.CFG_CartCurrentMaterials
                                                                                    .Where(ccm => ccm.CFG_CartId == assortingExecutor.CurrentCfgCartId.Value)
                                                                                    .ToList();
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

                                result.CartPositions.Add(cfgCartCurrentMaterial.Position);
                            }
                        }
                    }
                }
                else
                {
                    AST_CartTaskItem astCartTaskItem = dbContext.AST_CartTaskItems
                                                           .First(cti => cti.Id == assortingExecutor.CurrentAstCartTaskItemId.Value);
                    result.CartPositions.Add(astCartTaskItem.CartPosition);
                    result.PickedQuantity = astCartTaskItem.AssortedQuantity;
                    result.AssortingStatus = astCartTaskItem.AssortingStatus;
                }

                if (assortingExecutor.CurrentAstPalletTaskItemId != null)
                {
                    AST_PalletTaskItem astPalletTaskItem = dbContext.AST_PalletTaskItems
                                                               .First(pti => pti.Id == assortingExecutor.CurrentAstPalletTaskItemId.Value);
                    result.MaterialCode = astPalletTaskItem.MaterialCode;
                    result.MaterialName = astPalletTaskItem.MaterialName;
                    result.ToPickQuantity = astPalletTaskItem.ToPickQuantity;
                }
            }

            return result;
        }

        /// <summary>
        /// 尝试引发 900U 分拣事件。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        /// <param name="cartPosition">料车储位。</param>
        public void TryRaisePtl900UAssortingPressed(int cfgChannelId, int cartPosition)
        {
            AssortingExecutor assortingExecutor = AssortingExecutorLoader.Instance.GetByChannelId(cfgChannelId);
            assortingExecutor.TryRaisePtl900UPressed(cartPosition);
        }

        /// <summary>
        /// 尝试引发显示屏确认事件。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        public void TryRaisePtlPublisherAssortingPressed(int cfgChannelId)
        {
            AssortingExecutor assortingExecutor = AssortingExecutorLoader.Instance.GetByChannelId(cfgChannelId);
            assortingExecutor.TryRaisePtlPublisherPressed();
        }

        /// <summary>
        /// 测试用方法。
        /// </summary>
        public void TestMethod_ClearCart(string cfgCartCodeOrRfid)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Cart cfgCart = dbContext.CFG_Carts
                                       .First(c => c.Code == cfgCartCodeOrRfid || c.Rfid1 == cfgCartCodeOrRfid || c.Rfid2 == cfgCartCodeOrRfid);
                List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                            .ToList();
                List<CFG_ChannelCurrentCart> cfgChannelCurrentCarts = cfgCart.CFG_ChannelCurrentCarts
                                                                          .ToList();
                List<CFG_WorkStationCurrentCart> cfgWorkStationCurrentCarts = cfgCart.CFG_WorkStationCurrentCarts
                                                                                  .ToList();

                cfgCart.CartStatus = CartStatus.Free;

                foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                {
                    cfgCartCurrentMaterial.AST_CartTaskItemId = null;
                    cfgCartCurrentMaterial.ProjectCode = null;
                    cfgCartCurrentMaterial.WbsId = null;
                    cfgCartCurrentMaterial.ProjectStep = null;
                    cfgCartCurrentMaterial.CFG_WorkStationId = null;
                    cfgCartCurrentMaterial.BatchCode = null;
                    cfgCartCurrentMaterial.CFG_ChannelId = null;
                    cfgCartCurrentMaterial.CFG_PalletId = null;
                    cfgCartCurrentMaterial.BoxCode = null;
                    cfgCartCurrentMaterial.FromPalletPosition = null;
                    cfgCartCurrentMaterial.MaterialCode = null;
                    cfgCartCurrentMaterial.MaterialName = null;
                    cfgCartCurrentMaterial.MaterialBarcode = null;
                    cfgCartCurrentMaterial.Quantity = null;
                    cfgCartCurrentMaterial.AssortedTime = null;
                    cfgCartCurrentMaterial.CFG_EmployeeId = null;
                    if (cfgCartCurrentMaterial.Usability != CartPositionUsability.DisableByOffLineDevice)
                        cfgCartCurrentMaterial.Usability = CartPositionUsability.Enable;
                }

                foreach (CFG_ChannelCurrentCart cfgChannelCurrentCart in cfgChannelCurrentCarts)
                {
                    cfgChannelCurrentCart.CFG_CartId = null;
                    cfgChannelCurrentCart.DockedTime = null;
                }

                foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in cfgWorkStationCurrentCarts)
                {
                    cfgWorkStationCurrentCart.CFG_CartId = null;
                    cfgWorkStationCurrentCart.DockedTime = null;
                }

                dbContext.SaveChanges();

                //设备控制
                CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(cfgCart.Id);
                Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();
                Ptl900U ptl900ULight = cartPtl.GetPtl900ULight();

                ptl900UPublisher.Clear(true);
                ptl900UPublisher.Unlock();

                foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                {
                    Ptl900U ptl900U = cartPtl.GetPtl900UByPosition(cfgCartCurrentMaterial.Position);

                    ptl900U.Clear(true);
                    ptl900U.Unlock();
                }

                ptl900ULight.Clear();
            }
        }

        /// <summary>
        /// 控制小车停靠显示
        /// </summary>
        /// <param name="nCartID"></param>
        /// <param name="sName"></param>
        /// <param name="sDescription"></param>
        /// <param name="nCount"></param>
        /// <param name="sUnit"></param>
        /// <returns></returns>
        public string DockCart(int nCartID, string sName, string sDescription, int nCount, string sUnit)
        {
            return ChannelCurrentCartBinder.Instance.DockCart(nCartID, sName, sDescription, nCount, sUnit);
        }

        /// <summary>
        /// 解绑小车
        /// </summary>
        /// <param name="nCartID"></param>
        /// <returns></returns>
        public string UnDockCart(int nCartID)
        {
            return ChannelCurrentCartBinder.Instance.UnDockCart(nCartID);
        }
    }
}