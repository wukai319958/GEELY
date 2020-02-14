using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using AssemblyIndicating;
using DataAccess;
using DataAccess.AssemblyIndicating;
using Interfaces.Converters;
using Interfaces.Entities;
using DataAccess.Other;

namespace Interfaces.Services
{
    /// <summary>
    /// MES 到 PTL 的接口服务。
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class MesToPtlService : IMesToPtlService
    {
        static readonly MesToPtlService instance = new MesToPtlService();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static MesToPtlService Instance
        {
            get { return MesToPtlService.instance; }
        }

        MesToPtlService()
        { }

        /// <summary>
        /// 车身抵达工位。
        /// </summary>
        public string MesAssemblePTL(string xml)
        {
            try { XDocument.Parse(xml); }
            catch (Exception ex) { return "ERROR：" + ex.Message; }

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    ASM_AssembleIndication asmAssembleIndication = ASM_AssembleIndicationConverter.ConvertRequest(xml, dbContext);
                    if (asmAssembleIndication.ASM_AssembleIndicationItems.Count > 0
                        && !dbContext.ASM_AssembleIndications.Any(ai => ai.CFG_WorkStationId == asmAssembleIndication.CFG_WorkStationId
                                                                        && ai.GzzList == asmAssembleIndication.GzzList
                                                                        && ai.ProductSequence == asmAssembleIndication.ProductSequence))
                    {
                        ASM_AssembleIndicationMessage asmAssembleIndicationMessage = new ASM_AssembleIndicationMessage();
                        asmAssembleIndicationMessage.ASM_AssembleIndication = asmAssembleIndication;
                        asmAssembleIndicationMessage.ReceivedXml = xml;
                        asmAssembleIndicationMessage.ReceivedTime = DateTime.Now;

                        dbContext.ASM_AssembleIndications.Add(asmAssembleIndication);
                        dbContext.ASM_AssembleIndicationMessages.Add(asmAssembleIndicationMessage);

                        foreach (ASM_AssembleIndicationItem asmAssembleIndicationItem in asmAssembleIndication.ASM_AssembleIndicationItems)
                            dbContext.ASM_AssembleIndicationItems.Add(asmAssembleIndicationItem);

                        //装配任务生成
                        IndicatingTaskGenerator.Generate(asmAssembleIndication, dbContext);

                        dbContext.SaveChanges();
                    }
                }

                return ASM_AssembleIndicationConverter.ConvertResponse(xml, true, string.Empty);
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

                Logger.Log("MesAssemblePTL", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                             + xml + Environment.NewLine
                                             + message + Environment.NewLine
                                             + Environment.NewLine);

                return ASM_AssembleIndicationConverter.ConvertResponse(xml, false, message);
            }
        }


        public string FeedZonePTL(string xml)
        {
            try { XDocument.Parse(xml); }
            catch (Exception ex) { return "ERROR：" + ex.Message; }
            string message = string.Empty;
            bool result = false;
            try
            {
                FeedRecord model = FeedZonePTLConverter.ConvertRequest(xml);
                if (model != null)
                {
                    if ((!string.IsNullOrEmpty(model.PACKLINE)) && (!string.IsNullOrEmpty(model.PRDSEQ)))
                    {
                        //保存记录
                        try
                        {
                            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                            {
                                var regionAreaId = dbContext.CacheRegions.Select(x => x.AreaId).Distinct();
                                if (regionAreaId.Contains(model.PACKLINE))
                                {
                                    CacheRegionLightOrder cacheRegionOrder = new CacheRegionLightOrder()
                                    {
                                        AreaId = model.PACKLINE,
                                        MaterialId = model.PRDSEQ
                                    };
                                    dbContext.CacheRegionLightOrders.Add(cacheRegionOrder);
                                    dbContext.SaveChanges();
                                    result = true;
                                    message = "数据保存成功！";
                                }
                                else
                                {
                                    dbContext.FeedRecords.Add(model);
                                    dbContext.SaveChanges();
                                    result = true;
                                    message = "数据保存成功！";
                                }
                            }
                        }
                        catch
                        {
                            message = "数据保存失败！";
                        }
                    }
                    else
                    {
                        message = "分装线码或样车码为空！";
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return FeedZonePTLConverter.ConvertResponse(xml, result, message);
        }







        #region AssemblingPTL
        public string AssemblingPTL(string xml)
        {
            try { XDocument.Parse(xml); }
            catch (Exception ex) { return "ERROR：" + ex.Message; }

            string message = string.Empty;
            bool result = false;

            try
            {
                AssemblyLightOrder model = AssemblingPTLConverter.ConvertRequest(xml);
                if (model != null)
                {
                    if (!string.IsNullOrEmpty(model.MaterialId))
                    {
                        try
                        {
                            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                            {
                                dbContext.AssemblyLightOrders.Add(model);

                                dbContext.SaveChanges();

                                result = true;
                                message = string.Empty;
                            }
                        }
                        catch (Exception)
                        {
                            message = "数据发送失败！";
                        }
                    }
                    else
                    {
                        message = "样车码为空！";
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;

            }
            return AssemblingPTLConverter.ConvertResponse(xml, result, message);

        }
        #endregion
    }
}