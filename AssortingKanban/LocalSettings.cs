using System;

namespace AssortingKanban
{
    /// <summary>
    /// 本机设置。
    /// </summary>
    [Serializable]
    public class LocalSettings
    {
        /// <summary>
        /// 获取或设置服务器 IP。
        /// </summary>
        public string ServerIP { get; set; }

        /// <summary>
        /// 获取或设置服务端口。
        /// </summary>
        public int ServicePort { get; set; }

        /// <summary>
        /// 获取或设置所属分拣口的编码。
        /// </summary>
        public string CfgChannelCode { get; set; }

        /// <summary>
        /// 初始化本机设置。
        /// </summary>
        public LocalSettings()
        {
            this.ServerIP = "localhost";
            this.ServicePort = 5980;
        }
    }
}