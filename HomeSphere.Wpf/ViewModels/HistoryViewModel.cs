using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using HomeSphere.Core.Enums;
using HomeSphere.Core.Models;
using HomeSphere.Core.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HomeSphere.Wpf.ViewModels;

class HistoryViewModel: ObservableObject {
  private readonly Core.HomeSphere _homeSphere;
  private readonly HistoriqueEnergieService _historiqueEnergieService;
  public IReadOnlyList<ReleveEnergie> Releves => _historiqueEnergieService.Releves;
  public List<ConsommationJournaliere> ConsommationsJournalieres => _historiqueEnergieService.CalculerConsommationsJournalieres();
  public List<ConsommationHebdomadaire> ConsommationsHebdomadaires => _historiqueEnergieService.CalculerConsommationsHebdomadaires();
  public List<ConsommationMensuelle> ConsommationsMensuelles => _historiqueEnergieService.CalculerConsommationsMensuelles();
  public IEnumerable<ReleveEnergie> RelevesFiltres =>
  Releves
      .Where(r => r.Horodatage >= new DateTimeOffset(DateDebut)
               && r.Horodatage < new DateTimeOffset(DateFin))
      .OrderBy(r => r.Horodatage);

  public List<ConsommationIntervalle> ConsommationsParIntervalle => _historiqueEnergieService.CalculerConsommationsParIntervalle();

  public IEnumerable<ConsommationIntervalle> ConsommationsParIntervalleFiltrees =>
    ConsommationsParIntervalle
        .Where(c => c.Debut >= new DateTimeOffset(DateDebut)
                 && c.Fin <= new DateTimeOffset(DateFin))
        .OrderBy(c => c.Debut);

  private readonly PointPuissance pointPuissance = new();
  public ObservableCollection<DateTimePoint?> PointsGraphique { get; } = [];
  public ISeries[] Series { get; set; }
  public Axis[] XAxes { get; set; }
  public Axis[] YAxes { get; set; }
  private DateTime _dateDebut = DateTime.Today;

  public DateTime DateDebut {
    get => _dateDebut;
    set {
      if (SetProperty(ref _dateDebut,value)) {
        OnPropertyChanged(nameof(ConsommationPeriode_kWh));
        OnPropertyChanged(nameof(RelevesFiltres));
        OnPropertyChanged(nameof(ConsommationsParIntervalleFiltrees));
        ActualiserPointsGraphique();
      }
    }
  }

  private DateTime _dateFin = DateTime.Today.AddDays(1);
  public DateTime DateFin {
    get => _dateFin;
    set {
      if (SetProperty(ref _dateFin,value)) {
        OnPropertyChanged(nameof(ConsommationPeriode_kWh));
        OnPropertyChanged(nameof(RelevesFiltres));
        OnPropertyChanged(nameof(ConsommationsParIntervalleFiltrees));
        ActualiserPointsGraphique();
      }
    }
  }

  public double? ConsommationPeriode_kWh {
    get {
      if (DateFin <= DateDebut)
        return null;

      try {
        return _historiqueEnergieService.CalculerConsommationPlageHoraire_kWh(
            new DateTimeOffset(DateDebut),
            new DateTimeOffset(DateFin));
      }
      catch (InvalidOperationException) {
        return null;
      }
    }
  }

  public ISeries[] SeriesJournalieres { get; set; } = [
      new ColumnSeries<double> {
        Name = "Consommation journalière"
    }
  ];

  public ISeries[] SeriesHebdomadaires { get; set; } = [
      new ColumnSeries<double> {
        Name = "Consommation par semaine"
    }
  ];

  public ISeries[] SeriesMensuelles { get; set; } = [
      new ColumnSeries<double> {
        Name = "Consommation par mois"
    }
  ];

  public Axis[] XAxesJournalieres { get; set; } = [
      new Axis {
        Labels = [],
        LabelsRotation = 0,
        LabelsPaint = new SolidColorPaint(SKColors.White)
    }
  ];

  public Axis[] YAxesJournalieres { get; set; } = [
      new Axis {
        Name = "Énergie (kWh)",
        LabelsPaint = new SolidColorPaint(SKColors.White),
        NamePaint = new SolidColorPaint(SKColors.White),
        MinLimit = 0
    }
  ];

  public Axis[] XAxesHebdomadaires { get; set; } = [
      new Axis {
        Labels = [],
        LabelsRotation = 0,
        LabelsPaint = new SolidColorPaint(SKColors.White)
    }
  ];

  public Axis[] YAxesHebdomadaires { get; set; } = [
      new Axis {
        Name = "Énergie (kWh)",
        LabelsPaint = new SolidColorPaint(SKColors.White),
        NamePaint = new SolidColorPaint(SKColors.White),
        MinLimit = 0
    }
  ];

  public Axis[] XAxesMensuelles { get; set; } = [
      new Axis {
        Labels = [],
        LabelsRotation = 0,
        LabelsPaint = new SolidColorPaint(SKColors.White)
    }
  ];

