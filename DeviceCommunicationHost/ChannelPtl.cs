using System.Collections.Generic;
using Ptl.Device;

namespace DeviceCommunicationHost
{
    /// <summary>
    /// 转台上 PTL 控制入口。
    /// </summary>
    public class ChannelPtl
    {
        /// <summary>
        /// 获取巷道的主键。
        /// </summary>
        public int CFG_ChannelId { get; private set; }

        readonly Dictionary<int, Ptl900U> ptl900UByPosition = new Dictionary<int, Ptl900U>();

        /// <summary>
        /// 初始化转台 PTL 控制入口。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        public ChannelPtl(int cfgChannelId)
        {
            this.CFG_ChannelId = cfgChannelId;
        }

        /// <summary>
        /// 按托盘库位设置标签。
        /// </summary>
        /// <param name="position">托盘库位。</param>
        /// <param name="ptl900U">指示标签。</param>
        internal void SetPtl900UByPosition(int position, Ptl900U ptl900U)
        {
            this.ptl900UByPosition[position] = ptl900U;
        }

        /// <summary>
        /// 按托盘库位获取标签。
        /// </summary>
        /// <param name="position">托盘库位。</param>
        /// <returns>指示标签。</returns>
        public Ptl900U GetPtl900UByPosition(int position)
        {
            return this.ptl900UByPosition[position];
        }
    }
}