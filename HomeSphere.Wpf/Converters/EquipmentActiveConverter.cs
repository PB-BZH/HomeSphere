using System.Globalization;
using System.Windows.Data;
using HomeSphere.Core.Models;

namespace HomeSphere.Wpf.Converters;

public class EquipmentActiveConverter: IMultiValueConverter {
  public object Convert(object[] values,Type targetType,object parameter,CultureInfo culture) {
    return values[0] is Equipement selected &&
           values[1] is Equipement equipement &&
           selected.Id == equipement.Id;
  }

  public object[] ConvertBack(object value,Type[] targetTypes,object parameter,CultureInfo culture) {
    throw new NotSupportedException();
  }
}
