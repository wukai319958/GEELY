using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Config
{
    /// <summary>
    /// 分拣转台上的 PTL 设备。
    /// </summary>
    public class CFG_ChannelPtlDevice
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置所属巷道的外键。
        /// </summary>
        [Index("UK_CFG_ChannelPtlDevice", 1, IsUnique = true)]
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置库位。
        /// </summary>
        [Index("UK_CFG_ChannelPtlDevice", 2, IsUnique = true)]
        public int Position { get; set; }

        /// <summary>
        /// 获取或设置 XGate 的 IP 地址。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string XGateIP { get; set; }

        /// <summary>
        /// 获取或设置总线索引。
        /// </summary>
        public byte RS485BusIndex { get; set; }

        /// <summary>
        /// 获取或设置标签地址。
        /// </summary>
        public byte Ptl900UAddress { get; set; }

        /// <summary>
        /// 获取或设置是否在线。
        /// </summary>
        public bool OnLine { get; set; }

        /// <summary>
        /// 获取或设置所属巷道。
        /// </summary>
        public virtual CFG_Channel CFG_Channel { get; set; }
    }
}