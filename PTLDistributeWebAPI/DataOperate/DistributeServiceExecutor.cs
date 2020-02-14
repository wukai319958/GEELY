using DataAccess;
using DataAccess.AssemblyIndicating;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using DataAccess.Other;
using Distributing;
using Newtonsoft.Json;
using PTLDistributeWebAPI.HttpOperate;
using PTLDistributeWebAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace PTLDistributeWebAPI.DataOperate
{
    public class DistributeServiceExecutor : IDistributeServiceExecutor
    {
        /// <summary>
        /// 获取巷道信息
        /// </summary>
        /// <returns></returns>
        public string GetChannelInfo()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<ChannelDto> channelDtos = new List<ChannelDto>();

                List<CFG_Channel> cfgChannels = dbContext.CFG_Channels.OrderBy(t => t.Id).ToList();
                foreach (CFG_Channel cfgChannel in cfgChannels)
                {
                    ChannelDto channelDto = new ChannelDto();
                    channelDto.Id = cfgChannel.Id;
                    channelDto.Code = cfgChannel.Code;
                    channelDto.Name = cfgChannel.Name;

                    channelDtos.Add(channelDto);
                }

                string result = JsonConvert.SerializeObject(channelDtos);
                return result;
            }
        }

        /// <summary>
        /// 生成拣料区铺线任务
        /// </summary>
        /// <param name="InitChannelIds">拣料区铺线的巷道ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GeneratePickAreaInitTask(string InitChannelIds, bool IsPTLPick)
        {
            string[] arrInitChannelId = InitChannelIds.Split(',');
            List<int> listInitChannelId = new List<int>();
            foreach (string sInitChannelId in arrInitChannelId)
            {
                listInitChannelId.Add(Convert.ToInt32(sInitChannelId));
            }

            bool isSuccess = DistributingTaskGenerator.Instance.GeneratePickAreaInitTask(listInitChannelId, IsPTLPick);
            string result = isSuccess ? "success" : "error";
            return result;
        }

        /// <summary>
        /// 生成PDA拣料区配送任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="sWorkStationCode">工位编码</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateHandPickAreaDistributeTask(string sCartRFID, string sWorkStationCode, bool IsPTLPick)
        {
            bool isSuccess = DistributingTaskGenerator.Instance.GenerateHandPickAreaDistributeTask(sCartRFID, sWorkStationCode, IsPTLPick);
            string result = isSuccess ? "success" : "error";
            return result;
        }

        /// <summary>
        /// 生成料架绑定或解绑任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="sPonitCode">储位编号</param>
        /// <param name="IsBind">是否绑定</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="sAreaFlag">区域标识（0：料架缓冲区，1：拣料区，2：物料超市，3：生产线边）</param>
        /// <param name="sAreaCode">区域编码</param>
        /// <param name="sPosition">区域车位</param>
        /// <returns></returns>
        public string GenerateBindOrUnBindTask(string sCartRFID, string sPonitCode, bool IsBind, bool IsPTLPick, string sAreaFlag, string sAreaCode, string sPosition)
        {
            string retMsg = "error";
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                if (cfgCart == null)
                {
                    retMsg = "没有找到对应的料架";
                    return retMsg;
                }
                int cfgCartId = cfgCart.Id;
                int nAreaFlag = Convert.ToInt32(sAreaFlag);
                int nPosition = Convert.ToInt32(sPosition);

                #region 判断料架是否符合条件
                if (nAreaFlag == 0) //料架缓冲区
                {
                    if (IsBind)
                    {
                        //if (cfgCart.CartStatus != CartStatus.Free)
                        //{
                        //    retMsg = "小车 " + cfgCart.Code + " 未释放：" + cfgCart.CartStatus;
                        //    return retMsg;
                        //}

                        if (IsPTLPick)
                        {
                            if (!cfgCart.OnLine)
                            {
                                retMsg = "小车 " + cfgCart.Code + " 不在线";
                                return retMsg;
                            }
                            foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                            {
                                if (!cfgCartPtlDevice.OnLine)
                                {
                                    retMsg = "小车上的 " + cfgCartPtlDevice.DeviceAddress + " 号标签不在线";
                                    return retMsg;
                                }
                            }
                        }
                    }
                }
                else if (nAreaFlag == 1) //拣料区
                {
                    CFG_Channel cfgChannel = dbContext.CFG_Channels.FirstOrDefault(t => t.Code.Equals(sAreaCode));
                    if (cfgChannel == null)
                    {
                        retMsg = "没有找到对应的巷道";
                        return retMsg;
                    }

                    if (nPosition < 0 || nPosition > 4)
                    {
                        retMsg = "该巷道找不到对应的车位";
                        return retMsg;
                    }

                    if (IsBind)
                    {
                        if (IsPTLPick)
                        {
                            if (!cfgCart.OnLine)
                            {
                                retMsg = "小车 " + cfgCart.Code + " 不在线";
                                return retMsg;
                            }
                            foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                            {
                                if (!cfgCartPtlDevice.OnLine)
                                {
                                    retMsg = "小车上的 " + cfgCartPtlDevice.DeviceAddress + " 号标签不在线";
                                    return retMsg;
                                }
                            }
                        }

                        CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                       .FirstOrDefault(ccc => ccc.CFG_ChannelId == cfgChannel.Id && ccc.Position == nPosition);
                        if (cfgChannelCurrentCart == null)
                        {
                            retMsg = "车位 " + nPosition + " 不存在";
                            return retMsg;
                        }

                        if (cfgChannelCurrentCart.CFG_CartId != null && cfgChannelCurrentCart.CFG_CartId != cfgCartId)
                        {
                            retMsg = "车位 " + nPosition + " 上的小车还未解除绑定";
                            return retMsg;
                        }

                        if (cfgChannelCurrentCart.CFG_CartId == null)
                        {
                            CFG_ChannelCurrentCart dockedCfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                                .FirstOrDefault(ccc => ccc.CFG_CartId == cfgCartId);
                            if (dockedCfgChannelCurrentCart != null)
                            {
                                retMsg = "小车 " + dockedCfgChannelCurrentCart.CFG_Cart.Code + " 已停靠在 " + dockedCfgChannelCurrentCart.CFG_Channel.Name + " 车位 " + dockedCfgChannelCurrentCart.Position;
                                return retMsg;
                            }
                        }
                    }
                }
                else if (nAreaFlag == 3) //生产线边
                {
                    CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations.FirstOrDefault(t => t.Code.Equals(sAreaCode));
                    if (cfgWorkStation == null)
                    {
                        retMsg = "没有找到对应的工位";
                        return retMsg;
                    }

                    if (nPosition < 0 || nPosition > 8)
                    {
                        retMsg = "该工位找不到对应的车位";
                        return retMsg;
                    }

                    if (IsBind)
                    {
                        CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                          .FirstOrDefault(ccc => ccc.CFG_WorkStation.Code.Equals(sAreaCode) && ccc.Position == nPosition);
                        if (cfgWorkStationCurrentCart == null)
                        {
                            retMsg = "车位 " + nPosition + " 不存在";
                            return retMsg;
                        }

                        if (cfgWorkStationCurrentCart.CFG_CartId != null && cfgWorkStationCurrentCart.CFG_CartId != cfgCartId)
                        {
                            retMsg = "车位 " + nPosition + " 上的小车还未解除绑定";
                            return retMsg;
                        }

                        if (cfgWorkStationCurrentCart.CFG_CartId == null)
                        {
                            CFG_WorkStationCurrentCart dockedCfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                                .FirstOrDefault(ccc => ccc.CFG_CartId == cfgCartId);
                            if (dockedCfgWorkStationCurrentCart != null)
                            {
                                retMsg = "小车 " + dockedCfgWorkStationCurrentCart.CFG_Cart.Code + " 已停靠在 线边工位" + dockedCfgWorkStationCurrentCart.CFG_WorkStation.Code + " 车位 " + dockedCfgWorkStationCurrentCart.Position;
                                return retMsg;
                            }
                        }
                    }
                }
                #endregion
            }
            string result = DistributingTaskGenerator.Instance.GenerateBindOrUnBindTask(sCartRFID, sPonitCode, IsBind);
            if (!result.Contains("error"))
            {
                retMsg = SendBindOrUnBindTask(result, sCartRFID, IsBind, IsPTLPick, sAreaFlag, sAreaCode, sPosition, sPonitCode);
            }
            return retMsg;
        }

        /// <summary>
        /// 发送绑定或解绑任务
        /// </summary>
        /// <param name="sReqCode">配送请求单号</param>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsBind">是否绑定</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="sAreaFlag">区域标识（0：料架缓冲区，1：拣料区，2：物料超市，3：生产线边）</param>
        /// <param name="sAreaCode">区域编码</param>
        /// <param name="sPosition">区域车位</param>
        /// <param name="sPonitCode">储位编号</param>
        /// <returns></returns>
        private string SendBindOrUnBindTask(string sReqCode, string sCartRFID, bool IsBind, bool IsPTLPick, string sAreaFlag, string sAreaCode, string sPosition, string sPonitCode)
        {
            string retMsg = "error";
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ArrayList arrNullCartAreaLog = new ArrayList();
                    ArrayList arrPickAreaLog = new ArrayList();
                    ArrayList arrMarketAreaLog = new ArrayList();
                    ArrayList arrProductAreaLog = new ArrayList();
                    string logMessage = "";

                    DST_DistributeTask distributeTask = dbContext.DST_DistributeTasks.FirstOrDefault(t => t.reqCode.Equals(sReqCode));

                    if (distributeTask != null)
                    {
                        string ptlToAgvServiceUrl = System.Configuration.ConfigurationManager.AppSettings["PtlToAgvServiceUrl"];
                        //发送信息
                        string sendInfo = "";
                        //AGV服务地址
                        string sURL = ptlToAgvServiceUrl + "/bindPodAndBerth";
                        //HTTP响应结果
                        string result = "";

                        //绑定配送任务
                        DST_BindOrUnBindTaskDto distributeTaskDto = new DST_BindOrUnBindTaskDto();
                        distributeTaskDto.reqCode = distributeTask.reqCode;
                        distributeTaskDto.reqTime = distributeTask.reqTime.ToString("yyyy-MM-dd HH:mm:ss");
                        distributeTaskDto.clientCode = distributeTask.clientCode;
                        distributeTaskDto.tokenCode = distributeTask.tokenCode;
                        distributeTaskDto.podCode = distributeTask.podCode;
                        distributeTaskDto.pointCode = distributeTask.startPosition;
                        distributeTaskDto.indBind = distributeTask.taskTyp;

                        //发送信息
                        sendInfo = JsonConvert.SerializeObject(distributeTaskDto);
                        //发送HTTP请求，并返回响应结果
                        result = HttpService.HttpPost(sURL, sendInfo);

                        if (!string.IsNullOrEmpty(result))
                        {
                            //实例化HTTP响应结果
                            DST_DistributeTaskResultDto distributeTaskResultDto = JsonConvert.DeserializeObject<DST_DistributeTaskResultDto>(result);
                            if (distributeTaskResultDto != null)
                            {
                                //新增配送任务结果
                                DST_DistributeTaskResult distributeTaskResult = new DST_DistributeTaskResult();
                                distributeTaskResult.code = distributeTaskResultDto.code;
                                distributeTaskResult.message = distributeTaskResultDto.message;
                                distributeTaskResult.reqCode = distributeTaskResultDto.reqCode;
                                distributeTaskResult.data = distributeTaskResultDto.data;
                                distributeTaskResult.receiveTime = DateTime.Now;

                                dbContext.DST_DistributeTaskResults.Add(distributeTaskResult);

                                //更新配送任务响应值
                                if (distributeTaskResultDto.code.Equals("0"))
                                {
                                    distributeTask.isResponse = true;

                                    retMsg = "success";

                                    #region 更新料架状态及停靠车位
                                    //更新料架状态及停靠车位
                                    int nAreaFlag = Convert.ToInt32(sAreaFlag); //0：料架缓冲区，1：拣料区，2：物料超市，3：生产线边
                                    int nPosition = Convert.ToInt32(sPosition);
                                    CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                                    if (cfgCart != null)
                                    {
                                        int cfgCartId = cfgCart.Id;
                                        string sCartCode = cfgCart.Code;
                                        if (IsBind) //绑定
                                        {
                                            if (nAreaFlag == 0) //空料架缓冲区
                                            {
                                                if (IsPTLPick)
                                                {
                                                    CartBinder.UnDockCart(cfgCartId);
                                                }

                                                logMessage = "料架" + sCartCode + "在空料架缓冲区绑定成功";
                                                arrNullCartAreaLog.Add(logMessage);

                                                //空闲
                                                cfgCart.CartStatus = CartStatus.Free;

                                                //logMessage = "料架" + sCartCode + "更新料架状态为" + CartStatus.Free;
                                                //arrNullCartAreaLog.Add(logMessage);

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
                                                    arrNullCartAreaLog.Add(logMessage);
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
                                                    arrNullCartAreaLog.Add(logMessage);
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
                                                    arrNullCartAreaLog.Add(logMessage);
                                                }

                                                ////结束装配任务
                                                //List<ASM_TaskItem> asmTaskItems = dbContext.ASM_TaskItems.Where(t => t.CFG_CartId == cfgCart.Id && t.AssembleStatus != AssembleStatus.Finished).ToList();
                                                //foreach (ASM_TaskItem asmTaskItem in asmTaskItems)
                                                //{
                                                //    asmTaskItem.AssembleStatus = AssembleStatus.Finished;
                                                //    asmTaskItem.AssembledQuantity = asmTaskItem.ToAssembleQuantity;
                                                //    asmTaskItem.AssembledTime = DateTime.Now;
                                                //}
                                            }
                                            else if (nAreaFlag == 1) //拣料区
                                            {
                                                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts.FirstOrDefault(t => t.CFG_Channel.Code.Equals(sAreaCode) && t.Position == nPosition);
                                                if (cfgChannelCurrentCart != null)
                                                {
                                                    //停靠即开始播种
                                                    cfgCart.CartStatus = CartStatus.WaitingAssorting;

                                                    //绑定当前巷道上的小车
                                                    cfgChannelCurrentCart.CFG_CartId = cfgCartId;
                                                    cfgChannelCurrentCart.DockedTime = DateTime.Now;

                                                    logMessage = "料架" + sCartCode + "在巷道H" + sAreaCode + "P" + nPosition + "绑定成功";
                                                    arrPickAreaLog.Add(logMessage);

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
                                                        arrPickAreaLog.Add(logMessage);
                                                    }

                                                    ////结束装配任务
                                                    //List<ASM_TaskItem> asmTaskItems = dbContext.ASM_TaskItems.Where(t => t.CFG_CartId == cfgCart.Id && t.AssembleStatus != AssembleStatus.Finished).ToList();
                                                    //foreach (ASM_TaskItem asmTaskItem in asmTaskItems)
                                                    //{
                                                    //    asmTaskItem.AssembleStatus = AssembleStatus.Finished;
                                                    //    asmTaskItem.AssembledQuantity = asmTaskItem.ToAssembleQuantity;
                                                    //    asmTaskItem.AssembledTime = DateTime.Now;
                                                    //}

                                                    if (IsPTLPick)
                                                    {
                                                        CartBinder.DockCart(cfgCartId, "停靠成功", cfgChannelCurrentCart.CFG_Channel.Name, nPosition, "位");
                                                    }
                                                }
                                            }
                                            else if (nAreaFlag == 2) //物料超市
                                            {
                                                logMessage = "料架" + sCartCode + "在物料超市" + sAreaCode + "绑定成功";
                                                arrMarketAreaLog.Add(logMessage);

                                                if (IsPTLPick)
                                                {
                                                    if (cfgCart.CartStatus < CartStatus.WaitingToWorkStation)
                                                    {
                                                        cfgCart.CartStatus = CartStatus.NeedToWorkStation;

                                                        FND_Task fndTask = dbContext.FND_Tasks.FirstOrDefault(t => t.CFG_CartId == cfgCart.Id && t.FindingStatus == DataAccess.CartFinding.FindingStatus.New);
                                                        if (fndTask != null)
                                                        {
                                                            fndTask.FindingStatus = DataAccess.CartFinding.FindingStatus.NeedDisplay;
                                                            fndTask.CFG_EmployeeId = 1;

                                                            logMessage = "料架" + cfgCart.Code + "更新寻车任务的状态为" + FindingStatus.NeedDisplay + "，批次：" + fndTask.BatchCode;
                                                            arrMarketAreaLog.Add(logMessage);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //已抵达缓存区
                                                    cfgCart.CartStatus = CartStatus.ArrivedAtBufferArea;

                                                    ////绑定物料超市料架
                                                    //CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations.FirstOrDefault(t => t.Code.Equals(sAreaCode));
                                                    //if (cfgWorkStation != null)
                                                    //{
                                                    //    CFG_MarketWorkStationCurrentCart cfgMarketWorkStationCurrentCart = new CFG_MarketWorkStationCurrentCart();
                                                    //    cfgMarketWorkStationCurrentCart.CFG_CartId = cfgCart.Id;
                                                    //    cfgMarketWorkStationCurrentCart.CFG_WorkStationId = cfgWorkStation.Id;
                                                    //    cfgMarketWorkStationCurrentCart.DockedTime = DateTime.Now;

                                                    //    dbContext.CFG_MarketWorkStationCurrentCarts.Add(cfgMarketWorkStationCurrentCart);

                                                    //    logMessage = "物料超市" + sAreaCode + "新增料架" + sCartCode + "的物料超市绑定关系";
                                                    //    arrMarketAreaLog.Add(logMessage);
                                                    //}
                                                }

                                                //物料超市停靠
                                                MarketZone marketZone = dbContext.MarketZones.FirstOrDefault(t => t.GroundId.Equals(sPonitCode));
                                                if (marketZone != null)
                                                {
                                                    marketZone.Status = 1;
                                                    marketZone.CFG_CartId = cfgCart.Id;
                                                    marketZone.DockedTime = DateTime.Now;

                                                    logMessage = "物料超市" + sAreaCode + "新增料架" + sCartCode + "的物料超市绑定关系";
                                                    arrMarketAreaLog.Add(logMessage);
                                                }
                                            }
                                            else if (nAreaFlag == 3) //生产线边
                                            {
                                                CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts.FirstOrDefault(t => t.CFG_WorkStation.Code.Equals(sAreaCode) && t.Position == nPosition);
                                                if (cfgWorkStationCurrentCart != null)
                                                {
                                                    //已抵达生产线边
                                                    cfgCart.CartStatus = CartStatus.ArrivedAtWorkStation;

                                                    //绑定当前工位上的小车
                                                    cfgWorkStationCurrentCart.CFG_CartId = cfgCartId;
                                                    cfgWorkStationCurrentCart.DockedTime = DateTime.Now;

                                                    if (IsPTLPick)
                                                    {
                                                        //设备控制
                                                        CFG_CartCurrentMaterial firstNotEmptyCfgCartCurrentMaterial = cfgCart.CFG_CartCurrentMaterials
                                                                                                                          .FirstOrDefault(ccm => ccm.AST_CartTaskItemId != null);
                                                        if (firstNotEmptyCfgCartCurrentMaterial != null)
                                                        {
                                                            CartBinder.DockCart(cfgCartId, "抵达工位 " + firstNotEmptyCfgCartCurrentMaterial.CFG_WorkStation.Name, string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
批次：{2}", firstNotEmptyCfgCartCurrentMaterial.ProjectCode, firstNotEmptyCfgCartCurrentMaterial.ProjectStep, firstNotEmptyCfgCartCurrentMaterial.BatchCode), nPosition, "位");
                                                        }
                                                    }

                                                    logMessage = "料架" + sCartCode + "在生产线边" + sAreaCode + "-" + nPosition + "绑定成功";
                                                    arrProductAreaLog.Add(logMessage);
                                                }
                                            }
                                        }
                                        else //解绑
                                        {
                                            //空闲
                                            cfgCart.CartStatus = CartStatus.Free;

                                            if (nAreaFlag == 0) //空料架缓冲区
                                            {
                                                if (IsPTLPick)
                                                {
                                                    CartBinder.UnDockCart(cfgCartId);
                                                }

                                                logMessage = "料架" + sCartCode + "在空料架缓冲区解绑成功";
                                                arrNullCartAreaLog.Add(logMessage);
                                            }
                                            else if (nAreaFlag == 1) //拣料区
                                            {
                                                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts.FirstOrDefault(t => t.CFG_CartId == cfgCartId);
                                                if (cfgChannelCurrentCart != null)
                                                {
                                                    cfgChannelCurrentCart.CFG_CartId = null;
                                                    cfgChannelCurrentCart.DockedTime = null;
                                                }

                                                logMessage = "料架" + sCartCode + "在巷道H" + sAreaCode + "P" + nPosition + "解绑成功";
                                                arrPickAreaLog.Add(logMessage);
                                            }
                                            else if (nAreaFlag == 2) //物料超市
                                            {
                                                //if (!IsPTLPick)
                                                //{
                                                //移除物料超市料架
                                                //List<CFG_MarketWorkStationCurrentCart> cfgMarketWorkStationCurrentCarts = dbContext.CFG_MarketWorkStationCurrentCarts.Where(t => t.CFG_WorkStation.Code.Equals(sAreaCode) && t.CFG_CartId == cfgCartId).ToList();
                                                //List<CFG_MarketWorkStationCurrentCart> cfgMarketWorkStationCurrentCarts = dbContext.CFG_MarketWorkStationCurrentCarts.Where(t => t.CFG_CartId == cfgCartId).ToList();
                                                //if (cfgMarketWorkStationCurrentCarts.Count > 0)
                                                //{
                                                //    dbContext.CFG_MarketWorkStationCurrentCarts.RemoveRange(cfgMarketWorkStationCurrentCarts);
                                                //}
                                                //}

                                                //移除物料超市停靠
                                                MarketZone marketZone = dbContext.MarketZones.FirstOrDefault(t => t.GroundId.Equals(sPonitCode));
                                                if (marketZone != null)
                                                {
                                                    marketZone.Status = 0;
                                                    marketZone.CFG_CartId = null;
                                                    marketZone.DockedTime = null;
                                                }

                                                logMessage = "料架" + sCartCode + "在物料超市" + sAreaCode + "解绑成功";
                                                arrMarketAreaLog.Add(logMessage);
                                            }
                                            else if (nAreaFlag == 3) //生产线边
                                            {
                                                CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts.FirstOrDefault(t => t.CFG_CartId == cfgCartId);
                                                if (cfgWorkStationCurrentCart != null)
                                                {
                                                    cfgWorkStationCurrentCart.CFG_CartId = null;
                                                    cfgWorkStationCurrentCart.DockedTime = null;
                                                }

                                                logMessage = "料架" + sCartCode + "在生产线边" + sAreaCode + "-" + nPosition + "解绑成功";
                                                arrProductAreaLog.Add(logMessage);
                                            }
                                        }

                                        foreach (string sLog in arrNullCartAreaLog)
                                        {
                                            Logger.Log("料架缓冲区绑定与解绑", DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                                        }

                                        foreach (string sLog in arrPickAreaLog)
                                        {
                                            Logger.Log("拣料区绑定与解绑", DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                                        }

                                        foreach (string sLog in arrMarketAreaLog)
                                        {
                                            Logger.Log("物料超市绑定与解绑", DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                                        }

                                        foreach (string sLog in arrProductAreaLog)
                                        {
                                            Logger.Log("生产线边绑定与解绑", DateTime.Now.ToString("HH:mm:ss") + sLog + Environment.NewLine);
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    distributeTask.sendErrorCount = distributeTask.sendErrorCount + 1;

                                    retMsg = distributeTaskResultDto.message;
                                }
                            }
                        }
                    }

                    //更新数据库
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                retMsg = ex.Message;
            }
            return retMsg;
        }

        /// <summary>
        /// 获取当前储位信息
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <returns></returns>
        public string GetCurrentPointInfoByCartRFID(string sCartRFID)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<MarketPointDto> marketPointDtos = new List<MarketPointDto>();
                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                if (cfgCart != null)
                {
                    DST_DistributeArriveResult dstDistributeArriveResult = dbContext.DST_DistributeArriveResults.Where(t => t.podCode.Equals(cfgCart.Code) && t.DistributeReqTypes == DistributeReqTypes.PickAreaDistribute).OrderByDescending(t => t.ID).FirstOrDefault();
                    if (dstDistributeArriveResult != null)
                    {
                        MarketPointDto marketPointDto = new MarketPointDto();
                        marketPointDto.PointCode = dstDistributeArriveResult.currentPointCode;
                        marketPointDto.WorkStationCode = dstDistributeArriveResult.endPosition;

                        marketPointDtos.Add(marketPointDto);
                    }
                }
                string result = JsonConvert.SerializeObject(marketPointDtos);
                return result;
            }
        }

        /// <summary>
        /// 生成PDA物料超市配送任务
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="sPointCode">储位编号</param>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateMarketDistributeTask(string sWorkStationCode, string sPointCode, string sCartRFID, bool IsPTLPick)
        {
            string result = DistributingTaskGenerator.Instance.GenerateMarketDistributeTask(sWorkStationCode, sPointCode, sCartRFID, IsPTLPick);
            return result;
        }

        /// <summary>
        /// 生成单个线边里侧到外侧任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateSingleProductInToOutTask(string sCartRFID, bool IsPTLPick)
        {
            string result = DistributingTaskGenerator.Instance.GenerateSingleProductInToOutTask(sCartRFID, IsPTLPick);
            return result;
        }

        /// <summary>
        /// 生成单个空料架返回任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateSingleNullCartBackTask(string sCartRFID, bool IsPTLPick)
        {
            string result = DistributingTaskGenerator.Instance.GenerateSingleNullCartBackTask(sCartRFID, IsPTLPick);
            return result;
        }

        /// <summary>
        /// 获取工位信息
        /// </summary>
        /// <returns></returns>
        public string GetWorkStationInfo()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<WorkStationDto> workStationDtos = new List<WorkStationDto>();

                List<CFG_WorkStation> cfgWorkStations = dbContext.CFG_WorkStations.OrderBy(t => t.Code).ToList();
                foreach (CFG_WorkStation cfgWorkStation in cfgWorkStations)
                {
                    WorkStationDto workStationDto = new WorkStationDto();
                    workStationDto.Id = cfgWorkStation.Id;
                    workStationDto.Code = cfgWorkStation.Code;
                    workStationDto.Name = cfgWorkStation.Name;

                    workStationDtos.Add(workStationDto);
                }

                string result = JsonConvert.SerializeObject(workStationDtos);
                return result;
            }
        }

        /// <summary>
        /// 生成生产线边铺线任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边铺线的工位ID</param>
        /// <returns></returns>
        public string GenerateProductAreaInitTask(string InitWorkStationIds)
        {
            string[] arrInitWorkStationId = InitWorkStationIds.Split(',');
            List<int> listInitWorkStationId = new List<int>();
            foreach (string sWorkStationId in arrInitWorkStationId)
            {
                listInitWorkStationId.Add(Convert.ToInt32(sWorkStationId));
            }
            string result = DistributingTaskGenerator.Instance.GenerateProductAreaInitTask(listInitWorkStationId);
            return result;
        }

        /// <summary>
        /// 获取工位信息
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <returns></returns>
        public string GetWorkStationInfoByCartRFID(string sCartRFID)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<WorkStationDto> workStationDtos = new List<WorkStationDto>();

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                if (cfgCart != null)
                {
                    CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts.FirstOrDefault(t => t.CFG_CartId == cfgCart.Id);
                    if (cfgWorkStationCurrentCart != null)
                    {
                        CFG_WorkStation cfgWorkStation = cfgWorkStationCurrentCart.CFG_WorkStation;

                        WorkStationDto workStationDto = new WorkStationDto();
                        workStationDto.Id = cfgWorkStation.Id;
                        workStationDto.Code = cfgWorkStation.Code;
                        workStationDto.Name = cfgWorkStation.Name;

                        workStationDtos.Add(workStationDto);
                    }
                }
                string result = JsonConvert.SerializeObject(workStationDtos);
                return result;
            }
        }

        /// <summary>
        /// 获取可以清线的线边工位信息
        /// </summary>
        /// <returns></returns>
        public string GetProductAreaClearWorkStationInfo()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<WorkStationDto> workStationDtos = new List<WorkStationDto>();

                List<CFG_WorkStation> cfgWorkStations = dbContext.CFG_WorkStationCurrentCarts.Where(t => t.CFG_CartId != null && t.Position <= 4).Select(t => t.CFG_WorkStation).Distinct().OrderBy(w => w.Code).ToList();

                if (cfgWorkStations.Count > 0)
                {
                    foreach (CFG_WorkStation cfgWorkStation in cfgWorkStations)
                    {
                        WorkStationDto workStationDto = new WorkStationDto();
                        workStationDto.Id = cfgWorkStation.Id;
                        workStationDto.Code = cfgWorkStation.Code;
                        workStationDto.Name = cfgWorkStation.Name;

                        workStationDtos.Add(workStationDto);
                    }
                }
                string result = JsonConvert.SerializeObject(workStationDtos);
                return result;
            }
        }

        /// <summary>
        /// 获取可以进行空料架返回的工位信息
        /// </summary>
        /// <returns></returns>
        public string GetNullCartBackWorkStationInfo()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<WorkStationDto> workStationDtos = new List<WorkStationDto>();

                List<CFG_WorkStation> cfgWorkStations = dbContext.CFG_WorkStationCurrentCarts.Where(t => t.CFG_CartId != null && t.Position > 4).Select(t => t.CFG_WorkStation).Distinct().OrderBy(w => w.Code).ToList();

                if (cfgWorkStations.Count > 0)
                {
                    foreach (CFG_WorkStation cfgWorkStation in cfgWorkStations)
                    {
                        WorkStationDto workStationDto = new WorkStationDto();
                        workStationDto.Id = cfgWorkStation.Id;
                        workStationDto.Code = cfgWorkStation.Code;
                        workStationDto.Name = cfgWorkStation.Name;

                        workStationDtos.Add(workStationDto);
                    }
                }
                string result = JsonConvert.SerializeObject(workStationDtos);
                return result;
            }
        }

        /// <summary>
        /// 批量生成线边里侧到外侧任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边清线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateBatchProductInToOutTask(string InitWorkStationIds, bool IsPTLPick)
        {
            string[] arrInitWorkStationId = InitWorkStationIds.Split(',');
            List<int> listInitWorkStationId = new List<int>();
            foreach (string sWorkStationId in arrInitWorkStationId)
            {
                listInitWorkStationId.Add(Convert.ToInt32(sWorkStationId));
            }
            string result = DistributingTaskGenerator.Instance.GenerateBatchProductInToOutTask(listInitWorkStationId, IsPTLPick);
            return result;
        }

        /// <summary>
        /// 批量生成空料架返回任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边清线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateBatchNullCartBackTask(string InitWorkStationIds, bool IsPTLPick)
        {
            string[] arrInitWorkStationId = InitWorkStationIds.Split(',');
            List<int> listInitWorkStationId = new List<int>();
            foreach (string sWorkStationId in arrInitWorkStationId)
            {
                listInitWorkStationId.Add(Convert.ToInt32(sWorkStationId));
            }
            string result = DistributingTaskGenerator.Instance.GenerateBatchNullCartBackTask(listInitWorkStationId, IsPTLPick);
            return result;
        }

        /// <summary>
        /// 生成单个拣料区铺线任务
        /// </summary>
        /// <param name="sChannelCode">巷道编码</param>
        /// <param name="sPosition">车位</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateSinglePickAreaInitTask(string sChannelCode, string sPosition, bool IsPTLPick)
        {
            string result = DistributingTaskGenerator.Instance.GenerateSinglePickAreaInitTask(sChannelCode, sPosition, IsPTLPick);
            return result;
        }

        /// <summary>
        /// 手动生成料架转换任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        public string GenerateHandProductCartSwitchTask(string sCartRFID, bool IsPTLPick)
        {
            string result = DistributingTaskGenerator.Instance.GenerateHandProductCartSwitchTask(sCartRFID, IsPTLPick);
            return result;
        }

        /// <summary>
        /// 获取巷道车位信息
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <returns></returns>
        public string GetChannelPositionInfoByCartRFID(string sCartRFID)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<ChannelCurrentCartDto> channelCurrentCartDtos = new List<ChannelCurrentCartDto>();

                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts.FirstOrDefault(t => t.CFG_Cart.Rfid1.Equals(sCartRFID));
                if (cfgChannelCurrentCart != null)
                {
                    ChannelCurrentCartDto channelCurrentCartDto = new ChannelCurrentCartDto();
                    channelCurrentCartDto.ChannelCode = cfgChannelCurrentCart.CFG_Channel.Code;
                    channelCurrentCartDto.Position = cfgChannelCurrentCart.Position.ToString();

                    channelCurrentCartDtos.Add(channelCurrentCartDto);
                }
                string result = JsonConvert.SerializeObject(channelCurrentCartDtos);
                return result;
            }
        }

        /// <summary>
        /// 获取工位车位信息
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <returns></returns>
        public string GetWorkStationPositionInfoByCartRFID(string sCartRFID)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<WorkStationCurrentCartDto> workStationCurrentCartDtos = new List<WorkStationCurrentCartDto>();

                CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts.FirstOrDefault(t => t.CFG_Cart.Rfid1.Equals(sCartRFID));
                if (cfgWorkStationCurrentCart != null)
                {
                    WorkStationCurrentCartDto workStationCurrentCartDto = new WorkStationCurrentCartDto();
                    workStationCurrentCartDto.WorkStationCode = cfgWorkStationCurrentCart.CFG_WorkStation.Code;
                    workStationCurrentCartDto.Position = cfgWorkStationCurrentCart.Position.ToString();

                    workStationCurrentCartDtos.Add(workStationCurrentCartDto);
                }
                string result = JsonConvert.SerializeObject(workStationCurrentCartDtos);
                return result;
            }
        }

        /// <summary>
        /// 批量生成物料超市配送任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边铺线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateBatchMarketDistributeTask(string InitWorkStationIds, bool IsPTLPick)
        {
            string[] arrInitWorkStationId = InitWorkStationIds.Split(',');
            List<int> listInitWorkStationId = new List<int>();
            foreach (string sWorkStationId in arrInitWorkStationId)
            {
                listInitWorkStationId.Add(Convert.ToInt32(sWorkStationId));
            }
            string result = DistributingTaskGenerator.Instance.GenerateBatchMarketDistributeTask(listInitWorkStationId, IsPTLPick);
            return result;
        }

        /// <summary>
        /// 获取物料超市料架信息
        /// </summary>
        /// <param name="sWorkStationID">工位ID</param>
        /// <returns></returns>
        public string GetMarketCartByWorkStationID(string sWorkStationID)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<MarketCartDto> marketCartDtos = new List<MarketCartDto>();

                int nWorkStationID = Convert.ToInt32(sWorkStationID);
                //List<CFG_MarketWorkStationCurrentCart> cfgMarketWorkStationCurrentCarts = dbContext.CFG_MarketWorkStationCurrentCarts.Where(t => t.CFG_WorkStationId == nWorkStationID).ToList();
                //if (cfgMarketWorkStationCurrentCarts.Count > 0)
                //{
                //    foreach (CFG_MarketWorkStationCurrentCart cfgMarketWorkStationCurrentCart in cfgMarketWorkStationCurrentCarts)
                //    {
                //        MarketCartDto marketCartDto = new MarketCartDto();
                //        marketCartDto.Id = cfgMarketWorkStationCurrentCart.Id;
                //        marketCartDto.WorkStationCode = cfgMarketWorkStationCurrentCart.CFG_WorkStation.Code;
                //        marketCartDto.CartCode = cfgMarketWorkStationCurrentCart.CFG_Cart.Code;

                //        marketCartDtos.Add(marketCartDto);
                //    }
                //}

                CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations.FirstOrDefault(t => t.Id == nWorkStationID);
                if (cfgWorkStation != null)
                {
                    List<MarketZone> marketZones = dbContext.MarketZones.Where(t => t.AreaId.Equals(cfgWorkStation.Code)).ToList();
                    if (marketZones.Count > 0)
                    {
                        foreach (MarketZone marketZone in marketZones)
                        {
                            MarketCartDto marketCartDto = new MarketCartDto();
                            marketCartDto.Id = marketZone.Id;
                            marketCartDto.WorkStationCode = marketZone.AreaId;
                            marketCartDto.CartCode = dbContext.CFG_Carts.FirstOrDefault(t => t.Id == marketZone.CFG_CartId).Code;

                            marketCartDtos.Add(marketCartDto);
                        }
                    }
                }
                string result = JsonConvert.SerializeObject(marketCartDtos);
                return result;
            }
        }

        /// <summary>
        /// 删除物料超市料架信息
        /// </summary>
        /// <param name="sDelID">物料超市记录ID</param>
        /// <returns></returns>
        public string DeleteMarketCart(string sDelID)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string[] arrDelID = sDelID.Split(',');
                List<long> listDelID = new List<long>();
                foreach (string sID in arrDelID)
                {
                    listDelID.Add(long.Parse(sID));
                }

                List<CFG_MarketWorkStationCurrentCart> cfgMarketWorkStationCurrentCarts = dbContext.CFG_MarketWorkStationCurrentCarts.Where(t => listDelID.Contains(t.Id)).ToList();
                if (cfgMarketWorkStationCurrentCarts.Count > 0)
                {
                    dbContext.CFG_MarketWorkStationCurrentCarts.RemoveRange(cfgMarketWorkStationCurrentCarts);
                }

                return dbContext.SaveChanges() > 0 ? "删除成功" : "删除失败";
            }
        }

        /// <summary>
        /// 获取配送任务
        /// </summary>
        /// <param name="MinTime">任务生成开始时间</param>
        /// <param name="MaxTime">任务生成结束时间</param>
        /// <param name="ReqType">配送任务类型</param>
        /// <param name="Response">是否响应</param>
        /// <param name="Arrive">是否到达</param>
        /// <returns></returns>
        public string GetDistributeTask(string MinTime, string MaxTime, string ReqType, string Response, string Arrive)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                DateTime dMinTime = Convert.ToDateTime(MinTime);
                DateTime dMaxTime = Convert.ToDateTime(MaxTime).AddDays(1);

                IQueryable<DST_DistributeTask> queryable = dbContext.DST_DistributeTasks.Where(lt => lt.reqTime > dMinTime && lt.reqTime < dMaxTime);

                if (!string.IsNullOrEmpty(ReqType) && !ReqType.Equals("0"))
                {
                    DistributeReqTypes distributeReqTypes = ConvertOperate.ReqTextToTypeConvert(ReqType);
                    queryable = queryable.Where(lt => lt.DistributeReqTypes == distributeReqTypes);
                }

                if (!string.IsNullOrEmpty(Response) && !Response.Equals("0"))
                {
                    bool IsResponse = false;
                    if (Response.Equals("是"))
                    {
                        IsResponse = true;
                    }
                    else if (Response.Equals("否"))
                    {
                        IsResponse = false;
                    }
                    queryable = queryable.Where(lt => lt.isResponse == IsResponse);
                }

                if (!string.IsNullOrEmpty(Arrive) && !Arrive.Equals("0"))
                {
                    if (Arrive.Equals("是"))
                    {
                        queryable = queryable.Where(lt => lt.arriveTime != null);
                    }
                    else if (Arrive.Equals("否"))
                    {
                        queryable = queryable.Where(lt => lt.arriveTime == null);
                    }
                }

                List<DST_DistributeTask> dstDistributeTasks = queryable.ToList();

                string result = JsonConvert.SerializeObject(dstDistributeTasks);
                return result;
            }
        }

        /// <summary>
        /// 配送任务重发
        /// </summary>
        /// <param name="DistributeTaskIds">配送任务ID</param>
        /// <returns></returns>
        public string ReSendDistributeTask(string DistributeTaskIds)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                string[] arrDistributeTaskId = DistributeTaskIds.Split(',');
                List<long> SelDistributeTaskIds = new List<long>();
                foreach (string sDistributeTaskId in arrDistributeTaskId)
                {
                    SelDistributeTaskIds.Add(long.Parse(sDistributeTaskId));
                }

                List<DST_DistributeTask> dstDistributeTasks = dbContext.DST_DistributeTasks.Where(t => SelDistributeTaskIds.Contains(t.ID) && !t.isResponse && t.sendErrorCount >= 5).ToList();
                if (dstDistributeTasks.Count == 0)
                {
                    result = "选择的配送任务中没有未响应且发送错误次数大于5的任务";
                    return result;
                }
                foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                {
                    dstDistributeTask.sendErrorCount = 0;
                }

                result = dbContext.SaveChanges() > 0 ? "重发成功" : "重发失败";
                return result;
            }
        }

        /// <summary>
        /// 结束配送任务
        /// </summary>
        /// <param name="DistributeTaskIds">配送任务ID</param>
        /// <returns></returns>
        public string StopDistributeTask(string DistributeTaskIds)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                string[] arrDistributeTaskId = DistributeTaskIds.Split(',');
                List<long> SelDistributeTaskIds = new List<long>();
                foreach (string sDistributeTaskId in arrDistributeTaskId)
                {
                    SelDistributeTaskIds.Add(long.Parse(sDistributeTaskId));
                }

                List<DST_DistributeTask> dstDistributeTasks = dbContext.DST_DistributeTasks.Where(t => SelDistributeTaskIds.Contains(t.ID)).ToList();
                //if (dstDistributeTasks.Count > 0)
                //{
                //    foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                //    {
                //        if (dstDistributeTask.DistributeReqTypes != DistributeReqTypes.ProductAreaInit && dstDistributeTask.DistributeReqTypes != DistributeReqTypes.MaterialMarketDistribute)
                //        {
                //            result = "选择的配送任务中存在不为生产线边铺线和物料超市配送的任务，不能强制结束";
                //            return result;
                //        }
                //    }
                //}

                if (dstDistributeTasks.Count > 0)
                {
                    foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                    {
                        dstDistributeTask.sendErrorCount = 5;
                    }
                }

                result = dbContext.SaveChanges() > 0 ? "强制结束配送任务成功" : "强制结束配送任务失败";
                return result;
            }
        }

        /// <summary>
        /// 生成点对点配送任务
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="StartPoint">起始储位</param>
        /// <param name="EndPoint">目标储位</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="AreaFlag">区域标识（0：料架缓冲区，1：物料超市，2：其他区域）</param>
        /// <returns></returns>
        public string GeneratePointToPointDistribute(string CartRFID, string StartPoint, string EndPoint, bool IsPTLPick, string AreaFlag)
        {
            string result = DistributingTaskGenerator.Instance.GeneratePointToPointDistribute(CartRFID, StartPoint, EndPoint, IsPTLPick, AreaFlag);
            return result;
        }

        /// <summary>
        /// 获取PTL配送工位
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        public string GetPTLWorkStationByCart(string CartRFID)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(CartRFID));
                if (cfgCart != null)
                {
                    CFG_WorkStation cfgWorkStation = dbContext.CFG_CartCurrentMaterials.Where(t => t.CFG_CartId == cfgCart.Id && t.CFG_WorkStationId != null).Select(t => t.CFG_WorkStation).FirstOrDefault();
                    if (cfgWorkStation != null)
                    {
                        result = cfgWorkStation.Code;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 更新巷道料架停靠时间
        /// </summary>
        /// <returns></returns>
        public string UpdateChannelCurrentCartDockedTime()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "更新成功";

                List<CFG_ChannelCurrentCart> cfgChannelCurrentCarts = dbContext.CFG_ChannelCurrentCarts.Where(t => t.CFG_CartId != null).ToList();
                if (cfgChannelCurrentCarts.Count > 0)
                {
                    foreach (CFG_ChannelCurrentCart cfgChannelCurrentCart in cfgChannelCurrentCarts)
                    {
                        cfgChannelCurrentCart.DockedTime = DateTime.Now;
                        Thread.Sleep(5);
                    }
                    result = dbContext.SaveChanges() > 0 ? "更新成功" : "更新失败";
                }
                return result;
            }
        }

        /// <summary>
        /// 获取料架信息
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        public string GetCartInfoByRFID(string CartRFID)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<CartDto> cartDtos = new List<CartDto>();

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(CartRFID));
                if (cfgCart != null)
                {
                    string sOnline = "小车 " + cfgCart.Code + " 在线";
                    if (!cfgCart.OnLine)
                    {
                        sOnline = "小车 " + cfgCart.Code + " 不在线";
                    }
                    else
                    {
                        foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                        {
                            if (!cfgCartPtlDevice.OnLine)
                            {
                                sOnline = "小车上的 " + cfgCartPtlDevice.DeviceAddress + " 号标签不在线";
                                break;
                            }
                        }
                    }

                    CartDto cartDto = new CartDto();
                    cartDto.CartCode = cfgCart.Code;
                    cartDto.CartStatus = ConvertOperate.CartStatusToTextConvert(cfgCart.CartStatus);
                    cartDto.OnlineInfo = sOnline;

                    cartDtos.Add(cartDto);
                }

                string result = JsonConvert.SerializeObject(cartDtos);
                return result;
            }
        }
    }
}