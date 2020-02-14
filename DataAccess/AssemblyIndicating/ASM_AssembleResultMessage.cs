using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 装配指示拣料结果的通讯消息。
    /// </summary>
    public class ASM_AssembleResultMessage
    {
        /// <summary>
        /// 获取或设置所属结果的外键。
        /// </summary>
        [Key]
        [ForeignKey("ASM_AssembleResult")]
        public long ASM_AssembleResultId { get; set; }

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
        public virtual ASM_AssembleResult ASM_AssembleResult { get; set; }
    }
}