using PTLDistributeWebAPI.DataOperate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PTLDistributeWebAPI.Controllers
{
    public class agvDistributeServiceController : ApiController
    {
        /// <summary>
        /// 获取巷道信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetChannelInfo()
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetChannelInfo();

            return Ok(result);
        }

        /// <summary>
        /// 生成拣料区铺线任务
        /// </summary>
        /// <param name="InitChannelIds">拣料区铺线的巷道ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GeneratePickAreaInitTask(string InitChannelIds, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GeneratePickAreaInitTask(InitChannelIds, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 生成PDA拣料区配送任务
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="WorkStationCode">工位编码</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateHandPickAreaDistributeTask(string CartRFID, string WorkStationCode, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateHandPickAreaDistributeTask(CartRFID, WorkStationCode, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 生成料架绑定或解绑任务
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="PonitCode">储位编号</param>
        /// <param name="IsBind">是否绑定</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="AreaFlag">区域标识（0：料架缓冲区，1：拣料区，2：物料超市，3：生产线边）</param>
        /// <param name="AreaCode">区域编码</param>
        /// <param name="Position">区域车位</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateBindOrUnBindTask(string CartRFID, string PonitCode, bool IsBind, bool IsPTLPick, string AreaFlag, string AreaCode, string Position)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateBindOrUnBindTask(CartRFID, PonitCode, IsBind, IsPTLPick, AreaFlag, AreaCode, Position);

            return Ok(result);
        }

        /// <summary>
        /// 获取储位信息
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetCurrentPointInfoByCartRFID(string CartRFID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetCurrentPointInfoByCartRFID(CartRFID);

            return Ok(result);
        }

        /// <summary>
        /// 生成PDA物料超市配送任务
        /// </summary>
        /// <param name="WorkStationCode">工位</param>
        /// <param name="PointCode">储位编号</param>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateMarketDistributeTask(string WorkStationCode, string PointCode, string CartRFID, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateMarketDistributeTask(WorkStationCode, PointCode, CartRFID, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 生成单个线边里侧到外侧任务
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateSingleProductInToOutTask(string CartRFID, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateSingleProductInToOutTask(CartRFID, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 生成单个空料架返回任务
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateSingleNullCartBackTask(string CartRFID, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateSingleNullCartBackTask(CartRFID, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 获取工位信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetWorkStationInfo()
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetWorkStationInfo();

            return Ok(result);
        }

        /// <summary>
        /// 生成生产线边铺线任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边铺线的工位ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateProductAreaInitTask(string InitWorkStationIds)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateProductAreaInitTask(InitWorkStationIds);

            return Ok(result);
        }

        /// <summary>
        /// 获取工位信息
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetWorkStationInfoByCartRFID(string CartRFID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetWorkStationInfoByCartRFID(CartRFID);

            return Ok(result);
        }

        /// <summary>
        /// 获取可以清线的线边工位信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetProductAreaClearWorkStationInfo()
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetProductAreaClearWorkStationInfo();

            return Ok(result);
        }

        /// <summary>
        /// 获取可以进行空料架返回的工位信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetNullCartBackWorkStationInfo()
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetNullCartBackWorkStationInfo();

            return Ok(result);
        }

        /// <summary>
        /// 批量生成线边里侧到外侧任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边清线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateBatchProductInToOutTask(string InitWorkStationIds, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateBatchProductInToOutTask(InitWorkStationIds, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 批量生成空料架返回任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边清线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateBatchNullCartBackTask(string InitWorkStationIds, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateBatchNullCartBackTask(InitWorkStationIds, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 生成单个拣料区铺线任务
        /// </summary>
        /// <param name="ChannelCode">巷道编码</param>
        /// <param name="Position">车位</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateSinglePickAreaInitTask(string ChannelCode, string Position, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateSinglePickAreaInitTask(ChannelCode, Position, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 手动生成料架转换任务
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        [HttpGet]
        public IHttpActionResult GenerateHandProductCartSwitchTask(string CartRFID, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateHandProductCartSwitchTask(CartRFID, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 获取巷道车位信息
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetChannelPositionInfoByCartRFID(string CartRFID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetChannelPositionInfoByCartRFID(CartRFID);

            return Ok(result);
        }

        /// <summary>
        /// 获取工位车位信息
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetWorkStationPositionInfoByCartRFID(string CartRFID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetWorkStationPositionInfoByCartRFID(CartRFID);

            return Ok(result);
        }

        /// <summary>
        /// 批量生成物料超市配送任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边铺线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GenerateBatchMarketDistributeTask(string InitWorkStationIds, bool IsPTLPick)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GenerateBatchMarketDistributeTask(InitWorkStationIds, IsPTLPick);

            return Ok(result);
        }

        /// <summary>
        /// 获取物料超市料架信息
        /// </summary>
        /// <param name="WorkStationID">工位ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetMarketCartByWorkStationID(string WorkStationID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetMarketCartByWorkStationID(WorkStationID);

            return Ok(result);
        }

        /// <summary>
        /// 删除物料超市料架信息
        /// </summary>
        /// <param name="DelID">物料超市记录ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult DeleteMarketCart(string DelID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.DeleteMarketCart(DelID);

            return Ok(result);
        }

        /// <summary>
        /// 获取配送任务
        /// </summary>
        /// <param name="MinTime">任务生成开始时间</param>
        /// <param name="MaxTime">任务生成结束时间</param>
        /// <param name="ReqType">配送任务类型</param>
        /// <param name="Response">是否响应</param>
        /// <param name="Arrive">是否到达</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetDistributeTask(string MinTime, string MaxTime, string ReqType, string Response, string Arrive)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetDistributeTask(MinTime, MaxTime, ReqType, Response, Arrive);

            return Ok(result);
        }

        /// <summary>
        /// 配送任务重发
        /// </summary>
        /// <param name="DistributeTaskIds">配送任务ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult ReSendDistributeTask(string DistributeTaskIds)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.ReSendDistributeTask(DistributeTaskIds);

            return Ok(result);
        }

        /// <summary>
        /// 结束配送任务
        /// </summary>
        /// <param name="DistributeTaskIds">配送任务ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult StopDistributeTask(string DistributeTaskIds)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.StopDistributeTask(DistributeTaskIds);

            return Ok(result);
        }

        /// <summary>
        /// 生成点对点配送任务
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="StartPoint">起始储位</param>
        /// <param name="EndPoint">目标储位</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="AreaFlag">区域标识（0：料架缓冲区，1：物料超市，2：其他区域）</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GeneratePointToPointDistribute(string CartRFID, string StartPoint, string EndPoint, bool IsPTLPick, string AreaFlag)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GeneratePointToPointDistribute(CartRFID, StartPoint, EndPoint, IsPTLPick, AreaFlag);

            return Ok(result);
        }

        /// <summary>
        /// 获取PTL配送工位
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetPTLWorkStationByCart(string CartRFID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetPTLWorkStationByCart(CartRFID);

            return Ok(result);
        }

        /// <summary>
        /// 更新巷道料架停靠时间
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult UpdateChannelCurrentCartDockedTime()
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.UpdateChannelCurrentCartDockedTime();

            return Ok(result);
        }

        /// <summary>
        /// 获取料架信息
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetCartInfoByRFID(string CartRFID)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            IDistributeServiceExecutor service = new DistributeServiceExecutor();
            string result = service.GetCartInfoByRFID(CartRFID);

            return Ok(result);
        }
    }
}
