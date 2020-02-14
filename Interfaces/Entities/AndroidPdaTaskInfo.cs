using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using DataAccess.Assorting;

namespace Interfaces.Entities
{
    /// <summary>
    /// 手持机上的当前分拣任务。
    /// </summary>
    [DataContract]
    public class AndroidPdaTaskInfo
    {
        /// <summary>
        /// 初始化手持机上的当前分拣任务。
        /// </summary>
        public AndroidPdaTaskInfo()
        {
            this.CartPositions = new Collection<int>();
        }

        /// <summary>
        /// 获取或设置分拣口的主键。
        /// </summary>
        [DataMember]
        public int CFG_ChannelId { get; set; }

        /// <summary>
        /// 获取或设置所作业料车的主键，为空表示不在作业。
        /// </summary>
        [DataMember]
        public int? CFG_CartId { get; set; }

        /// <summary>
        /// 获取或设置所停靠料车的名称。
        /// </summary>
        [DataMember]
        public string CartName { get; set; }

        /// <summary>
        /// 获取或设置所停靠的料车是否在线。
        /// </summary>
        [DataMember]
        public bool? CartOnLine { get; set; }

        /// <summary>
        /// 获取或设置当前分拣车位的集合。
        /// </summary>
        [DataMember]
        public Collection<int> CartPositions { get; set; }

        /// <summary>
        /// 获取或设置物料编码。
        /// </summary>
        [DataMember]
        public string MaterialCode { get; set; }

        /// <summary>
        /// 获取或设置物料名称。
        /// </summary>
        [DataMember]
        public string MaterialName { get; set; }

        /// <summary>
        /// 获取或设置实拣数量。
        /// </summary>
        [DataMember]
        public int? PickedQuantity { get; set; }

        /// <summary>
        /// 获取或设置应拣数量。
        /// </summary>
        [DataMember]
        public int? ToPickQuantity { get; set; }

        /// <summary>
        /// 获取或设置分拣状态。
        /// </summary>
        [DataMember]
        public AssortingStatus AssortingStatus { get; set; }
    }
}