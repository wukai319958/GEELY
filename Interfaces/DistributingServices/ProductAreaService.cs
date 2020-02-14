using DataAccess;
using DataAccess.CartFinding;
using DataAccess.Config;
using DataAccess.Distributing;
using Interfaces.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;

namespace Interfaces.DistributingServices
{
    public class ProductAreaService
    {
        static readonly ProductAreaService instance = new ProductAreaService();

        public static ProductAreaService Instance
        {
            get { return ProductAreaService.instance; }
        }

        string ptlToAgvServiceUrl;

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(2);
        Thread thread;
        bool threadNeedQuit;

        public bool IsRunning { get; private set; }

        ProductAreaService()
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
                    this.SendProductAreaDistributeTask();
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
        /// 发送生产线边配送任务
        /// </summary>
        /// <returns></returns>
        private bool SendProductAreaDistributeTask()
        {
            bool isSuccess = false;
            try
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap { ExeConfigFilename = @"服务控制台.exe.Config" };
                Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                string StartMaterialMarketDistributeFlag = configuration.AppSettings.Settings["StartMaterialMarketDistribute"].Value;

                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    //获取未响应的配送任务
                    List<DST_DistributeTask> distributeTasks = dbContext.DST_DistributeTasks.
                            Where(t => !t.isResponse &&
                                (t.DistributeReqTypes == DistributeReqTypes.ProductCartSwitch
                                || t.DistributeReqTypes == DistributeReqTypes.ProductNullCartBack
                                || t.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute
                                || t.DistributeReqTypes == DistributeReqTypes.ProductOutToIn
                                || t.DistributeReqTypes == DistributeReqTypes.ProductInToOut
                                ) && t.sendErrorCount < 5).
                            OrderBy(t => t.reqTime).ToList();

                    //List<DST_DistributeTask> distributeTasks = new List<DST_DistributeTask>();

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
                        if (StartMaterialMarketDistributeFlag.Equals("no"))
                        {
                            if (distributeTask.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute)
                            {
                                distributeTask.sendErrorCount = 5;
                                continue;
                            }
                        }

                        if (distributeTask.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute)
                        {
                            Thread.Sleep(500);
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

                                    //if (distributeTask.DistributeReqTypes == DistributeReqTypes.MaterialMarketDistribute)
                                    //{
                                    //    string sWorkStationCode = distributeTask.startPosition;
                                    //    //string sWorkStationCode = distributeTask.endPosition.Split('-')[0];
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
                                    if (distributeTask.DistributeReqTypes == DistributeReqTypes.ProductCartSwitch
                                       || distributeTask.DistributeReqTypes == DistributeReqTypes.ProductNullCartBack
                                       || distributeTask.DistributeReqTypes == DistributeReqTypes.ProductOutToIn
                                       || distributeTask.DistributeReqTypes == DistributeReqTypes.ProductInToOut)
                                    {
                                        string[] arrStartPosition = distributeTask.startPosition.Split('-');
                                        string sWorkStationCode = arrStartPosition[0];
                                        int nPosition = Convert.ToInt32(arrStartPosition[1]);

                                        //解除线边停靠
                                        CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = dbContext.CFG_WorkStationCurrentCarts
                                                                                       .FirstOrDefault(wscc => wscc.CFG_WorkStation.Code.Equals(sWorkStationCode) && wscc.Position == nPosition);
                                        if (cfgWorkStationCurrentCart != null)
                                        {
                                            if (distributeTask.DistributeReqTypes == DistributeReqTypes.ProductNullCartBack)
                                            {
                                                //清空小车上的物料
                                                CFG_Cart cfgCart = cfgWorkStationCurrentCart.CFG_Cart;
                                                if (cfgCart != null)
                                                {
                                                    List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials.Where(t => t.AST_CartTaskItemId != null).ToList();
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
                                                }
                                            }

                                            //解除线边位置停靠
                                            cfgWorkStationCurrentCart.CFG_CartId = null;
                                            cfgWorkStationCurrentCart.DockedTime = null;
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
