using System.ComponentModel.DataAnnotations;
using DataAccess.Config;

namespace DataAccess.Assorting
{
    /// <summary>
    /// 单托拣选结果的明细。
    /// </summary>
    public class AST_PalletResultItem
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属结果的外键。
        /// </summary>
        public long AST_PalletResultId { get; set; }

        /// <summary>
        /// 获取或设置试制工位的外键。
        /// </summary>
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置量产工位的逗号分隔列表。
        /// </summary>
        [MaxLength(100)]
        public string GzzList { get; set; }

        /// <summary>
        /// 获取或设置拣选单明细的编号。
        /// </summary>
        [MaxLength(100)]
        public string BillDetailId { get; set; }

        /// <summary>
        /// 获取或设置料箱条码。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string BoxCode { get; set; }

        /// <summary>
        /// 获取或设置托盘源库位。
        /// </summary>
        public int PalletPosition { get; set; }

        /// <summary>
        /// 获取或设置物料编码。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 获取或设置物料名称。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MaterialName { get; set; }

        /// <summary>
        /// 获取或设置物料条码。
        /// </summary>
        [MaxLength(100)]
        public string MaterialBarcode { get; set; }

        /// <summary>
        /// 获取或设置应拣数量。
        /// </summary>
        public int ToPickQuantity { get; set; }

        /// <summary>
        /// 获取或设置实拣数量。
        /// </summary>
        public int PickedQuantity { get; set; }

        /// <summary>
        /// 获取或设置所放料车的外键。
        /// </summary>
        public int? CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置所放料车的储位。
        /// </summary>
        public int? CartPosition { get; set; }

        /// <summary>
        /// 获取或设置所属结果。
        /// </summary>
        public virtual AST_PalletResult AST_PalletResult { get; set; }

        /// <summary>
        /// 获取或设置试制工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置所放的料车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }
    }
}