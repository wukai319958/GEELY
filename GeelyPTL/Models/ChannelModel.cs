using DataAccess.Config;

namespace GeelyPTL.Models
{
    /// <summary>
    /// 分拣口模型。
    /// </summary>
    public class ChannelModel
    {
        /// <summary>
        /// 获取或设置关联的分拣口。
        /// </summary>
        public CFG_Channel CFG_Channel { get; set; }

        /// <summary>
        /// 获取或设置当前抵达的托盘编码。
        /// </summary>
        public string CurrentPalletCode { get; set; }

        /// <summary>
        /// 获取或设置车位一上的料车名称。
        /// </summary>
        public string CurrentCartName1 { get; set; }

        /// <summary>
        /// 获取或设置车位二上的料车名称。
        /// </summary>
        public string CurrentCartName2 { get; set; }

        /// <summary>
        /// 获取或设置车位三上的料车名称。
        /// </summary>
        public string CurrentCartName3 { get; set; }

        /// <summary>
        /// 获取或设置车位四上的料车名称。
        /// </summary>
        public string CurrentCartName4 { get; set; }

        /// <summary>
        /// 获取或设置一号分拣指示灯的在线状态。
        /// </summary>
        public bool Light1OnLine { get; set; }

        /// <summary>
        /// 获取或设置二号分拣指示灯的在线状态。
        /// </summary>
        public bool Light2OnLine { get; set; }

        /// <summary>
        /// 获取或设置三号分拣指示灯的在线状态。
        /// </summary>
        public bool Light3OnLine { get; set; }
    }
}