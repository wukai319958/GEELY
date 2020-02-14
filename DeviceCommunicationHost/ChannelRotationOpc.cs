using System;

namespace DeviceCommunicationHost
{
    /// <summary>
    /// 分拣口转台的 OPC 点位状态订阅入口。
    /// </summary>
    public class ChannelRotationOpc
    {
        /// <summary>
        /// 获取巷道的主键。
        /// </summary>
        public int CFG_ChannelId { get; private set; }

        /// <summary>
        /// 初始化转台 OPC 信号订阅入口。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        public ChannelRotationOpc(int cfgChannelId)
        {
            this.CFG_ChannelId = cfgChannelId;
        }

        /// <summary>
        /// 开始旋转事件。
        /// </summary>
        public event EventHandler RotationStart;
        /// <summary>
        /// 开始反向旋转事件。
        /// </summary>
        public event EventHandler ReverseRotationStart;

        /// <summary>
        /// 引发开始旋转事件。
        /// </summary>
        protected virtual void OnRotationStart()
        {
            EventHandler handler = this.RotationStart;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// 引发开始反向旋转事件。
        /// </summary>
        protected virtual void OnReverseRotationStart()
        {
            EventHandler handler = this.ReverseRotationStart;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// 引发开始旋转事件。
        /// </summary>
        internal void PerformRotationStart()
        {
            this.OnRotationStart();
        }

        /// <summary>
        /// 引发开始反向旋转事件。
        /// </summary>
        internal void PerformReverseRotationStart()
        {
            this.OnReverseRotationStart();
        }
    }
}