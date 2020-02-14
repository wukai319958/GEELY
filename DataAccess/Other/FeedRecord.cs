using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DataAccess.Other
{
    public class FeedRecord
    {
        public int Id { get; set; }

        /// <summary>
        /// 工厂
        /// </summary>
        [MaxLength(64)]
        public string FCCODE { get; set; }
        /// <summary>
        /// 流水线
        /// </summary>
        [MaxLength(64)]
        public string PLCODE { get; set; }
        /// <summary>
        /// 试制工位
        /// </summary>
        [MaxLength(64)]
        public string STCODE { get; set; }
        /// <summary>
        /// 量产工位列表
        /// </summary>
        [MaxLength(64)]
        public string GZZLIST { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        [MaxLength(64)]
        public string MONUM { get; set; }
        /// <summary>
        /// 样车编号
        /// </summary>
        [MaxLength(64)]
        public string PRDSEQ { get; set; }
        /// <summary>
        /// 项目
        /// </summary>
        [MaxLength(64)]
        public string PRJCODE { get; set; }
        /// <summary>
        /// 阶段
        /// </summary>
        [MaxLength(64)]
        public string PRJSTEP { get; set; }
        /// <summary>
        /// 配置
        /// </summary>
        [MaxLength(64)]
        public string PRJCFG { get; set; }
        /// <summary>
        /// 分装线代码
        /// </summary>
        [MaxLength(64)]
        public string PACKLINE { get; set; }
    }
}
