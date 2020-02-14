using DataAccess;
using DataAccess.Other;
using Ptl.Device;
using Ptl.Device.Communication.Command;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DeviceCommunicationHost
{
    public class FeedZoneHost
    {
        readonly static FeedZoneHost instance = new FeedZoneHost();
        public static FeedZoneHost Instance { get { return instance; } }

        public List<XGate> xGates = new List<XGate>();
        public List<FeedZoneDevice> devices = new List<FeedZoneDevice>();
        private Dictionary<int, FeedZoneDevice> dict = new Dictionary<int, FeedZoneDevice>();
        public FeedZoneHost()
        {

        }
        /// <summary>
        /// PTL点灯线程
        /// </summary>
        Thread thread = null;
        /// <summary>
        /// 根据拣选任务更改亮灯状态
        /// </summary>
        //Thread threadStatus = null;
        
        /// <summary>
        /// true表示某个区域的灯被拍下
        /// </summary>
        ConcurrentDictionary<string, bool> dictBool = new ConcurrentDictionary<string, bool>();
        ConcurrentDictionary<string, Thread> dictThread = new ConcurrentDictionary<string, Thread>();
        
        public void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread.Join();
                thread = null;
            }
            //if (threadStatus != null)
            //{
            //    threadStatus.Abort();
            //    threadStatus.Join();
            //    threadStatus = null;
            //}

            foreach (var threadStatus in dictThread.Values)
            {
                threadStatus.Abort();
                threadStatus.Join();
            }
            
            dictThread.Clear();
            dictBool.Clear();


            foreach (XGate xGate in this.xGates)
            {
                foreach (RS485Bus bus in xGate.Buses)
                {
                    foreach (PtlDevice device in bus.Devices)
                    {
                        device.Initialize();
                        var targetDevice = devices.FirstOrDefault(x => x.CallButton == device && x.IsInteractive == true);
                        if (targetDevice != null)
                        {
                            targetDevice.CallButton.Pressed -= CallButton_Pressed;
                        }
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
                List<FeedZone> items = new List<FeedZone>();
                 using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                 {
                     items = dbContext.FeedZones.ToList();
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
                     if (item.IsInteractive == true)//交互灯是PTL900U
                     {
                         Ptl900U callButton = (Ptl900U)bus.Devices.FirstOrDefault(d => d.Address == (byte)item.Address);
                         if (callButton == null)
                         {
                             callButton = new Ptl900U();
                             callButton.Address = (byte)item.Address;

                             bus.Devices.AddOrUpdate(callButton);
                         }

                         FeedZoneDevice logicDevice = new FeedZoneDevice
                         {
                             XGate = xgate,
                             Bus = bus,
                             CallButton = callButton,
                             IsM3 = false,
                             IsInteractive = item.IsInteractive,
                             Id = item.Id,
                             RFID = item.RFID,
                             GroundId = item.GroundId,
                             AreaId = item.AreaId
                         };

                         this.devices.Add(logicDevice);
                     }
                     else
                     {
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

                         FeedZoneDevice logicDevice = new FeedZoneDevice
                         {
                             XGate = xgate,
                             Bus = bus,
                             M3 = m3,
                             RedLighthouse = redLighthouse,
                             GreenLighthouse = greenLighthouse,
                             OrangeLighthouse = orangeLighthouse,
                             IsM3 = item.IsM3,
                             IsInteractive = false,
                             Id = item.Id,
                             RFID = item.RFID,
                             GroundId = item.GroundId,
                             AreaId = item.AreaId
                         };

                         this.devices.Add(logicDevice);

                         
                     }
                 }
                 //启动 PTL 通讯
                 foreach (XGate xGate in this.xGates)
                 {
                     xGate.StartUnicastCommandQueue();

                     foreach (RS485Bus bus in xGate.Buses)
                     {
                         foreach (PtlDevice target in bus.Devices)
                         {
                             target.Initialize();
                             var targetDevice = devices.FirstOrDefault(x => x.CallButton == target && x.IsInteractive == true);
                             if (targetDevice != null)
                             {
                                 targetDevice.CallButton.Pressed += CallButton_Pressed;
                             }
                         }
                     }
                 }

                 dict = this.devices.ToDictionary(x => x.Id);

                 thread = new Thread(new ParameterizedThreadStart(Dowork));
                 thread.IsBackground = true;
                 thread.Start();

                 //threadStatus = new Thread(new ParameterizedThreadStart(DoStatus));
                 //threadStatus.IsBackground = true;
                 //threadStatus.Start();
                var groups = this.devices.GroupBy(x => x.AreaId);
                foreach (var group in groups)
                {
                    dictBool.AddOrUpdate(group.Key, false, (a, b) => b);

                    Thread threadStatus = new Thread(new ParameterizedThreadStart(DoStatus));
                    threadStatus.IsBackground = true;
                    threadStatus.Start(group.Key);

                    dictThread.AddOrUpdate(group.Key, threadStatus, (a, b) => b);
                }
            }
            catch
            {
                this.Stop();
            }
        }

        private void DoStatus(object obj)
        {
            string areaId = (string)obj;
            while (true)
            {
                if (dictBool[areaId] == false)
                {
                    //查找该区域下是否有亮灯，有则不执行,表示有任务
                    var count = devices.Where(x => x.AreaId == areaId && x.Status == 1).Count();
                    if (count > 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    FeedRecord model = null;
                    try
                    {
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            model = dbContext.FeedRecords.Where(x => x.PACKLINE == areaId).OrderBy(x => x.Id).FirstOrDefault();
                        }
                    }
                    catch
                    {

                    }
                    if (model != null)
                    {
                        //根据分装线代码找到指定的灯
                        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                        {
                            using (var dbContextTransaction = dbContext.Database.BeginTransaction())
                            {
                                try
                                {
                                    //先找到需要亮的灯
                                    var target = dbContext.FeedZones.Where(x => x.AreaId == model.PACKLINE && x.MaterialId == model.PRDSEQ).OrderBy(x => x.Id).FirstOrDefault();
                                    if (target != null)
                                    {
                                        target.Status = 1;//点灯
                                        //找到柱灯
                                        var m3 = dbContext.FeedZones.FirstOrDefault(x => x.AreaId == model.PACKLINE && x.IsM3 == true);
                                        if (m3 != null)
                                        {
                                            m3.Status = 1;
                                        }
                                        //找到交互灯,可能有多个，都点亮
                                        var items = dbContext.FeedZones.Where(x => x.AreaId == model.PACKLINE && x.IsInteractive == true).ToList();
                                        foreach (var item in items)
                                        {
                                            item.Status = 1;
                                        }
                                        dbContext.SaveChanges();
                                        dbContextTransaction.Commit();
                                    }
                                    else
                                    {
                                    }
                                }
                                catch (Exception ex)
                                {
                                    dbContextTransaction.Rollback();
                                }
                            }
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void Dowork(object obj)
        {
            while (true)
            {
                List<FeedZone> items = new List<FeedZone>();
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        items = dbContext.FeedZones.ToList();
                    }
                }
                catch
                {

                }

                foreach (var item in items)
                {
                    FeedZoneDevice device = null;
                    dict.TryGetValue(item.Id, out device);
                    if (device != null)
                    {
                        if (item.Status != device.Status)
                        {
                            if (device.IsInteractive)//是交互灯
                            {
                                if (item.Status == 1)
                                {
                                    device.CallButton.Display(new Display900UItem(), LightColor.Green, true);//交互灯开启采集
                                }
                                else
                                {
                                    device.CallButton.Clear(true);
                                }
                            }
                            else
                            {
                                if (item.Status == 1)
                                {
                                    device.GreenLighthouse.Display();
                                }
                                else
                                {
                                    device.GreenLighthouse.Clear();
                                }
                            }

                            device.Status = item.Status;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        void CallButton_Pressed(object sender, Ptl900UPressedEventArgs e)
        {
            var targetDevice = devices.FirstOrDefault(x => x.CallButton == sender);
            if (targetDevice != null)
            {
                targetDevice.CallButton.Clear(true);

                //将地堆与总成件解绑，我这边要解绑
                ThreadPool.QueueUserWorkItem(new WaitCallback(_ =>
                {
                    dictBool[targetDevice.AreaId] = true;
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        using (var dbContextTransaction = dbContext.Database.BeginTransaction())
                        {
                            try
                            {
                                //查找该分装线下所有的亮灯
                                List<FeedZone> items = dbContext.FeedZones.Where(x => x.AreaId == targetDevice.AreaId && x.Status == 1).ToList();
                                foreach (var item in items)
                                {
                                    var target = dbContext.FeedZones.FirstOrDefault(x => x.Id == item.Id);
                                    if (target != null)
                                    {
                                        target.MaterialId = null;//解绑
                                        target.Status = 0;//灭灯
                                    }
                                }

                                var record = dbContext.FeedRecords.Where(x => x.PACKLINE == targetDevice.AreaId).OrderBy(x => x.Id).FirstOrDefault();
                                dbContext.FeedRecords.Remove(record);

                                dbContext.SaveChanges();
                                dbContextTransaction.Commit();
                            }
                            catch
                            {
                                dbContextTransaction.Rollback();
                            }
                        }
                    }
                    dictBool[targetDevice.AreaId] = false;
                }));
            }
        }
    }

    public class FeedZoneDevice
    {
        public XGate XGate { get; set; }

        public RS485Bus Bus { get; set; }

        public PtlMXP1O5 M3 { get; set; }

        public Lighthouse RedLighthouse { get; set; }

        public Lighthouse GreenLighthouse { get; set; }

        public Lighthouse OrangeLighthouse { get; set; }

        public Ptl900U CallButton { get; set; }

        public int Id { get; set; }
        /// <summary>
        /// 是否区域灯
        /// </summary>
        public bool IsM3 { get; set; }
        /// <summary>
        /// RFID码，即地堆码
        /// </summary>
        public string RFID { get; set; }
        /// <summary>
        /// MES中维护的地堆码
        /// </summary>
        public string GroundId { get; set; }
        /// <summary>
        /// MES中维护的区域码
        /// </summary>
        public string AreaId { get; set; }

        /// <summary>
        /// 是否交互灯
        /// </summary>
        public bool IsInteractive { get; set; }

        public int Status { get; set; }
    }
}
