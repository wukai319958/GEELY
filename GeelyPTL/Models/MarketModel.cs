using DataAccess.Config;

namespace GeelyPTL.Models
{
    /// <summary>
    /// 物料超市模型。
    /// </summary>
    public class MarketModel
    {
        /// <summary>
        /// 获取或设置关联的工位。
        /// </summary>
        public string WorkStationCode { get; set; }

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
    }
}