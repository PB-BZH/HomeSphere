using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HomeSphere.Core.Models;
using HomeSphere.Core.Services;
using Microsoft.Win32;

namespace HomeSphere.Wpf.ViewModels;

public partial class EnergyViewModel: ObservableObject {

  private readonly EnergieService _energieService;

  private readonly Core.HomeSphere _homeSphere;

  [ObservableProperty]
  public partial double DureeObservationGlobale { get; set; }

  [ObservableProperty]
  public partial double DureeObservationParPiece { get; set; }

  [ObservableProperty]
  public partial Dictionary<string,double> PuissancesParPiece_ParNom_W { get; private set; } = new();

  [ObservableProperty]
  public partial Dictionary<string,double> EnergieParPiece_ParNom_kWh { get; set; } = new();


  [ObservableProperty]
  public partial double PuissanceTotale { get; set; }

  [ObservableProperty]
  public partial double EnergieTotaleKWh { get; set; }

  public EnergyViewModel(Core.HomeSphere homeSphere,EnergieService energieService) {
    _homeSphere = homeSphere;
    _energieService = energieService;
    ActualiserPuissance();
    foreach (Equipement equipement in homeSphere.maisons.SelectMany(m => m.Pieces).SelectMany(p => p.Equipements)) {
      equipement.PropertyChanged += Equipement_PropertyChanged;
    }
  }

  private void SystemEvents_PowerModeChanged(object sender,PowerModeChangedEventArgs e) {
    throw new NotImplementedException();
  }

  internal void ActualiserPuissance() {
    PuissanceTotale = _energieService.CalculerPuissanceTotale_W();
    EnergieTotaleKWh = _energieService.EnergieTotaleKWh;
    PuissancesParPiece_ParNom_W = ConvertDictionary(_energieService.CalculerPuissanceParPiece_W());
    EnergieParPiece_ParNom_kWh = ConvertDictionary(_energieService.EnergieParPiece_kWh);
    DureeObservationGlobale = _energieService.DureeObservationGlobaleHeures;
    DureeObservationParPiece = _energieService.DureeObservationParPieceHeures;
  }

  private void Equipement_PropertyChanged(object? sender,PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(Equipement.Puissance)) {
      _energieService.ActualiserConsommation_kWh();
      _energieService.ActualiserConsommationParPiece_kWh();
      ActualiserPuissance();
    }
  }

  private Dictionary<string,double> ConvertDictionary(Dictionary<Guid,double> guidKeyValuePairs) {
    Dictionary<string,double> nameKeyValuePairs;

    nameKeyValuePairs = guidKeyValuePairs.ToDictionary(
    entree => _homeSphere.maisons.SelectMany(m => m.Pieces)
                                 .First(p => p.Id == entree.Key).Nom,
    entree => entree.Value);

    return nameKeyValuePairs;
  }
}
