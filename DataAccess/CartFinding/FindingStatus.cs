namespace DataAccess.CartFinding
{
    /// <summary>
    /// 亮灯指示状态。
    /// </summary>
    public enum FindingStatus
    {
        /// <summary>
        /// 新任务，未点亮。
        /// </summary>
        New,

        /// <summary>
        /// 需要亮灯指示。
        /// </summary>
        NeedDisplay,

        /// <summary>
        /// 亮灯指示中。
        /// </summary>
        Displaying,

        /// <summary>
        /// 运输开始，需要慢闪。
        /// </summary>
        NeedBlink,

        /// <summary>
        /// 运输中，慢闪，一段时间后自动进入 NeedClear 状态。
        /// </summary>
        Blinking,

        /// <summary>
        /// 需要熄灭指示灯。
        /// </summary>
        NeedClear,

        /// <summary>
        /// 已完成。
        /// </summary>
        Finished,
    }
}