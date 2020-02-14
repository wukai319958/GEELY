using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 装配指示明细。
    /// </summary>
    public class ASM_AssembleIndicationItem
    {
        /// <summary>
        /// 初始化装配指示明细。
        /// </summary>
        public ASM_AssembleIndicationItem()
        {
            this.ASM_TaskItems = new HashSet<ASM_TaskItem>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属指示的外键。
        /// </summary>
        public long ASM_AssembleIndicationId { get; set; }

        /// <summary>
        /// 获取或设置量产工位。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Gzz { get; set; }

        /// <summary>
        /// 获取或设置物料编码。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 获取或设置物料名称。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MaterialName { get; set; }

        /// <summary>
        /// 获取或设置装配顺序。
        /// </summary>
        public int AssembleSequence { get; set; }

        /// <summary>
        /// 获取或设置需装配数量。
        /// </summary>
        public int ToAssembleQuantity { get; set; }

        /// <summary>
        /// 获取或设置齐套性标识。
        /// </summary>
        [MaxLength(100)]
        public string Qtxbs { get; set; }

        /// <summary>
        /// 获取或设置项目编码。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置项目阶段。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置批次，20181109A1。
        /// </summary>
        [MaxLength(100)]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置装配指示状态。
        /// </summary>
        public AssembleStatus AssembleStatus { get; set; }

        /// <summary>
        /// 获取或设置实际装配数量。
        /// </summary>
        public int? AssembledQuantity { get; set; }

        /// <summary>
        /// 获取或设置实际装配时间。
        /// </summary>
        public DateTime? AssembledTime { get; set; }

        /// <summary>
        /// 获取或设置所属指示。
        /// </summary>
        public virtual ASM_AssembleIndication ASM_AssembleIndication { get; set; }

        /// <summary>
        /// 获取解析后任务明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_TaskItem> ASM_TaskItems { get; set; }
    }
}