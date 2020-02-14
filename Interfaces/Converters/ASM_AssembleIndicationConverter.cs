using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DataAccess;
using DataAccess.AssemblyIndicating;
using DataAccess.Config;

namespace Interfaces.Converters
{
    /// <summary>
    /// 装配指引报文转换器。
    /// </summary>
    public static class ASM_AssembleIndicationConverter
    {
        /// <summary>
        /// 转换装配指引报文。
        /// </summary>
        /// <param name="xml">装配指引报文。</param>
        /// <param name="dbContext">数据库上下文。</param>
        /// <returns>成功解析的实体。</returns>
        public static ASM_AssembleIndication ConvertRequest(string xml, GeelyPtlEntities dbContext)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement indicationElement = requestElement.Descendants("ASSEMBLE").First();
            List<XElement> indicationItemElements = requestElement.Descendants("ASSEMBLEITEM").ToList();

            ASM_AssembleIndication asmAssembleIndication = new ASM_AssembleIndication();
            asmAssembleIndication.FactoryCode = indicationElement.Element("FCCODE").Value;
            asmAssembleIndication.ProductionLineCode = indicationElement.Element("PLCODE").Value;

            string cfgWorkStationCode = indicationElement.Element("STCODE").Value;
            CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                 .FirstOrDefault(ws => ws.Code == cfgWorkStationCode);
            if (cfgWorkStation == null)
                throw new InterfaceDataException("无效的工位编码：" + cfgWorkStationCode);

            asmAssembleIndication.CFG_WorkStationId = cfgWorkStation.Id;
            asmAssembleIndication.GzzList = indicationElement.Element("GZZLIST").Value;
            asmAssembleIndication.MONumber = indicationElement.Element("MONUM").Value;
            asmAssembleIndication.ProductSequence = indicationElement.Element("PRDSEQ").Value;
            if (indicationElement.Element("PLANBATCH") != null)
                asmAssembleIndication.PlanBatch = indicationElement.Element("PLANBATCH").Value;
            if (indicationElement.Element("CARBATCH") != null)
                asmAssembleIndication.CarBatch = indicationElement.Element("CARBATCH").Value;
            asmAssembleIndication.CarArrivedTime = DateTime.Now;

            if (string.IsNullOrEmpty(asmAssembleIndication.FactoryCode))
                asmAssembleIndication.FactoryCode = "<NULL>";
            if (string.IsNullOrEmpty(asmAssembleIndication.ProductionLineCode))
                asmAssembleIndication.ProductionLineCode = "<NULL>";
            if (string.IsNullOrEmpty(asmAssembleIndication.GzzList))
                asmAssembleIndication.GzzList = "<NULL>";
            if (string.IsNullOrEmpty(asmAssembleIndication.MONumber))
                asmAssembleIndication.MONumber = "<NULL>";
            if (string.IsNullOrEmpty(asmAssembleIndication.ProductSequence))
                asmAssembleIndication.ProductSequence = "<NULL>";

            string materialCodeJoin = string.Join(", ", indicationItemElements.Select(iie => iie.Element("MTCODE").Value));
            if (!string.IsNullOrEmpty(materialCodeJoin)
                && dbContext.CFG_Carts.Count(c => (c.CartStatus == CartStatus.ArrivedAtWorkStation || c.CartStatus == CartStatus.Indicating)
                                                  && c.CFG_CartCurrentMaterials.Any(ccm => materialCodeJoin.Contains(ccm.MaterialCode))) == 0)
            {
                string message = "装配线边没有存放这些物料的料车。";

                if (indicationItemElements.Count > 0)
                {
                    XElement firstItem = indicationItemElements.First();

                    string batchCode = asmAssembleIndication.PlanBatch;
                    if (firstItem.Element("BATCHCODE") != null)
                        batchCode += firstItem.Element("BATCHCODE").Value;

                    List<string> onWayCartNames = dbContext.CFG_CartCurrentMaterials
                                                      .Where(ccm => ccm.BatchCode == batchCode && ccm.CFG_WorkStationId == cfgWorkStation.Id && materialCodeJoin.Contains(ccm.MaterialCode))
                                                      .Select(ccm => ccm.CFG_Cart)
                                                      .Distinct()
                                                      .ToList()
                                                      .Select(c => c.Name + "(" + c.CartStatus + ")")
                                                      .ToList();

                    if (onWayCartNames.Count > 0)
                        message += "但以下料车拥有：" + string.Join(", ", onWayCartNames);
                }

                throw new InterfaceDataException(message);
            }

