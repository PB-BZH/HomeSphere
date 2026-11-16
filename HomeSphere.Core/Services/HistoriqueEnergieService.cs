using System.Globalization;
using System.Text.Json;
using HomeSphere.Core.Enums;
using HomeSphere.Core.Models;

namespace HomeSphere.Core.Services;

public class HistoriqueEnergieService {

  private readonly EnergieService _energieService;
  private readonly Func<DateTimeOffset> _horloge;
  private readonly DonneesHistoriqueEnergie _donneesHistoriqueEnergie = new();
  public IReadOnlyList<ReleveEnergie> Releves => _donneesHistoriqueEnergie.Releves.AsReadOnly();
  private readonly string _dossierSauvegarde;
  private string CheminFichierHistoriqueEnergie => Path.Combine(_dossierSauvegarde,"Historique energie.json");
  public event Action<ReleveEnergie>? NouveauReleve;


  public HistoriqueEnergieService(EnergieService energieService,Func<DateTimeOffset>? horloge = null,string? dossierSauvegarde = null) {
    _energieService = energieService;
    _horloge = horloge ?? (() => DateTimeOffset.Now);
    _dossierSauvegarde = dossierSauvegarde ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"HomeSphere");
    Directory.CreateDirectory(_dossierSauvegarde);
    if (File.Exists(CheminFichierHistoriqueEnergie))
      ChargerHistorique();
  }

  public void SauvegarderHistorique() {
    string json = JsonSerializer.Serialize(_donneesHistoriqueEnergie,
        new JsonSerializerOptions { WriteIndented = true });

    File.WriteAllText(CheminFichierHistoriqueEnergie,json);
  }

  public void ChargerHistorique() {
    if (!File.Exists(CheminFichierHistoriqueEnergie))
      return;

    string json = File.ReadAllText(CheminFichierHistoriqueEnergie);
    DonneesHistoriqueEnergie? donnees = JsonSerializer.Deserialize<DonneesHistoriqueEnergie>(json);

    if (donnees is null)
      throw new InvalidDataException("Le fichier historique énergétique est invalide.");

    if (donnees.Version != 1)
      throw new InvalidDataException($"Version d'historique non prise en charge : {donnees.Version}");

    if (donnees.Releves is null)
      throw new InvalidDataException("La liste des relevés est absente.");

    _donneesHistoriqueEnergie.Releves.Clear();
    _donneesHistoriqueEnergie.Releves.AddRange(donnees.Releves.OrderBy(r => r.Horodatage));
  }

  public ReleveEnergie CreerReleve(TypeReleveEnergie typeReleve = TypeReleveEnergie.Normal) {
    _energieService.ActualiserConsommation_kWh();
    _energieService.ActualiserConsommationParPiece_kWh();

    ReleveEnergie releve = new() {
      Horodatage = _horloge(),
      TypeReleve = typeReleve,
      PuissanceTotale_W = _energieService.CalculerPuissanceTotale_W(),
      EnergieTotaleConsommee_kWh = _energieService.EnergieTotaleKWh,
      PuissanceParPiece_W = new(_energieService.CalculerPuissanceParPiece_W()),
      EnergieConsommeeParPiece_kWh = new(_energieService.EnergieParPiece_kWh),
    };

    _donneesHistoriqueEnergie.Releves.Add(releve);
    NouveauReleve?.Invoke(releve);

    return releve;
  }

  public static double CalculerConsommationEntreReleves_kWh(ReleveEnergie debut,ReleveEnergie fin) {
    ArgumentNullException.ThrowIfNull(debut);
    ArgumentNullException.ThrowIfNull(fin);

    if (fin.Horodatage < debut.Horodatage)
      throw new ArgumentException("Le relevé final précède le relevé initial.");

    double consommation = fin.EnergieTotaleConsommee_kWh - debut.EnergieTotaleConsommee_kWh;

    if (consommation < 0)
      throw new InvalidDataException("Le cumul énergétique final est inférieur au cumul initial.");

    return consommation;
  }

  public double CalculerConsommationPeriode_kWh(DateTimeOffset debut,DateTimeOffset fin) {
    if (fin < debut)
      throw new ArgumentException("La fin de période précède le début.");

    ReleveEnergie? releveDebut = _donneesHistoriqueEnergie.Releves
        .FirstOrDefault(r => r.Horodatage == debut);

    ReleveEnergie? releveFin = _donneesHistoriqueEnergie.Releves
        .FirstOrDefault(r => r.Horodatage == fin);

    if (releveDebut is null || releveFin is null)
      throw new InvalidOperationException("Un relevé est absent pour la période demandée.");

    return CalculerConsommationEntreReleves_kWh(releveDebut,releveFin);
  }

  public double CalculerConsommationPiecePeriode_kWh(Piece piece,DateTimeOffset debut,DateTimeOffset fin) {
    ArgumentNullException.ThrowIfNull(piece);

    if (fin < debut)
      throw new ArgumentException("La fin de période précède le début.");

    Guid idPiece = piece.Id;

    ReleveEnergie? releveDebut = _donneesHistoriqueEnergie.Releves
        .FirstOrDefault(r => r.Horodatage == debut);

    ReleveEnergie? releveFin = _donneesHistoriqueEnergie.Releves
        .FirstOrDefault(r => r.Horodatage == fin);

    if (releveDebut is null || releveFin is null)
      throw new InvalidOperationException("Un relevé est absent pour la période demandée.");

    if (!releveDebut.EnergieConsommeeParPiece_kWh.TryGetValue(idPiece,out double energieDebut))
      throw new InvalidDataException("Pièce absente du relevé initial.");

    if (!releveFin.EnergieConsommeeParPiece_kWh.TryGetValue(idPiece,out double energieFin))
      throw new InvalidDataException("Pièce absente du relevé final.");

    double consommation = energieFin - energieDebut;

    if (consommation < 0)
      throw new InvalidDataException("Le cumul énergétique final est inférieur au cumul initial.");

    return consommation;
  }

  public double CalculerConsommationPlageHoraire_kWh(DateTimeOffset debut,DateTimeOffset fin) {
    if (fin < debut)
      throw new ArgumentException("La fin de période précède le début.");

    ReleveEnergie? releveDebut = _donneesHistoriqueEnergie.Releves
    .FirstOrDefault(r => r.Horodatage >= debut);

    ReleveEnergie? releveFin = _donneesHistoriqueEnergie.Releves
    .LastOrDefault(r => r.Horodatage <= fin);

    if (releveDebut is null || releveFin is null)
      throw new InvalidOperationException("Un relevé est absent pour la période demandée.");

    if (releveFin.Horodatage < releveDebut.Horodatage)
      throw new InvalidOperationException("Aucun intervalle de relevés disponible dans la période demandée.");

    return CalculerConsommationEntreReleves_kWh(releveDebut,releveFin);
  }

  public double CalculerConsommationPiecePlageHoraire_kWh(Piece piece,DateTimeOffset debut,DateTimeOffset fin) {
    ArgumentNullException.ThrowIfNull(piece);

    if (fin < debut)
      throw new ArgumentException("La fin de période précède le début.");

    ReleveEnergie? releveDebut = _donneesHistoriqueEnergie.Releves
    .FirstOrDefault(r => r.Horodatage >= debut);

    ReleveEnergie? releveFin = _donneesHistoriqueEnergie.Releves
    .LastOrDefault(r => r.Horodatage <= fin);

    if (releveDebut is null || releveFin is null)
      throw new InvalidOperationException("Un relevé est absent pour la période demandée.");

    if (releveFin.Horodatage < releveDebut.Horodatage)
      throw new InvalidOperationException("Aucun intervalle de relevés disponible dans la période demandée.");

    return CalculerConsommationPiecePeriode_kWh(piece,releveDebut.Horodatage,releveFin.Horodatage);
  }

  public List<ConsommationHoraire> CalculerConsommationsHoraires(DateTimeOffset debut,DateTimeOffset fin) {
    List<ConsommationHoraire> listeConsommationHoraire = [];

    if (fin < debut)
      throw new ArgumentException("La fin de période précède le début.");

    for (DateTimeOffset plage = debut;plage < fin;plage = plage.AddHours(1)) {
      DateTimeOffset plageDebutHeure = new(plage.Year,plage.Month,plage.Day,plage.Hour,0,0,plage.Offset);
      DateTimeOffset plageFinHeure = plageDebutHeure.AddHours(1);
      ConsommationHoraire consommationHorairePlage = new();
      DateTimeOffset debutCalcul = plageDebutHeure > debut ? plageDebutHeure : debut;
      DateTimeOffset finCalcul = plageFinHeure < fin ? plageFinHeure : fin;
      consommationHorairePlage.DebutHeure = plageDebutHeure;
      consommationHorairePlage.EnergieConsommee_kWh = CalculerConsommationPlageHoraire_kWh(debutCalcul,finCalcul);
      listeConsommationHoraire.Add(consommationHorairePlage);
    }
    return listeConsommationHoraire;
  }

  public List<ConsommationIntervalle> CalculerConsommationsParIntervalle() {
    List<ConsommationIntervalle> listeConsommationIntervalle = [];

    for (int i = 1;i < _donneesHistoriqueEnergie.Releves.Count;i++) {
      ReleveEnergie relevePrecedent = _donneesHistoriqueEnergie.Releves[i - 1];
      ReleveEnergie releveActuel = _donneesHistoriqueEnergie.Releves[i];

      if (releveActuel.TypeReleve is TypeReleveEnergie.PremierApresDemarrage or TypeReleveEnergie.PremierApresReprise)
        continue;

      double energie = CalculerConsommationEntreReleves_kWh(relevePrecedent,releveActuel);

      ConsommationIntervalle intervalle = new() {
        Debut = relevePrecedent.Horodatage,
        Fin = releveActuel.Horodatage,
        EnergieConsommee_kWh = energie
      };

      listeConsommationIntervalle.Add(intervalle);
    }

    return listeConsommationIntervalle;
  }

  public List<ConsommationJournaliere> CalculerConsommationsJournalieres() {
    List<ConsommationJournaliere> consommationJournalieres = [];
    Dictionary<DateOnly,double> consommationsParJour = [];

    List<ConsommationIntervalle> listeConsommationIntervalle = CalculerConsommationsParIntervalle();
    for (int i = 0;i < listeConsommationIntervalle.Count;i++) {
      double energieAvantMinuit;
      double energieApresMinuit;
      DateTimeOffset debut = listeConsommationIntervalle[i].Debut;
      DateTimeOffset fin = listeConsommationIntervalle[i].Fin;
      DateOnly jour = DateOnly.FromDateTime(debut.DateTime);
      DateOnly jourFin = DateOnly.FromDateTime(fin.DateTime);
      DateTimeOffset minuit = new(debut.Date.AddDays(1),debut.Offset);
      if (jourFin != jour) {
        TimeSpan dureeTotale = fin - debut;
        TimeSpan dureeAvantMinuit = minuit - debut;
        TimeSpan dureeApresMinuit = fin - minuit;
        double delta = dureeAvantMinuit / dureeTotale;
        energieAvantMinuit = delta * listeConsommationIntervalle[i].EnergieConsommee_kWh;
        energieApresMinuit = (1 - delta) * listeConsommationIntervalle[i].EnergieConsommee_kWh;
        consommationsParJour[jour] = consommationsParJour.GetValueOrDefault(jour) + energieAvantMinuit;
        consommationsParJour[jourFin] = consommationsParJour.GetValueOrDefault(jourFin) + energieApresMinuit;
      }
      else {
        consommationsParJour[jour] = consommationsParJour.GetValueOrDefault(jour) + listeConsommationIntervalle[i].EnergieConsommee_kWh;
      }
    }

    foreach (var element in consommationsParJour) {
      consommationJournalieres.Add(new ConsommationJournaliere {
        Date = element.Key,
        ConsommationJournaliere_kWh = element.Value
      });
    }

    return consommationJournalieres.OrderBy(c => c.Date).ToList();
  }

  public List<ConsommationHebdomadaire> CalculerConsommationsHebdomadaires() {
    List<ConsommationHebdomadaire> consommationHebdomadaires = [];
    Dictionary<(int Annee,int Semaine),double> consommationsParSemaine = [];

    List<ConsommationJournaliere> listeConsommationJournaliere = CalculerConsommationsJournalieres();

    for (int i = 0;i < listeConsommationJournaliere.Count;i++) {
      DateOnly date = listeConsommationJournaliere[i].Date;
      int semaine = ISOWeek.GetWeekOfYear(date);
      int anneeSemaine = ISOWeek.GetYear(date);
      var cle = (anneeSemaine,semaine);
      consommationsParSemaine[cle] = consommationsParSemaine.GetValueOrDefault(cle) + listeConsommationJournaliere[i].ConsommationJournaliere_kWh;
    }

    foreach (var element in consommationsParSemaine) {
      consommationHebdomadaires.Add(new ConsommationHebdomadaire {
        Annee = element.Key.Annee,
        Semaine = element.Key.Semaine,
        ConsommationHebdomadaire_kWh = element.Value
      });
    }

    return consommationHebdomadaires
        .OrderBy(c => c.Annee)
        .ThenBy(c => c.Semaine)
        .ToList();
  }

  public List<ConsommationMensuelle> CalculerConsommationsMensuelles() {
    List<ConsommationMensuelle> consommationsMensuelles = [];
    Dictionary<(int Annee,int Mois),double> consommationsParMois = [];

    List<ConsommationJournaliere> listeConsommationJournaliere = CalculerConsommationsJournalieres();

    for (int i = 0;i < listeConsommationJournaliere.Count;i++) {
      DateOnly date = listeConsommationJournaliere[i].Date;
      int annee = date.Year;
      int mois = date.Month;
      var cle = (annee,mois);
      consommationsParMois[cle] = consommationsParMois.GetValueOrDefault(cle) + listeConsommationJournaliere[i].ConsommationJournaliere_kWh;
    }

    foreach (var element in consommationsParMois) {
      consommationsMensuelles.Add(new ConsommationMensuelle {
        Annee = element.Key.Annee,
        Mois = element.Key.Mois,
        ConsommationMensuelle_kWh = element.Value
      });
    }

    return consommationsMensuelles
        .OrderBy(c => c.Annee)
        .ThenBy(c => c.Mois)
        .ToList();
  }
}