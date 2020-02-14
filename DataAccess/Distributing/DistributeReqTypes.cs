using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Distributing
{
    /// <summary>
    /// 配送任务类型
    /// </summary>
    public enum DistributeReqTypes
    {
        /// <summary>
        /// 拣料区铺线
        /// </summary>
        PickAreaInit = 0,
        /// <summary>
        /// 生产线边铺线
        /// </summary>
        ProductAreaInit = 1,
        /// <summary>
        /// 拣料区配送
        /// </summary>
        PickAreaDistribute = 2,
        /// <summary>
        /// 物料超市配送
        /// </summary>
        MaterialMarketDistribute = 3,
        /// <summary>
        /// 空料架缓冲区配送
        /// </summary>
        NullCartAreaDistribute = 4,
        /// <summary>
        /// 生产线边料架转换
        /// </summary>
        ProductCartSwitch = 5,
        /// <summary>
        /// 生产线边空料架返回
        /// </summary>
        ProductNullCartBack = 6,
        /// <summary>
        /// 生产线边外侧到里侧
        /// </summary>
        ProductOutToIn=7,
        /// <summary>
        /// 生产线边里侧到外侧
        /// </summary>
        ProductInToOut = 8,
        /// <summary>
        /// 绑定货架
        /// </summary>
        BindPod = 9,
        /// <summary>
        /// 解绑货架
        /// </summary>
        UnBindPod = 10,
        /// <summary>
        /// 点对点配送
        /// </summary>
        PointToPointDistribute=11,
    }
}
