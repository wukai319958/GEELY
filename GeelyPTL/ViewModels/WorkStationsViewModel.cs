using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataAccess;
using DataAccess.Config;
using GeelyPTL.Models;

namespace GeelyPTL.ViewModels
{
    /// <summary>
    /// 工位列表界面的视图模型。
    /// </summary>
    public class WorkStationsViewModel
    {
        readonly ObservableCollection<WorkStationModel> items = new ObservableCollection<WorkStationModel>();

        /// <summary>
        /// 获取工位的集合。
        /// </summary>
        public ObservableCollection<WorkStationModel> Items
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
                List<CFG_WorkStation> cfgWorkStations = dbContext.CFG_WorkStations
                                                            .OrderBy(c => c.Code)
                                                            .ToList();
                foreach (CFG_WorkStation cfgWorkStation in cfgWorkStations)
                {
                    WorkStationModel workStationModel = new WorkStationModel();
                    workStationModel.CFG_WorkStation = cfgWorkStation;

                    CFG_Cart cfgCart1 = cfgWorkStation.CFG_WorkStationCurrentCarts.Where(wscc => wscc.Position == 1).Select(wscc => wscc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart2 = cfgWorkStation.CFG_WorkStationCurrentCarts.Where(wscc => wscc.Position == 2).Select(wscc => wscc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart3 = cfgWorkStation.CFG_WorkStationCurrentCarts.Where(wscc => wscc.Position == 3).Select(wscc => wscc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart4 = cfgWorkStation.CFG_WorkStationCurrentCarts.Where(wscc => wscc.Position == 4).Select(wscc => wscc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart5 = cfgWorkStation.CFG_WorkStationCurrentCarts.Where(wscc => wscc.Position == 5).Select(wscc => wscc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart6 = cfgWorkStation.CFG_WorkStationCurrentCarts.Where(wscc => wscc.Position == 6).Select(wscc => wscc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart7 = cfgWorkStation.CFG_WorkStationCurrentCarts.Where(wscc => wscc.Position == 7).Select(wscc => wscc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart8 = cfgWorkStation.CFG_WorkStationCurrentCarts.Where(wscc => wscc.Position == 8).Select(wscc => wscc.CFG_Cart).FirstOrDefault();

                    if (cfgCart1 != null)
                        workStationModel.CurrentCartName1 = cfgCart1.Name;
                    if (cfgCart2 != null)
                        workStationModel.CurrentCartName2 = cfgCart2.Name;
                    if (cfgCart3 != null)
                        workStationModel.CurrentCartName3 = cfgCart3.Name;
                    if (cfgCart4 != null)
                        workStationModel.CurrentCartName4 = cfgCart4.Name;
                    if (cfgCart5 != null)
                        workStationModel.CurrentCartName5 = cfgCart5.Name;
                    if (cfgCart6 != null)
                        workStationModel.CurrentCartName6 = cfgCart6.Name;
                    if (cfgCart7 != null)
                        workStationModel.CurrentCartName7 = cfgCart7.Name;
                    if (cfgCart8 != null)
                        workStationModel.CurrentCartName8 = cfgCart8.Name;

                    this.Items.Add(workStationModel);
                }
            }
        }
    }
}