using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using DataAccess.AssemblyIndicating;
using DataAccess.Assorting;
using DataAccess.AssortingPDA;
using DataAccess.Distributing;

namespace DataAccess
{
    /// <summary>
    /// 历史纪录定时删除。
    /// </summary>
    public class HistoryRecordsRemover
    {
        static readonly HistoryRecordsRemover instance = new HistoryRecordsRemover();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static HistoryRecordsRemover Instance
        {
            get { return HistoryRecordsRemover.instance; }
        }

        readonly TimeSpan threadPeriod = TimeSpan.FromMinutes(1);
        Thread thread;
        bool threadNeedQuit;

        int holdingDays = int.MaxValue;

        /// <summary>
        /// 获取是否正在运行。
        /// </summary>
        public bool IsRunning { get; private set; }

        HistoryRecordsRemover()
        { }

        /// <summary>
        /// 启动定时删除服务。
        /// </summary>
        /// <param name="holdingDays">历史纪录保存的天数。</param>
        public void Start(int holdingDays)
        {
            this.holdingDays = holdingDays;

            if (this.thread == null)
            {
                this.thread = new Thread(this.threadStart);
                this.thread.Name = this.GetType().FullName;
                this.thread.IsBackground = true;

                this.threadNeedQuit = false;
                this.thread.Start();
            }

            this.IsRunning = true;
        }

        /// <summary>
        /// 停止自动删除服务。
        /// </summary>
        public void Stop()
        {
            if (this.thread != null)
            {
                this.threadNeedQuit = true;
                try { this.thread.Join(); }
                catch { }

                this.thread = null;
            }

            this.IsRunning = false;
        }

