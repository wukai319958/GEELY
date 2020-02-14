using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataAccess.Other
{
    /// <summary>
    /// 投料区,即分装总成缓存区
    /// </summary>
    public class FeedZone
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string XGateIP { get; set; }
        [Required]
        public int Bus { get; set; }
        [Required]
        public int Address { get; set; }
        /// <summary>
        /// RFID码，即地堆码
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string RFID { get; set; }
        /// <summary>
        /// MES中维护的地堆码,这里直接与RFID一样，目前MES没有维护这数据
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string GroundId { get; set; }
        /// <summary>
        /// MES中维护的区域码
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string AreaId { get; set; }
        /// <summary>
        /// 总成件条码，可以为null
        /// </summary>
        [MaxLength(64)]
        public string MaterialId { get; set; }
        /// <summary>
        /// 是否区域灯
        /// </summary>
        public bool IsM3 { get; set; }
        /// <summary>
        /// 是否交互灯
        /// </summary>
        public bool IsInteractive { get; set; }
        /// <summary>
        /// 状态，0灭灯，1点灯
        /// </summary>
        [Required]
        public int Status { get; set; }
    }
}
