using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 装配指示的通讯消息。
    /// </summary>
    public class ASM_AssembleIndicationMessage
    {
        /// <summary>
        /// 获取或设置所属指示的外键。
        /// </summary>
        [Key]
        [ForeignKey("ASM_AssembleIndication")]
        public long ASM_AssembleIndicationId { get; set; }

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
        /// 获取或设置所属指示。
        /// </summary>
        public virtual ASM_AssembleIndication ASM_AssembleIndication { get; set; }
    }
}