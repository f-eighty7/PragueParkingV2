namespace PragueParkingV2.Core
{
	public class VehicleTypeConfig
	{
		public string TypeName { get; set; } = string.Empty;

		// BORTTAGEN FÖR VG:
		// public int MaxPerSpot { get; set; }
		// Denna logik ersätts nu helt av IVehicle.Size och Config.ParkingSpotSize
	}
}