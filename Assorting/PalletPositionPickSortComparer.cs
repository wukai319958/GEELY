using System.Collections.Generic;
using DataAccess.Assorting;

namespace Assorting
{
    /// <summary>
    /// 近端 3、4、5 库位先拣选，远端 2 1 库位后拣选
    /// </summary>
    public class PalletPositionPickSortComparer : IComparer<AST_PalletTaskItem>
    {
        /// <summary>
        /// 比较两个按托任务的拣选顺序。
        /// </summary>
        /// <param name="item1">任务一。</param>
        /// <param name="item2">任务二。</param>
        /// <returns>比较结果。</returns>
        public int Compare(AST_PalletTaskItem item1, AST_PalletTaskItem item2)
        {
            List<int> sortIndexArray = new List<int> { 3, 4, 5, 2, 1 };

            int xIndex = sortIndexArray.IndexOf(item1.FromPalletPosition);
            int yIndex = sortIndexArray.IndexOf(item2.FromPalletPosition);

            return xIndex.CompareTo(yIndex);
        }
    }
}