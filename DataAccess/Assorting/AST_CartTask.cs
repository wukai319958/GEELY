using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Config;

namespace DataAccess.Assorting
{
    /// <summary>
    /// 按车的播种任务，同一辆小车只能放同一批次并且同一工位的物料。
    /// </summary>
    public class AST_CartTask
    {
        /// <summary>
        /// 初始化按车的播种任务。
        /// </summary>
        public AST_CartTask()
        {
            this.AST_CartTaskItems = new HashSet<AST_CartTaskItem>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置小车的外键。
        /// </summary>
        [Index("UK_AST_CartTask", 1, IsUnique = true)]
        public int CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置目标工位的外键。
        /// </summary>
        [Index("UK_AST_CartTask", 2, IsUnique = true)]
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置配送批次。
        /// </summary>
        [Index("UK_AST_CartTask", 3, IsUnique = true)]
        [Required]
        [MaxLength(100)]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置项目编号。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置项目 WBS 编号。
        /// </summary>
        [MaxLength(100)]
        public string WbsId { get; set; }

        /// <summary>
        /// 获取或设置项目阶段。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置分拣通道的外键。
        /// </summary>
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置播种状态。
        /// </summary>
        [Index]
        public AssortingStatus AssortingStatus { get; set; }

        /// <summary>
        /// 获取或设置任务生成时间。
        /// </summary>
        [Index]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 获取或设置小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }

        /// <summary>
        /// 获取或设置目标工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置分拣巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_CartTaskItem> AST_CartTaskItems { get; set; }
    }
}