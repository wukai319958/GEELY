using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataAccess;
using DataAccess.Config;
using GeelyPTL.Models;

namespace GeelyPTL.Dialogs
{
    /// <summary>
    /// 铺线巷道选择界面。
    /// </summary>
    public partial class SelectInitChannelWindow : Window
    {
        /// <summary>
        /// 选择的巷道ID
        /// </summary>
        public List<int> InitChannelIds { get; set; }

        /// <summary>
        /// 巷道选择数据集
        /// </summary>
        private List<ChannelSelModel> channelSelModels;

        /// <summary>
        /// 初始化铺线巷道选择界面
        /// </summary>
        public SelectInitChannelWindow()
        {
            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitChannelIds = new List<int>();

                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    channelSelModels = new List<ChannelSelModel>();

                    List<CFG_Channel> cfgChannels = dbContext.CFG_Channels.OrderBy(t => t.Id).ToList();

                    foreach (CFG_Channel cfgChannel in cfgChannels)
                    {
                        ChannelSelModel channelSelModel = new ChannelSelModel();
                        channelSelModel.Id = cfgChannel.Id;
                        channelSelModel.Code = cfgChannel.Code;
                        channelSelModel.Name = cfgChannel.Name;
                        channelSelModel.IsChecked = false;

                        channelSelModels.Add(channelSelModel);
                    }

                    this.dataGrid.ItemsSource = channelSelModels;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk1 = (CheckBox)sender;

            if (channelSelModels != null && channelSelModels.Count > 0)
            {
                foreach (ChannelSelModel channelSelModel in channelSelModels)
                {
                    channelSelModel.IsChecked = chk1.IsChecked.Value;
                }

                this.dataGrid.ItemsSource = channelSelModels;
            }
        }

        private void cb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            ChannelSelModel vd = chk.Tag as ChannelSelModel;
            vd.IsChecked = chk.IsChecked.Value;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            InitChannelIds.Clear();
            if (channelSelModels != null && channelSelModels.Count > 0)
            {
                foreach (ChannelSelModel channelSelModel in channelSelModels)
                {
                    if (channelSelModel.IsChecked)
                    {
                        InitChannelIds.Add(channelSelModel.Id);
                    }
                }
            }

            if (InitChannelIds.Count == 0)
            {
                MessageBox.Show("请选择需要铺线的巷道");
                return;
            }
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            InitChannelIds.Clear();
            this.Close();
        }
        
    }
}