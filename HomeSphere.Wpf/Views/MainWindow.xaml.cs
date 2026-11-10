using System.ComponentModel;
using System.Windows;
using HomeSphere.Wpf.ViewModels;

namespace HomeSphere.Wpf.Views;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow: Window {
  public MainWindow() {
    InitializeComponent();
    DataContext = new MainViewModel();
  }

  private void Window_Closing(object? sender,CancelEventArgs e) {
    if (DataContext is MainViewModel viewModel)
      viewModel.SauvegarderEnergies();
  }
}