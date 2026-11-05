using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeSphere.Core.Models;
using HomeSphere.Core.Services;
using HomeSphere.Wpf.Services;
using HomeSphere.Wpf.Views;

namespace HomeSphere.Wpf.ViewModels;

public partial class MainViewModel: ObservableObject {
  private readonly Core.HomeSphere _homeSphere = new();
  private readonly SimulationTemperaturesService _simulationTemperaturesService;
  private readonly EquipmentsViewModel _equipmentsViewModel;
  private readonly ScenarioViewModel _scenarioViewModel;
  private readonly HistoryViewModel _historyViewModel;
  private readonly DashboardViewModel _dashboardViewModel;
  private readonly RoomsViewModel _roomsViewModel;
  private readonly EnergyViewModel _energyViewModel;
  private readonly DispatcherService _dispatcherService;
  private readonly ParametersViewModel _parametersViewModel;
  private readonly ScenarioService _scenarioService;

  [ObservableProperty]
  public partial string CurrentPage { get; set; } = "Dashboard";

  [ObservableProperty]
  public partial object CurrentViewModel { get; set; } = null!;

  public MainViewModel() {
    _simulationTemperaturesService = new(_homeSphere);
    _dispatcherService = new(_simulationTemperaturesService);
    _dashboardViewModel = new(_homeSphere,_dispatcherService);
    _parametersViewModel = new(_homeSphere,_dispatcherService);
    _roomsViewModel = new(_homeSphere,OuvrirEquipement);
    _energyViewModel = new(_homeSphere);
    _scenarioService = new(_homeSphere);
    _historyViewModel = new(_homeSphere);
    _equipmentsViewModel = new(_homeSphere);
    _scenarioViewModel = new(_scenarioService,OuvrirEditeurScenario);
    CurrentViewModel = _dashboardViewModel;
  }

  [RelayCommand]
  private void AfficherDashboard() {
    CurrentViewModel = _dashboardViewModel;
    CurrentPage = "Dashboard";
  }

  [RelayCommand]
  private void AfficherPieces() {
    CurrentViewModel = _roomsViewModel;
    CurrentPage = "Rooms";
  }

  [RelayCommand]
  private void AfficherEquipements() {
    CurrentViewModel = _equipmentsViewModel;
    CurrentPage = "Equipments";
  }

  internal void OuvrirEquipement(Piece piece,Equipement equipement) {
    _equipmentsViewModel.SelectedRoom = piece;
    _equipmentsViewModel.SelectedEquipment = equipement;

    CurrentViewModel = _equipmentsViewModel;
    CurrentPage = "Equipments";
  }

  [RelayCommand]
  private void AfficherScenario() {
    CurrentViewModel = _scenarioViewModel;
    CurrentPage = "Scenario";
  }

  [RelayCommand]
  private void AfficherHistorique() {
    CurrentViewModel = _historyViewModel;
    CurrentPage = "History";
  }

  [RelayCommand]
  private void AfficherParametres() {
    CurrentViewModel = _parametersViewModel;
    CurrentPage = "Parameters";
  }

  [RelayCommand]
  private void AfficherEnergie() {
    CurrentViewModel = _energyViewModel;
    CurrentPage = "Energy";
  }

  private void OuvrirEditeurScenario(Scenario? scenario) {
    var fenetre = scenario is not null ? new ScenarioEditorWindow(_scenarioViewModel.AjouterScenario,_homeSphere,scenario) {
      Owner = Application.Current.MainWindow,
      WindowStartupLocation = WindowStartupLocation.CenterOwner
    } : new ScenarioEditorWindow(_scenarioViewModel.AjouterScenario,_homeSphere) {
      Owner = Application.Current.MainWindow,
      WindowStartupLocation = WindowStartupLocation.CenterOwner
    };


    fenetre.ShowDialog();
  }
}