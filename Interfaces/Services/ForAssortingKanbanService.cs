using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.Config;
using Interfaces.Entities;
using DataAccess.AssortingPDA;
using System.Data.Entity.Validation;
using Interfaces.Converters;

namespace Interfaces.Services
{
    /// <summary>
    /// 为分拣看板提供服务。
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class ForAssortingKanbanService : IForAssortingKanbanService
    {
        static readonly ForAssortingKanbanService instance = new ForAssortingKanbanService();

        /// <summary>
        /// 获取唯一实例。
        /// </summary>
        public static ForAssortingKanbanService Instance
        {
            get { return ForAssortingKanbanService.instance; }
        }

        ForAssortingKanbanService()
        { }

        /// <summary>
        /// 查询所有分拣口。
        /// </summary>
        public List<CFG_ChannelDto> QueryChannels()
        {
            using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
            {
                return (from ws in dbContext.CFG_Channels
                        select new CFG_ChannelDto
                        {
                            Id = ws.Id,
                            Code = ws.Code,
                            Name = ws.Name
                        })
                           .ToList();
            }
        }

        /// <summary>
        /// 按巷道查询今日统计。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        public AssortingKanbanTodayStatistics QueryTodayStatistics(int cfgChannelId)
        {
            AssortingKanbanTodayStatistics result = new AssortingKanbanTodayStatistics();

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    DateTime minTime = DateTime.Today;
                    DateTime maxTime = minTime.AddDays(1);

                    result.FinishedBatchCount = dbContext.AST_PalletResults
                                                    .Where(pr => pr.CFG_ChannelId == cfgChannelId && pr.EndPickTime > minTime && pr.EndPickTime < maxTime)
                                                    .Select(pr => pr.BatchCode)
                                                    .Distinct()
                                                    .Count();
                    result.TotalBatchCount = dbContext.AST_LesTasks
                                                 .Where(lt => lt.CFG_ChannelId == cfgChannelId && lt.RequestTime > minTime && lt.RequestTime < maxTime)
                                                 .Select(lt => lt.BatchCode)
                                                 .Distinct()
                                                 .Count();
                    result.FinishedPalletCount = dbContext.AST_PalletResults
                                                     .Where(pr => pr.CFG_ChannelId == cfgChannelId && pr.EndPickTime > minTime && pr.EndPickTime < maxTime)
                                                     .Select(pr => new { pr.BatchCode, pr.CFG_PalletId })
                                                     .Distinct()
                                                     .Count();
                    result.TotalPalletCount = dbContext.AST_LesTasks
                                                  .Where(lt => lt.CFG_ChannelId == cfgChannelId && lt.RequestTime > minTime && lt.RequestTime < maxTime)
                                                  .Select(lt => new { lt.BatchCode, lt.CFG_PalletId })
                                                  .Distinct()
                                                  .Count();
                    result.FinishedMaterialCount = dbContext.AST_PalletResultItems
                                                       .Where(pri => pri.AST_PalletResult.CFG_ChannelId == cfgChannelId && pri.AST_PalletResult.EndPickTime > minTime && pri.AST_PalletResult.EndPickTime < maxTime)
                                                       .Select(pri => (int?)pri.PickedQuantity)
                                                       .Sum()
                                                       .GetValueOrDefault();
                    result.TotalMaterialCount = dbContext.AST_LesTaskItems
                                                    .Where(lti => lti.AST_LesTask.CFG_ChannelId == cfgChannelId && lti.AST_LesTask.RequestTime > minTime && lti.AST_LesTask.RequestTime < maxTime)
                                                    .Select(lti => (int?)lti.ToPickQuantity)
                                                    .Sum()
                                                    .GetValueOrDefault();
                }
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

                Logger.Log("ForAssortingKanbanService.QueryTodayStatistics", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                            + message + Environment.NewLine
                                                            + Environment.NewLine);
            }

