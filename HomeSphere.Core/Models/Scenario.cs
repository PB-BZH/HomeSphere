namespace HomeSphere.Core.Models;

public class Scenario {
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Nom { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public List<ActionScenario> ActionScenarios { get; set; } = [];
  public string Icone { get; set; } = string.Empty;
  public string CouleurIcone { get; set; } = string.Empty;
}
