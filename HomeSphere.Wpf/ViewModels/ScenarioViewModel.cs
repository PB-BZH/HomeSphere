using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeSphere.Core.Models;
using HomeSphere.Core.Services;

namespace HomeSphere.Wpf.ViewModels;

public partial class ScenarioViewModel: ObservableObject {
  public string Titre { get; } = "Scénario";
  private readonly ScenarioService _scenarioService;
  public ObservableCollection<ScenarioItemViewModel> Scenarios { get; }

  [ObservableProperty]
  public partial Scenario? SelectedScenario { get; set; }
  private readonly Action<Scenario?> _ouvrirEditeurScenario;

  [RelayCommand]
  private void ExecuterScenario(ScenarioItemViewModel item) {
    foreach (var scenario in Scenarios)
      scenario.MessageExecution = string.Empty;

    ResultatScenario resultat = _scenarioService.ExecuterScenario(item.Scenario);

    if (resultat.ActionsIgnorees == 0)
      item.MessageExecution = $"Scénario exécuté : {resultat.ActionsAppliquees} action(s) appliquée(s).";
    else
      item.MessageExecution = $"Exécution partielle : {resultat.ActionsAppliquees} appliquée(s), {resultat.ActionsIgnorees} ignorée(s).";
  }

  public ScenarioViewModel(ScenarioService scenarioService,Action<Scenario?> ouvrirEditeurScenario) {
    _scenarioService = scenarioService;
    _ouvrirEditeurScenario = ouvrirEditeurScenario;
    Scenarios = new ObservableCollection<ScenarioItemViewModel>(_scenarioService.Scenarios.Select(s => new ScenarioItemViewModel(s)));
  }

  [RelayCommand]
  private void AjouterScenario() {
    _ouvrirEditeurScenario(null);
  }

  public ScenarioItemViewModel? GetScenario(Scenario scenario) {
    return Scenarios.FirstOrDefault(s => s.Scenario.Id == scenario.Id);
  }

  public void AjouterScenario(Scenario scenario) {
    var scenarioExistant = GetScenario(scenario);
    if (scenarioExistant is null) {
      _scenarioService.AjouterScenario(scenario);
      Scenarios.Add(new ScenarioItemViewModel(scenario));
    }
    else {
      _scenarioService.ModifierScenario(scenario);
      int index = Scenarios.IndexOf(scenarioExistant);
      Scenarios[index] = new ScenarioItemViewModel(scenarioExistant.Scenario);
    }
  }

  [RelayCommand]
  private void ModifierScenario(ScenarioItemViewModel item) {
    _ouvrirEditeurScenario(item.Scenario);
  }

  [RelayCommand]
  private void SupprimerScenario(ScenarioItemViewModel item) {
    if (MessageBoxResult.Yes == MessageBox.Show(
      $"Êtes-vous sûr de vouloir supprimer le scénario '{item.Scenario.Nom}' ?",
      "Confirmation",
      MessageBoxButton.YesNo,
      MessageBoxImage.Warning)) {
      SupprimerScenario(item.Scenario);
    }
  }

  public void SupprimerScenario(Scenario scenario) {
    var scenarioExistant = GetScenario(scenario);

    if (scenarioExistant is not null) {
      _scenarioService.SupprimerScenario(scenario);
      Scenarios.Remove(scenarioExistant);
    }
  }
}

