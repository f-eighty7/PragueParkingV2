namespace PragueParkingV2.Core
{
	// Detta är "kontraktet" för vad en parkeringsplats MÅSTE ha.
	public interface IParkingSpot
	{
		// Alla platser måste ha ett nummer
		int SpotNumber { get; set; }

		// Alla platser måste ha en lista av fordon (som använder det andra interfacet)
		List<IVehicle> ParkedVehicles { get; set; }

		// Alla platser måste kunna rapportera sin beläggning
		int OccupiedSpace { get; }
	}
}