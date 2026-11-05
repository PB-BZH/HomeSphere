using CommunityToolkit.Mvvm.ComponentModel;
namespace HomeSphere.Core.Models;

public partial class Equipement: ObservableObject {
  public readonly bool EstDisponible;

  public Guid Id { get; set; } = Guid.NewGuid();
  public string Nom { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public string Image { get; set; } = string.Empty;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(ImageOpacity))]
  [NotifyPropertyChangedFor(nameof(StatutConnexion))]
  public partial bool EstConnecte { get; set; }

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(StatutTexte))]
  [NotifyPropertyChangedFor(nameof(ImageOpacity))]
  public partial bool EstAllume { get; set; }

  public virtual string StatutTexte => EstAllume ? "Allumé" : "Éteint";
  public virtual double ImageOpacity => !EstConnecte ? 0.35 : (!EstCommandable || EstAllume ? 1.0 : 0.35);
  public bool EstCommandable { get; set; }
  public string StatutConnexion => EstConnecte ? "Connecté" : "Déconnecté";
  public virtual bool EstCommandeMarcheArret => true;
}
