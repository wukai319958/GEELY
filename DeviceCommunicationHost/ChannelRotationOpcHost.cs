using System.Collections.Generic;
using System.Linq;
using Aris.KEPServerExRelay.Service.Shared;
using DataAccess;
using DataAccess.Config;

namespace DeviceCommunicationHost
{
    /// <summary>
    /// 分拣口转台的 OPC 点位状态监视宿主。
    /// </summary>
    public class ChannelRotationOpcHost
    {
        static readonly ChannelRotationOpcHost instance = new ChannelRotationOpcHost();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static ChannelRotationOpcHost Instance
        {
            get { return ChannelRotationOpcHost.instance; }
        }

        readonly Dictionary<int, ChannelRotationOpc> channelRotationOpcByChannelId = new Dictionary<int, ChannelRotationOpc>();

        /// <summary>
        /// 获取通讯是否运行中。
        /// </summary>
        public bool IsRunning
        {
            get { return KEPServerExRelayServiceProxy.Instance.IsConnected; }
        }

        readonly string opcGroupName = "PTL.ZhuanTai";
        readonly int opcUpdateRate = 50;
        readonly string opcItemChannel1RotationStartName = "PTL.ZhuanTai.ZTDJ1_ZZ_SD";
        readonly string opcItemChannel1ReverseRotationStartName = "PTL.ZhuanTai.ZTDJ1_FZ_SD";
        readonly string opcItemChannel2RotationStartName = "PTL.ZhuanTai.ZTDJ2_ZZ_SD";
        readonly string opcItemChannel2ReverseRotationStartName = "PTL.ZhuanTai.ZTDJ2_FZ_SD";
        readonly string opcItemChannel3RotationStartName = "PTL.ZhuanTai.ZTDJ3_ZZ_SD";
        readonly string opcItemChannel3ReverseRotationStartName = "PTL.ZhuanTai.ZTDJ3_FZ_SD";
        readonly string opcItemChannel4RotationStartName = "PTL.ZhuanTai.ZTDJ4_ZZ_SD";
        readonly string opcItemChannel4ReverseRotationStartName = "PTL.ZhuanTai.ZTDJ4_FZ_SD";
        readonly string opcItemChannel5RotationStartName = "PTL.ZhuanTai.ZTDJ5_ZZ_SD";
        readonly string opcItemChannel5ReverseRotationStartName = "PTL.ZhuanTai.ZTDJ5_FZ_SD";
        readonly string opcItemChannel6RotationStartName = "PTL.ZhuanTai.ZTDJ6_ZZ_SD";
        readonly string opcItemChannel6ReverseRotationStartName = "PTL.ZhuanTai.ZTDJ6_FZ_SD";
        readonly string opcItemChannel7RotationStartName = "PTL.ZhuanTai.ZTDJ7_ZZ_SD";
        readonly string opcItemChannel7ReverseRotationStartName = "PTL.ZhuanTai.ZTDJ7_FZ_SD";

        OpcProject opcProject;

        ChannelRotationOpcHost()
        { }

        /// <summary>
        /// 启动监听。
        /// </summary>
        public void Start(string serviceName, string serverIP)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<CFG_Channel> cfgChannels = dbContext.CFG_Channels
                                                    .OrderBy(c => c.Code)
                                                    .ToList();
                foreach (CFG_Channel cfgChannel in cfgChannels)
                {
                    ChannelRotationOpc channelRotationOpc = new ChannelRotationOpc(cfgChannel.Id);
                    this.channelRotationOpcByChannelId.Add(cfgChannel.Id, channelRotationOpc);
                }
            }

