using System;
using System.Runtime.Serialization;
using DataAccess.CartFinding;

namespace Interfaces.Entities
{
    /// <summary>
    /// 给寻车客户端用的寻车任务数据传输对象。
    /// </summary>
    [DataContract]
    public class FND_TaskDto
    {
        /// <summary>
        /// 获取或设置任务的主键。
        /// </summary>
        [DataMember]
        public long FND_TaskId { get; set; }

        /// <summary>
        /// 获取或设置项目编码。
        /// </summary>
        [DataMember]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置项目阶段。
        /// </summary>
        [DataMember]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置需配送到工位的编码。
        /// </summary>
        [DataMember]
        public string WorkStationCode { get; set; }

        /// <summary>
        /// 获取或设置需料车的编码。
        /// </summary>
        [DataMember]
        public string CartName { get; set; }

        /// <summary>
        /// 获取或设置配送批次。
        /// </summary>
        [DataMember]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置最迟抵达时间。
        /// </summary>
        [DataMember]
        public DateTime MaxNeedArrivedTime { get; set; }

        /// <summary>
        /// 获取或设置亮灯颜色。
        /// </summary>
        [DataMember]
        public byte LightColor { get; set; }

        /// <summary>
        /// 获取或设置是否亮灯指示状态。
        /// </summary>
        [DataMember]
        public FindingStatus FindingStatus { get; set; }

        /// <summary>
        /// 获取或设置亮灯指示时间。
        /// </summary>
        [DataMember]
        public DateTime? DisplayTime { get; set; }

        /// <summary>
        /// 获取或设置发车时间。
        /// </summary>
        [DataMember]
        public DateTime? DepartedTime { get; set; }
    }
}