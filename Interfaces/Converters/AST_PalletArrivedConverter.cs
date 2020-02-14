using System;
using System.Linq;
using System.Xml.Linq;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.Config;
using DataAccess.AssortingPDA;

namespace Interfaces.Converters
{
    /// <summary>
    /// 托盘抵达分拣口报文转换器。
    /// </summary>
    public static class AST_PalletArrivedConverter
    {
        /// <summary>
        /// 转换托盘抵达分拣口报文。
        /// </summary>
        /// <param name="xml">托盘抵达分拣口报文。</param>
        /// <param name="dbContext">数据库上下文。</param>
        /// <returns>成功解析的实体。</returns>
        public static AST_PalletArrived ConvertRequest(string xml, GeelyPtlEntities dbContext)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement arrivedElement = requestElement.Descendants("ASSEMBLE").First();

            AST_PalletArrived astPalletArrived = new AST_PalletArrived();
            astPalletArrived.ProjectCode = arrivedElement.Element("ProjectCode").Value;
            astPalletArrived.WbsId = arrivedElement.Element("PS_POSID").Value;
            astPalletArrived.ProjectStep = arrivedElement.Element("StageCode").Value;

            string batchCode = arrivedElement.Element("BatchCode").Value;
            if (!dbContext.AST_LesTasks.Any(lt => lt.BatchCode == batchCode))
                throw new InterfaceDataException("该批次的任务未下发：" + batchCode);

            astPalletArrived.BatchCode = batchCode;

            string cfgChannelCode = arrivedElement.Element("ChannelCode").Value;
            CFG_Channel cfgChannel = dbContext.CFG_Channels
                                         .FirstOrDefault(c => c.Code == cfgChannelCode);
            if (cfgChannel == null)
                throw new InterfaceDataException("无效的分拣巷道：" + cfgChannelCode);

            astPalletArrived.CFG_ChannelId = cfgChannel.Id;

            string cfgPalletCode = arrivedElement.Element("PalletCode").Value;
            CFG_Pallet cfgPallet = dbContext.CFG_Pallets
                                       .FirstOrDefault(c => c.Code == cfgPalletCode);
            if (cfgPallet == null)
                throw new InterfaceDataException("无效的托盘编码：" + cfgPalletCode);

            cfgPallet.PalletType = arrivedElement.Element("PalletType").Value;

            astPalletArrived.CFG_PalletId = cfgPallet.Id;
            astPalletArrived.PickBillIds = arrivedElement.Element("PickBillIDs").Value;
            astPalletArrived.ArrivedTime = DateTime.Now;

            return astPalletArrived;
        }

        /// <summary>
        /// 转换托盘抵达分拣口报文-PDA。
        /// </summary>
        /// <param name="xml">托盘抵达分拣口报文。</param>
        /// <param name="dbContext">数据库上下文。</param>
        /// <returns>成功解析的实体。</returns>
        public static AST_PalletArrived_PDA ConvertRequestPDA(string xml, GeelyPtlEntities dbContext)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement arrivedElement = requestElement.Descendants("ASSEMBLE").First();

            AST_PalletArrived_PDA astPalletArrived = new AST_PalletArrived_PDA();
            astPalletArrived.ProjectCode = arrivedElement.Element("ProjectCode").Value;
            astPalletArrived.WbsId = arrivedElement.Element("PS_POSID").Value;
            astPalletArrived.ProjectStep = arrivedElement.Element("StageCode").Value;

            string batchCode = arrivedElement.Element("BatchCode").Value;
            if (!dbContext.AST_LesTask_PDAs.Any(lt => lt.BatchCode == batchCode))
                throw new InterfaceDataException("该批次的任务未下发：" + batchCode);

            astPalletArrived.BatchCode = batchCode;

            string cfgChannelCode = arrivedElement.Element("ChannelCode").Value;
            CFG_Channel cfgChannel = dbContext.CFG_Channels
                                         .FirstOrDefault(c => c.Code == cfgChannelCode);
            if (cfgChannel == null)
                throw new InterfaceDataException("无效的分拣巷道：" + cfgChannelCode);

            astPalletArrived.CFG_ChannelId = cfgChannel.Id;

            string cfgPalletCode = arrivedElement.Element("PalletCode").Value;
            CFG_Pallet cfgPallet = dbContext.CFG_Pallets
                                       .FirstOrDefault(c => c.Code == cfgPalletCode);
            if (cfgPallet == null)
                throw new InterfaceDataException("无效的托盘编码：" + cfgPalletCode);

            cfgPallet.PalletType = arrivedElement.Element("PalletType").Value;

            astPalletArrived.CFG_PalletId = cfgPallet.Id;
            astPalletArrived.PickBillIds = arrivedElement.Element("PickBillIDs").Value;
            astPalletArrived.ArrivedTime = DateTime.Now;

            return astPalletArrived;
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
            XElement arrivedElement = requestElement.Descendants("ASSEMBLE").First();

            requestElement.RemoveNodes();
            responseElement.Add(arrivedElement);

            arrivedElement.Add(new XElement("TYPE", successful ? "S" : "E"));
            arrivedElement.Add(new XElement("MESSAGE", errorMessage));

            return Converter.ToXml(xDocument);
        }
    }
}