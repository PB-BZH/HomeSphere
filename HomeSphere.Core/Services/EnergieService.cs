using System.Text.Json;
using HomeSphere.Core.Models;

namespace HomeSphere.Core.Services;

public class EnergieService {
  private readonly HomeSphere _homeSphere;
  private DateTime _dernierCalcul;
  private DateTime _dernierCalculParPiece;
  public DateTime _debutMesureGlobale;
  public DateTime _debutMesureParPiece;

  private readonly DateTime _debutSession;
  private double _dureeObservationGlobaleHeures = 0.0;
  private double _dureeObservationParPieceHeures = 0.0;

  public double EnergieTotaleKWh => _energieTotale_kWh;
  public Dictionary<Guid,double> EnergieParPiece_kWh => _energieParPiece_kWh;
  private double _energieTotale_kWh = 0.0;
  private double _dernierePuissance_W;
  private readonly Dictionary<Guid,double> _puissancesParPiece_W = new();
  private readonly Dictionary<Guid,double> _energieParPiece_kWh = new();
  private Dictionary<Guid,double> _dernierePuissanceParPicece_W = new();

  public double DureeObservationGlobaleHeures => _dureeObservationGlobaleHeures + (DateTime.Now - _debutSession).TotalHours;

  public double DureeObservationParPieceHeures => _dureeObservationParPieceHeures + (DateTime.Now - _debutSession).TotalHours;

  private readonly EtatEquipementsService _etatEquipementsService;
  private readonly Func<DateTime> _horloge;
  private readonly string _dossierSauvegarde;

  private string CheminFichierEnergie => Path.Combine(_dossierSauvegarde,"energie.json");

  public EnergieService(HomeSphere homeSphere,Func<DateTime>? horloge = null,string? dossierSauvegarde = null) {
    _homeSphere = homeSphere;
    _horloge = horloge ?? (() => DateTime.Now);

    _dossierSauvegarde = dossierSauvegarde ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"HomeSphere");

    Directory.CreateDirectory(_dossierSauvegarde);

    _dernierCalcul = _horloge();
    _dernierCalculParPiece = _dernierCalcul;
    _debutSession = _dernierCalcul;

    _etatEquipementsService = new(homeSphere,_dossierSauvegarde);

    _etatEquipementsService.ChargerEtatEquipements();
    _dernierePuissanceParPicece_W = new Dictionary<Guid,double>(CalculerPuissanceParPiece_W());
    _dernierePuissance_W = CalculerPuissanceTotale_W();
    IntitialiseEnergieParPiece();
    if (File.Exists(CheminFichierEnergie))
      ChargerEnergies();
    else {
      _debutMesureGlobale = _debutMesureParPiece = DateTime.Now;
      SauvegarderEnergies();
    }
  }

  private void IntitialiseEnergieParPiece() {
    foreach (Piece piece in _homeSphere.maisons.SelectMany(m => m.Pieces))
      _energieParPiece_kWh[piece.Id] = 0.0;
  }

  public double CalculerPuissanceTotale_W() {
    double puissanceTotale = 0.0;
    puissanceTotale = _homeSphere.maisons.SelectMany(m => m.Pieces)
                                         .SelectMany(p => p.Equipements)
                                         .Sum(e => e.Puissance);
    return puissanceTotale;
  }

  public static double CalculerEnergie_kWh(double puissance_W,TimeSpan duree) {
    return puissance_W * duree.TotalHours / 1000.0;
  }

  public void ActualiserConsommation_kWh() {
    DateTime now = _horloge();
    _energieTotale_kWh += CalculerEnergie_kWh(_dernierePuissance_W,now - _dernierCalcul);
    _dernierCalcul = now;
    _dernierePuissance_W = CalculerPuissanceTotale_W();
  }

  public Dictionary<Guid,double> CalculerPuissanceParPiece_W() {
    foreach (Piece piece in _homeSphere.maisons.SelectMany(m => m.Pieces))
      _puissancesParPiece_W[piece.Id] = piece.Equipements.Sum(e => e.Puissance);
    return _puissancesParPiece_W;
  }

  public void ActualiserConsommationParPiece_kWh() {
    DateTime now = _horloge();
    foreach (Piece piece in _homeSphere.maisons.SelectMany(m => m.Pieces))
      _energieParPiece_kWh[piece.Id] += CalculerEnergie_kWh(_dernierePuissanceParPicece_W[piece.Id],now - _dernierCalculParPiece);

    _dernierCalculParPiece = now;
    _dernierePuissanceParPicece_W = new Dictionary<Guid,double>(CalculerPuissanceParPiece_W());
  }

  public void SauvegarderEnergies() {
    DonneesEnergie donnees = new() {
      EnergieTotaleKWh = _energieTotale_kWh,
      EnergieParPieceKWh = _energieParPiece_kWh,
      DureeObservationGlobaleHeures = DureeObservationGlobaleHeures,
      DureeObservationParPieceHeures = DureeObservationParPieceHeures
    };

    string json = JsonSerializer.Serialize(donnees,new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(CheminFichierEnergie,json);
  }

  public void ChargerEnergies() {
    if (!File.Exists(CheminFichierEnergie))
      return;
    try {
      string json = File.ReadAllText(CheminFichierEnergie);
      DonneesEnergie? donnees = JsonSerializer.Deserialize<DonneesEnergie>(json);

      if (donnees is null)
        return;

      if (donnees.EnergieTotaleKWh < 0 || donnees.DureeObservationGlobaleHeures < 0 || donnees.DureeObservationParPieceHeures < 0)
        throw new InvalidDataException("Le fichier energie.json contient des valeurs négatives incohérentes.");

      _energieTotale_kWh = donnees.EnergieTotaleKWh;

      _energieParPiece_kWh.Clear();
      foreach (var energie in donnees.EnergieParPieceKWh)
        _energieParPiece_kWh[energie.Key] = energie.Value;

      _dureeObservationGlobaleHeures = donnees.DureeObservationGlobaleHeures;
      _dureeObservationParPieceHeures = donnees.DureeObservationParPieceHeures;
    }
    catch (JsonException ex) {
      throw new InvalidDataException(
          $"Erreur dans energie.json\n" +
          $"Ligne : {ex.LineNumber}\n" +
          $"Position : {ex.BytePositionInLine}\n" +
          $"Chemin : {ex.Path}\n" +
          $"Détail : {ex.Message}",ex);
    }
  }
}
