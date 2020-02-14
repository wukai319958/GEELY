using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DataAccess.Other
{
    /// <summary>
    /// 超市区
    /// </summary>
    public class MarketZone
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string XGateIP { get; set; }

        [Required]
        public int Bus { get; set; }

        [Required]
        public int Address { get; set; }

        /// <summary>
        ///地堆码
        /// </summary>
        [Required]
        public string GroundId { get; set; }

        /// <summary>
        /// 区域码：工位码
        /// </summary>
        [Required]
        public string AreaId { get; set; }

        /// <summary>
        /// 获取或设置小车停靠的位置。
        /// </summary>
        [Required]
        public int Position { get; set; }

        /// <summary>
        /// 获取或设置当前小车的外键。
        /// </summary>
        public int? CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置停靠时间。
        /// </summary>
        public DateTime? DockedTime { get; set; }

        /// <summary>
        /// 灯的状态：0灭灯、1绿灯、2黄闪
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 区域状态：0无任务、1有任务
        /// </summary>
        public int AreaStatus { get; set; }
    }
}
