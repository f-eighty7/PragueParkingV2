using PragueParkingV2.Core;
using System.IO;		// Behövs för att hantera filer
using System.Text.Json; // Behövs för serialisering

namespace PragueParkingV2.Data
{
	public class DataAccess
	{
		// Definierar filnamnet på ett ställe för enkelhetens skull
		private const string GarageDataFile = "garage.json";

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

		public ParkingGarage LoadGarage()
		{
			try
			{
				// Steg 1: Kontrollera om datafilen existerar.
				if (File.Exists(GarageDataFile))
				{
					// Steg 2: Läs allt textinnehåll från filen.
					string jsonString = File.ReadAllText(GarageDataFile);

					// Steg 3: Deserialisera (omvandla) textsträngen tillbaka till ett ParkingGarage-objekt.
					// Om JSON-filen är tom eller korrupt kan detta kasta ett undantag (exception).
					if (!string.IsNullOrWhiteSpace(jsonString))
					{
						ParkingGarage? loadedGarage = JsonSerializer.Deserialize<ParkingGarage>(jsonString);
						if (loadedGarage != null)
						{
							return loadedGarage;
						}
					}
				}
			}
			catch (JsonException jsonEx)
			{
				// Fångar specifikt fel som kan uppstå om JSON-datan är felaktig.
				Console.WriteLine($"Fel vid läsning av JSON-data: {jsonEx.Message}");
				Console.WriteLine("Skapar ett nytt, tomt garage istället.");
			}
			catch (Exception ex)
			{
				// Fångar andra oväntade fel vid filläsning.
				Console.WriteLine($"Fel vid laddning av data: {ex.Message}");
				Console.WriteLine("Skapar ett nytt, tomt garage istället.");
			}

			// Steg 4: Om filen inte fanns, var tom, korrupt, eller om något annat fel inträffade,
			// skapa ett helt nytt, tomt garage-objekt och returnera det.
			// Denna kod ser också till att skapa de 100 tomma platserna direkt.
			ParkingGarage newGarage = new ParkingGarage();
			for (int i = 0; i < 100; i++) // Standard 100 platser om ingen fil finns
			{
				newGarage.Spots.Add(new ParkingSpot { SpotNumber = i + 1 });
			}
			return newGarage;
		}
	}
}