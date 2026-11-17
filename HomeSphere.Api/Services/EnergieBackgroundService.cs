using HomeSphere.Core.Enums;
using HomeSphere.Core.Services;

namespace HomeSphere.Api.Services;

public class EnergieBackgroundService: BackgroundService {
  private readonly EnergieService _energieService;
  private readonly HistoriqueEnergieService _historiqueEnergieService;
  private readonly EtatEquipementsService _etatEquipementsService;

  public EnergieBackgroundService(EnergieService energieService,
                                  HistoriqueEnergieService historiqueEnergieService,
                                  EtatEquipementsService etatEquipementsService) {
    _energieService = energieService;
    _historiqueEnergieService = historiqueEnergieService;
    _etatEquipementsService = etatEquipementsService;
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));

    try {
      while (await timer.WaitForNextTickAsync(stoppingToken)) {
        _energieService.ActualiserConsommation_kWh();
        _energieService.ActualiserConsommationParPiece_kWh();
        _energieService.SauvegarderEnergies();

        _historiqueEnergieService.CreerReleve();
        _historiqueEnergieService.SauvegarderHistorique();
      }
    }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
      // Arrêt normal du serveur.
    }
  }

  public override async Task StartAsync(CancellationToken cancellationToken) {
    _energieService.ReprendreMesuresApresVeille();
    _historiqueEnergieService.CreerReleve(TypeReleveEnergie.PremierApresDemarrage);
    _historiqueEnergieService.SauvegarderHistorique();

    await base.StartAsync(cancellationToken);
  }

  public override async Task StopAsync(CancellationToken cancellationToken) {
    await base.StopAsync(cancellationToken);

    _etatEquipementsService.SauvegarderEtatEquipements();
    _historiqueEnergieService.CreerReleve(TypeReleveEnergie.DernierAvantArret);
    _energieService.SauvegarderEnergies();
    _historiqueEnergieService.SauvegarderHistorique();
  }
}