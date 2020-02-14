namespace DataAccess.Config
{
    /// <summary>
    /// 托盘旋转状态。
    /// </summary>
    public enum PalletRotationStatus
    {
        /// <summary>
        /// 正常。
        /// </summary>
        Normal,

        /// <summary>
        /// 开始旋转。
        /// </summary>
        BeginRotation,

        /// <summary>
        /// 已反向。
        /// </summary>
        Reversed,

        /// <summary>
        /// 开始反向旋转。
        /// </summary>
        BeginReverseRotation,
    }
}