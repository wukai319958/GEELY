using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Config
{
    /// <summary>
    /// 小车上设备的状态。
    /// </summary>
    public class CFG_CartPtlDevice
    {
        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置所属小车的外键。
        /// </summary>
        [Index("UK_CFG_CartPtlDevice", 1, IsUnique = true)]
        public int CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置 PTL 设备的地址。
        /// </summary>
        [Index("UK_CFG_CartPtlDevice", 2, IsUnique = true)]
        public byte DeviceAddress { get; set; }

        /// <summary>
        /// 获取或设置是否在线。
        /// </summary>
        public bool OnLine { get; set; }

        /// <summary>
        /// 获取或设置所属小车。
        /// </summary>
        public virtual CFG_Cart CFG_Cart { get; set; }
    }
}