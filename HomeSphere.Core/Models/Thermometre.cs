using CommunityToolkit.Mvvm.ComponentModel;

namespace HomeSphere.Core.Models;

public partial class Thermometre: Equipement {

  [ObservableProperty]
  public partial double Temperature { get; set; } = 21.5;
  public double TemperatureReference { get; set; }
  public bool TemperatureMonte { get; set; } = true;

  public Thermometre() {
    EstConnecte = true;
    EstCommandable = false;
    EstAllume = false;
    Image = "thermometre.png";
  }
}
