using HomeSphere.Api.Services;
using HomeSphere.Core.Models;
using HomeSphere.Core.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSingleton<HomeSphere.Core.HomeSphere>();
builder.Services.AddSingleton<EtatEquipementsService>();
builder.Services.AddSingleton<CommandeEquipementsService>();
builder.Services.AddSingleton<SimulationTemperaturesService>();
builder.Services.AddSingleton<EnergieService>();
builder.Services.AddSingleton<HistoriqueEnergieService>();

builder.Services.AddHostedService<SimulationTemperaturesBackgroundService>();
builder.Services.AddHostedService<EnergieBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.MapOpenApi();
}

app.MapGet("/api/status",() => new {
  Application = "HomeSphere.Api",
  Version = "1.0",
  Etat = "En fonctionnement",
  Horodatage = DateTimeOffset.Now
});

app.MapGet("/api/maisons",(HomeSphere.Core.HomeSphere homeSphere) => {
  return homeSphere.maisons.Select(maison => new {
    maison.Id,
    maison.Nom
  });
});

app.MapGet("/api/maisons/{id:guid}/pieces",(Guid id,HomeSphere.Core.HomeSphere homeSphere) => {
  var maison = homeSphere.maisons.FirstOrDefault(m => m.Id == id);

  if (maison is null)
    return Results.NotFound();

  return Results.Ok(maison.Pieces.Select(piece => new {
    piece.Id,
    piece.Nom
  }));
});

app.MapGet("/api/pieces/{id:guid}/equipements",(Guid id,HomeSphere.Core.HomeSphere homeSphere) => {
  var piece = homeSphere.maisons
      .SelectMany(m => m.Pieces)
      .FirstOrDefault(p => p.Id == id);

  if (piece is null)
    return Results.NotFound();

  return Results.Ok(piece.Equipements.Select(equipement => new {
    equipement.Id,
    equipement.Nom,
    equipement.Type,
    equipement.EstAllume,
    equipement.EstConnecte,
    Temperature = equipement is Thermometre thermometre
    ? thermometre.Temperature
    : (double?)null

  }));
});

app.MapPost("/api/equipements/{id:guid}/basculer",
    (Guid id,CommandeEquipementsService service,EtatEquipementsService etatService) => {

      bool resultat = service.BasculerEquipement(id);

      if (!resultat)
        return Results.BadRequest("Impossible de commander cet équipement.");

      etatService.SauvegarderEtatEquipements();

      return Results.Ok("Commande exécutée avec succès.");
    });

app.MapGet("/api/energie/puissance",(EnergieService energieService) => {
  return Results.Ok(new {
    PuissanceTotale_W = energieService.CalculerPuissanceTotale_W()
  });
});

app.MapGet("/api/energie/consommation",(EnergieService energieService) => {
  return Results.Ok(new {
    EnergieTotale_kWh = energieService.EnergieTotaleKWh
  });
});

app.MapGet("/api/energie/historique",
    (DateTimeOffset? debut,DateTimeOffset? fin,HistoriqueEnergieService historiqueService) => {

      IEnumerable<ReleveEnergie> releves = historiqueService.Releves;

      if (debut.HasValue)
        releves = releves.Where(r => r.Horodatage >= debut.Value);

      if (fin.HasValue)
        releves = releves.Where(r => r.Horodatage < fin.Value);

      return Results.Ok(releves);
    });

app.MapGet("/api/energie/consommations/journalieres",(HistoriqueEnergieService historiqueService) => {
  return Results.Ok(historiqueService.CalculerConsommationsJournalieres());
});

app.MapGet("/api/energie/consommations/hebdomadaires",(HistoriqueEnergieService historiqueService) => {
  return Results.Ok(historiqueService.CalculerConsommationsHebdomadaires());
});

app.MapGet("/api/energie/consommations/mensuelles",(HistoriqueEnergieService historiqueService) => {
  return Results.Ok(historiqueService.CalculerConsommationsMensuelles());
});

app.MapGet("/api/energie/consommations/intervalles",(HistoriqueEnergieService historiqueService) => {
  return Results.Ok(historiqueService.CalculerConsommationsParIntervalle());
});

app.UseHttpsRedirection();

EtatEquipementsService etatEquipementsService = app.Services.GetRequiredService<EtatEquipementsService>();
etatEquipementsService.ChargerEtatEquipements();

app.Run();
