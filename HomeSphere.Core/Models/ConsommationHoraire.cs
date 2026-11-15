namespace HomeSphere.Core.Models;

public class ConsommationHoraire {
  public DateTimeOffset DebutHeure { get; set; }
  public double EnergieConsommee_kWh { get; set; }
}