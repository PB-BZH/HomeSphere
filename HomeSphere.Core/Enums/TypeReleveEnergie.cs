using System.Text.Json.Serialization;

namespace HomeSphere.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TypeReleveEnergie {
  /*
  Periodique :	Relevé normal
  AvantVeille:	Dernier relevé avant suspension
  ApresVeille:	Premier relevé après reprise 
   */
  Normal = 0,
  DernierAvantSuspension,
  PremierApresReprise,
  DernierAvantArret,
  PremierApresDemarrage
}
