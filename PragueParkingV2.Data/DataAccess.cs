using PragueParkingV2.Core;
using System.IO;		// Behövs för att hantera filer
using System.Text.Json; // Behövs för serialisering
using System.Text.Json.Serialization;

namespace PragueParkingV2.Data
{
	public class DataAccess
	{
		// Definierar filnamnet på ett ställe för enkelhetens skull
		private const string GarageDataFile = "garage.json";
		private const string ConfigFile = "config.json";

		public void SaveGarage(ParkingGarage garage)
		{
			try
			{
				// Steg 1: Serialisera (omvandla) hela garage-objektet till en JSON-formaterad textsträng.
				// Lägger till ett alternativ för att göra JSON-filen mer lättläst för människor (indenterad).
				var options = new JsonSerializerOptions { WriteIndented = true };
				string jsonString = JsonSerializer.Serialize(garage, options);

				// Steg 2: Skriv textsträngen till filen. Om filen redan finns skrivs den över.
				File.WriteAllText(GarageDataFile, jsonString);
			}
			catch (Exception ex)
			{
				// En enkel felhantering om något går fel vid sparandet.
				Console.WriteLine($"Fel vid sparande av data: {ex.Message}");

			}
		}

		public ParkingGarage LoadGarage(Config config) // <-- Tar nu emot Config!
		{
			try
			{
				if (File.Exists(GarageDataFile))
				{
					string jsonString = File.ReadAllText(GarageDataFile);
					if (!string.IsNullOrWhiteSpace(jsonString))
					{
						ParkingGarage? loadedGarage = JsonSerializer.Deserialize<ParkingGarage>(jsonString);
						if (loadedGarage != null)
						{
							Console.WriteLine($"[Info] Garage-data laddad från {GarageDataFile}.");
							return loadedGarage;
						}
					}
				}
			}
			catch (JsonException jsonEx)
			{
				Console.WriteLine($"[Fel] Fel vid läsning av JSON-data från {GarageDataFile}: {jsonEx.Message}");
				Console.WriteLine("[Info] Skapar ett nytt, tomt garage istället.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Fel] Kunde inte ladda garage-data från {GarageDataFile}: {ex.Message}");
				Console.WriteLine("[Info] Skapar ett nytt, tomt garage istället.");
			}

			// Fallback: Skapa ett nytt, tomt garage med storlek från konfigurationen.
			Console.WriteLine($"[Info] Skapar nytt garage med {config.GarageSize} platser enligt konfiguration.");
			ParkingGarage newGarage = new ParkingGarage();
			for (int i = 0; i < config.GarageSize; i++) // Använder config.GarageSize!
			{
				newGarage.Spots.Add(new ParkingSpot { SpotNumber = i + 1 });
			}
			return newGarage;
		}

		public Config LoadConfig()
		{
			Config loadedConfig = null;
			try
			{
				if (File.Exists(ConfigFile))
				{
					string jsonString = File.ReadAllText(ConfigFile);
					if (!string.IsNullOrWhiteSpace(jsonString))
					{
						loadedConfig = JsonSerializer.Deserialize<Config>(jsonString);
					}
				}
				else
				{
					Console.WriteLine($"[Varning] Konfigurationsfilen {ConfigFile} hittades inte.");
				}
			}
			catch (JsonException jsonEx) { Console.WriteLine($"[Fel] JSON-fel i {ConfigFile}: {jsonEx.Message}"); }
			catch (Exception ex) { Console.WriteLine($"[Fel] Kunde inte ladda {ConfigFile}: {ex.Message}"); }

			if (loadedConfig == null)
			{
				Console.WriteLine("[Info] Använder eller skapar standardkonfiguration.");
				loadedConfig = new Config(); // Får standard GarageSize=100
			}

			if (loadedConfig.AllowedVehicleTypes == null || loadedConfig.AllowedVehicleTypes.Count == 0)
			{
				Console.WriteLine("[Info] Lägger till standardfordonstyper (CAR, MC) i konfigurationen.");
				loadedConfig.AllowedVehicleTypes = new List<VehicleTypeConfig>
				{
					new VehicleTypeConfig { TypeName = "CAR", MaxPerSpot = 1 },
					new VehicleTypeConfig { TypeName = "MC", MaxPerSpot = 2 }
				};
			}

			Console.WriteLine($"[Info] Konfiguration aktiv: {loadedConfig.GarageSize} platser.");
			return loadedConfig;
		}
	}
}