using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataAccess;
using DataAccess.Config;
using Ptl.Device;

namespace DeviceCommunicationHost
{
    /// <summary>
    /// 转台上 PTL 设备的通讯宿主。
    /// </summary>
    public class ChannelPtlHost
    {
        static readonly ChannelPtlHost instance = new ChannelPtlHost();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static ChannelPtlHost Instance
        {
            get { return ChannelPtlHost.instance; }
        }

        //所有巷道共用一个 XGate
        InstallProject installProject;
        XGate xGate;

        readonly Dictionary<int, ChannelPtl> channelPtlByChannelId = new Dictionary<int, ChannelPtl>();

        /// <summary>
        /// 获取通讯是否运行中。
        /// </summary>
        public bool IsRunning { get; private set; }

        ChannelPtlHost()
        { }

        /// <summary>
        /// 加载并启动所有转台的 PTL 通讯。
        /// </summary>
        public void Start()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<CFG_Channel> cfgChannels = dbContext.CFG_Channels
                                                    .ToList();
                CFG_ChannelPtlDevice firstCFG_ChannelPtlDevice = dbContext.CFG_ChannelPtlDevices
                                                                     .First();

                //单个 XGate
                this.xGate = new XGate(firstCFG_ChannelPtlDevice.XGateIP);
                this.installProject = new InstallProject();
                this.installProject.XGates.AddOrUpdate(this.xGate);

                foreach (CFG_Channel cfgChannel in cfgChannels)
                {
                    ChannelPtl channelPtl = new ChannelPtl(cfgChannel.Id);

                    this.channelPtlByChannelId.Add(cfgChannel.Id, channelPtl);

                    List<CFG_ChannelPtlDevice> cfgChannelPtlDevices = dbContext.CFG_ChannelPtlDevices
                                                                          .Where(cpd => cpd.CFG_ChannelId == cfgChannel.Id)
                                                                          .ToList();
                    //各个分拣口的指示灯
                    foreach (CFG_ChannelPtlDevice cfgChannelPtlDevice in cfgChannelPtlDevices)
                    {
                        Ptl900U ptl900U = (Ptl900U)this.xGate.Buses[cfgChannelPtlDevice.RS485BusIndex].Devices
                                                       .FirstOrDefault(d => d.Address == cfgChannelPtlDevice.Ptl900UAddress);
                        if (ptl900U == null)
                        {
                            ptl900U = new Ptl900U();
                            ptl900U.Address = cfgChannelPtlDevice.Ptl900UAddress;
                            ptl900U.MinorType = Ptl900UType.P0;

                            this.xGate.Buses[cfgChannelPtlDevice.RS485BusIndex].Devices.AddOrUpdate(ptl900U);
                        }

                        channelPtl.SetPtl900UByPosition(cfgChannelPtlDevice.Position, ptl900U);
                    }
                }

                this.xGate.StartUnicastCommandQueue();

                foreach (RS485Bus rs485Bus in this.xGate.Buses)
                {
                    foreach (PtlDevice ptlDevice in rs485Bus.Devices)
                    {
                        ptlDevice.InErrorChanged += this.ptlDevice_InErrorChanged;

                        ptlDevice.Initialize();

                        //转台上的标签始终锁定
                        ptlDevice.Lock();
                    }
                }

                this.installProject.HeartbeatGenerator.Period = TimeSpan.FromSeconds(10);
                this.installProject.HeartbeatGenerator.Enable = true;
            }

            this.IsRunning = true;
        }

        /// <summary>
        /// 停止所有转台的 PTL 通讯。
        /// </summary>
        public void Stop()
        {
            foreach (RS485Bus rs485Bus in this.xGate.Buses)
            {
                foreach (PtlDevice ptlDevice in rs485Bus.Devices)
                    ptlDevice.InErrorChanged -= this.ptlDevice_InErrorChanged;
            }

            this.xGate.StopUnicastCommandQueue();
            this.xGate.Dispose();
            this.xGate = null;

            this.installProject = null;

            this.channelPtlByChannelId.Clear();

            this.IsRunning = false;
        }

        /// <summary>
        /// 获取指定转台的 PTL 控制入口。
        /// </summary>
        /// <param name="cfgChannelId">巷道或转台的主键。</param>
        /// <returns>转台的 PTL 控制入口。</returns>
        public ChannelPtl GetChannelPtl(int cfgChannelId)
        {
            return this.channelPtlByChannelId[cfgChannelId];
        }

        /// <summary>
        /// 设备在线状态切换。
        /// </summary>
        void ptlDevice_InErrorChanged(object sender, EventArgs e)
        {
            while (true)
            {
                try
                {
                    PtlDevice ptlDevice = (PtlDevice)sender;

                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        List<CFG_ChannelPtlDevice> cfgChannelPtlDevices = dbContext.CFG_ChannelPtlDevices
                                                                              .Where(cpd => cpd.Ptl900UAddress == ptlDevice.Address)
                                                                              .ToList();

                        foreach (CFG_ChannelPtlDevice cfgChannelPtlDevice in cfgChannelPtlDevices)
                            cfgChannelPtlDevice.OnLine = ptlDevice.InError == false;

                        dbContext.SaveChanges();
                    }

                    break;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}