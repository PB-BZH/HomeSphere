using HomeSphere.Core.Models;

namespace HomeSphere.Core.Services;

public class CommandeEquipementsService {
  private readonly HomeSphere _homeSphere;

  public CommandeEquipementsService(HomeSphere homeSphere) {
    _homeSphere = homeSphere;
  }

  public Equipement? TrouverEquipement(Guid id) {
    Equipement? equipement = _homeSphere.maisons.SelectMany(m => m.Pieces)
                                    .SelectMany(p => p.Equipements)
                                    .FirstOrDefault(e => e.Id == id);

    return equipement;
  }

  public bool BasculerEquipement(Guid id) {
    Equipement? equipement = TrouverEquipement(id);

    if (equipement == null)
      return false;

    if (equipement.EstCommandable && equipement.EstConnecte && equipement.EstCommandeMarcheArret) {
      equipement.EstAllume = !equipement.EstAllume;
      return true;
    }

    return false;
  }
}
