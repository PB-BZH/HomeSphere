using System.IO;
using System.Windows;
using HomeSphere.Wpf.Views;

namespace HomeSphere.Wpf;

public partial class App: Application {

  protected override void OnStartup(StartupEventArgs e) {
    base.OnStartup(e);

    try {
      MainWindow fenetre = new();
      MainWindow = fenetre;
      fenetre.Show();
    }
    catch (System.Windows.Markup.XamlParseException ex) when (ex.InnerException is InvalidDataException) {
      MessageBox.Show(ex.InnerException!.Message,"HomeSphere - Erreur",MessageBoxButton.OK,MessageBoxImage.Warning);
      Shutdown();
    }
    catch (InvalidDataException ex) {
      MessageBox.Show(ex.Message,"HomeSphere - Erreur",MessageBoxButton.OK,MessageBoxImage.Warning);
      Shutdown();
    }
  }
}