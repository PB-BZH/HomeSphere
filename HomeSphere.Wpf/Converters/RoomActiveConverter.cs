using System.Globalization;
using System.Windows.Data;
using HomeSphere.Core.Models;

namespace HomeSphere.Wpf.Converters;

public class RoomActiveConverter: IMultiValueConverter {
  public object Convert(object[] values,Type targetType,object parameter,CultureInfo culture) {
    return values[0] is Piece selected && values[1] is Piece room && selected.Id == room.Id;
  }

  public object[] ConvertBack(object value,Type[] targetTypes,object parameter,CultureInfo culture) {
    throw new NotSupportedException();
  }
}