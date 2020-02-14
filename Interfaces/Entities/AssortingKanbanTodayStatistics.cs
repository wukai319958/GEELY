using System.Runtime.Serialization;

namespace Interfaces.Entities
{
    /// <summary>
    /// 分拣看板上的今日统计。
    /// </summary>
    [DataContract]
    public class AssortingKanbanTodayStatistics
    {
        /// <summary>
        /// 获取或设置已完成的批次数。
        /// </summary>
        [DataMember]
        public int FinishedBatchCount { get; set; }

        /// <summary>
        /// 获取或设置所有批次数。
        /// </summary>
        [DataMember]
        public int TotalBatchCount { get; set; }

        /// <summary>
        /// 获取或设置已完成的托盘数。
        /// </summary>
        [DataMember]
        public int FinishedPalletCount { get; set; }

        /// <summary>
        /// 获取或设置所有批次数。
        /// </summary>
        [DataMember]
        public int TotalPalletCount { get; set; }

        /// <summary>
        /// 获取或设置已完成的零件数。
        /// </summary>
        [DataMember]
        public int FinishedMaterialCount { get; set; }

        /// <summary>
        /// 获取或设置所有零件数。
        /// </summary>
        [DataMember]
        public int TotalMaterialCount { get; set; }
    }
}