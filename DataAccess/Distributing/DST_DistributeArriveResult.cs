using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DataAccess.Distributing
{
    /// <summary>
    /// 配送任务到达结果明细
    /// </summary>
    public class DST_DistributeArriveResult
    {
        /// <summary>
        /// 初始化配送任务到达结果明细
        /// </summary>
        public DST_DistributeArriveResult()
        {
            
        }

        /// <summary>
        /// 获取或设置自增主键
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 配送任务编号
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Index]
        public string reqCode { get; set; }

        /// <summary>
        /// 到达时间
        /// </summary>
        public DateTime arriveTime { get; set; }

        /// <summary>
        /// 起始位置
        /// </summary>
        [MaxLength(100)]
        public string startPosition { get; set; }

        /// <summary>
        /// 目标位置
        /// </summary>
        [MaxLength(100)]
        public string endPosition { get; set; }

        /// <summary>
        /// 料架编号
        /// </summary>
        [MaxLength(100)]
        public string podCode { get; set; }

        /// <summary>
        /// 当前储位编号
        /// </summary>
        public string currentPointCode { get; set; }

        /// <summary>
        /// 配送任务类型
        /// </summary>
        public DistributeReqTypes DistributeReqTypes { get; set; }
    }
}
