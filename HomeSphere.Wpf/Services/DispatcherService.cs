using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HomeSphere.Core.Services;

namespace HomeSphere.Wpf.Services;

public partial class DispatcherService: ObservableObject {
  private readonly DispatcherTimer _timer;
  private readonly SimulationTemperaturesService _simulationTemperaturesService;

  public bool EstActif => _timer.IsEnabled;

  public DispatcherService(SimulationTemperaturesService simulationTemperaturesService) {
    _simulationTemperaturesService = simulationTemperaturesService;
    _timer = new DispatcherTimer();
    _timer.Interval = TimeSpan.FromSeconds(10);
    _timer.Tick += timer_Tick;
    _timer.Start();
  }

  void timer_Tick(object sender,EventArgs e) {
    _simulationTemperaturesService.ActualiserTemperatures();
  }

  public void Demarrer() {
    if (!_timer.IsEnabled) _timer.Start();
    OnPropertyChanged(nameof(EstActif));
  }

  public void Arreter() {
    if (_timer.IsEnabled) _timer.Stop();
    OnPropertyChanged(nameof(EstActif));
  }
}