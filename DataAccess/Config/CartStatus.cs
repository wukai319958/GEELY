namespace DataAccess.Config
{
    /// <summary>
    /// 小车状态。
    /// </summary>
    public enum CartStatus
    {
        /// <summary>
        /// 空闲。
        /// </summary>
        Free = 0,

        /// <summary>
        /// 已绑定巷道，等待播种。
        /// </summary>
        WaitingAssorting = 1,

        /// <summary>
        /// 播种中。
        /// </summary>
        Assorting = 2,

        /// <summary>
        /// 播种完成。
        /// </summary>
        Assorted = 3,

        /// <summary>
        /// 已点亮，等待发往缓存区。
        /// </summary>
        WaitingToBufferArea = 4,

        /// <summary>
        /// 向缓存区运输中。
        /// </summary>
        InCarriageToBufferArea = 5,

        /// <summary>
        /// 已抵达缓存区。
        /// </summary>
        ArrivedAtBufferArea = 6,

        /// <summary>
        /// 需要发往生产线，还未点亮。
        /// </summary>
        NeedToWorkStation = 7,

        /// <summary>
        /// 已点亮，等待发往生产线。
        /// </summary>
        WaitingToWorkStation = 8,

        /// <summary>
        /// 向生产线运输中。
        /// </summary>
        InCarriageToWorkStation = 9,

        /// <summary>
        /// 已抵达生产线边。
        /// </summary>
        ArrivedAtWorkStation = 10,

        /// <summary>
        /// 装配指示中。
        /// </summary>
        Indicating = 11,
    }
}