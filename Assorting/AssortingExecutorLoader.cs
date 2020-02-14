using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.Config;

namespace Assorting
{
    /// <summary>
    /// 实时和轮询发现需要分拣任务，并调度到各巷道自己的执行器。
    /// </summary>
    public class AssortingExecutorLoader
    {
        static readonly AssortingExecutorLoader instance = new AssortingExecutorLoader();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static AssortingExecutorLoader Instance
        {
            get { return AssortingExecutorLoader.instance; }
        }

        readonly Dictionary<int, AssortingExecutor> executorByChannelId = new Dictionary<int, AssortingExecutor>();

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(1);
        Thread thread;
        bool threadNeedQuit;

        readonly object refreshSyncRoot = new object();

        /// <summary>
        /// 获取是否运行中。
        /// </summary>
        public bool IsRunning { get; private set; }

        AssortingExecutorLoader()
        { }

        /// <summary>
        /// 启动定时加载线程。
        /// </summary>
        public void Start()
        {
            if (this.executorByChannelId.Count == 0)
                this.PrepareExecutors();

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
        /// 为每个巷道准备执行器。
        /// </summary>
        void PrepareExecutors()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<CFG_Channel> cfgChannels = dbContext.CFG_Channels
                                                    .ToList();
                foreach (CFG_Channel cfgChannel in cfgChannels)
                {
                    AssortingExecutor assortingExecutor = new AssortingExecutor(cfgChannel.Id);

                    this.executorByChannelId.Add(cfgChannel.Id, assortingExecutor);
                }
            }
        }

        /// <summary>
        /// 按分拣口获取其执行器。
        /// </summary>
        /// <param name="cfgChannelId">分拣口主键。</param>
        public AssortingExecutor GetByChannelId(int cfgChannelId)
        {
            return this.executorByChannelId[cfgChannelId];
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
                    List<AST_PalletTask> astPalletTasks = dbContext.AST_PalletTasks
                                                              .Where(t => t.PickStatus != PickStatus.Finished)
                                                              .ToList();

                    foreach (AST_PalletTask astPalletTask in astPalletTasks)
                    {
                        AssortingExecutor assortingExecutor = this.executorByChannelId[astPalletTask.CFG_ChannelId];
                        if (assortingExecutor.CurrentAstPalletTaskId == null)
                            assortingExecutor.Start(astPalletTask.Id);
                    }
                }
            }
        }
    }
}