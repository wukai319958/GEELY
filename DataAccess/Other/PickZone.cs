using DataAccess.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataAccess.Other
{
    /// <summary>
    /// 7个拣选通道的柱灯配置
    /// </summary>
    public class PickZone
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
        /// 获取或设置分拣巷道的外键。
        /// </summary>
        public int CFG_ChannelId { get; set; }
        /// <summary>
        /// 安灯状态,0灭灯，1亮灯
        /// </summary>
        public int Status { get; set; }


        /// <summary>
        /// 获取或设置分拣巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }
    }
}
