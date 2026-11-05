using CommunityToolkit.Mvvm.ComponentModel;
using HomeSphere.Core.Models;

namespace HomeSphere.Wpf.ViewModels;

public partial class ScenarioItemViewModel: ObservableObject {
  public Scenario Scenario { get; }

  [ObservableProperty]
  public partial string MessageExecution { get; set; } = string.Empty;

  public ScenarioItemViewModel(Scenario scenario) {
    Scenario = scenario;
  }
}