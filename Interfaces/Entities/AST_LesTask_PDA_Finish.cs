using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interfaces.Entities
{
    /// <summary>
    /// PDA拣选完成
    /// </summary>
    public class AST_LesTask_PDA_Finish
    {
        public string ID { get; set; }
        /// <summary>
        /// 拣货ID
        /// </summary>
        public string BILLID { get; set; }
        /// <summary>
        /// 拣选状态
        /// </summary>
        public int STATUS { get; set; }
        /// <summary>
        /// 料箱编码
        /// </summary>
        public string BOXCODE { get; set; }
        /// <summary>
        /// 托盘编码
        /// </summary>
        public string PALLETCODE { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public double QUANTITY { get; set; }
        /// <summary>
        /// 批次
        /// </summary>
        public string BATCH { get; set; }
    }
}