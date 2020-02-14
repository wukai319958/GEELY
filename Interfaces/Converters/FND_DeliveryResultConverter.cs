using System.Linq;
using System.Xml.Linq;
using DataAccess.CartFinding;

namespace Interfaces.Converters
{
    /// <summary>
    /// 配送结果报文转换器。
    /// </summary>
    public static class FND_DeliveryResultConverter
    {
        /// <summary>
        /// 转换配送结果到报文。
        /// </summary>
        /// <param name="fndDeliveryResultInDbContext">处于数据库上下文中的实体。</param>
        /// <returns>报文。</returns>
        public static string ConvertRequest(FND_DeliveryResult fndDeliveryResultInDbContext)
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
            resultElement.Add(new XElement("ID", fndDeliveryResultInDbContext.Id));
            resultElement.Add(new XElement("ProjectCode", fndDeliveryResultInDbContext.ProjectCode));
            resultElement.Add(new XElement("StageCode", fndDeliveryResultInDbContext.ProjectStep));
            resultElement.Add(new XElement("STATIONCODE", fndDeliveryResultInDbContext.CFG_WorkStation.Code));
            resultElement.Add(new XElement("MaxNeedArrivedTime", fndDeliveryResultInDbContext.MaxNeedArrivedTime.ToString("yyyy-MM-dd HH:mm:ss")));
            resultElement.Add(new XElement("BatchCode", fndDeliveryResultInDbContext.BatchCode));
            resultElement.Add(new XElement("CartCode", fndDeliveryResultInDbContext.CFG_Cart.Code));
            resultElement.Add(new XElement("DepartedTime", fndDeliveryResultInDbContext.DepartedTime.ToString("yyyy-MM-dd HH:mm:ss")));
            resultElement.Add(new XElement("EmployeeCode", fndDeliveryResultInDbContext.CFG_Employee == null ? string.Empty : fndDeliveryResultInDbContext.CFG_Employee.Code));
            resultElement.Add(new XElement("EmployeeName", fndDeliveryResultInDbContext.CFG_Employee == null ? string.Empty : fndDeliveryResultInDbContext.CFG_Employee.Name));
            resultElement.Add(new XElement("TOTALNUM", fndDeliveryResultInDbContext.FND_DeliveryResultItems.Count));

            requestElement.Add(resultElement);

            int loopNumber = 1;
            foreach (FND_DeliveryResultItem fndDeliveryResultItem in fndDeliveryResultInDbContext.FND_DeliveryResultItems)
            {
                XElement resultItemElement = new XElement("ASSEMBLEITEM");
                resultItemElement.SetAttributeValue("p_type", "G");
                resultItemElement.SetAttributeValue("loop_num", loopNumber++);
                resultItemElement.Add(new XElement("CartPosition", fndDeliveryResultItem.CartPosition));
                resultItemElement.Add(new XElement("MaterialCode", fndDeliveryResultItem.MaterialCode));
                resultItemElement.Add(new XElement("MaterialName", fndDeliveryResultItem.MaterialName));
                resultItemElement.Add(new XElement("MaterialBarcode", fndDeliveryResultItem.MaterialBarcode));
                resultItemElement.Add(new XElement("Quantity", fndDeliveryResultItem.Quantity));

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