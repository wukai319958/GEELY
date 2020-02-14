using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Interfaces.Entities
{
    /// <summary>
    /// 料车任务。
    /// </summary>
    [DataContract]
    public class AST_CartTaskDto
    {
        /// <summary>
        /// 获取或设置主键。
        /// </summary>
        [DataMember]
        public long AST_CartTaskId { get; set; }

        /// <summary>
        /// 获取或设置关联料车的主键。
        /// </summary>
        [DataMember]
        public int CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置料车编码。
        /// </summary>
        [DataMember]
        public string CartCode { get; set; }

        /// <summary>
        /// 获取或设置料车名称。
        /// </summary>
        [DataMember]
        public string CartName { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [DataMember]
        public Collection<AST_CartTaskItemDto> Items { get; private set; }

        /// <summary>
        /// 初始化料车任务。
        /// </summary>
        public AST_CartTaskDto()
        {
            this.Items = new Collection<AST_CartTaskItemDto>();
        }
    }
}