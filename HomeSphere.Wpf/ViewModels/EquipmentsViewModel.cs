using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeSphere.Core.Models;

namespace HomeSphere.Wpf.ViewModels;

public partial class EquipmentsViewModel: ObservableObject {
  public string Titre { get; } = "Equipements";
  private readonly Core.HomeSphere _homeSphere;
  public Maison MaisonPrincipale => _homeSphere.maisons[0];
  public List<Piece> Rooms => MaisonPrincipale.Pieces;
  public EquipmentsViewModel(Core.HomeSphere homeSphere) {
    _homeSphere = homeSphere;
  }

  [ObservableProperty]
  public partial Piece? SelectedRoom { get; set; }

  [RelayCommand]
  private void SelectionnerPiece(Piece piece) {
    SelectedRoom = piece;
    SelectedEquipment = null;
  }
  [ObservableProperty]
  public partial Equipement? SelectedEquipment { get; set; }

  [RelayCommand]
  private void SelectionnerEquipement(Equipement equipement) {
    SelectedEquipment = equipement;
  }

  [RelayCommand(CanExecute = nameof(PeutBasculerEquipement))]
  private void BasculerEquipement(Equipement equipement) {
    equipement.EstAllume = !equipement.EstAllume;
  }

  private bool PeutBasculerEquipement(Equipement? equipement) =>
    equipement is not null && equipement.EstCommandable && equipement.EstConnecte && equipement.EstCommandeMarcheArret;

  [RelayCommand]
  private void OuvrirVolet(VoletRoulant volet) {
    if (volet.EstConnecte && volet.EstCommandable)
      volet.Ouverture = 100;
  }

  [RelayCommand]
  private void FermerVolet(VoletRoulant volet) {
    if (volet.EstConnecte && volet.EstCommandable)
      volet.Ouverture = 0;
  }
}

