using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DataAccess.AssortingPDA
{
    /// <summary>
    /// 托盘分拣结果通知的通讯消息。
    /// </summary>
    public class AST_PalletPickResultMessage_PDA
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属通知的标识。
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string AST_PalletPickResultKey { get; set; }

        /// <summary>
        /// 获取或设置已收到的消息。
        /// </summary>
        [Required]
        public string ReceivedXml { get; set; }

        /// <summary>
        /// 获取或设置接收时间。
        /// </summary>
        public DateTime ReceivedTime { get; set; }
    }
}
