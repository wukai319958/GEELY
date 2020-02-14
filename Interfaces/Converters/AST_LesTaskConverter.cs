using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.Config;
using DataAccess.AssortingPDA;

namespace Interfaces.Converters
{
    /// <summary>
    /// LES 原始任务报文转换器。
    /// </summary>
    public static class AST_LesTaskConverter
    {
        /// <summary>
        /// 转换 LES 原始任务报文。
        /// </summary>
        /// <param name="xml">LES 原始任务报文。</param>
        /// <param name="dbContext">数据库上下文。</param>
        /// <returns>成功解析的实体。</returns>
        public static AST_LesTask ConvertRequest(string xml, GeelyPtlEntities dbContext)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement taskElement = requestElement.Descendants("ASSEMBLE").First();
            List<XElement> taskItemElements = requestElement.Descendants("ASSEMBLEITEM").ToList();

            if (taskItemElements.Count == 0)
                throw new InterfaceDataException("没有明细。");

            AST_LesTask astLesTask = new AST_LesTask();
            string projectCode = taskElement.Element("ProjectCode").Value;
            string projectStep = taskElement.Element("StageCode").Value;

            string[] codes = projectCode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] steps = projectStep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (codes.Length > 1 && codes[0] == codes[1])
                projectCode = codes[0];
            if (steps.Length > 1 && steps[0] == steps[1])
                projectStep = steps[0];

            astLesTask.ProjectCode = projectCode;
            astLesTask.WbsId = taskElement.Element("PS_POSID").Value;
            astLesTask.ProjectStep = projectStep;
            astLesTask.BillCode = taskElement.Element("PickNO").Value;
            astLesTask.BillDate = DateTime.Parse(taskElement.Element("Bill_Date").Value, CultureInfo.InvariantCulture);

            //工位随时会新增
            string cfgWorkStationCode = taskElement.Element("STATIONCODE").Value;
            CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                 .FirstOrDefault(ws => ws.Code == cfgWorkStationCode);
            if (cfgWorkStation == null)
            {
                using (GeelyPtlEntities innerDbContext = new GeelyPtlEntities())
                {
                    cfgWorkStation = new CFG_WorkStation();
                    cfgWorkStation.Code = cfgWorkStationCode;
                    cfgWorkStation.Name = cfgWorkStation.Code;

                    innerDbContext.CFG_WorkStations.Add(cfgWorkStation);

                    //每个工位有 8 个车位
                    for (int position = 1; position <= 8; position++)
                    {
                        CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = new CFG_WorkStationCurrentCart();
                        cfgWorkStationCurrentCart.CFG_WorkStation = cfgWorkStation;
                        cfgWorkStationCurrentCart.Position = position;

                        innerDbContext.CFG_WorkStationCurrentCarts.Add(cfgWorkStationCurrentCart);
                    }

                    innerDbContext.SaveChanges();
                }
            }

            astLesTask.CFG_WorkStationId = cfgWorkStation.Id;
            astLesTask.GzzList = taskElement.Element("GZZLIST").Value;
            astLesTask.BatchCode = taskElement.Element("BatchCode").Value;

            string cfgChannelCode = taskElement.Element("ChannelCode").Value;
            CFG_Channel cfgChannel = dbContext.CFG_Channels
                                         .FirstOrDefault(c => c.Code == cfgChannelCode);
            if (cfgChannel == null)
                throw new InterfaceDataException("无效的分拣巷道：" + cfgChannelCode);

            astLesTask.CFG_ChannelId = cfgChannel.Id;

            //托盘随时会增加
            string cfgPalletCode = taskElement.Element("PalletCode").Value;
            CFG_Pallet cfgPallet = dbContext.CFG_Pallets
                                       .FirstOrDefault(p => p.Code == cfgPalletCode);
            if (cfgPallet == null)
            {
                using (GeelyPtlEntities innerDbContext = new GeelyPtlEntities())
                {
                    cfgPallet = new CFG_Pallet();
                    cfgPallet.Code = cfgPalletCode;
                    cfgPallet.PalletType = "01";

                    innerDbContext.CFG_Pallets.Add(cfgPallet);

                    innerDbContext.SaveChanges();
                }
            }

