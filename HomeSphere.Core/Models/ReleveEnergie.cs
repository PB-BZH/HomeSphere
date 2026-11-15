using HomeSphere.Core.Enums;

namespace HomeSphere.Core.Models;

public class ReleveEnergie {
  public DateTimeOffset Horodatage { get; set; }
  public TypeReleveEnergie TypeReleve { get; set; } = TypeReleveEnergie.Normal;
  public double PuissanceTotale_W { get; set; }
  public double EnergieTotaleConsommee_kWh { get; set; }
  public Dictionary<Guid,double> PuissanceParPiece_W { get; set; } = new();
  public Dictionary<Guid,double> EnergieConsommeeParPiece_kWh { get; set; } = new();
}