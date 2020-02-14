using DataAccess;
using DataAccess.Other;
using Ptl.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DeviceCommunicationHost
{
    public class PickZoneHost
    {
        readonly static PickZoneHost instance = new PickZoneHost();
        public static PickZoneHost Instance 
        {
            get { return instance; }
        }
        /// <summary>
        /// 亮灯线程
        /// </summary>
        Thread thread = null;
        /// <summary>
        /// 状态获取线程
        /// </summary>
        Thread threadStatus = null;

        public List<XGate> xGates = new List<XGate>();
        public List<PickZoneDevice> devices = new List<PickZoneDevice>();
        Dictionary<int, PickZoneDevice> dict = new Dictionary<int, PickZoneDevice>();
        public PickZoneHost()
        {
            
        }

        private void DoStatus(object state)
        {
            while (true)
            {                
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var device in devices)
                            {
                                var count = dbContext.AST_LesTask_PDAs
                                    .Where(x => x.IsFinished == false & x.WbsId.Contains("U") && x.CFG_ChannelId == device.CFG_ChannelId)
                                    .Count();
                                var target = dbContext.PickZones.FirstOrDefault(x => x.Id == device.Id);
                                if (target != null)
                                {
                                    if (count > 0)
                                    {
                                        target.Status = 1;
                                    }
                                    else
                                    {
                                        target.Status = 0;
                                    }
                                }
                                
                            }

                            dbContext.SaveChanges();
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void Dowork(object state)
        {
            while (true)
            {
                List<PickZone> items = new List<PickZone>();
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        items = dbContext.PickZones.ToList();
                    }
                }
                catch
                {

                }
                foreach (var item in items)
                {
                    PickZoneDevice device = null;
                    dict.TryGetValue(item.Id, out device);
                    if (device != null)
                    {
                        if (item.Status != device.Status)
                        {
                            if (item.Status == 1)
                            {
                                device.OrangeLighthouse.Display();
                            }
                            else
                            {
                                device.OrangeLighthouse.Clear();
                            }


                            device.Status = item.Status;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread.Join();
                thread = null;
            }
            if (threadStatus != null)
            {
                threadStatus.Abort();
                threadStatus.Join();
                threadStatus = null;
            }

            foreach (XGate xGate in this.xGates)
            {
                foreach (RS485Bus bus in xGate.Buses)
                {
                    foreach (PtlDevice device in bus.Devices)
                    {
                        device.Initialize();
                    }
                }

                Thread.Sleep(1000 * 2);

                xGate.StopUnicastCommandQueue();
            }

            xGates.Clear();
            devices.Clear();
            dict.Clear();
        }

        public void Start()
        {
            this.Stop();
            try
            {
                List<PickZone> items = new List<PickZone>();
                using (GeelyPtlEntities context = new GeelyPtlEntities())
                {
                    items = context.PickZones.ToList();
                }

                foreach (var item in items)
                {
                    XGate xgate = this.xGates
                                      .FirstOrDefault(xg => xg.IPEndPoint.Address.ToString() == item.XGateIP);
                    if (xgate == null)
                    {
                        xgate = new XGate(item.XGateIP);
                        this.xGates.Add(xgate);
                    }
                    RS485Bus bus = xgate.Buses[(byte)item.Bus];
                    PtlMXP1O5 m3 = bus.Devices.FirstOrDefault(d => d.Address == (byte)item.Address) as PtlMXP1O5;
                    if (m3 == null)
                    {
                        m3 = new PtlMXP1O5();
                        m3.Address = (byte)item.Address;

                        bus.Devices.AddOrUpdate(m3);
                    }

                    Lighthouse redLighthouse = m3.Lighthouses[(byte)2];
                    Lighthouse orangeLighthouse = m3.Lighthouses[(byte)1];
                    Lighthouse greenLighthouse = m3.Lighthouses[(byte)4];

                    PickZoneDevice logicDevice = new PickZoneDevice
                    {
                        XGate = xgate,
                        Bus = bus,
                        M3 = m3,
                        RedLighthouse = redLighthouse,
                        GreenLighthouse = greenLighthouse,
                        OrangeLighthouse = orangeLighthouse,
                        CFG_ChannelId = item.CFG_ChannelId,
                        Id = item.Id
                    };

                    this.devices.Add(logicDevice);
                }

                dict = this.devices.ToDictionary(x => x.Id);

                //启动 PTL 通讯
                foreach (XGate xGate in this.xGates)
                {
                    xGate.StartUnicastCommandQueue();

                    foreach (RS485Bus bus in xGate.Buses)
                    {
                        foreach (PtlDevice target in bus.Devices)
                        {
                            target.Initialize();
                        }
                    }
                }

                thread = new Thread(new ParameterizedThreadStart(Dowork));
                thread.IsBackground = true;
                thread.Start();

                threadStatus = new Thread(new ParameterizedThreadStart(DoStatus));
                threadStatus.IsBackground = true;
                threadStatus.Start();
            }
            catch
            {
                this.Stop();
            }
        }
    }

    public class PickZoneDevice
    {
        public XGate XGate { get; set; }

        public RS485Bus Bus { get; set; }

        public PtlMXP1O5 M3 { get; set; }

        public Lighthouse RedLighthouse { get; set; }

        public Lighthouse GreenLighthouse { get; set; }

        public Lighthouse OrangeLighthouse { get; set; }
        /// <summary>
        /// 巷道
        /// </summary>
        public int CFG_ChannelId { get; set; }

        public int Id { get; set; }

        public int Status { get; set; }
    }
}
