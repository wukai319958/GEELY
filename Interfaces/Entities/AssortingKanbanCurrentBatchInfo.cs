using System.Runtime.Serialization;

namespace Interfaces.Entities
{
    /// <summary>
    /// 分拣看板上的当前批次信息。
    /// </summary>
    [DataContract]
    public class AssortingKanbanCurrentBatchInfo
    {
        /// <summary>
        /// 获取或设置拣料类型（1：PTL料架拣料，2：PDA手持机拣料）。
        /// </summary>
        [DataMember]
        public string PickType { get; set; }
        //public int PickType { get; set; }

        /// <summary>
        /// 获取或设置项目编号。
        /// </summary>
        [DataMember]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置项目阶段。
        /// </summary>
        [DataMember]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置批次。
        /// </summary>
        [DataMember]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置已完成的托盘数。
        /// </summary>
        [DataMember]
        public int FinishedPalletCount { get; set; }

        /// <summary>
        /// 获取或设置所有托盘数。
        /// </summary>
        [DataMember]
        public int TotalPalletCount { get; set; }

        /// <summary>
        /// 获取或设置已完成的品种数。
        /// </summary>
        [DataMember]
        public int FinishedMaterialTypeCount { get; set; }

        /// <summary>
        /// 获取或设置所有品种数。
        /// </summary>
        [DataMember]
        public int TotalMaterialTypeCount { get; set; }

        /// <summary>
        /// 获取或设置已完成零件数。
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