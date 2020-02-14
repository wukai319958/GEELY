using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using DataAccess.Config;

namespace DataAccess.CartFinding
{
    /// <summary>
    /// 单车发车结果。
    /// </summary>
    public partial class FND_DeliveryResult
    {
        /// <summary>
        /// 初始化单车发车结果。
        /// </summary>
        public FND_DeliveryResult()
        {
            this.FND_DeliveryResultItems = new HashSet<FND_DeliveryResultItem>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 获取关联任务的外键。
        /// </summary>
        public long FND_TaskId { get; set; }

        /// <summary>
        /// 获取或设置项目编码。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectCode { get; set; }

        /// <summary>
        /// 获取或设置项目阶段。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ProjectStep { get; set; }

        /// <summary>
        /// 获取或设置目的工位的外键。
        /// </summary>
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置配送批次。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string BatchCode { get; set; }

        /// <summary>
        /// 获取或设置最迟抵达时间。
        /// </summary>
        public DateTime MaxNeedArrivedTime { get; set; }

        /// <summary>
        /// 获取或设置配送的小车的外键。
        /// </summary>
        public int CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置发车时间。
        /// </summary>
        public DateTime DepartedTime { get; set; }

        /// <summary>
        /// 获取或设置操作员的外键。
        /// </summary>
        public int CFG_EmployeeId { get; set; }

        /// <summary>
        /// 获取或设置关联的任务。
        /// </summary>
        public virtual FND_Task FND_Task { get; set; }

        /// <summary>
        /// 获取或设置目的工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置配送的小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }

        /// <summary>
        /// 获取或设置操作员。
        /// </summary>
        public virtual CFG_Employee CFG_Employee { get; set; }

        /// <summary>
        /// 获取或设置关联的通讯消息。
        /// </summary>
        public virtual FND_DeliveryResultMessage FND_DeliveryResultMessage { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<FND_DeliveryResultItem> FND_DeliveryResultItems { get; set; }
    }
}