            astLesTask.CFG_PalletId = cfgPallet.Id;
            astLesTask.BoxCode = taskElement.Element("BoxCode").Value;
            astLesTask.FromPalletPosition = int.Parse(taskElement.Element("FromPalletPosition").Value, CultureInfo.InvariantCulture);
            if (astLesTask.FromPalletPosition < 1 || astLesTask.FromPalletPosition > 10)
                astLesTask.FromPalletPosition = 1;
            astLesTask.RequestTime = DateTime.Now;

            foreach (XElement taskItemElement in taskItemElements)
            {
                AST_LesTaskItem astLesTaskItem = new AST_LesTaskItem();
                astLesTaskItem.AST_LesTask = astLesTask;
                astLesTaskItem.BillDetailId = taskItemElement.Element("BillDtlID").Value;
                astLesTaskItem.MaterialCode = taskItemElement.Element("MaterialCode").Value;
                astLesTaskItem.MaterialName = taskItemElement.Element("MaterialName").Value;
                astLesTaskItem.MaterialBarcode = taskItemElement.Element("MaterialBarcode").Value;
                astLesTaskItem.ToPickQuantity = (int)decimal.Parse(taskItemElement.Element("NEED_PICK_NUM").Value, CultureInfo.InvariantCulture);
                astLesTaskItem.MaxQuantityInSingleCartPosition = (int)decimal.Parse(taskItemElement.Element("MaxQuantityInSingleCartPosition").Value, CultureInfo.InvariantCulture);
                if (astLesTaskItem.MaxQuantityInSingleCartPosition <= 0)
                    astLesTaskItem.MaxQuantityInSingleCartPosition = int.MaxValue;
                astLesTaskItem.IsSpecial = taskItemElement.Element("IsSpecial").Value == "1";
                astLesTaskItem.IsBig = taskItemElement.Element("STORETYPE").Value == "04";

                astLesTask.AST_LesTaskItems.Add(astLesTaskItem);
            }

