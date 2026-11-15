using HomeSphere.Core.Models;

public class DonneesHistoriqueEnergie {
  public int Version { get; set; } = 1;
  public List<ReleveEnergie> Releves { get; set; } = new();
}