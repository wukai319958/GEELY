using System.Runtime.Serialization;
using DataAccess.Assorting;

namespace Interfaces.Entities
{
    /// <summary>
    /// 托盘任务的明细。
    /// </summary>
    [DataContract]
    public class AST_PalletTaskItemDto
    {
        /// <summary>
        /// 获取或设置明细的主键。
        /// </summary>
        [DataMember]
        public long AST_PalletTaskItemId { get; set; }

        /// <summary>
        /// 获取或设置托盘库位。
        /// </summary>
        [DataMember]
        public int FromPalletPosition { get; set; }

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
        /// 获取或设置应拣数量。
        /// </summary>
        [DataMember]
        public int ToPickQuantity { get; set; }

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
        public PickStatus PickStatus { get; set; }

        /// <summary>
        /// 获取或设置实拣数量。
        /// </summary>
        [DataMember]
        public int? PickedQuantity { get; set; }
    }
}