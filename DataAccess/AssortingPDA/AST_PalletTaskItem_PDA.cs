using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Config;
using DataAccess.Assorting;

namespace DataAccess.AssortingPDA
{
    /// <summary>
    /// 按托合并后的分拣任务的明细。
    /// </summary>
    public class AST_PalletTaskItem_PDA
    {
        /// <summary>
        /// 初始化按托合并后的分拣任务的明细。
        /// </summary>
        public AST_PalletTaskItem_PDA()
        {
            this.AST_LesTaskItem_PDAs = new HashSet<AST_LesTaskItem_PDA>();
            //this.AST_CartTaskItem_PDAs = new HashSet<AST_CartTaskItem_PDA>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属任务的外键。
        /// </summary>
        public long AST_PalletTaskId { get; set; }

        /// <summary>
        /// 获取或设置目标工位的外键。
        /// </summary>
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置料箱条码。
        /// </summary>
        [MaxLength(100)]
        public string BoxCode { get; set; }

        /// <summary>
        /// 获取或设置源托盘的库位。
        /// </summary>
        public int FromPalletPosition { get; set; }

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
        /// 获取或设置条码。
        /// </summary>
        [MaxLength(100)]
        public string MaterialBarcode { get; set; }

        /// <summary>
        /// 获取或设置应拣数量。
        /// </summary>
        public int ToPickQuantity { get; set; }

        /// <summary>
        /// 获取或设置小车单个库位能存放此种物料的最大数量。
        /// </summary>
        [Column("MaxQuantitySinglePosition")]
        public int MaxQuantityInSingleCartPosition { get; set; }

        /// <summary>
        /// 获取或设置是否特殊件，通用件需合并应拣数量，特殊件不合并。
        /// </summary>
        public bool IsSpecial { get; set; }

        /// <summary>
        /// 获取或设置是否大件，大件需要占用同层的两个储位。
        /// </summary>
        public bool IsBig { get; set; }

        /// <summary>
        /// 获取或设置分拣状态。
        /// </summary>
        [Index]
        public PickStatus PickStatus { get; set; }

        /// <summary>
        /// 获取或设置实拣数量。
        /// </summary>
        public int? PickedQuantity { get; set; }

        /// <summary>
        /// 获取或设置最后拣选时间。
        /// </summary>
        public DateTime? PickedTime { get; set; }

        /// <summary>
        /// 获取或设置操作员的外键。
        /// </summary>
        public int? CFG_EmployeeId { get; set; }

        /// <summary>
        /// 获取或设置所属任务。
        /// </summary>
        public virtual AST_PalletTask_PDA AST_PalletTask_PDA { get; set; }

        /// <summary>
        /// 获取或设置目标工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置操作员。
        /// </summary>
        public virtual CFG_Employee CFG_Employee { get; set; }

        /// <summary>
        /// 获取原始明细的集合。
        /// </summary>
        //[SoapIgnore]
        //[XmlIgnore]
        public virtual ICollection<AST_LesTaskItem_PDA> AST_LesTaskItem_PDAs { get; set; }

        ///// <summary>
        ///// 获取单车明细的集合。
        ///// </summary>
        //[SoapIgnore]
        //[XmlIgnore]
        //public virtual ICollection<AST_CartTaskItem_PDA> AST_CartTaskItem_PDAs { get; set; }
    }
}