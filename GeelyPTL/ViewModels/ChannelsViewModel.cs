using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataAccess;
using DataAccess.Config;
using GeelyPTL.Models;

namespace GeelyPTL.ViewModels
{
    /// <summary>
    /// 分拣口列表界面的视图模型。
    /// </summary>
    public class ChannelsViewModel
    {
        readonly ObservableCollection<ChannelModel> items = new ObservableCollection<ChannelModel>();

        /// <summary>
        /// 获取分拣口的集合。
        /// </summary>
        public ObservableCollection<ChannelModel> Items
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
                List<CFG_Channel> cfgChannels = dbContext.CFG_Channels
                                                    .OrderBy(c => c.Code)
                                                    .ToList();
                foreach (CFG_Channel cfgChannel in cfgChannels)
                {
                    ChannelModel channelModel = new ChannelModel();
                    channelModel.CFG_Channel = cfgChannel;

                    CFG_Pallet cfgPallet = cfgChannel.CFG_ChannelCurrentPallet.CFG_Pallet;

                    if (cfgPallet != null)
                        channelModel.CurrentPalletCode = cfgPallet.Code;

                    CFG_Cart cfgCart1 = cfgChannel.CFG_ChannelCurrentCarts.Where(ccc => ccc.Position == 1).Select(ccc => ccc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart2 = cfgChannel.CFG_ChannelCurrentCarts.Where(ccc => ccc.Position == 2).Select(ccc => ccc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart3 = cfgChannel.CFG_ChannelCurrentCarts.Where(ccc => ccc.Position == 3).Select(ccc => ccc.CFG_Cart).FirstOrDefault();
                    CFG_Cart cfgCart4 = cfgChannel.CFG_ChannelCurrentCarts.Where(ccc => ccc.Position == 4).Select(ccc => ccc.CFG_Cart).FirstOrDefault();

                    if (cfgCart1 != null)
                        channelModel.CurrentCartName1 = cfgCart1.Name;
                    if (cfgCart2 != null)
                        channelModel.CurrentCartName2 = cfgCart2.Name;
                    if (cfgCart3 != null)
                        channelModel.CurrentCartName3 = cfgCart3.Name;
                    if (cfgCart4 != null)
                        channelModel.CurrentCartName4 = cfgCart4.Name;

                    if (cfgChannel.Code.Equals("8"))
                    {
                        channelModel.Light1OnLine = false;
                        channelModel.Light2OnLine = false;
                        channelModel.Light3OnLine = false;
                    }
                    else
                    {
                        channelModel.Light1OnLine = cfgChannel.CFG_ChannelPtlDevices.Where(cpd => cpd.Position == 3).Select(cpd => cpd.OnLine).First();
                        channelModel.Light2OnLine = cfgChannel.CFG_ChannelPtlDevices.Where(cpd => cpd.Position == 4).Select(cpd => cpd.OnLine).First();
                        channelModel.Light3OnLine = cfgChannel.CFG_ChannelPtlDevices.Where(cpd => cpd.Position == 5).Select(cpd => cpd.OnLine).First();
                    }
                    this.Items.Add(channelModel);
                }
            }
        }
    }
}