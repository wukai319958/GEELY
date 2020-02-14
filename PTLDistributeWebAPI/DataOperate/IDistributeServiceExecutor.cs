using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTLDistributeWebAPI.DataOperate
{
    public interface IDistributeServiceExecutor
    {
        /// <summary>
        /// 获取巷道信息
        /// </summary>
        /// <returns></returns>
        string GetChannelInfo();

        /// <summary>
        /// 生成拣料区铺线任务
        /// </summary>
        /// <param name="InitChannelIds">拣料区铺线的巷道ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GeneratePickAreaInitTask(string InitChannelIds, bool IsPTLPick);

        /// <summary>
        /// 生成PDA拣料区配送任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="sWorkStationCode">工位编码</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GenerateHandPickAreaDistributeTask(string sCartRFID, string sWorkStationCode, bool IsPTLPick);

        /// <summary>
        /// 生成料架绑定或解绑任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="sPonitCode">储位编号</param>
        /// <param name="IsBind">是否绑定</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="sAreaFlag">区域标识（0：料架缓冲区，1：拣料区，2：物料超市，3：生产线边）</param>
        /// <param name="sAreaCode">区域编码</param>
        /// <param name="sPosition">区域车位</param>
        /// <returns></returns>
        string GenerateBindOrUnBindTask(string sCartRFID, string sPonitCode, bool IsBind, bool IsPTLPick, string sAreaFlag, string sAreaCode, string sPosition);

        /// <summary>
        /// 获取当前储位信息
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <returns></returns>
        string GetCurrentPointInfoByCartRFID(string sCartRFID);

        /// <summary>
        /// 生成PDA物料超市配送任务
        /// </summary>
        /// <param name="sWorkStationCode">工位</param>
        /// <param name="sPointCode">储位编号</param>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GenerateMarketDistributeTask(string sWorkStationCode, string sPointCode, string sCartRFID, bool IsPTLPick);

        /// <summary>
        /// 生成单个线边里侧到外侧任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GenerateSingleProductInToOutTask(string sCartRFID, bool IsPTLPick);

        /// <summary>
        /// 生成单个空料架返回任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GenerateSingleNullCartBackTask(string sCartRFID, bool IsPTLPick);

        /// <summary>
        /// 获取工位信息
        /// </summary>
        /// <returns></returns>
        string GetWorkStationInfo();

        /// <summary>
        /// 生成生产线边铺线任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边铺线的工位ID</param>
        /// <returns></returns>
        string GenerateProductAreaInitTask(string InitWorkStationIds);

        /// <summary>
        /// 获取工位信息
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <returns></returns>
        string GetWorkStationInfoByCartRFID(string sCartRFID);

        /// <summary>
        /// 获取可以清线的线边工位信息
        /// </summary>
        /// <returns></returns>
        string GetProductAreaClearWorkStationInfo();

        /// <summary>
        /// 获取可以进行空料架返回的工位信息
        /// </summary>
        /// <returns></returns>
        string GetNullCartBackWorkStationInfo();

        /// <summary>
        /// 批量生成线边里侧到外侧任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边清线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GenerateBatchProductInToOutTask(string InitWorkStationIds, bool IsPTLPick);

        /// <summary>
        /// 批量生成空料架返回任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边清线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GenerateBatchNullCartBackTask(string InitWorkStationIds, bool IsPTLPick);

        /// <summary>
        /// 生成单个拣料区铺线任务
        /// </summary>
        /// <param name="sChannelCode">巷道编码</param>
        /// <param name="sPosition">车位</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GenerateSinglePickAreaInitTask(string sChannelCode, string sPosition, bool IsPTLPick);

        /// <summary>
        /// 手动生成料架转换任务
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        string GenerateHandProductCartSwitchTask(string sCartRFID, bool IsPTLPick);

        /// <summary>
        /// 获取巷道车位信息
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <returns></returns>
        string GetChannelPositionInfoByCartRFID(string sCartRFID);

        /// <summary>
        /// 获取工位车位信息
        /// </summary>
        /// <param name="sCartRFID">料架RFID</param>
        /// <returns></returns>
        string GetWorkStationPositionInfoByCartRFID(string sCartRFID);

        /// <summary>
        /// 批量生成物料超市配送任务
        /// </summary>
        /// <param name="InitWorkStationIds">线边铺线的工位ID</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <returns></returns>
        string GenerateBatchMarketDistributeTask(string InitWorkStationIds, bool IsPTLPick);

        /// <summary>
        /// 获取物料超市料架信息
        /// </summary>
        /// <param name="sWorkStationID">工位ID</param>
        /// <returns></returns>
        string GetMarketCartByWorkStationID(string sWorkStationID);

        /// <summary>
        /// 删除物料超市料架信息
        /// </summary>
        /// <param name="sDelID">物料超市记录ID</param>
        /// <returns></returns>
        string DeleteMarketCart(string sDelID);

         /// <summary>
        /// 获取配送任务
        /// </summary>
        /// <param name="MinTime">任务生成开始时间</param>
        /// <param name="MaxTime">任务生成结束时间</param>
        /// <param name="ReqType">配送任务类型</param>
        /// <param name="Response">是否响应</param>
        /// <param name="Arrive">是否到达</param>
        /// <returns></returns>
        string GetDistributeTask(string MinTime, string MaxTime, string ReqType, string Response, string Arrive);

        /// <summary>
        /// 配送任务重发
        /// </summary>
        /// <param name="DistributeTaskIds">配送任务ID</param>
        /// <returns></returns>
        string ReSendDistributeTask(string DistributeTaskIds);

        /// <summary>
        /// 结束配送任务
        /// </summary>
        /// <param name="DistributeTaskIds">配送任务ID</param>
        /// <returns></returns>
        string StopDistributeTask(string DistributeTaskIds);

        /// <summary>
        /// 生成点对点配送任务
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <param name="StartPoint">起始储位</param>
        /// <param name="EndPoint">目标储位</param>
        /// <param name="IsPTLPick">是否PTL拣料</param>
        /// <param name="AreaFlag">区域标识（0：料架缓冲区，1：物料超市，2：其他区域）</param>
        /// <returns></returns>
        string GeneratePointToPointDistribute(string CartRFID, string StartPoint, string EndPoint, bool IsPTLPick, string AreaFlag);

        /// <summary>
        /// 获取PTL配送工位
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        string GetPTLWorkStationByCart(string CartRFID);

        /// <summary>
        /// 更新巷道料架停靠时间
        /// </summary>
        /// <returns></returns>
        string UpdateChannelCurrentCartDockedTime();

        /// <summary>
        /// 获取料架信息
        /// </summary>
        /// <param name="CartRFID">料架RFID</param>
        /// <returns></returns>
        string GetCartInfoByRFID(string CartRFID);
    }
}
