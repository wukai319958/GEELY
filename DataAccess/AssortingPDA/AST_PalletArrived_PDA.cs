using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Config;

namespace DataAccess.AssortingPDA
{
    /// <summary>
    /// 托盘抵达分拣口的通知。
    /// </summary>
    public class AST_PalletArrived_PDA
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置项目编号。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置 WBS 编号。
        /// </summary>
        [MaxLength(100)]
        public string WbsId { get; set; }

        /// <summary>
        /// 获取或设置项目阶段。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置配送批次。
        /// </summary>
        //[Index("UK_AST_PalletArrived_PDA", 1, IsUnique = true)]
        [Index]
        [Required]
        [MaxLength(100)]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置抵达的分拣巷道的外键。
        /// </summary>
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置抵达的托盘的外键。
        /// </summary>
        //[Index("UK_AST_PalletArrived_PDA", 2, IsUnique = true)]
        [Index]
        public int CFG_PalletId { get; set; }

        /// <summary>
        /// 此次到达服务于拣选单的 ID 的逗号分隔列表。
        /// </summary>
        //[Index("UK_AST_PalletArrived_PDA", 3, IsUnique = true)]
        [MaxLength(4000)]
        public string PickBillIds { get; set; }

        /// <summary>
        /// 获取或设置抵达时间。
        /// </summary>
        [Index]
        public DateTime ArrivedTime { get; set; }

        /// <summary>
        /// 获取或设置抵达的分拣巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }

        /// <summary>
        /// 获取或设置抵达的托盘。
        /// </summary>
        public virtual CFG_Pallet CFG_Pallet { get; set; }

        /// <summary>
        /// 获取或设置关联的通讯消息。
        /// </summary>
        public virtual AST_PalletArrivedMessage_PDA AST_PalletArrivedMessage_PDA { get; set; }
    }
}