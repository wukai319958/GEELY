using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using DataAccess.Assorting;
using DataAccess.AssortingPDA;

namespace Assorting
{
    /// <summary>
    /// 在托盘抵达时，按托合并原始分拣任务。
    /// </summary>
    public static class AssortingTaskGenerator
    {
        /// <summary>
        /// 按托合并原始分拣任务。
        /// </summary>
        /// <param name="astPalletArrived">从接口解析还未持久化的托盘抵达记录。</param>
        /// <param name="dbContext">数据上下文。</param>
        public static void Generate(AST_PalletArrived astPalletArrived, GeelyPtlEntities dbContext)
        {
            List<AST_LesTask> astLesTasks = dbContext.AST_LesTasks
                                                .Where(lt => lt.BatchCode == astPalletArrived.BatchCode
                                                             && lt.CFG_PalletId == astPalletArrived.CFG_PalletId
                                                             && lt.CFG_ChannelId == astPalletArrived.CFG_ChannelId
                                                             && astPalletArrived.PickBillIds.Contains(lt.BillCode)
                                                             && !lt.TaskGenerated)
                                                .ToList();
            if (astLesTasks.Count > 0)
            {
                AST_LesTask mainAstLesTask = astLesTasks.First();

                AST_PalletTask astPalletTask = new AST_PalletTask();
                astPalletTask.CFG_PalletId = astPalletArrived.CFG_PalletId;
                astPalletTask.BatchCode = mainAstLesTask.BatchCode;
                astPalletTask.PickBillIds = astPalletArrived.PickBillIds;
                astPalletTask.ProjectCode = astPalletArrived.ProjectCode;
                astPalletTask.WbsId = mainAstLesTask.WbsId;
                astPalletTask.ProjectStep = astPalletArrived.ProjectStep;
                astPalletTask.CFG_ChannelId = astPalletArrived.CFG_ChannelId;
                astPalletTask.PickStatus = PickStatus.New;
                astPalletTask.CreateTime = DateTime.Now;

                dbContext.AST_PalletTasks.Add(astPalletTask);

                //提取当前目标工位、当前批次、当前巷道、当前托盘未合并的原始任务
                List<int> cfgWorkStationIds = astLesTasks
                                                  .Select(lt => lt.CFG_WorkStationId)
                                                  .Distinct()
                                                  .ToList();
                foreach (int cfgWorkStationId in cfgWorkStationIds)
                {
                    ILookup<int, AST_LesTask> astLesTaskLookupByFromPalletPosition = astLesTasks
                                                                                         .Where(lt => lt.CFG_WorkStationId == cfgWorkStationId)
                                                                                         .OrderBy(lt => lt.FromPalletPosition)
                                                                                         .ToLookup(lt => lt.FromPalletPosition);

                    //明细的合并，特殊件单独拣
                    foreach (IGrouping<int, AST_LesTask> astLesTaskGroupingByFromPalletPosition in astLesTaskLookupByFromPalletPosition)
                    {
                        int fromPalletPosition = astLesTaskGroupingByFromPalletPosition.Key;
                        AST_LesTask mainAstLesTaskByWorkStation = astLesTaskGroupingByFromPalletPosition.First();

                        List<AST_LesTaskItem> normalAstLesTaskItems = new List<AST_LesTaskItem>();
                        List<AST_LesTaskItem> specialAstLesTaskItems = new List<AST_LesTaskItem>();

                        foreach (AST_LesTask astLesTask in astLesTaskGroupingByFromPalletPosition)
                        {
                            normalAstLesTaskItems.AddRange(astLesTask.AST_LesTaskItems.Where(lti => !lti.IsSpecial));
                            specialAstLesTaskItems.AddRange(astLesTask.AST_LesTaskItems.Where(lti => lti.IsSpecial));
                        }

                        //普通件
                        if (normalAstLesTaskItems.Count > 0)
                        {
                            AST_LesTaskItem mainAstLesTaskItem = normalAstLesTaskItems.First();
                            int totalNormalQuantity = normalAstLesTaskItems.Sum(lti => lti.ToPickQuantity);

                            AST_PalletTaskItem astPalletTaskItem = new AST_PalletTaskItem();
                            astPalletTaskItem.AST_PalletTask = astPalletTask;
                            astPalletTaskItem.CFG_WorkStationId = cfgWorkStationId;
                            astPalletTaskItem.BoxCode = mainAstLesTaskByWorkStation.BoxCode;
                            astPalletTaskItem.FromPalletPosition = fromPalletPosition;
                            astPalletTaskItem.MaterialCode = mainAstLesTaskItem.MaterialCode;
                            astPalletTaskItem.MaterialName = mainAstLesTaskItem.MaterialName;
                            astPalletTaskItem.MaterialBarcode = mainAstLesTaskItem.MaterialBarcode;
                            astPalletTaskItem.ToPickQuantity = totalNormalQuantity;
                            astPalletTaskItem.MaxQuantityInSingleCartPosition = mainAstLesTaskItem.MaxQuantityInSingleCartPosition;
                            astPalletTaskItem.IsSpecial = false;
                            astPalletTaskItem.IsBig = mainAstLesTaskItem.IsBig;
                            astPalletTaskItem.PickStatus = PickStatus.New;

                            dbContext.AST_PalletTaskItems.Add(astPalletTaskItem);

                            foreach (AST_LesTaskItem normalAstLesTaskItem in normalAstLesTaskItems)
                                normalAstLesTaskItem.AST_PalletTaskItem = astPalletTaskItem;
                        }

                        //特殊件
                        foreach (AST_LesTaskItem astLesTaskItem in specialAstLesTaskItems)
                        {
                            AST_PalletTaskItem astPalletTaskItem = new AST_PalletTaskItem();
                            astPalletTaskItem.AST_PalletTask = astPalletTask;
                            astPalletTaskItem.CFG_WorkStationId = cfgWorkStationId;
                            astPalletTaskItem.BoxCode = mainAstLesTaskByWorkStation.BoxCode;
                            astPalletTaskItem.FromPalletPosition = fromPalletPosition;
                            astPalletTaskItem.MaterialCode = astLesTaskItem.MaterialCode;
                            astPalletTaskItem.MaterialName = astLesTaskItem.MaterialName;
                            astPalletTaskItem.MaterialBarcode = astLesTaskItem.MaterialBarcode;
                            astPalletTaskItem.ToPickQuantity = astLesTaskItem.ToPickQuantity;
                            astPalletTaskItem.MaxQuantityInSingleCartPosition = astLesTaskItem.MaxQuantityInSingleCartPosition;
                            astPalletTaskItem.IsSpecial = true;
                            astPalletTaskItem.IsBig = astLesTaskItem.IsBig;
                            astPalletTaskItem.PickStatus = PickStatus.New;

                            dbContext.AST_PalletTaskItems.Add(astPalletTaskItem);

                            astLesTaskItem.AST_PalletTaskItem = astPalletTaskItem;
                        }

                        //标记已合并按托任务
                        foreach (AST_LesTask astLesTask in astLesTaskGroupingByFromPalletPosition)
                            astLesTask.TaskGenerated = true;
                    }
                }
            }
        }

