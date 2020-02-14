using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.AssortingPDA
{
    /// <summary>
    /// LES 原始分拣任务的通讯消息。
    /// </summary>
    public class AST_LesTaskMessage_PDA
    {
        /// <summary>
        /// 获取或设置所属任务的外键。
        /// </summary>
        [Key]
        [ForeignKey("AST_LesTask_PDA")]
        public long AST_LesTaskId { get; set; }

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
        /// 获取或设置所属任务。
        /// </summary>
        public virtual AST_LesTask_PDA AST_LesTask_PDA { get; set; }
    }
}