using DataAccess;
using DataAccess.Assorting;
using DataAccess.Config;
using DataAccess.Distributing;
using DataAccess.Other;
using Distributing.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distributing
{
    public class DistributingTaskGenerator
    {
        static readonly DistributingTaskGenerator instance = new DistributingTaskGenerator();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static DistributingTaskGenerator Instance
        {
            get { return DistributingTaskGenerator.instance; }
        }

        DistributingTaskGenerator()
        { }

        private const string NullCartArea = "C1${02}"; //空料架缓冲区

        private const string NullCartAreaToPickArea = "ptl01"; //空料架缓冲区到拣料区
        private const string PickAreaToMarketArea = "ptl02"; //拣料区到物料超市
        private const string MarketAreaToProductArea = "ptl03"; //物料超市到线边
        //private const string MarketAreaToProductArea = "m05"; //物料超市到线边
        private const string ProductCartSwitch = "ptl05"; //线边料架转换
        private const string NullCartBack = "ptl06"; //空料架返回空料架缓冲区
        private const string ProductPositionSwitch = "F01"; //线边位置转换
        private const string PointToNullCartArea = "ptl07"; //点到空料架缓冲区

        /// <summary>
        /// 生成拣料区铺线任务
        /// </summary>
        /// <param name="InitChannelIds">拣料区铺线的巷道ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public bool GeneratePickAreaInitTask(List<int> InitChannelIds, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string startPosition = NullCartArea;
                string endPosition = "";
                bool isNullChannel = false;
                string sTaskType = IsPTLPick ? "PTL" : "PDA";

                List<CFG_Channel> cfgChannels = dbContext.CFG_Channels.Where(t => InitChannelIds.Contains(t.Id)).ToList();
                foreach (CFG_Channel cfgchannel in cfgChannels)
                {
                    isNullChannel = cfgchannel.CFG_ChannelCurrentCarts.All(t => t.CFG_CartId == null);
                    if (isNullChannel)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            endPosition = "H" + cfgchannel.Code + "P" + i;  //巷道1车位1

                            DST_DistributeTask distributeTask = new DST_DistributeTask();
                            distributeTask.reqCode = GetReqCode();
                            distributeTask.reqTime = DateTime.Now;
                            distributeTask.clientCode = "";
                            distributeTask.tokenCode = "";
                            distributeTask.taskTyp = NullCartAreaToPickArea;
                            distributeTask.userCallCode = "";
                            distributeTask.taskGroupCode = "";
                            distributeTask.startPosition = startPosition;
                            distributeTask.endPosition = endPosition;
                            distributeTask.podCode = "";
                            distributeTask.podDir = "手动";
                            distributeTask.priority = 1;
                            distributeTask.robotCode = "";
                            distributeTask.taskCode = "";
                            distributeTask.data = sTaskType;
                            distributeTask.DistributeReqTypes = DistributeReqTypes.PickAreaInit;
                            distributeTask.isResponse = false;
                            distributeTask.sendErrorCount = 0;

                            dbContext.DST_DistributeTasks.Add(distributeTask);
                        }
                    }
                }

                return dbContext.SaveChanges() > 0 ? true : false;
            }
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
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                string startPosition = NullCartArea;
                string endPosition = "";
                string sTaskType = IsPTLPick ? "PTL" : "PDA";

                CFG_Channel cfgChannel = dbContext.CFG_Channels.FirstOrDefault(t => t.Code.Equals(sChannelCode));
                if (cfgChannel == null)
                {
                    result = "没有找到对应的巷道，不能铺线";
                    return result;
                }

                int nPosition = Convert.ToInt32(sPosition);
                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts.FirstOrDefault(t => t.CFG_ChannelId == cfgChannel.Id && t.Position == nPosition);
                if (cfgChannelCurrentCart == null)
                {
                    result = "没有找到巷道的对应车位，不能铺线";
                    return result;
                }

                if (cfgChannelCurrentCart.CFG_CartId != null)
                {
                    result = "巷道" + sChannelCode + "的" + sPosition + "车位已经停靠了车辆，不能铺线";
                    return result;
                }

                endPosition = "H" + cfgChannel.Code + "P" + sPosition;  //巷道1车位1

                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                distributeTask.taskTyp = NullCartAreaToPickArea;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = startPosition;
                distributeTask.endPosition = endPosition;
                distributeTask.podCode = "";
                distributeTask.podDir = "手动";
                distributeTask.priority = 1;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = sTaskType;
                distributeTask.DistributeReqTypes = DistributeReqTypes.PickAreaInit;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;

                dbContext.DST_DistributeTasks.Add(distributeTask);

                return dbContext.SaveChanges() > 0 ? "拣料区铺线成功" : "保存失败";
            }
        }

        /// <summary>
        /// 生成生产线边铺线任务（方法不用）
        /// </summary>
        /// <param name="InitWorkStationIds">线边铺线的工位ID</param>
        /// <returns></returns>
        public string GenerateProductAreaInitTask(List<int> InitWorkStationIds)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";
                /*
                string sNoFullWorkStation = "";
                string startPosition = "";
                string endPosition = "";
                bool isHaveCanInitWorkStation = false; //是否存在可以铺线的工位

                List<string> InitWorkStationCodes = new List<string>();
                List<CFG_WorkStation> cfgInitWorkStations = dbContext.CFG_WorkStations.Where(t => InitWorkStationIds.Contains(t.Id)).ToList();
                foreach (CFG_WorkStation cfgWorkStation in cfgInitWorkStations)
                {
                    InitWorkStationCodes.Add(cfgWorkStation.Code);
                }

                bool isHaveNoCompletedTask = dbContext.DST_DistributeTasks.Any(t => (t.DistributeReqTypes == DistributeReqTypes.ProductAreaInit || t.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute) && t.arriveTime == null && t.sendErrorCount < 5 && InitWorkStationCodes.Contains(t.startPosition));
                if (isHaveNoCompletedTask)
                {
                    result = "选择工位中存在未完成的生产线边铺线或物料超市配送任务，暂时不能再铺线";
                    return result;
                }

                bool isWorkStationFull = dbContext.CFG_WorkStationCurrentCarts.All(t => InitWorkStationIds.Contains(t.CFG_WorkStationId) && t.Position>4 && t.CFG_CartId != null);

                if (isWorkStationFull)
                {
                    result = "选择工位中存在外侧车位已经铺满了的线边工位，不能再铺线";
                }
                else
                {
                    List<CFG_WorkStation> cfgWorkStations = dbContext.CFG_WorkStations.Where(t => InitWorkStationIds.Contains(t.Id)).ToList();
                    int nMaterialMarketDockCart = 0;
                    foreach (CFG_WorkStation cfgWorkStation in cfgWorkStations)
                    {
                        List<CFG_Cart> cfgCarts = cfgWorkStation.CFG_CartCurrentMaterials.
                            Where(t => t.AST_CartTaskItemId != null).
                            Select(t => t.CFG_Cart).Distinct().
                            Where(c => c.CartStatus == CartStatus.ArrivedAtBufferArea
                            || c.CartStatus == CartStatus.NeedToWorkStation
                            || c.CartStatus == CartStatus.WaitingToWorkStation).ToList();

                        nMaterialMarketDockCart = cfgCarts.Count;

                        //if (nMaterialMarketDockCart < 4)
                        if (nMaterialMarketDockCart < 2)
                        {
                            sNoFullWorkStation += cfgWorkStation.Code + ",";
                        }
                        else
                        {
                            isHaveCanInitWorkStation = true;
                            //for (int i = 1; i <= 4; i++)
                            for (int i = 1; i <= 2; i++)
                            {
                                startPosition = cfgWorkStation.Code;
                                endPosition = cfgWorkStation.Code + "-" + (i + 6); //5、6、7、8

                                DST_DistributeTask distributeTask = new DST_DistributeTask();
                                distributeTask.reqCode = GetReqCode();
                                distributeTask.reqTime = DateTime.Now;
                                distributeTask.clientCode = "";
                                distributeTask.tokenCode = "";
                                distributeTask.taskTyp = MarketAreaToProductArea;
                                distributeTask.userCallCode = "";
                                distributeTask.taskGroupCode = "";
                                distributeTask.startPosition = startPosition;
                                distributeTask.endPosition = endPosition;
                                distributeTask.podCode = "";
                                distributeTask.podDir = "手动";
                                distributeTask.priority = 1;
                                distributeTask.robotCode = "";
                                distributeTask.taskCode = "";
                                distributeTask.data = "PTL";
                                distributeTask.DistributeReqTypes = DistributeReqTypes.ProductAreaInit;
                                distributeTask.isResponse = false;
                                distributeTask.sendErrorCount = 0;

                                dbContext.DST_DistributeTasks.Add(distributeTask);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(sNoFullWorkStation))
                {
                    //result = "物料超市工位:" + sNoFullWorkStation.Trim(',') + "没有停满4辆料架，暂时不能铺线";
                    result = "物料超市工位:" + sNoFullWorkStation.Trim(',') + "没有停满2辆料架，暂时不能铺线";
                }
                else
                {
                    if (!isHaveCanInitWorkStation)
                    {
                        result = "没有可以铺线的物料超市工位";
                    }
                    else
                    {
                        result = dbContext.SaveChanges() > 0 ? "生产线边铺线成功" : "保存失败";
                    }
                }
                 */
                return result;
            }
        }

        /// <summary>
        /// 生成拣料区配送任务
        /// </summary>
        /// <param name="cfgCart">当前料架信息</param>
        public List<DST_DistributeTask> GeneratePickAreaDistributeTask(CFG_Cart cfgCart)
        {
            List<DST_DistributeTask> distributeTasks = new List<DST_DistributeTask>();

            CFG_ChannelCurrentCart cfgChannelCurrentCart = cfgCart.CFG_ChannelCurrentCarts.FirstOrDefault();
            //CFG_WorkStation cfgWorkStation = cfgCart.AST_CartTasks.OrderByDescending(t => t.Id).FirstOrDefault().CFG_WorkStation;
            CFG_WorkStation cfgWorkStation = cfgCart.CFG_CartCurrentMaterials.FirstOrDefault(t => t.CFG_WorkStationId != null).CFG_WorkStation;

            if (cfgChannelCurrentCart != null && cfgWorkStation != null)
            {
                string sPickAreaPosition = "H" + cfgChannelCurrentCart.CFG_Channel.Code + "P" + cfgChannelCurrentCart.Position;
                string sMaterialMarketPosition = cfgWorkStation.Code;
                string sNullCartPosition = NullCartArea;

                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                distributeTask.taskTyp = PickAreaToMarketArea;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = sPickAreaPosition;
                distributeTask.endPosition = sMaterialMarketPosition;
                distributeTask.podCode = cfgCart.Code;
                distributeTask.podDir = "自动";
                distributeTask.priority = 2;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = "PTL";
                distributeTask.DistributeReqTypes = DistributeReqTypes.PickAreaDistribute;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;
                distributeTasks.Add(distributeTask);

                DST_DistributeTask distributeTask2 = new DST_DistributeTask();
                distributeTask2.reqCode = GetReqCode();
                distributeTask2.reqTime = DateTime.Now;
                distributeTask2.clientCode = "";
                distributeTask2.tokenCode = "";
                distributeTask2.taskTyp = NullCartAreaToPickArea;
                distributeTask2.userCallCode = "";
                distributeTask2.taskGroupCode = "";
                distributeTask2.startPosition = sNullCartPosition;
                distributeTask2.endPosition = sPickAreaPosition;
                distributeTask2.podCode = "";
                distributeTask2.podDir = "自动";
                distributeTask2.priority = 1;
                distributeTask2.robotCode = "";
                distributeTask2.taskCode = "";
                distributeTask2.data = "PTL";
                distributeTask2.DistributeReqTypes = DistributeReqTypes.NullCartAreaDistribute;
                distributeTask2.isResponse = false;
                distributeTask2.sendErrorCount = 0;
                distributeTasks.Add(distributeTask2);
            }
            return distributeTasks;
        }

        /// <summary>
        /// 手动生成拣料区配送任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="sWorkStationCode">工位编码</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public bool GenerateHandPickAreaDistributeTask(string sCartRFID, string sWorkStationCode, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                bool result = false;

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                if (cfgCart != null)
                {
                    CFG_ChannelCurrentCart cfgChannelCurrentCart = cfgCart.CFG_ChannelCurrentCarts.FirstOrDefault();

                    if (cfgChannelCurrentCart != null)
                    {
                        string sPickAreaPosition = "H" + cfgChannelCurrentCart.CFG_Channel.Code + "P" + cfgChannelCurrentCart.Position;
                        string sMaterialMarketPosition = sWorkStationCode;
                        string sNullCartPosition = NullCartArea;
                        string sTaskType = IsPTLPick ? "PTL" : "PDA";

                        DST_DistributeTask distributeTask = new DST_DistributeTask();
                        distributeTask.reqCode = GetReqCode();
                        distributeTask.reqTime = DateTime.Now;
                        distributeTask.clientCode = "";
                        distributeTask.tokenCode = "";
                        distributeTask.taskTyp = PickAreaToMarketArea;
                        distributeTask.userCallCode = "";
                        distributeTask.taskGroupCode = "";
                        distributeTask.startPosition = sPickAreaPosition;
                        distributeTask.endPosition = sMaterialMarketPosition;
                        distributeTask.podCode = cfgCart.Code;
                        distributeTask.podDir = "手动";
                        distributeTask.priority = 2;
                        distributeTask.robotCode = "";
                        distributeTask.taskCode = "";
                        distributeTask.data = sTaskType;
                        distributeTask.DistributeReqTypes = DistributeReqTypes.PickAreaDistribute;
                        distributeTask.isResponse = false;
                        distributeTask.sendErrorCount = 0;
                        dbContext.DST_DistributeTasks.Add(distributeTask);

                        DST_DistributeTask distributeTask2 = new DST_DistributeTask();
                        distributeTask2.reqCode = GetReqCode();
                        distributeTask2.reqTime = DateTime.Now;
                        distributeTask2.clientCode = "";
                        distributeTask2.tokenCode = "";
                        distributeTask2.taskTyp = NullCartAreaToPickArea;
                        distributeTask2.userCallCode = "";
                        distributeTask2.taskGroupCode = "";
                        distributeTask2.startPosition = sNullCartPosition;
                        distributeTask2.endPosition = sPickAreaPosition;
                        distributeTask2.podCode = "";
                        distributeTask2.podDir = "手动";
                        distributeTask2.priority = 1;
                        distributeTask2.robotCode = "";
                        distributeTask2.taskCode = "";
                        distributeTask2.data = sTaskType;
                        distributeTask2.DistributeReqTypes = DistributeReqTypes.NullCartAreaDistribute;
                        distributeTask2.isResponse = false;
                        distributeTask2.sendErrorCount = 0;
                        dbContext.DST_DistributeTasks.Add(distributeTask2);

                        //cfgChannelCurrentCart.CFG_CartId = null;
                        //cfgChannelCurrentCart.DockedTime = null;

                        result = dbContext.SaveChanges() > 0 ? true : false;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 生成料架转换任务
        /// </summary>
        /// <param name="cfgCart">当前料架信息</param>
        /// <returns></returns>
        public List<DST_DistributeTask> GenerateProductCartSwitchTask(CFG_Cart cfgCart)
        {
            List<DST_DistributeTask> distributeTasks = new List<DST_DistributeTask>();

            CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = cfgCart.CFG_WorkStationCurrentCarts.FirstOrDefault();
            if (cfgWorkStationCurrentCart != null)
            {
                string startPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + cfgWorkStationCurrentCart.Position;
                string endPosition = NullCartArea;

                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                distributeTask.taskTyp = ProductCartSwitch;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = startPosition;
                distributeTask.endPosition = endPosition;
                distributeTask.podCode = cfgCart.Code;
                distributeTask.podDir = "自动";
                distributeTask.priority = 2;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = "PTL";
                distributeTask.DistributeReqTypes = DistributeReqTypes.ProductCartSwitch;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;
                distributeTasks.Add(distributeTask);
            }
            return distributeTasks;
        }

        /// <summary>
        /// 手动生成料架转换任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateHandProductCartSwitchTask(string sCartRFID, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                if (cfgCart == null)
                {
                    result = "没有找到对应的料架";
                    return result;
                }

                CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts.FirstOrDefault(t => t.CFG_CartId == cfgCart.Id && t.Position <= 4);
                if (cfgWorkStationCurrentCart == null)
                {
                    result = "该料架没有停靠在线边内侧，不能进行空满交换";
                    return result;
                }
                string startPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + cfgWorkStationCurrentCart.Position;
                string endPosition = NullCartArea;
                string sTaskType = IsPTLPick ? "PTL" : "PDA";

                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                distributeTask.taskTyp = ProductCartSwitch;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = startPosition;
                distributeTask.endPosition = endPosition;
                distributeTask.podCode = cfgCart.Code;
                distributeTask.podDir = "手动";
                distributeTask.priority = 1;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = sTaskType;
                distributeTask.DistributeReqTypes = DistributeReqTypes.ProductCartSwitch;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;
                dbContext.DST_DistributeTasks.Add(distributeTask);

                ////解绑当前工位上的小车
                //cfgWorkStationCurrentCart.CFG_CartId = null;
                //cfgWorkStationCurrentCart.DockedTime = null;

                result = dbContext.SaveChanges() > 0 ? "空满交换成功" : "保存失败";
                return result;
            }
        }

        /// <summary>
        /// 生成线边配送任务
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="sPosition">车位</param>
        /// <param name="sCartCode">料架编号</param>
        /// <param name="sTaskSendType">任务发送类型</param>
        /// <param name="IsSendNullCartBack">是否触发空料架返回</param>
        /// <param name="dbContext">数据库上下文</param>
        /// <returns></returns>
        public List<DST_DistributeTask> GenerateProductAreaDistributeTask(string sWorkStationCode, string sPosition, string sCartCode, string sTaskSendType, bool IsSendNullCartBack, GeelyPtlEntities dbContext)
        {
            List<DST_DistributeTask> distributeTasks = new List<DST_DistributeTask>();

            string sProductAreaPosition = sWorkStationCode + "-" + sPosition;
            string sMaterialMarketPosition = sWorkStationCode + "L";
            string sNullCartPosition = NullCartArea;

            if (IsSendNullCartBack)
            {
                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                distributeTask.taskTyp = NullCartBack;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = sProductAreaPosition;
                distributeTask.endPosition = sNullCartPosition;
                distributeTask.podCode = sCartCode;
                distributeTask.podDir = sTaskSendType;
                distributeTask.priority = 2;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = "PTL";
                distributeTask.DistributeReqTypes = DistributeReqTypes.ProductNullCartBack;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;
                distributeTasks.Add(distributeTask);
            }

            string sql = string.Format(@"select distinct a.AreaId,a.CFG_CartId,a.Position,(case when a.Position<=2 then 'L' else 'R' end) as Direction,b.BatchCode 
from MarketZone a 
inner join CFG_CartCurrentMaterial b on a.CFG_CartId=b.CFG_CartId 
where a.CFG_CartId is not null and b.AST_CartTaskItemId is not null and a.AreaId='{0}' 
order by a.AreaId,b.BatchCode,a.Position", sWorkStationCode);
            MarketZoneDto marketZoneDto = dbContext.Database.SqlQuery<MarketZoneDto>(sql).FirstOrDefault();
            if (marketZoneDto != null)
            {
                sMaterialMarketPosition = sWorkStationCode + marketZoneDto.Direction;

                DST_DistributeTask distributeTask2 = new DST_DistributeTask();
                distributeTask2.reqCode = GetReqCode();
                distributeTask2.reqTime = DateTime.Now;
                distributeTask2.clientCode = "";
                distributeTask2.tokenCode = "";
                distributeTask2.taskTyp = MarketAreaToProductArea;
                distributeTask2.userCallCode = "";
                distributeTask2.taskGroupCode = "";
                distributeTask2.startPosition = sMaterialMarketPosition;
                distributeTask2.endPosition = sProductAreaPosition;
                distributeTask2.podCode = "";
                distributeTask2.podDir = sTaskSendType;
                distributeTask2.priority = 1;
                distributeTask2.robotCode = "";
                distributeTask2.taskCode = "";
                distributeTask2.data = "PTL-Auto";
                distributeTask2.DistributeReqTypes = DistributeReqTypes.MaterialMarketDistribute;
                distributeTask2.isResponse = false;
                distributeTask2.sendErrorCount = 0;
                distributeTasks.Add(distributeTask2);
            }
            return distributeTasks;
        }

        /// <summary>
        /// 生成线边自动清线配送任务
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="sPosition">车位</param>
        /// <param name="sCartCode">料架编号</param>
        /// <returns></returns>
        public List<DST_DistributeTask> GenerateProductAreaAutoClearTask(string sWorkStationCode, string sPosition, string sCartCode)
        {
            List<DST_DistributeTask> distributeTasks = new List<DST_DistributeTask>();

            string sProductAreaPosition = sWorkStationCode + "-" + sPosition;
            string sOutProductAreaPosition = sWorkStationCode + "-" + (Convert.ToInt32(sPosition) + 4).ToString();

            DST_DistributeTask distributeTask = new DST_DistributeTask();
            distributeTask.reqCode = GetReqCode();
            distributeTask.reqTime = DateTime.Now;
            distributeTask.clientCode = "";
            distributeTask.tokenCode = "";
            distributeTask.taskTyp = ProductPositionSwitch;
            distributeTask.userCallCode = "";
            distributeTask.taskGroupCode = "";
            distributeTask.startPosition = sProductAreaPosition;
            distributeTask.endPosition = sOutProductAreaPosition;
            distributeTask.podCode = sCartCode;
            distributeTask.podDir = "自动";
            distributeTask.priority = 1;
            distributeTask.robotCode = "";
            distributeTask.taskCode = "";
            distributeTask.data = "PTL-Auto";
            distributeTask.DistributeReqTypes = DistributeReqTypes.ProductInToOut;
            distributeTask.isResponse = false;
            distributeTask.sendErrorCount = 0;
            distributeTasks.Add(distributeTask);

            return distributeTasks;
        }

        /// <summary>
        /// 生成物料超市自动配送任务
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="sPosition">车位</param>
        /// <param name="sTaskSendType">任务发送类型</param>
        /// <param name="dbContext">数据库上下文</param>
        /// <returns></returns>
        public List<DST_DistributeTask> GenerateMarketAutoDistributeTask(string sWorkStationCode, string sPosition, string sTaskSendType, GeelyPtlEntities dbContext)
        {
            List<DST_DistributeTask> distributeTasks = new List<DST_DistributeTask>();

            string sProductAreaPosition = sWorkStationCode + "-" + sPosition;
            DST_DistributeTask marketDistributeTask = dbContext.DST_DistributeTasks.FirstOrDefault(t => t.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute
                                            && t.endPosition.Equals(sProductAreaPosition)
                                            && t.sendErrorCount < 5
                                            && t.arriveTime == null);
            if (marketDistributeTask == null)
            {
                string sql = string.Format(@"select distinct a.AreaId,a.CFG_CartId,a.Position,(case when a.Position<=2 then 'L' else 'R' end) as Direction,b.BatchCode 
from MarketZone a 
inner join CFG_CartCurrentMaterial b on a.CFG_CartId=b.CFG_CartId 
where a.CFG_CartId is not null and b.AST_CartTaskItemId is not null and a.AreaId='{0}' 
order by a.AreaId,b.BatchCode,a.Position", sWorkStationCode);
                MarketZoneDto marketZoneDto = dbContext.Database.SqlQuery<MarketZoneDto>(sql).FirstOrDefault();
                if (marketZoneDto != null)
                {
                    string sMaterialMarketPosition = sWorkStationCode + marketZoneDto.Direction;

                    DST_DistributeTask distributeTask = new DST_DistributeTask();
                    distributeTask.reqCode = GetReqCode();
                    distributeTask.reqTime = DateTime.Now;
                    distributeTask.clientCode = "";
                    distributeTask.tokenCode = "";
                    distributeTask.taskTyp = MarketAreaToProductArea;
                    distributeTask.userCallCode = "";
                    distributeTask.taskGroupCode = "";
                    distributeTask.startPosition = sMaterialMarketPosition;
                    distributeTask.endPosition = sProductAreaPosition;
                    distributeTask.podCode = "";
                    distributeTask.podDir = sTaskSendType;
                    distributeTask.priority = 1;
                    distributeTask.robotCode = "";
                    distributeTask.taskCode = "";
                    distributeTask.data = "PTL";
                    distributeTask.DistributeReqTypes = DistributeReqTypes.MaterialMarketDistribute;
                    distributeTask.isResponse = false;
                    distributeTask.sendErrorCount = 0;
                    distributeTasks.Add(distributeTask);
                }
            }
            return distributeTasks;
        }

        /// <summary>
        /// 生成线边外侧到里侧任务
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="sPosition">车位</param>
        /// <param name="sCartCode">料架编号</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="sTaskSendType">任务发送类型</param>
        /// <returns></returns>
        public List<DST_DistributeTask> GenerateProductOutToInTask(string sWorkStationCode, string sPosition, string sCartCode, bool IsPTLPick, string sTaskSendType)
        {
            List<DST_DistributeTask> distributeTasks = new List<DST_DistributeTask>();

            string sStartPosition = sWorkStationCode + "-" + sPosition;
            string sEndPosition = sWorkStationCode + "-" + (Convert.ToInt32(sPosition) - 4);
            string sTaskType = IsPTLPick ? "PTL" : "PDA";

            DST_DistributeTask distributeTask = new DST_DistributeTask();
            distributeTask.reqCode = GetReqCode();
            distributeTask.reqTime = DateTime.Now;
            distributeTask.clientCode = "";
            distributeTask.tokenCode = "";
            distributeTask.taskTyp = ProductPositionSwitch;
            distributeTask.userCallCode = "";
            distributeTask.taskGroupCode = "";
            distributeTask.startPosition = sStartPosition;
            distributeTask.endPosition = sEndPosition;
            distributeTask.podCode = sCartCode;
            distributeTask.podDir = sTaskSendType;
            distributeTask.priority = 1;
            distributeTask.robotCode = "";
            distributeTask.taskCode = "";
            distributeTask.data = sTaskType;
            distributeTask.DistributeReqTypes = DistributeReqTypes.ProductOutToIn;
            distributeTask.isResponse = false;
            distributeTask.sendErrorCount = 0;
            distributeTasks.Add(distributeTask);

            return distributeTasks;
        }

        /// <summary>
        /// 批量生成线边里侧到外侧任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边清线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateBatchProductInToOutTask(List<int> InitWorkStationIds, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                string sStartPosition = "";
                string sEndPosition = "";
                string sCartCode = "";
                string sPickType = IsPTLPick ? "PTL" : "PDA";

                List<CFG_WorkStationCurrentCart> CFG_WorkStationCurrentCarts = dbContext.CFG_WorkStationCurrentCarts.
                    Where(t => InitWorkStationIds.Contains(t.CFG_WorkStationId) && t.CFG_CartId != null && t.Position <= 4).
                    OrderBy(t => t.CFG_WorkStationId).
                    ThenBy(t => t.Position).ToList();

                if (CFG_WorkStationCurrentCarts.Count == 0)
                {
                    result = "选择的线边工位中内侧没有料架，不能清线";
                    return result;
                }

                List<CFG_WorkStationCurrentCart> CFG_OutWorkStationCurrentCarts = dbContext.CFG_WorkStationCurrentCarts.
                    Where(t => InitWorkStationIds.Contains(t.CFG_WorkStationId) && t.CFG_CartId != null && t.Position > 4).ToList();
                if (CFG_OutWorkStationCurrentCarts.Count > 0)
                {
                    result = "选择的线边清线工位外侧存在料架，不能清线";
                    return result;
                }

                //if (IsPTLPick)
                //{
                //    bool IsExistsCartNoFree = dbContext.CFG_WorkStationCurrentCarts.
                //        Any(t => InitWorkStationIds.Contains(t.CFG_WorkStationId) && t.CFG_CartId != null && t.Position <= 4 && t.CFG_Cart.CartStatus != CartStatus.Free);
                //    if (IsExistsCartNoFree)
                //    {
                //        result = "选择的线边清线工位中存在不为空闲的料架，不能清线";
                //        return result;
                //    }
                //}

                foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in CFG_WorkStationCurrentCarts)
                {
                    sStartPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + cfgWorkStationCurrentCart.Position;
                    sEndPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + (cfgWorkStationCurrentCart.Position + 4);
                    sCartCode = cfgWorkStationCurrentCart.CFG_Cart.Code;

                    DST_DistributeTask distributeTask = new DST_DistributeTask();
                    distributeTask.reqCode = GetReqCode();
                    distributeTask.reqTime = DateTime.Now;
                    distributeTask.clientCode = "";
                    distributeTask.tokenCode = "";
                    distributeTask.taskTyp = ProductPositionSwitch;
                    distributeTask.userCallCode = "";
                    distributeTask.taskGroupCode = "";
                    distributeTask.startPosition = sStartPosition;
                    distributeTask.endPosition = sEndPosition;
                    distributeTask.podCode = sCartCode;
                    distributeTask.podDir = "手动";
                    distributeTask.priority = 1;
                    distributeTask.robotCode = "";
                    distributeTask.taskCode = "";
                    distributeTask.data = sPickType;
                    distributeTask.DistributeReqTypes = DistributeReqTypes.ProductInToOut;
                    distributeTask.isResponse = false;
                    distributeTask.sendErrorCount = 0;

                    dbContext.DST_DistributeTasks.Add(distributeTask);

                    ////解绑当前工位上的小车
                    //cfgWorkStationCurrentCart.CFG_CartId = null;
                    //cfgWorkStationCurrentCart.DockedTime = null;
                }

                result = dbContext.SaveChanges() > 0 ? "生产线边清线成功" : "保存失败";
                return result;
            }
        }

        /// <summary>
        /// 生成空料架返回任务
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="sPosition">车位</param>
        /// <param name="sCartCode">料架编号</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="sTaskSendType">任务发送类型</param>
        /// <returns></returns>
        public List<DST_DistributeTask> GenerateNullCartBackTask(string sWorkStationCode, string sPosition, string sCartCode, bool IsPTLPick, string sTaskSendType)
        {
            List<DST_DistributeTask> distributeTasks = new List<DST_DistributeTask>();

            string sProductAreaPosition = sWorkStationCode + "-" + sPosition;
            string sNullCartPosition = NullCartArea;
            string sTaskType = IsPTLPick ? "PTL" : "PDA";


            DST_DistributeTask distributeTask = new DST_DistributeTask();
            distributeTask.reqCode = GetReqCode();
            distributeTask.reqTime = DateTime.Now;
            distributeTask.clientCode = "";
            distributeTask.tokenCode = "";
            distributeTask.taskTyp = NullCartBack;
            distributeTask.userCallCode = "";
            distributeTask.taskGroupCode = "";
            distributeTask.startPosition = sProductAreaPosition;
            distributeTask.endPosition = sNullCartPosition;
            distributeTask.podCode = sCartCode;
            distributeTask.podDir = sTaskSendType;
            distributeTask.priority = 1;
            distributeTask.robotCode = "";
            distributeTask.taskCode = "";
            distributeTask.data = sTaskType;
            distributeTask.DistributeReqTypes = DistributeReqTypes.ProductNullCartBack;
            distributeTask.isResponse = false;
            distributeTask.sendErrorCount = 0;
            distributeTasks.Add(distributeTask);

            return distributeTasks;
        }

        /// <summary>
        /// 批量生成空料架返回任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边清线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateBatchNullCartBackTask(List<int> InitWorkStationIds, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                string sStartPosition = "";
                string sEndPosition = "";
                string sCartCode = "";
                string sPickType = IsPTLPick ? "PTL" : "PDA";

                List<CFG_WorkStationCurrentCart> CFG_WorkStationCurrentCarts = dbContext.CFG_WorkStationCurrentCarts.
                   Where(t => InitWorkStationIds.Contains(t.CFG_WorkStationId) && t.CFG_CartId != null && t.Position > 4).
                   OrderBy(t => t.CFG_WorkStationId).
                   ThenBy(t => t.Position).ToList();

                if (CFG_WorkStationCurrentCarts.Count == 0)
                {
                    result = "选择的线边工位中外侧没有料架，不能进行空料架返回";
                    return result;
                }

                if (IsPTLPick)
                {
                    bool IsExistsCartNoFree = dbContext.CFG_WorkStationCurrentCarts.
                        Any(t => InitWorkStationIds.Contains(t.CFG_WorkStationId) && t.CFG_CartId != null && t.Position >4 && t.CFG_Cart.CartStatus != CartStatus.Free);
                    if (IsExistsCartNoFree)
                    {
                        result = "选择的线边工位中存在不为空闲的料架，不能进行空料架返回";
                        return result;
                    }
                }

                foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in CFG_WorkStationCurrentCarts)
                {
                    sStartPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + cfgWorkStationCurrentCart.Position;
                    sEndPosition = NullCartArea;
                    sCartCode = cfgWorkStationCurrentCart.CFG_Cart.Code;

                    DST_DistributeTask distributeTask = new DST_DistributeTask();
                    distributeTask.reqCode = GetReqCode();
                    distributeTask.reqTime = DateTime.Now;
                    distributeTask.clientCode = "";
                    distributeTask.tokenCode = "";
                    distributeTask.taskTyp = NullCartBack;
                    distributeTask.userCallCode = "";
                    distributeTask.taskGroupCode = "";
                    distributeTask.startPosition = sStartPosition;
                    distributeTask.endPosition = sEndPosition;
                    distributeTask.podCode = sCartCode;
                    distributeTask.podDir = "手动";
                    distributeTask.priority = 1;
                    distributeTask.robotCode = "";
                    distributeTask.taskCode = "";
                    distributeTask.data = sPickType;
                    distributeTask.DistributeReqTypes = DistributeReqTypes.ProductNullCartBack;
                    distributeTask.isResponse = false;
                    distributeTask.sendErrorCount = 0;

                    dbContext.DST_DistributeTasks.Add(distributeTask);

                    ////解绑当前工位上的小车
                    //cfgWorkStationCurrentCart.CFG_CartId = null;
                    //cfgWorkStationCurrentCart.DockedTime = null;
                }

                result = dbContext.SaveChanges() > 0 ? "生产线边空料架返回成功" : "保存失败";
                return result;
            }
        }

        /// <summary>
        /// 强制生成空料架返回任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边空料架返回的工位ID</param>
        /// <returns></returns>
        public string GeneratePTLNullCartBackTask(List<int> InitWorkStationIds)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                string sStartPosition = "";
                string sEndPosition = "";
                string sCartCode = "";

                List<CFG_WorkStationCurrentCart> CFG_WorkStationCurrentCarts = dbContext.CFG_WorkStationCurrentCarts.
                    Where(t => InitWorkStationIds.Contains(t.CFG_WorkStationId) && t.CFG_CartId != null && t.Position > 4).
                    OrderBy(t => t.CFG_WorkStationId).
                    ThenBy(t => t.Position).ToList();

                if (CFG_WorkStationCurrentCarts.Count == 0)
                {
                    result = "没有可以进行空料架返回的线边工位";
                    return result;
                }

                foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in CFG_WorkStationCurrentCarts)
                {
                    sStartPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + cfgWorkStationCurrentCart.Position;
                    sEndPosition = NullCartArea;
                    sCartCode = cfgWorkStationCurrentCart.CFG_Cart.Code;

                    DST_DistributeTask distributeTask = new DST_DistributeTask();
                    distributeTask.reqCode = GetReqCode();
                    distributeTask.reqTime = DateTime.Now;
                    distributeTask.clientCode = "";
                    distributeTask.tokenCode = "";
                    distributeTask.taskTyp = NullCartBack;
                    distributeTask.userCallCode = "";
                    distributeTask.taskGroupCode = "";
                    distributeTask.startPosition = sStartPosition;
                    distributeTask.endPosition = sEndPosition;
                    distributeTask.podCode = sCartCode;
                    distributeTask.podDir = "手动";
                    distributeTask.priority = 1;
                    distributeTask.robotCode = "";
                    distributeTask.taskCode = "";
                    distributeTask.data = "PTL";
                    distributeTask.DistributeReqTypes = DistributeReqTypes.ProductNullCartBack;
                    distributeTask.isResponse = false;
                    distributeTask.sendErrorCount = 0;

                    dbContext.DST_DistributeTasks.Add(distributeTask);

                    ////解绑当前工位上的小车
                    //cfgWorkStationCurrentCart.CFG_CartId = null;
                    //cfgWorkStationCurrentCart.DockedTime = null;
                }

                result = dbContext.SaveChanges() > 0 ? "生产线边空料架返回成功" : "保存失败";
                return result;
            }
        }

        /// <summary>
        /// 生成单个线边里侧到外侧任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateSingleProductInToOutTask(string sCartRFID, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                if (cfgCart == null)
                {
                    result = "没有找到对应的料架";
                    return result;
                }

                CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts.FirstOrDefault(t => t.CFG_CartId == cfgCart.Id && t.Position <= 4);
                if (cfgWorkStationCurrentCart == null)
                {
                    result = "该料架没有停靠在线边工位内侧，不能清线";
                    return result;
                }

                CFG_WorkStationCurrentCart cfgOutWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                    .FirstOrDefault(t => t.CFG_WorkStationId == cfgWorkStationCurrentCart.CFG_WorkStationId && t.Position == (cfgWorkStationCurrentCart.Position + 4) && t.CFG_CartId != null);
                if (cfgOutWorkStationCurrentCart != null)
                {
                    result = "该料架对应的线边工位外侧存在料架，不能清线";
                    return result;
                }

                string sPickType = IsPTLPick ? "PTL" : "PDA";

                string sStartPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + cfgWorkStationCurrentCart.Position;
                string sEndPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + (cfgWorkStationCurrentCart.Position + 4);
                string sCartCode = cfgCart.Code;

                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                distributeTask.taskTyp = ProductPositionSwitch;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = sStartPosition;
                distributeTask.endPosition = sEndPosition;
                distributeTask.podCode = sCartCode;
                distributeTask.podDir = "手动";
                distributeTask.priority = 1;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = sPickType;
                distributeTask.DistributeReqTypes = DistributeReqTypes.ProductInToOut;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;

                dbContext.DST_DistributeTasks.Add(distributeTask);

                ////解绑当前工位上的小车
                //cfgWorkStationCurrentCart.CFG_CartId = null;
                //cfgWorkStationCurrentCart.DockedTime = null;

                result = dbContext.SaveChanges() > 0 ? "生产线边清线成功" : "保存失败";
                return result;
            }
        }

        /// <summary>
        /// 生成单个空料架返回任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateSingleNullCartBackTask(string sCartRFID, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "";

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                if (cfgCart == null)
                {
                    result = "没有找到对应的料架";
                    return result;
                }

                CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts.FirstOrDefault(t => t.CFG_CartId == cfgCart.Id && t.Position > 4);
                if (cfgWorkStationCurrentCart == null)
                {
                    result = "该料架没有停靠在线边工位外侧，不能进行空料架返回";
                    return result;
                }

                string sPickType = IsPTLPick ? "PTL" : "PDA";

                string sProductAreaPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + cfgWorkStationCurrentCart.Position;
                string sNullCartPosition = NullCartArea;
                string sCartCode = cfgCart.Code;

                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                distributeTask.taskTyp = NullCartBack;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = sProductAreaPosition;
                distributeTask.endPosition = sNullCartPosition;
                distributeTask.podCode = sCartCode;
                distributeTask.podDir = "手动";
                distributeTask.priority = 1;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = sPickType;
                distributeTask.DistributeReqTypes = DistributeReqTypes.ProductNullCartBack;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;

                dbContext.DST_DistributeTasks.Add(distributeTask);

                ////解绑当前工位上的小车
                //cfgWorkStationCurrentCart.CFG_CartId = null;
                //cfgWorkStationCurrentCart.DockedTime = null;

                result = dbContext.SaveChanges() > 0 ? "生产线边空料架返回成功" : "保存失败";
                return result;
            }
        }

        /// <summary>
        /// 生成料架绑定或解绑任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="sPonitCode">储位编号</param>
        /// <param name="IsBind">是否绑定</param>
        /// <returns></returns>
        public string GenerateBindOrUnBindTask(string sCartRFID, string sPonitCode, bool IsBind)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "error";

                 CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                 if (cfgCart != null)
                 {
                     int nBind = 0;
                     DistributeReqTypes distributeReqTypes = DistributeReqTypes.BindPod;
                     if (IsBind)
                     {
                         nBind = 1;
                         distributeReqTypes = DistributeReqTypes.BindPod;
                     }
                     else
                     {
                         nBind = 0;
                         distributeReqTypes = DistributeReqTypes.UnBindPod;
                     }

                     DST_DistributeTask distributeTask = new DST_DistributeTask();
                     distributeTask.reqCode = GetReqCode();
                     distributeTask.reqTime = DateTime.Now;
                     distributeTask.clientCode = "";
                     distributeTask.tokenCode = "";
                     distributeTask.taskTyp = nBind.ToString();
                     distributeTask.userCallCode = "";
                     distributeTask.taskGroupCode = "";
                     distributeTask.startPosition = sPonitCode;
                     distributeTask.endPosition = "";
                     distributeTask.podCode = cfgCart.Code;
                     distributeTask.podDir = "手动";
                     distributeTask.priority = 1;
                     distributeTask.robotCode = "";
                     distributeTask.taskCode = "";
                     distributeTask.data = "";
                     distributeTask.DistributeReqTypes = distributeReqTypes;
                     distributeTask.isResponse = false;
                     distributeTask.sendErrorCount = 0;

                     dbContext.DST_DistributeTasks.Add(distributeTask);

                     if (dbContext.SaveChanges() > 0)
                     {
                         result = distributeTask.reqCode;
                     }
                 }
                 return result;
            }
        }

        /// <summary>
        /// 生成物料超市配送任务（方法不用）
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="sPointCode">储位编号</param>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateMarketDistributeTask(string sWorkStationCode, string sPointCode, string sCartRFID, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "error";
                /*
                string sTaskType = IsPTLPick ? "PTL" : "PDA";

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(sCartRFID));
                if (cfgCart == null)
                {
                    result = "没有找到对应料架";
                    return result;
                }

                //DST_DistributeArriveResult dstDistributeArriveResult = dbContext.DST_DistributeArriveResults.Where(t => t.podCode.Equals(cfgCart.Code) && t.DistributeReqTypes == DistributeReqTypes.PickAreaDistribute).OrderByDescending(t => t.ID).FirstOrDefault();
                //if (dstDistributeArriveResult == null)
                //{
                //    result = "没有该料架在物料超市的储位编号";
                //    return result;
                //}

                ArrayList arrUsePosition = new ArrayList();

                List<CFG_WorkStationCurrentCart> cfgWorkStationCurrentCarts = dbContext.CFG_WorkStationCurrentCarts
                    .Where(t => t.CFG_WorkStation.Code.Equals(sWorkStationCode)
                        && t.CFG_CartId != null)
                        .ToList();
                if (cfgWorkStationCurrentCarts != null)
                {
                    foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in cfgWorkStationCurrentCarts)
                    {
                        arrUsePosition.Add(cfgWorkStationCurrentCart.Position.ToString());
                    }
                }

                List<DST_DistributeTask> dstDistributeTasks = dbContext.DST_DistributeTasks
                    .Where(t => (t.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute || t.DistributeReqTypes == DistributeReqTypes.ProductAreaInit)
                        && t.endPosition.Contains(sWorkStationCode)
                        && t.sendErrorCount < 5
                        && t.arriveTime == null)
                        .ToList();
                if (dstDistributeTasks != null)
                {
                    string sUsePosition = "";
                    foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                    {
                        sUsePosition = dstDistributeTask.endPosition.Split('-')[1];
                        arrUsePosition.Add(sUsePosition);
                    }
                }

                if (arrUsePosition.Contains("7") && arrUsePosition.Contains("8"))
                {
                    result = "线边工位" + sWorkStationCode + "的外侧车位已全部被占用，暂时不能配送";
                    return result;
                }

                string sPosition = "7";
                if (arrUsePosition.Contains("7"))
                {
                    sPosition = "8";
                }

                if (arrUsePosition.Contains("8"))
                {
                    sPosition = "7";
                }

                string sEndPosition = sWorkStationCode + "-" + sPosition;
                string sStartPosition = sPointCode;
                //string sStartPosition = sWorkStationCode;

                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                //distributeTask.taskTyp = MarketAreaToProductArea;
                distributeTask.taskTyp = ProductPositionSwitch;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = sStartPosition;
                distributeTask.endPosition = sEndPosition;
                distributeTask.podCode = "";
                distributeTask.podDir = "手动";
                distributeTask.priority = 1;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = sTaskType;
                distributeTask.DistributeReqTypes = DistributeReqTypes.MaterialMarketDistribute;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;

                dbContext.DST_DistributeTasks.Add(distributeTask);

                if (dbContext.SaveChanges() > 0)
                {
                    result = "success";
                }
                 */
                return result;
            }
        }

        /// <summary>
        /// 手动批量生成物料超市配送任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边铺线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateBatchMarketDistributeTask(List<int> InitWorkStationIds, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "error";
                string sNoCartMarketWorkStation = ""; //没有停靠料架的物料超市工位
                string sFullProductWorkStation = ""; //线边外侧满了的工位
                string startPosition = "";
                string endPosition = "";
                bool isHaveCanInitWorkStation = false; //是否存在可以铺线的工位
                string sTaskType = IsPTLPick ? "PTL" : "PDA";

                List<string> InitWorkStationCodes = new List<string>();
                List<CFG_WorkStation> cfgInitWorkStations = dbContext.CFG_WorkStations.Where(t => InitWorkStationIds.Contains(t.Id)).ToList();
                foreach (CFG_WorkStation cfgWorkStation in cfgInitWorkStations)
                {
                    InitWorkStationCodes.Add(cfgWorkStation.Code);
                }

                Hashtable htUsePosition = new Hashtable();
                ArrayList arrUsePosition = new ArrayList();
                string sKey = "";
                string sUsePosition = "";
                string[] arrEndPosition = null;

                List<CFG_WorkStationCurrentCart> cfgWorkStationCurrentCarts = dbContext.CFG_WorkStationCurrentCarts
                    .Where(t => InitWorkStationIds.Contains(t.CFG_WorkStationId) && t.Position > 4
                        && t.CFG_CartId != null)
                        .ToList();
                if (cfgWorkStationCurrentCarts.Count > 0)
                {
                    foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in cfgWorkStationCurrentCarts)
                    {
                        arrUsePosition = new ArrayList();
                        sKey = cfgWorkStationCurrentCart.CFG_WorkStation.Code;
                        sUsePosition=cfgWorkStationCurrentCart.Position.ToString();
                        if (!htUsePosition.Contains(sKey))
                        {
                            arrUsePosition.Add(sUsePosition);
                            htUsePosition.Add(sKey, arrUsePosition);
                        }
                        else
                        {
                            arrUsePosition = htUsePosition[sKey] as ArrayList;
                            arrUsePosition.Add(sUsePosition);
                            htUsePosition[sKey] = arrUsePosition;
                        }
                    }
                }

                List<DST_DistributeTask> dstDistributeTasks = dbContext.DST_DistributeTasks
                    .Where(t => (t.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute || t.DistributeReqTypes == DistributeReqTypes.ProductAreaInit)
                        && InitWorkStationCodes.Contains(t.startPosition.Substring(0, t.startPosition.Length - 1))
                        && t.sendErrorCount < 5
                        && t.arriveTime == null)
                        .ToList();
                if (dstDistributeTasks.Count > 0)
                {
                    foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                    {
                        arrUsePosition = new ArrayList();
                        arrEndPosition = dstDistributeTask.endPosition.Split('-');
                        sKey = arrEndPosition[0];
                        sUsePosition = arrEndPosition[1];
                        if (!htUsePosition.Contains(sKey))
                        {
                            arrUsePosition.Add(sUsePosition);
                            htUsePosition.Add(sKey, arrUsePosition);
                        }
                        else
                        {
                            arrUsePosition = htUsePosition[sKey] as ArrayList;
                            arrUsePosition.Add(sUsePosition);
                            htUsePosition[sKey] = arrUsePosition;
                        }
                    }
                }

                List<CFG_WorkStation> cfgWorkStations = dbContext.CFG_WorkStations.Where(t => InitWorkStationIds.Contains(t.Id)).ToList();

                int nMaterialMarketDockCart = 0;
                bool IsCanInAllPosition = false;
                string sWorkStationCode = "";

                ArrayList arrCanInWorkStation = new ArrayList();
                //arrCanInWorkStation.Add("ZT06");

                string sql = "";
                if (IsPTLPick)
                {
                    sql = string.Format(@"select distinct a.AreaId,a.CFG_CartId,a.Position,(case when a.Position<=2 then 'L' else 'R' end) as Direction,b.BatchCode 
from MarketZone a 
inner join CFG_CartCurrentMaterial b on a.CFG_CartId=b.CFG_CartId 
where a.CFG_CartId is not null and b.AST_CartTaskItemId is not null
order by a.AreaId,b.BatchCode,a.Position");
                }
                else
                {
                    sql = string.Format(@"select distinct AreaId,CFG_CartId,Position,(case when Position<=2 then 'L' else 'R' end) as Direction,'' as BatchCode 
from MarketZone 
where CFG_CartId is not null
order by AreaId,Position");
                }
                List<MarketZoneDto> marketZoneDtos = dbContext.Database.SqlQuery<MarketZoneDto>(sql).ToList();
                List<MarketZoneDto> workStationMarketZoneDtos = new List<MarketZoneDto>();

                foreach (CFG_WorkStation cfgWorkStation in cfgWorkStations)
                {
                    //List<CFG_Cart> cfgCarts = new List<CFG_Cart>();
                    //if (IsPTLPick)
                    //{
                    //    cfgCarts = cfgWorkStation.CFG_CartCurrentMaterials.
                    //        Where(t => t.AST_CartTaskItemId != null).
                    //        Select(t => t.CFG_Cart).Distinct().
                    //        Where(c => c.CartStatus == CartStatus.ArrivedAtBufferArea
                    //        || c.CartStatus == CartStatus.NeedToWorkStation
                    //        || c.CartStatus == CartStatus.WaitingToWorkStation
                    //        || c.CartStatus == CartStatus.InCarriageToWorkStation).ToList();
                    //}
                    //else
                    //{
                    //    cfgCarts = cfgWorkStation.CFG_MarketWorkStationCurrentCarts.
                    //        Select(t => t.CFG_Cart).Distinct().
                    //        Where(c => c.CartStatus == CartStatus.ArrivedAtBufferArea).ToList();
                    //}
                    //nMaterialMarketDockCart = cfgCarts.Count;

                    sWorkStationCode = cfgWorkStation.Code;
                    workStationMarketZoneDtos = marketZoneDtos.Where(t => t.AreaId.Equals(sWorkStationCode)).ToList();
                    nMaterialMarketDockCart = workStationMarketZoneDtos.Count;

                    if (nMaterialMarketDockCart <= 0)
                    {
                        sNoCartMarketWorkStation += sWorkStationCode + ",";
                    }
                    else
                    {
                        arrUsePosition = htUsePosition[sWorkStationCode] as ArrayList;
                        if (arrUsePosition == null)
                        {
                            arrUsePosition = new ArrayList();
                        }

                        if (arrCanInWorkStation.Contains(sWorkStationCode))
                        {
                            IsCanInAllPosition = true;
                        }
                        else
                        {
                            IsCanInAllPosition = false;
                        }

                        if ((!IsCanInAllPosition && arrUsePosition.Contains("7") && arrUsePosition.Contains("8")) ||
                            (IsCanInAllPosition && arrUsePosition.Contains("5") && arrUsePosition.Contains("6") && arrUsePosition.Contains("7") && arrUsePosition.Contains("8")))
                            //(IsCanInAllPosition && arrUsePosition.Contains("5") && arrUsePosition.Contains("6")))
                        {
                            sFullProductWorkStation += sWorkStationCode + ",";
                        }
                        else
                        {
                            isHaveCanInitWorkStation = true;
                            string sDistributePosition = "";
                            if (nMaterialMarketDockCart > 0)
                            {
                                int nMax = 2;
                                int nAddNum = 6;
                                if (IsCanInAllPosition)
                                {
                                    nMax = 4;
                                    nAddNum = 4;
                                }

                                for (int i = 1; i <= nMax; i++)
                                {
                                    sDistributePosition = (i + nAddNum).ToString();
                                    if (arrUsePosition.Contains(sDistributePosition))
                                    {
                                        continue;
                                    }
                                    if (nMaterialMarketDockCart <= 0)
                                    {
                                        break;
                                    }

                                    MarketZoneDto marketZoneDto = workStationMarketZoneDtos[0];

                                    startPosition = sWorkStationCode + marketZoneDto.Direction;
                                    endPosition = sWorkStationCode + "-" + sDistributePosition; //5、6、7、8

                                    DST_DistributeTask distributeTask = new DST_DistributeTask();
                                    distributeTask.reqCode = GetReqCode();
                                    distributeTask.reqTime = DateTime.Now;
                                    distributeTask.clientCode = "";
                                    distributeTask.tokenCode = "";
                                    distributeTask.taskTyp = MarketAreaToProductArea;
                                    distributeTask.userCallCode = "";
                                    distributeTask.taskGroupCode = "";
                                    distributeTask.startPosition = startPosition;
                                    distributeTask.endPosition = endPosition;
                                    distributeTask.podCode = "";
                                    distributeTask.podDir = "手动";
                                    distributeTask.priority = 1;
                                    distributeTask.robotCode = "";
                                    distributeTask.taskCode = "";
                                    distributeTask.data = sTaskType;
                                    distributeTask.DistributeReqTypes = DistributeReqTypes.MaterialMarketDistribute;
                                    distributeTask.isResponse = false;
                                    distributeTask.sendErrorCount = 0;

                                    dbContext.DST_DistributeTasks.Add(distributeTask);

                                    workStationMarketZoneDtos.RemoveAt(0);
                                    nMaterialMarketDockCart--;
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(sNoCartMarketWorkStation))
                {
                    result = "物料超市工位:" + sNoCartMarketWorkStation.Trim(',') + "没有停靠料架，暂时不能配送";
                    return result;
                }

                if (!string.IsNullOrEmpty(sFullProductWorkStation))
                {
                    result = "线边工位" + sFullProductWorkStation.Trim(',') + "的外侧车位已全部被占用，暂时不能配送";
                    return result;
                }

                if (!isHaveCanInitWorkStation)
                {
                    result = "没有可以配送的物料超市工位";
                    return result;
                }
                result = dbContext.SaveChanges() > 0 ? "物料超市配送成功" : "保存失败";
                return result;
            }
        }

        /// <summary>
        /// 自动批量生成物料超市配送任务
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        public string GenerateAutoBatchMarketDistributeTask(string sWorkStationCode, bool IsPTLPick)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "error";
                string sNoCartMarketWorkStation = ""; //没有停靠料架的物料超市工位
                string sFullProductWorkStation = ""; //线边外侧满了的工位
                string startPosition = "";
                string endPosition = "";
                bool isHaveCanInitWorkStation = false; //是否存在可以铺线的工位
                string sTaskType = "PTL";

                ArrayList arrUsePosition = new ArrayList();
                string sUsePosition = "";

                List<CFG_WorkStationCurrentCart> cfgWorkStationCurrentCarts = dbContext.CFG_WorkStationCurrentCarts
                    .Where(t => t.CFG_WorkStation.Code.Equals(sWorkStationCode) && t.Position > 4
                        && t.CFG_CartId != null)
                        .ToList();
                if (cfgWorkStationCurrentCarts.Count > 0)
                {
                    foreach (CFG_WorkStationCurrentCart cfgWorkStationCurrentCart in cfgWorkStationCurrentCarts)
                    {
                        sUsePosition = cfgWorkStationCurrentCart.Position.ToString();
                        if (!arrUsePosition.Contains(sUsePosition))
                        {
                            arrUsePosition.Add(sUsePosition);
                        }
                    }
                }

                List<DST_DistributeTask> dstDistributeTasks = dbContext.DST_DistributeTasks
                    .Where(t => (((t.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute || t.DistributeReqTypes == DistributeReqTypes.ProductAreaInit) && t.startPosition.Contains(sWorkStationCode))
                        || (t.DistributeReqTypes == DistributeReqTypes.ProductInToOut && t.endPosition.Contains(sWorkStationCode)))
                        && t.sendErrorCount < 5
                        && t.arriveTime == null)
                        .ToList();
                if (dstDistributeTasks.Count > 0)
                {
                    foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                    {
                        sUsePosition = dstDistributeTask.endPosition.Split('-')[1];
                        if (!arrUsePosition.Contains(sUsePosition))
                        {
                            arrUsePosition.Add(sUsePosition);
                        }
                    }
                }

                bool IsCanInAllPosition = false;
                ArrayList arrCanInWorkStation = new ArrayList();
                //arrCanInWorkStation.Add("ZT06");

                string sql = "";
                if (IsPTLPick)
                {
                    sql = string.Format(@"select distinct a.AreaId,a.CFG_CartId,a.Position,(case when a.Position<=2 then 'L' else 'R' end) as Direction,b.BatchCode 
from MarketZone a 
inner join CFG_CartCurrentMaterial b on a.CFG_CartId=b.CFG_CartId 
where a.CFG_CartId is not null and b.AST_CartTaskItemId is not null and a.AreaId='{0}' 
order by a.AreaId,b.BatchCode,a.Position", sWorkStationCode);
                }
                else
                {
                    sql = string.Format(@"select distinct AreaId,CFG_CartId,Position,(case when Position<=2 then 'L' else 'R' end) as Direction,'' as BatchCode 
from MarketZone 
where CFG_CartId is not null and AreaId='{0}' 
order by AreaId,Position", sWorkStationCode);
                }
                List<MarketZoneDto> marketZoneDtos = dbContext.Database.SqlQuery<MarketZoneDto>(sql).ToList();

                int nMaterialMarketDockCart = marketZoneDtos.Count;

                if (nMaterialMarketDockCart <= 0)
                {
                    sNoCartMarketWorkStation = sWorkStationCode;
                }
                else
                {
                    if (arrCanInWorkStation.Contains(sWorkStationCode))
                    {
                        IsCanInAllPosition = true;
                    }
                    else
                    {
                        IsCanInAllPosition = false;
                    }

                    if ((!IsCanInAllPosition && arrUsePosition.Contains("7") && arrUsePosition.Contains("8")) ||
                        (IsCanInAllPosition && arrUsePosition.Contains("5") && arrUsePosition.Contains("6") && arrUsePosition.Contains("7") && arrUsePosition.Contains("8")))
                    {
                        sFullProductWorkStation = sWorkStationCode;
                    }
                    else
                    {
                        isHaveCanInitWorkStation = true;
                        string sDistributePosition = "";
                        if (nMaterialMarketDockCart > 0)
                        {
                            int nMax = 2;
                            int nAddNum = 6;
                            if (IsCanInAllPosition)
                            {
                                nMax = 4;
                                nAddNum = 4;
                            }

                            for (int i = 1; i <= nMax; i++)
                            {
                                sDistributePosition = (i + nAddNum).ToString();
                                if (arrUsePosition.Contains(sDistributePosition))
                                {
                                    continue;
                                }
                                if (nMaterialMarketDockCart <= 0)
                                {
                                    break;
                                }

                                MarketZoneDto marketZoneDto = marketZoneDtos[0];

                                startPosition = sWorkStationCode + marketZoneDto.Direction;
                                endPosition = sWorkStationCode + "-" + sDistributePosition; //5、6、7、8

                                DST_DistributeTask distributeTask = new DST_DistributeTask();
                                distributeTask.reqCode = GetReqCode();
                                distributeTask.reqTime = DateTime.Now;
                                distributeTask.clientCode = "";
                                distributeTask.tokenCode = "";
                                distributeTask.taskTyp = MarketAreaToProductArea;
                                distributeTask.userCallCode = "";
                                distributeTask.taskGroupCode = "";
                                distributeTask.startPosition = startPosition;
                                distributeTask.endPosition = endPosition;
                                distributeTask.podCode = "";
                                distributeTask.podDir = "自动";
                                distributeTask.priority = 1;
                                distributeTask.robotCode = "";
                                distributeTask.taskCode = "";
                                distributeTask.data = sTaskType;
                                distributeTask.DistributeReqTypes = DistributeReqTypes.MaterialMarketDistribute;
                                distributeTask.isResponse = false;
                                distributeTask.sendErrorCount = 0;

                                dbContext.DST_DistributeTasks.Add(distributeTask);

                                marketZoneDtos.RemoveAt(0);
                                nMaterialMarketDockCart--;
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(sNoCartMarketWorkStation))
                {
                    result = "物料超市工位:" + sNoCartMarketWorkStation + "没有停靠料架，暂时不能配送";
                    return result;
                }

                if (!string.IsNullOrEmpty(sFullProductWorkStation))
                {
                    result = "线边工位" + sFullProductWorkStation + "的外侧车位已全部被占用，暂时不能配送";
                    return result;
                }

                if (!isHaveCanInitWorkStation)
                {
                    result = "没有可以配送的物料超市工位";
                    return result;
                }
                result = dbContext.SaveChanges() > 0 ? "物料超市配送成功" : "保存失败";
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
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                string result = "error";

                bool IsNullCartArea = false;
                if (AreaFlag.Equals("0")) //料架缓冲区
                {
                    IsNullCartArea = true;
                }

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Rfid1.Equals(CartRFID));
                if (cfgCart == null)
                {
                    result = "没有找到对应料架";
                    return result;
                }

                string sStartPosition = StartPoint;
                string sEndPosition = EndPoint;
                string sTaskType = ProductPositionSwitch;
                if (AreaFlag.Equals("0")) //料架缓冲区
                {
                    sEndPosition = NullCartArea;
                    sTaskType = PointToNullCartArea;
                }
                else if (AreaFlag.Equals("1")) //物料超市
                {
                    sTaskType = PickAreaToMarketArea;
                    //sTaskType = PointToNullCartArea;
                }

                int cfgCartId = cfgCart.Id;

                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts.FirstOrDefault(t => t.CFG_CartId == cfgCartId);
                if (cfgChannelCurrentCart != null)
                {
                    sStartPosition = "H" + cfgChannelCurrentCart.CFG_Channel.Code + "P" + cfgChannelCurrentCart.Position.ToString();
                }
                else
                {
                    CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts.FirstOrDefault(t => t.CFG_CartId == cfgCartId);
                    if (cfgWorkStationCurrentCart != null)
                    {
                        sStartPosition = cfgWorkStationCurrentCart.CFG_WorkStation.Code + "-" + cfgWorkStationCurrentCart.Position.ToString();
                    }
                }

                string sTaskDesc = (IsPTLPick ? "PTL" : "PDA") + "&" + IsNullCartArea.ToString().ToLower();

                DST_DistributeTask distributeTask = new DST_DistributeTask();
                distributeTask.reqCode = GetReqCode();
                distributeTask.reqTime = DateTime.Now;
                distributeTask.clientCode = "";
                distributeTask.tokenCode = "";
                distributeTask.taskTyp = sTaskType;
                distributeTask.userCallCode = "";
                distributeTask.taskGroupCode = "";
                distributeTask.startPosition = sStartPosition;
                distributeTask.endPosition = sEndPosition;
                distributeTask.podCode = "";
                distributeTask.podDir = "手动";
                distributeTask.priority = 1;
                distributeTask.robotCode = "";
                distributeTask.taskCode = "";
                distributeTask.data = sTaskDesc;
                distributeTask.DistributeReqTypes = DistributeReqTypes.PointToPointDistribute;
                distributeTask.isResponse = false;
                distributeTask.sendErrorCount = 0;

                dbContext.DST_DistributeTasks.Add(distributeTask);

                if (dbContext.SaveChanges() > 0)
                {
                    result = "success";
                }
                return result;
            }
        }

        /// <summary>
        /// 获取请求编号
        /// </summary>
        /// <returns></returns>
        private string GetReqCode()
        {
            Thread.Sleep(5);
            return "T" + DateTime.Now.ToString("yyMMddHHmmssfff");
        }

        #region Test
        public void Test(int n)
        {
            //using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            //{
            //    CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations.FirstOrDefault(t => t.Id == 1);
            //    List<CFG_Cart> cfgCarts = cfgWorkStation.CFG_CartCurrentMaterials.
            //                    Where(t => t.AST_CartTaskItemId != null).
            //                    Select(t => t.CFG_Cart).Distinct().
            //                    Where(c => c.CartStatus == CartStatus.ArrivedAtBufferArea).ToList();

            //    return cfgCarts.Count.ToString();
            //}

            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                //CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts.Where(ccc => ccc.CFG_ChannelId == 2 && ccc.CFG_CartId != null).OrderBy(t => t.Position).FirstOrDefault();



                //List<DST_DistributeTask> distributeTasks = GeneratePickAreaDistributeTask(cfgChannelCurrentCart.CFG_Cart);
                //foreach (DST_DistributeTask distributeTask in distributeTasks)
                //{
                //    dbContext.DST_DistributeTasks.Add(distributeTask);
                //}

                CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Id == n);
                List<DST_DistributeTask> distributeTasks = DistributingTaskGenerator.Instance.GenerateProductCartSwitchTask(cfgCart);
                foreach (DST_DistributeTask distributeTask in distributeTasks)
                {
                    dbContext.DST_DistributeTasks.Add(distributeTask);
                }

                //List<DST_DistributeTask> distributeTasks = GenerateProductAreaDistributeTask("ZT03", "7", "100011");
                //foreach (DST_DistributeTask distributeTask in distributeTasks)
                //{
                //    dbContext.DST_DistributeTasks.Add(distributeTask);
                //}

                dbContext.SaveChanges();
            }
        }

        public string Test2()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Channel cfgchannel = dbContext.CFG_Channels.FirstOrDefault(t => t.Code.Equals("3"));
                bool isNullChannel = cfgchannel.CFG_ChannelCurrentCarts.All(t => t.CFG_CartId == null);

                return isNullChannel ? "true" : "false";
            }
        }
        #endregion
    }
}
