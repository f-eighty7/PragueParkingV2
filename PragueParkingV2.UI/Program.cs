using PragueParkingV2.Core;
using PragueParkingV2.Data;

DataAccess dataAccess = new DataAccess(); // Skapa objektet som hanterar filer
ParkingGarage garage = dataAccess.LoadGarage(); // Ladda från fil (eller få ett nytt tomt)



// --- UI-METODER ---
void AddVehicleUI(ParkingGarage garage) //
{
	Console.Write("Vilken fordonstyp? (1 för Bil, 2 för MC): "); 
	string vehicleChoice = Console.ReadLine();
	Console.Write("Ange registreringsnummer: "); 
	string regNum = Console.ReadLine();

	AddVehicle(garage, vehicleChoice, regNum);
}

void SearchVehicleUI(ParkingGarage garage)
{
	Console.Write("Ange registreringsnummer att söka efter: ");
	string regNumFromUser = Console.ReadLine();
	ParkingSpot? foundSpot = FindSpotByRegNum(garage, regNumFromUser);

	if (foundSpot != null)
	{
		Console.WriteLine($"\nFordonet hittades på plats: {foundSpot.SpotNumber}");
		Console.WriteLine("Fordon på denna plats:");
		foreach (Vehicle vehicle in foundSpot.ParkedVehicles ?? Enumerable.Empty<Vehicle>())
		{
			Console.WriteLine($"- Reg-nr: {vehicle.RegNum ?? "N/A"}, Ankomst: {vehicle.ArrivalTime}");
		}
	}
	else
	{
		Console.WriteLine($"\nEtt fordon med registreringsnummer '{regNumFromUser}' kunde inte hittas.");
	}
}

void RemoveVehicleUI(ParkingGarage garage) 
{
	Console.Write("Ange registreringsnummer på fordonet du vill ta bort: "); 
	string regNumFromUser = Console.ReadLine();
	RemoveVehicle(garage, regNumFromUser);
}

void MoveVehicleUI(ParkingGarage garage)
{
	Console.Write("Ange registreringsnummer på fordonet som ska flyttas: ");
	string regNumFromUser = Console.ReadLine();

	Console.Write($"Ange ny plats (1-100) för fordonet '{regNumFromUser}': "); 
	if (int.TryParse(Console.ReadLine(), out int toSpotNumber))
	{
		MoveVehicle(garage, regNumFromUser, toSpotNumber);
	}
	else
	{
		Console.WriteLine("\nFelaktig inmatning. Vänligen ange en siffra för platsnummer.");
	}
}

// --- LOGIK-METODER ---

void AddVehicle(ParkingGarage garage, string vehicleChoice, string regNum)
{
	if (string.IsNullOrWhiteSpace(regNum))
	{
		Console.WriteLine("\nOgiltigt registreringsnummer.");
		return;
	}

	regNum = regNum.ToUpper();

	if (vehicleChoice == "1")
	{
		ParkingSpot? foundSpot = null;
		foreach (ParkingSpot spot in garage.Spots ?? Enumerable.Empty<ParkingSpot>())
		{
			if ((spot.ParkedVehicles?.Count ?? 0) == 0)
			{
				foundSpot = spot;
				break;
			}
		}

		if (foundSpot != null)
		{
			if (FindSpotByRegNum(garage, regNum) != null)
			{
				Console.WriteLine($"\nEtt fordon med reg-nr {regNum} finns redan parkerat.");
				return;
			}
			Car newCar = new Car { RegNum = regNum, ArrivalTime = DateTime.Now };
			foundSpot.ParkedVehicles?.Add(newCar);
			Console.WriteLine($"\nBilen med reg-nr {regNum} har parkerats på plats {foundSpot.SpotNumber}.");
		}
		else
		{
			Console.WriteLine("\nTyvärr är parkeringen full för bilar.");
		}
	}
	else if (vehicleChoice == "2")
	{
		if (FindSpotByRegNum(garage, regNum) != null)
		{
			Console.WriteLine($"\nEtt fordon med reg-nr {regNum} finns redan parkerat.");
			return;
		}

		bool parked = false;
		foreach (ParkingSpot spot in garage.Spots ?? Enumerable.Empty<ParkingSpot>())
		{
			if ((spot.ParkedVehicles?.Count ?? 0) == 1 && spot.ParkedVehicles[0] is MC)
			{
				MC newMC = new MC { RegNum = regNum, ArrivalTime = DateTime.Now };
				spot.ParkedVehicles?.Add(newMC);
				Console.WriteLine($"\nMotorcykeln med reg-nr {regNum} har dubbelparkerats på plats {spot.SpotNumber}.");
				parked = true;
				break;
			}
		}

		if (!parked)
		{
			ParkingSpot? foundSpot = null;
			foreach (ParkingSpot spot in garage.Spots ?? Enumerable.Empty<ParkingSpot>())
			{
				if ((spot.ParkedVehicles?.Count ?? 0) == 0)
				{
					foundSpot = spot;
					break;
				}
			}

			if (foundSpot != null)
			{
				MC newMC = new MC { RegNum = regNum, ArrivalTime = DateTime.Now };
				foundSpot.ParkedVehicles?.Add(newMC);
				Console.WriteLine($"\nMotorcykeln med reg-nr {regNum} har parkerats på plats {foundSpot.SpotNumber}.");
			}
			else
			{
				Console.WriteLine("\nTyvärr är parkeringen full för motorcyklar.");
			}
		}
	}
	else
	{
		Console.WriteLine("\nOgiltigt val av fordonstyp.");
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


bool RemoveVehicle(ParkingGarage garage, string regNum)
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
		spot.ParkedVehicles?.Remove(vehicleToRemove);
		Console.WriteLine($"\nFordonet med reg-nr {regNum} har tagits bort från plats {spot.SpotNumber}.");
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