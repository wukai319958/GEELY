using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 按物料编码和顺序解析到小车库位后的装配指引任务。
    /// </summary>
    public class ASM_Task
    {
        /// <summary>
        /// 初始化装配指引任务。
        /// </summary>
        public ASM_Task()
        {
            this.ASM_TaskItems = new HashSet<ASM_TaskItem>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置关联的指示的外键。
        /// </summary>
        [Index(IsUnique = true)]
        public long ASM_AssembleIndicationId { get; set; }

        /// <summary>
        /// 获取或设置装配指示状态。
        /// </summary>
        [Index]
        public AssembleStatus AssembleStatus { get; set; }

        /// <summary>
        /// 获取或设置关联的指示。
        /// </summary>
        public virtual ASM_AssembleIndication ASM_AssembleIndication { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_TaskItem> ASM_TaskItems { get; set; }
    }
}