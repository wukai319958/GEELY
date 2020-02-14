using System.Runtime.Serialization;
using DataAccess.Assorting;

namespace Interfaces.Entities
{
    /// <summary>
    /// 料车任务的明细。
    /// </summary>
    [DataContract]
    public class AST_CartTaskItemDto
    {
        /// <summary>
        /// 获取或设置明细的主键。
        /// </summary>
        [DataMember]
        public long AST_CartTaskItemId { get; set; }

        /// <summary>
        /// 获取或设置储位。
        /// </summary>
        [DataMember]
        public int CartPosition { get; set; }

        /// <summary>
        /// 获取或设置目标工位的编码。
        /// </summary>
        [DataMember]
        public string WorkStationCode { get; set; }

        /// <summary>
        /// 获取或设置物料编码。
        /// </summary>
        [DataMember]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 获取或设置物料名称。
        /// </summary>
        [DataMember]
        public string MaterialName { get; set; }

        /// <summary>
        /// 获取或设置条码。
        /// </summary>
        [DataMember]
        public string MaterialBarcode { get; set; }

        /// <summary>
        /// 获取或设置库位容量。
        /// </summary>
        [DataMember]
        public int MaxQuantityInSingleCartPosition { get; set; }

        /// <summary>
        /// 获取或设置是否特殊件。
        /// </summary>
        [DataMember]
        public bool IsSpecial { get; set; }

        /// <summary>
        /// 获取或设置是否大件，大件需要占用同层的两个储位。
        /// </summary>
        [DataMember]
        public bool IsBig { get; set; }

        /// <summary>
        /// 获取或设置分拣状态。
        /// </summary>
        [DataMember]
        public AssortingStatus AssortingStatus { get; set; }

        /// <summary>
        /// 获取或设置已分放数量。
        /// </summary>
        [DataMember]
        public int? PickedQuantity { get; set; }
    }
}