using System.Linq;
using System.Xml.Linq;
using DataAccess.Assorting;

namespace Interfaces.Converters
{
    /// <summary>
    /// 按托分拣结果报文转换器。
    /// </summary>
    public static class AST_PalletResultConverter
    {
        /// <summary>
        /// 转换按托分拣结果到报文。
        /// </summary>
        /// <param name="astPalletResultInDbContext">处于数据库上下文中的实体。</param>
        /// <returns>报文。</returns>
        public static string ConvertRequest(AST_PalletResult astPalletResultInDbContext)
        {
            XDocument xDocument = new XDocument();
            XElement serviceElement = new XElement("Service");
            XElement dataElement = new XElement("Data");
            XElement requestElement = new XElement("Request");
            XElement responseElement = new XElement("Response");

            xDocument.Add(serviceElement);
            serviceElement.Add(dataElement);
            dataElement.Add(requestElement);
            dataElement.Add(responseElement);

            XElement resultElement = new XElement("ASSEMBLE");
            resultElement.SetAttributeValue("p_type", "G");
            resultElement.SetAttributeValue("loop_num", "1");
            resultElement.Add(new XElement("ID", astPalletResultInDbContext.Id));
            resultElement.Add(new XElement("ProjectCode", astPalletResultInDbContext.ProjectCode));
            resultElement.Add(new XElement("PS_POSID", astPalletResultInDbContext.WbsId));
            resultElement.Add(new XElement("StageCode", astPalletResultInDbContext.ProjectStep));
            resultElement.Add(new XElement("BatchCode", astPalletResultInDbContext.BatchCode));
            resultElement.Add(new XElement("ChannelCode", astPalletResultInDbContext.CFG_Channel.Code));
            resultElement.Add(new XElement("PalletCode", astPalletResultInDbContext.CFG_Pallet.Code));
            resultElement.Add(new XElement("BeginPickTime", astPalletResultInDbContext.BeginPickTime.ToString("yyyy-MM-dd HH:mm:ss")));
            resultElement.Add(new XElement("EndPickTime", astPalletResultInDbContext.EndPickTime.ToString("yyyy-MM-dd HH:mm:ss")));
            resultElement.Add(new XElement("EmployeeCode", astPalletResultInDbContext.CFG_Employee == null ? string.Empty : astPalletResultInDbContext.CFG_Employee.Code));
            resultElement.Add(new XElement("EmployeeName", astPalletResultInDbContext.CFG_Employee == null ? string.Empty : astPalletResultInDbContext.CFG_Employee.Name));
            resultElement.Add(new XElement("TOTALNUM", astPalletResultInDbContext.AST_PalletResultItems.Count));

            requestElement.Add(resultElement);

            int loopNumber = 1;
            foreach (AST_PalletResultItem astPalletResultItem in astPalletResultInDbContext.AST_PalletResultItems)
            {
                XElement resultItemElement = new XElement("ASSEMBLEITEM");
                resultItemElement.SetAttributeValue("p_type", "G");
                resultItemElement.SetAttributeValue("loop_num", loopNumber++);
                resultItemElement.Add(new XElement("STATIONCODE", astPalletResultItem.CFG_WorkStation.Code));
                resultItemElement.Add(new XElement("GZZLIST", astPalletResultItem.GzzList));
                resultItemElement.Add(new XElement("BillDtlID", astPalletResultItem.BillDetailId));
                resultItemElement.Add(new XElement("BoxCode", astPalletResultItem.BoxCode));
                resultItemElement.Add(new XElement("PalletPosition", astPalletResultItem.PalletPosition));
                resultItemElement.Add(new XElement("MaterialCode", astPalletResultItem.MaterialCode));
                resultItemElement.Add(new XElement("MaterialName", astPalletResultItem.MaterialName));
                resultItemElement.Add(new XElement("MaterialBarcode", astPalletResultItem.MaterialBarcode));
                resultItemElement.Add(new XElement("NEED_PICK_NUM", astPalletResultItem.ToPickQuantity));
                resultItemElement.Add(new XElement("PICKED_NUM", astPalletResultItem.PickedQuantity));
                resultItemElement.Add(new XElement("CartCode", astPalletResultItem.CFG_Cart == null ? string.Empty : astPalletResultItem.CFG_Cart.Code));
                resultItemElement.Add(new XElement("CartPosition", astPalletResultItem.CartPosition == null ? string.Empty : astPalletResultItem.CartPosition.ToString()));

                requestElement.Add(resultItemElement);
            }

            return Converter.ToXml(xDocument);
        }

        /// <summary>
        /// 检查响应是否成功。
        /// </summary>
        /// <param name="xml">响应报文。</param>
        /// <param name="errorMessage">失败时错误信息。</param>
        /// <returns>是否被成功处理。</returns>
        public static bool CheckResponse(string xml, out string errorMessage)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement responseElement = dataElement.Descendants("Response").First();
            XElement resultElement = responseElement.Descendants("ASSEMBLE").First();

            bool successful = resultElement.Element("TYPE").Value == "S";
            errorMessage = resultElement.Element("MESSAGE").Value;

            return successful;
        }
    }
}