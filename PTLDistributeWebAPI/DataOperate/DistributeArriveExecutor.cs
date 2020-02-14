using DataAccess;
using DataAccess.AssemblyIndicating;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using DataAccess.Other;
using Distributing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ptl.Device.Communication.Command;
using PTLDistributeWebAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace PTLDistributeWebAPI.DataOperate
{
    public class DistributeArriveExecutor : IDistributeArriveExecutor
    {
        /// <summary>
        /// 配送到达任务处理
        /// </summary>
        /// <param name="reqInfo">接收的数据</param>
        /// <returns></returns>
        public string DistributeArriveHandle(JObject reqInfo)
        {
            string result = "";
            string message = "";
            try
            {
                DST_DistributeArriveTaskDto distributeArriveTaskDto = JsonConvert.DeserializeObject<DST_DistributeArriveTaskDto>(JsonConvert.SerializeObject(reqInfo));
                if (distributeArriveTaskDto != null)
                {
                    SaveMarketZoneStatus(distributeArriveTaskDto);
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        string sMethod = distributeArriveTaskDto.method;
                        string sTaskCode = distributeArriveTaskDto.taskCode;
                        if (sMethod.Equals("StartService") || sMethod.Equals("OutFromBin"))
                        {
                            message = "";

                            if (sMethod.Equals("OutFromBin"))
                            {
                                DST_CartArriveDetailDto cartArriveDetailDto = distributeArriveTaskDto.data[0];
                                string sCurrentPosition = cartArriveDetailDto.currentPosition;
                                MarketZone marketZone = dbContext.MarketZones.FirstOrDefault(t => t.GroundId.Equals(sCurrentPosition));
                                if (marketZone != null)
                                {
                                    //解绑物料超市停靠
                                    marketZone.CFG_CartId = null;
                                    marketZone.DockedTime = null;

                                    //更新寻车任务
                                    FND_Task fndTask = dbContext.FND_Tasks.FirstOrDefault(t => t.CFG_WorkStation.Code.Equals(marketZone.AreaId) && t.CFG_CartId == marketZone.CFG_CartId && t.FindingStatus < FindingStatus.NeedBlink);
                                    if (fndTask != null)
                                    {
                                        if (fndTask.FindingStatus == FindingStatus.New)
                                        {
                                            fndTask.CFG_EmployeeId = 1;
                                            fndTask.DisplayTime = DateTime.Now;

                                            fndTask.CFG_Cart.CartStatus = CartStatus.WaitingToWorkStation;
                                        }
                                        fndTask.FindingStatus = FindingStatus.NeedBlink;
                                        fndTask.CFG_Cart.CartStatus = CartStatus.InCarriageToWorkStation;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //获取对应的配送任务信息
                            DST_DistributeTaskResult distributeTaskResult = dbContext.DST_DistributeTaskResults.FirstOrDefault(t => t.data.Equals(sTaskCode));
                            if (distributeTaskResult == null)
                            {
                                message = "AGV反馈的任务单" + sTaskCode + "没有找到对应的配送任务单";
                            }
                            else if (dbContext.DST_DistributeArriveTasks.Any(t => t.taskCode.Equals(sTaskCode) && t.method.Equals(sMethod)))
                            {
                                message = "配送到达反馈结果" + sTaskCode + "之前已下发，不能再重复下发";
                            }
                            else
                            {
                                DST_DistributeTask distributeTask = dbContext.DST_DistributeTasks.FirstOrDefault(t => t.reqCode.Equals(distributeTaskResult.reqCode));
                                DistributeReqTypes distributeReqType = distributeTask.DistributeReqTypes;
                                if (sMethod.Equals("TaskFinish10"))
                                {
                                    distributeReqType = DistributeReqTypes.ProductNullCartBack;
                                }

                                switch (distributeReqType)
                                {
                                    case DistributeReqTypes.PickAreaInit: //拣料区铺线
                                    case DistributeReqTypes.NullCartAreaDistribute: //空料架缓冲区配送
                                        {
                                            message = PickAreaDock(distributeArriveTaskDto, distributeTask);
                                            break;
                                        }
                                    case DistributeReqTypes.ProductAreaInit: //生产线边铺线
                                    case DistributeReqTypes.MaterialMarketDistribute: //物料超市配送
                                    case DistributeReqTypes.ProductOutToIn: //生产线边外侧到里侧
                                    case DistributeReqTypes.ProductInToOut: //生产线边里侧到外侧
                                        {
                                            message = ProductAreaDock(distributeArriveTaskDto, distributeTask);
                                            break;
                                        }
                                    case DistributeReqTypes.PickAreaDistribute: //拣料区配送
                                        {
                                            message = MaterialMarketDock(distributeArriveTaskDto, distributeTask);
                                            break;
                                        }
                                    case DistributeReqTypes.ProductCartSwitch: //生产线边料架转换
                                        {
                                            message = ProductCartSwitch(distributeArriveTaskDto, distributeTask);
                                            break;
                                        }
                                    case DistributeReqTypes.ProductNullCartBack: //生产线边空料架返回
                                        {
                                            message = NullCartAreaDock(distributeArriveTaskDto, distributeTask);
                                            break;
                                        }
                                    case DistributeReqTypes.PointToPointDistribute: //点对点配送
                                        {
                                            message = PointToPointDock(distributeArriveTaskDto, distributeTask);
                                            break;
                                        }
                                }
                            }
                        }
                        //返回处理结果
                        DST_DistributeArriveTaskResultDto distributeArriveTaskResultDto = GenerateDistributeArriveTaskResultDto(distributeArriveTaskDto.reqCode, message);
                        result = JsonConvert.SerializeObject(distributeArriveTaskResultDto);

                        //新增配送达到任务记录
                        DST_DistributeArriveTask distributeArriveTask = GenerateDistributeArriveTask(distributeArriveTaskDto);
                        dbContext.DST_DistributeArriveTasks.Add(distributeArriveTask);

                        //新增配送到达任务结果
                        DST_DistributeArriveTaskResult distributeArriveTaskResult = GenerateDistributeArriveTaskResult(distributeArriveTaskDto.reqCode, message, result);
                        dbContext.DST_DistributeArriveTaskResults.Add(distributeArriveTaskResult);

                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                message = null;
            }
            return result;
        }

        /// <summary>
        /// 停靠小车到分拣口
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        /// <returns></returns>
        private string PickAreaDock(DST_DistributeArriveTaskDto distributeArriveTaskDto, DST_DistributeTask distributeTask)
        {
            string sErrorMsg = "";
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ArrayList arrLog = new ArrayList();
                    string logMessage = "";

                    DST_CartArriveDetailDto cartArriveDetailDto = distributeArriveTaskDto.data[0];
                    string sCartCode = cartArriveDetailDto.podCode;
                    string sCurrentPosition = cartArriveDetailDto.currentPosition;

                    bool IsPTLPick = true; //是否PTL拣料
                    if (distributeTask.data.Equals("PDA"))
                    {
                        IsPTLPick = false;
                    }

                    string[] arrPosition = sCurrentPosition.Replace("H", "").Replace("P", ",").Split(',');
                    string sChannelCode = arrPosition[0]; //巷道
                    int nPosition = Convert.ToInt32(arrPosition[1]); //车位

                    CFG_Channel cfgChannel = dbContext.CFG_Channels.FirstOrDefault(t => t.Code.Equals(sChannelCode));
                    if (cfgChannel == null)
                    {
                        sErrorMsg = "巷道 " + sChannelCode + " 不存在";
                        return sErrorMsg;
                    }
                    CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(c => c.Code.Equals(sCartCode));
                    if (cfgCart == null)
                    {
                        sErrorMsg = "小车 " + sCartCode + " 不存在";
                        return sErrorMsg;
                    }

                    if (IsPTLPick)
                    {
                        if (cfgCart.CartStatus != CartStatus.Free)
                        {
                            sErrorMsg = "小车 " + cfgCart.Code + " 未释放：" + cfgCart.CartStatus;
                            return sErrorMsg;
                        }
                        if (!cfgCart.OnLine)
                        {
                            sErrorMsg = "小车 " + cfgCart.Code + " 不在线";
                            return sErrorMsg;
                        }

                        foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                        {
                            if (!cfgCartPtlDevice.OnLine)
                            {
                                sErrorMsg = "小车上的 " + cfgCartPtlDevice.DeviceAddress + " 号标签不在线";
                                return sErrorMsg;
                            }
                        }
                    }

                    int cfgChannelId = cfgChannel.Id;
                    int cfgCartId = cfgCart.Id;

                    CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                       .FirstOrDefault(ccc => ccc.CFG_ChannelId == cfgChannelId && ccc.Position == nPosition);
                    if (cfgChannelCurrentCart == null)
                    {
                        sErrorMsg = "车位 " + nPosition + " 不存在";
                        return sErrorMsg;
                    }

                    if (cfgChannelCurrentCart.CFG_CartId != null && cfgChannelCurrentCart.CFG_CartId != cfgCartId)
                    {
                        sErrorMsg = "车位 " + nPosition + " 上的小车还未解除绑定";
                        return sErrorMsg;
                    }

                    if (cfgChannelCurrentCart.CFG_CartId == null)
                    {
                        CFG_ChannelCurrentCart dockedCfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                            .FirstOrDefault(ccc => ccc.CFG_CartId == cfgCartId);
                        if (dockedCfgChannelCurrentCart != null)
                        {
                            sErrorMsg = "小车 " + dockedCfgChannelCurrentCart.CFG_Cart.Code + " 已停靠在 " + dockedCfgChannelCurrentCart.CFG_Channel.Name + " 车位 " + dockedCfgChannelCurrentCart.Position;
                            return sErrorMsg;
                        }
                    }

                    //停靠即开始播种
                    cfgCart.CartStatus = CartStatus.WaitingAssorting;

                    cfgChannelCurrentCart.CFG_CartId = cfgCart.Id;
                    cfgChannelCurrentCart.DockedTime = distributeTask.reqTime;

                    logMessage = "料架" + sCartCode + "停靠在" + sCurrentPosition + "，配送方式：" + distributeTask.podDir;
                    arrLog.Add(logMessage);

                    //if (IsPTLPick)
                    //{
                    //    cfgChannelCurrentCart.DockedTime = distributeTask.reqTime;
                    //}
                    //else
                    //{
                    //    cfgChannelCurrentCart.DockedTime = DateTime.Now;
                    //}

                    if (IsPTLPick)
                    {
                        //清空小车上的物料
                        List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials.ToList();
                        if (cfgCartCurrentMaterials.Count > 0)
                        {
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

                            logMessage = "料架" + sCartCode + "清空小车物料明细";
                            arrLog.Add(logMessage);
                        }
                    }

                    ////新增配送达到任务记录
                    //DST_DistributeArriveTask distributeArriveTask = GenerateDistributeArriveTask(distributeArriveTaskDto);
                    //dbContext.DST_DistributeArriveTasks.Add(distributeArriveTask);

                    ////新增配送任务到达结果明细
                    //List<DST_DistributeArriveResult> distributeArriveResults = GenerateDistributeArriveResult(dbContext, distributeArriveTaskDto, distributeTask);
                    //foreach (DST_DistributeArriveResult distributeArriveResult in distributeArriveResults)
                    //{
                    //    dbContext.DST_DistributeArriveResults.Add(distributeArriveResult);
                    //}

                    distributeTask.arriveTime = distributeArriveTaskDto.reqTime;

                    bool isSuccess = dbContext.SaveChanges() > 0 ? true : false;

                    if (!isSuccess)
                    {
                        sErrorMsg = "数据保存失败";
                        return sErrorMsg;
                    }

                    //设备控制
                    //CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(cfgCart.Id);
                    //Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();

                    //Display900UItem publisherDisplay900UItem = new Display900UItem();
                    //publisherDisplay900UItem.Name = "停靠成功";
                    //publisherDisplay900UItem.Description = cfgChannel.Name;
                    //publisherDisplay900UItem.Count = (ushort)nPosition;
                    //publisherDisplay900UItem.Unit = "位";

                    //ptl900UPublisher.Clear(true);
                    //ptl900UPublisher.Lock();
                    //ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off);

                    if (IsPTLPick)
                    {
                        CartBinder.DockCart(cfgCart.Id, "停靠成功", cfgChannel.Name, nPosition, "位");
                    }

                    foreach (string sLog in arrLog)
                    {
                        Logger.Log("拣料区-巷道" + cfgChannelId, DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
            }
            return sErrorMsg;
        }

        /// <summary>
        /// 停靠小车到物料超市
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        /// <returns></returns>
        private string MaterialMarketDock(DST_DistributeArriveTaskDto distributeArriveTaskDto, DST_DistributeTask distributeTask)
        {
            string sErrorMsg = "";
            try
            {
                string sWorkStationCode = distributeTask.endPosition;

                bool IsPTLPick = true; //是否PTL拣料
                if (distributeTask.data.Equals("PDA"))
                {
                    IsPTLPick = false;
                }

                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ArrayList arrLog = new ArrayList();
                    string logMessage = "";

                    DST_CartArriveDetailDto cartArriveDetailDto = distributeArriveTaskDto.data[0];
                    string sCartCode = cartArriveDetailDto.podCode;
                    string sCurrentPosition = cartArriveDetailDto.currentPosition;

                    CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(c => c.Code.Equals(sCartCode));
                    if (cfgCart == null)
                    {
                        sErrorMsg = "小车 " + sCartCode + " 不存在";
                        return sErrorMsg;
                    }

                    //if (!cfgCart.OnLine)
                    //{
                    //    sErrorMsg = "小车 " + cfgCart.Code + " 不在线";
                    //    return sErrorMsg;
                    //}

                    //foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                    //{
                    //    if (!cfgCartPtlDevice.OnLine)
                    //    {
                    //        sErrorMsg = "小车上的 " + cfgCartPtlDevice.DeviceAddress + " 号标签不在线";
                    //        return sErrorMsg;
                    //    }
                    //}

                    logMessage = "料架" + sCartCode + "抵达" + sWorkStationCode + "，配送方式：" + distributeTask.podDir;
                    arrLog.Add(logMessage);

                    if (!IsPTLPick)
                    {
                        //已抵达缓存区
                        cfgCart.CartStatus = CartStatus.ArrivedAtBufferArea;

                        ////绑定物料超市料架
                        //CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations.FirstOrDefault(t => t.Code.Equals(sWorkStationCode));
                        //if (cfgWorkStation != null)
                        //{
                        //    CFG_MarketWorkStationCurrentCart cfgMarketWorkStationCurrentCarts = new CFG_MarketWorkStationCurrentCart();
                        //    cfgMarketWorkStationCurrentCarts.CFG_CartId = cfgCart.Id;
                        //    cfgMarketWorkStationCurrentCarts.CFG_WorkStationId = cfgWorkStation.Id;
                        //    cfgMarketWorkStationCurrentCarts.DockedTime = DateTime.Now;

                        //    dbContext.CFG_MarketWorkStationCurrentCarts.Add(cfgMarketWorkStationCurrentCarts);

                        //    logMessage = "工位" + sWorkStationCode + "新增料架" + sCartCode + "的物料超市绑定关系";
                        //    arrLog.Add(logMessage);
                        //}
                    }

                    if (IsPTLPick)
                    {
                        if (cfgCart.CartStatus < CartStatus.ArrivedAtBufferArea)
                        {
                            cfgCart.CartStatus = CartStatus.ArrivedAtBufferArea;

                            logMessage = "料架" + sCartCode + "更新料架状态为" + CartStatus.ArrivedAtBufferArea;
                            arrLog.Add(logMessage);
                        }
                        FND_Task fndTask = dbContext.FND_Tasks.FirstOrDefault(t => t.CFG_CartId == cfgCart.Id && t.FindingStatus == DataAccess.CartFinding.FindingStatus.New);
                        if (fndTask != null)
                        {
                            fndTask.FindingStatus = DataAccess.CartFinding.FindingStatus.NeedDisplay;
                            fndTask.CFG_EmployeeId = 1;

                            logMessage = "料架" + sCartCode + "更新寻车任务状态为" + DataAccess.CartFinding.FindingStatus.NeedDisplay;
                            arrLog.Add(logMessage);
                        }
                    }

                    //物料超市停靠
                    MarketZone marketZone = dbContext.MarketZones.FirstOrDefault(t => t.GroundId.Equals(sCurrentPosition));
                    if (marketZone != null)
                    {
                        marketZone.CFG_CartId = cfgCart.Id;
                        marketZone.DockedTime = DateTime.Now;
                    }

                    //CFG_CartCurrentMaterial firstNotEmptyCfgCartCurrentMaterial = cfgCart.CFG_CartCurrentMaterials
                    //                                                                 .FirstOrDefault(ccm => ccm.AST_CartTaskItemId != null);
                    //if (firstNotEmptyCfgCartCurrentMaterial != null)
                    //{
                    //    int nWorkStationId = Convert.ToInt32(firstNotEmptyCfgCartCurrentMaterial.CFG_WorkStationId);

                    //    //尝试发起补充到生产线的搬运任务，需判断有没有在途的
                    //    int workStationCurrenCartCount = dbContext.CFG_WorkStationCurrentCarts
                    //                                         .Count(wscc => wscc.CFG_WorkStationId == nWorkStationId
                    //                                                        && wscc.CFG_CartId != null)
                    //                                     + dbContext.FND_Tasks
                    //                                           .Count(t => t.CFG_WorkStationId == nWorkStationId
                    //                                                       && (t.CFG_Cart.CartStatus == CartStatus.NeedToWorkStation
                    //                                                           || t.CFG_Cart.CartStatus == CartStatus.WaitingToWorkStation
                    //                                                           || t.CFG_Cart.CartStatus == CartStatus.InCarriageToWorkStation));
                    //    if (workStationCurrenCartCount < 8) //8：生产线边有 8 个车位
                    //    {
                    //        CFG_WorkStationCurrentCart firstEmptyCfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                    //                                                                             .FirstOrDefault(wscc => wscc.CFG_WorkStationId == nWorkStationId
                    //                                                                                                     && wscc.CFG_CartId == null);
                    //        if (firstEmptyCfgWorkStationCurrentCart != null)
                    //        {
                    //            cfgCart.CartStatus = CartStatus.NeedToWorkStation;

                    //            CFG_CartCurrentMaterial firstCfgCartFirstCartCurrentMaterial = cfgCart.CFG_CartCurrentMaterials
                    //                                                                               .First(ccm => ccm.Quantity > 0);

                    //            FND_Task fndTask = new FND_Task();
                    //            fndTask.ProjectCode = firstCfgCartFirstCartCurrentMaterial.ProjectCode;
                    //            fndTask.ProjectStep = firstCfgCartFirstCartCurrentMaterial.ProjectStep;
                    //            fndTask.BatchCode = firstCfgCartFirstCartCurrentMaterial.BatchCode;
                    //            fndTask.MaxNeedArrivedTime = DateTime.Now.AddHours(1);
                    //            fndTask.RequestTime = DateTime.Now;
                    //            fndTask.CFG_WorkStationId = firstCfgCartFirstCartCurrentMaterial.CFG_WorkStationId.Value;
                    //            fndTask.CFG_CartId = cfgCart.Id;
                    //            fndTask.LightColor = (byte)LightColor.Off;
                    //            fndTask.FindingStatus = DataAccess.CartFinding.FindingStatus.New;
                    //            fndTask.CFG_EmployeeId = 1;

                    //            dbContext.FND_Tasks.Add(fndTask);
                    //        }
                    //    }
                    //}

                    ////新增配送达到任务记录
                    //DST_DistributeArriveTask distributeArriveTask = GenerateDistributeArriveTask(distributeArriveTaskDto);
                    //dbContext.DST_DistributeArriveTasks.Add(distributeArriveTask);

                    ////新增配送任务到达结果明细
                    //List<DST_DistributeArriveResult> distributeArriveResults = GenerateDistributeArriveResult(dbContext, distributeArriveTaskDto, distributeTask);
                    //foreach (DST_DistributeArriveResult distributeArriveResult in distributeArriveResults)
                    //{
                    //    dbContext.DST_DistributeArriveResults.Add(distributeArriveResult);
                    //}

                    distributeTask.arriveTime = distributeArriveTaskDto.reqTime;

                    bool isSuccess = dbContext.SaveChanges() > 0 ? true : false;

                    if (!isSuccess)
                    {
                        sErrorMsg = "数据保存失败";
                        return sErrorMsg;
                    }

                    foreach (string sLog in arrLog)
                    {
                        Logger.Log("物料超市-" + sWorkStationCode, DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                    }
                }

                try
                {
                    //自动批量生成物料超市配送任务
                    DistributingTaskGenerator.Instance.GenerateAutoBatchMarketDistributeTask(sWorkStationCode, IsPTLPick);
                }
                catch { }
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
            }
            return sErrorMsg;
        }

        /// <summary>
        /// 停靠小车到线边
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        /// <returns></returns>
        private string ProductAreaDock(DST_DistributeArriveTaskDto distributeArriveTaskDto, DST_DistributeTask distributeTask)
        {
            string sErrorMsg = "";
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ArrayList arrLog = new ArrayList();
                    string logMessage = "";

                    DST_CartArriveDetailDto cartArriveDetailDto = distributeArriveTaskDto.data[0];
                    string sCartCode = cartArriveDetailDto.podCode;
                    string sCurrentPosition = cartArriveDetailDto.currentPosition;

                    bool IsPTLPick = true; //是否PTL拣料
                    if (distributeTask.data.Equals("PDA"))
                    {
                        IsPTLPick = false;
                    }

                    string[] arrPosition = sCurrentPosition.Split('-'); //ZT01-1
                    string sWorkStationCode = arrPosition[0];  //工位
                    int nPosition = Convert.ToInt32(arrPosition[1]); //车位

                    CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations.FirstOrDefault(t => t.Code.Equals(sWorkStationCode));
                    if (cfgWorkStation == null)
                    {
                        sErrorMsg = "工位 " + sWorkStationCode + " 不存在";
                        return sErrorMsg;
                    }
                    CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(c => c.Code.Equals(sCartCode));
                    if (cfgCart == null)
                    {
                        sErrorMsg = "小车 " + sCartCode + " 不存在";
                        return sErrorMsg;
                    }

                    //if (!cfgCart.OnLine)
                    //{
                    //    sErrorMsg = "小车 " + cfgCart.Code + " 不在线";
                    //    return sErrorMsg;
                    //}

                    //foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                    //{
                    //    if (!cfgCartPtlDevice.OnLine)
                    //    {
                    //        sErrorMsg = "小车上的 " + cfgCartPtlDevice.DeviceAddress + " 号标签不在线";
                    //        return sErrorMsg;
                    //    }
                    //}

                    int cfgCartId = cfgCart.Id;

                    CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                       .FirstOrDefault(ccc => ccc.CFG_WorkStation.Code.Equals(sWorkStationCode) && ccc.Position == nPosition);
                    if (cfgWorkStationCurrentCart == null)
                    {
                        sErrorMsg = "车位 " + nPosition + " 不存在";
                        return sErrorMsg;
                    }

                    if (cfgWorkStationCurrentCart.CFG_CartId != null && cfgWorkStationCurrentCart.CFG_CartId != cfgCartId)
                    {
                        sErrorMsg = "车位 " + nPosition + " 上的小车还未解除绑定";
                        return sErrorMsg;
                    }

                    if (cfgWorkStationCurrentCart.CFG_CartId == null)
                    {
                        CFG_WorkStationCurrentCart dockedCfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                            .FirstOrDefault(ccc => ccc.CFG_CartId == cfgCartId);
                        if (dockedCfgWorkStationCurrentCart != null)
                        {
                            //sErrorMsg = "小车 " + dockedCfgWorkStationCurrentCart.CFG_Cart.Code + " 已停靠在 线边工位" + dockedCfgWorkStationCurrentCart.CFG_WorkStation.Code + " 车位 " + dockedCfgWorkStationCurrentCart.Position;
                            //return sErrorMsg;

                            dockedCfgWorkStationCurrentCart.CFG_CartId = null;
                            dockedCfgWorkStationCurrentCart.DockedTime = null;
                        }
                    }

                    if (IsPTLPick)
                    {
                        if (cfgCart.CartStatus < CartStatus.ArrivedAtWorkStation)
                        {
                            CFG_CartCurrentMaterial firstCfgCartCurrentMaterial = dbContext.CFG_CartCurrentMaterials.FirstOrDefault(t => t.CFG_CartId == cfgCartId && t.BatchCode != null);
                            if (firstCfgCartCurrentMaterial != null)
                            {
                                FND_Task fndTaskEx = dbContext.FND_Tasks.FirstOrDefault(t => t.CFG_CartId == cfgCartId && t.BatchCode.Equals(firstCfgCartCurrentMaterial.BatchCode));
                                if (fndTaskEx == null)
                                {
                                    FND_Task fndTask = new FND_Task();
                                    fndTask.ProjectCode = firstCfgCartCurrentMaterial.ProjectCode;
                                    fndTask.ProjectStep = firstCfgCartCurrentMaterial.ProjectStep;
                                    fndTask.BatchCode = firstCfgCartCurrentMaterial.BatchCode;
                                    fndTask.MaxNeedArrivedTime = DateTime.Now.AddHours(1);
                                    fndTask.RequestTime = DateTime.Now;
                                    fndTask.CFG_WorkStationId = cfgWorkStation.Id;
                                    fndTask.CFG_CartId = cfgCartId;
                                    fndTask.LightColor = (byte)LightColor.Off;
                                    fndTask.FindingStatus = FindingStatus.NeedBlink;
                                    fndTask.CFG_EmployeeId = 1;
                                    fndTask.DisplayTime = DateTime.Now;

                                    dbContext.FND_Tasks.Add(fndTask);

                                    logMessage = "料架" + cfgCart.Code + "生成寻车任务，状态：" + FindingStatus.NeedBlink + "，批次：" + firstCfgCartCurrentMaterial.BatchCode;
                                    arrLog.Add(logMessage);
                                }
                                else
                                {
                                    FND_Task fndTask = dbContext.FND_Tasks.FirstOrDefault(t => t.CFG_CartId == cfgCart.Id && t.FindingStatus < FindingStatus.NeedBlink);
                                    if (fndTask != null)
                                    {
                                        fndTask.FindingStatus = FindingStatus.NeedBlink;

                                        logMessage = "料架" + cfgCart.Code + "更新寻车任务的状态为" + FindingStatus.NeedBlink + "，批次：" + firstCfgCartCurrentMaterial.BatchCode;
                                        arrLog.Add(logMessage);
                                    }
                                }
                            }
                        }
                    }

                    //已抵达生产线边
                    cfgCart.CartStatus = CartStatus.ArrivedAtWorkStation;

                    //绑定当前工位上的小车
                    cfgWorkStationCurrentCart.CFG_CartId = cfgCart.Id;
                    cfgWorkStationCurrentCart.DockedTime = DateTime.Now;

                    logMessage = "料架" + sCartCode + "抵达工位" + sWorkStationCode + "，位置" + nPosition;
                    arrLog.Add(logMessage);

                    //if (!IsPTLPick)
                    //{
                    //    //移除物料超市料架
                    //    List<CFG_MarketWorkStationCurrentCart> cfgMarketWorkStationCurrentCarts = dbContext.CFG_MarketWorkStationCurrentCarts.Where(t => t.CFG_WorkStationId == cfgWorkStationCurrentCart.CFG_WorkStationId && t.CFG_CartId == cfgCartId).ToList();
                    //    if (cfgMarketWorkStationCurrentCarts.Count > 0)
                    //    {
                    //        dbContext.CFG_MarketWorkStationCurrentCarts.RemoveRange(cfgMarketWorkStationCurrentCarts);

                    //        logMessage = "工位" + sWorkStationCode + "删除料架" + sCartCode + "的物料超市绑定关系";
                    //        arrLog.Add(logMessage);
                    //    }
                    //}

                    ////新增配送达到任务记录
                    //DST_DistributeArriveTask distributeArriveTask = GenerateDistributeArriveTask(distributeArriveTaskDto);
                    //dbContext.DST_DistributeArriveTasks.Add(distributeArriveTask);

                    ////新增配送任务到达结果明细
                    //List<DST_DistributeArriveResult> distributeArriveResults = GenerateDistributeArriveResult(dbContext, distributeArriveTaskDto, distributeTask);
                    //foreach (DST_DistributeArriveResult distributeArriveResult in distributeArriveResults)
                    //{
                    //    dbContext.DST_DistributeArriveResults.Add(distributeArriveResult);
                    //}

                    //if (distributeTask.DistributeReqTypes == DistributeReqTypes.ProductAreaInit || (distributeTask.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute && !distributeTask.data.Equals("PTL-Auto")))
                    if ((distributeTask.DistributeReqTypes == DistributeReqTypes.ProductAreaInit || distributeTask.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute) && nPosition > 4) //物料超市配送
                    {
                        int nInPosition = nPosition - 4;
                        CFG_WorkStationCurrentCart cfgInWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                       .FirstOrDefault(ccc => ccc.CFG_WorkStation.Code.Equals(sWorkStationCode) && ccc.Position == nInPosition && ccc.CFG_CartId != null);
                        if (cfgInWorkStationCurrentCart == null) //如果对应里侧没有料架，则生成线边外侧到里侧的配送任务
                        {
                            //生成线边外侧到里侧的配送任务
                            List<DST_DistributeTask> OutToInDistributeTasks = DistributingTaskGenerator.Instance.GenerateProductOutToInTask(sWorkStationCode, nPosition.ToString(), sCartCode, IsPTLPick, distributeTask.podDir);
                            if (OutToInDistributeTasks.Count > 0)
                            {
                                foreach (DST_DistributeTask OutToInDistributeTask in OutToInDistributeTasks)
                                {
                                    dbContext.DST_DistributeTasks.Add(OutToInDistributeTask);
                                }

                                ////解绑当前工位上的小车
                                //cfgWorkStationCurrentCart.CFG_CartId = null;
                                //cfgWorkStationCurrentCart.DockedTime = null;

                                logMessage = "工位" + sWorkStationCode + "生成从线边外侧到里侧的配送任务，料架：" + sCartCode + "，位置：" + nPosition + "->" + nInPosition + "，配送方式：" + distributeTask.podDir;
                                arrLog.Add(logMessage);
                            }
                        }
                        else //如果对应里侧有料架，且里侧料架为空闲，则生成里侧料架的空满转换任务
                        {
                            CFG_Cart inCfgCart = cfgInWorkStationCurrentCart.CFG_Cart;
                            if (inCfgCart != null)
                            {
                                if (inCfgCart.CartStatus == CartStatus.Free)
                                {
                                    //生成空满料架转换任务
                                    List<DST_DistributeTask> inDistributeTasks = DistributingTaskGenerator.Instance.GenerateProductCartSwitchTask(inCfgCart);
                                    if (inDistributeTasks.Count > 0)
                                    {
                                        foreach (DST_DistributeTask inDistributeTask in inDistributeTasks)
                                        {
                                            dbContext.DST_DistributeTasks.Add(inDistributeTask);
                                        }

                                        logMessage = "工位" + sWorkStationCode + "生成空满料架转换任务，空料架：" + inCfgCart.Code + "，位置：" + nInPosition + "，配送方式：" + distributeTask.podDir;
                                        arrLog.Add(logMessage);
                                    }
                                }
                            }
                        }
                    }
                    else if (distributeTask.DistributeReqTypes == DistributeReqTypes.ProductOutToIn) //线边外侧到里侧
                    {
                        //生成物料超市自动配送任务
                        int nOutPosition = nPosition + 4;
                        List<DST_DistributeTask> marketDistributeTasks = DistributingTaskGenerator.Instance.GenerateMarketAutoDistributeTask(sWorkStationCode, nOutPosition.ToString(), distributeTask.podDir, dbContext);
                        if (marketDistributeTasks.Count > 0)
                        {
                            foreach (DST_DistributeTask marketDistributeTask in marketDistributeTasks)
                            {
                                dbContext.DST_DistributeTasks.Add(marketDistributeTask);
                            }

                            logMessage = "工位" + sWorkStationCode + "生成物料超市配送任务，位置：" + nOutPosition + "，配送方式：" + distributeTask.podDir;
                            arrLog.Add(logMessage);
                        }
                    }
                    else if (distributeTask.DistributeReqTypes == DistributeReqTypes.ProductInToOut) //线边清线
                    {
                        //生成空料架返回任务
                        List<DST_DistributeTask> InToOutDistributeTasks = DistributingTaskGenerator.Instance.GenerateNullCartBackTask(sWorkStationCode, nPosition.ToString(), sCartCode, IsPTLPick, distributeTask.podDir);
                        if (InToOutDistributeTasks.Count > 0)
                        {
                            foreach (DST_DistributeTask InToOutDistributeTask in InToOutDistributeTasks)
                            {
                                dbContext.DST_DistributeTasks.Add(InToOutDistributeTask);
                            }

                            logMessage = "工位" + sWorkStationCode + "生成空料架返回任务，料架：" + sCartCode + "，位置：" + nPosition + "，配送方式：" + distributeTask.podDir;
                            arrLog.Add(logMessage);
                        }

                        //生成物料超市自动配送任务
                        if (distributeTask.data.Equals("PTL-Auto"))
                        {
                            List<DST_DistributeTask> marketDistributeTasks = DistributingTaskGenerator.Instance.GenerateMarketAutoDistributeTask(sWorkStationCode, nPosition.ToString(), distributeTask.podDir, dbContext);
                            if (marketDistributeTasks.Count > 0)
                            {
                                foreach (DST_DistributeTask marketDistributeTask in marketDistributeTasks)
                                {
                                    dbContext.DST_DistributeTasks.Add(marketDistributeTask);
                                }

                                logMessage = "工位" + sWorkStationCode + "生成物料超市配送任务，位置：" + nPosition + "，配送方式：" + distributeTask.podDir;
                                arrLog.Add(logMessage);
                            }
                        }

                        //空闲
                        cfgCart.CartStatus = CartStatus.Free;

                        logMessage = "料架" + sCartCode + "更新料架状态为" + CartStatus.Free;
                        arrLog.Add(logMessage);

                        ////解绑当前工位上的小车
                        //cfgWorkStationCurrentCart.CFG_CartId = null;
                        //cfgWorkStationCurrentCart.DockedTime = null;
                    }

                    distributeTask.arriveTime = distributeArriveTaskDto.reqTime;

                    bool isSuccess = dbContext.SaveChanges() > 0 ? true : false;

                    if (!isSuccess)
                    {
                        sErrorMsg = "数据保存失败";
                        return sErrorMsg;
                    }

                    if (IsPTLPick)
                    {
                        //设备控制
                        CFG_CartCurrentMaterial firstNotEmptyCfgCartCurrentMaterial = cfgCart.CFG_CartCurrentMaterials
                                                                                          .FirstOrDefault(ccm => ccm.AST_CartTaskItemId != null);
                        if (firstNotEmptyCfgCartCurrentMaterial != null)
                        {
                            CartBinder.DockCart(cfgCart.Id, "抵达工位 " + cfgWorkStation.Name, string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
批次：{2}", firstNotEmptyCfgCartCurrentMaterial.ProjectCode, firstNotEmptyCfgCartCurrentMaterial.ProjectStep, firstNotEmptyCfgCartCurrentMaterial.BatchCode), nPosition, "位");
                        }
                    }

                    foreach (string sLog in arrLog)
                    {
                        Logger.Log("生产线边单车业务-" + sWorkStationCode, DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
            }
            return sErrorMsg;
        }

        /// <summary>
        /// 生产线边料架转换
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        /// <param name="sNullCartCode">空料架编号</param>
        /// <returns></returns>
        private string ProductCartSwitch(DST_DistributeArriveTaskDto distributeArriveTaskDto, DST_DistributeTask distributeTask)
        {
            string sErrorMsg = "";
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ArrayList arrLog = new ArrayList();
                    string logMessage = "";

                    List<DST_CartArriveDetailDto> cartArriveDetailDtos = distributeArriveTaskDto.data;

                    foreach (DST_CartArriveDetailDto cartArriveDetailDto in cartArriveDetailDtos)
                    {
                        string sCartCode = cartArriveDetailDto.podCode;
                        string sCurrentPosition = cartArriveDetailDto.currentPosition;

                        CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(c => c.Code.Equals(sCartCode));
                        if (cfgCart == null)
                        {
                            sErrorMsg = "小车 " + sCartCode + " 不存在";
                            return sErrorMsg;
                        }

                        //if (!cfgCart.OnLine)
                        //{
                        //    sErrorMsg = "小车 " + cfgCart.Code + " 不在线";
                        //    return sErrorMsg;
                        //}

                        //foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                        //{
                        //    if (!cfgCartPtlDevice.OnLine)
                        //    {
                        //        sErrorMsg = "小车上的 " + cfgCartPtlDevice.DeviceAddress + " 号标签不在线";
                        //        return sErrorMsg;
                        //    }
                        //}

                        string[] arrPosition = sCurrentPosition.Split('-');
                        string sWorkStationCode = arrPosition[0]; //工位
                        int nPosition = Convert.ToInt32(arrPosition[1]); //车位

                        CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations.FirstOrDefault(t => t.Code.Equals(sWorkStationCode));
                        if (cfgWorkStation == null)
                        {
                            sErrorMsg = "工位 " + sWorkStationCode + " 不存在";
                            return sErrorMsg;
                        }

                        int cfgCartId = cfgCart.Id;

                        CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                           .FirstOrDefault(ccc => ccc.CFG_WorkStation.Code.Equals(sWorkStationCode) && ccc.Position == nPosition);
                        if (cfgWorkStationCurrentCart == null)
                        {
                            sErrorMsg = "车位 " + nPosition + " 不存在";
                            return sErrorMsg;
                        }

                        string sNullCartCode = distributeTask.podCode;
                        if (!sCartCode.Equals(sNullCartCode)) //满料架
                        {
                            if (cfgWorkStationCurrentCart.CFG_CartId != null && cfgWorkStationCurrentCart.CFG_CartId != cfgCartId)
                            {
                                sErrorMsg = "车位 " + nPosition + " 上的小车还未解除绑定";
                                return sErrorMsg;
                            }

                            //绑定当前工位上的小车
                            cfgWorkStationCurrentCart.CFG_CartId = cfgCart.Id;
                            cfgWorkStationCurrentCart.DockedTime = DateTime.Now;
                        }
                        else //空料架
                        {
                            cfgWorkStationCurrentCart.CFG_CartId = null;
                            cfgWorkStationCurrentCart.DockedTime = null;
                        }

                        logMessage = "料架" + sCartCode + "抵达工位" + sWorkStationCode + "，位置" + nPosition;
                        arrLog.Add(logMessage);

                        ////新增配送达到任务记录
                        //DST_DistributeArriveTask distributeArriveTask = GenerateDistributeArriveTask(distributeArriveTaskDto);
                        //dbContext.DST_DistributeArriveTasks.Add(distributeArriveTask);

                        ////新增配送任务到达结果明细
                        //List<DST_DistributeArriveResult> distributeArriveResults = GenerateDistributeArriveResult(dbContext, distributeArriveTaskDto, distributeTask);
                        //foreach (DST_DistributeArriveResult distributeArriveResult in distributeArriveResults)
                        //{
                        //    dbContext.DST_DistributeArriveResults.Add(distributeArriveResult);
                        //}

                        if (sCartCode.Equals(sNullCartCode)) //空料架，如果是线边缓冲区，才生成线边配送任务
                        {
                            //生成线边配送任务
                            List<DST_DistributeTask> productDistributeTasks = DistributingTaskGenerator.Instance.GenerateProductAreaDistributeTask(sWorkStationCode, nPosition.ToString(), sCartCode, distributeTask.podDir, false, dbContext);
                            if (productDistributeTasks.Count > 0)
                            {
                                foreach (DST_DistributeTask productDistributeTask in productDistributeTasks)
                                {
                                    dbContext.DST_DistributeTasks.Add(productDistributeTask);
                                }

                                logMessage = "工位" + sWorkStationCode + "生成线边配送任务，料架：" + sCartCode + "，位置：" + nPosition + "，配送方式：" + distributeTask.podDir;
                                arrLog.Add(logMessage);
                            }

                            //空闲
                            cfgCart.CartStatus = CartStatus.Free;

                            logMessage = "料架" + sCartCode + "更新料架状态为" + CartStatus.Free;
                            arrLog.Add(logMessage);

                            ////解绑当前工位上的小车
                            //cfgWorkStationCurrentCart.CFG_CartId = null;
                            //cfgWorkStationCurrentCart.DockedTime = null;
                        }

                        distributeTask.arriveTime = distributeArriveTaskDto.reqTime;

                        bool isSuccess = dbContext.SaveChanges() > 0 ? true : false;

                        if (!isSuccess)
                        {
                            sErrorMsg = "数据保存失败";
                            return sErrorMsg;
                        }

                        foreach (string sLog in arrLog)
                        {
                            Logger.Log("生产线边空满转换-" + sWorkStationCode, DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
            }
            return sErrorMsg;
        }

        /// <summary>
        /// 停靠小车到空料架缓冲区
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        /// <returns></returns>
        private string NullCartAreaDock(DST_DistributeArriveTaskDto distributeArriveTaskDto, DST_DistributeTask distributeTask)
        {
            string sErrorMsg = "";
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ArrayList arrLog = new ArrayList();
                    string logMessage = "";

                    string sCartCode = "";
                    List<DST_CartArriveDetailDto> cartArriveDetailDtos = distributeArriveTaskDto.data;

                    if (distributeTask.DistributeReqTypes == DistributeReqTypes.ProductCartSwitch) //空满转换之后的空料架返回
                    {
                        DST_CartArriveDetailDto cartArriveDetailDto = cartArriveDetailDtos[1];
                        sCartCode = cartArriveDetailDto.podCode;
                    }
                    else
                    {
                        DST_CartArriveDetailDto cartArriveDetailDto = cartArriveDetailDtos[0];
                        sCartCode = cartArriveDetailDto.podCode;
                    }

                    CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(c => c.Code.Equals(sCartCode));
                    if (cfgCart == null)
                    {
                        sErrorMsg = "小车 " + sCartCode + " 不存在";
                        return sErrorMsg;
                    }

                    bool IsPTLPick = true; //是否PTL拣料
                    if (distributeTask.data.Equals("PDA"))
                    {
                        IsPTLPick = false;
                    }
                    if (IsPTLPick)
                    {
                        CartBinder.UnDockCart(cfgCart.Id);
                    }

                    logMessage = "料架" + sCartCode + "抵达空料架缓冲区";
                    arrLog.Add(logMessage);

                    //空闲
                    cfgCart.CartStatus = CartStatus.Free;

                    //logMessage = "料架" + sCartCode + "更新料架状态为" + CartStatus.Free;
                    //arrLog.Add(logMessage);

                    //清空小车上的物料
                    List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials.ToList();
                    if (cfgCartCurrentMaterials.Count > 0)
                    {
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

                        logMessage = "料架" + sCartCode + "清空小车物料明细";
                        arrLog.Add(logMessage);
                    }

                    //移除巷道料架
                    List<CFG_ChannelCurrentCart> cfgChannelCurrentCarts = cfgCart.CFG_ChannelCurrentCarts.ToList();
                    if (cfgChannelCurrentCarts.Count > 0)
                    {
                        foreach (CFG_ChannelCurrentCart cfgChannelCurrentCart in cfgChannelCurrentCarts)
                        {
                            cfgChannelCurrentCart.CFG_CartId = null;
                            cfgChannelCurrentCart.DockedTime = null;
                        }

                        logMessage = "料架" + sCartCode + "删除巷道位置绑定关系";
                        arrLog.Add(logMessage);
                    }

                    //移除生产线边料架
                    List<CFG_WorkStationCurrentCart> cfgWorkStationCurrentCarts = cfgCart.CFG_WorkStationCurrentCarts.ToList();
                    if (cfgWorkStationCurrentCarts.Count > 0)
                    {
                        foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in cfgWorkStationCurrentCarts)
                        {
                            cfgWorkStationCurrentCart.CFG_CartId = null;
                            cfgWorkStationCurrentCart.DockedTime = null;
                        }

                        logMessage = "料架" + sCartCode + "删除生产线边位置绑定关系";
                        arrLog.Add(logMessage);
                    }

                    //结束还未启动的装配任务
                    List<ASM_TaskItem> asmTaskItems = dbContext.ASM_TaskItems.Where(t => t.CFG_CartId == cfgCart.Id && t.AssembleStatus == AssembleStatus.New).ToList();
                    if (asmTaskItems.Count > 0)
                    {
                        foreach (ASM_TaskItem asmTaskItem in asmTaskItems)
                        {
                            asmTaskItem.AssembleStatus = AssembleStatus.Finished;
                            asmTaskItem.AssembledQuantity = asmTaskItem.ToAssembleQuantity;
                            asmTaskItem.AssembledTime = DateTime.Now;

                            ASM_AssembleIndicationItem asmAssembleIndicationItem = asmTaskItem.ASM_AssembleIndicationItem;
                            asmAssembleIndicationItem.AssembleStatus = AssembleStatus.Finished;
                            asmAssembleIndicationItem.AssembledQuantity = asmAssembleIndicationItem.ToAssembleQuantity;
                            asmAssembleIndicationItem.AssembledTime = DateTime.Now;

                            ASM_AssembleIndication asmAssembleIndication = asmAssembleIndicationItem.ASM_AssembleIndication;
                            ASM_Task asmTask = asmTaskItem.ASM_Task;
                            if (asmTask.ASM_TaskItems.All(ti => ti.AssembleStatus == AssembleStatus.Finished))
                            {
                                asmTask.AssembleStatus = AssembleStatus.Finished;
                                asmAssembleIndication.AssembleStatus = AssembleStatus.Finished;

                                #region 当前装配可以提交

                                List<ASM_AssembleIndicationItem> asmAssembleIndicatonItemsByTask = asmAssembleIndication.ASM_AssembleIndicationItems
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

                                foreach (ASM_AssembleIndicationItem asmAssembleIndicationItemByTask in asmAssembleIndicatonItemsByTask)
                                {
                                    List<ASM_TaskItem> asmTaskItemsByTask = asmAssembleIndicationItemByTask.ASM_TaskItems
                                                                          .ToList();
                                    ASM_TaskItem lastAsmTaskItem = asmTaskItemsByTask.Last();

                                    ASM_AssembleResultItem asmAssembleResultItem = new ASM_AssembleResultItem();
                                    asmAssembleResultItem.ASM_AssembleResult = asmAssembleResult;
                                    asmAssembleResultItem.CFG_CartId = lastAsmTaskItem.CFG_CartId;
                                    asmAssembleResultItem.CartPosition = lastAsmTaskItem.CartPosition;
                                    asmAssembleResultItem.Gzz = asmAssembleIndicationItemByTask.Gzz;
                                    asmAssembleResultItem.MaterialCode = asmAssembleIndicationItemByTask.MaterialCode;
                                    asmAssembleResultItem.MaterialName = asmAssembleIndicationItemByTask.MaterialName;
                                    asmAssembleResultItem.AssembleSequence = asmAssembleIndicationItemByTask.AssembleSequence;
                                    asmAssembleResultItem.ToAssembleQuantity = asmAssembleIndicationItemByTask.ToAssembleQuantity;
                                    asmAssembleResultItem.AssembledQuantity = asmTaskItemsByTask.Sum(ti => ti.AssembledQuantity.Value);
                                    asmAssembleResultItem.PickedTime = lastAsmTaskItem.AssembledTime.Value;
                                    asmAssembleResultItem.ProjectCode = asmAssembleIndicationItemByTask.ProjectCode;
                                    asmAssembleResultItem.ProjectStep = asmAssembleIndicationItemByTask.ProjectStep;

                                    dbContext.ASM_AssembleResultItems.Add(asmAssembleResultItem);
                                }

                                #endregion
                            }
                        }

                        logMessage = "料架" + sCartCode + "结束还未启动的装配任务";
                        arrLog.Add(logMessage);
                    }

                    ////新增配送达到任务记录
                    //DST_DistributeArriveTask distributeArriveTask = GenerateDistributeArriveTask(distributeArriveTaskDto);
                    //dbContext.DST_DistributeArriveTasks.Add(distributeArriveTask);

                    ////新增配送任务到达结果明细
                    //List<DST_DistributeArriveResult> distributeArriveResults = GenerateDistributeArriveResult(dbContext, distributeArriveTaskDto, distributeTask);
                    //foreach (DST_DistributeArriveResult distributeArriveResult in distributeArriveResults)
                    //{
                    //    dbContext.DST_DistributeArriveResults.Add(distributeArriveResult);
                    //}

                    distributeTask.arriveTime = distributeArriveTaskDto.reqTime;

                    bool isSuccess = dbContext.SaveChanges() > 0 ? true : false;

                    if (!isSuccess)
                    {
                        sErrorMsg = "数据保存失败";
                        return sErrorMsg;
                    }

                    foreach (string sLog in arrLog)
                    {
                        Logger.Log("空料架缓冲区", DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
            }
            return sErrorMsg;
        }

        /// <summary>
        /// 点对点配送停靠
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        /// <returns></returns>
        private string PointToPointDock(DST_DistributeArriveTaskDto distributeArriveTaskDto, DST_DistributeTask distributeTask)
        {
            string sErrorMsg = "";
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ArrayList arrLog = new ArrayList();
                    string logMessage = "";

                    DST_CartArriveDetailDto cartArriveDetailDto = distributeArriveTaskDto.data[0];
                    string sCartCode = cartArriveDetailDto.podCode;

                    CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(c => c.Code.Equals(sCartCode));
                    if (cfgCart == null)
                    {
                        sErrorMsg = "小车 " + sCartCode + " 不存在";
                        return sErrorMsg;
                    }

                    string[] arrData = distributeTask.data.Split('&');
                    string sTaskype = arrData[0];
                    string sIsNullCartArea = arrData[1];
                    bool IsPTLPick = true; //是否PTL拣料
                    if (sTaskype.Equals("PDA"))
                    {
                        IsPTLPick = false;
                    }
                    int cfgCartId = cfgCart.Id;

                    if (sIsNullCartArea.Equals("true")) //料架缓冲区
                    {
                        if (IsPTLPick)
                        {
                            CartBinder.UnDockCart(cfgCartId);
                        }

                        logMessage = "料架" + sCartCode + "抵达空料架缓冲区";
                        arrLog.Add(logMessage);

                        //空闲
                        cfgCart.CartStatus = CartStatus.Free;

                        //logMessage = "料架" + sCartCode + "更新料架状态为" + CartStatus.Free;
                        //arrLog.Add(logMessage);

                        //清空小车上的物料
                        List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials.ToList();
                        if (cfgCartCurrentMaterials.Count > 0)
                        {
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

                            logMessage = "料架" + sCartCode + "清空小车物料明细";
                            arrLog.Add(logMessage);
                        }

                        MarketZone marketZone = dbContext.MarketZones.FirstOrDefault(t => t.CFG_CartId == cfgCartId);
                        if (marketZone != null)
                        {
                            marketZone.Status = 0;
                            marketZone.CFG_CartId = null;
                            marketZone.DockedTime = null;
                        }
                    }
                    else
                    {
                        logMessage = "料架" + sCartCode + "抵达点对点目的地";
                        arrLog.Add(logMessage);
                    }

                    ////移除物料超市料架
                    //List<CFG_MarketWorkStationCurrentCart> cfgMarketWorkStationCurrentCarts = dbContext.CFG_MarketWorkStationCurrentCarts.Where(t => t.CFG_CartId == cfgCartId).ToList();
                    //if (cfgMarketWorkStationCurrentCarts.Count > 0)
                    //{
                    //    dbContext.CFG_MarketWorkStationCurrentCarts.RemoveRange(cfgMarketWorkStationCurrentCarts);

                    //    logMessage = "删除料架" + sCartCode + "的物料超市绑定关系";
                    //    arrLog.Add(logMessage);
                    //}

                    //移除巷道料架
                    List<CFG_ChannelCurrentCart> cfgChannelCurrentCarts = cfgCart.CFG_ChannelCurrentCarts.ToList();
                    if (cfgChannelCurrentCarts.Count > 0)
                    {
                        foreach (CFG_ChannelCurrentCart cfgChannelCurrentCart in cfgChannelCurrentCarts)
                        {
                            cfgChannelCurrentCart.CFG_CartId = null;
                            cfgChannelCurrentCart.DockedTime = null;
                        }

                        logMessage = "料架" + sCartCode + "删除巷道位置绑定关系";
                        arrLog.Add(logMessage);
                    }

                    //移除生产线边料架
                    List<CFG_WorkStationCurrentCart> cfgWorkStationCurrentCarts = cfgCart.CFG_WorkStationCurrentCarts.ToList();
                    if (cfgWorkStationCurrentCarts.Count > 0)
                    {
                        foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in cfgWorkStationCurrentCarts)
                        {
                            cfgWorkStationCurrentCart.CFG_CartId = null;
                            cfgWorkStationCurrentCart.DockedTime = null;
                        }

                        logMessage = "料架" + sCartCode + "删除生产线边位置绑定关系";
                        arrLog.Add(logMessage);
                    }


                    ////新增配送达到任务记录
                    //DST_DistributeArriveTask distributeArriveTask = GenerateDistributeArriveTask(distributeArriveTaskDto);
                    //dbContext.DST_DistributeArriveTasks.Add(distributeArriveTask);

                    ////新增配送任务到达结果明细
                    //List<DST_DistributeArriveResult> distributeArriveResults = GenerateDistributeArriveResult(dbContext, distributeArriveTaskDto, distributeTask);
                    //foreach (DST_DistributeArriveResult distributeArriveResult in distributeArriveResults)
                    //{
                    //    dbContext.DST_DistributeArriveResults.Add(distributeArriveResult);
                    //}

                    distributeTask.arriveTime = distributeArriveTaskDto.reqTime;

                    bool isSuccess = dbContext.SaveChanges() > 0 ? true : false;

                    if (!isSuccess)
                    {
                        sErrorMsg = "数据保存失败";
                        return sErrorMsg;
                    }

                    foreach (string sLog in arrLog)
                    {
                        Logger.Log("点对点配送", DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
            }
            return sErrorMsg;
        }

        /// <summary>
        /// 新增配送任务到达结果明细
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        /// <param name="distributeTask"></param>
        /// <returns></returns>
        private List<DST_DistributeArriveResult> GenerateDistributeArriveResult(GeelyPtlEntities dbContext, DST_DistributeArriveTaskDto distributeArriveTaskDto, DST_DistributeTask distributeTask)
        {
            List<DST_DistributeArriveResult> distributeArriveResults = new List<DST_DistributeArriveResult>();

            List<DST_CartArriveDetailDto> cartArriveDetailDtos = distributeArriveTaskDto.data;

            string sMethod = distributeArriveTaskDto.method;

            string sStartPosition = "";
            string sEndPosition = "";

            string sCartCode = "";
            string sCurrentPosition = "";
            string sReqCode = "";
            bool isTaskExists = false;

            foreach (DST_CartArriveDetailDto cartArriveDetailDto in cartArriveDetailDtos)
            {
                sCartCode = cartArriveDetailDto.podCode;
                sCurrentPosition = cartArriveDetailDto.currentPosition;
                sReqCode = distributeTask.reqCode;

                isTaskExists = dbContext.DST_DistributeArriveResults.Any(t => t.reqCode.Equals(sReqCode) && t.podCode.Equals(sCartCode) && t.currentPointCode.Equals(sCurrentPosition));
                if (isTaskExists)
                {
                    continue;
                }

                DistributeReqTypes distributeReqType = distributeTask.DistributeReqTypes;
                if (sMethod.Equals("TaskFinish10"))
                {
                    distributeReqType = DistributeReqTypes.ProductNullCartBack;
                }

                if (distributeReqType == DistributeReqTypes.PickAreaInit || distributeReqType == DistributeReqTypes.NullCartAreaDistribute)
                {
                    sStartPosition = "空料架缓冲区";
                    sEndPosition = sCurrentPosition;
                }
                else if (distributeReqType == DistributeReqTypes.PickAreaDistribute)
                {
                    sStartPosition = distributeTask.startPosition;
                    sEndPosition = distributeTask.endPosition;
                }
                else if (distributeReqType == DistributeReqTypes.ProductAreaInit || distributeReqType == DistributeReqTypes.MaterialMarketDistribute)
                {
                    sStartPosition = distributeTask.startPosition;
                    sEndPosition = distributeTask.endPosition;
                }
                else if (distributeReqType == DistributeReqTypes.ProductCartSwitch)
                {
                    if (sCartCode.Equals(distributeTask.podCode)) //空料架
                    {
                        sStartPosition = distributeTask.startPosition;
                        sEndPosition = sCurrentPosition;
                    }
                    else //满料架
                    {
                        string[] arrPosition = distributeTask.startPosition.Split('-');
                        if (arrPosition.Count() > 0)
                        {
                            sStartPosition = arrPosition[0] + "-" + (Convert.ToInt32(arrPosition[1]) + 4);
                        }
                        else
                        {
                            sStartPosition = "";
                        }
                        sEndPosition = sCurrentPosition;
                    }
                }
                else if (distributeReqType == DistributeReqTypes.ProductNullCartBack)
                {
                    sStartPosition = distributeTask.startPosition;
                    sEndPosition = "空料架缓冲区";
                }
                else if (distributeReqType == DistributeReqTypes.PointToPointDistribute)
                {
                    sStartPosition = distributeTask.startPosition;
                    sEndPosition = distributeTask.endPosition;
                }

                DST_DistributeArriveResult distributeArriveResult = new DST_DistributeArriveResult();
                distributeArriveResult.reqCode = sReqCode;
                distributeArriveResult.arriveTime = distributeArriveTaskDto.reqTime;
                distributeArriveResult.startPosition = sStartPosition;
                distributeArriveResult.endPosition = sEndPosition;
                distributeArriveResult.podCode = sCartCode;
                distributeArriveResult.currentPointCode = sCurrentPosition;
                distributeArriveResult.DistributeReqTypes = distributeReqType;

                distributeArriveResults.Add(distributeArriveResult);
            }
            return distributeArriveResults;
        }

        /// <summary>
        /// 新增配送任务达到记录
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        /// <returns></returns>
        private DST_DistributeArriveTask GenerateDistributeArriveTask(DST_DistributeArriveTaskDto distributeArriveTaskDto)
        {
            DST_DistributeArriveTask distributeArriveTask = new DST_DistributeArriveTask();
            distributeArriveTask.method = distributeArriveTaskDto.method;
            distributeArriveTask.taskType = distributeArriveTaskDto.taskType;
            distributeArriveTask.reqCode = distributeArriveTaskDto.reqCode;
            distributeArriveTask.reqTime = Convert.ToDateTime(distributeArriveTaskDto.reqTime);
            distributeArriveTask.robotCode = distributeArriveTaskDto.robotCode;
            distributeArriveTask.taskCode = distributeArriveTaskDto.taskCode;
            distributeArriveTask.data = JsonConvert.SerializeObject(distributeArriveTaskDto.data);
            distributeArriveTask.receiveTime = DateTime.Now;

            return distributeArriveTask;
        }

        /// <summary>
        /// 新增配送到达任务结果
        /// </summary>
        /// <param name="sReqCode"></param>
        /// <param name="sErrorMsg"></param>
        /// <returns></returns>
        private DST_DistributeArriveTaskResult GenerateDistributeArriveTaskResult(string sReqCode, string sErrorMsg, string sResult)
        {
            string sCode = "0";
            if (!string.IsNullOrEmpty(sErrorMsg))
            {
                sCode = "99";
            }
            else
            {
                sErrorMsg = "成功";
            }
            DST_DistributeArriveTaskResult distributeArriveTaskResult = new DST_DistributeArriveTaskResult();
            distributeArriveTaskResult.code = sCode;
            distributeArriveTaskResult.message = sErrorMsg;
            distributeArriveTaskResult.reqCode = sReqCode;
            distributeArriveTaskResult.data = sResult;
            distributeArriveTaskResult.sendTime = DateTime.Now;

            return distributeArriveTaskResult;
        }

        /// <summary>
        /// 新增配送到达任务结果返回
        /// </summary>
        /// <param name="sReqCode"></param>
        /// <param name="sErrorMsg"></param>
        /// <returns></returns>
        private DST_DistributeArriveTaskResultDto GenerateDistributeArriveTaskResultDto(string sReqCode, string sErrorMsg)
        {
            string sCode = "0";
            if (!string.IsNullOrEmpty(sErrorMsg))
            {
                sCode = "99";
            }
            else
            {
                sErrorMsg = "成功";
            }
            DST_DistributeArriveTaskResultDto distributeArriveTaskResultDto = new DST_DistributeArriveTaskResultDto();
            distributeArriveTaskResultDto.code = sCode;
            distributeArriveTaskResultDto.message = sErrorMsg;
            distributeArriveTaskResultDto.reqCode = sReqCode;
            distributeArriveTaskResultDto.data = "";

            return distributeArriveTaskResultDto;
        }

        public string Test()
        {
            #region Test
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                //DST_DistributeArriveTask distributeArriveTask = new DST_DistributeArriveTask();
                //distributeArriveTask.method = "11";
                //distributeArriveTask.taskType = "22";
                //distributeArriveTask.reqCode = "33";
                //distributeArriveTask.reqTime = DateTime.Now;
                //distributeArriveTask.robotCode = "44";
                //distributeArriveTask.taskCode = "55";
                //distributeArriveTask.data = "66";
                //distributeArriveTask.receiveTime = DateTime.Now;
                //dbContext.DST_DistributeArriveTasks.Add(distributeArriveTask);

                //DST_DistributeArriveTaskResult distributeArriveTaskResult = new DST_DistributeArriveTaskResult();
                //distributeArriveTaskResult.code = "111";
                //distributeArriveTaskResult.message = "222";
                //distributeArriveTaskResult.reqCode = "333";
                //distributeArriveTaskResult.data = "444";
                //distributeArriveTaskResult.sendTime = DateTime.Now;

                //dbContext.DST_DistributeArriveTaskResults.Add(distributeArriveTaskResult);

                //return dbContext.SaveChanges() > 0 ? "true" : "false";

                string result = "111";

                //int n = dbContext.FND_Tasks.Count(t => t.CFG_WorkStationId == 4
                //                                                               && (t.CFG_Cart.CartStatus == CartStatus.NeedToWorkStation
                //                                                                   || t.CFG_Cart.CartStatus == CartStatus.WaitingToWorkStation
                //                                                                   || t.CFG_Cart.CartStatus == CartStatus.InCarriageToWorkStation));

                //int n = dbContext.FND_Tasks.Where(t => t.CFG_WorkStationId == 4
                //    && t.CFG_Cart.CFG_CartCurrentMaterials.FirstOrDefault(m => m.CFG_WorkStationId == 4) != null
                //    && (t.CFG_Cart.CartStatus == CartStatus.NeedToWorkStation
                //        || t.CFG_Cart.CartStatus == CartStatus.WaitingToWorkStation
                //        || t.CFG_Cart.CartStatus == CartStatus.InCarriageToWorkStation))
                //.Select(t => t.CFG_CartId).Distinct().Count();
                //result = n.ToString();

                //bool isSuccess = DistributingTaskGenerator.Instance.GeneratePDAPickAreaDistributeTask("00000000000000000007B1A2", "ZT01");
                //result = isSuccess ? "success" : "error";


                //result = DistributingTaskGenerator.Instance.GenerateMarketDistributeTask("ZT01", "051300XY035950", "00000000000000000007B1A7", true);

                //result = new DistributeServiceExecutor().GenerateBatchMarketDistributeTask("18", false);

                //string MinTime = "2018/8/21";
                //string MaxTime = "2018/8/21";
                //result = new DistributeServiceExecutor().GetDistributeTask(MinTime, MaxTime, "拣料区铺线", "0");

                //List<long> listNeedBlinkFndTask = new List<long>();
                //listNeedBlinkFndTask.Add(8);
                //listNeedBlinkFndTask.Add(28);
                //FND_Task fndTask = dbContext.FND_Tasks.Where(t=>(t.CFG_Cart.CartStatus == CartStatus.NeedToWorkStation || t.CFG_Cart.CartStatus == CartStatus.WaitingToWorkStation)
                //                            && !listNeedBlinkFndTask.Contains(t.Id)).
                //                            OrderBy(t => t.BatchCode).ThenByDescending(t => t.FindingStatus).FirstOrDefault();
                //if (fndTask != null)
                //{
                //    result = fndTask.Id.ToString();
                //}

                //DST_AgvSwitch dstAgvSwitch = dbContext.DST_AgvSwitchs.FirstOrDefault(t => t.isOpen);
                //if (dstAgvSwitch == null)
                //{
                //    result = "222";
                //}

                //result = System.Configuration.ConfigurationManager.AppSettings["PtlToAgvServiceUrl"];

                //List<string> NoDistributeWorkStationCode = new List<string>();
                //Hashtable htWorkStation = new Hashtable();
                //List<CFG_Cart> cfgCarts = dbContext.CFG_Carts.Where(t => (t.CartStatus == CartStatus.ArrivedAtBufferArea
                //    || t.CartStatus == CartStatus.NeedToWorkStation)
                //    && t.FND_Tasks.FirstOrDefault(f => f.FindingStatus == DataAccess.CartFinding.FindingStatus.New) != null).ToList();
                //foreach (CFG_Cart cfgCart in cfgCarts)
                //{
                //    CFG_CartCurrentMaterial firstNotEmptyCfgCartCurrentMaterial = cfgCart.CFG_CartCurrentMaterials
                //                                                                  .FirstOrDefault(ccm => ccm.AST_CartTaskItemId != null);
                //    if (firstNotEmptyCfgCartCurrentMaterial != null)
                //    {
                //        string sWorkStationCode = firstNotEmptyCfgCartCurrentMaterial.CFG_WorkStation.Code;
                //        if (!htWorkStation.Contains(sWorkStationCode))
                //        {
                //            htWorkStation.Add(sWorkStationCode, 1);
                //        }
                //        else
                //        {
                //            htWorkStation[sWorkStationCode] = Convert.ToInt32(htWorkStation[sWorkStationCode]) + 1;
                //            if (Convert.ToInt32(htWorkStation[sWorkStationCode]) > 2)
                //            {
                //                NoDistributeWorkStationCode.Add(sWorkStationCode);
                //            }
                //        }
                //    }
                //}

                //string sql = "select GroundId from MarketZone";
                //List<string> list = dbContext.Database.SqlQuery<string>(sql).ToList();
                //result = list.Count.ToString();
                return result;
            }

            //string sInfo = "{\"data\":[{\"currentPosition\":\"ZT03-7\",\"podCode\":\"100011\"}],\"method\":\"FinishTask\",\"taskType\":\"\",\"reqCode\":\"16483C173C2112B\",\"reqTime\":\"2018-07-10 18:34:10.000\",\"robotCode\":\"8197\",\"taskCode\":\"ptl0316483BC1F36111Y\"}";
            //string sInfo = "{\"data\":[{\"currentPosition\":\"ZT03-7\",\"podCode\":\"100008\"},{\"currentPosition\":\"ZT03-3\",\"podCode\":\"100007\"}],\"method\":\"taskFinish\",\"reqCode\":\"16483D74F8A1131\",\"reqTime\":\"2018-07-10 18:58:03\",\"robotCode\":\"8197\",\"taskCode\":\"plt0516483D3B5BB112P\"}";
            //JObject reqInfo = JObject.Parse(sInfo);
            //string result = DistributeArriveHandle(reqInfo);
            //return result;
            #endregion
        }

        /// <summary>
        /// 记录到货状态
        /// </summary>
        /// <param name="distributeArriveTaskDto"></param>
        private void SaveMarketZoneStatus(DST_DistributeArriveTaskDto distributeArriveTaskDto)
        {
            if (distributeArriveTaskDto.method == "taskFinish1")//绿灯
            {
                ChangeStatus(distributeArriveTaskDto, 1);
            }
            else if (distributeArriveTaskDto.method == "StartService")//蓝闪
            {
                ChangeStatus(distributeArriveTaskDto, 2);
            }
            else if (distributeArriveTaskDto.method == "OutFromBin")//灭灯
            {
                ChangeStatus(distributeArriveTaskDto, 0);
            }
        }

        private void ChangeStatus(DST_DistributeArriveTaskDto distributeArriveTaskDto, int status)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in distributeArriveTaskDto.data)
                        {
                            var target = dbContext.MarketZones.FirstOrDefault(x => x.GroundId == item.currentPosition);
                            if (target != null)
                            {
                                target.Status = status;
                            }
                        }

                        dbContext.SaveChanges();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }
    }
}