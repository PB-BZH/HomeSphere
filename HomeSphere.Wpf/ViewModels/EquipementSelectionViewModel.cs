using CommunityToolkit.Mvvm.ComponentModel;
using HomeSphere.Core.Models;

namespace HomeSphere.Wpf.ViewModels;

public partial class EquipementSelectionViewModel: ObservableObject {
  public Equipement Equipement { get; }

  [ObservableProperty]
  public partial bool EstSelectionne { get; set; } = false;

  public bool EstVoletRoulant => Equipement is VoletRoulant;

  [ObservableProperty]
  public partial int PositionSouhaiteVoletRoulant { get; set; } = 0;

  [ObservableProperty]
  public partial bool EtatSouhaite { get; set; } = false;

  public EquipementSelectionViewModel(Equipement equipement) {
    Equipement = equipement;
  }
}
