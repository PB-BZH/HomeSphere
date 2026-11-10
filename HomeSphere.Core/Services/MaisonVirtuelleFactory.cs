using System.Security.Cryptography;
using System.Text;
using HomeSphere.Core.Models;

namespace HomeSphere.Core.Services;

public static class MaisonVirtuelleFactory {

  public static Maison CreerMaisonPrincipale() {

    Maison maison = new() {
      Nom = "Maison principale",
      Adresse = "App 1, 7 rue Général Gouraud, 29200 BREST"
    };
    maison.Id = CreerGuidStable(maison.Adresse);

    Piece salon = new() {
      Nom = "Salon",
      Image = "salon.png"
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

    Lampe lampeSalon = new() {
      Nom = "Lampe principale",
      Type = "Éclairage",
      Image = "ampoule.png",
      EstCommandable = true,
      EstConnecte = true,
      EstAllume = true
    };

    Lampe lampeBureau = new() {
      Nom = "Lampe bureau",
      Type = "Éclairage",
      Image = "ampoule.png",
      EstCommandable = true,
      EstConnecte = true,
      EstAllume = true
    };

    Lampe lampeCuisine = new() {
      Nom = "Éclairage cuisine",
      Type = "Éclairage",
      Image = "ampoule.png",
      EstCommandable = true,
      EstConnecte = true,
      EstAllume = true
    };

    Lampe lampeChambre = new() {
      Nom = "Éclairage chambre",
      Type = "Éclairage",
      Image = "ampoule.png",
      EstCommandable = true,
      EstConnecte = true,
      EstAllume = true
    };

    Thermometre thermometreCuisine = new() {
      Nom = "Thermometre cuisine",
      Type = "Mesure temperature",
      TemperatureReference = 21.5,
      Temperature = 21.5
    };

    Thermometre thermometreSalon = new() {
      Nom = "Thermometre salon",
      Type = "Mesure temperature",
      Temperature = 18.5,
      TemperatureReference = 18.5,
    };

    Thermometre thermometreChambre = new() {
      Nom = "Thermometre chambre",
      Type = "Mesure temperature",
      TemperatureReference = 18.0,
      Temperature = 18.0
    };

    Thermometre thermometreBureau = new() {
      Nom = "Thermometre bureau",
      Type = "Mesure temperature",
      TemperatureReference = 20.0,
      Temperature = 20.0
    };

    Prise priseCuisine = new() {
      Nom = "Prise cuisine",
      EstCommandable = true,
      EstConnecte = true,
      Type = "Prise de courant",
      Image = "prise.png"
    };

    Prise priseBureau = new() {
      Nom = "Prise bureau",
      Type = "Prise de courant",
      Image = "prise.png",
      EstCommandable = true,
      EstConnecte = true,
    };

    Prise priseSalon = new() {
      Nom = "Prise salon",
      Type = "Prise de courant",
      Image = "prise.png",
      EstCommandable = true,
      EstConnecte = true,
    };

    Prise priseChambre = new() {
      Nom = "Prise chambre",
      Type = "Prise de courant",
      EstCommandable = true,
      Image = "prise.png",
      EstConnecte = true,
      EstAllume = true,
    };

    VoletRoulant voletRoulantSalon = new() {
      Nom = "Volet roulant salon",
      Ouverture = 50,
      Image = "volet.png"
    };

    VoletRoulant voletRoulantCuisine = new() {
      Nom = "Volet roulant cuisine",
      Ouverture = 50,
      Image = "volet.png"
    };

    VoletRoulant voletRoulantBureau = new() {
      Nom = "Volet roulant bureau",
      Ouverture = 50,
      Image = "volet.png"
    };

    VoletRoulant voletRoulantChambre = new() {
      Nom = "Volet roulant chambre",
      Ouverture = 50,
      Image = "volet.png"
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
    salon.Equipements.Add(voletRoulantSalon);
    chambre.Equipements.Add(voletRoulantChambre);
    bureau.Equipements.Add(voletRoulantBureau);
    cuisine.Equipements.Add(voletRoulantCuisine);
    maison.Pieces.Add(bureau);
    maison.Pieces.Add(salon);
    maison.Pieces.Add(chambre);
    maison.Pieces.Add(cuisine);

    foreach (Piece piece in maison.Pieces) {
      string key1 = $"{maison.Id}/{piece.Nom}";
      piece.Id = CreerGuidStable(key1);
      foreach (Equipement equipement in piece.Equipements) {
        string key2 = $"{piece.Id}/{equipement.Nom}";
        equipement.Id = CreerGuidStable(key2);
      }
    }

    return maison;
  }

  private static Guid CreerGuidStable(string cle) {
    byte[] donnees = Encoding.UTF8.GetBytes(cle);
    byte[] hash = SHA256.HashData(donnees);
    return new Guid(hash.AsSpan(0,16));
  }
}