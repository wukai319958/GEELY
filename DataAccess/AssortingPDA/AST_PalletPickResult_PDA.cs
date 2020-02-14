using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DataAccess.AssortingPDA
{
    /// <summary>
    /// 托盘分拣结果的通知。
    /// </summary>
    public class AST_PalletPickResult_PDA
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置批次
        /// </summary>
        [Index("UK_AST_PalletPickResult_PDA", 1, IsUnique = true)]
        [Required]
        [MaxLength(100)]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置托盘编码
        /// </summary>
        [Index("UK_AST_PalletPickResult_PDA", 2, IsUnique = true)]
        public int CFG_PalletId { get; set; }

        /// <summary>
        /// 此次到达服务于拣选单的 ID 的逗号分隔列表。
        /// </summary>
        [Index("UK_AST_PalletPickResult_PDA", 3, IsUnique = true)]
        [MaxLength(4000)]
        public string PickBillIds { get; set; }

        /// <summary>
        /// 获取或设置料箱编码
        /// </summary>
        [MaxLength(100)]
        public string BoxCode { get; set; }

        /// <summary>
        /// 获取或设置拣选状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 获取或设置数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 获取或设置接收时间
        /// </summary>
        public DateTime ReceivedTime { get; set; }

        /// <summary>
        /// 获取或设置所属通知的标识。
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string AST_PalletPickResultKey { get; set; }
    }
}
