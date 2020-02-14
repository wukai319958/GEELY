using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Config
{
    /// <summary>
    /// 分拣巷道的当前托盘。
    /// </summary>
    public class CFG_ChannelCurrentPallet
    {
        /// <summary>
        /// 获取或设置分拣巷道的主键。
        /// </summary>
        [Key]
        [ForeignKey("CFG_Channel")]
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置当前托盘的外键。
        /// </summary>
        public int? CFG_PalletId { get; set; }

        /// <summary>
        /// 获取或设置托盘抵达时间。
        /// </summary>
        public DateTime? ArrivedTime { get; set; }

        /// <summary>
        /// 获取或设置分拣巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }

        /// <summary>
        /// 获取或设置当前托盘。
        /// </summary>
        public virtual CFG_Pallet CFG_Pallet { get; set; }
    }
}