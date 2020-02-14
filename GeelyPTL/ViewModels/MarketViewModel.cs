using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataAccess;
using DataAccess.Config;
using GeelyPTL.Models;
using DataAccess.Other;

namespace GeelyPTL.ViewModels
{
    /// <summary>
    /// 物料超市列表界面的视图模型。
    /// </summary>
    public class MarketViewModel
    {
        readonly ObservableCollection<MarketModel> items = new ObservableCollection<MarketModel>();

        /// <summary>
        /// 获取物料超市的集合。
        /// </summary>
        public ObservableCollection<MarketModel> Items
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
                List<string> listWorkStationCode = dbContext.MarketZones.OrderBy(t => t.AreaId).Select(t => t.AreaId).Distinct().ToList();

                List<MarketZone> marketZones = dbContext.MarketZones.ToList();

                foreach (string workStationCode in listWorkStationCode)
                {
                    MarketModel marketModel = new MarketModel();
                    marketModel.WorkStationCode = workStationCode;

                    List<MarketZone> curMarketZones = marketZones.Where(t => t.AreaId.Equals(workStationCode)).ToList();
                    if (curMarketZones.Count > 0)
                    {
                        foreach (MarketZone marketZone in curMarketZones)
                        {
                            if (marketZone.CFG_CartId != null)
                            {
                                if (marketZone.Position == 1)
                                {
                                    CFG_Cart cfgCart1 = dbContext.CFG_Carts.FirstOrDefault(t => t.Id == marketZone.CFG_CartId);
                                    marketModel.CurrentCartName1 = cfgCart1.Name;
                                }
                                else if (marketZone.Position == 2)
                                {
                                    CFG_Cart cfgCart2 = dbContext.CFG_Carts.FirstOrDefault(t => t.Id == marketZone.CFG_CartId);
                                    marketModel.CurrentCartName2 = cfgCart2.Name;
                                }
                                else if (marketZone.Position == 3)
                                {
                                    CFG_Cart cfgCart3 = dbContext.CFG_Carts.FirstOrDefault(t => t.Id == marketZone.CFG_CartId);
                                    marketModel.CurrentCartName3 = cfgCart3.Name;
                                }
                                else if (marketZone.Position == 4)
                                {
                                    CFG_Cart cfgCart4 = dbContext.CFG_Carts.FirstOrDefault(t => t.Id == marketZone.CFG_CartId);
                                    marketModel.CurrentCartName4 = cfgCart4.Name;
                                }
                            }
                        }
                    }

                    this.Items.Add(marketModel);
                }
            }
        }
    }
}