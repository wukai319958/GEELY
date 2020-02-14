using DataAccess.Config;
using DataAccess.Distributing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTLDistributeWebAPI.DataOperate
{
    public class ConvertOperate
    {
        /// <summary>
        /// 配送任务类型转换
        /// </summary>
        /// <param name="sReqText">配送任务类型</param>
        /// <returns></returns>
        public static DistributeReqTypes ReqTextToTypeConvert(string sReqText)
        {
            switch (sReqText)
            {
                case "拣料区铺线":
                    return DistributeReqTypes.PickAreaInit;
                case "生产线边铺线":
                    return DistributeReqTypes.ProductAreaInit;
                case "拣料区配送":
                    return DistributeReqTypes.PickAreaDistribute;
                case "物料超市配送":
                    return DistributeReqTypes.MaterialMarketDistribute;
                case "空料架缓冲区配送":
                    return DistributeReqTypes.NullCartAreaDistribute;
                case "生产线边料架转换":
                    return DistributeReqTypes.ProductCartSwitch;
                case "生产线边空料架返回":
                    return DistributeReqTypes.ProductNullCartBack;
                case "生产线边外侧到里侧":
                    return DistributeReqTypes.ProductOutToIn;
                case "生产线边里侧到外侧":
                    return DistributeReqTypes.ProductInToOut;
                case "绑定货架":
                    return DistributeReqTypes.BindPod;
                case "解绑货架":
                    return DistributeReqTypes.UnBindPod;
                case "点对点配送":
                    return DistributeReqTypes.PointToPointDistribute;
                default:
                    return DistributeReqTypes.PickAreaInit;

            }
        }

        /// <summary>
        /// 料架状态转换
        /// </summary>
        /// <param name="cartStatus">料架状态</param>
        /// <returns></returns>
        public static string CartStatusToTextConvert(CartStatus cartStatus)
        {
            switch (cartStatus)
            {
                case CartStatus.Free:
                    return "空闲";
                case CartStatus.WaitingAssorting:
                    return "已停靠";
                case CartStatus.Assorting:
                    return "分拣中";
                case CartStatus.Assorted:
                    return "分拣完成";
                case CartStatus.WaitingToBufferArea:
                    return "等待发往缓存区";
                case CartStatus.InCarriageToBufferArea:
                    return "向缓存区运输中";
                case CartStatus.ArrivedAtBufferArea:
                    return "缓存区";
                case CartStatus.NeedToWorkStation:
                    return "需要发往生产线";
                case CartStatus.WaitingToWorkStation:
                    return "等待发往生产线";
                case CartStatus.InCarriageToWorkStation:
                    return "向生产线运输中";
                case CartStatus.ArrivedAtWorkStation:
                    return "生产线边";
                case CartStatus.Indicating:
                    return "装配中";

                default:
                    return cartStatus.ToString();
            }
        }
    }
}