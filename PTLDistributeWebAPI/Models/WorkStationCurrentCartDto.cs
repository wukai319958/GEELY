using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLDistributeWebAPI.Models
{
    public class WorkStationCurrentCartDto
    {
        /// <summary>
        /// 工位编码
        /// </summary>
        public string WorkStationCode { get; set; }

        /// <summary>
        /// 车位
        /// </summary>
        public string Position { get; set; }
    }
}