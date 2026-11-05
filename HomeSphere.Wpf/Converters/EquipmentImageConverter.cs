using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HomeSphere.Wpf.Converters;

public class EquipmentImageConverter: IValueConverter {

  public object? Convert(object value,Type targetType,object parameter,CultureInfo culture) {
    if (value is not string image || string.IsNullOrWhiteSpace(image))
      return null;

    return new BitmapImage(new Uri($"pack://application:,,,/Images/{image}",UriKind.Absolute));
  }

  public object ConvertBack(object value,Type targetType,object parameter,CultureInfo culture) {
    throw new NotSupportedException();
  }
}