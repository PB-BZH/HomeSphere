using System.Security.Cryptography;
using System.Text;
using HomeSphere.Core.Models;

namespace HomeSphere.Core.Services;

public static class MaisonVirtuelleFactory {

  public static Maison CreerMaisonPrincipale() {

    Maison maison = new() {
      Nom = "Maison principale",
      Adresse = "Adresse fictive"
    };

    Piece salon = new() {
      Nom = "Salon",
      Image = "salon.png"
    };

    Equipement lampeSalon = new() {
      Id = CreerGuidStable("MaisonPrincipale/Salon/LampePrincipale"),
      Nom = "Lampe principale",
      Type = "Éclairage",
      Image = "ampoule.png",
      EstCommandable = true,
      EstConnecte = true,
      EstAllume = true
    };

    Equipement lampeBureau = new() {
      Id = CreerGuidStable("MaisonPrincipale/Bureau/LampeBureau"),
      Nom = "Lampe bureau",
      Type = "Éclairage",
      Image = "ampoule.png",
      EstCommandable = true,
      EstConnecte = true,
      EstAllume = true
    };

    Equipement lampeCuisine = new() {
      Id = CreerGuidStable("MaisonPrincipale/Cuisine/LampeCuisine"),
      Nom = "Éclairage cuisine",
      Type = "Éclairage",
      Image = "ampoule.png",
      EstCommandable = true,
      EstConnecte = true,
      EstAllume = true
    };

    Equipement lampeChambre = new() {
      Id = CreerGuidStable("MaisonPrincipale/Chambre/LampeChambre"),
      Nom = "Éclairage chambre",
      Type = "Éclairage",
      Image = "ampoule.png",
      EstCommandable = true,
      EstConnecte = true,
      EstAllume = true
    };

    Thermometre thermometreCuisine = new() {
      Id = CreerGuidStable("MaisonPrincipale/Cuisine/ThermometreCuisine"),
      Nom = "Thermometre cuisine",
      Type = "Mesure temperature",
      TemperatureReference = 21.5,
      Temperature = 21.5
    };

    Thermometre thermometreSalon = new() {
      Id = CreerGuidStable("MaisonPrincipale/Salon/ThermometreSalon"),
      Nom = "Thermometre salon",
      Type = "Mesure temperature",
      Temperature = 18.5,
      TemperatureReference = 18.5,
    };

    Thermometre thermometreChambre = new() {
      Id = CreerGuidStable("MaisonPrincipale/Chambre/ThermometreChambre"),
      Nom = "Thermometre chambre",
      Type = "Mesure temperature",
      TemperatureReference = 18.0,
      Temperature = 18.0
    };

    Thermometre thermometreBureau = new() {
      Id = CreerGuidStable("MaisonPrincipale/Bureau/ThermometreBureau"),
      Nom = "Thermometre bureau",
      Type = "Mesure temperature",
      TemperatureReference = 20.0,
      Temperature = 20.0
    };

    Equipement priseCuisine = new() {
      Id = CreerGuidStable("MaisonPrincipale/Cuisine/PriseCuisine"),
      Nom = "Prise cuisine",
      EstCommandable = true,
      EstConnecte = true,
      Type = "Prise de courant",
      Image = "prise.png"
    };

    Equipement priseBureau = new() {
      Id = CreerGuidStable("MaisonPrincipale/Bureau/PriseBureau"),
      Nom = "Prise bureau",
      Type = "Prise de courant",
      Image = "prise.png",
      EstCommandable = true,
      EstConnecte = true,
    };

    Equipement priseSalon = new() {
      Id = CreerGuidStable("MaisonPrincipale/Salon/PriseSalon"),
      Nom = "Prise salon",
      Type = "Prise de courant",
      Image = "prise.png",
      EstCommandable = true,
      EstConnecte = true,
    };

    Equipement priseChambre = new() {
      Id = CreerGuidStable("MaisonPrincipale/Chambre/PriseChambre"),
      Nom = "Prise chambre",
      Type = "Prise de courant",
      EstCommandable = true,
      Image = "prise.png",
      EstConnecte = true,
      EstAllume = true,
    };

    Piece cuisine = new() {
      Nom = "Cuisine",
      Image = "cuisine.png"
    };

    Piece bureau = new() {
      Nom = "Bureau",
      Image = "bureau.png"
    };

    Piece chambre = new() {
      Nom = "Chambre",
      Image = "chambre.png"
    };

    salon.Equipements.Add(lampeSalon);
    salon.Equipements.Add(thermometreSalon);
    salon.Equipements.Add(priseSalon);
    bureau.Equipements.Add(lampeBureau);
    bureau.Equipements.Add(thermometreBureau);
    bureau.Equipements.Add(priseBureau);
    cuisine.Equipements.Add(lampeCuisine);
    cuisine.Equipements.Add(thermometreCuisine);
    cuisine.Equipements.Add(priseCuisine);
    chambre.Equipements.Add(lampeChambre);
    chambre.Equipements.Add(priseChambre);
    chambre.Equipements.Add(thermometreChambre);
    salon.Equipements.Add(new VoletRoulant {
      Id = CreerGuidStable("MaisonPrincipale/Salon/VoletRoulant"),
      Nom = "Volet roulant salon",
      Ouverture = 50,
      Image = "volet.png"
    });
    chambre.Equipements.Add(new VoletRoulant {
      Id = CreerGuidStable("MaisonPrincipale/Salon/VoletRoulant"),
      Nom = "Volet roulant chambre",
      Ouverture = 50,
      Image = "volet.png"
    });
    bureau.Equipements.Add(new VoletRoulant {
      Id = CreerGuidStable("MaisonPrincipale/Salon/VoletRoulant"),
      Nom = "Volet roulant bureau",
      Ouverture = 50,
      Image = "volet.png"
    });
    cuisine.Equipements.Add(new VoletRoulant {
      Id = CreerGuidStable("MaisonPrincipale/Salon/VoletRoulant"),
      Nom = "Volet roulant cuisine",
      Ouverture = 50,
      Image = "volet.png"
    });
    maison.Pieces.Add(bureau);
    maison.Pieces.Add(salon);
    maison.Pieces.Add(chambre);
    maison.Pieces.Add(cuisine);

    return maison;
  }

  private static Guid CreerGuidStable(string cle) {
    byte[] donnees = Encoding.UTF8.GetBytes(cle);
    byte[] hash = SHA256.HashData(donnees);
    return new Guid(hash.AsSpan(0,16));
  }
}