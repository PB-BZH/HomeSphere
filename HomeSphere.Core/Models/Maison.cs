namespace HomeSphere.Core.Models;

public class Maison {
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Nom { get; set; } = string.Empty;
  public string Adresse { get; set; } = string.Empty;
  public List<Piece> Pieces { get; set; } = [];
}