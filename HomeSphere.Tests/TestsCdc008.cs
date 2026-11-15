using System.IO;
using System.Text.Json;
using HomeSphere.Core.Models;
using HomeSphere.Core.Services;

namespace HomeSphere.Tests;

[TestClass]
public class TestsCdc008 {

  [TestMethod]
  public void TestH1_ReleveInitial() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule);
      ReleveEnergie releve = historiqueService.CreerReleve();

      // Les assertions 
      Assert.AreEqual(tempsSimule,releve.Horodatage);
      Assert.AreEqual(TimeSpan.Zero,releve.Horodatage.Offset);

      Assert.AreEqual(290.0,releve.PuissanceTotale_W,1e-6);
      Assert.AreEqual(0.0,releve.EnergieTotaleConsommee_kWh,1e-6);

      Assert.AreEqual(290.0,releve.PuissanceParPiece_W.Values.Sum(),1e-6);
      Assert.AreEqual(0.0,releve.EnergieConsommeeParPiece_kWh.Values.Sum(),1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH2_RelevesSuccessifs() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule);

      ReleveEnergie releve1 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      ReleveEnergie releve2 = historiqueService.CreerReleve();
      Assert.AreEqual(290.0,releve2.PuissanceTotale_W,1e-6);
      Assert.AreEqual(0.290,releve2.EnergieTotaleConsommee_kWh,1e-6);

      Piece bureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                .First(p => p.Nom == "Bureau");

      Lampe lampeBureau = bureau.Equipements.OfType<Lampe>().First();

      lampeBureau.EstAllume = false;

      ReleveEnergie releve3 = historiqueService.CreerReleve();
      Assert.AreEqual(3,historiqueService.Releves.Count);

      Assert.AreSame(releve1,historiqueService.Releves[0]);
      Assert.AreSame(releve2,historiqueService.Releves[1]);
      Assert.AreSame(releve3,historiqueService.Releves[2]);

      Assert.AreEqual(280.0,releve3.PuissanceTotale_W,1e-6);
      Assert.AreEqual(0.290,releve3.EnergieTotaleConsommee_kWh,1e-6);

      // Vérification de la persistance du relevé 1 suite à ce dernier changement
      Assert.AreEqual(290.0,releve1.PuissanceTotale_W,1e-6);
      Assert.AreEqual(0.0,releve1.EnergieTotaleConsommee_kWh,1e-6);

      Assert.AreEqual(10.0,releve1.PuissanceParPiece_W[bureau.Id],1e-6);
      Assert.AreEqual(0.0,releve1.EnergieConsommeeParPiece_kWh[bureau.Id],1e-6);

      Assert.AreEqual(0.0,releve3.PuissanceParPiece_W[bureau.Id],1e-6);

      double consommation1 = HistoriqueEnergieService.CalculerConsommationEntreReleves_kWh(releve1,releve2);
      double consommation2 = HistoriqueEnergieService.CalculerConsommationEntreReleves_kWh(releve2,releve3);
      double consommationTotale = HistoriqueEnergieService.CalculerConsommationEntreReleves_kWh(releve1,releve3);

      Assert.AreEqual(0.290,consommation1,1e-6);
      Assert.AreEqual(0.0,consommation2,1e-6);
      Assert.AreEqual(0.290,consommationTotale,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH3_SauvegardeHistorique() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      // Trois relevés successifs
      historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      historiqueService.CreerReleve();

      // Sauvegarde
      historiqueService.SauvegarderHistorique();

      // Vérifications à ajouter
      string fichierHistorique = Path.Combine(dossierTest,"Historique energie.json");

      Assert.IsTrue(File.Exists(fichierHistorique));

      string json = File.ReadAllText(fichierHistorique);
      DonneesHistoriqueEnergie? donnees = JsonSerializer.Deserialize<DonneesHistoriqueEnergie>(json);

      Assert.IsNotNull(donnees);
      Assert.AreEqual(1,donnees.Version);
      Assert.HasCount(3,donnees.Releves);

      Assert.AreEqual(290.0,donnees.Releves[0].PuissanceTotale_W,1e-6);
      Assert.AreEqual(0.0,donnees.Releves[0].EnergieTotaleConsommee_kWh,1e-6);

      Assert.AreEqual(0.290,donnees.Releves[1].EnergieTotaleConsommee_kWh,1e-6);
      Assert.AreEqual(0.580,donnees.Releves[2].EnergieTotaleConsommee_kWh,1e-6);

      Assert.AreEqual(tempsSimule,donnees.Releves[2].Horodatage);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH4_RestaurationHistorique() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      // Trois relevés successifs
      historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      historiqueService.CreerReleve();

      // Sauvegarde
      historiqueService.SauvegarderHistorique();

      // Simulation d'un redémarrage
      HistoriqueEnergieService historiqueRedemarre = new(energieService,() => tempsSimule,dossierTest);

      Assert.HasCount(3,historiqueRedemarre.Releves);

      Assert.AreEqual(0.0,historiqueRedemarre.Releves[0].EnergieTotaleConsommee_kWh,1e-6);
      Assert.AreEqual(0.290,historiqueRedemarre.Releves[1].EnergieTotaleConsommee_kWh,1e-6);
      Assert.AreEqual(0.580,historiqueRedemarre.Releves[2].EnergieTotaleConsommee_kWh,1e-6);

      Assert.AreEqual(290.0,historiqueRedemarre.Releves[2].PuissanceTotale_W,1e-6);
      Assert.AreEqual(tempsSimule,historiqueRedemarre.Releves[2].Horodatage);

      Assert.AreNotSame(historiqueService.Releves[0],historiqueRedemarre.Releves[0]);

      Assert.HasCount(4,historiqueRedemarre.Releves[2].PuissanceParPiece_W);
      Assert.HasCount(4,historiqueRedemarre.Releves[2].EnergieConsommeeParPiece_kWh);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH5_RestaurationHistoriqueJsonAbsent() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      // Le fichier n'existe pas avant le démarrage
      Assert.IsFalse(File.Exists(Path.Combine(dossierTest,"Historique energie.json")));

      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      Assert.HasCount(0,historiqueService.Releves);

      ReleveEnergie releve = historiqueService.CreerReleve();

      Assert.HasCount(1,historiqueService.Releves);
      Assert.AreEqual(290.0,releve.PuissanceTotale_W,1e-6);
      Assert.AreEqual(0.0,releve.EnergieTotaleConsommee_kWh,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH6_RestaurationHistoriqueJsonInvalide() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Directory.CreateDirectory(dossierTest);

      string fichierHistorique = Path.Combine(dossierTest,"Historique energie.json");
      File.WriteAllText(fichierHistorique,"{ JSON volontairement invalide");

      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);

      Assert.ThrowsExactly<JsonException>(() => {
        HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);
      });

      Assert.AreEqual("{ JSON volontairement invalide",File.ReadAllText(fichierHistorique));
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH7_RestaurationHistoriqueJsonVide() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Directory.CreateDirectory(dossierTest);

      string fichierHistorique = Path.Combine(dossierTest,"Historique energie.json");
      File.WriteAllText(fichierHistorique,string.Empty);

      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);

      Assert.ThrowsExactly<JsonException>(() => {
        HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);
      });

      Assert.AreEqual(string.Empty,File.ReadAllText(fichierHistorique));
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH8_RestaurationHistoriqueJsonDifferent() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Directory.CreateDirectory(dossierTest);

      string fichierHistorique = Path.Combine(dossierTest,"Historique energie.json");
      DonneesHistoriqueEnergie donnees = new() {
        Version = 99,
        Releves = new()
      };

      File.WriteAllText(fichierHistorique,JsonSerializer.Serialize(donnees));

      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      string contenuInitial = File.ReadAllText(fichierHistorique);

      Assert.ThrowsExactly<InvalidDataException>(() => {
        HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);
      });

      Assert.AreEqual(contenuInitial,File.ReadAllText(fichierHistorique));
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH9_ConsommationPeriode() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      ReleveEnergie releve1 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      ReleveEnergie releve2 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      ReleveEnergie releve3 = historiqueService.CreerReleve();

      // Calculs à vérifier
      double consommation1 = historiqueService.CalculerConsommationPeriode_kWh(
          releve1.Horodatage,releve2.Horodatage);

      double consommation2 = historiqueService.CalculerConsommationPeriode_kWh(
          releve2.Horodatage,releve3.Horodatage);

      double consommationTotale = historiqueService.CalculerConsommationPeriode_kWh(
          releve1.Horodatage,releve3.Horodatage);

      Assert.AreEqual(0.290,consommation1,1e-6);
      Assert.AreEqual(0.290,consommation2,1e-6);
      Assert.AreEqual(0.580,consommationTotale,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH10_ConsommationPiecePeriode() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      Piece bureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                .First(p => p.Nom == "Bureau");

      ReleveEnergie releve1 = historiqueService.CreerReleve();


      tempsSimule = tempsSimule.AddHours(1);
      ReleveEnergie releve2 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      ReleveEnergie releve3 = historiqueService.CreerReleve();

      // Calculs à vérifier
      double consommation1 = historiqueService.CalculerConsommationPiecePeriode_kWh(
          bureau,releve1.Horodatage,releve2.Horodatage);

      double consommation2 = historiqueService.CalculerConsommationPiecePeriode_kWh(
          bureau,releve2.Horodatage,releve3.Horodatage);

      double consommationTotale = historiqueService.CalculerConsommationPiecePeriode_kWh(
          bureau,releve1.Horodatage,releve3.Horodatage);

      Assert.AreEqual(0.010,consommation1,1e-6);
      Assert.AreEqual(0.010,consommation2,1e-6);
      Assert.AreEqual(0.020,consommationTotale,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH11_ConsommationPiecePeriodeAvecVariationDePuissance() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      foreach (Equipement equipement in homeSphere.maisons.SelectMany(m => m.Pieces)
                                              .SelectMany(p => p.Equipements)) {
        equipement.PropertyChanged += (sender,e) => {
          if (e.PropertyName == nameof(Equipement.Puissance)) {
            energieService.ActualiserConsommation_kWh();
            energieService.ActualiserConsommationParPiece_kWh();
          }
        };
      }


      Piece bureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                .First(p => p.Nom == "Bureau");

      Lampe lampeBureau = bureau.Equipements.OfType<Lampe>().First();

      ReleveEnergie releve1 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddHours(1);
      ReleveEnergie releve2 = historiqueService.CreerReleve();

      lampeBureau.EstAllume = false;

      tempsSimule = tempsSimule.AddHours(1);
      ReleveEnergie releve3 = historiqueService.CreerReleve();

      // Calculs à vérifier
      double consommation1 = historiqueService.CalculerConsommationPiecePeriode_kWh(
          bureau,releve1.Horodatage,releve2.Horodatage);

      double consommation2 = historiqueService.CalculerConsommationPiecePeriode_kWh(
          bureau,releve2.Horodatage,releve3.Horodatage);

      double consommationTotale = historiqueService.CalculerConsommationPiecePeriode_kWh(
          bureau,releve1.Horodatage,releve3.Horodatage);

      Assert.AreEqual(0.010,consommation1,1e-6);
      Assert.AreEqual(0.000,consommation2,1e-6);
      Assert.AreEqual(0.010,consommationTotale,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH12_ConsommationPlageHoraire() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      ReleveEnergie releve1 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(15); // 10h15
      ReleveEnergie releve2 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(30); // 10h45
      ReleveEnergie releve3 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(15); // 11h
      ReleveEnergie releve4 = historiqueService.CreerReleve();

      // Calculs à vérifier
      double consommation = historiqueService.CalculerConsommationPlageHoraire_kWh(
        new DateTimeOffset(2026,1,1,10,10,0,TimeSpan.Zero),
        new DateTimeOffset(2026,1,1,10,50,0,TimeSpan.Zero));

      Assert.AreEqual(0.145,consommation,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH13_ConsommationPlageHoraireParPiece() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      Piece bureau = homeSphere.maisons.SelectMany(m => m.Pieces)
                                .First(p => p.Nom == "Bureau");

      ReleveEnergie releve1 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(15); // 10h15
      ReleveEnergie releve2 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(30); // 10h45
      ReleveEnergie releve3 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(15); // 11h
      ReleveEnergie releve4 = historiqueService.CreerReleve();

      // Calculs à vérifier
      double consommationBureau = historiqueService.CalculerConsommationPiecePlageHoraire_kWh(
        bureau,
        new DateTimeOffset(2026,1,1,10,10,0,TimeSpan.Zero),
        new DateTimeOffset(2026,1,1,10,50,0,TimeSpan.Zero));

      Assert.AreEqual(0.005,consommationBureau,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH14_ConsommationsParIntervalle() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      ReleveEnergie releve1 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(15); // 10h15
      ReleveEnergie releve2 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(30); // 10h45
      ReleveEnergie releve3 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(15); // 11h
      ReleveEnergie releve4 = historiqueService.CreerReleve();

      // Calculs à vérifier
      List<ConsommationIntervalle> intervalles = historiqueService.CalculerConsommationsParIntervalle();

      Assert.HasCount(3,intervalles);
      Assert.AreEqual(0.0725,intervalles[0].EnergieConsommee_kWh,1e-6);
      Assert.AreEqual(0.1450,intervalles[1].EnergieConsommee_kWh,1e-6);
      Assert.AreEqual(0.0725,intervalles[2].EnergieConsommee_kWh,1e-6);

      Assert.AreEqual(releve1.Horodatage,intervalles[0].Debut);
      Assert.AreEqual(releve2.Horodatage,intervalles[0].Fin);
      Assert.AreEqual(releve3.Horodatage,intervalles[1].Fin);
      Assert.AreEqual(releve4.Horodatage,intervalles[2].Fin);

      double consommationTotale = intervalles.Sum(i => i.EnergieConsommee_kWh);

      Assert.AreEqual(0.290,consommationTotale,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }

  [TestMethod]
  public void TestH15_ConsommationsParIntervalledonnesMelangees() {
    DateTimeOffset tempsSimule = new(2026,1,1,10,0,0,TimeSpan.Zero);
    string dossierTest = Path.Combine(Path.GetTempPath(),"HomeSphere.Tests",Guid.NewGuid().ToString());

    try {
      Core.HomeSphere homeSphere = new();
      EnergieService energieService = new(homeSphere,() => tempsSimule.UtcDateTime,dossierTest);
      HistoriqueEnergieService historiqueService = new(energieService,() => tempsSimule,dossierTest);

      ReleveEnergie releve2 = historiqueService.CreerReleve(); // 10h00

      tempsSimule = tempsSimule.AddHours(1); // 11h00
      ReleveEnergie releve3 = historiqueService.CreerReleve();

      tempsSimule = tempsSimule.AddMinutes(30); // 11h30
      ReleveEnergie releve1 = historiqueService.CreerReleve();

      DonneesHistoriqueEnergie donnees = new() {
        Version = 1,
        Releves = [releve1,releve2,releve3]
      };

      string chemin = Path.Combine(dossierTest,"Historique energie.json");
      File.WriteAllText(chemin,JsonSerializer.Serialize(donnees));

      HistoriqueEnergieService historiqueRecharge = new(energieService,() => tempsSimule,dossierTest);

      // Calculs à vérifier
      List<ConsommationIntervalle> intervalles = historiqueRecharge.CalculerConsommationsParIntervalle();

      Assert.HasCount(2,intervalles);
      Assert.AreEqual(0.290,intervalles[0].EnergieConsommee_kWh,1e-6);
      Assert.AreEqual(0.145,intervalles[1].EnergieConsommee_kWh,1e-6);

      Assert.AreEqual(releve2.Horodatage,intervalles[0].Debut);
      Assert.AreEqual(releve3.Horodatage,intervalles[0].Fin);
      Assert.AreEqual(releve1.Horodatage,intervalles[1].Fin);

      double consommationTotale = intervalles.Sum(i => i.EnergieConsommee_kWh);

      Assert.AreEqual(0.435,consommationTotale,1e-6);
    }
    finally {
      if (Directory.Exists(dossierTest))
        Directory.Delete(dossierTest,true);
    }
  }
}
