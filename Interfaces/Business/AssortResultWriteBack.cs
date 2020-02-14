using System;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.Assorting;
using Interfaces.Converters;
using Interfaces.PtlToLes;

namespace Interfaces.Business
{
    /// <summary>
    /// 实时和轮询提交分拣结果给 LES，有按托和按小车两种。
    /// </summary>
    public class AssortResultWriteBack
    {
        static readonly AssortResultWriteBack instance = new AssortResultWriteBack();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static AssortResultWriteBack Instance
        {
            get { return AssortResultWriteBack.instance; }
        }

        string ptlToLesServiceUrl;

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(5);
        Thread thread;
        bool threadNeedQuit;

        /// <summary>
        /// 获取是否运行中。
        /// </summary>
        public bool IsRunning { get; private set; }

        AssortResultWriteBack()
        { }

        /// <summary>
        /// 启动回写线程。
        /// </summary>
        /// <param name="ptlToLesServiceUrl">回写的 LES 服务地址。</param>
        public void Start(string ptlToLesServiceUrl)
        {
            this.ptlToLesServiceUrl = ptlToLesServiceUrl;

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
                    this.WriteBackFirstPalletResult();
                    this.WriteBackFirstCartResult();
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

        void WriteBackFirstPalletResult()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                AST_PalletResultMessage astPalletResultMessage = dbContext.AST_PalletResultMessages
                                                                     .Where(prm => !prm.SentSuccessful)
                                                                     .OrderBy(prm => prm.LastSentTime)
                                                                     .FirstOrDefault();
                if (astPalletResultMessage != null)
                {
                    using (PtlToLesServiceService proxy = new PtlToLesServiceService())
                    {
                        proxy.Url = this.ptlToLesServiceUrl;

                        string sendXml = string.Empty;
                        string receivedXml = string.Empty;
                        string errorMessage = string.Empty;
                        bool successful = false;
                        try
                        {
                            sendXml = AST_PalletResultConverter.ConvertRequest(astPalletResultMessage.AST_PalletResult);
                            receivedXml = proxy.PTLPalletPickBackLes(sendXml);
                            successful = AST_PalletResultConverter.CheckResponse(receivedXml, out errorMessage);
                        }
                        catch (Exception ex)
                        {
                            receivedXml = ex.ToString();
                        }

                        astPalletResultMessage.SentXml = sendXml;
                        astPalletResultMessage.LastSentTime = DateTime.Now;
                        astPalletResultMessage.SentSuccessful = successful;
                        astPalletResultMessage.ReceivedXml = receivedXml;
                    }
                }

                dbContext.SaveChanges();
            }
        }

        void WriteBackFirstCartResult()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                AST_CartResultMessage astCartResultMessage = dbContext.AST_CartResultMessages
                                                                 .Where(prm => !prm.SentSuccessful)
                                                                 .OrderBy(prm => prm.LastSentTime)
                                                                 .FirstOrDefault();
                if (astCartResultMessage != null)
                {
                    using (PtlToLesServiceService proxy = new PtlToLesServiceService())
                    {
                        proxy.Url = this.ptlToLesServiceUrl;

                        string sendXml = string.Empty;
                        string receivedXml = string.Empty;
                        string errorMessage = string.Empty;
                        bool successful = false;
                        try
                        {
                            sendXml = AST_CartResultConverter.ConvertRequest(astCartResultMessage.AST_CartResult);
                            receivedXml = proxy.PTLCartPickBackLes(sendXml);
                            successful = AST_CartResultConverter.CheckResponse(receivedXml, out errorMessage);
                        }
                        catch (Exception ex)
                        {
                            receivedXml = ex.ToString();
                        }

                        astCartResultMessage.SentXml = sendXml;
                        astCartResultMessage.LastSentTime = DateTime.Now;
                        astCartResultMessage.SentSuccessful = successful;
                        astCartResultMessage.ReceivedXml = receivedXml;
                    }
                }

                dbContext.SaveChanges();
            }
        }
    }
}