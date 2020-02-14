using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using Assorting;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.Config;
using Interfaces.Converters;
using DataAccess.AssortingPDA;
using Interfaces.Entities;
using System.Collections.Generic;
using System.Threading;

namespace Interfaces.Services
{
    /// <summary>
    /// LES 到 PTL 的接口服务。
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class LesToPtlService : ILesToPtlService
    {
        static readonly LesToPtlService instance = new LesToPtlService();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static LesToPtlService Instance
        {
            get { return LesToPtlService.instance; }
        }

        LesToPtlService()
        { }

        /// <summary>
        /// 提前下发任务。
        /// </summary>
        public string LesStockPickPTL(string xml)
        {
            try { XDocument.Parse(xml); }
            catch (Exception ex) { return "ERROR：" + ex.Message; }

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    AST_LesTask astLesTask = AST_LesTaskConverter.ConvertRequest(xml, dbContext);
                    if (!dbContext.AST_LesTasks.Any(lt => lt.BillCode == astLesTask.BillCode))
                    {
                        AST_LesTaskMessage astLesTaskMessage = new AST_LesTaskMessage();
                        astLesTaskMessage.AST_LesTask = astLesTask;
                        astLesTaskMessage.ReceivedXml = xml;
                        astLesTaskMessage.ReceivedTime = DateTime.Now;

                        dbContext.AST_LesTasks.Add(astLesTask);
                        dbContext.AST_LesTaskMessages.Add(astLesTaskMessage);

                        foreach (AST_LesTaskItem astLesTaskItem in astLesTask.AST_LesTaskItems)
                            dbContext.AST_LesTaskItems.Add(astLesTaskItem);

                        dbContext.SaveChanges();
                    }
                }

                return AST_LesTaskConverter.ConvertResponse(xml, true, string.Empty);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                DbEntityValidationException dbEntityValidationException = ex as DbEntityValidationException;
                if (dbEntityValidationException != null)
                {
                    foreach (DbEntityValidationResult validationResult in dbEntityValidationException.EntityValidationErrors)
                    {
                        foreach (DbValidationError validationError in validationResult.ValidationErrors)
                            message += Environment.NewLine + validationError.ErrorMessage;
                    }
                }
                message += Environment.NewLine + ex.StackTrace;

                Logger.Log("LesStockPickPTL", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine +
                                             xml + Environment.NewLine
                                             + message + Environment.NewLine
                                             + Environment.NewLine);

                return AST_LesTaskConverter.ConvertResponse(xml, false, message);
            }
        }

        /// <summary>
        /// 托盘抵达分拣口。
        /// </summary>
        public string LesSendBoxPTL(string xml)
        {
            try { XDocument.Parse(xml); }
            catch (Exception ex) { return "ERROR：" + ex.Message; }

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    AST_PalletArrived astPalletArrived = AST_PalletArrivedConverter.ConvertRequest(xml, dbContext);
                    if (!dbContext.AST_PalletArriveds.Any(pa => pa.BatchCode == astPalletArrived.BatchCode
                                                                && pa.CFG_PalletId == astPalletArrived.CFG_PalletId
                                                                && pa.PickBillIds == astPalletArrived.PickBillIds))
                    {
                        AST_PalletArrivedMessage astPalletArrivedMessage = new AST_PalletArrivedMessage();
                        astPalletArrivedMessage.AST_PalletArrived = astPalletArrived;
                        astPalletArrivedMessage.ReceivedXml = xml;
                        astPalletArrivedMessage.ReceivedTime = DateTime.Now;

                        dbContext.AST_PalletArriveds.Add(astPalletArrived);
                        dbContext.AST_PalletArrivedMessages.Add(astPalletArrivedMessage);

                        //托盘到达时按托合并任务
                        CFG_ChannelCurrentPallet cfgChannelCurrentPallet = dbContext.CFG_ChannelCurrentPallets
                                                                               .First(ccp => ccp.CFG_ChannelId == astPalletArrived.CFG_ChannelId);
                        cfgChannelCurrentPallet.CFG_PalletId = astPalletArrived.CFG_PalletId;
                        cfgChannelCurrentPallet.ArrivedTime = astPalletArrived.ArrivedTime;

                        CFG_Pallet cfgPallet = astPalletArrived.CFG_Pallet;
                        cfgPallet.PalletRotationStatus = PalletRotationStatus.Normal;

                        //按托任务生成
                        AssortingTaskGenerator.Generate(astPalletArrived, dbContext);

                        dbContext.SaveChanges();
                    }
                }

                return AST_PalletArrivedConverter.ConvertResponse(xml, true, string.Empty);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                DbEntityValidationException dbEntityValidationException = ex as DbEntityValidationException;
                if (dbEntityValidationException != null)
                {
                    foreach (DbEntityValidationResult validationResult in dbEntityValidationException.EntityValidationErrors)
                    {
                        foreach (DbValidationError validationError in validationResult.ValidationErrors)
                            message += Environment.NewLine + validationError.ErrorMessage;
                    }
                }
                message += Environment.NewLine + ex.StackTrace;

                Logger.Log("LesSendBoxPTL", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine +
                                             xml + Environment.NewLine
                                             + message + Environment.NewLine
                                             + Environment.NewLine);
                return AST_PalletArrivedConverter.ConvertResponse(xml, false, message);
            }
        }

        /// <summary>
        /// 提前下发PDA拣料任务
        /// </summary>
        public string LesStockPickPDA(string xml)
        {
            try { XDocument.Parse(xml); }
            catch (Exception ex) { return "ERROR：" + ex.Message; }

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    AST_LesTask_PDA astLesTask = AST_LesTaskConverter.ConvertRequestPDA(xml, dbContext);
                    if (!dbContext.AST_LesTask_PDAs.Any(lt => lt.BillCode == astLesTask.BillCode))
                    {
                        AST_LesTaskMessage_PDA astLesTaskMessage = new AST_LesTaskMessage_PDA();
                        astLesTaskMessage.AST_LesTask_PDA = astLesTask;
                        astLesTaskMessage.ReceivedXml = xml;
                        astLesTaskMessage.ReceivedTime = DateTime.Now;

                        dbContext.AST_LesTask_PDAs.Add(astLesTask);
                        dbContext.AST_LesTaskMessage_PDAs.Add(astLesTaskMessage);

                        foreach (AST_LesTaskItem_PDA astLesTaskItem in astLesTask.AST_LesTaskItem_PDAs)
                            dbContext.AST_LesTaskItem_PDAs.Add(astLesTaskItem);

                        dbContext.SaveChanges();
                    }
                }

                return AST_LesTaskConverter.ConvertResponse(xml, true, string.Empty);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                DbEntityValidationException dbEntityValidationException = ex as DbEntityValidationException;
                if (dbEntityValidationException != null)
                {
                    foreach (DbEntityValidationResult validationResult in dbEntityValidationException.EntityValidationErrors)
                    {
                        foreach (DbValidationError validationError in validationResult.ValidationErrors)
                            message += Environment.NewLine + validationError.ErrorMessage;
                    }
                }
                message += Environment.NewLine + ex.StackTrace;

                Logger.Log("LesStockPickPDA", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine +
                                             xml + Environment.NewLine
                                             + message + Environment.NewLine
                                             + Environment.NewLine);

                return AST_LesTaskConverter.ConvertResponse(xml, false, message);
            }
        }

        /// <summary>
        /// PDA拣料托盘抵达分拣口
        /// </summary>
        public string LesSendBoxPDA(string xml)
        {
            try { XDocument.Parse(xml); }
            catch (Exception ex) { return "ERROR：" + ex.Message; }

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    AST_PalletArrived_PDA astPalletArrived = AST_PalletArrivedConverter.ConvertRequestPDA(xml, dbContext);
                    if (!dbContext.AST_PalletArrived_PDAs.Any(pa => pa.BatchCode == astPalletArrived.BatchCode
                                                                && pa.CFG_PalletId == astPalletArrived.CFG_PalletId
                                                                && pa.PickBillIds == astPalletArrived.PickBillIds))
                    {
                        AST_PalletArrivedMessage_PDA astPalletArrivedMessage = new AST_PalletArrivedMessage_PDA();
                        astPalletArrivedMessage.AST_PalletArrived_PDA = astPalletArrived;
                        astPalletArrivedMessage.ReceivedXml = xml;
                        astPalletArrivedMessage.ReceivedTime = DateTime.Now;

                        dbContext.AST_PalletArrived_PDAs.Add(astPalletArrived);
                        dbContext.AST_PalletArrivedMessage_PDAs.Add(astPalletArrivedMessage);

                        //托盘到达时按托合并任务
                        CFG_ChannelCurrentPallet cfgChannelCurrentPallet = dbContext.CFG_ChannelCurrentPallets
                                                                               .First(ccp => ccp.CFG_ChannelId == astPalletArrived.CFG_ChannelId);
                        cfgChannelCurrentPallet.CFG_PalletId = astPalletArrived.CFG_PalletId;
                        cfgChannelCurrentPallet.ArrivedTime = astPalletArrived.ArrivedTime;

                        CFG_Pallet cfgPallet = astPalletArrived.CFG_Pallet;
                        cfgPallet.PalletRotationStatus = PalletRotationStatus.Normal;

                        //按托任务生成
                        AssortingTaskGenerator.GeneratePDA(astPalletArrived, dbContext);

                        dbContext.SaveChanges();
                    }
                }

                return AST_PalletArrivedConverter.ConvertResponse(xml, true, string.Empty);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                DbEntityValidationException dbEntityValidationException = ex as DbEntityValidationException;
                if (dbEntityValidationException != null)
                {
                    foreach (DbEntityValidationResult validationResult in dbEntityValidationException.EntityValidationErrors)
                    {
                        foreach (DbValidationError validationError in validationResult.ValidationErrors)
                            message += Environment.NewLine + validationError.ErrorMessage;
                    }
                }
                message += Environment.NewLine + ex.StackTrace;

                Logger.Log("LesSendBoxPDA", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine +
                                             xml + Environment.NewLine
                                             + message + Environment.NewLine
                                             + Environment.NewLine);
                return AST_PalletArrivedConverter.ConvertResponse(xml, false, message);
            }
        }

        #region 注释不用方法
        /// <summary>
        /// PDA发送拣选任务完成
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        //public string LesSendFinishStatusPDA(string xml)
        //{
        //    string result = xml;
        //    try { XDocument.Parse(xml); }
        //    catch (Exception ex) { return "ERROR：" + ex.Message; }

        //    try
        //    {
        //        AST_LesTask_PDA_Finish model = AST_LesTask_PDA_FinishConverter.ConvertRequestPDA(xml);
        //        using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
        //        {
        //            List<AST_LesTask_PDA> targets = dbContext.AST_LesTask_PDAs.Where(x => model.BILLID.Contains(x.BillCode)).ToList();
        //            foreach (var target in targets)
        //            {
        //                target.IsFinished = true;
        //            }

        //            dbContext.SaveChanges();
        //        }

        //        XDocument xDocument = XDocument.Parse(xml);
        //        XElement serviceElement = xDocument.Descendants("Service").First();
        //        XElement dataElement = serviceElement.Descendants("Data").First();
        //        XElement requestElement = dataElement.Descendants("Request").First();
        //        XElement arrivedElement = requestElement.Descendants("ASSEMBLE").First();
        //        XElement type = new XElement("TYPE");
        //        type.Value = "succ";
        //        XElement message = new XElement("MESSAGE");
        //        message.Value = "接收成功！";

        //        arrivedElement.Add(type);
        //        arrivedElement.Add(message);

        //        result = Converter.ToXml(xDocument);
        //    }
        //    catch
        //    {
        //        XDocument xDocument = XDocument.Parse(xml);
        //        XElement serviceElement = xDocument.Descendants("Service").First();
        //        XElement dataElement = serviceElement.Descendants("Data").First();
        //        XElement requestElement = dataElement.Descendants("Request").First();
        //        XElement arrivedElement = requestElement.Descendants("ASSEMBLE").First();
        //        XElement type = new XElement("TYPE");
        //        type.Value = "error";
        //        XElement message = new XElement("MESSAGE");
        //        message.Value = "接收失败！";

        //        arrivedElement.Add(type);
        //        arrivedElement.Add(message);

        //        result = Converter.ToXml(xDocument);
        //    }

        //    return result;
        //}
        #endregion

        /// <summary>
        /// PDA发送拣选任务完成
        /// </summary>
        /// <param name="xml">报文</param>
        /// <returns></returns>
        public string LesSendFinishStatusPDA(string xml)
        {
            string result = xml;
            try { XDocument.Parse(xml); }
            catch (Exception ex) { return "ERROR：" + ex.Message; }

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    List<AST_PalletPickResult_PDA> astPalletPickResults = AST_PalletPickResult_PDAConverter.ConvertRequestPDA(xml, dbContext);
                    if (astPalletPickResults.Count > 0)
                    {
                        string sReqCode = GetReqCode();

                        AST_PalletPickResultMessage_PDA astPalletPickResultMessage = new AST_PalletPickResultMessage_PDA();
                        astPalletPickResultMessage.AST_PalletPickResultKey = sReqCode;
                        astPalletPickResultMessage.ReceivedXml = xml;
                        astPalletPickResultMessage.ReceivedTime = DateTime.Now;
                        dbContext.AST_PalletPickResultMessage_PDAs.Add(astPalletPickResultMessage);

                        foreach (AST_PalletPickResult_PDA astPalletPickResult in astPalletPickResults)
                        {
                            if (!dbContext.AST_PalletPickResult_PDAs.Any(pa => pa.BatchCode == astPalletPickResult.BatchCode
                                                                        && pa.CFG_PalletId == astPalletPickResult.CFG_PalletId
                                                                        && pa.PickBillIds == astPalletPickResult.PickBillIds))
                            {
                                astPalletPickResult.AST_PalletPickResultKey = sReqCode;
                                dbContext.AST_PalletPickResult_PDAs.Add(astPalletPickResult);

                                //结束LES拣料任务
                                List<AST_LesTask_PDA> targets = dbContext.AST_LesTask_PDAs.Where(x => astPalletPickResult.PickBillIds.Contains(x.BillCode)).ToList();
                                foreach (var target in targets)
                                {
                                    target.IsFinished = true;
                                }

                                //结束托盘拣料任务及其明细
                                List<AST_PalletTaskItem_PDA> astPalletTaskItems = dbContext.AST_PalletTaskItem_PDAs
                                    .Where(t => t.AST_PalletTask_PDA.BatchCode.Equals(astPalletPickResult.BatchCode)
                                    && t.AST_PalletTask_PDA.CFG_PalletId == astPalletPickResult.CFG_PalletId
                                    && t.BoxCode.Equals(astPalletPickResult.BoxCode)
                                    && t.PickStatus != PickStatus.Finished).ToList();
                                if (astPalletTaskItems.Count > 0)
                                {
                                    foreach (AST_PalletTaskItem_PDA astPalletTaskItem in astPalletTaskItems)
                                    {
                                        if (astPalletTaskItem.PickedQuantity == null)
                                        {
                                            astPalletTaskItem.PickedQuantity = 0;
                                        }
                                        astPalletTaskItem.PickedQuantity = Convert.ToInt32(astPalletTaskItem.PickedQuantity) + astPalletPickResult.Quantity;
                                        astPalletTaskItem.PickedTime = astPalletPickResult.ReceivedTime;
                                        astPalletTaskItem.PickStatus = astPalletTaskItem.PickedQuantity >= astPalletTaskItem.ToPickQuantity ? PickStatus.Finished : PickStatus.Picking;
                                    }
                                    AST_PalletTask_PDA astPalletTask = astPalletTaskItems.FirstOrDefault().AST_PalletTask_PDA;
                                    if (astPalletTask.AST_PalletTaskItem_PDAs.All(t => t.PickStatus == PickStatus.Finished))
                                    {
                                        astPalletTask.PickStatus = PickStatus.Finished;
                                    }
                                }
                            }
                        }

                        dbContext.SaveChanges();
                    }
                }

                return AST_PalletPickResult_PDAConverter.ConvertResponse(xml, true, string.Empty);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (message.Contains("无效的托盘编码"))
                {
                    return AST_PalletPickResult_PDAConverter.ConvertResponse(xml, true, string.Empty);
                }
                else
                {
                    DbEntityValidationException dbEntityValidationException = ex as DbEntityValidationException;
                    if (dbEntityValidationException != null)
                    {
                        foreach (DbEntityValidationResult validationResult in dbEntityValidationException.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationResult.ValidationErrors)
                                message += Environment.NewLine + validationError.ErrorMessage;
                        }
                    }
                    message += Environment.NewLine + ex.StackTrace;

                    Logger.Log("LesSendFinishStatusPDA", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine +
                                                 xml + Environment.NewLine
                                                 + message + Environment.NewLine
                                                 + Environment.NewLine);
                    return AST_PalletPickResult_PDAConverter.ConvertResponse(xml, false, message);
                }
            }
        }

        /// <summary>
        /// 获取请求编号
        /// </summary>
        /// <returns></returns>
        private string GetReqCode()
        {
            Thread.Sleep(5);
            return DateTime.Now.ToString("yyMMddHHmmssfff");
        }
    }
}