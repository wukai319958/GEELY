using System;
using System.Globalization;
using System.Windows.Data;
using AssortingKanban.ForAssortingKanban;

namespace AssortingKanban.Converters
{
    /// <summary>
    /// 特殊件信息到其文本的转换器。
    /// </summary>
    class PalletTaskItemDtoSpecialInfoToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AST_PalletTaskItemDto dto = (AST_PalletTaskItemDto)value;
            if (dto.IsSpecial)
                return string.IsNullOrEmpty(dto.MaterialBarcode) ? "√" : dto.MaterialBarcode;
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}