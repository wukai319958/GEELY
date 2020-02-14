using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Config;

namespace DataAccess.AssortingPDA
{
    /// <summary>
    /// LES 原始分拣任务。
    /// </summary>
    public class AST_LesTask_PDA
    {
        /// <summary>
        /// 初始化 LES 原始分拣任务。
        /// </summary>
        public AST_LesTask_PDA()
        {
            this.AST_LesTaskItem_PDAs = new HashSet<AST_LesTaskItem_PDA>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置项目编号。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置项目 WBS 编号。用来记录检料类型，包含“U”就是异常，然后就开始亮灯
        /// </summary>
        [MaxLength(100)]
        public string WbsId { get; set; }

        /// <summary>
        /// 获取或设置阶段。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置拣货单 ID。
        /// </summary>
        [Index(IsUnique = true)]
        [Required]
        [MaxLength(100)]
        public string BillCode { get; set; }

        /// <summary>
        /// 获取或设置拣货单生效时间。
        /// </summary>
        public DateTime BillDate { get; set; }

        /// <summary>
        /// 获取或设置目标工位的外键。
        /// </summary>
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置量产工位的逗号分隔列表。
        /// </summary>
        [MaxLength(100)]
        public string GzzList { get; set; }

        /// <summary>
        /// 获取或设置配送批次，不同批次不能放同一小车。
        /// </summary>
        [Index]
        [Required]
        [MaxLength(100)]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置分拣巷道的外键。
        /// </summary>
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置托盘的外键。
        /// </summary>
        public int CFG_PalletId { get; set; }

        /// <summary>
        /// 获取或设置料箱条码。
        /// </summary>
        [MaxLength(100)]
        public string BoxCode { get; set; }

        /// <summary>
        /// 获取或设置源托盘位置。
        /// </summary>
        public int FromPalletPosition { get; set; }

        /// <summary>
        /// 获取或设置请求接收时间。
        /// </summary>
        [Index]
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// 获取或设置是否已按托合并任务。
        /// </summary>
        public bool TaskGenerated { get; set; }

        /// <summary>
        /// 获取或设置目标工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置分拣巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }

        /// <summary>
        /// 获取或设置托盘。
        /// </summary>
        public virtual CFG_Pallet CFG_Pallet { get; set; }

        /// <summary>
        /// 获取或设置关联的通讯消息。
        /// </summary>
        public virtual AST_LesTaskMessage_PDA AST_LesTaskMessage_PDA { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_LesTaskItem_PDA> AST_LesTaskItem_PDAs { get; set; }

        public bool IsFinished { get; set; }
    }
}