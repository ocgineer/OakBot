using System;
using System.Windows.Data;
using System.Globalization;

namespace OakBot.View
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolToNotBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean type.");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Same code as convert so just using Convert...
            return this.Convert(value, targetType, parameter, culture);
        }
    }
}
