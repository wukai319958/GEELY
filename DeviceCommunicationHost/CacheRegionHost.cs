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
    public class CacheRegionHost
    {
        readonly static CacheRegionHost instance = new CacheRegionHost();
        public static CacheRegionHost Instance { get { return instance; } }

        private string connString = string.Empty;

        public CacheRegionHost()
        {
            //connString = System.Configuration.ConfigurationManager.ConnectionStrings["default"].ConnectionString;
        }

        public List<XGate> xGates = new List<XGate>();
        public List<CacheRegionDevice> devices = new List<CacheRegionDevice>();
        private Dictionary<int, CacheRegionDevice> dict = new Dictionary<int, CacheRegionDevice>();

        /// <summary>
        /// PTL点灯线程
        /// </summary>
        Thread thread = null;
        /// <summary>
        /// 锁灯线程
        /// </summary>
        Thread lockThread = null;
        /// <summary>
        /// 区域线程控制，设置灯的状态
        /// </summary>
        ConcurrentDictionary<string, Thread> dictThread = new ConcurrentDictionary<string, Thread>();
        /// <summary>
        /// true表示某个区域的灯被拍下
        /// </summary>
        ConcurrentDictionary<string, bool> dictBool = new ConcurrentDictionary<string, bool>();

        public void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread.Join();
                thread = null;
            }

            if (lockThread != null)
            {
                lockThread.Abort();
                lockThread.Join();
                lockThread = null;
            }

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
                        if (device is Ptl900U)
                        {
                            (device as Ptl900U).Pressed -= CallButton_Pressed;
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
            try
            {
                List<CacheRegion> items = new List<CacheRegion>();
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    items = dbContext.CacheRegions.ToList();
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

                        CacheRegionDevice logicDevice = new CacheRegionDevice
                        {
                            XGate = xgate,
                            Bus = bus,
                            CallButton = callButton,
                            IsInteractive = item.IsInteractive,
                            Id = item.Id,
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

                        CacheRegionDevice logicDevice = new CacheRegionDevice
                        {
                            XGate = xgate,
                            Bus = bus,
                            M3 = m3,
                            RedLighthouse = redLighthouse,
                            GreenLighthouse = greenLighthouse,
                            OrangeLighthouse = orangeLighthouse,
                            IsInteractive = false,
                            Id = item.Id,
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
                            if (target is Ptl900U)
                            {
                                (target as Ptl900U).Pressed += CallButton_Pressed;
                            }
                        }
                    }
                }

                dict = this.devices.ToDictionary(x => x.Id);

                thread = new Thread(new ParameterizedThreadStart(Dowork));
                thread.IsBackground = true;
                thread.Start();

                lockThread = new Thread(new ParameterizedThreadStart(DoLock));
                lockThread.IsBackground = true;
                lockThread.Start();

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
            catch (Exception ex)
            {
                this.Stop();
            }
        }

        private void DoStatus(object obj)
        {
            Thread.Sleep(2000);//这里休眠5秒是为了让程序先执行一次Dowork方法
            string areaId = (string)obj;
            while (true)
            {
                if (dictBool[areaId] == false)//该区域的灯没有被拍下，执行操作
                {
                    //查找该区域下是否有亮灯，有则不执行,表示有任务
                    var device = devices.Where(x => x.AreaId == areaId && x.Status == 1).FirstOrDefault();
                    if (device != null)
                    {
                        //获取第一个指令，在缓存区匹配物料，判断指令是否无效，无效则删除
                        GeelyPtlEntities context = new GeelyPtlEntities();
                        try
                        {
                            CacheRegionLightOrder lightorder = context.CacheRegionLightOrders.Where(x => x.AreaId == areaId).FirstOrDefault();
                            if (lightorder != null)
                            {
                                var item = context.CacheRegions.Where(x => x.Id == device.Id).FirstOrDefault();//context.Query<CacheRegion>().Where(x => x.AreaId == areaId && (x.Material_A == lightorder.MaterialId || x.Material_B == lightorder.MaterialId || x.Material_C == lightorder.MaterialId)).FirstOrDefault();
                                if (item != null)
                                {
                                    if (item.Material_A == lightorder.MaterialId || item.Material_B == lightorder.MaterialId || item.Material_C == lightorder.MaterialId)
                                    {
                                        //指令无效，删除
                                        context.CacheRegionLightOrders.Remove(lightorder);


                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                        finally
                        {
                            context.Dispose();
                        }

                        Thread.Sleep(1000);
                        continue;
                    }

                    //获取第一个指令，在装配区，判断指令是否无效，无效则删除

                    //读取指令表中的当前区域的第一个指令
                    CacheRegionLightOrder order = null;
                    try
                    {
                        using (GeelyPtlEntities context = new GeelyPtlEntities())
                        {
                            order = context.CacheRegionLightOrders.Where(x => x.AreaId == areaId).FirstOrDefault();
                        }
                    }
                    catch
                    {

                    }
                    if (order != null)
                    {
                        GeelyPtlEntities context = new GeelyPtlEntities();
                        var dbContextTransaction = context.Database.BeginTransaction();
                        try
                        {
                            //判断该指令在装配区是否有匹配项，有为无效指令删除，没有则继续
                            var assembling = context.Assemblings.Where(x => x.AreaId == areaId && x.MaterialId == order.MaterialId).Count();
                            if (assembling > 0)
                            {
                                context.CacheRegionLightOrders.Remove(order);
                            }
                            else
                            {
                                //执行亮灯操作

                                //根据区域和物料找到匹配的第一个交互灯
                                var target = context.CacheRegions
                                    .Where(x => x.AreaId == areaId && (x.Material_A == order.MaterialId || x.Material_B == order.MaterialId || x.Material_C == order.MaterialId) && x.IsInteractive == true)
                                    .FirstOrDefault();
                                if (target != null)//如果找不到，则认定相关物料未绑定，不执行任何操作
                                {
                                    target.Status = 1;//点灯

                                    //找到柱灯，亮灯，代码吴恺补充
                                    var m3 = context.CacheRegions
                                                 .Where(x => x.AreaId == areaId && (x.Material_A == order.MaterialId || x.Material_B == order.MaterialId || x.Material_C == order.MaterialId) && x.IsInteractive == false)
                                                 .FirstOrDefault();
                                    if (m3 != null)
                                    {
                                        m3.Status = 1;

                                    }
                                    context.SaveChanges();

                                    //查找装配区物料不为null的个数，大于0则表示正在装配，需要锁定灯
                                    var number = context.Assemblings.Where(x => x.AreaId == areaId && x.MaterialId != null).Count();
                                    if (number > 0)
                                    {
                                        target.IsLocked = true;//锁灯
                                        var targetDevice = this.devices.FirstOrDefault(x => x.Id == target.Id);
                                        if (targetDevice != null)
                                        {
                                            targetDevice.CallButton.Lock(); //这里锁一次
                                            //targetDevice.IsLocked = true;
                                        }
                                    }

                                    // context.Update<CacheRegion>(target);
                                    context.SaveChanges();

                                }

                                //删除当前指令
                                context.CacheRegionLightOrders.Remove(order);

                                dbContextTransaction.Commit();
                            }
                        }
                        catch
                        {
                            dbContextTransaction.Rollback();
                        }
                        finally
                        {
                            context.Dispose();
                        }
                    }
                }
                //休眠
                Thread.Sleep(1000);
            }
        }

        private void Dowork(object obj)
        {
            while (true)
            {
                List<CacheRegion> items = new List<CacheRegion>();
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        items = dbContext.CacheRegions.ToList();
                    }
                }
                catch
                {

                }

                foreach (var item in items)
                {
                    CacheRegionDevice device = null;
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

        private void DoLock(object obj)
        {
            while (true)
            {
                List<CacheRegion> items = new List<CacheRegion>();
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        //只读取交互灯即可，因为只有交互灯才可以锁定
                        items = dbContext.CacheRegions.Where(x => x.IsInteractive == true).ToList();
                    }
                }
                catch
                {

                }

                foreach (var item in items)
                {
                    CacheRegionDevice device = null;
                    dict.TryGetValue(item.Id, out device);
                    if (device != null)
                    {
                        if (item.IsLocked != device.IsLocked)
                        {
                            if (item.IsLocked)
                            {
                                device.CallButton.Lock();
                            }
                            else
                            {
                                device.CallButton.Unlock();
                            }

                            device.IsLocked = item.IsLocked;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        void CallButton_Pressed(object sender, Ptl900UPressedEventArgs e)
        {
            //灭灯
            var targetDevice = this.devices.FirstOrDefault(x => x.CallButton == sender);
            if (targetDevice != null)
            {
                targetDevice.CallButton.Clear(true);
                ThreadPool.QueueUserWorkItem(new WaitCallback(_ =>
                {
                    dictBool[targetDevice.AreaId] = true;

                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        using (var dbContextTransaction = dbContext.Database.BeginTransaction())
                        {

                            try
                            {
                                //灭交互灯
                                var item = dbContext.CacheRegions.Where(x => x.Id == targetDevice.Id).FirstOrDefault();
                                if (item != null)
                                {
                                    item.Status = 0;//灭灯


                                    //灭柱灯，吴恺补充
                                    var m3Item = dbContext.CacheRegions
                                                          .Where(x => (x.Material_A == item.Material_A || x.Material_B == item.Material_B || x.Material_C == item.Material_C) && x.IsInteractive == false)
                                                          .FirstOrDefault();
                                    //删除缓存区的绑定关系，在装配区插入绑定关系
                                    var assemblings = dbContext.Assemblings.Where(x => x.AreaId == item.AreaId).ToList();
                                    foreach (var assembling in assemblings)
                                    {
                                        switch (assembling.Type)
                                        {
                                            case "A":
                                                assembling.MaterialId = item.Material_A;
                                                break;
                                            case "B":
                                                assembling.MaterialId = item.Material_B;
                                                break;
                                            case "C":
                                                assembling.MaterialId = item.Material_C;
                                                break;
                                            default:
                                                break;
                                        }
                                        dbContext.SaveChanges();
                                    }
                                    item.Material_A = null;
                                    item.Material_B = null;
                                    item.Material_C = null;
                                    if (m3Item != null)
                                    {
                                        m3Item.Status = 0;
                                        m3Item.Material_A = null;
                                        m3Item.Material_B = null;
                                        m3Item.Material_C = null;
                                      //  dbContext.Update(m3Item);
                                    }

                                    //dbContext.Update<CacheRegion>(item);
                                    dbContext.SaveChanges();

                                }
                                // dbContext.Session.CommitTransaction();
                                dbContextTransaction.Commit();
                            }
                            catch
                            {
                                //if (dbContext.Session.IsInTransaction)
                                //{
                                //    dbContext.Session.RollbackTransaction();
                                //}
                                dbContextTransaction.Rollback();
                            }
                        }
                    }

                    dictBool[targetDevice.AreaId] = false;
                }));
            }
        }
    }



    public class CacheRegionDevice
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
        /// MES中维护的区域码
        /// </summary>
        public string AreaId { get; set; }

        /// <summary>
        /// 是否交互灯
        /// </summary>
        public bool IsInteractive { get; set; }

        public int Status { get; set; }

        public bool IsLocked { get; set; }
    }
}
