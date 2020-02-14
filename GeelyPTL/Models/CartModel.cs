using DataAccess.Config;

namespace GeelyPTL.Models
{
    /// <summary>
    /// 料车模型。
    /// </summary>
    public class CartModel
    {
        /// <summary>
        /// 获取或设置关联的料车。
        /// </summary>
        public CFG_Cart CFG_Cart { get; set; }

        /// <summary>
        /// 获取或设置库位一指示灯是否在线。
        /// </summary>
        public bool Light1OnLine { get; set; }

        /// <summary>
        /// 获取或设置库位二指示灯是否在线。
        /// </summary>
        public bool Light2OnLine { get; set; }

        /// <summary>
        /// 获取或设置库位三指示灯是否在线。
        /// </summary>
        public bool Light3OnLine { get; set; }

        /// <summary>
        /// 获取或设置库位四指示灯是否在线。
        /// </summary>
        public bool Light4OnLine { get; set; }

        /// <summary>
        /// 获取或设置库位五指示灯是否在线。
        /// </summary>
        public bool Light5OnLine { get; set; }

        /// <summary>
        /// 获取或设置库位六指示灯是否在线。
        /// </summary>
        public bool Light6OnLine { get; set; }

        /// <summary>
        /// 获取或设置库位七指示灯是否在线。
        /// </summary>
        public bool Light7OnLine { get; set; }

        /// <summary>
        /// 获取或设置库位八指示灯是否在线。
        /// </summary>
        public bool Light8OnLine { get; set; }

        /// <summary>
        /// 获取或设置信息发布器是否在线。
        /// </summary>
        public bool PublisherOnLine { get; set; }

        /// <summary>
        /// 获取或设置车顶灯塔是否在线。
        /// </summary>
        public bool LighthouseOnLine { get; set; }
    }
}