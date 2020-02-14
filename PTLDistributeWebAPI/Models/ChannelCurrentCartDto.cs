using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLDistributeWebAPI.Models
{
    public class ChannelCurrentCartDto
    {
        /// <summary>
        /// 巷道编码
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 车位
        /// </summary>
        public string Position { get; set; }
    }
}