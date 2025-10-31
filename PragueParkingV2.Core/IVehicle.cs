namespace PragueParkingV2.Core
{
	public interface IVehicle
	{
		// Alla fordon måste ha ett registreringsnummer
		string RegNum { get; set; }

		// Alla fordon måste ha en ankomsttid
		DateTime ArrivalTime { get; set; }

		// Alla fordon måste ha en storlek
		int Size { get; }
	}
}