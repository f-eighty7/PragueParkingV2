using PragueParkingV2.Core;
using PragueParkingV2.Data;
using Spectre.Console;
using System.Linq;
using System.Collections.Generic;

// ======== SKAPA DATAACCESS OCH LADDA GARAGET ========

// Skapar ett objekt för att hantera filåtkomst.
DataAccess dataAccess = new DataAccess();
// Laddar konfigurationen från config.json (eller använder standard).
Config config = dataAccess.LoadConfig();
// Laddar prislistan från pricelist.txt (eller använder standard).
Dictionary<string, decimal> priceList = dataAccess.LoadPriceList();
// Laddar garagets tillstånd från garage.json (eller skapar ett nytt baserat på config).
ParkingGarage garage = dataAccess.LoadGarage(config);

AnsiConsole.MarkupLine($"[grey]Konfiguration laddad: {config.GarageSize} platser.[/]");

// ======== HUVUDMENY ========
while (true)
{
	Console.Clear();
	PrintGarageStatus(garage);

	AnsiConsole.MarkupLine("[yellow]Välj ett alternativ:[/]");
	var choice = AnsiConsole.Prompt(
		new SelectionPrompt<string>()
			.PageSize(10)
			.AddChoices(new[] {
				"1. Parkera fordon",
				"2. Ta bort fordon",
				"3. Flytta fordon",
				"4. Sök fordon",
				"5. Visa Karta",
				"0. Avsluta"
			}));

	switch (choice)
	{
		case "1. Parkera fordon":
			AddVehicleUI(garage, config);
			if (garage != null) dataAccess.SaveGarage(garage);
			break;

		case "2. Ta bort fordon":
			RemoveVehicleUI(garage, priceList);
			if (garage != null) dataAccess.SaveGarage(garage);
			break;

		case "3. Flytta fordon":
			MoveVehicleUI(garage);
			if (garage != null) dataAccess.SaveGarage(garage);
			break;

		case "4. Sök fordon":
			SearchVehicleUI(garage); 
			break;

		case "5. Visa Karta":
			DisplayGarageMapUI(garage, config);
			break;

		case "0. Avsluta":
			AnsiConsole.MarkupLine("[green]Sparar och avslutar... Hejdå![/]");
			if (garage != null) dataAccess.SaveGarage(garage);
			return;

		default:
			AnsiConsole.MarkupLine("[red]Oväntat val.[/]");
			break;
	}
}

// === UI-METODER ===

