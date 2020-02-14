using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.AssemblyIndicating;

namespace AssemblyIndicating
{
    /// <summary>
    /// 实时和轮询发现需要指引的作业，并调度到各工位自己的执行器。
    /// </summary>
    public class IndicatingExecutorLoader
    {
        static readonly IndicatingExecutorLoader instance = new IndicatingExecutorLoader();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static IndicatingExecutorLoader Instance
        {
            get { return IndicatingExecutorLoader.instance; }
        }

        readonly Dictionary<int, IndicatingExecutor> executorByWorkCenterId = new Dictionary<int, IndicatingExecutor>();

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(1);
        Thread thread;
        bool threadNeedQuit;

        readonly object refreshSyncRoot = new object();

        /// <summary>
        /// 获取是否运行中。
        /// </summary>
        public bool IsRunning { get; private set; }

        IndicatingExecutorLoader()
        { }

        /// <summary>
        /// 启动定时加载线程。
        /// </summary>
        public void Start()
        {
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
                    List<ASM_Task> asmTasks = dbContext.ASM_Tasks
                                                  .Where(t => t.AssembleStatus != AssembleStatus.Finished)
                                                  .ToList();

                    foreach (ASM_Task asmTask in asmTasks)
                    {
                        //动态准备工位执行器
                        int cfgWorkStationId = asmTask.ASM_AssembleIndication.CFG_WorkStationId;
                        if (!this.executorByWorkCenterId.ContainsKey(cfgWorkStationId))
                            this.executorByWorkCenterId.Add(cfgWorkStationId, new IndicatingExecutor(cfgWorkStationId));

                        IndicatingExecutor indicatingExecutor = this.executorByWorkCenterId[cfgWorkStationId];
                        if (indicatingExecutor.CurrentAsmTaskId == null)
                            indicatingExecutor.Start(asmTask.Id);
                    }
                }
            }
        }
    }
}