using DataAccess;
using DataAccess.AssortingPDA;
using DataAccess.Config;
using Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Interfaces.Converters
{
    public static class AST_PalletPickResult_PDAConverter
    {
        /// <summary>
        /// 转换 LES 原始托盘分拣结果报文。
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static List<AST_PalletPickResult_PDA> ConvertRequestPDA(string xml, GeelyPtlEntities dbContext)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            //XElement arrivedElement = requestElement.Descendants("ASSEMBLE").First();
            List<XElement> taskItemElements = requestElement.Descendants("ASSEMBLE").ToList();

            if (taskItemElements.Count == 0)
                throw new InterfaceDataException("没有托盘分拣结果。");

            List<AST_PalletPickResult_PDA> astPalletPickResults = new List<AST_PalletPickResult_PDA>();
            foreach (XElement taskItemElement in taskItemElements)
            {
                AST_PalletPickResult_PDA result = new AST_PalletPickResult_PDA();
                result.BatchCode = taskItemElement.Element("BATCH").Value;
                result.PickBillIds = taskItemElement.Element("BILLID").Value;
                result.BoxCode = taskItemElement.Element("BOXCODE").Value;
                result.Status = int.Parse(taskItemElement.Element("STATUS").Value);
                result.Quantity = int.Parse(taskItemElement.Element("QUANTITY").Value);
                result.ReceivedTime = DateTime.Now;
                string cfgPalletCode = taskItemElement.Element("PALLETCODE").Value;
                CFG_Pallet cfgPallet = dbContext.CFG_Pallets
                                           .FirstOrDefault(c => c.Code == cfgPalletCode);
                if (cfgPallet == null)
                    throw new InterfaceDataException("无效的托盘编码：" + cfgPalletCode);
                result.CFG_PalletId = cfgPallet.Id;

                astPalletPickResults.Add(result);
            }
            return astPalletPickResults;
        }

        /// <summary>
        /// 生成响应报文。
        /// </summary>
        /// <param name="requestXml">源请求报文。</param>
        /// <param name="successful">是否成功处理。</param>
        /// <param name="errorMessage">失败时错误信息。</param>
        /// <returns>响应报文。</returns>
        public static string ConvertResponse(string requestXml, bool successful, string errorMessage)
        {
            XDocument xDocument = XDocument.Parse(requestXml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement responseElement = dataElement.Descendants("Response").First();
            //XElement arrivedElement = requestElement.Descendants("ASSEMBLE").First();
            List<XElement> taskItemElements = requestElement.Descendants("ASSEMBLE").ToList();

            requestElement.RemoveNodes();
            responseElement.Add(taskItemElements);

            foreach (XElement taskItemElement in taskItemElements)
            {
                taskItemElement.Add(new XElement("TYPE", successful ? "S" : "E"));
                taskItemElement.Add(new XElement("MESSAGE", errorMessage));
            }
            return Converter.ToXml(xDocument);
        }
    }
}
