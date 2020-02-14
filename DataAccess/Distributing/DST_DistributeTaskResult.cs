using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DataAccess.Distributing
{
    /// <summary>
    /// 配送任务结果
    /// </summary>
    public class DST_DistributeTaskResult
    {
        /// <summary>
        /// 初始化配送任务结果
        /// </summary>
        public DST_DistributeTaskResult()
        {

        }

        /// <summary>
        /// 获取或设置自增主键
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 返回码
        /// </summary>
        [MaxLength(10)]
        public string code { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        [MaxLength(1000)]
        public string message { get; set; }

        /// <summary>
        /// 请求编号
        /// </summary>
        [MaxLength(50)]
        [Index]
        public string reqCode { get; set; }

        /// <summary>
        /// 自定义返回
        /// </summary>
        [MaxLength(1000)]
        public string data { get; set; }

        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime receiveTime { get; set; }
    }
}
