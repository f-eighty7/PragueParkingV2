using System;
using System.Collections.Generic;

namespace PragueParkingV2.Core
{
	public class ParkingGarage
	{
		// ÄNDRING: Listan är nu av typen IParkingSpot
		public List<IParkingSpot> Spots { get; set; } = new List<IParkingSpot>();
	}

}