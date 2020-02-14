using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.Config;
using DeviceCommunicationHost;
using Ptl.Device;
using Ptl.Device.Communication.Command;

namespace Assorting
{
    /// <summary>
    /// 不停轮流使用所有巷道的 RFID 读取小车 RFID 标签，并和当前巷道绑定，并在小车任务完成后解除绑定。
    /// </summary>
    public class ChannelCurrentCartBinder
    {
        static readonly ChannelCurrentCartBinder instance = new ChannelCurrentCartBinder();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static ChannelCurrentCartBinder Instance
        {
            get { return ChannelCurrentCartBinder.instance; }
        }

        ChannelCurrentCartBinder()
        { }

        /// <summary>
        /// 获取第一个空车位。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        /// <returns>第一个空车位。</returns>
        public int? GetFirstEmptyPosition(int cfgChannelId)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                   .Where(ccc => ccc.CFG_ChannelId == cfgChannelId && ccc.CFG_CartId == null)
                                                                   .OrderBy(ccc => ccc.Position)
                                                                   .FirstOrDefault();

                return cfgChannelCurrentCart == null ? null : (int?)cfgChannelCurrentCart.Position;
            }
        }

        /// <summary>
        /// 停靠小车到分拣口。
        /// </summary>
        /// <param name="cfgChannelId">分拣口的主键。</param>
        /// <param name="position">车位。</param>
        /// <param name="cfgCartId">待停靠小车的主键。</param>
        /// <exception cref="System.ArgumentException">position 车位上的小车还未解除绑定。</exception>
        public void Dock(int cfgChannelId, int position, int cfgCartId)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_Channel cfgChannel = dbContext.CFG_Channels
                                             .First(c => c.Id == cfgChannelId);
                CFG_Cart cfgCart = dbContext.CFG_Carts
                                       .First(c => c.Id == cfgCartId);
                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                   .FirstOrDefault(ccc => ccc.CFG_ChannelId == cfgChannelId && ccc.Position == position);
                if (cfgCart.CartStatus != CartStatus.Free)
                    throw new ArgumentException("小车 " + cfgCart.Code + " 未释放：" + cfgCart.CartStatus + "。");
                if (!cfgCart.OnLine)
                    throw new ArgumentException("小车 " + cfgCart.Code + " 不在线。");
                foreach (CFG_CartPtlDevice cfgCartPtlDevice in cfgCart.CFG_CartPtlDevices)
                {
                    if (!cfgCartPtlDevice.OnLine)
                        throw new ArgumentException("小车上的 " + cfgCartPtlDevice.DeviceAddress + " 号标签不在线。");
                }
                if (cfgChannelCurrentCart == null)
                    throw new ArgumentException("车位 " + position + " 不存在。", "position");
                if (cfgChannelCurrentCart.CFG_CartId != null && cfgChannelCurrentCart.CFG_CartId != cfgCartId)
                    throw new ArgumentException("车位 " + position + " 上的小车还未解除绑定。", "position");

                if (cfgChannelCurrentCart.CFG_CartId == null)
                {
                    CFG_ChannelCurrentCart dockedCfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                             .FirstOrDefault(ccc => ccc.CFG_CartId == cfgCartId);
                    if (dockedCfgChannelCurrentCart != null)
                        throw new ArgumentException("小车 " + dockedCfgChannelCurrentCart.CFG_Cart.Code + " 已停靠在 " + dockedCfgChannelCurrentCart.CFG_Channel.Name + " 车位 " + dockedCfgChannelCurrentCart.Position + "。", "cfgCartId");

                    //停靠即开始播种
                    cfgCart.CartStatus = CartStatus.WaitingAssorting;

                    cfgChannelCurrentCart.CFG_CartId = cfgCart.Id;
                    cfgChannelCurrentCart.DockedTime = DateTime.Now;

                    //清空小车上的物料
                    List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                                .ToList();
                    foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                    {
                        cfgCartCurrentMaterial.AST_CartTaskItemId = null;
                        cfgCartCurrentMaterial.ProjectCode = null;
                        cfgCartCurrentMaterial.WbsId = null;
                        cfgCartCurrentMaterial.ProjectStep = null;
                        cfgCartCurrentMaterial.CFG_WorkStationId = null;
                        cfgCartCurrentMaterial.BatchCode = null;
                        cfgCartCurrentMaterial.CFG_ChannelId = null;
                        cfgCartCurrentMaterial.CFG_PalletId = null;
                        cfgCartCurrentMaterial.BoxCode = null;
                        cfgCartCurrentMaterial.FromPalletPosition = null;
                        cfgCartCurrentMaterial.MaterialCode = null;
                        cfgCartCurrentMaterial.MaterialName = null;
                        cfgCartCurrentMaterial.MaterialBarcode = null;
                        cfgCartCurrentMaterial.Quantity = null;
                        cfgCartCurrentMaterial.AssortedTime = null;
                        cfgCartCurrentMaterial.CFG_EmployeeId = null;
                        if (cfgCartCurrentMaterial.Usability != CartPositionUsability.DisableByOffLineDevice)
                            cfgCartCurrentMaterial.Usability = CartPositionUsability.Enable;
                    }

                    dbContext.SaveChanges();

                    //设备控制
                    CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(cfgCart.Id);
                    Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();

                    Display900UItem publisherDisplay900UItem = new Display900UItem();
                    publisherDisplay900UItem.Name = "停靠成功";
                    publisherDisplay900UItem.Description = cfgChannel.Name;
                    publisherDisplay900UItem.Count = (ushort)position;
                    publisherDisplay900UItem.Unit = "位";

                    ptl900UPublisher.Clear(true);
                    ptl900UPublisher.Lock();
                    ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off);

                    Logger.Log(this.GetType().Name, DateTime.Now.ToString("HH:mm:ss") + "停靠 " + cfgCart.Name + " 到 " + cfgChannel.Name + " 车位 " + position + Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// 从分拣口解除小车绑定。
        /// </summary>
        /// <param name="cfgChannelId">分拣口的主键。</param>
        /// <param name="cfgCartId">待移出小车的主键。</param>
        /// <exception cref="System.ArgumentException">position 车位上的小车还未作业完成。</exception>
        public void UnDock(int cfgChannelId, int cfgCartId)
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                CFG_ChannelCurrentCart cfgChannelCurrentCart = dbContext.CFG_ChannelCurrentCarts
                                                                   .FirstOrDefault(ccc => ccc.CFG_ChannelId == cfgChannelId && ccc.CFG_CartId == cfgCartId);
                if (cfgChannelCurrentCart != null && cfgChannelCurrentCart.CFG_CartId != null)
                {
                    CFG_Cart cfgCart = cfgChannelCurrentCart.CFG_Cart;
                    if (cfgCart.CartStatus == CartStatus.Assorting)
                        throw new ArgumentException("车位 " + cfgChannelCurrentCart.Position + " 上的小车 " + cfgCart.Code + " 还未作业完成。", "position");

                    //移出
                    cfgChannelCurrentCart.CFG_CartId = null;
                    cfgChannelCurrentCart.DockedTime = null;

                    //准备基础数据
                    AST_PalletTask astPalletTask = null;
                    CFG_WorkStation cfgWorkStation = null;
                    List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials
                                                                                .ToList();
                    CFG_CartCurrentMaterial firstNotEmptyCfgCartCurrentMaterial = cfgCartCurrentMaterials
                                                                                      .FirstOrDefault(ccm => ccm.AST_CartTaskItemId != null);
                    if (firstNotEmptyCfgCartCurrentMaterial != null)
                    {
                        AST_CartTaskItem astCartTask = firstNotEmptyCfgCartCurrentMaterial.AST_CartTaskItem;
                        astPalletTask = astCartTask.AST_PalletTaskItem.AST_PalletTask;
                        cfgWorkStation = astCartTask.AST_PalletTaskItem.CFG_WorkStation;
                    }

                    dbContext.SaveChanges();

                    //设备控制
                    CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(cfgCart.Id);
                    Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();
                    Ptl900U ptl900ULight = cartPtl.GetPtl900ULight();

                    ptl900UPublisher.Clear(true);
                    ptl900UPublisher.Unlock();

                    if (astPalletTask != null)
                    {
                        Display900UItem publisherDisplay900UItem = new Display900UItem();
                        publisherDisplay900UItem.Name = "已分拣完成";
                        publisherDisplay900UItem.Description = string.Format(CultureInfo.InvariantCulture, @"项目：{0}
阶段：{1}
工位：{2}", astPalletTask.ProjectCode, astPalletTask.ProjectStep, cfgWorkStation.Code);
                        publisherDisplay900UItem.Count = (ushort)cfgCartCurrentMaterials
                                                                     .Where(ccm => ccm.Quantity != null)
                                                                     .Select(ccm => ccm.Quantity.Value)
                                                                     .Sum();
                        publisherDisplay900UItem.Unit = "个";

                        ptl900UPublisher.Lock();
                        ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off);
                    }

                    foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                    {
                        Ptl900U ptl900U = cartPtl.GetPtl900UByPosition(cfgCartCurrentMaterial.Position);

                        ptl900U.Clear(true);
                        ptl900U.Unlock();
                    }

                    ptl900ULight.Clear();
                }
            }
        }

        /// <summary>
        /// 停靠小车
        /// </summary>
        /// <param name="nCartID"></param>
        /// <param name="sName"></param>
        /// <param name="sDescription"></param>
        /// <param name="nCount"></param>
        /// <param name="sUnit"></param>
        /// <returns></returns>
        public string DockCart(int nCartID, string sName, string sDescription, int nCount, string sUnit)
        {
            string result = "Success";
            try
            {
                //设备控制
                CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(nCartID);
                Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();

                Display900UItem publisherDisplay900UItem = new Display900UItem();
                publisherDisplay900UItem.Name = sName;
                publisherDisplay900UItem.Description = sDescription;
                publisherDisplay900UItem.Count = (ushort)nCount;
                publisherDisplay900UItem.Unit = sUnit;

                ptl900UPublisher.Clear(true);
                ptl900UPublisher.Lock();
                ptl900UPublisher.Display(publisherDisplay900UItem, LightColor.Off);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 解绑小车
        /// </summary>
        /// <param name="nCartID"></param>
        /// <returns></returns>
        public string UnDockCart(int nCartID)
        {
            string result = "Success";
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    CFG_Cart cfgCart = dbContext.CFG_Carts.FirstOrDefault(t => t.Id == nCartID);
                    if (cfgCart != null)
                    {
                        List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = cfgCart.CFG_CartCurrentMaterials.ToList();

                        //设备控制
                        CartPtl cartPtl = CartPtlHost.Instance.GetCartPtl(nCartID);
                        Ptl900U ptl900UPublisher = cartPtl.GetPtl900UPublisher();
                        Ptl900U ptl900ULight = cartPtl.GetPtl900ULight();

                        ptl900UPublisher.Clear(true);
                        ptl900UPublisher.Unlock();

                        foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials)
                        {
                            Ptl900U ptl900U = cartPtl.GetPtl900UByPosition(cfgCartCurrentMaterial.Position);

                            ptl900U.Clear(true);
                            ptl900U.Unlock();
                        }

                        ptl900ULight.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    }
}