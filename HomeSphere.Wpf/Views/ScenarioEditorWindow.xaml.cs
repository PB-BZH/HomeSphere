using System.Windows;
using HomeSphere.Core.Models;
using HomeSphere.Wpf.ViewModels;

namespace HomeSphere.Wpf.Views;

/// <summary>
/// Logique d'interaction pour ScenarioEditorWindow.xaml
/// </summary>
public partial class ScenarioEditorWindow: Window {

  public ScenarioEditorWindow(Action<Scenario> enregistrerScenario,Core.HomeSphere homeSphere,Scenario? scenario = null) {
    InitializeComponent();
    DataContext = new ScenarioEditorViewModel(homeSphere,() => Close(),enregistrerScenario,scenario);
  }
}
