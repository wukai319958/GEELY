using System.Linq;
using System.Xml.Linq;
using DataAccess.AssemblyIndicating;

namespace Interfaces.Converters
{
    /// <summary>
    /// 装配结果报文转换器。
    /// </summary>
    public static class ASM_AssembleResultConverter
    {
        /// <summary>
        /// 转换装配结果到报文。
        /// </summary>
        /// <param name="asmAssembleResultInDbContext">处于数据库上下文中的实体。</param>
        /// <returns>报文。</returns>
        public static string ConvertRequest(ASM_AssembleResult asmAssembleResultInDbContext)
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

            XElement resultElement = new XElement("ASSEMBLEDRESULT");
            resultElement.SetAttributeValue("p_type", "G");
            resultElement.SetAttributeValue("loop_num", "1");
            resultElement.Add(new XElement("ID", asmAssembleResultInDbContext.Id));
            resultElement.Add(new XElement("FCCODE", asmAssembleResultInDbContext.FactoryCode));
            resultElement.Add(new XElement("PLCODE", asmAssembleResultInDbContext.ProductionLineCode));
            resultElement.Add(new XElement("STCODE", asmAssembleResultInDbContext.CFG_WorkStation.Code));
            resultElement.Add(new XElement("GZZLIST", asmAssembleResultInDbContext.GzzList));
            resultElement.Add(new XElement("MONUM", asmAssembleResultInDbContext.MONumber));
            resultElement.Add(new XElement("PRDSEQ", asmAssembleResultInDbContext.ProductSequence));
            resultElement.Add(new XElement("BEGINTIME", asmAssembleResultInDbContext.BeginTime.ToString("yyyy-MM-dd HH:mm:ss")));
            resultElement.Add(new XElement("ENDTIME", asmAssembleResultInDbContext.EndTime.ToString("yyyy-MM-dd HH:mm:ss")));
            resultElement.Add(new XElement("TOTALNUM", asmAssembleResultInDbContext.ASM_AssembleResultItems.Count));

            requestElement.Add(resultElement);

            int loopNumber = 1;
            foreach (ASM_AssembleResultItem asmAssembleResultItem in asmAssembleResultInDbContext.ASM_AssembleResultItems)
            {
                XElement resultItemElement = new XElement("RESULTITEM");
                resultItemElement.SetAttributeValue("p_type", "G");
                resultItemElement.SetAttributeValue("loop_num", loopNumber++);
                resultItemElement.Add(new XElement("CARTCODE", asmAssembleResultItem.CFG_Cart.Code));
                resultItemElement.Add(new XElement("CARTPOSITION", asmAssembleResultItem.CartPosition));
                resultItemElement.Add(new XElement("GZZ", asmAssembleResultItem.Gzz));
                resultItemElement.Add(new XElement("MTCODE", asmAssembleResultItem.MaterialCode));
                resultItemElement.Add(new XElement("ASSSEQ", asmAssembleResultItem.AssembleSequence));
                resultItemElement.Add(new XElement("CPQTY", asmAssembleResultItem.ToAssembleQuantity));
                resultItemElement.Add(new XElement("ASSQTY", asmAssembleResultItem.AssembledQuantity));
                resultItemElement.Add(new XElement("PICKEDTIME", asmAssembleResultItem.PickedTime.ToString("yyyy-MM-dd HH:mm:ss")));
                resultItemElement.Add(new XElement("PRJCODE", asmAssembleResultItem.ProjectCode));
                resultItemElement.Add(new XElement("PRJSTEP", asmAssembleResultItem.ProjectStep));

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
            XElement resultElement = responseElement.Descendants("ASSEMBLEDRESULT").First();

            bool successful = resultElement.Element("TYPE").Value == "S";
            errorMessage = resultElement.Element("MESSAGE").Value;

            return successful;
        }
    }
}