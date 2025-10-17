using System.Collections.Generic;

namespace PragueParkingV2.Core
{
	public class ParkingGarage
	{
		// En egenskap som håller en lista av ParkingSpot-objekt.
		public List<ParkingSpot> Spots { get; set; } = new List<ParkingSpot>();
	}

}