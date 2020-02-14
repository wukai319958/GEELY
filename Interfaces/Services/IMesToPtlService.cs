using System.ServiceModel;

namespace Interfaces.Services
{
    /// <summary>
    /// MES 到 PTL 的接口服务。
    /// </summary>
    [ServiceContract]
    public interface IMesToPtlService
    {
        /// <summary>
        /// 车身抵达工位。
        /// </summary>
        [OperationContract]
        string MesAssemblePTL(string xml);
        /// <summary>
        /// MES触发分装配送的分装信息发送至至分装PTL
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        [OperationContract]
        string FeedZonePTL(string xml);

        [OperationContract]
        string AssemblingPTL(string xml);
    }
}