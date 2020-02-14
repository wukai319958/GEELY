using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Config;

namespace DataAccess.Assorting
{
    /// <summary>
    /// 按车的播种明细，运行时生成。
    /// </summary>
    public class AST_CartTaskItem
    {
        /// <summary>
        /// 初始化按车的播种明细。
        /// </summary>
        public AST_CartTaskItem()
        {
            this.CFG_CartCurrentMaterials = new HashSet<CFG_CartCurrentMaterial>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属任务的外键。
        /// </summary>
        public long AST_CartTaskId { get; set; }

        /// <summary>
        /// 获取或设置库位。
        /// </summary>
        public int CartPosition { get; set; }

        /// <summary>
        /// 获取或设置关联的按托任务明细的外键。
        /// </summary>
        public long AST_PalletTaskItemId { get; set; }

        /// <summary>
        /// 获取或设置播种状态。
        /// </summary>
        [Index]
        public AssortingStatus AssortingStatus { get; set; }

        /// <summary>
        /// 获取或设置实播数量。
        /// </summary>
        public int? AssortedQuantity { get; set; }

        /// <summary>
        /// 获取或设置最后播种时间。
        /// </summary>
        public DateTime? AssortedTime { get; set; }

        /// <summary>
        /// 获取或设置操作员的外键。
        /// </summary>
        public int? CFG_EmployeeId { get; set; }

        /// <summary>
        /// 获取或设置所属任务。
        /// </summary>
        public virtual AST_CartTask AST_CartTask { get; set; }

        /// <summary>
        /// 获取或设置关联的按托任务明细。
        /// </summary>
        public virtual AST_PalletTaskItem AST_PalletTaskItem { get; set; }

        /// <summary>
        /// 获取或设置操作员。
        /// </summary>
        public virtual CFG_Employee CFG_Employee { get; set; }

        /// <summary>
        /// 获取小车当前物料的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_CartCurrentMaterial> CFG_CartCurrentMaterials { get; set; }
    }
}