namespace PragueParkingV2.Core
{
	public class Config
	{
		public int GarageSize { get; set; } = 100;

		// NYTT FÖR VG:
		// Definierar kapaciteten för en enskild parkeringsruta.
		// Enligt specifikationen är en standardruta storlek 4.
		public int ParkingSpotSize { get; set; } = 4;

		public List<VehicleTypeConfig> AllowedVehicleTypes { get; set; } = new List<VehicleTypeConfig>();
	}
}