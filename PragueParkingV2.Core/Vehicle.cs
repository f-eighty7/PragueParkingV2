using System.Text.Json.Serialization;

namespace PragueParkingV2.Core
{
	// 1. Gör klassen 'abstract'
	// 2. Låt klassen implementera det nya interfacet 'IVehicle'
	public abstract class Vehicle : IVehicle
	{
		public string RegNum { get; set; } = string.Empty;
		public DateTime ArrivalTime { get; set; }

		// Denna uppfyller det sista kravet från IVehicle.
		// Gör den 'abstract' för att tvinga alla ärvande klasser
		// (som Car, MC, Bus, Bike) att själva definiera sin storlek.
		public abstract int Size { get; }
	}
}