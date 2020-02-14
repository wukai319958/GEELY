using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Aris.SystemExtension.Xml.Serialization;
using AssortingKanban.ForAssortingKanban;

namespace AssortingKanban
{
    /// <summary>
    /// 登录窗口，会定时尝试登录。
    /// </summary>
    public partial class LogOnWindow : Window
    {
        DispatcherTimer autoLogOnTimer;
        int retryCount;

        /// <summary>
        /// 初始化登录窗口。
        /// </summary>
        public LogOnWindow()
        {
            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //七个分拣口
            for (int i = 1; i <= 7; i++)
                this.comboBoxCfgChannelCode.Items.Add(i.ToString(CultureInfo.InvariantCulture));

            LocalSettings localSettings = new XmlSerializerWrapper<LocalSettings>().Entity;
            this.textBoxServerIP.Text = localSettings.ServerIP;
            this.textBoxServicePort.Text = localSettings.ServicePort.ToString(CultureInfo.InvariantCulture);
            this.comboBoxCfgChannelCode.Text = localSettings.CfgChannelCode;

            if (!string.IsNullOrEmpty(localSettings.CfgChannelCode))
            {
                try
                {
                    this.DoLogOn();

                    this.DialogResult = true;
                    this.Close();
                }
                catch { }

                this.autoLogOnTimer = new DispatcherTimer();
                this.autoLogOnTimer.Interval = TimeSpan.FromMinutes(1);
                this.autoLogOnTimer.Tick += this.timer_Tick;
                this.autoLogOnTimer.Start();
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                this.retryCount++;
                this.buttonLogOn.Content = string.Format(CultureInfo.InvariantCulture, "正在登录 {0} ...", this.retryCount);
                this.buttonLogOn.IsEnabled = false;

                this.DoLogOn();

                this.autoLogOnTimer.Stop();

                this.DialogResult = true;
                this.Close();
            }
            catch
            {
                this.IsEnabled = true;
            }
        }

        private void buttonLogOn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DoLogOn();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void DoLogOn()
        {
            using (ForAssortingKanbanService proxy = new ForAssortingKanbanService())
            {
                proxy.Url = string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/ForAssortingKanbanService/", this.textBoxServerIP.Text, this.textBoxServicePort.Text);

                CFG_ChannelDto[] cfgChannels = proxy.QueryChannels();
                if (cfgChannels.All(c => c.Code != this.comboBoxCfgChannelCode.Text))
                {
                    this.comboBoxCfgChannelCode.Items.Clear();
                    foreach (CFG_ChannelDto cfgChannel in cfgChannels)
                        this.comboBoxCfgChannelCode.Items.Add(cfgChannel.Code);

                    throw new Exception("无效的巷道编码。");
                }
            }

            XmlSerializerWrapper<LocalSettings> localSettingsXmlSerializer = new XmlSerializerWrapper<LocalSettings>();
            LocalSettings localSettings = localSettingsXmlSerializer.Entity;
            localSettings.ServerIP = this.textBoxServerIP.Text;
            localSettings.ServicePort = int.Parse(this.textBoxServicePort.Text, CultureInfo.InvariantCulture);
            localSettings.CfgChannelCode = this.comboBoxCfgChannelCode.Text;

            localSettingsXmlSerializer.Save();
        }
    }
}