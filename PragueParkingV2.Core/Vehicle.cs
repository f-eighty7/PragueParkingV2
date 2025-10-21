using System.Text.Json.Serialization; //

namespace PragueParkingV2.Core
{
	[JsonDerivedType(typeof(Car), typeDiscriminator: "car")]
	[JsonDerivedType(typeof(MC), typeDiscriminator: "mc")]
	public class Vehicle
	{
		// Egenskap 1: Alla fordon har ett reg-nummer.
		public string RegNum { get; set; }

		// Egenskap 2: Alla fordon har en ankomsttid.
		public DateTime ArrivalTime { get; set; }
	}
}