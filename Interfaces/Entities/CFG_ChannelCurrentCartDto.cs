using System.Runtime.Serialization;

namespace Interfaces.Entities
{
    /// <summary>
    /// 分拣口停靠的料车。
    /// </summary>
    [DataContract]
    public class CFG_ChannelCurrentCartDto
    {
        /// <summary>
        /// 获取或设置主键。
        /// </summary>
        [DataMember]
        public int CFG_ChannelCurrentCartId { get; set; }

        /// <summary>
        /// 获取或设置分拣口的主键。
        /// </summary>
        [DataMember]
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置车位。
        /// </summary>
        [DataMember]
        public int Position { get; set; }

        /// <summary>
        /// 获取或设置所停靠料车的主键。
        /// </summary>
        [DataMember]
        public int? CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置所停靠料车的编码。
        /// </summary>
        [DataMember]
        public string CartCode { get; set; }

        /// <summary>
        /// 获取或设置所停靠料车的名称。
        /// </summary>
        [DataMember]
        public string CartName { get; set; }
    }
}