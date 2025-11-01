namespace PragueParkingV2.Core
{
	public class ParkingSpot : IParkingSpot
	{
		public int SpotNumber { get; set; }

		public List<Vehicle> ParkedVehicles { get; set; } = new List<Vehicle>();

		// Den räknar automatiskt ut hur mycket plats som är upptagen
		// på just den här P-rutan.
		public int OccupiedSpace
		{
			get
			{
				// Använder LINQ (.Sum) för att summera 'Size'-egenskapen
				// för varje IVehicle-objekt i ParkedVehicles-listan.
				// Om listan är tom (ParkedVehicles.Count == 0) blir summan 0.
				return ParkedVehicles.Sum(v => v.Size);
			}
		}

		// ÄNDRING 3:
		// Den gamla metoden 'IsEmpty()' är nu borttagen.
		// All annan kod bör istället kontrollera:
		// if (spot.OccupiedSpace == 0)
	}
}