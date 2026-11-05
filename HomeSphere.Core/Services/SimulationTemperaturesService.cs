using HomeSphere.Core.Models;

namespace HomeSphere.Core.Services;

public class SimulationTemperaturesService {
  private readonly HomeSphere _homeSphere;

  public SimulationTemperaturesService(HomeSphere homeSphere) {
    _homeSphere = homeSphere;
  }

  public void ActualiserTemperatures() {
    foreach (Maison maison in _homeSphere.maisons) {
      foreach (Piece piece in maison.Pieces) {
        foreach (Equipement equipement in piece.Equipements) {
          if (equipement is Thermometre thermometre) {
            thermometre.Temperature = RegulationTemperature(thermometre);
          }
        }
      }
    }
  }

  private double RegulationTemperature(Thermometre thermometre) {
    double temperature = thermometre.Temperature;
    double tempRef = thermometre.TemperatureReference;
    double threshold = 1.0;

    double tempMax = tempRef + threshold;
    double tempMin = tempRef - threshold;

    if (temperature >= tempMax)
      thermometre.TemperatureMonte = false;
    else if (temperature <= tempMin)
      thermometre.TemperatureMonte = true;

    temperature += thermometre.TemperatureMonte ? 0.1 : -0.1;

    return Math.Round(temperature,1);
  }
}
