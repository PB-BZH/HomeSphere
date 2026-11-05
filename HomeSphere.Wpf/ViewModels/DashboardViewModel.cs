using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HomeSphere.Core.Models;
using HomeSphere.Wpf.Services;

namespace HomeSphere.Wpf.ViewModels;

public partial class DashboardViewModel: ObservableObject {
  private readonly Core.HomeSphere _homeSphere;
  public string Titre { get; } = "Tableau de bord";
  [ObservableProperty]
  public partial int NombrePieces { get; set; }
  [ObservableProperty]
  public partial int NombreEquipements { get; set; }
  [ObservableProperty]
  public partial int NombreEquipementsAllumes { get; set; }
  [ObservableProperty]
  public partial int NombreEquipementsConnectes { get; set; }
  public Maison MaisonPrincipale => _homeSphere.maisons[0];
  public List<Piece> Rooms => MaisonPrincipale.Pieces;
  public List<TemperaturePiece> TemperaturesPieces { get; }
  private readonly DispatcherService _dispatcherService;
  [ObservableProperty]
  public partial bool SimulationActive { get; set; }


  public DashboardViewModel(Core.HomeSphere homeSphere,DispatcherService dispatcherService) {
    _homeSphere = homeSphere;
    _dispatcherService = dispatcherService;
    SimulationActive = _dispatcherService.EstActif;
    _dispatcherService.PropertyChanged += DispatcherService_PropertyChanged;
    NombrePieces = CalculerNombrePieces();
    NombreEquipements = CalculerNombreEquipements();
    NombreEquipementsAllumes = CalculerNombreEquipementsAllumes();
    NombreEquipementsConnectes = CalculerNombreEquipementsConnectes();
    TemperaturesPieces = _homeSphere.maisons
        .SelectMany(m => m.Pieces)
        .SelectMany(p => p.Equipements.OfType<Thermometre>()
            .Select(t => new TemperaturePiece { Piece = p,Thermometre = t }))
        .ToList();
    foreach (var equipement in _homeSphere.maisons.SelectMany(m => m.Pieces).SelectMany(p => p.Equipements)) {
      equipement.PropertyChanged += Equipement_PropertyChanged;
    }
  }

  private void DispatcherService_PropertyChanged(object? sender,PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(DispatcherService.EstActif)) {
      SimulationActive = _dispatcherService.EstActif;
    }
  }

  private void Equipement_PropertyChanged(object? sender,PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(Equipement.EstAllume)) {
      NombreEquipementsAllumes = CalculerNombreEquipementsAllumes();
    }
    else if (e.PropertyName == nameof(Equipement.EstConnecte)) {
      NombreEquipementsConnectes = CalculerNombreEquipementsConnectes();
    }
  }

  private int CalculerNombrePieces() {
    return _homeSphere.maisons.Sum(m => m.Pieces.Count);
  }

  private int CalculerNombreEquipements() {
    return _homeSphere.maisons.SelectMany(m => m.Pieces)
                              .Sum(p => p.Equipements.Count);
  }

  private int CalculerNombreEquipementsAllumes() {
    return _homeSphere.maisons.SelectMany(m => m.Pieces)
                              .SelectMany(p => p.Equipements)
                              .Count(e => e.EstAllume);
  }

  private int CalculerNombreEquipementsConnectes() {
    return _homeSphere.maisons.SelectMany(m => m.Pieces)
                              .SelectMany(p => p.Equipements)
                              .Count(e => e.EstConnecte);
  }
}