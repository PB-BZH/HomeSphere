using HomeSphere.Core.Enums;

namespace HomeSphere.Core.Models;

public class ActionScenario {
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid EquipementId { get; set; }
  public bool EtatSouhaite { get; set; }
  public TypeActionScenario TypeAction { get; set; }
  public int? OuvertureSouhaitee { get; set; }
}