            foreach (XElement indicationItemElement in indicationItemElements)
            {
                ASM_AssembleIndicationItem asmAssembleIndicationItem = new ASM_AssembleIndicationItem();
                asmAssembleIndicationItem.ASM_AssembleIndication = asmAssembleIndication;
                asmAssembleIndicationItem.Gzz = indicationItemElement.Element("GZZ").Value;
                asmAssembleIndicationItem.MaterialCode = indicationItemElement.Element("MTCODE").Value;
                asmAssembleIndicationItem.MaterialName = indicationItemElement.Element("MTTEXT").Value;
                decimal assembleSequence;
                decimal toAssembleQuantity;
                if (decimal.TryParse(indicationItemElement.Element("ASSSEQ").Value, out assembleSequence))
                    asmAssembleIndicationItem.AssembleSequence = (int)assembleSequence;
                if (decimal.TryParse(indicationItemElement.Element("CPQTY").Value, out toAssembleQuantity))
                    asmAssembleIndicationItem.ToAssembleQuantity = (int)toAssembleQuantity;
                asmAssembleIndicationItem.Qtxbs = indicationItemElement.Element("QTXBS").Value;
                asmAssembleIndicationItem.ProjectCode = indicationItemElement.Element("PRJCODE").Value;
                asmAssembleIndicationItem.ProjectStep = indicationItemElement.Element("PRJSTEP").Value;
                //PlanBatch：20181109，BatchCode：A1
                if (indicationItemElement.Element("BATCHCODE") != null)
                    asmAssembleIndicationItem.BatchCode = asmAssembleIndication.PlanBatch + indicationItemElement.Element("BATCHCODE").Value;

                if (string.IsNullOrEmpty(asmAssembleIndicationItem.Gzz))
                    asmAssembleIndicationItem.Gzz = "<NULL>";
                if (string.IsNullOrEmpty(asmAssembleIndicationItem.MaterialName))
                    asmAssembleIndicationItem.MaterialName = "<NULL>";
                if (string.IsNullOrEmpty(asmAssembleIndicationItem.Qtxbs))
                    asmAssembleIndicationItem.Qtxbs = "<NULL>";
                if (string.IsNullOrEmpty(asmAssembleIndicationItem.ProjectCode))
                    asmAssembleIndicationItem.ProjectCode = "<NULL>";
                if (string.IsNullOrEmpty(asmAssembleIndicationItem.ProjectStep))
                    asmAssembleIndicationItem.ProjectStep = "<NULL>";

                // 只装需转配且配料车里有的明细
                if (asmAssembleIndicationItem.ToAssembleQuantity > 0
                    && dbContext.CFG_CartCurrentMaterials.Any(ccm => ccm.MaterialCode == asmAssembleIndicationItem.MaterialCode
                                                                     && ccm.Quantity > 0
                                                                     && (ccm.CFG_Cart.CartStatus == CartStatus.ArrivedAtWorkStation || ccm.CFG_Cart.CartStatus == CartStatus.Indicating)))
                {
                    asmAssembleIndication.ASM_AssembleIndicationItems.Add(asmAssembleIndicationItem);
                }
            }

            return asmAssembleIndication;
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
            XElement indicationElement = requestElement.Descendants("ASSEMBLE").First();
            List<XElement> indicationItemElements = requestElement.Descendants("ASSEMBLEITEM").ToList();

            requestElement.RemoveNodes();
            responseElement.Add(indicationElement);
            responseElement.Add(indicationItemElements);

            indicationElement.Add(new XElement("TYPE", successful ? "S" : "E"));
            indicationElement.Add(new XElement("MESSAGE", errorMessage));

            return Converter.ToXml(xDocument);
        }
    }
}