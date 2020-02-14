using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using Aris.KEPServerExRelay.Service.Client.KEPServerExRelayService;
using Aris.KEPServerExRelay.Service.Shared;

namespace DeviceCommunicationHost
{
    /// <summary>
    /// KEPServerEx 中继服务的代理，用于保持连接。
    /// </summary>
    class KEPServerExRelayServiceProxy : IDisposable
    {
        static readonly KEPServerExRelayServiceProxy instance = new KEPServerExRelayServiceProxy();

        public static KEPServerExRelayServiceProxy Instance
        {
            get { return KEPServerExRelayServiceProxy.instance; }
        }

        string serverIP = "localhost";
        int servicePort = 9034;
        OpcProject project;

        readonly TimeSpan ensureClientPeriod = TimeSpan.FromSeconds(1);
        object clientSyncRoot = new object();
        KEPServerExRelayServiceCallback serviceCallback;
        KEPServerExRelayServiceClient serviceClient;
        bool needRestart;

        Thread ensureClientConnectedThread;
        bool ensureClientConnectedThreadNeedQuit;

        KEPServerExRelayServiceProxy()
        { }

        /// <summary>
        /// 获取是否正在运行。
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 获取是否建立连接。
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// 接力 OPCGroup.DataChange 事件。
        /// </summary>
        public event EventHandler<OpcGroupDataChangeEventArgs> DataChange;

        /// <summary>
        /// 引发 DataChange 事件。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected virtual void OnDataChange(OpcGroupDataChangeEventArgs e)
        {
            EventHandler<OpcGroupDataChangeEventArgs> handler = this.DataChange;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// 启动代理。
        /// </summary>
        public void Start(string serverIP, int servicePort, OpcProject project)
        {
            this.Stop();

            this.serverIP = serverIP;
            this.servicePort = servicePort;
            this.project = project;

            this.ensureClientConnectedThread = new Thread(this.ensureClientConnectedThreadStart);
            this.ensureClientConnectedThread.Name = string.Format(CultureInfo.InvariantCulture, "{0}.ensureClientConnectedThreadStart", this.GetType().FullName);
            this.ensureClientConnectedThread.IsBackground = true;
            this.ensureClientConnectedThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            this.ensureClientConnectedThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

            this.needRestart = true;
            this.ensureClientConnectedThreadNeedQuit = false;
            this.ensureClientConnectedThread.Start();

            this.IsRunning = true;
        }

        /// <summary>
        /// 停止代理。
        /// </summary>
        public void Stop()
        {
            if (this.ensureClientConnectedThread != null)
            {
                this.ensureClientConnectedThreadNeedQuit = true;
                try { this.ensureClientConnectedThread.Join(); }
                catch { }

                this.ensureClientConnectedThread = null;
            }

            this.ReleaseClient();

            this.IsRunning = false;
        }

        void ensureClientConnectedThreadStart()
        {
            while (!this.ensureClientConnectedThreadNeedQuit)
            {
                try
                {
                    lock (this.clientSyncRoot)
                    {
                        if (this.serviceClient == null)
                        {
                            Uri serviceUrl = new Uri(string.Format(CultureInfo.InvariantCulture, "net.tcp://{0}:{1}/KEPServerExRelay/", this.serverIP, this.servicePort));

                            this.serviceCallback = new KEPServerExRelayServiceCallback();
                            this.serviceCallback.DataChange += this.serviceCallback_DataChange;

                            this.serviceClient = new KEPServerExRelayServiceClient(new InstanceContext(this.serviceCallback));
                            this.serviceClient.Endpoint.Address = new EndpointAddress(serviceUrl, this.serviceClient.Endpoint.Address.Identity);
                            this.serviceClient.Open();

                            this.serviceClient.InnerDuplexChannel.Faulted += this.clientInnerChannel_Faulted;

                            Subscriber subscriber = new Subscriber();
                            subscriber.Type = this.GetType().FullName;
                            subscriber.Name = string.Empty;
                            this.serviceClient.Subscribe(subscriber);

                            if (this.needRestart)
                            {
                                if (this.serviceClient.GetIsRunning())
                                    this.serviceClient.Stop();
                            }

                            if (!this.serviceClient.GetIsRunning())
                                this.serviceClient.Start(this.project);

                            this.needRestart = false;
                        }
                    }

                    this.IsConnected = true;
                }
                catch
                {
                    this.ReleaseClient();

                    DateTime sleepBegin = DateTime.Now;
                    while (!this.ensureClientConnectedThreadNeedQuit && (DateTime.Now - sleepBegin) < this.ensureClientPeriod)
                        Thread.Sleep(1);
                }

                Thread.Sleep(1);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "资源释放不应抛出异常")]
        void ReleaseClient()
        {
            // ClientBase.Close() 可能会引发 ICommunicationObject.Faulted，继而再次调用本方法，所以 ClientBase.Abort() 前需要判断非空
            lock (this.clientSyncRoot)
            {
                if (this.serviceCallback != null)
                {
                    this.serviceCallback.DataChange -= this.serviceCallback_DataChange;

                    this.serviceCallback = null;
                }

                if (this.serviceClient != null)
                {
                    try
                    {
                        if (this.serviceClient.State == CommunicationState.Opened)
                            this.serviceClient.Unsubscribe();

                        this.serviceClient.Close();
                    }
                    catch
                    {
                        if (this.serviceClient != null)
                            this.serviceClient.Abort();
                    }

                    this.serviceClient = null;
                }
            }

            this.IsConnected = false;
        }

        void clientInnerChannel_Faulted(object sender, EventArgs e)
        {
            ((ICommunicationObject)sender).Faulted -= this.clientInnerChannel_Faulted;

            this.ReleaseClient();

            this.needRestart = true;
        }

        void serviceCallback_DataChange(object sender, OpcGroupDataChangeEventArgs e)
        {
            this.OnDataChange(e);
        }

        /// <summary>
        /// 释放托管和非托管资源。
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放非托管资源并可选地释放托管资源。
        /// </summary>
        /// <param name="disposing">是否释放托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            this.Stop();

            if (disposing)
            {
                this.clientSyncRoot = null;
            }
        }

        /// <summary>
        /// 释放非托管资源。
        /// </summary>
        ~KEPServerExRelayServiceProxy()
        {
            this.Dispose(false);
        }
    }
}