using DataAccess.Distributing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace GeelyPTL.Models
{
    /// <summary>
    /// 配送任务
    /// </summary>
    public class DistributeTaskModel : INotifyPropertyChanged
    {
        public void INotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 获取或设置自增主键
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 请求编号
        /// </summary>
        public string reqCode { get; set; }

        /// <summary>
        /// 请求时间
        /// </summary>
        public DateTime reqTime { get; set; }

        /// <summary>
        /// 客户端编号
        /// </summary>
        public string clientCode { get; set; }

        /// <summary>
        /// 令牌号
        /// </summary>
        public string tokenCode { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public string taskTyp { get; set; }

        /// <summary>
        /// 用户呼叫编号
        /// </summary>
        public string userCallCode { get; set; }

        /// <summary>
        /// 任务组编号
        /// </summary>
        public string taskGroupCode { get; set; }

        /// <summary>
        /// 起始位置
        /// </summary>
        public string startPosition { get; set; }

        /// <summary>
        /// 目标位置
        /// </summary>
        public string endPosition { get; set; }

        /// <summary>
        /// 料架编号
        /// </summary>
        public string podCode { get; set; }

        /// <summary>
        /// 料架方向
        /// </summary>
        public string podDir { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int priority { get; set; }

        /// <summary>
        /// AGV编号
        /// </summary>
        public string robotCode { get; set; }

        /// <summary>
        /// 任务单号
        /// </summary>
        public string taskCode { get; set; }

        /// <summary>
        /// 自定义字段
        /// </summary>
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

        /// <summary>
        /// 是否选择
        /// </summary>
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                INotifyPropertyChanged("IsChecked");
            }
        }
    }
}
