namespace DataAccess.AssemblyIndicating
{
    /// <summary>
    /// 装配指引状态。
    /// </summary>
    public enum AssembleStatus
    {
        /// <summary>
        /// 未开始。
        /// </summary>
        New,

        /// <summary>
        /// 指引中。
        /// </summary>
        Assembling,

        /// <summary>
        /// 已完成。
        /// </summary>
        Finished,
    }
}