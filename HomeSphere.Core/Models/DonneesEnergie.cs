namespace HomeSphere.Core.Models;

public class DonneesEnergie {
  public required double EnergieTotaleKWh { get; set; }
  public required Dictionary<Guid,double> EnergieParPieceKWh { get; set; }
  public required double DureeObservationGlobaleHeures { get; set; }
  public required double DureeObservationParPieceHeures { get; set; }
}