            OpcItem opcItemChannel1RotationStart = new OpcItem { ID = this.opcItemChannel1RotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel1ReverseRotationStart = new OpcItem { ID = this.opcItemChannel1ReverseRotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel2RotationStart = new OpcItem { ID = this.opcItemChannel2RotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel2ReverseRotationStart = new OpcItem { ID = this.opcItemChannel2ReverseRotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel3RotationStart = new OpcItem { ID = this.opcItemChannel3RotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel3ReverseRotationStart = new OpcItem { ID = this.opcItemChannel3ReverseRotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel4RotationStart = new OpcItem { ID = this.opcItemChannel4RotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel4ReverseRotationStart = new OpcItem { ID = this.opcItemChannel4ReverseRotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel5RotationStart = new OpcItem { ID = this.opcItemChannel5RotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel5ReverseRotationStart = new OpcItem { ID = this.opcItemChannel5ReverseRotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel6RotationStart = new OpcItem { ID = this.opcItemChannel6RotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel6ReverseRotationStart = new OpcItem { ID = this.opcItemChannel6ReverseRotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel7RotationStart = new OpcItem { ID = this.opcItemChannel7RotationStartName, DataType = OpcDataType.Boolean };
            OpcItem opcItemChannel7ReverseRotationStart = new OpcItem { ID = this.opcItemChannel7ReverseRotationStartName, DataType = OpcDataType.Boolean };

            OpcGroup opcGroup = new OpcGroup();
            opcGroup.Name = this.opcGroupName;
            opcGroup.UpdateRate = this.opcUpdateRate;
            opcGroup.OpcItems.Add(opcItemChannel1RotationStart);
            opcGroup.OpcItems.Add(opcItemChannel1ReverseRotationStart);
            opcGroup.OpcItems.Add(opcItemChannel2RotationStart);
            opcGroup.OpcItems.Add(opcItemChannel2ReverseRotationStart);
            opcGroup.OpcItems.Add(opcItemChannel3RotationStart);
            opcGroup.OpcItems.Add(opcItemChannel3ReverseRotationStart);
            opcGroup.OpcItems.Add(opcItemChannel4RotationStart);
            opcGroup.OpcItems.Add(opcItemChannel4ReverseRotationStart);
            opcGroup.OpcItems.Add(opcItemChannel5RotationStart);
            opcGroup.OpcItems.Add(opcItemChannel5ReverseRotationStart);
            opcGroup.OpcItems.Add(opcItemChannel6RotationStart);
            opcGroup.OpcItems.Add(opcItemChannel6ReverseRotationStart);
            opcGroup.OpcItems.Add(opcItemChannel7RotationStart);
            opcGroup.OpcItems.Add(opcItemChannel7ReverseRotationStart);

            this.opcProject = new OpcProject();
            this.opcProject.ServerName = serviceName;
            this.opcProject.OpcGroups.Add(opcGroup);

            KEPServerExRelayServiceProxy.Instance.Start(serverIP, 9034, this.opcProject);
            KEPServerExRelayServiceProxy.Instance.DataChange += this.KEPServerExRelayServiceProxy_Instance_DataChange;
        }

        /// <summary>
        /// 停止监听。
        /// </summary>
        public void Stop()
        {
            if (this.opcProject != null)
            {
                KEPServerExRelayServiceProxy.Instance.DataChange -= this.KEPServerExRelayServiceProxy_Instance_DataChange;
                KEPServerExRelayServiceProxy.Instance.Stop();

                this.opcProject = null;
            }

            this.channelRotationOpcByChannelId.Clear();
        }

        /// <summary>
        /// 获取指定转台的 OPC 点位订阅入口。
        /// </summary>
        /// <param name="cfgChannelId">巷道或转台的主键。</param>
        /// <returns>转台的 OPC 点位订阅入口。</returns>
        public ChannelRotationOpc GetChannelRotationOpc(int cfgChannelId)
        {
            return this.channelRotationOpcByChannelId[cfgChannelId];
        }

        void KEPServerExRelayServiceProxy_Instance_DataChange(object sender, OpcGroupDataChangeEventArgs e)
        {
            foreach (OpcGroupDataChangeEventArgsItem eItem in e.Items)
            {
                try
                {
                    //所有点位都是 bool 型
                    if (!string.IsNullOrEmpty(eItem.ValueText))
                    {
                        bool value = bool.Parse(eItem.ValueText);

                        if (value)
                        {
                            if (eItem.OpcItemID == this.opcItemChannel1RotationStartName)
                                this.channelRotationOpcByChannelId[1].PerformRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel1ReverseRotationStartName)
                                this.channelRotationOpcByChannelId[1].PerformReverseRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel2RotationStartName)
                                this.channelRotationOpcByChannelId[2].PerformRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel2ReverseRotationStartName)
                                this.channelRotationOpcByChannelId[2].PerformReverseRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel3RotationStartName)
                                this.channelRotationOpcByChannelId[3].PerformRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel3ReverseRotationStartName)
                                this.channelRotationOpcByChannelId[3].PerformReverseRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel4RotationStartName)
                                this.channelRotationOpcByChannelId[4].PerformRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel4ReverseRotationStartName)
                                this.channelRotationOpcByChannelId[4].PerformReverseRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel5RotationStartName)
                                this.channelRotationOpcByChannelId[5].PerformRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel5ReverseRotationStartName)
                                this.channelRotationOpcByChannelId[5].PerformReverseRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel6RotationStartName)
                                this.channelRotationOpcByChannelId[6].PerformRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel6ReverseRotationStartName)
                                this.channelRotationOpcByChannelId[6].PerformReverseRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel7RotationStartName)
                                this.channelRotationOpcByChannelId[7].PerformRotationStart();
                            else if (eItem.OpcItemID == this.opcItemChannel7ReverseRotationStartName)
                                this.channelRotationOpcByChannelId[7].PerformReverseRotationStart();
                        }
                    }
                }
                catch { }
            }
        }
    }
}