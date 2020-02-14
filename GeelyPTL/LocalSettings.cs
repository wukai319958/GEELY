using System;

namespace GeelyPTL
{
    /// <summary>
    /// 本机设置。
    /// </summary>
    [Serializable]
    public class LocalSettings
    {
        /// <summary>
        /// 获取或设置承载的各服务的 IP。
        /// </summary>
        public string ServiceIP { get; set; }

        /// <summary>
        /// 获取或设置承载的各服务的端口。
        /// </summary>
        public int ServicePort { get; set; }

        /// <summary>
        /// 获取或设置 LES 的服务地址。
        /// </summary>
        public string PtlToLesServiceUrl { get; set; }

        /// <summary>
        /// 获取或设置 MES 的服务地址。
        /// </summary>
        public string PtlToMesServiceUrl { get; set; }

        /// <summary>
        /// 获取或设置数据库提供者。
        /// </summary>
        public string ConnectionStringProviderName { get; set; }

        /// <summary>
        /// 获取或设置数据库连接字符串的模板，其中密码使用 {0} 占位符。
        /// </summary>
        public string ConnectionStringFormat { get; set; }

        /// <summary>
        /// 获取或设置数据库连接字符串的密码。
        /// </summary>
        public string ConnectionStringPassword { get; set; }

        /// <summary>
        /// 获取或设置 OPC 服务的名称。
        /// </summary>
        public string OpcServiceName { get; set; }

        /// <summary>
        /// 获取或设置 OPC 服务器地址。
        /// </summary>
        public string OpcServerIP { get; set; }

        /// <summary>
        /// 获取或设置日志保存的天数。
        /// </summary>
        public int HistoryHoldingDays { get; set; }

        /// <summary>
        /// 获取或设置 AGV 服务器地址。
        /// </summary>
        public string PtlToAgvServiceUrl { get; set; }

        /// <summary>
        /// 初始化本机设置。
        /// </summary>
        public LocalSettings()
        {
            this.ServiceIP = "localhost";
            this.ServicePort = 5980;
            this.PtlToLesServiceUrl = "http://172.22.80.28:1839/Service/PtlToLesService";
            this.PtlToMesServiceUrl = "http://10.34.91.56:8088/mes-interface/remote/toMes";
            this.ConnectionStringProviderName = "System.Data.SqlClient";
            this.ConnectionStringFormat = "Server=10.34.252.94;Database=GEELY_PTL;User Id=PTL;Password={0};Application Name='GeelyPTL';";
            this.ConnectionStringPassword = "123456";
            this.OpcServiceName = "Kepware.KEPServerEX.V5";
            this.OpcServerIP = "172.22.80.33";
            this.HistoryHoldingDays = 30;

            this.PtlToAgvServiceUrl = "http://10.34.252.94:80/rcs/services/rest/hikRpcService";
        }
    }
}