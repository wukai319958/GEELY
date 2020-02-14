using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Interfaces.Entities
{
    /// <summary>
    /// 分拣看板上的当前任务信息。
    /// </summary>
    [DataContract]
    public class AssortingKanbanTaskInfo
    {
        /// <summary>
        /// 获取或设置当前批次信息。
        /// </summary>
        [DataMember]
        public AssortingKanbanCurrentBatchInfo CurrentBatchInfo { get; set; }

        /// <summary>
        /// 获取当前停靠料车的集合。
        /// </summary>
        [DataMember]
        public Collection<CFG_ChannelCurrentCartDto> CurrentCarts { get; private set; }

        /// <summary>
        /// 获取或设置当前托盘任务。
        /// </summary>
        [DataMember]
        public AST_PalletTaskDto CurrentPalletTask { get; set; }

        /// <summary>
        /// 获取或设置当前料车任务。
        /// </summary>
        [DataMember]
        public AST_CartTaskDto CurrentCartTask { get; set; }

        /// <summary>
        /// 初始化分拣看板上的当前任务信息。
        /// </summary>
        public AssortingKanbanTaskInfo()
        {
            this.CurrentBatchInfo = new AssortingKanbanCurrentBatchInfo();
            this.CurrentCarts = new Collection<CFG_ChannelCurrentCartDto>();
        }
    }
}