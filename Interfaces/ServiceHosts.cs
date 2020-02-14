using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Description;
using Interfaces.Services;

namespace Interfaces
{
    /// <summary>
    /// 统一管理所有的服务宿主。
    /// </summary>
    public static class ServiceHosts
    {
        static ServiceHost forAssortingKanbanServiceHost;
        static ServiceHost forCartFindingClientServiceHost;
        static ServiceHost lesToPtlServiceHost;
        static ServiceHost mesToPtlServiceHost;
        static ServiceHost feedZonePDAServiceHost;

        /// <summary>
        /// 打开所有的服务。
        /// </summary>
        /// <param name="ip">各服务统一的 IP 地址。</param>
        /// <param name="httpPort">各 HTTP 通道服务统一的端口。</param>
        public static void Open(string ip, int httpPort)
        {
            Uri forAssortingKanbanServiceBaseAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/ForAssortingKanbanService/", ip, httpPort));
            Uri forCartFindingClientServiceBaseAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/ForCartFindingClientService/", ip, httpPort));
            Uri lesToPtlServiceBaseAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/LesToPtlService/", ip, httpPort));
            Uri mesToPtlServiceBaseAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/MesToPtlService/", ip, httpPort));
            Uri feedZonePDABaseAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}/FeedZonePDAService/", ip, httpPort));

            ServiceHosts.forAssortingKanbanServiceHost = new ServiceHost(ForAssortingKanbanService.Instance, forAssortingKanbanServiceBaseAddress);
            ServiceHosts.forCartFindingClientServiceHost = new ServiceHost(ForCartFindingClientService.Instance, forCartFindingClientServiceBaseAddress);
            ServiceHosts.lesToPtlServiceHost = new ServiceHost(LesToPtlService.Instance, lesToPtlServiceBaseAddress);
            ServiceHosts.mesToPtlServiceHost = new ServiceHost(MesToPtlService.Instance, mesToPtlServiceBaseAddress);
            ServiceHosts.feedZonePDAServiceHost = new ServiceHost(FeedZonePDAService.Instance, feedZonePDABaseAddress);

            ServiceHosts.AddDefaultAndHttpMexEndPoints(ServiceHosts.forAssortingKanbanServiceHost);
            ServiceHosts.AddDefaultAndHttpMexEndPoints(ServiceHosts.forCartFindingClientServiceHost);
            ServiceHosts.AddDefaultAndHttpMexEndPoints(ServiceHosts.lesToPtlServiceHost);
            ServiceHosts.AddDefaultAndHttpMexEndPoints(ServiceHosts.mesToPtlServiceHost);
            ServiceHosts.AddDefaultAndHttpMexEndPoints(ServiceHosts.feedZonePDAServiceHost);

            try { ServiceHosts.forAssortingKanbanServiceHost.Open(); }
            catch
            {
                ServiceHosts.forAssortingKanbanServiceHost = null;
                throw;
            }

            try { ServiceHosts.forCartFindingClientServiceHost.Open(); }
            catch
            {
                ServiceHosts.forCartFindingClientServiceHost = null;
                throw;
            }

            try { ServiceHosts.lesToPtlServiceHost.Open(); }
            catch
            {
                ServiceHosts.lesToPtlServiceHost = null;
                throw;
            }

            try { ServiceHosts.mesToPtlServiceHost.Open(); }
            catch
            {
                ServiceHosts.mesToPtlServiceHost = null;
                throw;
            }

            try { ServiceHosts.feedZonePDAServiceHost.Open(); }
            catch
            {
                ServiceHosts.feedZonePDAServiceHost = null;
                throw;
            }
        }

        /// <summary>
        /// 关闭所有的服务。
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "WCF 资源释放模式")]
        public static void Close()
        {
            if (ServiceHosts.forAssortingKanbanServiceHost != null)
            {
                try { ServiceHosts.forAssortingKanbanServiceHost.Close(); }
                catch { ServiceHosts.forAssortingKanbanServiceHost.Abort(); }

                ServiceHosts.forAssortingKanbanServiceHost = null;
            }

            if (ServiceHosts.forCartFindingClientServiceHost != null)
            {
                try { ServiceHosts.forCartFindingClientServiceHost.Close(); }
                catch { ServiceHosts.forCartFindingClientServiceHost.Abort(); }

                ServiceHosts.forCartFindingClientServiceHost = null;
            }

            if (ServiceHosts.lesToPtlServiceHost != null)
            {
                try { ServiceHosts.lesToPtlServiceHost.Close(); }
                catch { ServiceHosts.lesToPtlServiceHost.Abort(); }

                ServiceHosts.lesToPtlServiceHost = null;
            }

            if (ServiceHosts.mesToPtlServiceHost != null)
            {
                try { ServiceHosts.mesToPtlServiceHost.Close(); }
                catch { ServiceHosts.mesToPtlServiceHost.Abort(); }

                ServiceHosts.mesToPtlServiceHost = null;
            }

            if (ServiceHosts.feedZonePDAServiceHost != null)
            {
                try { ServiceHosts.feedZonePDAServiceHost.Close(); }
                catch { ServiceHosts.feedZonePDAServiceHost.Abort(); }

                ServiceHosts.feedZonePDAServiceHost = null;
            }
        }

        static void AddDefaultAndHttpMexEndPoints(ServiceHost host)
        {
            ServiceMetadataBehavior serviceMetadataBehavior = (ServiceMetadataBehavior)host.Description.Behaviors[typeof(ServiceMetadataBehavior)];
            serviceMetadataBehavior.HttpGetEnabled = true;

            host.AddDefaultEndpoints();
            host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
        }
    }
}