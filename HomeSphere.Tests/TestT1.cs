using System.IO;
using System.Text.Json;
using HomeSphere.Core.Models;
using HomeSphere.Core.Services;
using HomeSphere.Wpf.ViewModels;

namespace HomeSphere.Tests;

[TestClass]
public class EnergieServiceTests {

  [TestMethod]
  public void TestConsommation290W_Pendant1Heure() {
    // Préparation
    double puissance_W = 290.0;
    TimeSpan duree = TimeSpan.FromHours(1);
    double energieAttendue_kWh = 0.290;

    // Exécution
    double energieCalculee_kWh = EnergieService.CalculerEnergie_kWh(puissance_W,duree);

    // Vérification
    Assert.AreEqual(energieAttendue_kWh,energieCalculee_kWh,0.000001);
  }

  [TestMethod]
  public void TestConsommation290W_Puis250W() {
    // Préparation
    double puissanceInitiale_W = 290.0;
    double puissanceFinale_W = 250.0;
    TimeSpan duree = TimeSpan.FromMinutes(30);
    double energieAttendue_kWh = 0.270;

    // Exécution
    double energie1 = EnergieService.CalculerEnergie_kWh(puissanceInitiale_W,duree);
    double energie2 = EnergieService.CalculerEnergie_kWh(puissanceFinale_W,duree);
    double energieTotale = energie1 + energie2;

    // Vérification
    Assert.AreEqual(energieAttendue_kWh,energieTotale,0.000001);
  }

