using PragueParkingV2.Core;
using PragueParkingV2.Data;
using Spectre.Console;
using System.Linq;
using System.Collections.Generic;

// ======== SKAPA DATAACCESS OCH LADDA GARAGET ========
DataAccess dataAccess = new DataAccess();
Config config = dataAccess.LoadConfig();
Dictionary<string, decimal> priceList = dataAccess.LoadPriceList();
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
void AddVehicleUI(ParkingGarage garage, Config config)
{
	AnsiConsole.MarkupLine("[underline blue]Parkera Fordon[/]");
	bool actionTaken = false;

	var vehicleTypeChoices = config.AllowedVehicleTypes?
								.Select(vt => vt.TypeName)
								.ToList() ?? new List<string>(); 

	vehicleTypeChoices.Add("[grey]Avbryt[/]"); 

	var selectedTypeName = AnsiConsole.Prompt(
		new SelectionPrompt<string>()
			.Title("Vilken [green]fordonstyp[/]?")
			.PageSize(vehicleTypeChoices.Count)
			.AddChoices(vehicleTypeChoices));

	if (selectedTypeName == "[grey]Avbryt[/]") return;

	var regNum = AnsiConsole.Prompt(
		new TextPrompt<string>("Ange [green]registreringsnummer[/] (lämna tomt för att avbryta):")
			.AllowEmpty()
		)?.ToUpper();

	if (string.IsNullOrWhiteSpace(regNum)) return;

	AddVehicle(garage, config, selectedTypeName, regNum); 
	actionTaken = true;

	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

void SearchVehicleUI(ParkingGarage garage)
{
	AnsiConsole.MarkupLine("[underline blue]Sök Fordon[/]");
	bool actionTaken = false;

	var searchType = AnsiConsole.Prompt(
		new SelectionPrompt<string>()
			.Title("Sök efter [green]fordon via[/]?")
			.PageSize(3)
			.AddChoices(new[] { "Registreringsnummer", "Platsnummer", "[grey]Avbryt[/]" }));

	if (searchType == "[grey]Avbryt[/]") return;

	if (searchType == "Registreringsnummer")
	{
		var regNumToSearch = AnsiConsole.Prompt(
			new TextPrompt<string>("Ange [green]registreringsnummer[/] att söka efter (lämna tomt för att avbryta):")
				.AllowEmpty()
			)?.ToUpper();

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
	else
	{
		var spotNumToShow = AnsiConsole.Prompt(
			new TextPrompt<int?>("Ange [green]platsnummer[/] (1-100) att visa (lämna tomt för att avbryta):")
				.AllowEmpty()
				.HideDefaultValue()
				.ValidationErrorMessage("[red]Ange ett giltigt nummer eller lämna tomt.[/]")
		);


		if (!spotNumToShow.HasValue) return;

		ShowSpotContent(spotNumToShow.Value);
		actionTaken = true;
	}

	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

void RemoveVehicleUI(ParkingGarage garage, Dictionary<string, decimal> priceList) // <-- NY PARAMETER HÄR
{
	AnsiConsole.MarkupLine("[underline blue]Ta bort Fordon[/]");
	bool actionTaken = false;

	var regNumToRemove = AnsiConsole.Prompt(
		new TextPrompt<string>("Ange [green]registreringsnummer[/] på fordonet att ta bort (lämna tomt för att avbryta):")
			.AllowEmpty()
		)?.ToUpper();

	if (string.IsNullOrWhiteSpace(regNumToRemove)) return;

	bool success = RemoveVehicle(garage, regNumToRemove, priceList);
	actionTaken = true;

	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

void MoveVehicleUI(ParkingGarage garage)
{
	AnsiConsole.MarkupLine("[underline blue]Flytta Fordon[/]");
	bool actionTaken = false;

	var regNumToMove = AnsiConsole.Prompt(
		new TextPrompt<string>("Ange [green]registreringsnummer[/] på fordonet som ska flyttas (lämna tomt för att avbryta):")
			.AllowEmpty()
		)?.ToUpper();

	if (string.IsNullOrWhiteSpace(regNumToMove)) return;

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

		bool success = MoveVehicle(garage, regNumToMove, toSpotNumber.Value);
		actionTaken = true;
	}

	if (actionTaken)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
		Console.ReadKey(true);
	}
}

void DisplayGarageMapUI(ParkingGarage garage, Config config)
{
	AnsiConsole.MarkupLine("[underline blue]Översiktskarta - Parkeringshuset[/]");
	AnsiConsole.WriteLine();

	int spotsPerRow = 10;

	// Loopa igenom alla platser, sorterade efter nummer
	foreach (ParkingSpot spot in garage.Spots?.OrderBy(s => s.SpotNumber) ?? Enumerable.Empty<ParkingSpot>())
	{
		string color;
		string content = $"{spot.SpotNumber:D2}";

		int vehicleCount = spot.ParkedVehicles?.Count ?? 0;
		int maxCapacity = 1; // Standardkapacitet om ingen specifik finns

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

		// Sätt färg baserat på status
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
		else // vehicleCount >= maxCapacity (Full plats)
		{
			color = "red";
			content = $"{spot.SpotNumber:D2}:";

			string regNums = "";
			if (spot.ParkedVehicles != null)
			{
				// Loopa igenom alla fordon på platsen (kan vara 1 bil eller 2 MC)
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

		AnsiConsole.Markup($"[{color}]{content}[/] ");

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

	AnsiConsole.MarkupLine("[grey]Tryck valfri tangent för att återgå...[/]");
	Console.ReadKey(true);
}

void ShowSpotContent(int spotNumber)
{
	if (spotNumber >= 1 && spotNumber <= (garage.Spots?.Count ?? 0))
	{
		int index = spotNumber - 1;
		ParkingSpot? spot = garage.Spots?[index];

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
		else
		{
			AnsiConsole.MarkupLine($"[red]Fel: Kunde inte hitta information för plats {spotNumber}.[/]");
		}
	}
	else
	{
		AnsiConsole.MarkupLine($"\n[red]Ogiltigt platsnummer. Ange ett nummer mellan 1 och {garage.Spots?.Count ?? 100}.[/]");
	}
}

// --- Statusutskrift (Används i huvudloopen) ---
void PrintGarageStatus(ParkingGarage garage)
{
	AnsiConsole.MarkupLine("[cyan]--- GARAGE STATUS ---[/]");
	bool isEmpty = true;
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
void AddVehicle(ParkingGarage garage, Config config, string selectedTypeName, string regNum)
{
	if (string.IsNullOrWhiteSpace(regNum)) { AnsiConsole.MarkupLine("\n[red]Ogiltigt registreringsnummer.[/]"); return; }
	regNum = regNum.ToUpper();

	if (FindSpotByRegNum(garage, regNum) != null) { AnsiConsole.MarkupLine($"\n[red]Ett fordon med reg-nr {regNum} finns redan parkerat.[/]"); return; }

	VehicleTypeConfig? typeConfig = config.AllowedVehicleTypes?.FirstOrDefault(vt => vt.TypeName == selectedTypeName);

	if (typeConfig == null) { AnsiConsole.MarkupLine($"\n[red]Okänd fordonstyp: {selectedTypeName}.[/]"); return; }

	ParkingSpot? targetSpot = null;

	if (typeConfig.MaxPerSpot > 1)
	{
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

	if (targetSpot == null)
	{
		foreach (ParkingSpot spot in garage.Spots ?? Enumerable.Empty<ParkingSpot>())
		{
			if ((spot.ParkedVehicles?.Count ?? 0) == 0)
			{
				targetSpot = spot;
				break;
			}
		}
	}

	if (targetSpot != null)
	{
		Vehicle? newVehicle = null;

		if (selectedTypeName == "CAR")
		{
			newVehicle = new Car { RegNum = regNum, ArrivalTime = DateTime.Now };
		}
		else if (selectedTypeName == "MC")
		{
			newVehicle = new MC { RegNum = regNum, ArrivalTime = DateTime.Now };
		}
		else
		{
			AnsiConsole.MarkupLine($"\n[red]Kan inte skapa okänd fordonstyp: {selectedTypeName}.[/]");
			return;
		}

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

ParkingSpot? FindSpotByRegNum(ParkingGarage garage, string regNum)
{
	if (string.IsNullOrWhiteSpace(regNum)) return null;

	foreach (ParkingSpot spot in garage.Spots ?? Enumerable.Empty<ParkingSpot>())
	{
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

bool RemoveVehicle(ParkingGarage garage, string regNum, Dictionary<string, decimal> priceList)
{
	if (string.IsNullOrWhiteSpace(regNum))
	{
		Console.WriteLine("\nOgiltigt registreringsnummer angivet.");
		return false;
	}

	regNum = regNum.ToUpper();

	ParkingSpot? spot = FindSpotByRegNum(garage, regNum); 
	if (spot == null)
	{
		Console.WriteLine($"\nEtt fordon med registreringsnummer '{regNum}' kunde inte hittas.");
		return false;
	}

	Vehicle? vehicleToRemove = null;
	foreach (Vehicle vehicle in spot.ParkedVehicles ?? Enumerable.Empty<Vehicle>())
	{
		if (vehicle.RegNum != null && vehicle.RegNum.Equals(regNum, StringComparison.OrdinalIgnoreCase))
		{
			vehicleToRemove = vehicle;
			break;
		}
	}

	if (vehicleToRemove != null)
	{
		TimeSpan parkedDuration = DateTime.Now - vehicleToRemove.ArrivalTime;
		decimal cost = 0;

		if (parkedDuration.TotalMinutes > 10)
		{
			string vehicleTypeName = vehicleToRemove is Car ? "CAR" : (vehicleToRemove is MC ? "MC" : "");

			if (priceList.TryGetValue(vehicleTypeName, out decimal pricePerHour))
			{
				double totalHours = parkedDuration.TotalHours;
				int billableHours = (int)Math.Ceiling(totalHours); // Avrunda alltid uppåt

				cost = billableHours * pricePerHour;
			}
			else if (!string.IsNullOrEmpty(vehicleTypeName))
			{
				AnsiConsole.MarkupLine($"[yellow]Varning: Kunde inte hitta pris för fordonstyp '{vehicleTypeName}' i prislistan. Kostnad blir 0.[/]");
			}
		}

		spot.ParkedVehicles?.Remove(vehicleToRemove);

		AnsiConsole.MarkupLine($"\nFordonet med reg-nr [green]{regNum}[/] har tagits bort från plats [yellow]{spot.SpotNumber}[/].");
		AnsiConsole.MarkupLine($"Parkerad tid: {parkedDuration.TotalMinutes:F0} minuter."); // F0 = inga decimaler
		AnsiConsole.MarkupLine($"Kostnad: [bold yellow]{cost} CZK[/].");

		return true;
	}

	return false;
}

bool MoveVehicle(ParkingGarage garage, string regNum, int toSpotNumber)
{
	if (string.IsNullOrWhiteSpace(regNum))
	{
		Console.WriteLine("\nOgiltigt registreringsnummer angivet.");
		return false;
	}

	regNum = regNum.ToUpper();

	ParkingSpot? fromSpot = FindSpotByRegNum(garage, regNum);
	if (fromSpot == null)
	{
		Console.WriteLine($"\nFordonet med registreringsnummer '{regNum}' kunde inte hittas.");
		return false;
	}

	int totalSpots = garage.Spots?.Count ?? 0;
	if (toSpotNumber < 1 || toSpotNumber > totalSpots)
	{
		Console.WriteLine($"\nOgiltig destinationsplats. Ange ett nummer mellan 1-{totalSpots}.");
		return false;
	}

	ParkingSpot? toSpot = garage.Spots?[toSpotNumber - 1];

	if (toSpot == null)
	{
		Console.WriteLine($"\nInternt fel: Kunde inte hitta destinationsplats {toSpotNumber}.");
		return false;
	}

	if ((toSpot.ParkedVehicles?.Count ?? 0) > 0)
	{
		Console.WriteLine($"\nDestinationsplatsen {toSpotNumber} är redan upptagen.");
		return false;
	}

	if (fromSpot.SpotNumber == toSpot.SpotNumber)
	{
		Console.WriteLine("\nFordonet står redan på denna plats. Ingen flytt utförd.");
		return false;
	}

	Vehicle? vehicleToMove = null;
	foreach (Vehicle vehicle in fromSpot.ParkedVehicles ?? Enumerable.Empty<Vehicle>())
	{
		if (vehicle.RegNum != null && vehicle.RegNum.Equals(regNum, StringComparison.OrdinalIgnoreCase))
		{
			vehicleToMove = vehicle;
			break;
		}
	}

	if (vehicleToMove != null)
	{
		fromSpot.ParkedVehicles?.Remove(vehicleToMove);
		toSpot.ParkedVehicles?.Add(vehicleToMove);

		Console.WriteLine($"\nFordonet {regNum} har flyttats från plats {fromSpot.SpotNumber} till plats {toSpot.SpotNumber}.");
		return true;
	}

	return false;
}