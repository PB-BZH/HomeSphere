using CommunityToolkit.Mvvm.ComponentModel;

namespace HomeSphere.Core.Models;

public partial class VoletRoulant: Equipement {

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(StatutOuverture))]
  public partial int Ouverture { get; set; } = 100;
  public override bool EstCommandeMarcheArret => false;
  public override double ImageOpacity => EstConnecte ? 1.0 : 0.35;
  public string StatutOuverture => $"{Ouverture}%";
  public override string StatutTexte => Ouverture == 0 ? "Fermé" : (Ouverture == 100 ? "Ouvert" : $"Ouvert à {Ouverture}%");
  public VoletRoulant() {
    Type = "Volet roulant";
    EstCommandable = true;
    EstConnecte = true;
    Image = "volet.png";
  }
}