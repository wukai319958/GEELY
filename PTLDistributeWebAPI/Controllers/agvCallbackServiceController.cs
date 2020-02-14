using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTLDistributeWebAPI.DataOperate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PTLDistributeWebAPI.Controllers
{
    public class agvCallbackServiceController : ApiController
    {
        /// <summary>
        /// AGV配送到达回调方法
        /// </summary>
        /// <param name="reqInfo">配送到达信息</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult agvCallback([FromBody] JObject reqInfo)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeArriveExecutor service = new DistributeArriveExecutor();
            string result = service.DistributeArriveHandle(reqInfo);

            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult Test()
        {
            
            if (!ModelState.IsValid)
                return BadRequest();

            //string result = "Success";
            string result = new DistributeArriveExecutor().Test();
            return Ok(result);
        }
    }
}
