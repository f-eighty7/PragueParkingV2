using PragueParkingV2.Core;

ParkingGarage garage = new ParkingGarage();

for (int i = 0; i < 100; i++)
{
	// 1. Skapa ett nytt, tomt parkeringsplats-objekt från ritningen.
	ParkingSpot newSpot = new ParkingSpot();

	// 2. Ge den nya platsen dess egenskaper. Platsnummer ska vara 1-100.
	newSpot.SpotNumber = i + 1;

	// 3. Lägg till det färdiga objektet i garagets lista av platser.
	garage.Spots.Add(newSpot);
}

void AddVehicle(ParkingGarage garage, string vehicleChoice, string RegNum)
{
	if (vehicleChoice == "1")
	{
		ParkingSpot foundSpot = null;

		foreach (ParkingSpot spot in garage.Spots)
		{
			if (spot.ParkedVehicles.Count == 0)
			{
				foundSpot = spot;
				break;
			}
		}

		if (foundSpot != null)
		{
			Car newCar = new Car();
			newCar.RegNum = RegNum;
			newCar.ArrivalTime = DateTime.Now;

			foundSpot.ParkedVehicles.Add(newCar);
			Console.WriteLine($"Bilen med reg-nr {RegNum} har parkerats på plats {foundSpot.SpotNumber}.");
		}
		else
		{
			Console.WriteLine("Tyvärr är parkeringen full för bilar.");
		}
	}
	else if (vehicleChoice == "2")
	{
		bool parked = false; // En flagga för att se om vi lyckades parkera

		foreach (ParkingSpot spot in garage.Spots)
		{
			// Kolla att det finns ETT fordon, OCH att fordonet ÄR en MC.
			if (spot.ParkedVehicles.Count == 1 && spot.ParkedVehicles[0] is MC)
			{
				MC newMC = new MC();
				newMC.RegNum = RegNum;
				newMC.ArrivalTime = DateTime.Now;

				spot.ParkedVehicles.Add(newMC);

				Console.WriteLine($"Motorcykeln med reg-nr {RegNum} har dubbelparkerats på plats {spot.SpotNumber}.");

				parked = true; // Markera att vi har parkerat.
				break;
			}
		}
		// OM VI INTE HITTADE EN PLATS ATT DELA, LETA EFTER EN TOM PLATS
		if (!parked) // Denna kod körs bara om vi inte lyckades dubbelparkera.
		{
			ParkingSpot foundSpot = null;
			foreach (ParkingSpot spot in garage.Spots)
			{
				if (spot.ParkedVehicles.Count == 0)
				{
					foundSpot = spot;
					break;
				}
			}

			if (foundSpot != null)
			{
				MC newMC = new MC();
				newMC.RegNum = RegNum;
				newMC.ArrivalTime = DateTime.Now;

				foundSpot.ParkedVehicles.Add(newMC);
				Console.WriteLine($"Motorcykeln med reg-nr {RegNum} har parkerats på plats {foundSpot.SpotNumber}.");
			}
			else
			{
				Console.WriteLine("Tyvärr är parkeringen full för motorcyklar.");
			}
		}
	}
}

ParkingSpot FindSpotByRegNum(ParkingGarage garage, string regNum)
{
	foreach (ParkingSpot spot in garage.Spots)
	{
		foreach (Vehicle vehicle in spot.ParkedVehicles)
		{
			if (vehicle.RegNum.ToUpper() == regNum.ToUpper())
			{
				return spot;
			}
		}
	}
	return null;
}

void SearchVehicleUI(ParkingGarage garage)
{
	Console.Write("Ange registreringsnummer att söka efter: ");
	string regNumFromUser = Console.ReadLine();

	ParkingSpot foundSpot = FindSpotByRegNum(garage, regNumFromUser);

	if (foundSpot != null)
	{
		Console.WriteLine($"\nFordonet hittades på plats: {foundSpot.SpotNumber}");
		Console.WriteLine("Fordon på denna plats:");

		foreach (Vehicle vehicle in foundSpot.ParkedVehicles)
		{
			Console.WriteLine($"- Reg-nr: {vehicle.RegNum}, Ankomst: {vehicle.ArrivalTime}");
		}
	}
	else
	{
		Console.WriteLine($"\nEtt fordon med registreringsnummer '{regNumFromUser}' kunde inte hittas.");
	}
}

bool RemoveVehicle(ParkingGarage garage, string regNum)
{
	ParkingSpot spot = FindSpotByRegNum(garage, regNum);

	if (spot == null)
	{
		Console.WriteLine($"\nEtt fordon med registreringsnummer '{regNum}' kunde inte hittas.");
		return false;
	}

	Vehicle vehicleToRemove = null;
	foreach (Vehicle vehicle in spot.ParkedVehicles)
	{
		if (vehicle.RegNum.ToUpper() == regNum.ToUpper())
		{
			vehicleToRemove = vehicle;
			break;
		}
	}

	if (vehicleToRemove != null)
	{
		spot.ParkedVehicles.Remove(vehicleToRemove);
		Console.WriteLine($"\nFordonet med reg-nr {regNum} har tagits bort från plats {spot.SpotNumber}.");
		return true;
	}
	return false;
}

void RemoveVehicleUI(ParkingGarage garage)
{
	Console.Write("Ange registreringsnummer på fordonet du vill ta bort: ");
	string regNumFromUser = Console.ReadLine();

	RemoveVehicle(garage, regNumFromUser);
}

bool MoveVehicle(ParkingGarage garage, string regNum, int toSpotNumber)
{
	ParkingSpot fromSpot = FindSpotByRegNum(garage, regNum);

	if (fromSpot == null)
	{
		Console.WriteLine($"\nFordonet med registreringsnummer '{regNum}' kunde inte hittas.");
		return false;
	}

	if (toSpotNumber < 1 || toSpotNumber > 100)
	{
		Console.WriteLine("\nOgiltig destinationsplats. Ange ett nummer mellan 1-100.");
		return false;
	}

	ParkingSpot toSpot = garage.Spots[toSpotNumber - 1];

	if (toSpot.ParkedVehicles.Count > 0)
	{
		Console.WriteLine($"\nDestinationsplatsen {toSpotNumber} är redan upptagen.");
		return false;
	}

	if (fromSpot.SpotNumber == toSpot.SpotNumber)
	{
		Console.WriteLine("\nFordonet står redan på denna plats. Ingen flytt utförd.");
		return false;
	}


	Vehicle vehicleToMove = null;
	foreach (Vehicle vehicle in fromSpot.ParkedVehicles)
	{
		if (vehicle.RegNum.ToUpper() == regNum.ToUpper())
		{
			vehicleToMove = vehicle;
			break;
		}
	}

	if (vehicleToMove != null)
	{
		fromSpot.ParkedVehicles.Remove(vehicleToMove);
		toSpot.ParkedVehicles.Add(vehicleToMove);

		Console.WriteLine($"\nFordonet {regNum} har flyttats från plats {fromSpot.SpotNumber} till plats {toSpot.SpotNumber}.");
		return true;
	}

	return false;
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

