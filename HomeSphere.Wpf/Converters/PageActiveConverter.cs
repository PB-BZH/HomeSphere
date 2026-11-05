using System.Globalization;
using System.Windows.Data;

namespace HomeSphere.Wpf.Converters;

public class PageActiveConverter: IMultiValueConverter {

  public object Convert(object[] values,Type targetType,object parameter,CultureInfo culture) {
    return values.Length == 2
        && values[0] is string currentPage
        && values[1] is string buttonPage
        && string.Equals(currentPage,buttonPage,StringComparison.Ordinal);
  }

  public object[] ConvertBack(object value,Type[] targetTypes,object parameter,CultureInfo culture) {
    throw new NotSupportedException();
  }
}