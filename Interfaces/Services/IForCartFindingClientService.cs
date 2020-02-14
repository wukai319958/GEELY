using System.Collections.Generic;
using System.ServiceModel;
using Interfaces.Entities;

namespace Interfaces.Services
{
    /// <summary>
    /// 为手持机提供服务。
    /// </summary>
    [ServiceContract]
    [XmlSerializerFormat]
    public interface IForCartFindingClientService
    {
        /// <summary>
        /// 查询所有操作员的登录名。
        /// </summary>
        [OperationContract]
        List<string> QueryCfgEmployeeLoginNames();

        /// <summary>
        /// 查询所有的巷道。
        /// </summary>
        [OperationContract]
        List<CFG_ChannelDto> QueryCfgChannels();

        /// <summary>
        /// 查询所有的工位。
        /// </summary>
        [OperationContract]
        List<CFG_WorkStationDto> QueryCfgWorkStations();

        /// <summary>
        /// 按编码或 RFID 标签查询料车。
        /// </summary>
        /// <param name="cfgCartCodeOrRfid">料车编码或 RFID 标签。</param>
        [OperationContract]
        CFG_CartDto QueryCfgCart(string cfgCartCodeOrRfid);

        /// <summary>
        /// 登录。
        /// </summary>
        /// <param name="loginName">用户名。</param>
        /// <param name="password">密码。</param>
        [OperationContract]
        LoginResult Login(string loginName, string password);

        /// <summary>
        /// 查询料车配送任务。
        /// </summary>
        /// <param name="cfgWorkStationIds">按工位过滤的工位主键集合。</param>
        [OperationContract]
        List<FND_TaskDto> QueryFndTasks(List<int> cfgWorkStationIds);

        /// <summary>
        /// 点亮需配送的料车。
        /// </summary>
        /// <param name="fndTaskId">料车配送任务的主键。</param>
        /// <param name="lightColor">灯色。</param>
        /// <param name="cfgEmployeeId">操作员的主键。</param>
        [OperationContract]
        void FindCart(long fndTaskId, byte lightColor, int cfgEmployeeId);

        /// <summary>
        /// 发出需配送的料车。
        /// </summary>
        /// <param name="fndTaskId">料车配送任务的主键。</param>
        [OperationContract]
        void DepartCart(long fndTaskId);

        /// <summary>
        /// 停靠料车到巷道。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        /// <param name="cfgCartCodeOrRfid">料车编码或 RFID 标签。</param>
        [OperationContract]
        void DockCartToChannel(int cfgChannelId, string cfgCartCodeOrRfid);

        /// <summary>
        /// 从巷道解除料车的停靠。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        /// <param name="cfgCartCodeOrRfid">料车编码或 RFID 标签。</param>
        [OperationContract]
        void UnDockCartFromChannel(int cfgChannelId, string cfgCartCodeOrRfid);

        /// <summary>
        /// 绑定料车的 RFID 标签。
        /// </summary>
        /// <param name="cfgCartCode">料车编码。</param>
        /// <param name="rfid1">RFID 标签一。</param>
        /// <param name="rfid2">RFID 标签二。</param>
        [OperationContract]
        void BindCartRfid(string cfgCartCode, string rfid1, string rfid2);

        /// <summary>
        /// 查询当前任务。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        [OperationContract]
        AndroidPdaTaskInfo QueryCurrentTaskInfo(int cfgChannelId);

        /// <summary>
        /// 尝试引发 900U 分拣事件。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        /// <param name="cartPosition">料车储位。</param>
        [OperationContract]
        void TryRaisePtl900UAssortingPressed(int cfgChannelId, int cartPosition);

        /// <summary>
        /// 尝试引发显示屏确认事件。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        [OperationContract]
        void TryRaisePtlPublisherAssortingPressed(int cfgChannelId);

        /// <summary>
        /// 测试用方法-清空料车，用于拣选数量大于装配数量时。
        /// </summary>
        [OperationContract]
        void TestMethod_ClearCart(string cfgCartCodeOrRfid);

        /// <summary>
        /// 控制小车停靠显示
        /// </summary>
        /// <param name="nCartID"></param>
        /// <param name="sName"></param>
        /// <param name="sDescription"></param>
        /// <param name="nCount"></param>
        /// <param name="sUnit"></param>
        /// <returns></returns>
        [OperationContract]
        string DockCart(int nCartID, string sName, string sDescription, int nCount, string sUnit);

        [OperationContract]
        string UnDockCart(int nCartID);
    }
}