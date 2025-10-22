namespace PragueParkingV2.Core
{
	public class Config
	{
		public int GarageSize { get; set; } = 100;

		public List<VehicleTypeConfig> AllowedVehicleTypes { get; set; } = new List<VehicleTypeConfig>();
	}
}