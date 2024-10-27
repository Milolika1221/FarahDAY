using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;


namespace OmniApp
{
    public class MessageSourceToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string Source)
            {
                return Source == "system" || Source == "user" ? Brushes.Transparent : (SolidColorBrush)(new BrushConverter().ConvertFrom("#b1c1e7"));
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
