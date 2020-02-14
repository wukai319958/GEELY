namespace DataAccess.Config
{
    /// <summary>
    /// 料车库位可用性。
    /// </summary>
    public enum CartPositionUsability
    {
        /// <summary>
        /// 可用。
        /// </summary>
        Enable,

        /// <summary>
        /// 因为 PTL 设备不在线而不可用。
        /// </summary>
        DisableByOffLineDevice,

        /// <summary>
        /// 因为储位放满而不可用。
        /// </summary>
        DisableByFull,

        /// <summary>
        /// 因为相邻储位放了大件而不可用。
        /// </summary>
        DisableByNeighborBigMaterial,
    }
}