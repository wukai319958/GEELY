using System;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.CartFinding;
using Interfaces.Converters;
using Interfaces.PtlToLes;

namespace Interfaces.Business
{
    /// <summary>
    /// 待删除，无需回写配送信息。
    /// </summary>
    public class CartFindingDeliveryResultWriteBack
    {
        static readonly CartFindingDeliveryResultWriteBack instance = new CartFindingDeliveryResultWriteBack();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static CartFindingDeliveryResultWriteBack Instance
        {
            get { return CartFindingDeliveryResultWriteBack.instance; }
        }

        string ptlToLesServiceUrl;

        readonly TimeSpan threadPeriod = TimeSpan.FromSeconds(5);
        Thread thread;
        bool threadNeedQuit;

        /// <summary>
        /// 获取是否运行中。
        /// </summary>
        public bool IsRunning { get; private set; }

        CartFindingDeliveryResultWriteBack()
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
                    this.WriteBackFirstDeliveryResult();
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

        void WriteBackFirstDeliveryResult()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                FND_DeliveryResultMessage fndDeliveryResultMessage = dbContext.FND_DeliveryResultMessages
                                                                         .Where(prm => !prm.SentSuccessful)
                                                                         .OrderBy(prm => prm.LastSentTime)
                                                                         .FirstOrDefault();
                if (fndDeliveryResultMessage != null)
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
                            sendXml = FND_DeliveryResultConverter.ConvertRequest(fndDeliveryResultMessage.FND_DeliveryResult);
                            receivedXml = proxy.PTLCartDepart(sendXml);
                            successful = FND_DeliveryResultConverter.CheckResponse(receivedXml, out errorMessage);
                        }
                        catch (Exception ex)
                        {
                            receivedXml = ex.ToString();
                        }

                        fndDeliveryResultMessage.SentXml = sendXml;
                        fndDeliveryResultMessage.LastSentTime = DateTime.Now;
                        fndDeliveryResultMessage.SentSuccessful = successful;
                        fndDeliveryResultMessage.ReceivedXml = receivedXml;
                    }
                }

                dbContext.SaveChanges();
            }
        }
    }
}