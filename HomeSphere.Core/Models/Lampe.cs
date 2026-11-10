namespace HomeSphere.Core.Models;

public class Lampe: Equipement {
  public override double Puissance => EstConnecte && EstAllume ? 10.0 : 0.0;
  public Lampe() {
    Type = "Lampe";
    EstCommandable = true;
    EstConnecte = true;
    Image = "lampe.png";
  }
}
