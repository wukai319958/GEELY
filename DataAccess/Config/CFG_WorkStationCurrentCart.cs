using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Config
{
    /// <summary>
    /// 工位所停靠的小车。
    /// </summary>
    public class CFG_WorkStationCurrentCart
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置所在工位的外键。
        /// </summary>
        [Index("UK_CFG_WorkStationCurrentCart", 1, IsUnique = true)]
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置小车停靠的位置。
        /// </summary>
        [Index("UK_CFG_WorkStationCurrentCart", 2, IsUnique = true)]
        public int Position { get; set; }

        /// <summary>
        /// 获取或设置当前小车的外键。
        /// </summary>
        public int? CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置停靠时间。
        /// </summary>
        public DateTime? DockedTime { get; set; }

        /// <summary>
        /// 获取或设置所在工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置当前小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }
    }
}