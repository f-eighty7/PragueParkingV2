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

#region // ======== HUVUDMENY ========
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
			MoveVehicleUI(garage, config);
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
#endregion

#region // === UI-METODER ===

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

		IParkingSpot? foundSpot = FindSpotByRegNum(garage, regNumToSearch);
		if (foundSpot != null)
		{
			AnsiConsole.MarkupLine($"\nFordonet hittades på plats: [yellow]{foundSpot.SpotNumber}[/]");
			AnsiConsole.MarkupLine("Fordon på denna plats:");

			// === UPPDATERAD LOOP (VG) ===
			foreach (IVehicle vehicle in foundSpot.ParkedVehicles ?? Enumerable.Empty<IVehicle>())
			{
				// Dynamiskt typnamn och färg
				string vehicleType = GetVehicleTypeMarkup(vehicle);
				AnsiConsole.MarkupLine($"- Reg-nr: [green]{vehicle.RegNum ?? "N/A"}[/], Typ: {vehicleType}, Ankomst: {vehicle.ArrivalTime}");
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

		ShowSpotContent(spotNumToShow.Value, config);
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
void MoveVehicleUI(ParkingGarage garage, Config config)
{
	AnsiConsole.MarkupLine("[underline blue]Flytta Fordon[/]");
	bool actionTaken = false;

	var regNumToMove = AnsiConsole.Prompt(
		new TextPrompt<string>("Ange [green]registreringsnummer[/] på fordonet som ska flyttas (lämna tomt för att avbryta):")
			.AllowEmpty()
		)?.ToUpper();

	if (string.IsNullOrWhiteSpace(regNumToMove)) return;

	// Hitta den FÖRSTA platsen fordonet står på (för att kunna validera)
	var fromSpot = FindSpotByRegNum(garage, regNumToMove);
	if (fromSpot == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Fordonet med registreringsnummer '{regNumToMove}' kunde inte hittas.[/]");
		actionTaken = true;
	}
	else
	{
		var toSpotNumber = AnsiConsole.Prompt(
			new TextPrompt<int?>($"Ange [green]ny plats[/] (1-100) för fordonet '{regNumToMove}' (lämna tomt för att avbryta):")
				.AllowEmpty()
				.HideDefaultValue()
				.ValidationErrorMessage("[red]Ange ett giltigt nummer eller lämna tomt.[/]")
		);

		if (!toSpotNumber.HasValue) return;

		bool success = MoveVehicle(garage, config, regNumToMove, toSpotNumber.Value);
		actionTaken = true;
	}

	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

// Visar en visuell karta över parkeringshuset med färger för status.
// === NY DisplayGarageMapUI-METOD FÖR VG ===
void DisplayGarageMapUI(ParkingGarage garage, Config config)
{
	AnsiConsole.MarkupLine("[underline blue]Översiktskarta - Parkeringshuset[/]");
	AnsiConsole.WriteLine();

	int spotsPerRow = 10;
	int spotCapacity = config.ParkingSpotSize;

	// Loopa igenom alla platser, sorterade efter nummer
	foreach (IParkingSpot spot in garage.Spots?.OrderBy(s => s.SpotNumber) ?? Enumerable.Empty<IParkingSpot>())
	{
		string color;
		string content = $"{spot.SpotNumber:D2}"; // "01", "02", etc.
		int occupiedSpace = spot.OccupiedSpace; // Hämta beläggningen

		// === NY VG-LOGIK FÖR FÄRG ===
		if (occupiedSpace == 0)
		{
			// Platsen är helt tom
			color = "green";
			content += ": ---";
		}
		else if (occupiedSpace >= spotCapacity)
		{
			// Platsen är helt full (eller överfull, t.ex. en buss)
			color = "red";
		}
		else // occupiedSpace > 0 AND occupiedSpace < spotCapacity
		{
			// Platsen är delvis fylld (t.ex. 1 MC eller 2 Cyklar)
			color = "yellow";
		}

		// Om platsen inte är tom, lägg till reg-nummer
		if (occupiedSpace > 0)
		{
			content += ":";
			string regNums = "";

			// Använd Distinct() för att bara visa en buss reg-nr en gång per ruta
			foreach (IVehicle vehicle in spot.ParkedVehicles.Distinct())
			{
				if (!string.IsNullOrEmpty(regNums))
				{
					regNums += ","; // Separera flera fordon (t.ex. 2 MC)
				}
				regNums += vehicle.RegNum ?? "???";
			}
			content += regNums;
		}

		// Skriv ut den formaterade platsen (med 8 tecken för att justera)
		AnsiConsole.Markup($"[{color}]{content.PadRight(8)}[/] ");

		// Radbrytning var 10:e plats
		if (spot.SpotNumber % spotsPerRow == 0)
		{
			Console.WriteLine();
			Console.WriteLine();
		}
	}

	AnsiConsole.WriteLine();
	AnsiConsole.MarkupLine("[underline]Förklaring:[/]");
	AnsiConsole.MarkupLine("[green]XX: ---[/] : Ledig Plats (0/" + spotCapacity + ")");
	AnsiConsole.MarkupLine("[yellow]XX: ABC...[/] : Delvis upptagen (1-" + (spotCapacity - 1) + "/" + spotCapacity + ")");
	AnsiConsole.MarkupLine("[red]XX: DEF...[/] : Full Plats (" + spotCapacity + "/" + spotCapacity + " eller mer)");
	AnsiConsole.WriteLine();

	AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
	Console.ReadKey(true);
}

// Visar detaljerat innehåll för en specifik parkeringsplats.
void ShowSpotContent(int spotNumber, Config config)
{
	if (spotNumber >= 1 && spotNumber <= (garage.Spots?.Count ?? 0))
	{
		int index = spotNumber - 1;
		IParkingSpot? spot = garage.Spots?[index];
		int spotCapacity = config.ParkingSpotSize;

		// Visar status och innehåll för platsen.
		if (spot != null && spot.OccupiedSpace == 0)
		{
			AnsiConsole.MarkupLine($"\nPlats [yellow]{spotNumber}[/] är [green]tom[/].");
		}
		else if (spot != null)
		{
			AnsiConsole.MarkupLine($"\nPå plats [yellow]{spotNumber}[/] står:");

			// === UPPDATERAD LOOP (VG) ===
			foreach (IVehicle vehicle in spot.ParkedVehicles ?? Enumerable.Empty<IVehicle>())
			{
				string vehicleType = GetVehicleTypeMarkup(vehicle); // Använd ny hjälpmetod
				AnsiConsole.MarkupLine($"- Reg-nr: [green]{vehicle.RegNum ?? "N/A"}[/], Typ: {vehicleType}, Size: {vehicle.Size}, Ankomst: {vehicle.ArrivalTime}");
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
	foreach (IParkingSpot spot in garage.Spots?.OrderBy(s => s.SpotNumber) ?? Enumerable.Empty<IParkingSpot>())
	{
		if (spot.OccupiedSpace > 0)
		{
			isEmpty = false;
			AnsiConsole.Markup($"Plats [bold]{spot.SpotNumber}[/]: ");

			// === UPPDATERAD LOOP (VG) ===
			// Använd LINQ för att undvika dubletter (för bussar som upptar flera platser)
			var distinctVehicles = spot.ParkedVehicles.Distinct();

			foreach (IVehicle vehicle in distinctVehicles)
			{
				string vehicleType = GetVehicleTypeMarkup(vehicle); // Använd ny hjälpmetod
				AnsiConsole.Markup($"{vehicleType}:[green]{vehicle.RegNum ?? "N/A"}[/] (Size:{vehicle.Size}) ");
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
#endregion

#region // === LOGIK-METODER ===

// Hanterar logiken för att parkera ett fordon baserat på konfiguration.
// === NY AddVehicle-METOD FÖR VG ===
void AddVehicle(ParkingGarage garage, Config config, string selectedTypeName, string regNum)
{
	if (string.IsNullOrWhiteSpace(regNum))
	{
		AnsiConsole.MarkupLine("\n[red]Ogiltigt registreringsnummer.[/]");
		return;
	}

	regNum = regNum.ToUpper();

	if (FindSpotByRegNum(garage, regNum) != null)
	{
		AnsiConsole.MarkupLine($"\n[red]Ett fordon med reg-nr {regNum} finns redan parkerat.[/]");
		return;
	}

	// --- Skapa fordonet ---
	IVehicle? newVehicle = CreateVehicle(selectedTypeName, regNum);
	if (newVehicle == null)
	{
		return;
	}

	// --- Hämta relevanta storlekar ---
	int vehicleSize = newVehicle.Size;
	int spotCapacity = config.ParkingSpotSize;
	bool parked = false;

	// --- Huvudlogik: Välj parkeringsstrategi baserat på storlek ---

	// STRATEGI A: Enkel parkering (t.ex. BIL, MC, CYKEL)
	if (vehicleSize <= spotCapacity)
	{
		IParkingSpot? targetSpot = null;

		// Hitta första platsen där fordonet ryms
		foreach (IParkingSpot spot in garage.Spots ?? Enumerable.Empty<IParkingSpot>())
		{
			int availableSpace = spotCapacity - spot.OccupiedSpace;

			// Kontrollera om fordonet får plats i det återstående utrymmet
			if (vehicleSize <= availableSpace)
			{
				targetSpot = spot;
				break;
			}
		}

		// Om vi hittade en plats, parkera fordonet
		if (targetSpot != null)
		{
			targetSpot.ParkedVehicles.Add(newVehicle);
			parked = true;
			AnsiConsole.MarkupLine($"\nFordonet [green]{regNum}[/] ({selectedTypeName}, Size: {vehicleSize}) har parkerats på plats [yellow]{targetSpot.SpotNumber}[/].");
			AnsiConsole.MarkupLine($"Platsens beläggning: {targetSpot.OccupiedSpace}/{spotCapacity}");
		}
	}
	// STRATEGI B: Block-parkering (t.ex. BUSS)
	else
	{
		// Räkna ut hur många sammanhängande platser som behövs
		int spotsNeeded = (int)Math.Ceiling((double)vehicleSize / spotCapacity); // T.ex. 16 / 4 = 4 platser

		// Bussar får bara parkeras på plats 1-50
		const int busMaxSpotIndex = 49; // Index 49 = Plats 50
		List<ParkingSpot>? targetSpots = null;

		// Loopa från plats 0 upp till (max-plats - platser_som_behövs)
		for (int i = 0; i <= (busMaxSpotIndex - spotsNeeded + 1); i++)
		{
			// Hämta en "kandidat-lista" av platser
			var consecutiveSpots = garage.Spots.Skip(i).Take(spotsNeeded).ToList();

			// Kontrollera om ALLA platser i denna lista är HELT tomma
			if (consecutiveSpots.Count == spotsNeeded && consecutiveSpots.All(s => s.OccupiedSpace == 0))
			{
				targetSpots = consecutiveSpots;
				break;
			}
		}

		// Om vi hittade ett block av platser, parkera fordonet
		if (targetSpots != null)
		{
			// Lägg till SAMMA fordons-objekt i VARJE plats den upptar
			// Detta säkerställer att OccupiedSpace blir korrekt (16) för alla 4 platser
			foreach (ParkingSpot spot in targetSpots)
			{
				spot.ParkedVehicles.Add(newVehicle);
			}
			parked = true;
			AnsiConsole.MarkupLine($"\nFordonet [green]{regNum}[/] ({selectedTypeName}, Size: {vehicleSize}) har parkerats över platserna [yellow]{targetSpots.First().SpotNumber} - {targetSpots.Last().SpotNumber}[/].");
		}
	}

	// --- 5. Hantera misslyckande ---
	if (!parked)
	{
		AnsiConsole.MarkupLine($"\n[red]Tyvärr finns det ingen ledig plats för {selectedTypeName}.[/]");
	}
}

// Hittar och returnerar den ParkingSpot där ett fordon med angivet regNr finns.
IParkingSpot? FindSpotByRegNum(ParkingGarage garage, string regNum)
{
	if (string.IsNullOrWhiteSpace(regNum)) return null;

	// Loopar igenom alla platser.
	foreach (IParkingSpot spot in garage.Spots ?? Enumerable.Empty<IParkingSpot>())
	{
		// Loopar igenom alla fordon på den aktuella platsen.
		// Letar efter 'IVehicle' istället för 'Vehicle'
		foreach (IVehicle vehicle in spot.ParkedVehicles ?? Enumerable.Empty<IVehicle>())
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
// === NY RemoveVehicle-METOD FÖR VG ===
bool RemoveVehicle(ParkingGarage garage, string regNum, Dictionary<string, decimal> priceList)
{
	if (string.IsNullOrWhiteSpace(regNum))
	{
		AnsiConsole.MarkupLine("\n[red]Ogiltigt registreringsnummer angivet.[/]");
		return false;
	}

	regNum = regNum.ToUpper();

	// --- Hitta fordonet ---
	// Hitta den FÖRSTA platsen fordonet står på.
	// Variabeln 'firstSpot' är nu av typen interface
	IParkingSpot? firstSpot = FindSpotByRegNum(garage, regNum);
	if (firstSpot == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Ett fordon med registreringsnummer '{regNum}' kunde inte hittas.[/]");
		return false;
	}

	// Hämta det faktiska fordons-objektet från platsen
	IVehicle? vehicleToRemove = firstSpot.ParkedVehicles
		.FirstOrDefault(v => v.RegNum != null && v.RegNum.Equals(regNum, StringComparison.OrdinalIgnoreCase));

	if (vehicleToRemove == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Internt fel: Kunde inte hämta fordonsobjektet {regNum}.[/]");
		return false;
	}

	// --- Beräkna kostnad (VG-anpassad) ---
	TimeSpan parkedDuration = DateTime.Now - vehicleToRemove.ArrivalTime;
	decimal cost = 0;

	if (parkedDuration.TotalMinutes > 10)
	{
		// Hämta typnamnet direkt från klassen (t.ex. "CAR", "MC", "BIKE", "BUS")
		string vehicleTypeName = vehicleToRemove.GetType().Name.ToUpper();

		if (priceList.TryGetValue(vehicleTypeName, out decimal pricePerHour))
		{
			double totalHours = parkedDuration.TotalHours;
			int billableHours = (int)Math.Ceiling(totalHours); // Avrunda alltid uppåt
			cost = billableHours * pricePerHour;
		}
		else
		{
			AnsiConsole.MarkupLine($"[yellow]Varning: Kunde inte hitta pris för fordonstyp '{vehicleTypeName}' i prislistan. Kostnad blir 0.[/]");
		}
	}

	// --- Ta bort fordonet ---
	// Loopa igenom ALLA platser i hela garaget
	// och ta bort detta specifika fordons-objekt överallt där det finns.
	// För en bil/MC tas den bort från 1 plats.
	// För en buss tas den bort från 4 platser.
	int spotsCleared = 0;
	foreach (IParkingSpot spot in garage.Spots ?? Enumerable.Empty<IParkingSpot>())
	{
		if (spot.ParkedVehicles.Remove(vehicleToRemove))
		{
			spotsCleared++;
		}
	}

	AnsiConsole.MarkupLine($"\nFordonet med reg-nr [green]{regNum}[/] har tagits bort.");
	if (spotsCleared > 1)
	{
		AnsiConsole.MarkupLine($"Frigjorde [yellow]{spotsCleared}[/] platser (Plats {firstSpot.SpotNumber} m.fl.).");
	}
	else
	{
		AnsiConsole.MarkupLine($"Frigjorde plats [yellow]{firstSpot.SpotNumber}[/].");
	}

	AnsiConsole.MarkupLine($"Parkerad tid: {parkedDuration.TotalMinutes:F0} minuter.");
	AnsiConsole.MarkupLine($"Kostnad: [bold yellow]{cost} CZK[/].");

	return true;
}

// Försöker flytta ett fordon från en plats till en annan.
// === NY MoveVehicle-METOD FÖR VG ===
bool MoveVehicle(ParkingGarage garage, Config config, string regNum, int toSpotNumber)
{
	// --- Hitta fordonsobjektet ---
	IParkingSpot? fromSpot = FindSpotByRegNum(garage, regNum);
	if (fromSpot == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Fordonet med registreringsnummer '{regNum}' kunde inte hittas.[/]");
		return false;
	}

	IVehicle? vehicleToMove = fromSpot.ParkedVehicles
		.FirstOrDefault(v => v.RegNum != null && v.RegNum.Equals(regNum, StringComparison.OrdinalIgnoreCase));

	if (vehicleToMove == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Internt fel: Kunde inte hämta fordonsobjektet {regNum}.[/]");
		return false;
	}

	// --- Kontrollera giltighet för 'till'-platsen ---
	int totalSpots = garage.Spots?.Count ?? 0;
	if (toSpotNumber < 1 || toSpotNumber > totalSpots)
	{
		AnsiConsole.MarkupLine($"\n[red]Ogiltig destinationsplats. Ange ett nummer mellan 1-{totalSpots}.[/]");
		return false;
	}

	ParkingSpot? toSpot = garage.Spots?[toSpotNumber - 1];
	if (toSpot == null)
	{
		AnsiConsole.MarkupLine($"\n[red]Internt fel: Kunde inte hitta destinationsplats {toSpotNumber}.[/]");
		return false;
	}

	// Kontrollera om vi försöker flytta till samma plats
	if (fromSpot.SpotNumber == toSpot.SpotNumber || toSpot.ParkedVehicles.Contains(vehicleToMove))
	{
		AnsiConsole.MarkupLine("\n[yellow]Fordonet står redan på denna plats. Ingen flytt utförd.[/]");
		return false;
	}

	// --- Hantera fordon baserat på storlek ---
	int vehicleSize = vehicleToMove.Size;
	int spotCapacity = config.ParkingSpotSize;

	// BEGRÄNSNING: Vi tillåter inte flytt av fordon som upptar flera platser
	if (vehicleSize > spotCapacity)
	{
		AnsiConsole.MarkupLine($"\n[red]Fordonet {regNum} (Size: {vehicleSize}) är för stort för att flyttas.[/]");
		AnsiConsole.MarkupLine("[red]Flytt stöds endast för fordon som ryms på en enskild plats.[/]");
		return false;
	}

	// KONTROLL: Har 'till'-platsen tillräckligt med utrymme?
	int availableSpace = spotCapacity - toSpot.OccupiedSpace;
	if (vehicleSize > availableSpace)
	{
		AnsiConsole.MarkupLine($"\n[red]Destinationsplatsen {toSpotNumber} har inte tillräckligt med utrymme.[/]");
		AnsiConsole.MarkupLine($"[red]Kräver: {vehicleSize}, Tillgängligt: {availableSpace} (Beläggning: {toSpot.OccupiedSpace}/{spotCapacity})[/]");
		return false;
	}

	// --- Utför flytten ---
	// Steg 4a: Ta bort fordonet från ALLA platser det kan finnas på (för säkerhets skull)
	foreach (IParkingSpot spot in garage.Spots ?? Enumerable.Empty<IParkingSpot>())
	{
		spot.ParkedVehicles.Remove(vehicleToMove);
	}

	// Steg 4b: Lägg till fordonet på den nya platsen
	toSpot.ParkedVehicles.Add(vehicleToMove);

	AnsiConsole.MarkupLine($"\n[green]Fordonet {regNum} har flyttats till plats {toSpot.SpotNumber}.[/]");
	return true;
}
#endregion

#region// === HJÄLPMETODER ===
IVehicle? CreateVehicle(string typeName, string regNum)
{
	DateTime arrival = DateTime.Now;

	// En switch för att returnera rätt typ av IVehicle
	switch (typeName.ToUpper())
	{
		case "CAR":
			return new Car { RegNum = regNum, ArrivalTime = arrival };
		case "MC":
			return new MC { RegNum = regNum, ArrivalTime = arrival };
		case "BIKE":
			return new Bike { RegNum = regNum, ArrivalTime = arrival };
		case "BUS":
			return new Bus { RegNum = regNum, ArrivalTime = arrival };
		default:
			AnsiConsole.MarkupLine($"\n[red]Internt fel: Okänd fordonstyp: {typeName}.[/]");
			return null;
	}
}

string GetVehicleTypeMarkup(IVehicle vehicle)
{
	// Hämta klassnamnet (t.ex. "CAR", "BIKE")
	string typeName = vehicle.GetType().Name.ToUpper();

	// Returnera en färgkodad sträng baserat på typ
	switch (typeName)
	{
		case "CAR":
			return "[blue]BIL[/]";
		case "MC":
			return "[magenta]MC[/]";
		case "BIKE":
			return "[grey]CYKEL[/]";
		case "BUS":
			return "[red]BUSS[/]";
		default:
			return $"[grey]{typeName}[/]";
	}
}
#endregion