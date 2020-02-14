using System.Collections.Generic;
using DataAccess.Assorting;

namespace Interfaces.Converters
{
    /// <summary>
    /// 正在拣选的排序最前面，已经拣选完的排在最后面
    /// </summary>
    public class PalletTaskSortComparer : IComparer<AST_PalletTaskItem>
    {
        /// <summary>
        /// 比较两个按托任务的拣选顺序。
        /// </summary>
        /// <param name="item1">任务一。</param>
        /// <param name="item2">任务二。</param>
        /// <returns>比较结果。</returns>
        public int Compare(AST_PalletTaskItem item1, AST_PalletTaskItem item2)
        {
            List<PickStatus> sortIndexArray = new List<PickStatus> { PickStatus.Picking, PickStatus.New, PickStatus.Finished };

            int xIndex = sortIndexArray.IndexOf(item1.PickStatus);
            int yIndex = sortIndexArray.IndexOf(item2.PickStatus);

            return xIndex.CompareTo(yIndex);
        }
    }
}