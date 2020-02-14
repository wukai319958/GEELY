using System.ComponentModel.DataAnnotations;

namespace DataAccess.Assorting
{
    /// <summary>
    /// 单车拣选结果的明细。
    /// </summary>
    public class AST_CartResultItem
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属结果的外键。
        /// </summary>
        public long AST_CartResultId { get; set; }

        /// <summary>
        /// 获取或设置小车库位。
        /// </summary>
        public int CartPosition { get; set; }

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
        public int Quantity { get; set; }

        /// <summary>
        /// 获取或设置所属结果。
        /// </summary>
        public virtual AST_CartResult AST_CartResult { get; set; }
    }
}