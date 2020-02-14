using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataAccess.Other
{
    public class CacheRegion
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string AreaId { get; set; }
        /// <summary>
        /// 缓存区小区域编号，等同于RFID编号，Mes中不维护
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string ChildAreaId { get; set; }
        [MaxLength(64)]
        public string Material_A { get; set; }
        [MaxLength(64)]
        public string Material_B { get; set; }
        [MaxLength(64)]
        public string Material_C { get; set; }
        /// <summary>
        /// 是否为交互灯
        /// </summary>
        [Required]
        public bool IsInteractive { get; set; }

        /// <summary>
        /// 是否为锁定状态
        /// </summary>
        public bool IsLocked { get; set; }
        [Required]
        public int Status { get; set; }
        [Required]
        [MaxLength(64)]
        public string XGateIP { get; set; }
        [Required]
        public int Bus { get; set; }
        [Required]
        public int Address { get; set; }


    }
}
