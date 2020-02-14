using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AssortingKanban.ForAssortingKanban;

namespace AssortingKanban.Converters
{
    /// <summary>
    /// 统计进度条长度的转换器。
    /// </summary>
    class GridColumnProgressBarColumnWidthConverter : IValueConverter
    {
        /// <summary>
        /// 转换器参数之已完成批次。
        /// </summary>
        public const string Parameter_FinishedBatchCount = "FinishedBatchCount";
        /// <summary>
        /// 转换器参数之剩余批次。
        /// </summary>
        public const string Parameter_RemainBatchCount = "RemainBatchCount";
        /// <summary>
        /// 转换器参数之已完成托盘。
        /// </summary>
        public const string Parameter_FinishedPalletCount = "FinishedPalletCount";
        /// <summary>
        /// 转换器参数之剩余托盘。
        /// </summary>
        public const string Parameter_RemainPalletCount = "RemainPalletCount";
        /// <summary>
        /// 转换器参数之已完成零件数量。
        /// </summary>
        public const string Parameter_FinishedMaterialCount = "FinishedMaterialCount";
        /// <summary>
        /// 转换器参数之剩余零件数量。
        /// </summary>
        public const string Parameter_RemainMaterialCount = "RemainMaterialCount";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AssortingKanbanTodayStatistics todayStatistics = (AssortingKanbanTodayStatistics)value;

            GridLength finishedBatchCount = new GridLength(0, GridUnitType.Star);
            GridLength remainBatchCount = new GridLength(1, GridUnitType.Star);
            GridLength finishedPalletCount = new GridLength(0, GridUnitType.Star);
            GridLength remainPalletCount = new GridLength(1, GridUnitType.Star);
            GridLength finishedMaterialCount = new GridLength(0, GridUnitType.Star);
            GridLength remainMaterialCount = new GridLength(1, GridUnitType.Star);

            if (todayStatistics != null)
            {
                finishedBatchCount = new GridLength(todayStatistics.FinishedBatchCount, GridUnitType.Star);
                remainBatchCount = new GridLength(todayStatistics.TotalBatchCount - todayStatistics.FinishedBatchCount, GridUnitType.Star);
                finishedPalletCount = new GridLength(todayStatistics.FinishedPalletCount, GridUnitType.Star);
                remainPalletCount = new GridLength(todayStatistics.TotalPalletCount - todayStatistics.FinishedPalletCount, GridUnitType.Star);
                finishedMaterialCount = new GridLength(todayStatistics.FinishedMaterialCount, GridUnitType.Star);
                remainMaterialCount = new GridLength(todayStatistics.TotalMaterialCount - todayStatistics.FinishedMaterialCount, GridUnitType.Star);
            }

            switch ((string)parameter)
            {
                case Parameter_FinishedBatchCount:
                    return finishedBatchCount;
                case Parameter_RemainBatchCount:
                    return remainBatchCount;
                case Parameter_FinishedPalletCount:
                    return finishedPalletCount;
                case Parameter_RemainPalletCount:
                    return remainPalletCount;
                case Parameter_FinishedMaterialCount:
                    return finishedMaterialCount;
                case Parameter_RemainMaterialCount:
                    return remainMaterialCount;

                default:
                    return new GridLength(1, GridUnitType.Star);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}