namespace HomeSphere.Core.Models;

public class Prise: Equipement {
  public override double Puissance => EstConnecte && EstAllume ? 250.0 : 0.0;
  public Prise() {
    Type = "Prise";
    EstCommandable = true;
    EstConnecte = true;
    Image = "prise.png";
  }
}
