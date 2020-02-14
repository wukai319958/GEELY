using DataAccess;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using DataAccess.Other;
using Interfaces.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Interfaces.DistributingServices
{
    public class PickAreaService
    {
        static readonly PickAreaService instance = new PickAreaService();

        public static PickAreaService Instance
        {
            get { return PickAreaService.instance; }
        }

        string ptlToAgvServiceUrl;

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(2);
        Thread thread;
        bool threadNeedQuit;

        public bool IsRunning { get; private set; }

        PickAreaService()
        { }

        public void Start(string ptlToAgvServiceUrl)
        {
            this.ptlToAgvServiceUrl = ptlToAgvServiceUrl;

            this.thread = new Thread(this.threadStart);
            this.thread.Name = this.GetType().FullName;
            this.thread.IsBackground = true;
            this.thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            this.thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

            this.threadNeedQuit = false;
            this.thread.Start();

            this.IsRunning = true;
        }

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

        void threadStart(object notUsed)
        {
            while (!this.threadNeedQuit)
            {
                try
                {
                    this.SendPickAreaDistributeTask();
                }
                catch
                {
                }
                finally
                {
                    DateTime beginTime = DateTime.Now;

                    while (!this.threadNeedQuit && (DateTime.Now - beginTime) < this.threadPeriod)
                        Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 发送拣料区配送任务
        /// </summary>
        /// <returns></returns>
        private bool SendPickAreaDistributeTask()
        {
            bool isSuccess = false;
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    #region 注释
                    ////获取物料超市已满4辆车的工位
                    //List<string> NoDistributeWorkStationCode = new List<string>();
                    //Hashtable htWorkStation = new Hashtable();

                    //List<CFG_Cart> cfgCarts = dbContext.CFG_Carts.Where(t => (t.CartStatus == CartStatus.ArrivedAtBufferArea
                    //    || t.CartStatus == CartStatus.NeedToWorkStation
                    //    || t.CartStatus == CartStatus.WaitingToWorkStation)
                    //    && t.FND_Tasks.FirstOrDefault(f => f.FindingStatus == FindingStatus.New 
                    //        || f.FindingStatus == FindingStatus.NeedDisplay
                    //        || f.FindingStatus == FindingStatus.Displaying) != null).ToList();
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

                    //获取未响应的配送任务
                    //List<DST_DistributeTask> distributeTasks = dbContext.DST_DistributeTasks.
                    //        Where(t => !t.isResponse &&
                    //            (t.DistributeReqTypes == DistributeReqTypes.PickAreaDistribute
                    //            || t.DistributeReqTypes == DistributeReqTypes.NullCartAreaDistribute)
                    //            && !NoDistributeWorkStationCode.Contains(t.endPosition)
                    //            && t.sendErrorCount < 5).
                    //        OrderBy(t => t.reqTime).ToList();
                    #endregion

                    List<DST_DistributeTask> distributeTasks = dbContext.DST_DistributeTasks.
                            Where(t => !t.isResponse &&
                                (t.DistributeReqTypes == DistributeReqTypes.PickAreaDistribute
                                || t.DistributeReqTypes == DistributeReqTypes.NullCartAreaDistribute
                                || t.DistributeReqTypes == DistributeReqTypes.PointToPointDistribute)
                                && t.sendErrorCount < 5).
                            OrderBy(t => t.reqTime).ToList();

                    if (distributeTasks.Count == 0)
                    {
                        return false;
                    }

                    //发送信息
                    string sendInfo = "";
                    //AGV服务地址
                    string sURL = ptlToAgvServiceUrl + "/genAgvSchedulingTask";
                    //HTTP响应结果
                    string result = "";
                    ArrayList arrChannel = new ArrayList();

                    foreach (DST_DistributeTask distributeTask in distributeTasks)
                    {
                        if (distributeTask.DistributeReqTypes == DistributeReqTypes.PickAreaDistribute) //拣料区配送
                        {
                            //查询巷道对应一侧的物料超市停靠的车辆信息
                            string[] arrPosition = distributeTask.startPosition.Replace("H", "").Replace("P", ",").Split(',');
                            int nChannelId = Convert.ToInt32(arrPosition[0]); //巷道
                            int nPosition = Convert.ToInt32(arrPosition[1]); //车位

                            if (arrChannel.Contains(nChannelId))
                            {
                                continue;
                            }
                            arrChannel.Add(nChannelId);

                            int nMaxPosition = 0;
                            int nMinPosition = 0;
                            if (nPosition <= 2)
                            {
                                nMinPosition = 1;
                                nMaxPosition = 2;
                            }
                            else
                            {
                                nMinPosition = 3;
                                nMaxPosition = 4;
                            }
                            string sWorkStationCode = distributeTask.endPosition;

                            CFG_CartCurrentMaterial firstCurDisCartCurrentMaterial = dbContext.CFG_CartCurrentMaterials.FirstOrDefault(t => t.CFG_Cart.Code.Equals(distributeTask.podCode) && t.AST_CartTaskItemId != null);
                            if (firstCurDisCartCurrentMaterial != null)
                            {
                                //查询拣料区正在往物料超市配送的料架的批次跟需配送的巷道的料架的批次是否一致
                                List<string> arrPodCodes = new List<string>();
                                List<DST_DistributeTask> pickDistributeTasks = dbContext.DST_DistributeTasks.Where(t => t.DistributeReqTypes == DistributeReqTypes.PickAreaDistribute
                                    && t.isResponse && t.sendErrorCount < 5 && t.arriveTime == null
                                    && !t.reqCode.Equals(distributeTask.reqCode)
                                    && t.endPosition.Equals(sWorkStationCode)).ToList();

                                int nPickStartPosition = 0;
                                foreach (DST_DistributeTask pickDistributeTask in pickDistributeTasks)
                                {
                                    nPickStartPosition = Convert.ToInt32(pickDistributeTask.startPosition.Split('P')[1]);
                                    if (nPickStartPosition >= nMinPosition && nPickStartPosition <= nMaxPosition)
                                    {
                                        arrPodCodes.Add(pickDistributeTask.podCode);
                                    }
                                }

                                if (dbContext.CFG_CartCurrentMaterials.Any(t => arrPodCodes.Contains(t.CFG_Cart.Code) && t.AST_CartTaskItemId != null && t.BatchCode != firstCurDisCartCurrentMaterial.BatchCode))
                                {
                                    continue;
                                }
                            }
                            List<MarketZone> marketZones = dbContext.MarketZones.Where(t => t.AreaId.Equals(sWorkStationCode)
                                && t.CFG_CartId != null
                                && t.Position >= nMinPosition && t.Position <= nMaxPosition).ToList();

                            //string sWorkStationCode = distributeTask.endPosition;
                            //List<MarketZone> marketZones = dbContext.MarketZones.Where(t => t.AreaId.Equals(sWorkStationCode)
                            //    && t.CFG_CartId != null).ToList();
                            if (marketZones.Count > 0)
                            {
                                //物料超市对应一侧已停满
                                if (marketZones.Count >= 2)
                                {
                                    continue;
                                }

                                //物料超市停靠的车+正在配送的车>=2时，拣料区对应工位相应一侧不再配送
                                string sql = string.Format(@"select count(*) from DST_DistributeTask a
inner join DST_DistributeTaskResult b on a.reqCode=b.reqCode
where a.DistributeReqTypes=3 and a.isResponse=1 and a.sendErrorCount<5 and a.arriveTime is null
and b.data not in(select taskCode from DST_DistributeArriveTask where method='OutFromBin')
and ((a.startPosition='{0}'+'L' and {1}<=2) or (a.startPosition='{0}'+'R' and {1}>2))", sWorkStationCode, nPosition);
                                int nNoCompleteDistributeCount = dbContext.Database.SqlQuery<int>(sql).FirstOrDefault();

                                if (marketZones.Count + nNoCompleteDistributeCount >= 2)
                                {
                                    continue;
                                }

                                //查询需配送的巷道的料架和物料超市的料架批次是否一致
                                if (firstCurDisCartCurrentMaterial != null)
                                {
                                    int nFirstMarketCartId = Convert.ToInt32(marketZones[0].CFG_CartId);
                                    CFG_CartCurrentMaterial firstDockCartCurrentMaterial = dbContext.CFG_CartCurrentMaterials.FirstOrDefault(t => t.CFG_CartId == nFirstMarketCartId && t.AST_CartTaskItemId != null);
                                    if (firstDockCartCurrentMaterial != null)
                                    {
                                        if (!firstCurDisCartCurrentMaterial.BatchCode.Equals(firstDockCartCurrentMaterial.BatchCode))
                                        {
                                            continue;
                                        }
                                    }
                                }

                                //if (marketZones.Count >= 4)
                                //{
                                //    continue;
                                //}

                                //CFG_CartCurrentMaterial firstCurDisCartCurrentMaterial = dbContext.CFG_CartCurrentMaterials.FirstOrDefault(t => t.CFG_Cart.Code.Equals(distributeTask.podCode) && t.AST_CartTaskItemId != null);
                                //if (firstCurDisCartCurrentMaterial != null)
                                //{
                                //    List<int?> ListCartId = new List<int?>();
                                //    foreach (MarketZone marketZone in marketZones)
                                //    {
                                //        ListCartId.Add(marketZone.CFG_CartId);
                                //    }

                                //    CFG_CartCurrentMaterial firstDockCartCurrentMaterial = dbContext.CFG_CartCurrentMaterials.FirstOrDefault(t => ListCartId.Contains(t.CFG_CartId)
                                //        && t.AST_CartTaskItemId != null
                                //        && !t.BatchCode.Equals(firstCurDisCartCurrentMaterial.BatchCode));
                                //    if (firstDockCartCurrentMaterial != null)
                                //    {
                                //        continue;
                                //    }
                                //}
                            }
                        }
                        else if (distributeTask.DistributeReqTypes == DistributeReqTypes.NullCartAreaDistribute) //空料架缓冲区配送
                        {
                            string[] arrPosition = distributeTask.endPosition.Replace("H", "").Replace("P", ",").Split(',');
                            int nChannelId = Convert.ToInt32(arrPosition[0]); //巷道
                            int nPosition = Convert.ToInt32(arrPosition[1]); //车位

                            //如果对应巷道车位上停靠了料架，则先不补空料架
                            CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts.FirstOrDefault(ccc => ccc.CFG_ChannelId == nChannelId && ccc.Position == nPosition && ccc.CFG_CartId != null);
                            if (cfgChannelCurrentCart != null)
                            {
                                continue;
                            }
                        }

                        //绑定配送任务
                        DST_DistributeTaskDto distributeTaskDto = new DST_DistributeTaskDto();
                        distributeTaskDto.reqCode = distributeTask.reqCode;
                        distributeTaskDto.reqTime = distributeTask.reqTime.ToString("yyyy-MM-dd HH:mm:ss");
                        distributeTaskDto.clientCode = distributeTask.clientCode;
                        distributeTaskDto.tokenCode = distributeTask.tokenCode;
                        distributeTaskDto.taskTyp = distributeTask.taskTyp;
                        distributeTaskDto.userCallCode = distributeTask.userCallCode;
                        distributeTaskDto.userCallCodePath = new List<string>();
                        distributeTaskDto.userCallCodePath.AddRange(new string[] { distributeTask.startPosition, distributeTask.endPosition });
                        distributeTaskDto.podCode = distributeTask.podCode;
                        distributeTaskDto.robotCode = distributeTask.robotCode;
                        distributeTaskDto.taskCode = distributeTask.taskCode;
                        distributeTaskDto.data = distributeTask.data;

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

                                    if (distributeTask.DistributeReqTypes == DistributeReqTypes.PickAreaDistribute) //拣料区配送
                                    {
                                        //解绑巷道停靠
                                        CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts.FirstOrDefault(ccc => ccc.CFG_Cart.Code.Equals(distributeTask.podCode));
                                        if (cfgChannelCurrentCart != null)
                                        {
                                            cfgChannelCurrentCart.CFG_CartId = null;
                                            cfgChannelCurrentCart.DockedTime = null;
                                        }
                                    }
                                }
                                else
                                {
                                    distributeTask.sendErrorCount = distributeTask.sendErrorCount + 1;
                                }
                            }
                        }
                    }

                    //更新数据库
                    isSuccess = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception)
            {
                isSuccess = false;
            }
            return isSuccess;
        }
    }
}
