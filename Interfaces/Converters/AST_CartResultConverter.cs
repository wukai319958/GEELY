using System.Linq;
using System.Xml.Linq;
using DataAccess.Assorting;

namespace Interfaces.Converters
{
    /// <summary>
    /// 按车分拣结果报文转换器。
    /// </summary>
    public static class AST_CartResultConverter
    {
        /// <summary>
        /// 转换按车分拣结果到报文。
        /// </summary>
        /// <param name="astCartResultInDbContext">处于数据库上下文中的实体。</param>
        /// <returns>报文。</returns>
        public static string ConvertRequest(AST_CartResult astCartResultInDbContext)
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
            resultElement.Add(new XElement("ID", astCartResultInDbContext.Id));
            resultElement.Add(new XElement("ProjectCode", astCartResultInDbContext.ProjectCode));
            resultElement.Add(new XElement("PS_POSID", astCartResultInDbContext.WbsId));
            resultElement.Add(new XElement("StageCode", astCartResultInDbContext.ProjectStep));
            resultElement.Add(new XElement("STATIONCODE", astCartResultInDbContext.CFG_WorkStation.Code));
            resultElement.Add(new XElement("BatchCode", astCartResultInDbContext.BatchCode));
            resultElement.Add(new XElement("ChannelCode", astCartResultInDbContext.CFG_Channel.Code));
            resultElement.Add(new XElement("CartCode", astCartResultInDbContext.CFG_Cart.Code));
            resultElement.Add(new XElement("BeginAssortTime", astCartResultInDbContext.BeginPickTime.ToString("yyyy-MM-dd HH:mm:ss")));
            resultElement.Add(new XElement("EndAssortTime", astCartResultInDbContext.EndPickTime.ToString("yyyy-MM-dd HH:mm:ss")));
            resultElement.Add(new XElement("EmployeeCode", astCartResultInDbContext.CFG_Employee == null ? string.Empty : astCartResultInDbContext.CFG_Employee.Code));
            resultElement.Add(new XElement("EmployeeName", astCartResultInDbContext.CFG_Employee == null ? string.Empty : astCartResultInDbContext.CFG_Employee.Name));
            resultElement.Add(new XElement("TOTALNUM", astCartResultInDbContext.AST_CartResultItems.Count));

            requestElement.Add(resultElement);

            int loopNumber = 1;
            foreach (AST_CartResultItem astCartResultItem in astCartResultInDbContext.AST_CartResultItems)
            {
                XElement resultItemElement = new XElement("ASSEMBLEITEM");
                resultItemElement.SetAttributeValue("p_type", "G");
                resultItemElement.SetAttributeValue("loop_num", loopNumber++);
                resultItemElement.Add(new XElement("CartPosition", astCartResultItem.CartPosition));
                resultItemElement.Add(new XElement("MaterialCode", astCartResultItem.MaterialCode));
                resultItemElement.Add(new XElement("MaterialName", astCartResultItem.MaterialName));
                resultItemElement.Add(new XElement("MaterialBarcode", astCartResultItem.MaterialBarcode));
                resultItemElement.Add(new XElement("Quantity", astCartResultItem.Quantity));

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