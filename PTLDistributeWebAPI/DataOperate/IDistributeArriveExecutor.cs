using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTLDistributeWebAPI.DataOperate
{
    public interface IDistributeArriveExecutor
    {
        /// <summary>
        /// 配送到达任务处理
        /// </summary>
        /// <param name="reqInfo">接收的数据</param>
        /// <returns></returns>
        string DistributeArriveHandle(JObject reqInfo);
    }
}
