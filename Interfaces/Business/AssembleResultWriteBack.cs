using System;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.AssemblyIndicating;
using Interfaces.Converters;
using Interfaces.PtlToMes;

namespace Interfaces.Business
{
    /// <summary>
    /// 实时和轮询提交装配结果给 MES。
    /// </summary>
    public class AssembleResultWriteBack
    {
        static readonly AssembleResultWriteBack instance = new AssembleResultWriteBack();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static AssembleResultWriteBack Instance
        {
            get { return AssembleResultWriteBack.instance; }
        }

        string ptlToMesServiceUrl;

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(5);
        Thread thread;
        bool threadNeedQuit;

        /// <summary>
        /// 获取是否运行中。
        /// </summary>
        public bool IsRunning { get; private set; }

        AssembleResultWriteBack()
        { }

        /// <summary>
        /// 启动回写线程。
        /// </summary>
        /// <param name="ptlToMesServiceUrl">回写的 MES 服务地址。</param>
        public void Start(string ptlToMesServiceUrl)
        {
            this.ptlToMesServiceUrl = ptlToMesServiceUrl;

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
        /// 停止回写线程。
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
                    this.WriteBackFirstAssembleResult();
                }
                catch { }
                finally
                {
                    DateTime beginTime = DateTime.Now;

                    while (!this.threadNeedQuit && (DateTime.Now - beginTime) < this.threadPeriod)
                        Thread.Sleep(500);
                }
            }
        }

        void WriteBackFirstAssembleResult()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                ASM_AssembleResultMessage asmAssembleResultMessage = dbContext.ASM_AssembleResultMessages
                                                                         .Where(prm => !prm.SentSuccessful)
                                                                         .OrderBy(prm => prm.LastSentTime)
                                                                         .FirstOrDefault();
                if (asmAssembleResultMessage != null)
                {
                    using (ToMesRemoteServiceService proxy = new ToMesRemoteServiceService())
                    {
                        proxy.Url = this.ptlToMesServiceUrl;

                        string sendXml = string.Empty;
                        string receivedXml = string.Empty;
                        string errorMessage = string.Empty;
                        bool successful = false;
                        try
                        {
                            sendXml = ASM_AssembleResultConverter.ConvertRequest(asmAssembleResultMessage.ASM_AssembleResult);
                            receivedXml = proxy.assemblyMatResult(sendXml);
                            successful = ASM_AssembleResultConverter.CheckResponse(receivedXml, out errorMessage);
                        }
                        catch (Exception ex)
                        {
                            receivedXml = ex.ToString();
                        }

                        asmAssembleResultMessage.SentXml = sendXml;
                        asmAssembleResultMessage.LastSentTime = DateTime.Now;
                        asmAssembleResultMessage.SentSuccessful = successful;
                        asmAssembleResultMessage.ReceivedXml = receivedXml;
                    }
                }

                dbContext.SaveChanges();
            }
        }
    }
}