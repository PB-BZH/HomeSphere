using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeSphere.Wpf.Services;

namespace HomeSphere.Wpf.ViewModels;

public partial class ParametersViewModel: ObservableObject {
  private readonly Core.HomeSphere _homeSphere;
  public string Titre { get; } = "Parametres";
  private readonly DispatcherService _dispatcherService;

  public ParametersViewModel(Core.HomeSphere homeSphere,DispatcherService dispatcherService) {
    _homeSphere = homeSphere;
    _dispatcherService = dispatcherService;
    SimulationActive = _dispatcherService.EstActif;
  }

  [ObservableProperty]
  public partial bool SimulationActive { get; set; }

  [RelayCommand]
  private void BasculerSimulation() {
    SimulationActive = !SimulationActive;
  }

  partial void OnSimulationActiveChanged(bool value) {
    if (value)
      _dispatcherService.Demarrer();
    else
      _dispatcherService.Arreter();
  }
}
