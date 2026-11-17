
using System.Text.Json;
using HomeSphere.Core.Models;

namespace HomeSphere.Core.Services;

public class EtatEquipementsService {
  private readonly HomeSphere _homeSphere;
  private readonly string _dossierSauvegarde;
  private string CheminFichierEtatEquipements => Path.Combine(_dossierSauvegarde,"EtatEquipements.json");

  public EtatEquipementsService(HomeSphere homeSphere,string? dossierSauvegarde = null) {
    _homeSphere = homeSphere;
    _dossierSauvegarde = dossierSauvegarde ??
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"HomeSphere");

    Directory.CreateDirectory(_dossierSauvegarde);
  }

  public void SauvegarderEtatEquipements() {
    Dictionary<Guid,List<EtatEquipement>> etatsParPiece = new();

    foreach (Piece piece in _homeSphere.maisons.SelectMany(m => m.Pieces)) {
      List<EtatEquipement> etatEquipements = new();

      foreach (Equipement equipement in piece.Equipements) {
        EtatEquipement etat = new() {
          EquipementId = equipement.Id,
          EstAllume = equipement.EstAllume,
          EstConnecte = equipement.EstConnecte
        };

        if (equipement is VoletRoulant volet)
          etat.Ouverture = volet.Ouverture;

        if (equipement is Thermometre thermometre) {
          etat.Temperature = thermometre.Temperature;
          etat.TemperatureReference = thermometre.TemperatureReference;
        }

        etatEquipements.Add(etat);
      }

      etatsParPiece[piece.Id] = etatEquipements;
    }

    string json = JsonSerializer.Serialize(etatsParPiece,new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(CheminFichierEtatEquipements,json);
  }

  public void ChargerEtatEquipements() {
    if (!File.Exists(CheminFichierEtatEquipements))
      return;

    string json = File.ReadAllText(CheminFichierEtatEquipements);
    Dictionary<Guid,List<EtatEquipement>>? etatsParPiece = JsonSerializer.Deserialize<Dictionary<Guid,List<EtatEquipement>>>(json);

    if (etatsParPiece == null)
      return;

    foreach (Piece piece in _homeSphere.maisons.SelectMany(m => m.Pieces)) {
      if (!etatsParPiece.TryGetValue(piece.Id,out List<EtatEquipement>? etatsEquipements))
        continue;
      foreach (Equipement equipement in piece.Equipements) {
        EtatEquipement? etat = etatsEquipements.FirstOrDefault(e => e.EquipementId == equipement.Id);

        if (etat is null)
          continue;

        if (equipement is VoletRoulant volet && etat.Ouverture is int ouverture)
          volet.Ouverture = ouverture;

        if (equipement is Thermometre thermometre) {
          if (etat.Temperature is double temperature)
            thermometre.Temperature = temperature;

          if (etat.TemperatureReference is double reference)
            thermometre.TemperatureReference = reference;
        }

        equipement.EstAllume = etat.EstAllume;
        equipement.EstConnecte = etat.EstConnecte;
      }
    }
  }
}