        /// <summary>
        /// 按托合并原始分拣任务-PDA。
        /// </summary>
        /// <param name="astPalletArrived">从接口解析还未持久化的托盘抵达记录。</param>
        /// <param name="dbContext">数据上下文。</param>
        public static void GeneratePDA(AST_PalletArrived_PDA astPalletArrived, GeelyPtlEntities dbContext)
        {
            List<AST_LesTask_PDA> astLesTasks = dbContext.AST_LesTask_PDAs
                                                .Where(lt => lt.BatchCode == astPalletArrived.BatchCode
                                                             && lt.CFG_PalletId == astPalletArrived.CFG_PalletId
                                                             && lt.CFG_ChannelId == astPalletArrived.CFG_ChannelId
                                                             && astPalletArrived.PickBillIds.Contains(lt.BillCode)
                                                             && !lt.TaskGenerated)
                                                .ToList();
            if (astLesTasks.Count > 0)
            {
                AST_LesTask_PDA mainAstLesTask = astLesTasks.First();

                AST_PalletTask_PDA astPalletTask = new AST_PalletTask_PDA();
                astPalletTask.CFG_PalletId = astPalletArrived.CFG_PalletId;
                astPalletTask.BatchCode = mainAstLesTask.BatchCode;
                astPalletTask.PickBillIds = astPalletArrived.PickBillIds;
                astPalletTask.ProjectCode = astPalletArrived.ProjectCode;
                astPalletTask.WbsId = mainAstLesTask.WbsId;
                astPalletTask.ProjectStep = astPalletArrived.ProjectStep;
                astPalletTask.CFG_ChannelId = astPalletArrived.CFG_ChannelId;
                astPalletTask.PickStatus = PickStatus.New;
                astPalletTask.CreateTime = DateTime.Now;

                dbContext.AST_PalletTask_PDAs.Add(astPalletTask);

                //提取当前目标工位、当前批次、当前巷道、当前托盘未合并的原始任务
                List<int> cfgWorkStationIds = astLesTasks
                                                  .Select(lt => lt.CFG_WorkStationId)
                                                  .Distinct()
                                                  .ToList();
                foreach (int cfgWorkStationId in cfgWorkStationIds)
                {
                    ILookup<int, AST_LesTask_PDA> astLesTaskLookupByFromPalletPosition = astLesTasks
                                                                                         .Where(lt => lt.CFG_WorkStationId == cfgWorkStationId)
                                                                                         .OrderBy(lt => lt.FromPalletPosition)
                                                                                         .ToLookup(lt => lt.FromPalletPosition);

                    //明细的合并，特殊件单独拣
                    foreach (IGrouping<int, AST_LesTask_PDA> astLesTaskGroupingByFromPalletPosition in astLesTaskLookupByFromPalletPosition)
                    {
                        int fromPalletPosition = astLesTaskGroupingByFromPalletPosition.Key;
                        AST_LesTask_PDA mainAstLesTaskByWorkStation = astLesTaskGroupingByFromPalletPosition.First();

                        List<AST_LesTaskItem_PDA> normalAstLesTaskItems = new List<AST_LesTaskItem_PDA>();
                        List<AST_LesTaskItem_PDA> specialAstLesTaskItems = new List<AST_LesTaskItem_PDA>();

                        foreach (AST_LesTask_PDA astLesTask in astLesTaskGroupingByFromPalletPosition)
                        {
                            normalAstLesTaskItems.AddRange(astLesTask.AST_LesTaskItem_PDAs.Where(lti => !lti.IsSpecial));
                            specialAstLesTaskItems.AddRange(astLesTask.AST_LesTaskItem_PDAs.Where(lti => lti.IsSpecial));
                        }

                        //普通件
                        if (normalAstLesTaskItems.Count > 0)
                        {
                            AST_LesTaskItem_PDA mainAstLesTaskItem = normalAstLesTaskItems.First();
                            int totalNormalQuantity = normalAstLesTaskItems.Sum(lti => lti.ToPickQuantity);

                            AST_PalletTaskItem_PDA astPalletTaskItem = new AST_PalletTaskItem_PDA();
                            astPalletTaskItem.AST_PalletTask_PDA = astPalletTask;
                            astPalletTaskItem.CFG_WorkStationId = cfgWorkStationId;
                            astPalletTaskItem.BoxCode = mainAstLesTaskByWorkStation.BoxCode;
                            astPalletTaskItem.FromPalletPosition = fromPalletPosition;
                            astPalletTaskItem.MaterialCode = mainAstLesTaskItem.MaterialCode;
                            astPalletTaskItem.MaterialName = mainAstLesTaskItem.MaterialName;
                            astPalletTaskItem.MaterialBarcode = mainAstLesTaskItem.MaterialBarcode;
                            astPalletTaskItem.ToPickQuantity = totalNormalQuantity;
                            astPalletTaskItem.MaxQuantityInSingleCartPosition = mainAstLesTaskItem.MaxQuantityInSingleCartPosition;
                            astPalletTaskItem.IsSpecial = false;
                            astPalletTaskItem.IsBig = mainAstLesTaskItem.IsBig;
                            astPalletTaskItem.PickStatus = PickStatus.New;

                            dbContext.AST_PalletTaskItem_PDAs.Add(astPalletTaskItem);

                            foreach (AST_LesTaskItem_PDA normalAstLesTaskItem in normalAstLesTaskItems)
                                normalAstLesTaskItem.AST_PalletTaskItem_PDA = astPalletTaskItem;
                        }

                        //特殊件
                        foreach (AST_LesTaskItem_PDA astLesTaskItem in specialAstLesTaskItems)
                        {
                            AST_PalletTaskItem_PDA astPalletTaskItem = new AST_PalletTaskItem_PDA();
                            astPalletTaskItem.AST_PalletTask_PDA = astPalletTask;
                            astPalletTaskItem.CFG_WorkStationId = cfgWorkStationId;
                            astPalletTaskItem.BoxCode = mainAstLesTaskByWorkStation.BoxCode;
                            astPalletTaskItem.FromPalletPosition = fromPalletPosition;
                            astPalletTaskItem.MaterialCode = astLesTaskItem.MaterialCode;
                            astPalletTaskItem.MaterialName = astLesTaskItem.MaterialName;
                            astPalletTaskItem.MaterialBarcode = astLesTaskItem.MaterialBarcode;
                            astPalletTaskItem.ToPickQuantity = astLesTaskItem.ToPickQuantity;
                            astPalletTaskItem.MaxQuantityInSingleCartPosition = astLesTaskItem.MaxQuantityInSingleCartPosition;
                            astPalletTaskItem.IsSpecial = true;
                            astPalletTaskItem.IsBig = astLesTaskItem.IsBig;
                            astPalletTaskItem.PickStatus = PickStatus.New;

                            dbContext.AST_PalletTaskItem_PDAs.Add(astPalletTaskItem);

                            astLesTaskItem.AST_PalletTaskItem_PDA = astPalletTaskItem;
                        }

                        //标记已合并按托任务
                        foreach (AST_LesTask_PDA astLesTask in astLesTaskGroupingByFromPalletPosition)
                            astLesTask.TaskGenerated = true;
                    }
                }
            }
        }
    }
}