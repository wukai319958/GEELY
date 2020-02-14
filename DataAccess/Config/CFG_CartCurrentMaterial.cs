using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Assorting;

namespace DataAccess.Config
{
    /// <summary>
    /// 小车上的当前物料。
    /// </summary>
    public class CFG_CartCurrentMaterial
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置小车的外键。
        /// </summary>
        [Index("UK_CFG_CartCurrentMaterial", 1, IsUnique = true)]
        public int CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置小车的库位。
        /// </summary>
        [Index("UK_CFG_CartCurrentMaterial", 2, IsUnique = true)]
        public int Position { get; set; }

        /// <summary>
        /// 获取或设置关联任务明细的外键。
        /// </summary>
        public long? AST_CartTaskItemId { get; set; }

        /// <summary>
        /// 获取或设置项目编号。
        /// </summary>
        [MaxLength(100)]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置 WBS 编号。
        /// </summary>
        [MaxLength(100)]
        public string WbsId { get; set; }

        /// <summary>
        /// 获取或设置项目阶段。
        /// </summary>
        [MaxLength(100)]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置目标工位的外键。
        /// </summary>
        public int? CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置配送批次。
        /// </summary>
        [MaxLength(100)]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置分拣巷道的外键。
        /// </summary>
        public int? CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置源托盘的外键。
        /// </summary>
        public int? CFG_PalletId { get; set; }

        /// <summary>
        /// 获取或设置料箱条码。
        /// </summary>
        [MaxLength(100)]
        public string BoxCode { get; set; }

        /// <summary>
        /// 获取或设置源托盘库位。
        /// </summary>
        public int? FromPalletPosition { get; set; }

        /// <summary>
        /// 获取或设置物料编码。
        /// </summary>
        [MaxLength(100)]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 获取或设置物料名称。
        /// </summary>
        [MaxLength(100)]
        public string MaterialName { get; set; }

        /// <summary>
        /// 获取或设置物料条码。
        /// </summary>
        [MaxLength(100)]
        public string MaterialBarcode { get; set; }

        /// <summary>
        /// 获取或设置当前存放数量。
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// 获取或设置播种时间。
        /// </summary>
        public DateTime? AssortedTime { get; set; }

        /// <summary>
        /// 获取或设置操作员的外键。
        /// </summary>
        public int? CFG_EmployeeId { get; set; }

        /// <summary>
        /// 获取或设置储位可用性。
        /// </summary>
        public CartPositionUsability Usability { get; set; }

        /// <summary>
        /// 获取或设置小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }

        /// <summary>
        /// 获取关联的任务明细。
        /// </summary>
        public virtual AST_CartTaskItem AST_CartTaskItem { get; set; }

        /// <summary>
        /// 获取或设置目标工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置分拣巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }

        /// <summary>
        /// 获取或设置源托盘。
        /// </summary>
        public virtual CFG_Pallet CFG_Pallet { get; set; }
    }
}