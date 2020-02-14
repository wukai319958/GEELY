using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLDistributeWebAPI.Models
{
    public class WorkStationDto
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置编码。
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 获取或设置名称。
        /// </summary>
        public string Name { get; set; }
    }
}