using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLDistributeWebAPI.Models
{
    public class CartDto
    {
        /// <summary>
        /// 获取或设置料架编码。
        /// </summary>
        public string CartCode { get; set; }

        /// <summary>
        /// 获取或设置料架状态。
        /// </summary>
        public string CartStatus { get; set; }

        /// <summary>
        /// 获取或设置料架在线信息。
        /// </summary>
        public string OnlineInfo { get; set; }
    }
}