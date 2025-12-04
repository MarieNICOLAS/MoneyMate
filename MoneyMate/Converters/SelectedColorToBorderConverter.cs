using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MoneyMate.Converters
{
    public class SelectedColorToBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorHex = value as string;
            var selectedColor = parameter as string;

            if (colorHex == null || selectedColor == null)
                return Colors.Transparent;

            return colorHex == selectedColor
                ? Colors.Black   // bordure si sélectionné
                : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
