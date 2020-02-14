using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DataAccess.Distributing
{
    /// <summary>
    /// 配送任务
    /// </summary>
    public class DST_DistributeTask
    {
        /// <summary>
        /// 初始化配送任务
        /// </summary>
        public DST_DistributeTask()
        {

        }

        /// <summary>
        /// 获取或设置自增主键
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 请求编号
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Index]
        public string reqCode { get; set; }

        /// <summary>
        /// 请求时间
        /// </summary>
        public DateTime reqTime { get; set; }

        /// <summary>
        /// 客户端编号
        /// </summary>
        [MaxLength(100)]
        public string clientCode { get; set; }

        /// <summary>
        /// 令牌号
        /// </summary>
        [MaxLength(100)]
        public string tokenCode { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        [MaxLength(100)]
        public string taskTyp { get; set; }

        /// <summary>
        /// 用户呼叫编号
        /// </summary>
        [MaxLength(100)]
        public string userCallCode { get; set; }

        /// <summary>
        /// 任务组编号
        /// </summary>
        [MaxLength(100)]
        public string taskGroupCode { get; set; }

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
        /// 料架方向
        /// </summary>
        [MaxLength(100)]
        public string podDir { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int priority { get; set; }

        /// <summary>
        /// AGV编号
        /// </summary>
        [MaxLength(100)]
        public string robotCode { get; set; }

        /// <summary>
        /// 任务单号
        /// </summary>
        [MaxLength(100)]
        public string taskCode { get; set; }

        /// <summary>
        /// 自定义字段
        /// </summary>
        [MaxLength(1000)]
        public string data { get; set; }

        /// <summary>
        /// 配送任务类型
        /// </summary>
        public DistributeReqTypes DistributeReqTypes { get; set; }

        /// <summary>
        /// 是否响应
        /// </summary>
        public bool isResponse { get; set; }

        /// <summary>
        /// 配送到达时间
        /// </summary>
        public DateTime? arriveTime { get; set; }

        /// <summary>
        /// 发送错误次数
        /// </summary>
        public int sendErrorCount { get; set; }
    }
}
