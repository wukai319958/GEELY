using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using DataAccess;
using DataAccess.Config;
using Ptl.Device;
using Ptl.Device.Communication;

namespace DeviceCommunicationHost
{
    /// <summary>
    /// 小车上的 PTL 的控制入口。
    /// </summary>
    public class CartPtl
    {
        /// <summary>
        /// 获取小车主键。
        /// </summary>
        public int CFG_CartId { get; private set; }

        //每辆小车有自己的 XGate
        readonly string xGateIP;

        /// <summary>
        /// 获取可空的 XGate。
        /// </summary>
        public XGate XGate { get; private set; }

        readonly TimeSpan continuousConnectionErrorTimeout = TimeSpan.FromSeconds(5);
        readonly TimeSpan continuousDeviceErrorTimeout = TimeSpan.FromSeconds(5);
        DateTime lastConnectionErrorTime = DateTime.MinValue;
        readonly Dictionary<byte, DateTime> lastDeviceErrorTime = new Dictionary<byte, DateTime>();

        /// <summary>
        /// 初始化小车 PTL 的控制入口。
        /// </summary>
        /// <param name="cfgCartId">小车主键。</param>
        /// <param name="xGateIP">XGate 的 IP。</param>
        public CartPtl(int cfgCartId, string xGateIP)
        {
            this.CFG_CartId = cfgCartId;
            this.xGateIP = xGateIP;
        }

        /// <summary>
        /// 启动通讯。
        /// </summary>
        public void Start()
        {
            IPAddress ipAddress = IPAddress.Parse(this.xGateIP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, XGate.DefaultIPEndPointPort);
            this.XGate = new XGate(ipEndPoint, 1, 5000, 5000, 5000, 0, DefaultRS485AddressMapper.Original);

            // 8 个库位标签
            for (byte i = 1; i <= 8; i++)
            {
                Ptl900U ptl900U = new Ptl900U();
                ptl900U.Address = i;
                ptl900U.MinorType = Ptl900UType.P24;

                this.XGate.Buses[RS485BusName.Bus1].Devices.AddOrUpdate(ptl900U);
                this.lastDeviceErrorTime.Add(ptl900U.Address, DateTime.MinValue);
            }

            // 1 个中文信息屏
            Ptl900U ptl900UPublisher = new Ptl900U();
            ptl900UPublisher.Address = 9;
            ptl900UPublisher.MinorType = Ptl900UType.S1;

            this.XGate.Buses[RS485BusName.Bus1].Devices.AddOrUpdate(ptl900UPublisher);
            this.lastDeviceErrorTime.Add(ptl900UPublisher.Address, DateTime.MinValue);

            // 1 个指示球灯
            Ptl900U ptl900ULight = new Ptl900U();
            ptl900ULight.Address = 10;
            ptl900ULight.MinorType = Ptl900UType.P0;

            this.XGate.Buses[RS485BusName.Bus1].Devices.AddOrUpdate(ptl900ULight);
            this.lastDeviceErrorTime.Add(ptl900ULight.Address, DateTime.MinValue);

            this.XGate.StartUnicastCommandQueue();

            foreach (RS485Bus rs485Bus in this.XGate.Buses)
            {
                rs485Bus.CommunicationClient.ConnectedChanged += this.rs485Bus_CommunicationClient_ConnectedChanged;

                foreach (PtlDevice ptlDevice in rs485Bus.Devices)
                {
                    ptlDevice.InErrorChanged += this.ptlDevice_InErrorChanged;

                    ptlDevice.Initialize();
                }
            }

            //寻车指示灯始终锁定
            ptl900ULight.Lock();
        }

        /// <summary>
        /// 停止通讯。
        /// </summary>
        public void Stop()
        {
            this.lastDeviceErrorTime.Clear();
            this.lastConnectionErrorTime = DateTime.MinValue;

            foreach (RS485Bus rs485Bus in this.XGate.Buses)
            {
                rs485Bus.CommunicationClient.ConnectedChanged -= this.rs485Bus_CommunicationClient_ConnectedChanged;

                foreach (PtlDevice ptlDevice in rs485Bus.Devices)
                    ptlDevice.InErrorChanged -= this.ptlDevice_InErrorChanged;
            }

            this.XGate.StopUnicastCommandQueue();
            this.XGate.Dispose();
            this.XGate = null;
        }

        /// <summary>
        /// 按库位获取库位标签。
        /// </summary>
        /// <param name="position">库位号。</param>
        public Ptl900U GetPtl900UByPosition(int position)
        {
            return this.XGate.Buses[RS485BusName.Bus1].Devices[(byte)position] as Ptl900U;
        }

        /// <summary>
        /// 获取中文信息屏。
        /// </summary>
        public Ptl900U GetPtl900UPublisher()
        {
            return this.XGate.Buses[RS485BusName.Bus1].Devices[9] as Ptl900U;
        }

        /// <summary>
        /// 获取指示球灯。
        /// </summary>
        public Ptl900U GetPtl900ULight()
        {
            return this.XGate.Buses[RS485BusName.Bus1].Devices[10] as Ptl900U;
        }

        /// <summary>
        /// 判断标签是否属于当前小车。
        /// </summary>
        public bool Contains(PtlDevice ptlDevice)
        {
            return this.XGate.Buses[RS485BusName.Bus1].Devices.Contains(ptlDevice);
        }

        /// <summary>
        /// 小车在线状态切换。
        /// </summary>
        void rs485Bus_CommunicationClient_ConnectedChanged(object sender, EventArgs e)
        {
            ICommunicationClient communicationClient = (ICommunicationClient)sender;

            if (communicationClient.Connected != true && DateTime.Now - this.lastConnectionErrorTime < this.continuousConnectionErrorTimeout)
                return;

            this.lastConnectionErrorTime = DateTime.Now;

            while (true)
            {
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        CFG_Cart cfgCart = dbContext.CFG_Carts
                                               .First(c => c.Id == this.CFG_CartId);

                        cfgCart.OnLine = communicationClient.Connected == true;

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

        /// <summary>
        /// 标签设备在线状态切换。
        /// </summary>
        void ptlDevice_InErrorChanged(object sender, EventArgs e)
        {
            PtlDevice ptlDevice = (PtlDevice)sender;

            if (ptlDevice.InError != false && DateTime.Now - this.lastDeviceErrorTime[ptlDevice.Address] < this.continuousDeviceErrorTimeout)
                return;

            this.lastDeviceErrorTime[ptlDevice.Address] = DateTime.Now;

            while (true)
            {
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        CFG_CartPtlDevice cfgCartPtlDevice = dbContext.CFG_CartPtlDevices
                                                                 .First(cpd => cpd.CFG_CartId == this.CFG_CartId
                                                                               && cpd.DeviceAddress == ptlDevice.Address);
                        cfgCartPtlDevice.OnLine = ptlDevice.InError == false;

                        CFG_CartCurrentMaterial cfgCartCurrentMaterial = dbContext.CFG_CartCurrentMaterials
                                                                             .FirstOrDefault(ccm => ccm.Position == ptlDevice.Address);
                        if (cfgCartCurrentMaterial != null)
                        {
                            if (cfgCartCurrentMaterial.Usability == CartPositionUsability.Enable && ptlDevice.InError == true)
                                cfgCartCurrentMaterial.Usability = CartPositionUsability.DisableByOffLineDevice;
                            else if (cfgCartCurrentMaterial.Usability == CartPositionUsability.DisableByOffLineDevice && ptlDevice.InError == false)
                                cfgCartCurrentMaterial.Usability = CartPositionUsability.Enable;
                        }

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