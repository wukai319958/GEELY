using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Config;

namespace DataAccess.Assorting
{
    /// <summary>
    /// 按托合并后的分拣任务。
    /// </summary>
    public class AST_PalletTask
    {
        /// <summary>
        /// 初始化按托合并后的分拣任务。
        /// </summary>
        public AST_PalletTask()
        {
            this.AST_PalletTaskItems = new HashSet<AST_PalletTaskItem>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置托盘的外键。
        /// </summary>
        //[Index("UK_AST_PalletTask", 1, IsUnique = true)]
        [Index]
        public int CFG_PalletId { get; set; }

        /// <summary>
        /// 获取或设置配送批次，不同批次不能放同一小车。
        /// </summary>
        //[Index("UK_AST_PalletTask", 2, IsUnique = true)]
        [Index]
        [Required]
        [MaxLength(100)]
        public string BatchCode { get; set; }

        /// <summary>
        /// 此次到达服务于拣选单的 ID 的逗号分隔列表。
        /// </summary>
        //[Index("UK_AST_PalletTask", 3, IsUnique = true)]
        [MaxLength(4000)]
        public string PickBillIds { get; set; }

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
        //[Index("UK_AST_PalletTask", 4, IsUnique = true)]
        [Index]
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置分拣状态。
        /// </summary>
        [Index]
        public PickStatus PickStatus { get; set; }

        /// <summary>
        /// 获取或设置任务生成时间。
        /// </summary>
        [Index]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 获取或设置托盘。
        /// </summary>
        public virtual CFG_Pallet CFG_Pallet { get; set; }

        /// <summary>
        /// 获取或设置分拣巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_PalletTaskItem> AST_PalletTaskItems { get; set; }
    }
}