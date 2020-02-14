using System.Runtime.Serialization;

namespace Interfaces.Entities
{
    /// <summary>
    /// 料车。
    /// </summary>
    [DataContract]
    public class CFG_CartDto
    {
        /// <summary>
        /// 获取或设置主键。
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置编码。
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        /// 获取或设置名称。
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}