// Hanterar användardialogen för att parkera ett fordon.
void AddVehicleUI(ParkingGarage garage, Config config)
{
	AnsiConsole.MarkupLine("[underline blue]Parkera Fordon[/]");
	bool actionTaken = false;

	// Hämtar tillåtna fordonstyper från konfigurationen för menyn.
	var vehicleTypeChoices = config.AllowedVehicleTypes?
								.Select(vt => vt.TypeName)
								.ToList() ?? new List<string>(); 

	vehicleTypeChoices.Add("[grey]Avbryt[/]");

	var selectedTypeName = AnsiConsole.Prompt(
		new SelectionPrompt<string>()
			.Title("Vilken [green]fordonstyp[/]?")
			.PageSize(vehicleTypeChoices.Count)
			.AddChoices(vehicleTypeChoices));

	// Avbryter om användaren väljer det.
	if (selectedTypeName == "[grey]Avbryt[/]") return;

	var regNum = AnsiConsole.Prompt(
		new TextPrompt<string>("Ange [green]registreringsnummer[/] (lämna tomt för att avbryta):")
			.AllowEmpty()
		)?.ToUpper();

	// Avbryter om användaren lämnar fältet tomt.
	if (string.IsNullOrWhiteSpace(regNum)) return;

	AddVehicle(garage, config, selectedTypeName, regNum); 
	actionTaken = true;

	// Pausar endast om en åtgärd (försök till parkering) faktiskt gjordes.
	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

// Hanterar användardialogen för att söka efter ett fordon.
void SearchVehicleUI(ParkingGarage garage)
{
	AnsiConsole.MarkupLine("[underline blue]Sök Fordon[/]");
	bool actionTaken = false;

	var searchType = AnsiConsole.Prompt(
		new SelectionPrompt<string>()
			.Title("Sök efter [green]fordon via[/]?")
			.PageSize(3)
			.AddChoices(new[] { "Registreringsnummer", "Platsnummer", "[grey]Avbryt[/]" }));

	// Avbryter om användaren väljer det.
	if (searchType == "[grey]Avbryt[/]") return;

	// Hanterar sökning via registreringsnummer.
	if (searchType == "Registreringsnummer")
	{
		var regNumToSearch = AnsiConsole.Prompt(
			new TextPrompt<string>("Ange [green]registreringsnummer[/] att söka efter (lämna tomt för att avbryta):")
				.AllowEmpty()
			)?.ToUpper();

		// Avbryter om användaren lämnar fältet tomt.
		if (string.IsNullOrWhiteSpace(regNumToSearch)) return;

		ParkingSpot? foundSpot = FindSpotByRegNum(garage, regNumToSearch);
		if (foundSpot != null)
		{
			AnsiConsole.MarkupLine($"\nFordonet hittades på plats: [yellow]{foundSpot.SpotNumber}[/]");
			AnsiConsole.MarkupLine("Fordon på denna plats:");
			foreach (Vehicle vehicle in foundSpot.ParkedVehicles ?? Enumerable.Empty<Vehicle>())
			{
				string vehicleType = vehicle is Car ? "[blue]BIL[/]" : (vehicle is MC ? "[magenta]MC[/]" : "[grey]Okänd[/]");
				AnsiConsole.MarkupLine($"- Reg-nr: [green]{vehicle.RegNum ?? "N/A"}[/], Ankomst: {vehicle.ArrivalTime}");
			}
		}
		else
		{
			AnsiConsole.MarkupLine($"\n[red]Ett fordon med registreringsnummer '{regNumToSearch}' kunde inte hittas.[/]");
		}
		actionTaken = true;
	}
	// Hanterar sökning via platsnummer.
	else
	{
		var spotNumToShow = AnsiConsole.Prompt(
			new TextPrompt<int?>("Ange [green]platsnummer[/] (1-100) att visa (lämna tomt för att avbryta):")
				.AllowEmpty()
				.HideDefaultValue()
				.ValidationErrorMessage("[red]Ange ett giltigt nummer eller lämna tomt.[/]")
		);

		// Avbryter om inget angavs.
		if (!spotNumToShow.HasValue) return;

		ShowSpotContent(spotNumToShow.Value);
		actionTaken = true;
	}

	// Pausar endast om en sökning eller visning faktiskt gjordes.
	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

// Hanterar användardialogen för att ta bort ett fordon.
void RemoveVehicleUI(ParkingGarage garage, Dictionary<string, decimal> priceList)
{
	AnsiConsole.MarkupLine("[underline blue]Ta bort Fordon[/]");
	bool actionTaken = false;

	var regNumToRemove = AnsiConsole.Prompt(
		new TextPrompt<string>("Ange [green]registreringsnummer[/] på fordonet att ta bort (lämna tomt för att avbryta):")
			.AllowEmpty()
		)?.ToUpper();

	// Avbryter om inget angavs.
	if (string.IsNullOrWhiteSpace(regNumToRemove)) return;

	bool success = RemoveVehicle(garage, regNumToRemove, priceList);
	actionTaken = true;

	// Pausar oavsett om borttagningen lyckades eller ej, så användaren ser meddelandet.
	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

// Hanterar användardialogen för att flytta ett fordon.
void MoveVehicleUI(ParkingGarage garage)
{
	AnsiConsole.MarkupLine("[underline blue]Flytta Fordon[/]");
	bool actionTaken = false;

	var regNumToMove = AnsiConsole.Prompt(
		new TextPrompt<string>("Ange [green]registreringsnummer[/] på fordonet som ska flyttas (lämna tomt för att avbryta):")
			.AllowEmpty()
		)?.ToUpper();

	// Avbryter om inget angavs.
	if (string.IsNullOrWhiteSpace(regNumToMove)) return;

	// Kontrollerar direkt om fordonet finns för snabbare feedback.
	var fromSpot = FindSpotByRegNum(garage, regNumToMove);
	if (fromSpot == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Fordonet med registreringsnummer '{regNumToMove}' kunde inte hittas.[/]");
		actionTaken = true;
	}
	// Om fordonet finns, fråga efter ny plats.
	else
	{
		var toSpotNumber = AnsiConsole.Prompt(
			new TextPrompt<int?>($"Ange [green]ny plats[/] (1-100) för fordonet '{regNumToMove}' (lämna tomt för att avbryta):")
				.AllowEmpty()
				.HideDefaultValue()
				.ValidationErrorMessage("[red]Ange ett giltigt nummer eller lämna tomt.[/]")
		);

		// Avbryter om inget angavs.
		if (!toSpotNumber.HasValue) return;

		bool success = MoveVehicle(garage, regNumToMove, toSpotNumber.Value);
		actionTaken = true;
	}

	// Pausar oavsett om flytten lyckades eller ej, så användaren ser meddelandet.
	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

// Visar en visuell karta över parkeringshuset med färger för status.
void DisplayGarageMapUI(ParkingGarage garage, Config config)
{
	AnsiConsole.MarkupLine("[underline blue]Översiktskarta - Parkeringshuset[/]");
	AnsiConsole.WriteLine();

	int spotsPerRow = 10;

	// Loopar igenom alla platser för att rita kartan.
	foreach (ParkingSpot spot in garage.Spots?.OrderBy(s => s.SpotNumber) ?? Enumerable.Empty<ParkingSpot>())
	{
		string color;
		string content = $"{spot.SpotNumber:D2}";

		int vehicleCount = spot.ParkedVehicles?.Count ?? 0;
		int maxCapacity = 1; // Standardkapacitet

		// Bestämmer platsens maxkapacitet baserat på fordonstyp (om upptagen) eller config (om tom).
		if (vehicleCount > 0 && spot.ParkedVehicles != null && spot.ParkedVehicles.Count > 0)
		{
			string firstVehicleTypeName = spot.ParkedVehicles[0].GetType().Name.ToUpper();
			VehicleTypeConfig? typeConfig = config.AllowedVehicleTypes?.FirstOrDefault(vt => vt.TypeName == firstVehicleTypeName);
			if (typeConfig != null)
			{
				maxCapacity = typeConfig.MaxPerSpot;
			}
		}
		else if (vehicleCount == 0 && config.AllowedVehicleTypes != null && config.AllowedVehicleTypes.Count > 0)
		{
			maxCapacity = config.AllowedVehicleTypes.Max(vt => vt.MaxPerSpot);
		}

		// Sätter färg och innehållsbeskrivning baserat på status.
		if (vehicleCount == 0)
		{
			color = "green";
			content += ": ---";
		}
		else if (vehicleCount < maxCapacity)
		{
			color = "yellow";
			content += $": {spot.ParkedVehicles?[0]?.RegNum ?? "???"}...";
		}
		else // Full
		{
			color = "red";
			content = $"{spot.SpotNumber:D2}:";

			string regNums = "";
			if (spot.ParkedVehicles != null)
			{
				// Bygger en sträng med regnr för alla fordon på platsen.
				foreach (Vehicle vehicle in spot.ParkedVehicles)
				{
					if (!string.IsNullOrEmpty(regNums)) 
					{
						regNums += ",";
					}
					regNums += vehicle.RegNum ?? "???";
				}
			}
			content += regNums;
		}

		// Skriver ut den färgade rutan.
		AnsiConsole.Markup($"[{color}]{content}[/] ");

		// Skapar radbrytning efter angivet antal platser per rad.
		if (spot.SpotNumber % spotsPerRow == 0)
		{
			Console.WriteLine();
			Console.WriteLine();
		}
	}

	AnsiConsole.WriteLine();
	AnsiConsole.MarkupLine("[underline]Förklaring:[/]");
	AnsiConsole.MarkupLine("[green]XX: ---[/] : Ledig Plats");
	AnsiConsole.MarkupLine("[yellow]XX: ABC...[/] : Halvfull Plats (plats för fler av samma typ)");
	AnsiConsole.MarkupLine("[red]XX: DEF...[/] : Full Plats");
	AnsiConsole.WriteLine();

	// Pausar så användaren hinner se kartan.
	AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
	Console.ReadKey(true);
}

// Visar detaljerat innehåll för en specifik parkeringsplats.
void ShowSpotContent(int spotNumber)
{
	if (spotNumber >= 1 && spotNumber <= (garage.Spots?.Count ?? 0))
	{
		int index = spotNumber - 1;
		ParkingSpot? spot = garage.Spots?[index];

		// Visar status och innehåll för platsen.
		if (spot != null && (spot.ParkedVehicles?.Count ?? 0) == 0)
		{
			AnsiConsole.MarkupLine($"\nPlats [yellow]{spotNumber}[/] är [green]tom[/].");
		}
		else if (spot != null)
		{
			AnsiConsole.MarkupLine($"\nPå plats [yellow]{spotNumber}[/] står:");
			foreach (Vehicle vehicle in spot.ParkedVehicles ?? Enumerable.Empty<Vehicle>())
			{
				string vehicleType = vehicle is Car ? "[blue]BIL[/]" : (vehicle is MC ? "[magenta]MC[/]" : "[grey]Okänd[/]");
				AnsiConsole.MarkupLine($"- Reg-nr: [green]{vehicle.RegNum ?? "N/A"}[/], Ankomst: {vehicle.ArrivalTime}");
			}
		}
		else // Bör inte hända
		{
			AnsiConsole.MarkupLine($"[red]Fel: Kunde inte hitta information för plats {spotNumber}.[/]");
		}
	}
	else
	{
		AnsiConsole.MarkupLine($"\n[red]Ogiltigt platsnummer. Ange ett nummer mellan 1 och {garage.Spots?.Count ?? 100}.[/]");
	}
}

// Skriver ut en statuslista över upptagna platser (används i huvudloopen).
void PrintGarageStatus(ParkingGarage garage)
{
	AnsiConsole.MarkupLine("[cyan]--- GARAGE STATUS ---[/]");
	bool isEmpty = true;
	// Loopar igenom alla platser sorterade efter nummer.
	foreach (ParkingSpot spot in garage.Spots?.OrderBy(s => s.SpotNumber) ?? Enumerable.Empty<ParkingSpot>())
	{
		if ((spot.ParkedVehicles?.Count ?? 0) > 0)
		{
			isEmpty = false;
			AnsiConsole.Markup($"Plats [bold]{spot.SpotNumber}[/]: ");
			foreach (Vehicle vehicle in spot.ParkedVehicles)
			{
				string vehicleType = vehicle is Car ? "[blue]BIL[/]" : (vehicle is MC ? "[magenta]MC[/]" : "[grey]Okänd[/]");
				AnsiConsole.Markup($"{vehicleType}:[green]{vehicle.RegNum ?? "N/A"}[/] ");
			}
			Console.WriteLine();
		}
	}
	if (isEmpty)
	{
		AnsiConsole.MarkupLine("[grey](Garaget är tomt)[/]");
	}
	AnsiConsole.MarkupLine("[cyan]---------------------[/]\n");
}

// === LOGIK-METODER ===

// Hanterar logiken för att parkera ett fordon baserat på konfiguration.
void AddVehicle(ParkingGarage garage, Config config, string selectedTypeName, string regNum)
{
	if (string.IsNullOrWhiteSpace(regNum)) { AnsiConsole.MarkupLine("\n[red]Ogiltigt registreringsnummer.[/]"); return; }
	regNum = regNum.ToUpper();

	if (FindSpotByRegNum(garage, regNum) != null) { AnsiConsole.MarkupLine($"\n[red]Ett fordon med reg-nr {regNum} finns redan parkerat.[/]"); return; }

	// Hämtar konfigurationen för den valda fordonstypen.
	VehicleTypeConfig? typeConfig = config.AllowedVehicleTypes?.FirstOrDefault(vt => vt.TypeName == selectedTypeName);

	// Kontrollerar att fordonstypen är känd.
	if (typeConfig == null) { AnsiConsole.MarkupLine($"\n[red]Okänd fordonstyp: {selectedTypeName}.[/]"); return; }

	ParkingSpot? targetSpot = null;

	// Försöker först hitta en plats att dela på om typen tillåter det.
	if (typeConfig.MaxPerSpot > 1)
	{
		// Loopar igenom platserna för att hitta en lämplig delbar plats.
		foreach (ParkingSpot spot in garage.Spots ?? Enumerable.Empty<ParkingSpot>())
		{
			if ((spot.ParkedVehicles?.Count ?? 0) > 0 &&
				(spot.ParkedVehicles?.Count ?? 0) < typeConfig.MaxPerSpot &&
				 spot.ParkedVehicles.All(v => v.GetType().Name.Equals(selectedTypeName, StringComparison.OrdinalIgnoreCase))) // Alla fordon på platsen är av samma typ
			{
				targetSpot = spot;
				break;
			}
		}
	}

	// Om ingen delbar plats hittades, leta efter en helt tom plats.
	if (targetSpot == null)
	{
		// Loopar igenom platserna för att hitta den första helt tomma.
		foreach (ParkingSpot spot in garage.Spots ?? Enumerable.Empty<ParkingSpot>())
		{
			if ((spot.ParkedVehicles?.Count ?? 0) == 0)
			{
				targetSpot = spot;
				break;
			}
		}
	}

	// Om en lämplig plats (antingen delbar eller tom) hittades:
	if (targetSpot != null)
	{
		Vehicle? newVehicle = null;

		// Skapar rätt typ av fordonsobjekt baserat på typnamnet.
		if (selectedTypeName == "CAR")
		{
			newVehicle = new Car { RegNum = regNum, ArrivalTime = DateTime.Now };
		}
		else if (selectedTypeName == "MC")
		{
			newVehicle = new MC { RegNum = regNum, ArrivalTime = DateTime.Now };
		}
		// Hanterar fallet om typnamnet mot förmodan är okänt här.
		else
		{
			AnsiConsole.MarkupLine($"\n[red]Kan inte skapa okänd fordonstyp: {selectedTypeName}.[/]");
			return;
		}

		// Om fordonsobjektet skapades korrekt:
		if (newVehicle != null)
		{
			targetSpot.ParkedVehicles?.Add(newVehicle);

			if ((targetSpot.ParkedVehicles?.Count ?? 0) > 1)
			{
				AnsiConsole.MarkupLine($"\nFordonet [green]{regNum}[/] ({selectedTypeName}) har lagts till på delad plats [yellow]{targetSpot.SpotNumber}[/].");
			}
			else
			{
				AnsiConsole.MarkupLine($"\nFordonet [green]{regNum}[/] ({selectedTypeName}) har parkerats på plats [yellow]{targetSpot.SpotNumber}[/].");
			}
		}
	}
	else
	{
		AnsiConsole.MarkupLine($"\n[red]Tyvärr är parkeringen full för fordonstypen {selectedTypeName}.[/]");
	}
}

// Hittar och returnerar den ParkingSpot där ett fordon med angivet regNr finns.
ParkingSpot? FindSpotByRegNum(ParkingGarage garage, string regNum)
{
	if (string.IsNullOrWhiteSpace(regNum)) return null;

	// Loopar igenom alla platser.
	foreach (ParkingSpot spot in garage.Spots ?? Enumerable.Empty<ParkingSpot>())
	{
		// Loopar igenom alla fordon på den aktuella platsen.
		foreach (Vehicle vehicle in spot.ParkedVehicles ?? Enumerable.Empty<Vehicle>())
		{
			if (vehicle.RegNum != null && vehicle.RegNum.Equals(regNum, StringComparison.OrdinalIgnoreCase))
			{
				return spot;
			}
		}
	}
	return null;
}

// Försöker ta bort ett fordon baserat på regNr och beräknar kostnad.
bool RemoveVehicle(ParkingGarage garage, string regNum, Dictionary<string, decimal> priceList)
{
	if (string.IsNullOrWhiteSpace(regNum))
	{
		AnsiConsole.MarkupLine($"\n[red]Fordonet med registreringsnummer '{regNum}' kunde inte hittas.[/]");
		return false;
	}

	regNum = regNum.ToUpper();

	// Hittar platsen där fordonet står.
	ParkingSpot? spot = FindSpotByRegNum(garage, regNum); 

	if (spot == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Ett fordon med registreringsnummer '{regNum}' kunde inte hittas.[/]");
		return false;
	}

	Vehicle? vehicleToRemove = null;

	// Hittar det exakta fordonsobjektet på platsen.
	foreach (Vehicle vehicle in spot.ParkedVehicles ?? Enumerable.Empty<Vehicle>())
	{
		if (vehicle.RegNum != null && vehicle.RegNum.Equals(regNum, StringComparison.OrdinalIgnoreCase))
		{
			vehicleToRemove = vehicle;
			break;
		}
	}

	// Om fordonsobjektet hittades:
	if (vehicleToRemove != null)
	{
		// Beräknar parkeringstid.
		TimeSpan parkedDuration = DateTime.Now - vehicleToRemove.ArrivalTime;
		decimal cost = 0;

		// Beräknar kostnad om parkeringstiden överstiger gratistiden.
		if (parkedDuration.TotalMinutes > 10)
		{
			// Bestämmer fordonstypens namn för pris-lookup.
			string vehicleTypeName = vehicleToRemove is Car ? "CAR" : (vehicleToRemove is MC ? "MC" : "");

			// Hämtar pris från prislistan.
			if (priceList.TryGetValue(vehicleTypeName, out decimal pricePerHour))
			{
				double totalHours = parkedDuration.TotalHours;
				int billableHours = (int)Math.Ceiling(totalHours); // Avrunda alltid uppåt

				cost = billableHours * pricePerHour;
			}
			// Varnar om pris saknas för typen.
			else if (!string.IsNullOrEmpty(vehicleTypeName))
			{
				AnsiConsole.MarkupLine($"[yellow]Varning: Kunde inte hitta pris för fordonstyp '{vehicleTypeName}' i prislistan. Kostnad blir 0.[/]");
			}
		}

		spot.ParkedVehicles?.Remove(vehicleToRemove);

		// Meddelar användaren om borttagning, tid och kostnad.
		AnsiConsole.MarkupLine($"\nFordonet med reg-nr [green]{regNum}[/] har tagits bort från plats [yellow]{spot.SpotNumber}[/].");
		AnsiConsole.MarkupLine($"Parkerad tid: {parkedDuration.TotalMinutes:F0} minuter."); // F0 = inga decimaler
		AnsiConsole.MarkupLine($"Kostnad: [bold yellow]{cost} CZK[/].");

		return true;
	}

	return false;
}

// Försöker flytta ett fordon från en plats till en annan.
bool MoveVehicle(ParkingGarage garage, string regNum, int toSpotNumber)
{
	if (string.IsNullOrWhiteSpace(regNum))
	{
		AnsiConsole.MarkupLine("\n[red]Ogiltigt registreringsnummer angivet.[/]");
		return false;
	}

	regNum = regNum.ToUpper();

	ParkingSpot? fromSpot = FindSpotByRegNum(garage, regNum);

	if (fromSpot == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Fordonet med registreringsnummer '{regNum}' kunde inte hittas.[/]");
		return false;
	}

	// Validerar destinationsplatsens nummer.
	int totalSpots = garage.Spots?.Count ?? 0;
	if (toSpotNumber < 1 || toSpotNumber > totalSpots)
	{
		AnsiConsole.MarkupLine($"\n[red]Ogiltig destinationsplats. Ange ett nummer mellan 1-{totalSpots}.[/]");
		return false;
	}

	ParkingSpot? toSpot = garage.Spots?[toSpotNumber - 1];

	// Hanterar om platsen oväntat inte finns.
	if (toSpot == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Internt fel: Kunde inte hitta destinationsplats {toSpotNumber}.[/]");
		return false;
	}

	// Kontrollerar om destinationen är upptagen.
	if ((toSpot.ParkedVehicles?.Count ?? 0) > 0)
	{
		AnsiConsole.MarkupLine($"\n[red]Destinationsplatsen {toSpotNumber} är redan upptagen.[/]");
		return false;
	}
	// Kontrollerar om källan och destinationen är samma plats.
	if (fromSpot.SpotNumber == toSpot.SpotNumber)
	{
		AnsiConsole.MarkupLine("\n[yellow]Fordonet står redan på denna plats. Ingen flytt utförd.[/]");
		return false;
	}

	Vehicle? vehicleToMove = null;
	// Hittar det exakta fordonsobjektet som ska flyttas.
	foreach (Vehicle vehicle in fromSpot.ParkedVehicles ?? Enumerable.Empty<Vehicle>())
	{
		if (vehicle.RegNum != null && vehicle.RegNum.Equals(regNum, StringComparison.OrdinalIgnoreCase))
		{
			vehicleToMove = vehicle;
			break;
		}
	}

	// Om fordonsobjektet hittades:
	if (vehicleToMove != null)
	{
		fromSpot.ParkedVehicles?.Remove(vehicleToMove);
		toSpot.ParkedVehicles?.Add(vehicleToMove);

		AnsiConsole.MarkupLine($"\nFordonet [green]{regNum}[/] har flyttats från plats [yellow]{fromSpot.SpotNumber}[/] till plats [yellow]{toSpot.SpotNumber}[/].");
		return true;
	}

	return false;
}