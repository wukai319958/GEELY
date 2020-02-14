using System.Runtime.Serialization;

namespace Interfaces.Entities
{
    /// <summary>
    /// 登录结果。
    /// </summary>
    [DataContract]
    public class LoginResult
    {
        /// <summary>
        /// 获取或设置是否成功。
        /// </summary>
        [DataMember]
        public bool Successful { get; set; }

        /// <summary>
        /// 获取或设置成功登录的操作员的主键。
        /// </summary>
        [DataMember]
        public int? CFG_EmployeeId { get; set; }

        /// <summary>
        /// 获取或设置失败信息。
        /// </summary>
        [DataMember]
        public string FailedMessage { get; set; }
    }
}