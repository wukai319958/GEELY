using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using DataAccess.Config;

namespace Interfaces.Entities
{
    /// <summary>
    /// 托盘任务。
    /// </summary>
    [DataContract]
    public class AST_PalletTaskDto
    {
        /// <summary>
        /// 获取或设置任务的主键。
        /// </summary>
        [DataMember]
        public long AST_PalletTaskId { get; set; }

        /// <summary>
        /// 获取或设置所属托盘的主键。
        /// </summary>
        [DataMember]
        public int CFG_PalletId { get; set; }

        /// <summary>
        /// 获取或设置托盘的编码。
        /// </summary>
        [DataMember]
        public string PalletCode { get; set; }

        /// <summary>
        /// 获取或设置托盘的类型。
        /// </summary>
        [DataMember]
        public string PalletType { get; set; }

        /// <summary>
        /// 获取或设置托盘旋转状态。
        /// </summary>
        [DataMember]
        public PalletRotationStatus PalletRotationStatus { get; set; }

        /// <summary>
        /// 获取明细的集合。
        /// </summary>
        [DataMember]
        public Collection<AST_PalletTaskItemDto> Items { get; private set; }

        /// <summary>
        /// 初始化托盘任务。
        /// </summary>
        public AST_PalletTaskDto()
        {
            this.Items = new Collection<AST_PalletTaskItemDto>();
        }
    }
}