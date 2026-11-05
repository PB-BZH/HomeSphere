namespace HomeSphere.Core.Models;

public class Piece {
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Nom { get; set; } = string.Empty;
  public List<Equipement> Equipements { get; set; } = [];
  public string Image { get; set; } = string.Empty;
}
