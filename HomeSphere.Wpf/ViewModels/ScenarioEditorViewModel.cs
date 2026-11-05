using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeSphere.Core.Enums;
using HomeSphere.Core.Models;

namespace HomeSphere.Wpf.ViewModels;

public partial class ScenarioEditorViewModel: ObservableObject {
  private readonly Core.HomeSphere _homeSphere;
  public List<EquipementSelectionViewModel> EquipementDisponible { get; }
  private readonly Action _fermerEditeur;
  private readonly Scenario? _scenarioOriginal;
  private readonly Action<Scenario> _enregistrerScenario;
  private List<Equipement> _equipementDisponible = [];

  public ScenarioEditorViewModel(Core.HomeSphere homeSphere,Action fermerEditeur,Action<Scenario> enregistrerScenario,Scenario? scenario = null) {
    _homeSphere = homeSphere;
    _fermerEditeur = fermerEditeur;
    _enregistrerScenario = enregistrerScenario;
    _scenarioOriginal = scenario;
    EquipementDisponible = AfficherEquipementCommandable();

    if (scenario is not null) {
      Nom = scenario.Nom;
      Description = scenario.Description;
      RestaurerActionsScenario(scenario);
    }
  }

  [ObservableProperty]
  public partial string Nom { get; set; } = string.Empty;

  [ObservableProperty]
  public partial string Description { get; set; } = string.Empty;

  [RelayCommand]
  private void FermerEditeurScenario() {
    _fermerEditeur();
  }

  [RelayCommand]
  private void EnregistrerScenario() {
    if (string.IsNullOrWhiteSpace(Nom))
      return;

    var scenario = new Scenario {
      Id = _scenarioOriginal?.Id ?? Guid.NewGuid(),
      Nom = Nom.Trim(),
      Description = Description.Trim(),
      Icone = "⚙",
      CouleurIcone = "DodgerBlue"
    };

    var equipementsSelectionnes = EquipementDisponible
      .Where(vm => vm.EstSelectionne)
      .ToList();

    foreach (var vm in equipementsSelectionnes) {
      var action = new ActionScenario {
        EquipementId = vm.Equipement.Id,
        TypeAction = vm.EstVoletRoulant ? TypeActionScenario.PositionVolet : TypeActionScenario.MarcheArret,
        EtatSouhaite = vm.EtatSouhaite,
        OuvertureSouhaitee = vm.EstVoletRoulant ? vm.PositionSouhaiteVoletRoulant : null
      };

      scenario.ActionScenarios.Add(action);
    }

    _enregistrerScenario(scenario);
    _fermerEditeur();
  }

  public List<EquipementSelectionViewModel> AfficherEquipementCommandable() {
    return _homeSphere.maisons.SelectMany(m => m.Pieces)
                              .SelectMany(p => p.Equipements)
                              .Where(e => e.EstCommandable)
                              .Select(e => new EquipementSelectionViewModel(e))
                              .ToList();
  }

  [RelayCommand]
  public void EnregistrerEquipement(Equipement equipement) {
    _equipementDisponible = _homeSphere.maisons.SelectMany(m => m.Pieces)
                                               .SelectMany(p => p.Equipements)
                                               .Where(e => e.EstCommandable)
                                               .ToList();

  }

  private void RestaurerActionsScenario(Scenario scenario) {
    foreach (var action in scenario.ActionScenarios) {
      var equipementVm = EquipementDisponible.FirstOrDefault(vm => vm.Equipement.Id == action.EquipementId);
      if (equipementVm is not null) {
        equipementVm.EstSelectionne = true;
        equipementVm.EtatSouhaite = action.EtatSouhaite;
        if (equipementVm.EstVoletRoulant)
          equipementVm.PositionSouhaiteVoletRoulant = action.OuvertureSouhaitee ?? 0;
      }
    }
  }

}