using DataAccess;
using DataAccess.Other;
using Ptl.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ptl.Device.Communication.Command;

namespace DeviceCommunicationHost
{
    public class MarketZoneHost
    {
        readonly static MarketZoneHost instance = new MarketZoneHost();
        public static MarketZoneHost Instance
        {
            get { return instance; }
        }

        public List<XGate> xGates = new List<XGate>();
        public List<MarketZoneDevice> devices = new List<MarketZoneDevice>();
        public Dictionary<int, MarketZoneDevice> dict = new Dictionary<int, MarketZoneDevice>();
        
        public MarketZoneHost()
        {

        }
        //点灯线程
        Thread thread = null;
        //获取区域状态线程
        Thread areaThread = null;

        public void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread.Join();
                thread = null;
            }
            if (areaThread != null)
            {
                areaThread.Abort();
                areaThread.Join();
                areaThread = null;
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
                List<MarketZone> items = new List<MarketZone>();
                using(GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    items = dbContext.MarketZones.ToList();
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

                    Lighthouse redLighthouse = m3.Lighthouses[2];
                    Lighthouse blueLighthouse = m3.Lighthouses[0];
                    Lighthouse greenLighthouse = m3.Lighthouses[4];

                    MarketZoneDevice logicDevice = new MarketZoneDevice
                    {
                        XGate = xgate,
                        Bus = bus,
                        M3 = m3,
                        RedLighthouse = redLighthouse,
                        GreenLighthouse = greenLighthouse,
                        BlueLighthouse = blueLighthouse,
                        Id = item.Id,
                        GroundId = item.GroundId,
                        AreaId = item.AreaId
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

                areaThread = new Thread(new ParameterizedThreadStart(DoStatus));
                areaThread.IsBackground = true;
                areaThread.Start();
            }
            catch
            {
                this.Stop();
            }
        }

        private void Dowork(object obj)
        {
            while (true)
            {
                List<MarketZone> items = new List<MarketZone>();
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        items = dbContext.MarketZones.ToList();
                    }
                }
                catch
                {

                }
                foreach (var item in items)
                {
                    MarketZoneDevice device = null;
                    dict.TryGetValue(item.Id, out device);
                    if (device != null)
                    {
                        if (item.Status != device.Status || item.AreaStatus!= device.AreaStatus)
                        {
                            Thread.Sleep(100);
                            if (item.Status == 2)
                            {
                                //蓝闪
                                device.BlueLighthouse.Clear();
                                device.RedLighthouse.Display(LightOnOffPeriod.Period100, LightOnOffRatio.RatioP1V2);
                                device.GreenLighthouse.Display(LightOnOffPeriod.Period100, LightOnOffRatio.RatioP1V2);
                            }
                            else if (item.AreaStatus == 1)
                            {
                                //黄=红+绿
                                device.BlueLighthouse.Clear();
                                device.RedLighthouse.Display();
                                device.GreenLighthouse.Display();
                            }
                            else
                            {
                                if (item.Status == 1)//绿色
                                {
                                    device.GreenLighthouse.Display();
                                    device.BlueLighthouse.Clear();
                                    device.RedLighthouse.Clear();
                                }
                                else if (item.Status == 0)//灭灯
                                {
                                    device.GreenLighthouse.Clear();
                                    device.BlueLighthouse.Clear();
                                    device.RedLighthouse.Clear();
                                }
                            }

                            device.Status = item.Status;
                            device.AreaStatus = item.AreaStatus;
                        }
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void DoStatus(object obj)
        {
            while (true)
            {
                var groups = this.devices.GroupBy(x => x.AreaId);
                foreach (var group in groups)
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            try
                            {
                                //string sql = string.Format("select COUNT(*) from [DST_DistributeTask] where DistributeReqTypes=3 and isResponse=1 and [sendErrorCount]<5 and [arriveTime] is null and [startPosition]='{0}'", group.Key);
                                string sql = string.Format(@"select count(*) from DST_DistributeTask a
inner join DST_DistributeTaskResult b on a.reqCode=b.reqCode
where a.DistributeReqTypes=3 and a.sendErrorCount<5 and a.arriveTime is null
and b.data not in(select taskCode from DST_DistributeArriveTask where method='OutFromBin') and a.[startPosition] like '{0}%'", group.Key);
                                int number = dbContext.Database.SqlQuery<int>(sql).FirstOrDefault();
                                if (number > 0)
                                {
                                    //区域有任务
                                    var targets = dbContext.MarketZones.Where(x => x.AreaId == group.Key).ToList();
                                    foreach (var target in targets)
                                    {
                                        target.AreaStatus = 1;
                                    }
                                }
                                else
                                {
                                    //区域没任务
                                    var targets = dbContext.MarketZones.Where(x => x.AreaId == group.Key).ToList();
                                    foreach (var target in targets)
                                    {
                                        target.AreaStatus = 0;
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
                }

                Thread.Sleep(1000);
            }
        }
    }



    public class MarketZoneDevice
    {
        public XGate XGate { get; set; }

        public RS485Bus Bus { get; set; }

        public PtlMXP1O5 M3 { get; set; }

        public Lighthouse RedLighthouse { get; set; }

        public Lighthouse GreenLighthouse { get; set; }

        public Lighthouse BlueLighthouse { get; set; }
        /// <summary>
        /// 地堆码
        /// </summary>
        public string GroundId { get; set; }
        /// <summary>
        /// 区域码
        /// </summary>
        public string AreaId { get; set; }

        public int Id { get; set; }

        public int Status { get; set; }

        public int AreaStatus { get; set; }
    }
}