  [TestMethod]
  public void TestCumulReel290W_Puis250W() {
    // Préparation
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      Assert.AreEqual(290.0,service.CalculerPuissanceTotale_W(),0.000001);

      // Première période : 290 W pendant 30 minutes
      tempsSimule = tempsSimule.AddMinutes(30);

      // Extinction des quatre lampes
      foreach (Lampe lampe in homeSphere.maisons.SelectMany(m => m.Pieces)
                                                .SelectMany(p => p.Equipements)
                                                .OfType<Lampe>())
        lampe.EstAllume = false;

      service.ActualiserConsommation_kWh();

      // Deuxième période : 250 W pendant 30 minutes
      tempsSimule = tempsSimule.AddMinutes(30);
      service.ActualiserConsommation_kWh();

      // Vérification
      Assert.AreEqual(250.0,service.CalculerPuissanceTotale_W(),0.000001);
      Assert.AreEqual(0.270,service.EnergieTotaleKWh,0.000001);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestCumulReel250W_Pendant2h() {
    // Préparation
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      // Extinction des quatre lampes
      foreach (Lampe lampe in homeSphere.maisons.SelectMany(m => m.Pieces)
                                                .SelectMany(p => p.Equipements)
                                                .OfType<Lampe>())
        lampe.EstAllume = false;

      service.ActualiserConsommation_kWh();
      Assert.AreEqual(0.0,service.EnergieTotaleKWh,0.000001);

      // Deuxième période : 250 W pendant 2 heures
      tempsSimule = tempsSimule.AddHours(2);
      service.ActualiserConsommation_kWh();

      // Vérification
      Assert.AreEqual(250.0,service.CalculerPuissanceTotale_W(),0.000001);
      Assert.AreEqual(0.500,service.EnergieTotaleKWh,0.000001);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestCumulReel_pPuissanceNulle() {
    // Préparation
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      // Extinction des quatre lampes
      foreach (Lampe lampe in homeSphere.maisons.SelectMany(m => m.Pieces)
                                                .SelectMany(p => p.Equipements)
                                                .OfType<Lampe>())
        lampe.EstAllume = false;

      foreach (Prise prise in homeSphere.maisons.SelectMany(m => m.Pieces)
                                                .SelectMany(p => p.Equipements)
                                                .OfType<Prise>())
        prise.EstAllume = false;

      service.ActualiserConsommation_kWh();
      Assert.AreEqual(0.0,service.EnergieTotaleKWh,0.000001);

      // Période : 0 W pendant 2 heures
      tempsSimule = tempsSimule.AddHours(2);
      service.ActualiserConsommation_kWh();

      // Vérification
      Assert.AreEqual(0.0,service.CalculerPuissanceTotale_W(),0.000001);
      Assert.AreEqual(0.0,service.EnergieTotaleKWh,0.000001);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestVariationAvecDeclanchementEvenement() {
    // Préparation
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      foreach (Equipement equipement in homeSphere.maisons.SelectMany(m => m.Pieces)
                                                    .SelectMany(p => p.Equipements)) {
        equipement.PropertyChanged += (sender,e) => {
          if (e.PropertyName == nameof(Equipement.Puissance)) {
            service.ActualiserConsommation_kWh();
            service.ActualiserConsommationParPiece_kWh();
          }
        };
      }

      // Extinction de la lampe du bureau
      Guid idBureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                 .First(p => p.Nom == "Bureau").Id;

      Lampe lampeBureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                     .Where(p => p.Id == idBureau)
                                     .SelectMany(p => p.Equipements)
                                     .OfType<Lampe>()
                                     .First();

      lampeBureau.EstAllume = false;


      Assert.AreEqual(0.0,service.EnergieTotaleKWh,0.000001);
      Assert.AreEqual(280.0,service.CalculerPuissanceTotale_W(),1e-6);

      // Première période : lampe éteinte pendant 1 heure
      tempsSimule = tempsSimule.AddHours(1);
      service.ActualiserConsommation_kWh();
      Assert.AreEqual(0.280,service.EnergieTotaleKWh,0.000001);
      Assert.AreEqual(280.0,service.CalculerPuissanceTotale_W(),1e-6);

      lampeBureau.EstAllume = true;

      Assert.AreEqual(0.280,service.EnergieTotaleKWh,0.000001);
      Assert.AreEqual(290.0,service.CalculerPuissanceTotale_W(),1e-6);

      // Première période : lampe allumée
      tempsSimule = tempsSimule.AddHours(1);
      service.ActualiserConsommation_kWh();

      Assert.AreEqual(0.570,service.EnergieTotaleKWh,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestVariationAvecDeclanchementEvenement_ParPiece() {
    // Préparation
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      foreach (Equipement equipement in homeSphere.maisons.SelectMany(m => m.Pieces)
                                                    .SelectMany(p => p.Equipements)) {
        equipement.PropertyChanged += (sender,e) => {
          if (e.PropertyName == nameof(Equipement.Puissance)) {
            service.ActualiserConsommation_kWh();
            service.ActualiserConsommationParPiece_kWh();
          }
        };
      }

      // Extinction de la lampe du bureau
      Guid idBureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                 .First(p => p.Nom == "Bureau").Id;

      Lampe lampeBureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                     .Where(p => p.Id == idBureau)
                                     .SelectMany(p => p.Equipements)
                                     .OfType<Lampe>()
                                     .First();

      lampeBureau.EstAllume = false;

      Prise priseBureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                     .Where(p => p.Id == idBureau)
                                     .SelectMany(p => p.Equipements)
                                     .OfType<Prise>()
                                     .First();

      priseBureau.EstAllume = false;

      Assert.AreEqual(0.0,service.EnergieTotaleKWh,0.000001);
      Assert.AreEqual(0.0,service.CalculerPuissanceParPiece_W()[idBureau],0.000001);

      // Première période : lampe éteinte pendant 1 heure
      tempsSimule = tempsSimule.AddHours(1);
      service.ActualiserConsommation_kWh();
      service.ActualiserConsommationParPiece_kWh();

      Assert.AreEqual(0.0,service.EnergieParPiece_kWh[idBureau],1e-6);
      Assert.AreEqual(0.0,service.CalculerPuissanceParPiece_W()[idBureau],1e-6);


      lampeBureau.EstAllume = true;

      Assert.AreEqual(0.0,service.EnergieParPiece_kWh[idBureau],1e-6);
      Assert.AreEqual(10.0,service.CalculerPuissanceParPiece_W()[idBureau],1e-6);

      tempsSimule = tempsSimule.AddHours(1);
      service.ActualiserConsommation_kWh();
      service.ActualiserConsommationParPiece_kWh();

      double sommeEnergiesPieces = service.EnergieParPiece_kWh.Values.Sum();

      Assert.AreEqual(0.570,service.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(service.EnergieTotaleKWh,sommeEnergiesPieces,1e-6);

      Assert.AreEqual(0.01,service.EnergieParPiece_kWh[idBureau],1e-6);
      Assert.AreEqual(10.0,service.CalculerPuissanceParPiece_W()[idBureau],1e-6);

    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestVariationEnergieSelonScenario() {
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);
      ScenarioService scenarioService = new(homeSphere,dossierTest);

      Scenario bonneNuit = scenarioService.Scenarios.First(s => s.Nom == "Bonne nuit");
      Scenario bonjour = scenarioService.Scenarios.First(s => s.Nom == "Bonjour");

      // Ici, nous conserverons les abonnements PropertyChanged.
      foreach (Equipement equipement in homeSphere.maisons.SelectMany(m => m.Pieces)
                                              .SelectMany(p => p.Equipements)) {
        equipement.PropertyChanged += (sender,e) => {
          if (e.PropertyName == nameof(Equipement.Puissance)) {
            service.ActualiserConsommation_kWh();
            service.ActualiserConsommationParPiece_kWh();
          }
        };
      }


      // Première période : scenario "Bonne nuit" pendant 1 heure
      Assert.AreEqual(290.0,service.CalculerPuissanceTotale_W(),1e-6);

      scenarioService.ExecuterScenario(bonneNuit);
      tempsSimule = tempsSimule.AddHours(1);
      service.ActualiserConsommation_kWh();
      service.ActualiserConsommationParPiece_kWh();

      Assert.AreEqual(0.250,service.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(250.0,service.CalculerPuissanceTotale_W(),1e-6);

      // Seconde période : scenario "Bonne nuit" pendant 1 heure
      scenarioService.ExecuterScenario(bonjour);
      tempsSimule = tempsSimule.AddHours(1);
      service.ActualiserConsommation_kWh();
      service.ActualiserConsommationParPiece_kWh();

      Assert.AreEqual(0.540,service.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(290.0,service.CalculerPuissanceTotale_W(),1e-6);


    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }


  [TestMethod]
  public void TestPersistancePendantArret() {
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      // Première heure de fonctionnement
      tempsSimule = tempsSimule.AddHours(1);

      service.ActualiserConsommation_kWh();
      service.ActualiserConsommationParPiece_kWh();
      service.SauvegarderEnergies();

      Assert.AreEqual(0.290,service.EnergieTotaleKWh,1e-6);

      // Application arrêtée pendant 10 heures
      tempsSimule = tempsSimule.AddHours(10);

      // redémarrage de l'application
      Core.HomeSphere homeSphereRedemarree = new();
      EnergieService serviceRedemarre = new(homeSphereRedemarree,() => tempsSimule,dossierTest);

      Assert.AreEqual(0.290,serviceRedemarre.EnergieTotaleKWh,1e-6);

      // Une heure de consommation supplémentaire 
      tempsSimule = tempsSimule.AddHours(1);
      serviceRedemarre.ActualiserConsommation_kWh();
      serviceRedemarre.ActualiserConsommationParPiece_kWh();

      Assert.AreEqual(0.580,serviceRedemarre.EnergieTotaleKWh,1e-6);

      // Vérification par pièce
      double sommeEnergiesPieces = serviceRedemarre.EnergieParPiece_kWh.Values.Sum();
      Assert.AreEqual(serviceRedemarre.EnergieTotaleKWh,sommeEnergiesPieces,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestPersistancePendantArretApresModificationEtats() {
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    string dossierAppData = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "HomeSphere");

    string fichierSource = Path.Combine(dossierAppData,"scenarios.json");
    string fichierTest = Path.Combine(dossierTest,"scenarios.json");

    Directory.CreateDirectory(dossierTest);
    File.Copy(fichierSource,fichierTest,true);


    try {
      Core.HomeSphere homeSphere = new();
      ScenarioService scenarioService = new(homeSphere,dossierTest);

      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      EtatEquipementsService etatService = new(homeSphere,dossierTest);

      Scenario testConsommationMaximale = scenarioService.Scenarios.First(s => s.Nom == "Test Consommation maximale");  // 1040 W
      Scenario bonneNuit = scenarioService.Scenarios.First(s => s.Nom == "Bonne nuit");
      Scenario bonjour = scenarioService.Scenarios.First(s => s.Nom == "Bonjour");

      // Ici, nous conserverons les abonnements PropertyChanged.
      foreach (Equipement equipement in homeSphere.maisons.SelectMany(m => m.Pieces)
                                              .SelectMany(p => p.Equipements)) {
        equipement.PropertyChanged += (sender,e) => {
          if (e.PropertyName == nameof(Equipement.Puissance)) {
            service.ActualiserConsommation_kWh();
            service.ActualiserConsommationParPiece_kWh();
          }
        };
      }

      // Première heure de fonctionnement démarrage sur 290 W
      tempsSimule = tempsSimule.AddHours(1);

      Assert.AreEqual(0.0,service.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(290.0,service.CalculerPuissanceTotale_W(),1e-6);

      scenarioService.ExecuterScenario(testConsommationMaximale);// 1040 W
      tempsSimule = tempsSimule.AddHours(1);
      service.ActualiserConsommation_kWh();
      service.ActualiserConsommationParPiece_kWh();

      Assert.AreEqual(1040.0,service.CalculerPuissanceTotale_W(),1e-6);
      Assert.AreEqual(1.330,service.EnergieTotaleKWh,1e-6);

      // Arrêt de l'application
      service.ActualiserConsommation_kWh();
      service.ActualiserConsommationParPiece_kWh();

      etatService.SauvegarderEtatEquipements();
      service.SauvegarderEnergies();

      // Application arrêtée pendant 10 heures
      tempsSimule = tempsSimule.AddHours(10);

      // redémarrage de l'application
      Core.HomeSphere homeSphereRedemarree = new();
      EnergieService serviceRedemarre = new(homeSphereRedemarree,() => tempsSimule,dossierTest);

      ScenarioService scenarioServiceRedemarre = new(homeSphereRedemarree,dossierTest);
      Scenario bonneNuitRedemarre = scenarioServiceRedemarre.Scenarios.First(s => s.Nom == "Bonne nuit");
      Scenario bonjourRedemarre = scenarioServiceRedemarre.Scenarios.First(s => s.Nom == "Bonjour");

      foreach (Equipement equipement in homeSphereRedemarree.maisons.SelectMany(m => m.Pieces)
                                                              .SelectMany(p => p.Equipements)) {
        equipement.PropertyChanged += (sender,e) => {
          if (e.PropertyName == nameof(Equipement.Puissance)) {
            serviceRedemarre.ActualiserConsommation_kWh();
            serviceRedemarre.ActualiserConsommationParPiece_kWh();
          }
        };
      }

      Assert.AreEqual(1.330,serviceRedemarre.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(1040,serviceRedemarre.CalculerPuissanceTotale_W(),1e-6);

      scenarioServiceRedemarre.ExecuterScenario(bonneNuitRedemarre); // 250 W prise chambre allumée
      tempsSimule = tempsSimule.AddHours(1);
      serviceRedemarre.ActualiserConsommation_kWh();
      serviceRedemarre.ActualiserConsommationParPiece_kWh();

      Assert.AreEqual(1.580,serviceRedemarre.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(250,serviceRedemarre.CalculerPuissanceTotale_W(),1e-6);

      // Une heure de consommation supplémentaire 
      scenarioServiceRedemarre.ExecuterScenario(bonjourRedemarre); // 290 W prise chambre allumée
      tempsSimule = tempsSimule.AddHours(1);
      serviceRedemarre.ActualiserConsommation_kWh();
      serviceRedemarre.ActualiserConsommationParPiece_kWh();

      Assert.AreEqual(1.870,serviceRedemarre.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(290,serviceRedemarre.CalculerPuissanceTotale_W(),1e-6);

      // Vérification par pièce
      double sommeEnergiesPieces = serviceRedemarre.EnergieParPiece_kWh.Values.Sum();
      Assert.AreEqual(serviceRedemarre.EnergieTotaleKWh,sommeEnergiesPieces,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestEnergieJsonAbsent() {
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      // Le fichier n'existe pas avant le démarrage
      Assert.IsFalse(File.Exists(Path.Combine(dossierTest,"energie.json")));

      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      // Le service doit démarrer avec une consommation nulle
      Assert.AreEqual(0.0,service.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(290.0,service.CalculerPuissanceTotale_W(),1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestEnergieJsonCorrompu() {
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Directory.CreateDirectory(dossierTest);

      string fichierEnergie = Path.Combine(dossierTest,"energie.json");
      File.WriteAllText(fichierEnergie,"{ JSON volontairement invalide");

      Core.HomeSphere homeSphere = new();

      Assert.ThrowsExactly<InvalidDataException>(() => {
        EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);
      });
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestEnergieJsonVide() {
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Directory.CreateDirectory(dossierTest);

      string fichierEnergie = Path.Combine(dossierTest,"energie.json");
      File.WriteAllText(fichierEnergie,string.Empty);

      Core.HomeSphere homeSphere = new();

      Assert.ThrowsExactly<InvalidDataException>(() => {
        EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);
      });
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestEnergieJsonErroné() {
    DateTime tempsSimule = new(2026,1,1,10,0,0);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Directory.CreateDirectory(dossierTest);

      string fichierEnergie = Path.Combine(dossierTest,"energie.json");
      var donnees = new DonneesEnergie {
        EnergieTotaleKWh = -0.500,
        EnergieParPieceKWh = new Dictionary<Guid,double>(),
        DureeObservationGlobaleHeures = 1.0,
        DureeObservationParPieceHeures = 1.0
      };

      File.WriteAllText(fichierEnergie,JsonSerializer.Serialize(donnees));

      Core.HomeSphere homeSphere = new();

      Assert.ThrowsExactly<InvalidDataException>(() => {
        EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);
      });
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestNavigationSansReinitialisationEnergyViewModel() {
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      MainViewModel mainViewModel = new(dossierTest,activerTimer: false);

      // Ouverture de la page Énergie
      mainViewModel.AfficherEnergieCommand.Execute(null);
      Assert.AreEqual("Energy",mainViewModel.CurrentPage);

      object energyViewModelInitial = mainViewModel.CurrentViewModel;

      // Navigation vers les pièces
      mainViewModel.AfficherPiecesCommand.Execute(null);
      Assert.AreEqual("Rooms",mainViewModel.CurrentPage);

      // Retour vers la page Énergie
      mainViewModel.AfficherEnergieCommand.Execute(null);

      Assert.AreEqual("Energy",mainViewModel.CurrentPage);
      Assert.AreSame(energyViewModelInitial,mainViewModel.CurrentViewModel);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestNavigationSansReinitialisationEnergie() {
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());
    DateTime tempsSimule = new(2026,1,1,10,0,0);

    try {
      // Préparation d'une consommation réelle de 0,290 kWh
      Core.HomeSphere homeSphere = new();
      EnergieService service = new(homeSphere,() => tempsSimule,dossierTest);

      tempsSimule = tempsSimule.AddHours(1);
      service.ActualiserConsommation_kWh();
      service.ActualiserConsommationParPiece_kWh();
      service.SauvegarderEnergies();

      Assert.AreEqual(0.290,service.EnergieTotaleKWh,1e-6);

      // Création du ViewModel principal avec les données sauvegardées
      MainViewModel mainViewModel = new(dossierTest,activerTimer: false,horloge: () => tempsSimule);

      // Première ouverture de la page Énergie
      mainViewModel.AfficherEnergieCommand.Execute(null);
      EnergyViewModel energyViewModelInitial = (EnergyViewModel)mainViewModel.CurrentViewModel;

      double energieInitiale = energyViewModelInitial.EnergieTotaleKWh;
      Assert.AreEqual(0.290,energieInitiale,1e-6);

      // Navigation entre plusieurs pages
      mainViewModel.AfficherPiecesCommand.Execute(null);
      mainViewModel.AfficherEquipementsCommand.Execute(null);
      mainViewModel.AfficherDashboardCommand.Execute(null);

      // Retour à la page Énergie
      mainViewModel.AfficherEnergieCommand.Execute(null);
      EnergyViewModel energyViewModelFinal = (EnergyViewModel)mainViewModel.CurrentViewModel;

      // Vérifications
      Assert.AreSame(energyViewModelInitial,energyViewModelFinal);
      Assert.AreEqual(0.290,energyViewModelFinal.EnergieTotaleKWh,1e-6);
      Assert.AreEqual(energieInitiale,energyViewModelFinal.EnergieTotaleKWh,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }
}

