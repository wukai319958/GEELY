using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Assorting
{
    /// <summary>
    /// 单车拣选结果的通讯消息。
    /// </summary>
    public class AST_CartResultMessage
    {
        /// <summary>
        /// 获取或设置所属结果的外键。
        /// </summary>
        [Key]
        [ForeignKey("AST_CartResult")]
        public long AST_CartResultId { get; set; }

        /// <summary>
        /// 获取或设置已发送的消息。
        /// </summary>
        public string SentXml { get; set; }

        /// <summary>
        /// 获取或设置最后发送时间。
        /// </summary>
        public DateTime? LastSentTime { get; set; }

        /// <summary>
        /// 获取或设置是否发送成功。
        /// </summary>
        public bool SentSuccessful { get; set; }

        /// <summary>
        /// 获取或设置已收到的消息。
        /// </summary>
        public string ReceivedXml { get; set; }

        /// <summary>
        /// 获取或设置所属结果。
        /// </summary>
        public virtual AST_CartResult AST_CartResult { get; set; }
    }
}