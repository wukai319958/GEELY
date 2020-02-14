using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Config
{
    /// <summary>
    /// 物料超市工位所停靠的小车。
    /// </summary>
    public class CFG_MarketWorkStationCurrentCart
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置所在工位的外键。
        /// </summary>
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置当前小车的外键。
        /// </summary>
        public int? CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置停靠时间。
        /// </summary>
        public DateTime? DockedTime { get; set; }

        /// <summary>
        /// 获取或设置所在工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置当前小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }
    }
}
