using HomeSphere.Core.Services;

namespace HomeSphere.Api.Services;

public class SimulationTemperaturesBackgroundService: BackgroundService {
  private readonly SimulationTemperaturesService _simulationTemperaturesService;

  public SimulationTemperaturesBackgroundService(SimulationTemperaturesService simulationTemperaturesService) {
    _simulationTemperaturesService = simulationTemperaturesService;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

    while (await timer.WaitForNextTickAsync(stoppingToken)) {
      _simulationTemperaturesService.ActualiserTemperatures();
    }
  }
}