using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeSphere.Core.Models;

namespace HomeSphere.Wpf.ViewModels;

public partial class RoomsViewModel: ObservableObject {
  public string Titre { get; } = "Pièces";
  private readonly Core.HomeSphere _homeSphere;
  private readonly Action<Piece,Equipement> _ouvrirEquipement;
  public Maison MaisonPrincipale => _homeSphere.maisons[0];
  public List<Piece> Rooms => MaisonPrincipale.Pieces;

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
    if (SelectedRoom is not null)
      _ouvrirEquipement(SelectedRoom,equipement);
  }

  public RoomsViewModel(Core.HomeSphere homeSphere,Action<Piece,Equipement> ouvrirEquipement) {
    _homeSphere = homeSphere;
    _ouvrirEquipement = ouvrirEquipement;
  }
}