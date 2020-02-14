using System;
using System.ComponentModel.DataAnnotations;
using DataAccess.Config;

namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 装配结果的明细。
    /// </summary>
    public class ASM_AssembleResultItem
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所属结果的外键。
        /// </summary>
        public long ASM_AssembleResultId { get; set; }

        /// <summary>
        /// 获取或设置料车的外键。
        /// </summary>
        public int CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置料车库位。
        /// </summary>
        public int CartPosition { get; set; }

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
        /// 获取或设置应装配数量。
        /// </summary>
        public int ToAssembleQuantity { get; set; }

        /// <summary>
        /// 获取或设置实际装配数量。
        /// </summary>
        public int AssembledQuantity { get; set; }

        /// <summary>
        /// 获取或设置取料时间。
        /// </summary>
        public DateTime PickedTime { get; set; }

        /// <summary>
        /// 获取或设置项目编号。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置阶段。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置所属结果。
        /// </summary>
        public virtual ASM_AssembleResult ASM_AssembleResult { get; set; }

        /// <summary>
        /// 获取或设置料车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }
    }
}