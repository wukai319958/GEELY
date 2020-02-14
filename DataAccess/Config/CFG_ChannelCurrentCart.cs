using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Config
{
    /// <summary>
    /// 分拣巷道的当前小车。
    /// </summary>
    public class CFG_ChannelCurrentCart
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置分拣巷道的主键。
        /// </summary>
        [Index("UK_CFG_ChannelCurrentCart", 1, IsUnique = true)]
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置小车停靠的位置。
        /// </summary>
        [Index("UK_CFG_ChannelCurrentCart", 2, IsUnique = true)]
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
        /// 获取或设置分拣巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }

        /// <summary>
        /// 获取或设置当前小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }
    }
}