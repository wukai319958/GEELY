using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace AssortingKanban.Converters
{
    /// <summary>
    /// 拣料类型到其文本的转换器
    /// </summary>
    class PickTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string sPickType = null == value ? "" : value.ToString();
            string sPickTypeName = string.Empty;
            //switch (sPickType)
            //{
            //    case "P":
            //        sPickTypeName = "PTL料架拣料";
            //        break;
            //    case "N":
            //        sPickTypeName = "PDA正常领料";
            //        break;
            //    case "U":
            //        sPickTypeName = "PDA异常领料";
            //        break;
            //    case "T":
            //        sPickTypeName = "PDA试验领料";
            //        break;
            //    case "C":
            //        sPickTypeName = "库存盘点";
            //        break;
            //    case "R":
            //        sPickTypeName = "退货出库";
            //        break;
            //    case "B":
            //        sPickTypeName = "借用出库";
            //        break;
            //    case "E":
            //        sPickTypeName = "空托出库";
            //        break;
            //    default:
            //        sPickTypeName = string.Empty;
            //        break;
            //}
            if (string.IsNullOrEmpty(sPickType))
            {
                sPickTypeName = string.Empty;
            }
            else if (sPickType.Equals("P"))
            {
                sPickTypeName = "PTL料架拣料";
            }
            else
            {
                sPickTypeName = sPickType;
            }
            return sPickTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
