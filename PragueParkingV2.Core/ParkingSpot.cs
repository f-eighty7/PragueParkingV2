namespace PragueParkingV2.Core
{
	public class ParkingSpot
	{
		public int SpotNumber { get; set; }
		public List<Vehicle> ParkedVehicles { get; set; } = new List<Vehicle>();

		// En metod som tillhör klassen ParkingSpot
		public bool IsEmpty()
		{
			if (ParkedVehicles.Count == 0) return true;
			else return false;
		}
	}
}