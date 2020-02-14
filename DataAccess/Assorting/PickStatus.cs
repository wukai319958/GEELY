namespace DataAccess.Assorting
{
    /// <summary>
    /// 分拣状态。
    /// </summary>
    public enum PickStatus
    {
        /// <summary>
        /// 未开始。
        /// </summary>
        New,

        /// <summary>
        /// 分拣中。
        /// </summary>
        Picking,

        /// <summary>
        /// 已完成。
        /// </summary>
        Finished,
    }
}