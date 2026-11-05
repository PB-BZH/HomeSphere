using System.Text.Json;
using HomeSphere.Core.Enums;
using HomeSphere.Core.Models;

namespace HomeSphere.Core.Services;

public class ScenarioService {
  private readonly HomeSphere _homeSphere;
  public List<Scenario> Scenarios { get; set; } = [];
  private ActionScenario? _selectedActionScenario { get; set; }

  public ScenarioService(HomeSphere homeSphere) {
    _homeSphere = homeSphere;
    if (File.Exists(CheminFichierScenarios)) {
      ChargerScenarios();
    }
    else {
      Scenarios.Add(CreerScenarioBonneNuit());
      Scenarios.Add(CreerScenarioBonjour());
      SauvegarderScenarios();
    }
  }

  private string CheminFichierScenarios {
    get {
      string dossierAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      string dossierHomeSphere = Path.Combine(dossierAppData,"HomeSphere");

      Directory.CreateDirectory(dossierHomeSphere);

      return Path.Combine(dossierHomeSphere,"scenarios.json");
    }
  }

  public void SauvegarderScenarios() {
    string json = JsonSerializer.Serialize(Scenarios,new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(CheminFichierScenarios,json);
  }

  public void ChargerScenarios() {
    if (File.Exists(CheminFichierScenarios)) {
      string json = File.ReadAllText(CheminFichierScenarios);
      var scenarios = JsonSerializer.Deserialize<List<Scenario>>(json);

      if (scenarios is not null) {
        Scenarios.Clear();
        Scenarios.AddRange(scenarios);
      }
    }
  }

  public void AjouterScenario(Scenario scenario) {
    Scenarios.Add(scenario);
    SauvegarderScenarios();
  }

  public void SupprimerScenario(Scenario scenario) {
    var scenarioExistant = Scenarios.FirstOrDefault(s => s.Id == scenario.Id);

    if (scenarioExistant is not null) {
      Scenarios.Remove(scenarioExistant);
      SauvegarderScenarios();
    }
  }

  public void ModifierScenario(Scenario scenario) {
    var scenarioExistant = Scenarios.FirstOrDefault(s => s.Id == scenario.Id);
    if (scenarioExistant is not null) {
      scenarioExistant.Nom = scenario.Nom;
      scenarioExistant.Description = scenario.Description;
      scenarioExistant.Icone = scenario.Icone;
      scenarioExistant.CouleurIcone = scenario.CouleurIcone;
      scenarioExistant.ActionScenarios = scenario.ActionScenarios;
      SauvegarderScenarios();
    }
  }

  private Scenario CreerScenarioBonneNuit() {
    var equipements = _homeSphere.maisons.SelectMany(m => m.Pieces)
                                         .SelectMany(p => p.Equipements)
                                         .ToList();

    var actionsLampes = equipements
        .Where(e => e.EstCommandable && e.Type == "Éclairage")
        .Select(e => new ActionScenario {
          EquipementId = e.Id,
          TypeAction = TypeActionScenario.MarcheArret,
          EtatSouhaite = false
        });

    var actionsVolets = equipements
        .OfType<VoletRoulant>()
        .Where(v => v.EstCommandable)
        .Select(v => new ActionScenario {
          EquipementId = v.Id,
          TypeAction = TypeActionScenario.PositionVolet,
          OuvertureSouhaitee = 0
        });

    return new Scenario {
      Nom = "Bonne nuit",
      Description = "Éteint les lampes et ferme les volets roulants",
      Icone = "🌙",
      CouleurIcone = "MediumPurple",
      ActionScenarios = actionsLampes.Concat(actionsVolets).ToList()
    };
  }

  private Scenario CreerScenarioBonjour() {
    var equipements = _homeSphere.maisons.SelectMany(m => m.Pieces)
                                         .SelectMany(p => p.Equipements)
                                         .ToList();

    var actionsLampes = equipements
        .Where(e => e.EstCommandable && e.Type == "Éclairage")
        .Select(e => new ActionScenario {
          EquipementId = e.Id,
          TypeAction = TypeActionScenario.MarcheArret,
          EtatSouhaite = true
        });

    var actionsVolets = equipements
        .OfType<VoletRoulant>()
        .Where(v => v.EstCommandable)
        .Select(v => new ActionScenario {
          EquipementId = v.Id,
          TypeAction = TypeActionScenario.PositionVolet,
          OuvertureSouhaitee = 100
        });

    return new Scenario {
      Nom = "Bonjour",
      Description = "Allume les lampes et ouvre les volets roulants",
      Icone = "☀",
      CouleurIcone = "Yellow",
      ActionScenarios = actionsLampes.Concat(actionsVolets).ToList()
    };
  }

  private Equipement? ObtenirEquipement(ActionScenario actionScenario) {
    return _homeSphere.maisons.SelectMany(m => m.Pieces)
                              .SelectMany(p => p.Equipements)
                              .FirstOrDefault(e => e.Id == actionScenario.EquipementId);

  }

  public ResultatScenario ExecuterScenario(Scenario scenario) {
    int appliquees = 0;
    int ignorees = 0;

    foreach (var action in scenario.ActionScenarios) {
      var equipement = ObtenirEquipement(action);

      if (equipement is null || !equipement.EstConnecte || !equipement.EstCommandable) {
        ignorees++;
        continue;
      }

      switch (action.TypeAction) {
        case TypeActionScenario.MarcheArret when equipement.EstCommandeMarcheArret:
          equipement.EstAllume = action.EtatSouhaite;
          appliquees++;
          break;

        case TypeActionScenario.PositionVolet when equipement is VoletRoulant volet
              && action.OuvertureSouhaitee is >= 0 and <= 100:
          volet.Ouverture = action.OuvertureSouhaitee.Value;
          appliquees++;
          break;

        default:
          ignorees++;
          break;
      }
    }

    return new ResultatScenario(appliquees,ignorees);
  }
}

