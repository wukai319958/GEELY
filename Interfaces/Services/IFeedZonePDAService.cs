using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Interfaces.Services
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IFeedZonePDAService”。
    [ServiceContract]
    public interface IFeedZonePDAService
    {
        [OperationContract]
        string Bind(string materialId,string groundId);
        [OperationContract]
        List<string> QueryTypeNames();
        [OperationContract]
        string UnBind(string groundId);
        [OperationContract]
        string BindCacheRegion(string groundId, string materialId1, string materialId2, string materialId3);
        [OperationContract]
        string UnBindCacheRegion(string groundId);
    }
}
