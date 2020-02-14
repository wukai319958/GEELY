using DataAccess.Config;

namespace GeelyPTL.Models
{
    /// <summary>
    /// 工位模型。
    /// </summary>
    public class WorkStationModel
    {
        /// <summary>
        /// 获取或设置关联的工位。
        /// </summary>
        public CFG_WorkStation CFG_WorkStation { get; set; }

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
        /// 获取或设置车位五上的料车名称。
        /// </summary>
        public string CurrentCartName5 { get; set; }

        /// <summary>
        /// 获取或设置车位六上的料车名称。
        /// </summary>
        public string CurrentCartName6 { get; set; }

        /// <summary>
        /// 获取或设置车位七上的料车名称。
        /// </summary>
        public string CurrentCartName7 { get; set; }

        /// <summary>
        /// 获取或设置车位八上的料车名称。
        /// </summary>
        public string CurrentCartName8 { get; set; }
    }
}