        void threadStart(object unused)
        {
            while (!this.threadNeedQuit)
            {
                try
                {
                    if (this.holdingDays > 0 && this.holdingDays < TimeSpan.MaxValue.TotalDays)
                    {
                        DateTime minTime = DateTime.Today.Subtract(TimeSpan.FromDays(this.holdingDays));

                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            //装配指引部分
                            List<ASM_AssembleIndication> asmAssembleIndications = dbContext.ASM_AssembleIndications
                                                                                      .Where(ai => ai.CarArrivedTime < minTime
                                                                                                   && ai.AssembleStatus == AssembleStatus.Finished
                                                                                                   && ai.ASM_AssembleResults.All(ar => ar.ASM_AssembleResultMessage.SentSuccessful))
                                                                                      .ToList();
                            foreach (ASM_AssembleIndication asmAssembleIndication in asmAssembleIndications)
                            {
                                ASM_AssembleIndicationMessage asmAssembleIndicationMessage = asmAssembleIndication.ASM_AssembleIndicationMessage;
                                List<ASM_AssembleIndicationItem> asmAssembleIndicationItems = asmAssembleIndication.ASM_AssembleIndicationItems.ToList();
                                List<ASM_Task> asmTasks = asmAssembleIndication.ASM_Tasks.ToList();
                                List<ASM_AssembleResult> asmAssembleResults = asmAssembleIndication.ASM_AssembleResults.ToList();

                                foreach (ASM_AssembleResult asmAssembleResult in asmAssembleResults)
                                {
                                    ASM_AssembleResultMessage asmAssembleResultMessage = asmAssembleResult.ASM_AssembleResultMessage;
                                    List<ASM_AssembleResultItem> asmAssembleResultItems = asmAssembleResult.ASM_AssembleResultItems.ToList();

                                    dbContext.ASM_AssembleResultItems.RemoveRange(asmAssembleResultItems);
                                    dbContext.ASM_AssembleResultMessages.Remove(asmAssembleResultMessage);
                                    dbContext.ASM_AssembleResults.Remove(asmAssembleResult);
                                }

                                foreach (ASM_Task asmTask in asmTasks)
                                {
                                    List<ASM_TaskItem> asmTaskItems = asmTask.ASM_TaskItems.ToList();

                                    dbContext.ASM_TaskItems.RemoveRange(asmTaskItems);
                                    dbContext.ASM_Tasks.Remove(asmTask);
                                }

                                dbContext.ASM_AssembleIndicationItems.RemoveRange(asmAssembleIndicationItems);
                                dbContext.ASM_AssembleIndicationMessages.Remove(asmAssembleIndicationMessage);
                                dbContext.ASM_AssembleIndications.Remove(asmAssembleIndication);
                            }

                            //分拣部分
                            List<AST_PalletArrived> astPalletArriveds = dbContext.AST_PalletArriveds
                                                                            .Where(pa => pa.ArrivedTime < minTime)
                                                                            .ToList();
                            foreach (AST_PalletArrived astPalletArrived in astPalletArriveds)
                            {
                                AST_PalletArrivedMessage astPalletArrivedMessage = astPalletArrived.AST_PalletArrivedMessage;

                                dbContext.AST_PalletArrivedMessages.Remove(astPalletArrivedMessage);
                                dbContext.AST_PalletArriveds.Remove(astPalletArrived);
                            }

                            List<AST_PalletTask> astPalletTasks = dbContext.AST_PalletTasks
                                                                      .Where(pt => pt.CreateTime < minTime
                                                                                   && pt.PickStatus == PickStatus.Finished)
                                                                      .ToList();
                            foreach (AST_PalletTask astPalletTask in astPalletTasks)
                            {
                                List<AST_PalletTaskItem> astPalletTaskItems = astPalletTask.AST_PalletTaskItems.ToList();
                                foreach (AST_PalletTaskItem astPalletTaskItem in astPalletTaskItems)
                                {
                                    List<AST_LesTaskItem> astLesTaskItems = astPalletTaskItem.AST_LesTaskItems.ToList();
                                    List<AST_CartTaskItem> astCartTaskItems = astPalletTaskItem.AST_CartTaskItems.ToList();

                                    List<AST_LesTask> astLesTasks = new List<AST_LesTask>();
                                    List<AST_LesTaskMessage> astLesTaskMessages = new List<AST_LesTaskMessage>();
                                    foreach (AST_LesTaskItem astLesTaskItem in astLesTaskItems)
                                    {
                                        AST_LesTask astLesTask = astLesTaskItem.AST_LesTask;
                                        AST_LesTaskMessage astLesTaskMessage = astLesTask.AST_LesTaskMessage;

                                        if (astLesTask != null && !astLesTasks.Contains(astLesTask))
                                            astLesTasks.Add(astLesTask);
                                        if (astLesTaskMessage != null && !astLesTaskMessages.Contains(astLesTaskMessage))
                                            astLesTaskMessages.Add(astLesTaskMessage);

                                        dbContext.AST_LesTaskItems.Remove(astLesTaskItem);
                                    }
                                    dbContext.AST_LesTaskMessages.RemoveRange(astLesTaskMessages);
                                    dbContext.AST_LesTasks.RemoveRange(astLesTasks);

                                    List<AST_CartTask> astCartTasks = new List<AST_CartTask>();
                                    foreach (AST_CartTaskItem astCartTaskItem in astCartTaskItems)
                                    {
                                        AST_CartTask astCartTask = astCartTaskItem.AST_CartTask;

                                        if (astCartTask != null && !astCartTasks.Contains(astCartTask))
                                            astCartTasks.Add(astCartTask);

                                        dbContext.AST_CartTaskItems.Remove(astCartTaskItem);
                                    }
                                    dbContext.AST_CartTasks.RemoveRange(astCartTasks);

                                    dbContext.AST_PalletTaskItems.Remove(astPalletTaskItem);
                                }

                                dbContext.AST_PalletTasks.Remove(astPalletTask);
                            }

                            List<AST_PalletResult> astPalletResults = dbContext.AST_PalletResults
                                                                          .Where(pr => pr.EndPickTime < minTime
                                                                                       && pr.AST_PalletResultMessage.SentSuccessful)
                                                                          .ToList();
                            foreach (AST_PalletResult astPalletResult in astPalletResults)
                            {
                                AST_PalletResultMessage astPalletResultMessage = astPalletResult.AST_PalletResultMessage;
                                List<AST_PalletResultItem> astPalletResultItems = astPalletResult.AST_PalletResultItems.ToList();

                                dbContext.AST_PalletResultItems.RemoveRange(astPalletResultItems);
                                dbContext.AST_PalletResultMessages.Remove(astPalletResultMessage);
                                dbContext.AST_PalletResults.Remove(astPalletResult);
                            }

                            List<AST_CartResult> astCartResults = dbContext.AST_CartResults
                                                                      .Where(cr => cr.EndPickTime < minTime
                                                                                   && cr.AST_CartResultMessage.SentSuccessful)
                                                                      .ToList();
                            foreach (AST_CartResult astCartResult in astCartResults)
                            {
                                AST_CartResultMessage astCartResultMessage = astCartResult.AST_CartResultMessage;
                                List<AST_CartResultItem> astCartResultItems = astCartResult.AST_CartResultItems.ToList();

                                dbContext.AST_CartResultItems.RemoveRange(astCartResultItems);
                                dbContext.AST_CartResultMessages.Remove(astCartResultMessage);
                                dbContext.AST_CartResults.Remove(astCartResult);
                            }

                            //手持机分拣部分
                            List<AST_PalletArrived_PDA> astPalletArrived_PDAs = dbContext.AST_PalletArrived_PDAs
                                                                            .Where(pa => pa.ArrivedTime < minTime)
                                                                            .ToList();
                            foreach (AST_PalletArrived_PDA astPalletArrived_PDA in astPalletArrived_PDAs)
                            {
                                AST_PalletArrivedMessage_PDA astPalletArrivedMessage_PDA = astPalletArrived_PDA.AST_PalletArrivedMessage_PDA;

                                dbContext.AST_PalletArrivedMessage_PDAs.Remove(astPalletArrivedMessage_PDA);
                                dbContext.AST_PalletArrived_PDAs.Remove(astPalletArrived_PDA);
                            }

                            List<AST_PalletTask_PDA> astPalletTask_PDAs = dbContext.AST_PalletTask_PDAs
                                                                      .Where(pt => pt.CreateTime < minTime
                                                                                   && pt.PickStatus == PickStatus.Finished)
                                                                      .ToList();
                            foreach (AST_PalletTask_PDA astPalletTask_PDA in astPalletTask_PDAs)
                            {
                                List<AST_PalletTaskItem_PDA> astPalletTaskItem_PDAs = astPalletTask_PDA.AST_PalletTaskItem_PDAs.ToList();
                                foreach (AST_PalletTaskItem_PDA astPalletTaskItem_PDA in astPalletTaskItem_PDAs)
                                {
                                    List<AST_LesTaskItem_PDA> astLesTaskItem_PDAs = astPalletTaskItem_PDA.AST_LesTaskItem_PDAs.ToList();

                                    List<AST_LesTask_PDA> astLesTask_PDAs = new List<AST_LesTask_PDA>();
                                    List<AST_LesTaskMessage_PDA> astLesTaskMessage_PDAs = new List<AST_LesTaskMessage_PDA>();
                                    foreach (AST_LesTaskItem_PDA astLesTaskItem_PDA in astLesTaskItem_PDAs)
                                    {
                                        // AST_LesTaskItem_PDA -> AST_LesTask_PDA FK ERROR
                                        AST_LesTask_PDA astLesTask_PDA = astLesTaskItem_PDA.AST_LesTask_PDA;
                                        AST_LesTaskMessage_PDA astLesTaskMessage_PDA = null;
                                        if (astLesTask_PDA != null)
                                            astLesTaskMessage_PDA = astLesTask_PDA.AST_LesTaskMessage_PDA;

                                        if (astLesTask_PDA != null && !astLesTask_PDAs.Contains(astLesTask_PDA))
                                            astLesTask_PDAs.Add(astLesTask_PDA);
                                        if (astLesTaskMessage_PDA != null && !astLesTaskMessage_PDAs.Contains(astLesTaskMessage_PDA))
                                            astLesTaskMessage_PDAs.Add(astLesTaskMessage_PDA);

                                        dbContext.AST_LesTaskItem_PDAs.Remove(astLesTaskItem_PDA);
                                    }
                                    dbContext.AST_LesTaskMessage_PDAs.RemoveRange(astLesTaskMessage_PDAs);
                                    dbContext.AST_LesTask_PDAs.RemoveRange(astLesTask_PDAs);

                                    dbContext.AST_PalletTaskItem_PDAs.Remove(astPalletTaskItem_PDA);
                                }

                                dbContext.AST_PalletTask_PDAs.Remove(astPalletTask_PDA);
                            }

                            List<AST_PalletPickResult_PDA> astPalletPickResults = dbContext.AST_PalletPickResult_PDAs
                                                                            .Where(pa => pa.ReceivedTime < minTime)
                                                                            .ToList();
                            foreach (AST_PalletPickResult_PDA astPalletPickResult in astPalletPickResults)
                            {
                                dbContext.AST_PalletPickResult_PDAs.Remove(astPalletPickResult);
                            }

                            List<AST_PalletPickResultMessage_PDA> astPalletPickResultMessages = dbContext.AST_PalletPickResultMessage_PDAs
                                .Where(t => t.ReceivedTime < minTime).ToList();
                            foreach (AST_PalletPickResultMessage_PDA astPalletPickResultMessage in astPalletPickResultMessages)
                            {
                                dbContext.AST_PalletPickResultMessage_PDAs.Remove(astPalletPickResultMessage);
                            }

                            //AGV搬运部分
                            List<DST_DistributeTask> dstDistributeTasks = dbContext.DST_DistributeTasks.Where(t => t.reqTime < minTime).ToList();
                            foreach (DST_DistributeTask dstDistributeTask in dstDistributeTasks)
                            {
                                dbContext.DST_DistributeTasks.Remove(dstDistributeTask);
                            }

                            List<DST_DistributeTaskResult> dstDistributeTaskResults = dbContext.DST_DistributeTaskResults.Where(t => t.receiveTime < minTime).ToList();
                            foreach (DST_DistributeTaskResult dstDistributeTaskResult in dstDistributeTaskResults)
                            {
                                dbContext.DST_DistributeTaskResults.Remove(dstDistributeTaskResult);
                            }

                            List<DST_DistributeArriveTask> dstDistributeArriveTasks = dbContext.DST_DistributeArriveTasks.Where(t => t.receiveTime < minTime).ToList();
                            foreach (DST_DistributeArriveTask dstDistributeArriveTask in dstDistributeArriveTasks)
                            {
                                dbContext.DST_DistributeArriveTasks.Remove(dstDistributeArriveTask);
                            }

                            List<DST_DistributeArriveTaskResult> dstDistributeArriveTaskResults = dbContext.DST_DistributeArriveTaskResults.Where(t => t.sendTime < minTime).ToList();
                            foreach (DST_DistributeArriveTaskResult dstDistributeArriveTaskResult in dstDistributeArriveTaskResults)
                            {
                                dbContext.DST_DistributeArriveTaskResults.Remove(dstDistributeArriveTaskResult);
                            }

                            List<DST_DistributeArriveResult> dstDistributeArriveResults = dbContext.DST_DistributeArriveResults.Where(t => t.arriveTime < minTime).ToList();
                            foreach (DST_DistributeArriveResult dstDistributeArriveResult in dstDistributeArriveResults)
                            {
                                dbContext.DST_DistributeArriveResults.Remove(dstDistributeArriveResult);
                            }

                            dbContext.SaveChanges();
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

                    Logger.Log(this.GetType().Name, DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                                     + message + Environment.NewLine
                                                                     + Environment.NewLine);
                }
                finally
                {
                    Thread.Sleep(this.threadPeriod);
                }
            }
        }
    }
}