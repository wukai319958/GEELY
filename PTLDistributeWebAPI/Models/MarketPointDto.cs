using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLDistributeWebAPI.Models
{
    public class MarketPointDto
    {
        /// <summary>
        /// 获取或设置储位编号。
        /// </summary>
        public string PointCode { get; set; }

        /// <summary>
        /// 获取或设置工位编码。
        /// </summary>
        public string WorkStationCode { get; set; }
    }
}