            return astLesTask;
        }

        /// <summary>
        /// 转换 LES 原始任务报文-PDA。
        /// </summary>
        /// <param name="xml">LES 原始任务报文。</param>
        /// <param name="dbContext">数据库上下文。</param>
        /// <returns>成功解析的实体。</returns>
        public static AST_LesTask_PDA ConvertRequestPDA(string xml, GeelyPtlEntities dbContext)
        {
            XDocument xDocument = XDocument.Parse(xml);
            XElement serviceElement = xDocument.Descendants("Service").First();
            XElement dataElement = serviceElement.Descendants("Data").First();
            XElement requestElement = dataElement.Descendants("Request").First();
            XElement taskElement = requestElement.Descendants("ASSEMBLE").First();
            List<XElement> taskItemElements = requestElement.Descendants("ASSEMBLEITEM").ToList();

            if (taskItemElements.Count == 0)
                throw new InterfaceDataException("没有明细。");

            AST_LesTask_PDA astLesTask = new AST_LesTask_PDA();
            astLesTask.ProjectCode = taskElement.Element("ProjectCode").Value;
            astLesTask.WbsId = taskElement.Element("PS_POSID").Value;
            astLesTask.ProjectStep = taskElement.Element("StageCode").Value;
            astLesTask.BillCode = taskElement.Element("PickNO").Value;
            astLesTask.BillDate = DateTime.Parse(taskElement.Element("Bill_Date").Value, CultureInfo.InvariantCulture);

            //工位随时会新增
            string cfgWorkStationCode = taskElement.Element("STATIONCODE").Value;
            if (string.IsNullOrEmpty(cfgWorkStationCode))
            {
                cfgWorkStationCode = "#";
            }
            CFG_WorkStation cfgWorkStation = dbContext.CFG_WorkStations
                                                 .FirstOrDefault(ws => ws.Code == cfgWorkStationCode);
            if (cfgWorkStation == null)
            {
                using (GeelyPtlEntities innerDbContext = new GeelyPtlEntities())
                {
                    cfgWorkStation = new CFG_WorkStation();
                    cfgWorkStation.Code = cfgWorkStationCode;
                    cfgWorkStation.Name = cfgWorkStation.Code;

                    innerDbContext.CFG_WorkStations.Add(cfgWorkStation);

                    //每个工位有 8 个车位
                    for (int position = 1; position <= 8; position++)
                    {
                        CFG_WorkStationCurrentCart cfgWorkStationCurrentCart = new CFG_WorkStationCurrentCart();
                        cfgWorkStationCurrentCart.CFG_WorkStation = cfgWorkStation;
                        cfgWorkStationCurrentCart.Position = position;

                        innerDbContext.CFG_WorkStationCurrentCarts.Add(cfgWorkStationCurrentCart);
                    }

                    innerDbContext.SaveChanges();
                }
            }

            astLesTask.CFG_WorkStationId = cfgWorkStation.Id;
            astLesTask.GzzList = taskElement.Element("GZZLIST").Value;
            astLesTask.BatchCode = taskElement.Element("BatchCode").Value;

            string cfgChannelCode = taskElement.Element("ChannelCode").Value;
            CFG_Channel cfgChannel = dbContext.CFG_Channels
                                         .FirstOrDefault(c => c.Code == cfgChannelCode);
            if (cfgChannel == null)
                throw new InterfaceDataException("无效的分拣巷道：" + cfgChannelCode);

            astLesTask.CFG_ChannelId = cfgChannel.Id;

            //托盘随时会增加
            string cfgPalletCode = taskElement.Element("PalletCode").Value;
            CFG_Pallet cfgPallet = dbContext.CFG_Pallets
                                       .FirstOrDefault(p => p.Code == cfgPalletCode);
            if (cfgPallet == null)
            {
                using (GeelyPtlEntities innerDbContext = new GeelyPtlEntities())
                {
                    cfgPallet = new CFG_Pallet();
                    cfgPallet.Code = cfgPalletCode;
                    cfgPallet.PalletType = "01";

                    innerDbContext.CFG_Pallets.Add(cfgPallet);

                    innerDbContext.SaveChanges();
                }
            }

            astLesTask.CFG_PalletId = cfgPallet.Id;
            astLesTask.BoxCode = taskElement.Element("BoxCode").Value;
            astLesTask.FromPalletPosition = int.Parse(taskElement.Element("FromPalletPosition").Value, CultureInfo.InvariantCulture);
            if (astLesTask.FromPalletPosition < 1 || astLesTask.FromPalletPosition > 10)
                astLesTask.FromPalletPosition = 1;
            astLesTask.RequestTime = DateTime.Now;

            foreach (XElement taskItemElement in taskItemElements)
            {
                AST_LesTaskItem_PDA astLesTaskItem = new AST_LesTaskItem_PDA();
                astLesTaskItem.AST_LesTask_PDA = astLesTask;
                astLesTaskItem.BillDetailId = taskItemElement.Element("BillDtlID").Value;
                astLesTaskItem.MaterialCode = taskItemElement.Element("MaterialCode").Value;
                astLesTaskItem.MaterialName = taskItemElement.Element("MaterialName").Value;
                astLesTaskItem.MaterialBarcode = taskItemElement.Element("MaterialBarcode").Value;
                astLesTaskItem.ToPickQuantity = (int)decimal.Parse(taskItemElement.Element("NEED_PICK_NUM").Value, CultureInfo.InvariantCulture);
                astLesTaskItem.MaxQuantityInSingleCartPosition = (int)decimal.Parse(taskItemElement.Element("MaxQuantityInSingleCartPosition").Value, CultureInfo.InvariantCulture);
                if (astLesTaskItem.MaxQuantityInSingleCartPosition <= 0)
                    astLesTaskItem.MaxQuantityInSingleCartPosition = int.MaxValue;
                astLesTaskItem.IsSpecial = taskItemElement.Element("IsSpecial").Value == "1";
                astLesTaskItem.IsBig = taskItemElement.Element("STORETYPE").Value == "04";

                astLesTask.AST_LesTaskItem_PDAs.Add(astLesTaskItem);
            }

            return astLesTask;
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
            XElement taskElement = requestElement.Descendants("ASSEMBLE").First();
            List<XElement> taskItemElements = requestElement.Descendants("ASSEMBLEITEM").ToList();

            requestElement.RemoveNodes();
            responseElement.Add(taskElement);
            responseElement.Add(taskItemElements);

            taskElement.Add(new XElement("TYPE", successful ? "S" : "E"));
            taskElement.Add(new XElement("MESSAGE", errorMessage));

            return Converter.ToXml(xDocument);
        }
    }
}