using System.ServiceModel;

namespace Interfaces.Services
{
    /// <summary>
    /// LES 到 PTL 的接口服务。
    /// </summary>
    [ServiceContract]
    public interface ILesToPtlService
    {
        /// <summary>
        /// 提前下发任务。
        /// </summary>
        [OperationContract]
        string LesStockPickPTL(string xml);

        /// <summary>
        /// 托盘抵达分拣口。
        /// </summary>
        [OperationContract]
        string LesSendBoxPTL(string xml);

        /// <summary>
        /// 提前下发PDA拣料任务
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        [OperationContract]
        string LesStockPickPDA(string xml);

        /// <summary>
        /// PDA拣料托盘抵达分拣口
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        [OperationContract]
        string LesSendBoxPDA(string xml);
        /// <summary>
        /// PDA发送拣选任务完成
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        [OperationContract]
        string LesSendFinishStatusPDA(string xml);
    }
}