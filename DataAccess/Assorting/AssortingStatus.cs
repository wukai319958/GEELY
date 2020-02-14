namespace DataAccess.Assorting
{
    /// <summary>
    /// 播种状态。
    /// </summary>
    public enum AssortingStatus
    {
        /// <summary>
        /// 无效的状态，托盘任务点亮后小车任务直接进入分拣中。
        /// </summary>
        None,

        /// <summary>
        /// 分拣中。
        /// </summary>
        Assorting,

        /// <summary>
        /// 已满，待确认，也可跳过此状态直接确认。
        /// </summary>
        WaitingConfirm,

        /// <summary>
        /// 已完成。
        /// </summary>
        Finished,
    }
}