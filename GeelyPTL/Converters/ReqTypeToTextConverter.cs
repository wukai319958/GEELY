using DataAccess.Distributing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace GeelyPTL.Converters
{
    public class ReqTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DistributeReqTypes reqTypes = (DistributeReqTypes)value;
            switch (reqTypes)
            {
                case DistributeReqTypes.PickAreaInit:
                    return "拣料区铺线";
                case DistributeReqTypes.ProductAreaInit:
                    return "生产线边铺线";
                case DistributeReqTypes.PickAreaDistribute:
                    return "拣料区配送";
                case DistributeReqTypes.MaterialMarketDistribute:
                    return "物料超市配送";
                case DistributeReqTypes.NullCartAreaDistribute:
                    return "空料架缓冲区配送";
                case DistributeReqTypes.ProductCartSwitch:
                    return "生产线边料架转换";
                case DistributeReqTypes.ProductNullCartBack:
                    return "生产线边空料架返回";
                case DistributeReqTypes.ProductOutToIn:
                    return "生产线边外侧到里侧";
                case DistributeReqTypes.ProductInToOut:
                    return "生产线边里侧到外侧";
                case DistributeReqTypes.BindPod:
                    return "绑定货架";
                case DistributeReqTypes.UnBindPod:
                    return "解绑货架";
                case DistributeReqTypes.PointToPointDistribute:
                    return "点对点配送";
                default:
                    return reqTypes.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
