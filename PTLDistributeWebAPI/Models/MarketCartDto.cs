using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLDistributeWebAPI.Models
{
    public class MarketCartDto
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取或设置工位编码。
        /// </summary>
        public string WorkStationCode { get; set; }

        /// <summary>
        /// 获取或设置料架编码。
        /// </summary>
        public string CartCode { get; set; }
    }
}