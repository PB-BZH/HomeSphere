using HomeSphere.Core.Models;
using HomeSphere.Core.Services;

namespace HomeSphere.Core;

public class HomeSphere {
  public List<Maison> maisons { get; } = [];
  public HomeSphere() {
    maisons.Add(MaisonVirtuelleFactory.CreerMaisonPrincipale());
  }
}
