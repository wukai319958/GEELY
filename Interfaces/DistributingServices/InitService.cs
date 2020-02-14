using DataAccess;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using Interfaces.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Interfaces.DistributingServices
{
    public class InitService
    {
        static readonly InitService instance = new InitService();

        public static InitService Instance
        {
            get { return InitService.instance; }
        }

        string ptlToAgvServiceUrl;

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(10);
        Thread thread;
        bool threadNeedQuit;

        public bool IsRunning { get; private set; }

        InitService()
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
                    this.SendInitDistributeTask();
                }
                catch
                {
                }
                finally
                {
                    DateTime beginTime = DateTime.Now;

                    while (!this.threadNeedQuit && (DateTime.Now - beginTime) < this.threadPeriod)
                        Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 发送铺线任务
        /// </summary>
        /// <returns></returns>
        public bool SendInitDistributeTask()
        {
            bool isSuccess = false;
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    List<DST_DistributeTask> distributeTasks = null;
                    DistributeReqTypes distributeReqType = DistributeReqTypes.PickAreaInit;

                    //获取未响应的铺线任务
                    if (distributeReqType == DistributeReqTypes.PickAreaInit) //拣料区铺线
                    {
                        distributeTasks = dbContext.DST_DistributeTasks.
                            Where(t => !t.isResponse && t.DistributeReqTypes == DistributeReqTypes.PickAreaInit && t.sendErrorCount < 5).
                            OrderBy(t => t.reqTime).ToList();
                    }

                    //if (distributeTasks.Count == 0)
                    //{
                    //    distributeReqType = DistributeReqTypes.ProductAreaInit;

                    //    if (distributeReqType == DistributeReqTypes.ProductAreaInit) //生产线边铺线
                    //    {
                    //        distributeTasks = dbContext.DST_DistributeTasks.
                    //            Where(t => !t.isResponse && t.DistributeReqTypes == DistributeReqTypes.ProductAreaInit && t.sendErrorCount < 5).
                    //            OrderBy(t => t.reqTime).ToList();
                    //    }
                    //}

                    if (distributeTasks.Count == 0)
                    {
                        return false;
                    }

                    //已经设置成运输开始的寻车任务ID集合
                    //List<long> listNeedBlinkFndTask = new List<long>(); 

                    //发送信息
                    string sendInfo = "";
                    //AGV服务地址
                    string sURL = ptlToAgvServiceUrl + "/genAgvSchedulingTask";
                    //HTTP响应结果
                    string result = "";

                    foreach (DST_DistributeTask distributeTask in distributeTasks)
                    {
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

                        if (string.IsNullOrEmpty(result))
                        {
                            isSuccess = false;
                            distributeTask.sendErrorCount = distributeTask.sendErrorCount + 1;
                        }
                        else
                        {
                            //实例化HTTP响应结果
                            DST_DistributeTaskResultDto distributeTaskResultDto = JsonConvert.DeserializeObject<DST_DistributeTaskResultDto>(result);
                            if (distributeTaskResultDto == null)
                            {
                                isSuccess = false;
                                distributeTask.sendErrorCount = distributeTask.sendErrorCount + 1;
                            }
                            else
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

                                    //if (distributeTask.DistributeReqTypes == DistributeReqTypes.ProductAreaInit)
                                    //{
                                    //    string sWorkStationCode = distributeTask.startPosition;
                                    //    FND_Task fndTask = dbContext.FND_Tasks.Where(t => t.CFG_WorkStation.Code.Equals(sWorkStationCode)
                                    //        && (t.FindingStatus == FindingStatus.New || t.FindingStatus == FindingStatus.NeedDisplay || t.FindingStatus == FindingStatus.Displaying)
                                    //        && (t.CFG_Cart.CartStatus == CartStatus.NeedToWorkStation || t.CFG_Cart.CartStatus == CartStatus.WaitingToWorkStation)
                                    //        && !listNeedBlinkFndTask.Contains(t.Id)).
                                    //        OrderBy(t => t.BatchCode).ThenByDescending(t => t.FindingStatus).FirstOrDefault();
                                    //    if (fndTask != null)
                                    //    {
                                    //        if (fndTask.FindingStatus == FindingStatus.New)
                                    //        {
                                    //            fndTask.CFG_EmployeeId = 1;
                                    //            fndTask.DisplayTime = DateTime.Now;

                                    //            fndTask.CFG_Cart.CartStatus = CartStatus.WaitingToWorkStation;
                                    //        }
                                    //        fndTask.FindingStatus = FindingStatus.NeedBlink;
                                    //        fndTask.CFG_Cart.CartStatus = CartStatus.InCarriageToWorkStation;

                                    //        listNeedBlinkFndTask.Add(fndTask.Id);
                                    //    }
                                    //}
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
