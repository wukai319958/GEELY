using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Config;

namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 装配指引任务的明细，原始明细可能会分成多个指引明细。
    /// </summary>
    public class ASM_TaskItem
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属任务的外键。
        /// </summary>
        public long ASM_TaskId { get; set; }

        /// <summary>
        /// 获取或设置关联原始明细的外键。
        /// </summary>
        public long ASM_AssembleIndicationItemId { get; set; }

        /// <summary>
        /// 获取或设置量产工位。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Gzz { get; set; }

        /// <summary>
        /// 获取或设置小车的外键。
        /// </summary>
        public int CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置小车库位。
        /// </summary>
        public int CartPosition { get; set; }

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
        /// 获取或设置装配指示状态。
        /// </summary>
        [Index]
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
        /// 获取或设置所属任务。
        /// </summary>
        public virtual ASM_Task ASM_Task { get; set; }

        /// <summary>
        /// 获取或设置关联的原始明细。
        /// </summary>
        public virtual ASM_AssembleIndicationItem ASM_AssembleIndicationItem { get; set; }

        /// <summary>
        /// 获取或设置小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }
    }
}