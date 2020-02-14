using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DataAccess.Distributing
{
    /// <summary>
    /// 配送到达任务
    /// </summary>
    public class DST_DistributeArriveTask
    {
        /// <summary>
        /// 初始化配送到达任务
        /// </summary>
        public DST_DistributeArriveTask()
        {
            
        }

        /// <summary>
        /// 获取或设置自增主键
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 自定义数据
        /// </summary>
        [MaxLength(1000)]
        public string data { get; set; }

        /// <summary>
        /// 方法
        /// </summary>
        [MaxLength(100)]
        public string method { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        [MaxLength(100)]
        public string taskType { get; set; }

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
        /// AGV编号
        /// </summary>
        [MaxLength(100)]
        public string robotCode { get; set; }

        /// <summary>
        /// 任务编号
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Index]
        public string taskCode { get; set; }

        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime receiveTime { get; set; }
    }
}
