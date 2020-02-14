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
    public class AssemblySectionHost
    {
        readonly static AssemblySectionHost instance = new AssemblySectionHost();
        public static AssemblySectionHost Instance
        {
            get { return instance; }
        }

        public List<XGate> xGates = new List<XGate>();
        public List<AssemblingDevice> devices = new List<AssemblingDevice>();
        private Dictionary<int, AssemblingDevice> dict = new Dictionary<int, AssemblingDevice>();

        private string connString = string.Empty;

        public AssemblySectionHost()
        {
            //connString = System.Configuration.ConfigurationManager.ConnectionStrings["default"].ConnectionString;
        }

        /// <summary>
        /// PTL点灯线程
        /// </summary>
        Thread thread = null;
        /// <summary>
        /// 修改状态线程
        /// </summary>
        Thread threadStatus = null;


        //ConcurrentDictionary<string, bool> dictBool = new ConcurrentDictionary<string, bool>();
        ConcurrentDictionary<string, Thread> dictThread = new ConcurrentDictionary<string, Thread>();

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

            foreach (var threadStatus in dictThread.Values)
            {
                threadStatus.Abort();
                threadStatus.Join();    
            }

            dictThread.Clear();
            //dictBool.Clear();

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
            this.Stop();
            try
            {

                List<Assembling> items = new List<Assembling>();
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    items = dbContext.Assemblings.ToList();
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
                    Ptl900U callButton = (Ptl900U)bus.Devices.FirstOrDefault(d => d.Address == (byte)item.Address);
                    if (callButton == null)
                    {
                        callButton = new Ptl900U();
                        callButton.Address = (byte)item.Address;

                        bus.Devices.AddOrUpdate(callButton);
                    }

                    AssemblingDevice logicDevice = new AssemblingDevice
                    {
                        XGate = xgate,
                        Bus = bus,
                        CallButton = callButton,
                        Id = item.Id,
                        AreaId = item.AreaId,
                        Type = item.Type
                    };

                    this.devices.Add(logicDevice);
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



                //threadStatus = new Thread(new ParameterizedThreadStart(DoStatus));
                //threadStatus.IsBackground = true;
                //threadStatus.Start();


                var groups = this.devices.GroupBy(x => x.AreaId);
                foreach (var group in groups)
                {
                    //dictBool.AddOrUpdate(group.Key, false, (a, b) => b);

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

        private void Dowork(object obj)
        {
            while (true)
            {
                List<Assembling> items = new List<Assembling>();
                try
                {
                    using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                    {
                        items = dbContext.Assemblings.ToList();
                    }
                }
                catch
                {

                }

                foreach (var item in items)
                {
                    AssemblingDevice device = null;
                    dict.TryGetValue(item.Id, out device);
                    if (device != null)
                    {
                        if (item.Status != device.Status)
                        {
                            if (item.Status == 1)
                            {
                                device.CallButton.Display(new Display900UItem(), LightColor.Green, true);//交互灯开启采集
                            }
                            else
                            {
                                device.CallButton.Clear(true);
                            }

                            device.Status = item.Status;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private void DoStatus(object obj)
        {
            Thread.Sleep(2000);//这里休眠5秒是为了让程序先执行一次Dowork方法
            string areaId = obj.ToString();
            while (true)
            {

                GeelyPtlEntities context = new GeelyPtlEntities();
                var dbContextTransaction = context.Database.BeginTransaction();
                var model = context.Assemblings.Where(x => x.Status == 1 && x.AreaId == areaId).FirstOrDefault();
                if (model == null)
                {
                    //获取所有该区域所有装配指令，有匹配的物料则亮灯，亮灯后删除指令，没有则不亮灯
                    AssemblyLightOrder order = new AssemblyLightOrder();

                    try
                    {
                        //context.Session.BeginTransaction();
                        order = context.AssemblyLightOrders.OrderBy(p => p.Id).FirstOrDefault();

                        var item = context.Assemblings.Where(x => x.MaterialId == order.MaterialId).FirstOrDefault();
                        if (item != null)
                        {
                            item.Status = 1;//亮灯
                           
                            //删除已使用指令
                            context.AssemblyLightOrders.Remove(order);
                        }
                        context.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        //if (context.Session.IsInTransaction)
                        //{
                        //    context.Session.RollbackTransaction();
                        //}
                        dbContextTransaction.Rollback();
                    }
                    finally
                    {
                        context.Dispose();
                    }
                    Thread.Sleep(1000);
                }
            }


        }

        void CallButton_Pressed(object sender, Ptl900UPressedEventArgs e)
        {
            var targetDevice = devices.FirstOrDefault(x => x.CallButton == sender);
            if (targetDevice != null)
            {
                targetDevice.CallButton.Clear(true);//灭灯
                ThreadPool.QueueUserWorkItem(new WaitCallback(_ =>
                {
                    GeelyPtlEntities context = new GeelyPtlEntities();
                    try
                    {
                        //解绑
                        var item = context.Assemblings.Where(x => x.Id == targetDevice.Id).FirstOrDefault();
                        if (item != null)
                        {
                            item.Status = 0;
                            item.MaterialId = null;
                           
                        }
                        context.SaveChanges();
                        //解锁
                        var count = context.Assemblings.Where(x => x.AreaId == targetDevice.AreaId && x.MaterialId == null).Count();
                        if (count == 3)
                        {
                            //读取同缓存区内所有锁定的灯，执行解锁
                            var list = context.CacheRegions.Where(x => x.AreaId == targetDevice.AreaId && x.IsLocked == true).ToList();
                            foreach (var li in list)
                            {
                                li.IsLocked = false;
                                
                            }
                            context.SaveChanges();
                        }
                    }
                    catch
                    {

                    }
                    finally
                    {
                        context.Dispose();
                    }


                }));
            }
        }
    }
}
public class AssemblingDevice
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


    public int Status { get; set; }

    public string Type { get; set; }
}