            return result;
        }

        /// <summary>
        /// 按巷道查询当前任务。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        public AssortingKanbanTaskInfo QueryCurrentTaskInfo(int cfgChannelId)
        {
            AssortingKanbanTaskInfo result = new AssortingKanbanTaskInfo();

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    AST_PalletTask astPalletTask = dbContext.AST_PalletTasks
                                                       .FirstOrDefault(pt => pt.CFG_ChannelId == cfgChannelId && pt.PickStatus != PickStatus.Finished);
                    //新增PDA拣料显示
                    if (astPalletTask == null)
                    {
                        AST_PalletTask lastPalletTask = dbContext.AST_PalletTasks.Where(t => t.CFG_ChannelId == cfgChannelId).OrderByDescending(t => t.CreateTime).FirstOrDefault();
                        DateTime dPtlPalletLastArriveTime = DateTime.MinValue;
                        if (lastPalletTask != null)
                        {
                            dPtlPalletLastArriveTime = lastPalletTask.CreateTime;
                        }

                        result = QueryPDACurrentTaskInfo(cfgChannelId, dPtlPalletLastArriveTime);
                        if (result != null)
                        {
                            return result;
                        }
                        result = new AssortingKanbanTaskInfo();
                    }

                    AST_CartTask astCartTask = dbContext.AST_CartTaskItems
                                                   .Where(cti => cti.AST_CartTask.CFG_ChannelId == cfgChannelId && cti.AssortingStatus != AssortingStatus.Finished)
                                                   .Select(cti => cti.AST_CartTask)
                                                   .FirstOrDefault();

                    string currentBatchCode;
                    if (astPalletTask == null)
                    {
                        currentBatchCode = dbContext.AST_LesTasks
                                               .Where(lt => lt.CFG_ChannelId == cfgChannelId && !lt.TaskGenerated)
                                               .Select(lt => lt.BatchCode)
                                               .FirstOrDefault();
                    }
                    else
                    {
                        currentBatchCode = astPalletTask.BatchCode;
                    }

                    if (!string.IsNullOrEmpty(currentBatchCode))
                    {
                        AST_LesTask currentBatchFirstAstLesTask = new AST_LesTask();
                        if (astPalletTask == null)
                        {
                            currentBatchFirstAstLesTask = dbContext.AST_LesTasks.First(lt => lt.BatchCode == currentBatchCode);
                        }
                        else
                        {
                            currentBatchFirstAstLesTask = dbContext.AST_LesTasks.First(lt => lt.BatchCode == currentBatchCode && lt.CFG_PalletId == astPalletTask.CFG_PalletId);
                        }

                        string projectCode = currentBatchFirstAstLesTask.ProjectCode;
                        string projectStep = currentBatchFirstAstLesTask.ProjectStep;

                        string[] codes = projectCode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] steps = projectStep.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (codes.Length > 1 && codes[0] == codes[1])
                            projectCode = codes[0];
                        if (steps.Length > 1 && steps[0] == steps[1])
                            projectStep = steps[0];

                        result.CurrentBatchInfo.PickType = "P"; //PTL料架拣料
                        //result.CurrentBatchInfo.PickType = 1; //PTL料架拣料
                        result.CurrentBatchInfo.ProjectCode = projectCode;
                        result.CurrentBatchInfo.ProjectStep = projectStep;
                        result.CurrentBatchInfo.BatchCode = currentBatchFirstAstLesTask.BatchCode;
                        result.CurrentBatchInfo.FinishedPalletCount = dbContext.AST_PalletResults
                                                                          .Count(pr => pr.CFG_ChannelId == cfgChannelId && pr.BatchCode == currentBatchCode);
                        result.CurrentBatchInfo.TotalPalletCount = dbContext.AST_LesTasks
                                                                       .Where(lt => lt.CFG_ChannelId == cfgChannelId && lt.BatchCode == currentBatchCode)
                                                                       .Select(lt => lt.CFG_PalletId)
                                                                       .Distinct()
                                                                       .Count();
                        //以下 4 个汇总界面不展示
                        result.CurrentBatchInfo.FinishedMaterialTypeCount = 0;
                        result.CurrentBatchInfo.TotalMaterialTypeCount = 0;
                        result.CurrentBatchInfo.FinishedMaterialCount = 0;
                        result.CurrentBatchInfo.TotalMaterialCount = 0;

                        List<CFG_ChannelCurrentCart> cfgChannelCurrentCarts = dbContext.CFG_ChannelCurrentCarts
                                                                                  .Where(ccc => ccc.CFG_ChannelId == cfgChannelId)
                                                                                  .OrderBy(ccc => ccc.Position)
                                                                                  .ToList();
                        foreach (CFG_ChannelCurrentCart cfgChannelCurrentCart in cfgChannelCurrentCarts)
                        {
                            CFG_ChannelCurrentCartDto cfgChannelCurrentCartDto = new CFG_ChannelCurrentCartDto();
                            cfgChannelCurrentCartDto.CFG_ChannelCurrentCartId = cfgChannelCurrentCart.Id;
                            cfgChannelCurrentCartDto.CFG_ChannelId = cfgChannelCurrentCart.CFG_ChannelId;
                            cfgChannelCurrentCartDto.Position = cfgChannelCurrentCart.Position;
                            cfgChannelCurrentCartDto.CFG_CartId = cfgChannelCurrentCart.CFG_CartId;
                            if (cfgChannelCurrentCart.CFG_Cart != null)
                            {
                                cfgChannelCurrentCartDto.CartCode = cfgChannelCurrentCart.CFG_Cart.Code;
                                cfgChannelCurrentCartDto.CartName = cfgChannelCurrentCart.CFG_Cart.Name;
                            }
                        }

                        if (astPalletTask != null)
                        {
                            AST_PalletTaskDto astPalletTaskDto = new AST_PalletTaskDto();
                            astPalletTaskDto.AST_PalletTaskId = astPalletTask.Id;
                            astPalletTaskDto.CFG_PalletId = astPalletTask.CFG_PalletId;
                            astPalletTaskDto.PalletCode = astPalletTask.CFG_Pallet.Code;
                            astPalletTaskDto.PalletType = astPalletTask.CFG_Pallet.PalletType;
                            astPalletTaskDto.PalletRotationStatus = astPalletTask.CFG_Pallet.PalletRotationStatus;

                            List<AST_PalletTaskItem> astPalletTaskItems = astPalletTask.AST_PalletTaskItems
                                                                              .OrderBy(pti => pti.FromPalletPosition)
                                                                              .ToList();
                            astPalletTaskItems.Sort(new PalletTaskSortComparer()); //正在拣选的排在最前面
                            foreach (AST_PalletTaskItem astPalletTaskItem in astPalletTaskItems)
                            {
                                AST_PalletTaskItemDto astPalletTaskItemDto = new AST_PalletTaskItemDto();
                                astPalletTaskItemDto.AST_PalletTaskItemId = astPalletTaskItem.Id;
                                astPalletTaskItemDto.FromPalletPosition = astPalletTaskItem.FromPalletPosition;
                                astPalletTaskItemDto.WorkStationCode = astPalletTaskItem.CFG_WorkStation.Code;
                                astPalletTaskItemDto.MaterialCode = astPalletTaskItem.MaterialCode;
                                astPalletTaskItemDto.MaterialName = astPalletTaskItem.MaterialName;
                                astPalletTaskItemDto.MaterialBarcode = astPalletTaskItem.MaterialBarcode;
                                astPalletTaskItemDto.ToPickQuantity = astPalletTaskItem.ToPickQuantity;
                                astPalletTaskItemDto.IsSpecial = astPalletTaskItem.IsSpecial;
                                astPalletTaskItemDto.IsBig = astPalletTaskItem.IsBig;
                                astPalletTaskItemDto.PickStatus = astPalletTaskItem.PickStatus;
                                astPalletTaskItemDto.PickedQuantity = astPalletTaskItem.PickedQuantity;

                                astPalletTaskDto.Items.Add(astPalletTaskItemDto);
                            }

                            result.CurrentPalletTask = astPalletTaskDto;
                        }

                        if (astCartTask != null)
                        {
                            AST_CartTaskDto astCartTaskDto = new AST_CartTaskDto();
                            astCartTaskDto.AST_CartTaskId = astCartTask.Id;
                            astCartTaskDto.CFG_CartId = astCartTask.CFG_CartId;
                            astCartTaskDto.CartCode = astCartTask.CFG_Cart.Code;
                            astCartTaskDto.CartName = astCartTask.CFG_Cart.Name;

                            List<AST_CartTaskItem> astCartTaskItems = astCartTask.AST_CartTaskItems
                                                                          .OrderBy(cti => cti.CartPosition)
                                                                          .ToList();
                            foreach (AST_CartTaskItem astCartTaskItem in astCartTaskItems)
                            {
                                AST_PalletTaskItem astPalletTaskItem = astCartTaskItem.AST_PalletTaskItem;

                                AST_CartTaskItemDto astCartTaskItemDto = new AST_CartTaskItemDto();
                                astCartTaskItemDto.AST_CartTaskItemId = astCartTaskItem.Id;
                                astCartTaskItemDto.CartPosition = astCartTaskItem.CartPosition;
                                astCartTaskItemDto.WorkStationCode = astCartTask.CFG_WorkStation.Code;
                                astCartTaskItemDto.MaterialCode = astPalletTaskItem.MaterialCode;
                                astCartTaskItemDto.MaterialName = astPalletTaskItem.MaterialName;
                                astCartTaskItemDto.MaterialBarcode = astPalletTaskItem.MaterialBarcode;
                                astCartTaskItemDto.MaxQuantityInSingleCartPosition = astPalletTaskItem.MaxQuantityInSingleCartPosition;
                                astCartTaskItemDto.IsSpecial = astPalletTaskItem.IsSpecial;
                                astCartTaskItemDto.IsBig = astPalletTaskItem.IsBig;
                                astCartTaskItemDto.AssortingStatus = astCartTaskItem.AssortingStatus;
                                astCartTaskItemDto.PickedQuantity = astCartTaskItem.AssortedQuantity;

                                astCartTaskDto.Items.Add(astCartTaskItemDto);
                            }

                            result.CurrentCartTask = astCartTaskDto;
                        }
                    }
                }
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

                Logger.Log("ForAssortingKanbanService.QueryCurrentTaskInfo", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                            + message + Environment.NewLine
                                                            + Environment.NewLine);
            }

            return result;
        }

        // <summary>
        /// 按巷道查询PDA今日统计。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        public AssortingKanbanTodayStatistics QueryPDATodayStatistics(int cfgChannelId)
        {
            AssortingKanbanTodayStatistics result = new AssortingKanbanTodayStatistics();

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    DateTime minTime = DateTime.Today;
                    DateTime maxTime = minTime.AddDays(1);

                    result.FinishedBatchCount = dbContext.AST_PalletTask_PDAs
                                                    .Where(pr => pr.CFG_ChannelId == cfgChannelId && pr.CreateTime > minTime && pr.CreateTime < maxTime && pr.PickStatus == PickStatus.Finished)
                                                    .Select(pr => pr.BatchCode)
                                                    .Distinct()
                                                    .Count();
                    result.TotalBatchCount = dbContext.AST_LesTask_PDAs
                                                 .Where(lt => lt.CFG_ChannelId == cfgChannelId && lt.RequestTime > minTime && lt.RequestTime < maxTime)
                                                 .Select(lt => lt.BatchCode)
                                                 .Distinct()
                                                 .Count();
                    result.FinishedPalletCount = dbContext.AST_PalletTask_PDAs
                                                     .Where(pr => pr.CFG_ChannelId == cfgChannelId && pr.CreateTime > minTime && pr.CreateTime < maxTime && pr.PickStatus == PickStatus.Finished)
                                                     .Select(pr => new { pr.BatchCode, pr.CFG_PalletId })
                                                     .Distinct()
                                                     .Count();
                    result.TotalPalletCount = dbContext.AST_LesTask_PDAs
                                                  .Where(lt => lt.CFG_ChannelId == cfgChannelId && lt.RequestTime > minTime && lt.RequestTime < maxTime)
                                                  .Select(lt => new { lt.BatchCode, lt.CFG_PalletId })
                                                  .Distinct()
                                                  .Count();
                    result.FinishedMaterialCount = dbContext.AST_PalletTaskItem_PDAs
                                                       .Where(pri => pri.AST_PalletTask_PDA.CFG_ChannelId == cfgChannelId && pri.AST_PalletTask_PDA.CreateTime > minTime && pri.AST_PalletTask_PDA.CreateTime < maxTime && pri.PickStatus == PickStatus.Finished)
                                                       .Select(pri => (int?)pri.PickedQuantity)
                                                       .Sum()
                                                       .GetValueOrDefault();
                    result.TotalMaterialCount = dbContext.AST_LesTaskItem_PDAs
                                                    .Where(lti => lti.AST_LesTask_PDA.CFG_ChannelId == cfgChannelId && lti.AST_LesTask_PDA.RequestTime > minTime && lti.AST_LesTask_PDA.RequestTime < maxTime)
                                                    .Select(lti => (int?)lti.ToPickQuantity)
                                                    .Sum()
                                                    .GetValueOrDefault();
                }
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

                Logger.Log("ForAssortingKanbanService.QueryPDATodayStatistics", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                            + message + Environment.NewLine
                                                            + Environment.NewLine);
            }

            return result;
        }

        /// <summary>
        /// 按巷道查询当前PDA任务。
        /// </summary>
        /// <param name="cfgChannelId">巷道的主键。</param>
        /// <param name="dPtlPalletLastArriveTime">PTL托盘最后到达时间。</param>
        private AssortingKanbanTaskInfo QueryPDACurrentTaskInfo(int cfgChannelId, DateTime dPtlPalletLastArriveTime)
        {
            AssortingKanbanTaskInfo result = new AssortingKanbanTaskInfo();

            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    DateTime minTime = DateTime.Today;
                    DateTime maxTime = minTime.AddDays(1);

                    AST_PalletTask_PDA astPalletTask = null;
                    if (dPtlPalletLastArriveTime == DateTime.MinValue)
                    {
                        astPalletTask = dbContext.AST_PalletTask_PDAs
                                                           .Where(pt => pt.CFG_ChannelId == cfgChannelId && pt.PickStatus != PickStatus.Finished && pt.CreateTime > minTime && pt.CreateTime < maxTime)
                                                           .OrderByDescending(pt => pt.Id)
                                                           .FirstOrDefault();
                    }
                    else
                    {
                        astPalletTask = dbContext.AST_PalletTask_PDAs
                                                       .Where(pt => pt.CFG_ChannelId == cfgChannelId && pt.PickStatus != PickStatus.Finished && pt.CreateTime > minTime && pt.CreateTime < maxTime
                                                           && pt.CreateTime > dPtlPalletLastArriveTime)
                                                       .OrderByDescending(pt => pt.Id)
                                                       .FirstOrDefault();
                    }

                    //AST_PalletTask_PDA astPalletTask = dbContext.AST_PalletTask_PDAs
                    //                                   .Where(pt => pt.CFG_ChannelId == cfgChannelId && pt.PickStatus != PickStatus.Finished && pt.CreateTime > minTime && pt.CreateTime < maxTime)
                    //                                   .OrderByDescending(pt => pt.Id)
                    //                                   .FirstOrDefault();

                    //AST_CartTask astCartTask = dbContext.AST_CartTaskItems
                    //                               .Where(cti => cti.AST_CartTask.CFG_ChannelId == cfgChannelId && cti.AssortingStatus != AssortingStatus.Finished)
                    //                               .Select(cti => cti.AST_CartTask)
                    //                               .FirstOrDefault();

                    if (astPalletTask == null)
                    {
                        result = null;
                        return result;
                    }

                    string currentBatchCode = astPalletTask.BatchCode;

                    if (!string.IsNullOrEmpty(currentBatchCode))
                    {
                        AST_LesTask_PDA currentBatchFirstAstLesTask = dbContext.AST_LesTask_PDAs
                                                                      .Where(lt => lt.CFG_ChannelId == cfgChannelId && lt.BatchCode == currentBatchCode && lt.CFG_PalletId == astPalletTask.CFG_PalletId)
                                                                      .OrderByDescending(lt => lt.Id).FirstOrDefault();

                        result.CurrentBatchInfo.PickType = ConverterPickTypeToText(currentBatchFirstAstLesTask.WbsId); //PDA手持机拣料
                        //result.CurrentBatchInfo.PickType = currentBatchFirstAstLesTask.WbsId; //PDA手持机拣料
                        //result.CurrentBatchInfo.PickType = 2; //PDA手持机拣料
                        result.CurrentBatchInfo.ProjectCode = currentBatchFirstAstLesTask.ProjectCode;
                        result.CurrentBatchInfo.ProjectStep = currentBatchFirstAstLesTask.ProjectStep;
                        result.CurrentBatchInfo.BatchCode = currentBatchFirstAstLesTask.BatchCode;
                        result.CurrentBatchInfo.FinishedPalletCount = dbContext.AST_PalletTask_PDAs
                                                                          .Count(pr => pr.CFG_ChannelId == cfgChannelId && pr.BatchCode == currentBatchCode && pr.PickStatus == PickStatus.Finished);
                        //result.CurrentBatchInfo.FinishedPalletCount = 0;
                        result.CurrentBatchInfo.TotalPalletCount = dbContext.AST_LesTask_PDAs
                                                                       .Where(lt => lt.CFG_ChannelId == cfgChannelId && lt.BatchCode == currentBatchCode)
                                                                       .Select(lt => lt.CFG_PalletId)
                                                                       .Distinct()
                                                                       .Count();
                        //以下 4 个汇总界面不展示
                        result.CurrentBatchInfo.FinishedMaterialTypeCount = 0;
                        result.CurrentBatchInfo.TotalMaterialTypeCount = 0;
                        result.CurrentBatchInfo.FinishedMaterialCount = 0;
                        result.CurrentBatchInfo.TotalMaterialCount = 0;

                        List<CFG_ChannelCurrentCart> cfgChannelCurrentCarts = dbContext.CFG_ChannelCurrentCarts
                                                                                  .Where(ccc => ccc.CFG_ChannelId == cfgChannelId)
                                                                                  .OrderBy(ccc => ccc.Position)
                                                                                  .ToList();
                        foreach (CFG_ChannelCurrentCart cfgChannelCurrentCart in cfgChannelCurrentCarts)
                        {
                            CFG_ChannelCurrentCartDto cfgChannelCurrentCartDto = new CFG_ChannelCurrentCartDto();
                            cfgChannelCurrentCartDto.CFG_ChannelCurrentCartId = cfgChannelCurrentCart.Id;
                            cfgChannelCurrentCartDto.CFG_ChannelId = cfgChannelCurrentCart.CFG_ChannelId;
                            cfgChannelCurrentCartDto.Position = cfgChannelCurrentCart.Position;
                            cfgChannelCurrentCartDto.CFG_CartId = cfgChannelCurrentCart.CFG_CartId;
                            if (cfgChannelCurrentCart.CFG_Cart != null)
                            {
                                cfgChannelCurrentCartDto.CartCode = cfgChannelCurrentCart.CFG_Cart.Code;
                                cfgChannelCurrentCartDto.CartName = cfgChannelCurrentCart.CFG_Cart.Name;
                            }
                        }

                        if (astPalletTask != null)
                        {
                            AST_PalletTaskDto astPalletTaskDto = new AST_PalletTaskDto();
                            astPalletTaskDto.AST_PalletTaskId = astPalletTask.Id;
                            astPalletTaskDto.CFG_PalletId = astPalletTask.CFG_PalletId;
                            astPalletTaskDto.PalletCode = astPalletTask.CFG_Pallet.Code;
                            astPalletTaskDto.PalletType = astPalletTask.CFG_Pallet.PalletType;
                            astPalletTaskDto.PalletRotationStatus = astPalletTask.CFG_Pallet.PalletRotationStatus;

                            List<AST_PalletTaskItem_PDA> astPalletTaskItems = astPalletTask.AST_PalletTaskItem_PDAs
                                                                              .OrderBy(pti => pti.FromPalletPosition)
                                                                              .ToList();
                            foreach (AST_PalletTaskItem_PDA astPalletTaskItem in astPalletTaskItems)
                            {
                                AST_PalletTaskItemDto astPalletTaskItemDto = new AST_PalletTaskItemDto();
                                astPalletTaskItemDto.AST_PalletTaskItemId = astPalletTaskItem.Id;
                                astPalletTaskItemDto.FromPalletPosition = astPalletTaskItem.FromPalletPosition;
                                astPalletTaskItemDto.WorkStationCode = astPalletTaskItem.CFG_WorkStation.Code;
                                astPalletTaskItemDto.MaterialCode = astPalletTaskItem.MaterialCode;
                                astPalletTaskItemDto.MaterialName = astPalletTaskItem.MaterialName;
                                astPalletTaskItemDto.MaterialBarcode = astPalletTaskItem.MaterialBarcode;
                                astPalletTaskItemDto.ToPickQuantity = astPalletTaskItem.ToPickQuantity;
                                astPalletTaskItemDto.IsSpecial = astPalletTaskItem.IsSpecial;
                                astPalletTaskItemDto.IsBig = astPalletTaskItem.IsBig;
                                astPalletTaskItemDto.PickStatus = astPalletTaskItem.PickStatus;
                                astPalletTaskItemDto.PickedQuantity = astPalletTaskItem.PickedQuantity;

                                astPalletTaskDto.Items.Add(astPalletTaskItemDto);
                            }

                            result.CurrentPalletTask = astPalletTaskDto;
                        }

                        //if (astCartTask != null)
                        //{
                        //    AST_CartTaskDto astCartTaskDto = new AST_CartTaskDto();
                        //    astCartTaskDto.AST_CartTaskId = astCartTask.Id;
                        //    astCartTaskDto.CFG_CartId = astCartTask.CFG_CartId;
                        //    astCartTaskDto.CartCode = astCartTask.CFG_Cart.Code;
                        //    astCartTaskDto.CartName = astCartTask.CFG_Cart.Name;

                        //    List<AST_CartTaskItem> astCartTaskItems = astCartTask.AST_CartTaskItems
                        //                                                  .OrderBy(cti => cti.CartPosition)
                        //                                                  .ToList();
                        //    foreach (AST_CartTaskItem astCartTaskItem in astCartTaskItems)
                        //    {
                        //        AST_PalletTaskItem astPalletTaskItem = astCartTaskItem.AST_PalletTaskItem;

                        //        AST_CartTaskItemDto astCartTaskItemDto = new AST_CartTaskItemDto();
                        //        astCartTaskItemDto.AST_CartTaskItemId = astCartTaskItem.Id;
                        //        astCartTaskItemDto.CartPosition = astCartTaskItem.CartPosition;
                        //        astCartTaskItemDto.WorkStationCode = astCartTask.CFG_WorkStation.Code;
                        //        astCartTaskItemDto.MaterialCode = astPalletTaskItem.MaterialCode;
                        //        astCartTaskItemDto.MaterialName = astPalletTaskItem.MaterialName;
                        //        astCartTaskItemDto.MaterialBarcode = astPalletTaskItem.MaterialBarcode;
                        //        astCartTaskItemDto.MaxQuantityInSingleCartPosition = astPalletTaskItem.MaxQuantityInSingleCartPosition;
                        //        astCartTaskItemDto.IsSpecial = astPalletTaskItem.IsSpecial;
                        //        astCartTaskItemDto.IsBig = astPalletTaskItem.IsBig;
                        //        astCartTaskItemDto.AssortingStatus = astCartTaskItem.AssortingStatus;
                        //        astCartTaskItemDto.PickedQuantity = astCartTaskItem.AssortedQuantity;

                        //        astCartTaskDto.Items.Add(astCartTaskItemDto);
                        //    }

                        //    result.CurrentCartTask = astCartTaskDto;
                        //}
                    }
                }
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

                Logger.Log("ForAssortingKanbanService.QueryPDACurrentTaskInfo", DateTime.Now.ToString("HH:mm:ss") + Environment.NewLine
                                                            + message + Environment.NewLine
                                                            + Environment.NewLine);
            }

            return result;
        }

        /// <summary>
        /// 拣料类型转换到其文本
        /// </summary>
        /// <param name="sPickType">拣料类型</param>
        /// <returns></returns>
        private string ConverterPickTypeToText(string sPickType)
        {
            string sPickTypeName = string.Empty;
            switch (sPickType)
            {
                case "N":
                    sPickTypeName = "PDA正常领料";
                    break;
                case "U":
                    sPickTypeName = "PDA异常领料";
                    break;
                case "T":
                    sPickTypeName = "PDA试验领料";
                    break;
                case "C":
                    sPickTypeName = "库存盘点";
                    break;
                case "R":
                    sPickTypeName = "退货出库";
                    break;
                case "L":
                    sPickTypeName = "借用出库";
                    break;
                case "E":
                    sPickTypeName = "空托出库";
                    break;
                case "P":
                    sPickTypeName = "合托出库";
                    break;
                case "B":
                    sPickTypeName = "合箱出库";
                    break;
                case "M":
                    sPickTypeName = "移库出库";
                    break;
                default:
                    sPickTypeName = string.Empty;
                    break;
            }
            return sPickTypeName;
        }
    }
}