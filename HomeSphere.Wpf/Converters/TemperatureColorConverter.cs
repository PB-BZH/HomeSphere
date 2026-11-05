using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HomeSphere.Wpf.Converters;

public class TemperatureColorConverter: IValueConverter {
  public object Convert(object value,Type targetType,object parameter,CultureInfo culture) {
    if (value is not double temperature) return Brushes.Gray;

    if (temperature < 18) return Brushes.DodgerBlue;
    if (temperature <= 22) return Brushes.LightGreen;
    return Brushes.Orange;
  }

  public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
    throw new NotSupportedException();
  }
}