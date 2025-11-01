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
		private const string PriceListFile = "pricelist.txt";

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

		public ParkingGarage LoadGarage(Config config)
		{
			try
			{
				// 1. KONTROLLERA OM FILEN EXISTERAR
				if (File.Exists(GarageDataFile))
				{
					// 1A. FILEN FINNS. FÖRSÖK LÄSA DEN.
					string jsonString = File.ReadAllText(GarageDataFile);
					if (!string.IsNullOrWhiteSpace(jsonString))
					{
						ParkingGarage? loadedGarage = JsonSerializer.Deserialize<ParkingGarage>(jsonString);
						if (loadedGarage != null)
						{
							Console.WriteLine($"[Info] Garage-data laddad från {GarageDataFile}.");
							return loadedGarage; // LYCKADES! Returnera det sparade garaget.
						}
					}

					// Filen fanns, men var tom. Skapa ett tomt garage.
					Console.WriteLine($"[Varning] {GarageDataFile} var tom. Skapar nytt, tomt garage.");
					return CreateEmptyGarage(config);
				}
				else
				{
					// 1B. FILEN FINNS INTE. SKAPA TESTDATA (VG-krav)
					ParkingGarage newGarage = CreateTestDataGarage(config);

					// Spara garaget med testdata direkt så filen skapas
					Console.WriteLine($"[Info] Sparar nytt garage med testdata till {GarageDataFile}...");
					SaveGarage(newGarage);

					return newGarage; // Returnera det nyskapade garaget med testdata.
				}
			}
			catch (JsonException jsonEx)
			{
				// 2. FILEN FINNS, MEN ÄR KORRUPT (JsonSerializer misslyckades)
				Console.WriteLine($"[FEL] Fel vid läsning av JSON-data från {GarageDataFile}: {jsonEx.Message}");
				Console.WriteLine("[FEL] Skapar ett nytt, TOMT garage för att förhindra dataförlust.");
				Console.WriteLine($"[Varning] Din gamla garage-fil finns kvar men döps om till '{GarageDataFile}.corrupt'");

				// Döper om den korrupta filen så att den inte skrivs över
				try
				{
					File.Move(GarageDataFile, $"{GarageDataFile}.corrupt", true);
				}
				catch (Exception moveEx)
				{
					Console.WriteLine($"[FEL] Kunde inte döpa om korrupt fil: {moveEx.Message}");
				}

				return CreateEmptyGarage(config);
			}
			catch (Exception ex)
			{
				// 3. ANNAT ALLVARLIGT FEL
				Console.WriteLine($"[FEL] Kunde inte ladda garage-data från {GarageDataFile}: {ex.Message}");
				Console.WriteLine("[FEL] Skapar ett nytt, tomt garage.");
				return CreateEmptyGarage(config);
			}
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
				// MaxPerSpot är borttaget, logiken styrs nu av IVehicle.Size
					new VehicleTypeConfig { TypeName = "CAR" },
					new VehicleTypeConfig { TypeName = "MC" },
					new VehicleTypeConfig { TypeName = "BIKE" },
					new VehicleTypeConfig { TypeName = "BUS" }
				};
			}

			Console.WriteLine($"[Info] Konfiguration aktiv: {loadedConfig.GarageSize} platser.");
			return loadedConfig;
		}

		public Dictionary<string, decimal> LoadPriceList()
		{
			var priceList = new Dictionary<string, decimal>();
			// Standardpriser om filen inte finns eller är felaktig
			priceList["CAR"] = 20;
			priceList["MC"] = 10;
			priceList["BUS"] = 80;
			priceList["BIKE"] = 5;

			try
			{
				if (File.Exists(PriceListFile))
				{
					var lines = File.ReadAllLines(PriceListFile);
					var loadedPrices = new Dictionary<string, decimal>(); // Temporär för att bara använda filens värden om den är OK

					foreach (var line in lines)
					{
						if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
						{
							continue;
						}

						// Dela upp raden vid ':'
						var parts = line.Split(':');
						if (parts.Length == 2)
						{
							string vehicleType = parts[0].Trim().ToUpper();
			
							if (decimal.TryParse(parts[1].Trim(), out decimal pricePerHour)) // Försök omvandla priset till decimal
							{
								loadedPrices[vehicleType] = pricePerHour; // Lägg till i den temporära listan
							}
							else
							{
								Console.WriteLine($"[Varning] Kunde inte tolka priset på raden i {PriceListFile}: {line}");
							}
						}
						else
						{
							Console.WriteLine($"[Varning] Ignorerar ogiltig rad i {PriceListFile}: {line}");
						}
					}
					// Om det lyckades ladda några priser från filen, använd dem istället för standard
					if (loadedPrices.Count > 0)
					{
						Console.WriteLine($"[Info] Prislista laddad från {PriceListFile}.");
						return loadedPrices;
					}
					else
					{
						Console.WriteLine($"[Varning] Inga giltiga priser hittades i {PriceListFile}. Använder standardpriser.");
					}
				}
				else
				{
					Console.WriteLine($"[Varning] Prislistan {PriceListFile} hittades inte. Använder standardpriser.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Fel] Kunde inte ladda prislistan från {PriceListFile}: {ex.Message}");
				Console.WriteLine("[Info] Använder standardpriser.");
			}

			return priceList;
		}

		// Skapar ett nytt garage-objekt och fyller det med testdata
		private ParkingGarage CreateTestDataGarage(Config config)
		{
			Console.WriteLine("[Info] Skapar nytt garage och fyller med testdata...");
			ParkingGarage newGarage = new ParkingGarage();

			// Fyll med tomma platser
			for (int i = 0; i < config.GarageSize; i++)
			{
				newGarage.Spots.Add(new ParkingSpot { SpotNumber = i + 1 });
			}

			// Använder try/catch om config.GarageSize är för litet
			try
			{
				// Parkera en BIL på plats 3
				newGarage.Spots[2].ParkedVehicles.Add(new Car { RegNum = "CAR-01", ArrivalTime = DateTime.Now.AddHours(-1) });

				// Parkera två MC på plats 5
				newGarage.Spots[4].ParkedVehicles.Add(new MC { RegNum = "MC-01A", ArrivalTime = DateTime.Now.AddHours(-2) });
				newGarage.Spots[4].ParkedVehicles.Add(new MC { RegNum = "MC-01B", ArrivalTime = DateTime.Now.AddHours(-3) });

				// Parkera en BUSS på plats 10-13 (4 platser, 16 size / 4 capacity)
				// Vi skapar buss-objektet FÖRST
				Bus testBus = new Bus { RegNum = "BUS-01", ArrivalTime = DateTime.Now.AddMinutes(-30) };
				// ...och lägger till SAMMA objekt på alla 4 platser
				newGarage.Spots[9].ParkedVehicles.Add(testBus); // Plats 10
				newGarage.Spots[10].ParkedVehicles.Add(testBus); // Plats 11
				newGarage.Spots[11].ParkedVehicles.Add(testBus); // Plats 12
				newGarage.Spots[12].ParkedVehicles.Add(testBus); // Plats 13

				// Parkera två CYKLAR på plats 20
				newGarage.Spots[19].ParkedVehicles.Add(new Bike { RegNum = "BIKE-1", ArrivalTime = DateTime.Now.AddMinutes(-15) });
				newGarage.Spots[19].ParkedVehicles.Add(new Bike { RegNum = "BIKE-2", ArrivalTime = DateTime.Now.AddMinutes(-16) });
			}
			catch (ArgumentOutOfRangeException)
			{
				// Detta händer om config.GarageSize < 20
				Console.WriteLine("[Varning] Garaget är för litet för att lägga till all testdata. Viss data skippades.");
			}

			return newGarage;
		}

		// === SPARA CONFIG.JSON ===
		public void SaveConfig(Config config)
		{
			try
			{
				var options = new JsonSerializerOptions { WriteIndented = true };
				string jsonString = JsonSerializer.Serialize(config, options);
				File.WriteAllText(ConfigFile, jsonString);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fel vid sparande av {ConfigFile}: {ex.Message}");
			}
		}

		// === SPARA PRICELIST.TXT ===
		public void SavePriceList(Dictionary<string, decimal> priceList)
		{
			try
			{
				// Skapa en lista av strängar, t.ex. "CAR: 20"
				List<string> lines = new List<string> { "# Prislista i CZK per timme" };
				foreach (var entry in priceList)
				{
					lines.Add($"{entry.Key.ToUpper()}: {entry.Value}");
				}

				// Skriv alla rader till filen
				File.WriteAllLines(PriceListFile, lines);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fel vid sparande av {PriceListFile}: {ex.Message}");
			}
		}

		// NY HJÄLPMETOD: Skapar bara ett tomt garage
		private ParkingGarage CreateEmptyGarage(Config config)
		{
			Console.WriteLine($"[Info] Skapar nytt tomt garage med {config.GarageSize} platser.");
			ParkingGarage newGarage = new ParkingGarage();
			for (int i = 0; i < config.GarageSize; i++)
			{
				newGarage.Spots.Add(new ParkingSpot { SpotNumber = i + 1 });
			}
			return newGarage;
		}
	}

}

