using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Assorting
{
    /// <summary>
    /// LES 原始分拣任务的明细项。
    /// </summary>
    public class AST_LesTaskItem
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属任务的外键。
        /// </summary>
        public long AST_LesTaskId { get; set; }

        /// <summary>
        /// 获取或设置拣选单明细的编号。
        /// </summary>
        [MaxLength(100)]
        public string BillDetailId { get; set; }

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
        /// 获取或设置小车单个库位能存放此种物料的最大数量。
        /// </summary>
        [Column("MaxQuantitySinglePosition")]
        public int MaxQuantityInSingleCartPosition { get; set; }

        /// <summary>
        /// 获取或设置是否特殊件，通用件需合并应拣数量，特殊件不合并。
        /// </summary>
        public bool IsSpecial { get; set; }

        /// <summary>
        /// 获取或设置是否大件，大件需要占用同层的两个储位。
        /// </summary>
        public bool IsBig { get; set; }

        /// <summary>
        /// 获取或设置合并到的按托明细的外键。
        /// </summary>
        public long? AST_PalletTaskItemId { get; set; }

        /// <summary>
        /// 获取或设置所属任务。
        /// </summary>
        public virtual AST_LesTask AST_LesTask { get; set; }

        /// <summary>
        /// 获取或设置合并到的按托明细。
        /// </summary>
        public virtual AST_PalletTaskItem AST_PalletTaskItem { get; set; }
    }
}