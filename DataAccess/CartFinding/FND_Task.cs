using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Config;

namespace DataAccess.CartFinding
{
    /// <summary>
    /// 单车寻车任务。
    /// </summary>
    public class FND_Task
    {
        /// <summary>
        /// 初始化单车寻车任务。
        /// </summary>
        public FND_Task()
        {
            this.FND_DeliveryResults = new HashSet<FND_DeliveryResult>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public long Id { get; set; }

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
        /// 获取或设置接收时间。
        /// </summary>
        [Index]
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// 获取或设置需配送到工位的外键。
        /// </summary>
        public int CFG_WorkStationId { get; set; }

        /// <summary>
        /// 获取或设置需发送小车的外键。
        /// </summary>
        public int CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置亮灯颜色。
        /// </summary>
        public byte LightColor { get; set; }

        /// <summary>
        /// 获取或设置是否亮灯指示状态。
        /// </summary>
        [Index]
        public FindingStatus FindingStatus { get; set; }

        /// <summary>
        /// 获取或设置亮灯指示时间。
        /// </summary>
        public DateTime? DisplayTime { get; set; }

        /// <summary>
        /// 获取或设置发车时间。
        /// </summary>
        public DateTime? DepartedTime { get; set; }

        /// <summary>
        /// 获取或设置操作员的外键。
        /// </summary>
        public int? CFG_EmployeeId { get; set; }

        /// <summary>
        /// 获取或设置需配送到工位。
        /// </summary>
        public virtual CFG_WorkStation CFG_WorkStation { get; set; }

        /// <summary>
        /// 获取或设置需发送小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }

        /// <summary>
        /// 获取或设置操作员。
        /// </summary>
        public virtual CFG_Employee CFG_Employee { get; set; }

        /// <summary>
        /// 获取关联的配送结果，0 或 1 个元素。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<FND_DeliveryResult> FND_DeliveryResults { get; set; }
    }
}