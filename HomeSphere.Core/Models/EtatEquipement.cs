namespace HomeSphere.Core.Models;

public class EtatEquipement {
  public Guid EquipementId { get; set; }
  public bool EstAllume { get; set; }
  public bool EstConnecte { get; set; }
  public int? Ouverture { get; set; }
  public double? Temperature { get; set; }
  public double? TemperatureReference { get; set; }
}