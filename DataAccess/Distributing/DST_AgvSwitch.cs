using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Distributing
{
    public class DST_AgvSwitch
    {
        /// <summary>
        /// AGV开关信息
        /// </summary>
        public DST_AgvSwitch()
        {
            
        }

        /// <summary>
        /// 获取或设置自增主键
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool isOpen { get; set; }

        /// <summary>
        /// 开启时间
        /// </summary>
        public DateTime? lastOpenTime { get; set; }

        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? lastCloseTime { get; set; }
    }
}
