using System.ComponentModel.DataAnnotations;

namespace DataAccess.CartFinding
{
    /// <summary>
    /// 单车发车结果的明细。
    /// </summary>
    public class FND_DeliveryResultItem
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属发车结果的外键。
        /// </summary>
        public long FND_DeliveryResultId { get; set; }

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
        /// 获取或设置发出数量。
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 获取或设置所属发车结果。
        /// </summary>
        public virtual FND_DeliveryResult FND_DeliveryResult { get; set; }
    }
}