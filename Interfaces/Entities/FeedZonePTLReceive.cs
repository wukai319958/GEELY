using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces.Entities
{
    public class FeedZonePTLReceive
    {
        /// <summary>
        /// 工厂
        /// </summary>
        public string FCCODE { get; set; }
        /// <summary>
        /// 流水线
        /// </summary>
        public string PLCODE { get; set; }
        /// <summary>
        /// 试制工位
        /// </summary>
        public string STCODE { get; set; }
        /// <summary>
        /// 量产工位列表
        /// </summary>
        public string GZZLIST { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string MONUM { get; set; }
        /// <summary>
        /// 样车编号
        /// </summary>
        public string PRDSEQ { get; set; }
        /// <summary>
        /// 项目
        /// </summary>
        public string PRJCODE { get; set; }
        /// <summary>
        /// 阶段
        /// </summary>
        public string PRJSTEP { get; set; }
        /// <summary>
        /// 配置
        /// </summary>
        public string PRJCFG { get; set; }
        /// <summary>
        /// 分装线代码
        /// </summary>
        public string PACKLINE { get; set; }
    }
}
