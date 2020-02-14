using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using DataAccess.Config;
using Ptl.Device;

namespace DeviceCommunicationHost
{
    /// <summary>
    /// 小车上 PTL 设备的通讯宿主。
    /// </summary>
    public class CartPtlHost
    {
        static readonly CartPtlHost instance = new CartPtlHost();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static CartPtlHost Instance
        {
            get { return CartPtlHost.instance; }
        }

        readonly Dictionary<int, CartPtl> cartPtlByCartId = new Dictionary<int, CartPtl>();
        readonly InstallProject installProject = new InstallProject();

        /// <summary>
        /// 获取通讯是否运行中。
        /// </summary>
        public bool IsRunning { get; private set; }

        CartPtlHost()
        { }

        /// <summary>
        /// 启动所有小车的 PTL 通讯。
        /// </summary>
        public void Start()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<CFG_Cart> cfgCarts = dbContext.CFG_Carts
                                              .ToList();

                foreach (CFG_Cart cfgCart in cfgCarts)
                {
                    CartPtl cartPtl = new CartPtl(cfgCart.Id, cfgCart.XGateIP);

                    this.cartPtlByCartId.Add(cfgCart.Id, cartPtl);
                }

                dbContext.SaveChanges();
            }

            foreach (CartPtl cartPtl in this.cartPtlByCartId.Values)
            {
                cartPtl.Start();

                this.installProject.XGates.AddOrUpdate(cartPtl.XGate);
            }

            this.installProject.HeartbeatGenerator.Period = TimeSpan.FromSeconds(60);
            this.installProject.HeartbeatGenerator.Enable = true;

            this.IsRunning = true;
        }

        /// <summary>
        /// 停止所有小车的 PTL 通讯。
        /// </summary>
        public void Stop()
        {
            this.installProject.HeartbeatGenerator.Enable = false;

            foreach (CartPtl cartPtl in this.cartPtlByCartId.Values)
                cartPtl.Stop();

            this.cartPtlByCartId.Clear();
            this.installProject.XGates.Clear();

            this.IsRunning = false;
        }

        /// <summary>
        /// 获取指定小车的 PTL 控制入口。
        /// </summary>
        /// <param name="cfgCartId">小车的主键。</param>
        /// <returns>小车的 PTL 控制入口。</returns>
        public CartPtl GetCartPtl(int cfgCartId)
        {
            return this.cartPtlByCartId[cfgCartId];
        }

        /// <summary>
        /// 按设备获取小车的 PTL 控制入口。
        /// </summary>
        public CartPtl GetCartPtlByPtlDevice(PtlDevice ptlDevice)
        {
            foreach (CartPtl cartPtl in this.cartPtlByCartId.Values)
            {
                if (cartPtl.Contains(ptlDevice))
                    return cartPtl;
            }

            return null;
        }
    }
}