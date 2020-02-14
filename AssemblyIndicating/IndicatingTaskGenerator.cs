using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataAccess;
using DataAccess.AssemblyIndicating;
using DataAccess.Assorting;
using DataAccess.Config;

namespace AssemblyIndicating
{
    /// <summary>
    /// 在车身抵达时，生成装配指引任务。
    /// </summary>
    public static class IndicatingTaskGenerator
    {
        /// <summary>
        /// 生成装配指引任务。
        /// </summary>
        /// <param name="asmAssembleIndication">从接口解析还未持久化的车身抵达记录。</param>
        /// <param name="dbContext">数据上下文。</param>
        public static void Generate(ASM_AssembleIndication asmAssembleIndication, GeelyPtlEntities dbContext)
        {
            List<ASM_AssembleIndicationItem> asmAssembleIndicationItems = asmAssembleIndication.ASM_AssembleIndicationItems
                                                                              .OrderBy(aii => aii.AssembleSequence)
                                                                              .ToList();
            List<CFG_Cart> dockedCfgCarts = dbContext.CFG_WorkStationCurrentCarts
                                                .Where(wscc => wscc.CFG_WorkStationId == asmAssembleIndication.CFG_WorkStationId
                                                               && (wscc.CFG_Cart.CartStatus == CartStatus.ArrivedAtWorkStation || wscc.CFG_Cart.CartStatus == CartStatus.Indicating))
                                                .OrderBy(wscc => wscc.DockedTime)
                                                .Select(wscc => wscc.CFG_Cart)
                                                .Distinct()
                                                .ToList();

            List<CFG_CartCurrentMaterial> cfgCartCurrentMaterials = new List<CFG_CartCurrentMaterial>();
            foreach (CFG_Cart cfgCart in dockedCfgCarts)
                cfgCartCurrentMaterials.AddRange(cfgCart.CFG_CartCurrentMaterials.Where(ccm => ccm.AST_CartTaskItemId != null && ccm.Quantity > 0));

            ASM_Task asmTask = new ASM_Task();
            asmTask.ASM_AssembleIndication = asmAssembleIndication;
            asmTask.AssembleStatus = AssembleStatus.New;

            dbContext.ASM_Tasks.Add(asmTask);

            Dictionary<string, int> lockedQuantityByMaterialCode = new Dictionary<string, int>();

            foreach (ASM_AssembleIndicationItem asmAssembleIndicationItem in asmAssembleIndicationItems)
            {
                string materialCode = asmAssembleIndicationItem.MaterialCode;
                string projectCode = asmAssembleIndicationItem.ProjectCode;
                string projectStep = asmAssembleIndicationItem.ProjectStep;
                string batchCode = asmAssembleIndicationItem.BatchCode;

                int lockedQuantity = 0;
                if (lockedQuantityByMaterialCode.ContainsKey(materialCode))
                    lockedQuantity = lockedQuantityByMaterialCode[materialCode];
                else
                    lockedQuantityByMaterialCode.Add(materialCode, lockedQuantity);

                List<CFG_CartCurrentMaterial> sameMaterialCfgCartCurrentMaterials = cfgCartCurrentMaterials
                                                                                        .Where(ccm => ccm.MaterialCode == materialCode
                                                                                                      && string.Equals(ccm.ProjectCode, projectCode, StringComparison.OrdinalIgnoreCase)
                                                                                                      && string.Equals(ccm.ProjectStep, projectStep, StringComparison.OrdinalIgnoreCase))
                                                                                        .OrderBy(ccm => ccm.Position)
                                                                                        .ToList();
                //替代料
                foreach (CFG_CartCurrentMaterial cfgCartCurrentMaterial in cfgCartCurrentMaterials
                                                                               .Where(ccm => ccm.MaterialCode == materialCode))
                {
                    if (!sameMaterialCfgCartCurrentMaterials.Contains(cfgCartCurrentMaterial))
                    {
                        List<AST_LesTaskItem> astLesTaskItems = cfgCartCurrentMaterial
                                                                    .AST_CartTaskItem
                                                                    .AST_PalletTaskItem
                                                                    .AST_LesTaskItems
                                                                    .ToList();
                        foreach (AST_LesTaskItem astLesTaskItem in astLesTaskItems)
                        {
                            if (astLesTaskItem.AST_LesTask.ProjectCode.EndsWith(projectCode) && astLesTaskItem.AST_LesTask.ProjectStep.EndsWith(projectStep))
                            {
                                sameMaterialCfgCartCurrentMaterials.Add(cfgCartCurrentMaterial);
                                break;
                            }
                        }
                    }
                }

                //如果给出了批次，则应用限制
                if (!string.IsNullOrEmpty(batchCode))
                {
                    for (int i = sameMaterialCfgCartCurrentMaterials.Count - 1; i >= 0; i--)
                    {
                        if (sameMaterialCfgCartCurrentMaterials[i].BatchCode != batchCode)
                            sameMaterialCfgCartCurrentMaterials.RemoveAt(i);
                    }
                }

                //如果料不够，一个原始装配明细可能被拆分到不同库位，生成多个指引明细
                int currentAsmAssembleIndicationItemAssignedQuantity = 0;
                while (currentAsmAssembleIndicationItemAssignedQuantity < asmAssembleIndicationItem.ToAssembleQuantity)
                {
                    //找到第一个未被完全锁定的库位
                    CFG_CartCurrentMaterial cfgCartCurrentMaterial = null;

                    //针对单个储位的未完成明细数量
                    int unfinishedQuantity = 0;

                    int temporaryLockedQuantity = lockedQuantity;
                    foreach (CFG_CartCurrentMaterial temporaryCfgCartCurrentMaterial in sameMaterialCfgCartCurrentMaterials)
                    {
                        //未完成的任务也会锁定库存
                        List<ASM_TaskItem> unfinishedAsmTaskItems = dbContext.ASM_TaskItems
                                                                        .Where(ti => ti.CFG_CartId == temporaryCfgCartCurrentMaterial.CFG_CartId
                                                                                     && ti.CartPosition == temporaryCfgCartCurrentMaterial.Position
                                                                                     && ti.AssembleStatus != AssembleStatus.Finished)
                                                                        .ToList();
                        unfinishedQuantity = 0;
                        foreach (ASM_TaskItem unfinishedAsmTaskItem in unfinishedAsmTaskItems)
                        {
                            unfinishedQuantity += unfinishedAsmTaskItem.ToAssembleQuantity;
                            if (unfinishedAsmTaskItem.AssembledQuantity != null)
                                unfinishedQuantity -= unfinishedAsmTaskItem.AssembledQuantity.Value;
                        }

                        if (temporaryLockedQuantity < temporaryCfgCartCurrentMaterial.Quantity - unfinishedQuantity)
                        {
                            cfgCartCurrentMaterial = temporaryCfgCartCurrentMaterial;

                            break;
                        }
                        else
                        {
                            temporaryLockedQuantity -= temporaryCfgCartCurrentMaterial.Quantity.Value - unfinishedQuantity;

                            if (temporaryLockedQuantity < 0)
                                temporaryLockedQuantity = 0;
                        }
                    }

                    if (cfgCartCurrentMaterial == null)
                    {
                        int unfinishedAsmTaskItemsQuantity = dbContext.ASM_TaskItems
                                                                 .Where(ti => ti.ASM_Task.ASM_AssembleIndication.CFG_WorkStationId == asmAssembleIndication.CFG_WorkStationId
                                                                              && ti.ASM_AssembleIndicationItem.MaterialCode == asmAssembleIndicationItem.MaterialCode
                                                                              && ti.AssembleStatus != AssembleStatus.Finished)
                                                                 .Select(ti => ti.ToAssembleQuantity)
                                                                 .ToList()
                                                                 .Sum();
                        List<string> storageDetails = dbContext.CFG_CartCurrentMaterials
                                                          .Where(ccm => ccm.BatchCode == asmAssembleIndicationItem.BatchCode
                                                                        && ccm.CFG_WorkStationId == asmAssembleIndication.CFG_WorkStationId
                                                                        && ccm.MaterialCode == asmAssembleIndicationItem.MaterialCode
                                                                        && ccm.Quantity > 0)
                                                          .ToList()
                                                          .Select(ccm => ccm.CFG_Cart.Name + " 储位 " + ccm.Position + " 存量 " + ccm.Quantity + " (" + ccm.ProjectCode + ", " + ccm.ProjectStep + ")")
                                                          .ToList();
                        List<string> dockedCfgCartNames = dockedCfgCarts
                                                              .Select(c => c.Name)
                                                              .ToList();
                        int lesPickedQuantity = dbContext.AST_LesTaskItems
                                                    .Where(lti => lti.MaterialCode == asmAssembleIndicationItem.MaterialCode
                                                                  && lti.AST_LesTask.BatchCode == asmAssembleIndicationItem.BatchCode)
                                                    .Select(lti => lti.ToPickQuantity)
                                                    .ToList()
                                                    .Sum();
                        List<string> mesAssembledItems = dbContext.ASM_AssembleIndicationItems
                                                             .Include(aii => aii.ASM_AssembleIndication)
                                                             .Where(aii => aii.MaterialCode == asmAssembleIndicationItem.MaterialCode
                                                                           && aii.BatchCode == asmAssembleIndicationItem.BatchCode)
                                                             .ToList()
                                                             .Select(aii => aii.ASM_AssembleIndication.ProductSequence + "，" + aii.AssembledQuantity)
                                                             .ToList();

                        string message = "线边库存不足:" + materialCode + "。" + Environment.NewLine
                                         + "需求数量：" + asmAssembleIndicationItem.ToAssembleQuantity + Environment.NewLine;
                        if (unfinishedAsmTaskItemsQuantity > 0)
                            message += "前车未完：" + unfinishedAsmTaskItemsQuantity + Environment.NewLine;
                        if (storageDetails.Count > 0)
                        {
                            message += "料车存量：" + Environment.NewLine;
                            message += string.Join(Environment.NewLine, storageDetails) + Environment.NewLine;
                        }
                        if (dockedCfgCartNames.Count > 0)
                            message += "停靠料车：" + string.Join(", ", dockedCfgCartNames) + Environment.NewLine;
                        message += "LES 出库数量：" + lesPickedQuantity + Environment.NewLine;
                        if (mesAssembledItems.Count > 0)
                        {
                            message += "MES 使用情况：" + Environment.NewLine;
                            message += string.Join(Environment.NewLine, mesAssembledItems) + Environment.NewLine;
                        }

                        throw new Exception(message);
                    }

                    int remainQuantity = cfgCartCurrentMaterial.Quantity.Value - unfinishedQuantity - temporaryLockedQuantity;
                    int toAssembleQuantity = asmAssembleIndicationItem.ToAssembleQuantity - currentAsmAssembleIndicationItemAssignedQuantity;
                    toAssembleQuantity = Math.Min(remainQuantity, toAssembleQuantity);

                    ASM_TaskItem asmTaskItem = new ASM_TaskItem();
                    asmTaskItem.ASM_Task = asmTask;
                    asmTaskItem.ASM_AssembleIndicationItem = asmAssembleIndicationItem;
                    asmTaskItem.Gzz = asmAssembleIndicationItem.Gzz;
                    asmTaskItem.CFG_CartId = cfgCartCurrentMaterial.CFG_CartId;
                    asmTaskItem.CartPosition = cfgCartCurrentMaterial.Position;
                    asmTaskItem.AssembleSequence = asmAssembleIndicationItem.AssembleSequence;
                    asmTaskItem.ToAssembleQuantity = toAssembleQuantity;
                    asmTaskItem.Qtxbs = asmAssembleIndicationItem.Qtxbs;
                    asmTaskItem.AssembleStatus = AssembleStatus.New;

                    dbContext.ASM_TaskItems.Add(asmTaskItem);

                    lockedQuantity += toAssembleQuantity;
                    lockedQuantityByMaterialCode[materialCode] = lockedQuantity;

                    currentAsmAssembleIndicationItemAssignedQuantity += toAssembleQuantity;
                }
            }
        }
    }
}