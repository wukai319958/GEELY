using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Config;

namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 装配指示取料结果。
    /// </summary>
    public class ASM_AssembleResult
    {
        /// <summary>
        /// 初始化装配指示取料结果。
        /// </summary>
        public ASM_AssembleResult()
        {
            this.ASM_AssembleResultItems = new HashSet<ASM_AssembleResultItem>();
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
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置量产工位的逗号分隔列表。
        /// </summary>
        [Required]
        [MaxLength(100)]
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
        [Required]
        [MaxLength(100)]
        public string ProductSequence { get; set; }

        /// <summary>
        /// 获取或设置开始装配时间。
        /// </summary>
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// 获取或设置最后装配时间。
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 获取或设置关联的指示。
        /// </summary>
        public virtual ASM_AssembleIndication ASM_AssembleIndication { get; set; }

        /// <summary>
        /// 获取或设置试制工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置关联的通讯消息。
        /// </summary>
        public virtual ASM_AssembleResultMessage ASM_AssembleResultMessage { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_AssembleResultItem> ASM_AssembleResultItems { get; set; }
    }
}