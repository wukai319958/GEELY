using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataAccess;
using DataAccess.Config;
using GeelyPTL.Models;

namespace GeelyPTL.ViewModels
{
    /// <summary>
    /// 料车列表界面的视图模型。
    /// </summary>
    public class CartsViewModel
    {
        readonly ObservableCollection<CartModel> items = new ObservableCollection<CartModel>();

        /// <summary>
        /// 获取料车的集合。
        /// </summary>
        public ObservableCollection<CartModel> Items
        {
            get { return this.items; }
        }

        /// <summary>
        /// 刷新视图模型。
        /// </summary>
        public void Refresh()
        {
            this.Items.Clear();

            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                List<CFG_Cart> cfgCarts = dbContext.CFG_Carts
                                              .OrderBy(c => c.Code)
                                              .ToList();
                foreach (CFG_Cart cfgCart in cfgCarts)
                {
                    CartModel cartModel = new CartModel();
                    cartModel.CFG_Cart = cfgCart;

                    cartModel.Light1OnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 1).Select(cpd => cpd.OnLine).First();
                    cartModel.Light2OnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 2).Select(cpd => cpd.OnLine).First();
                    cartModel.Light3OnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 3).Select(cpd => cpd.OnLine).First();
                    cartModel.Light4OnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 4).Select(cpd => cpd.OnLine).First();
                    cartModel.Light5OnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 5).Select(cpd => cpd.OnLine).First();
                    cartModel.Light6OnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 6).Select(cpd => cpd.OnLine).First();
                    cartModel.Light7OnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 7).Select(cpd => cpd.OnLine).First();
                    cartModel.Light8OnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 8).Select(cpd => cpd.OnLine).First();
                    cartModel.PublisherOnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 9).Select(cpd => cpd.OnLine).First();
                    cartModel.LighthouseOnLine = cfgCart.CFG_CartPtlDevices.Where(cpd => cpd.DeviceAddress == 10).Select(cpd => cpd.OnLine).First();

                    this.Items.Add(cartModel);
                }
            }
        }
    }
}