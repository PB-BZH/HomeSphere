namespace HomeSphere.Core.Models;

public class ConsommationIntervalle {
  public DateTimeOffset Debut { get; set; }
  public DateTimeOffset Fin { get; set; }
  public double EnergieConsommee_kWh { get; set; }
}