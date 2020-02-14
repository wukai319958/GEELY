using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.AssortingPDA
{
    /// <summary>
    /// 托盘抵达分拣口通知的通讯消息。
    /// </summary>
    public class AST_PalletArrivedMessage_PDA
    {
        /// <summary>
        /// 获取或设置所属通知的外键。
        /// </summary>
        [Key]
        [ForeignKey("AST_PalletArrived_PDA")]
        public long AST_PalletArrivedId { get; set; }

        /// <summary>
        /// 获取或设置已收到的消息。
        /// </summary>
        [Required]
        public string ReceivedXml { get; set; }

        /// <summary>
        /// 获取或设置接收时间。
        /// </summary>
        public DateTime ReceivedTime { get; set; }

        /// <summary>
        /// 获取或设置所属通知。
        /// </summary>
        public virtual AST_PalletArrived_PDA AST_PalletArrived_PDA { get; set; }
    }
}