  public Axis[] YAxesMensuelles { get; set; } = [
      new Axis {
        Name = "Énergie (kWh)",
        LabelsPaint = new SolidColorPaint(SKColors.White),
        NamePaint = new SolidColorPaint(SKColors.White),
        MinLimit = 0
    }
  ];

  public HistoryViewModel(Core.HomeSphere homeSphere,HistoriqueEnergieService historiqueEnergieService) {
    _homeSphere = homeSphere;
    _historiqueEnergieService = historiqueEnergieService;

    ActualiserPointsGraphique();

    Series = [
        new StepLineSeries<DateTimePoint?> {
        Values = PointsGraphique,
        Fill = null,
        EnableNullSplitting = true
    }
    ];

    XAxes = [
      new DateTimeAxis(TimeSpan.FromMinutes(30),date => date.ToString("HH:mm")){
        LabelsPaint = new SolidColorPaint(SKColors.White)
      }
    ];

    YAxes = [
      new Axis(){
        Name = "Puissance (W)",
        LabelsPaint = new SolidColorPaint(SKColors.White),
        NamePaint = new SolidColorPaint(SKColors.White)
      }
    ];

    _historiqueEnergieService.NouveauReleve += TraiterNouveauReleve;
    ActualiserGraphiqueJournalier();
    ActualiserGraphiqueHebdomadaire();
    ActualiserGraphiqueMensuelle();
  }

  private void ActualiserGraphiqueJournalier() {
    List<ConsommationJournaliere> consommations = ConsommationsJournalieres;

    double[] valeurs = consommations
        .Select(c => c.ConsommationJournaliere_kWh)
        .ToArray();

    ((ColumnSeries<double>)SeriesJournalieres[0]).Values = valeurs;

    XAxesJournalieres[0].Labels = consommations
    .Select(c => c.Date.ToString("dd/MM"))
    .ToArray();
  }

  private void ActualiserGraphiqueHebdomadaire() {
    List<ConsommationHebdomadaire> consommations = ConsommationsHebdomadaires;

    double[] valeurs = consommations
        .Select(c => c.ConsommationHebdomadaire_kWh)
        .ToArray();

    ((ColumnSeries<double>)SeriesHebdomadaires[0]).Values = valeurs;

    XAxesHebdomadaires[0].Labels = consommations
      .Select(c => $"S{c.Semaine:00}/{c.Annee}")
      .ToArray();
  }

  private void ActualiserGraphiqueMensuelle() {
    List<ConsommationMensuelle> consommations = ConsommationsMensuelles;

    double[] valeurs = consommations
        .Select(c => c.ConsommationMensuelle_kWh)
        .ToArray();

    ((ColumnSeries<double>)SeriesMensuelles[0]).Values = valeurs;

    XAxesMensuelles[0].Labels = consommations
      .Select(c => $"{c.Mois:00}/{c.Annee}")
      .ToArray();
  }


  private void TraiterNouveauReleve(ReleveEnergie releve) {
    AjouterPointGraphique(releve);
    OnPropertyChanged(nameof(ConsommationsJournalieres));
    OnPropertyChanged(nameof(RelevesFiltres));
    OnPropertyChanged(nameof(ConsommationsParIntervalleFiltrees));
    ActualiserGraphiqueJournalier();
    ActualiserGraphiqueHebdomadaire();
    ActualiserGraphiqueMensuelle();
  }

  private void ActualiserPointsGraphique() {
    PointsGraphique.Clear();

    foreach (ReleveEnergie releve in RelevesFiltres)
      AjouterPointGraphique(releve);
  }

  private void AjouterPointGraphique(ReleveEnergie releve) {
    if (!Application.Current.Dispatcher.CheckAccess()) {
      Application.Current.Dispatcher.Invoke(() => AjouterPointGraphique(releve));
      return;
    }

    if (releve.Horodatage < new DateTimeOffset(DateDebut) || releve.Horodatage >= new DateTimeOffset(DateFin))
      return;

    if (releve.TypeReleve is TypeReleveEnergie.PremierApresReprise
        or TypeReleveEnergie.PremierApresDemarrage)
      PointsGraphique.Add(null);

    PointsGraphique.Add(new DateTimePoint {
      DateTime = releve.Horodatage.DateTime,
      Value = releve.PuissanceTotale_W
    });
  }

  public double CalculerConsommationPlageHoraire_kWh(DateTimeOffset debut,DateTimeOffset fin) {
    return _historiqueEnergieService.CalculerConsommationPlageHoraire_kWh(debut,fin);
  }

  public List<PointPuissance> CreerPointsPuissance() {
    return Releves
      .OrderBy(r => r.Horodatage)
      .Select(r => new PointPuissance {
        Horodatage = r.Horodatage,
        Puissance_W = r.PuissanceTotale_W
      }).ToList();
  }
}
