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
	
}