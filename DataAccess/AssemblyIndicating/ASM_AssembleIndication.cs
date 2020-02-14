using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Config;

namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 装配指示。
    /// </summary>
    public class ASM_AssembleIndication
    {
        /// <summary>
        /// 初始化装配指示。
        /// </summary>
        public ASM_AssembleIndication()
        {
            this.ASM_AssembleIndicationItems = new HashSet<ASM_AssembleIndicationItem>();
            this.ASM_AssembleResults = new HashSet<ASM_AssembleResult>();
            this.ASM_Tasks = new HashSet<ASM_Task>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置工厂编码。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FactoryCode { get; set; }

        /// <summary>
        /// 获取或设置产线编码。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProductionLineCode { get; set; }

        /// <summary>
        /// 获取或设置试制工位的外键。
        /// </summary>
        [Index("UK_ASM_AssembleIndication", 1, IsUnique = true)]
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置量产工位的逗号分隔列表。
        /// </summary>
        [Index("UK_ASM_AssembleIndication", 2, IsUnique = true)]
        [Required]
        [MaxLength(1000)]
        public string GzzList { get; set; }

        /// <summary>
        /// 获取或设置工单号。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MONumber { get; set; }

        /// <summary>
        /// 获取或设置样车编号。
        /// </summary>
        [Index("UK_ASM_AssembleIndication", 3, IsUnique = true)]
        [Required]
        [MaxLength(100)]
        public string ProductSequence { get; set; }

        /// <summary>
        /// 获取或设置批次，20181109。
        /// </summary>
        [MaxLength(100)]
        public string PlanBatch { get; set; }

        /// <summary>
        /// 获取或设置批次，1 或 2。
        /// </summary>
        [MaxLength(100)]
        public string CarBatch { get; set; }

        /// <summary>
        /// 获取或设置车身到达时间。
        /// </summary>
        [Index]
        public DateTime CarArrivedTime { get; set; }

        /// <summary>
        /// 获取或设置装配指示状态。
        /// </summary>
        [Index]
        public AssembleStatus AssembleStatus { get; set; }

        /// <summary>
        /// 获取或设置工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置关联的通讯消息。
        /// </summary>
        public virtual ASM_AssembleIndicationMessage ASM_AssembleIndicationMessage { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_AssembleIndicationItem> ASM_AssembleIndicationItems { get; set; }

        /// <summary>
        /// 获取关联的拣料结果，0 或 1 个元素。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_AssembleResult> ASM_AssembleResults { get; set; }

        /// <summary>
        /// 获取关联的指引任务，1 个元素。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_Task> ASM_Tasks { get; set; }
    }
}