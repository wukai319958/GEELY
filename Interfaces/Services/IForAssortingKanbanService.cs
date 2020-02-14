using System.Collections.Generic;
using System.ServiceModel;
using Interfaces.Entities;

namespace Interfaces.Services
{
    /// <summary>
    /// 为分拣看板提供服务。
    /// </summary>
    [ServiceContract]
    [XmlSerializerFormat]
    public interface IForAssortingKanbanService
    {
        /// <summary>
        /// 查询所有分拣口。
        /// </summary>
        [OperationContract]
        List<CFG_ChannelDto> QueryChannels();

        /// <summary>
        /// 按巷道查询今日统计。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        [OperationContract]
        AssortingKanbanTodayStatistics QueryTodayStatistics(int cfgChannelId);

        /// <summary>
        /// 按巷道查询当前任务。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        [OperationContract]
        AssortingKanbanTaskInfo QueryCurrentTaskInfo(int cfgChannelId);

        /// <summary>
        /// 按巷道查询PDA今日统计。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        [OperationContract]
        AssortingKanbanTodayStatistics QueryPDATodayStatistics(int cfgChannelId);
    }
}