using DataAccess;
using DataAccess.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Interfaces.Services
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“FeedZonePDAService”。
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
    public class FeedZonePDAService : IFeedZonePDAService
    {
        readonly static FeedZonePDAService instance = new FeedZonePDAService();
        public static FeedZonePDAService Instance { get { return instance; } }

        public string Bind(string materialId, string groundId)
        {
            string result = string.Empty;
            try
            {
                materialId = materialId.Trim();
                groundId = groundId.Trim();

                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    FeedZone item = dbContext.FeedZones.FirstOrDefault(x => x.GroundId == groundId);
                    if (item == null)
                    {
                        result = "该地堆不存在！";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.MaterialId))
                        {
                            item.MaterialId = materialId;
                            dbContext.SaveChanges();
                            result = "绑定成功！";
                        }
                        else
                        {
                            result = "该地堆已绑定小车，不能重复绑定！";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public string UnBind(string groundId)
        {
            string result = string.Empty;
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    FeedZone item = dbContext.FeedZones.FirstOrDefault(x => x.GroundId == groundId);
                    if (item == null)
                    {
                        result = "该地堆不存在！";
                    }
                    else
                    {
                        item.MaterialId = null;
                        dbContext.SaveChanges();
                        result = "解绑成功！";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }


            return result;
        }

        public List<string> QueryTypeNames()
        {
            List<string> result = new List<string>();
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    result = dbContext.Database.SqlQuery<string>("select DISTINCT [ProjectCode] from [AST_LesTask]").ToList();
                }
            }
            catch
            {

            }

            //result.Add("a,d");
            //result.Add("a");
            //result.Add("b");
            //result.Add("c");

            List<string> targets = new List<string>();
            List<string> deletes = new List<string>();
            foreach (var item in result)
            {
                if (item.Contains(","))
                {
                    string[] values = item.Split(',');
                    targets.Add(values[1]);
                    deletes.Add(item);
                }
            }
            foreach (var item in deletes)
            {
                result.Remove(item);
            }
            result.AddRange(targets);
            result = result.Distinct().ToList();
            return result;
        }


        public string BindCacheRegion(string groundId, string materialId1, string materialId2, string materialId3)
        {
            string message = string.Empty;
            try
            {
                materialId1 = materialId1.Trim();
                materialId2 = materialId2.Trim();
                materialId3 = materialId3.Trim();
                groundId = groundId.Trim();
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    CacheRegion entity = dbContext.CacheRegions.FirstOrDefault(x => x.ChildAreaId == groundId);
                    if (entity == null)
                    {
                        message = "该地堆不存在！";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(entity.Material_A) && string.IsNullOrEmpty(entity.Material_B) && string.IsNullOrEmpty(entity.Material_C))
                        {
                            entity.Material_A = materialId1;
                            entity.Material_B = materialId2;
                            entity.Material_C = materialId3;
                            dbContext.SaveChanges();
                            message = "绑定成功！";
                        }
                        else
                        {
                            message = "该地堆已经绑定小车";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;


        }
        public string UnBindCacheRegion(string groundId)
        {
            string message = string.Empty;
            groundId = groundId.Trim();
            try
            {
                using (GeelyPtlEntities dbContext = new GeelyPtlEntities())
                {
                    CacheRegion entity = dbContext.CacheRegions.FirstOrDefault(x => x.ChildAreaId == groundId);
                    if (entity == null)
                    {
                        message = "该地堆不可用！";
                    }
                    else
                    {
                        entity.Material_A = null;
                        entity.Material_B = null;
                        entity.Material_C = null;
                        dbContext.SaveChanges();
                        message = "解绑成功！";
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return message;


        }


    }
}
