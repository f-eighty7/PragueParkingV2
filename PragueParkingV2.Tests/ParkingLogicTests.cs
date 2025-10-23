using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParkingV2.Core;
using System;
using System.Linq;

namespace PragueParkingV2.Tests
{
	[TestClass]
	public sealed class ParkingLogicTests 
	{
		// --- TEST 1: Kontrollera att en ny plats är tom ---
		[TestMethod]
		public void ParkingSpot_IsInitiallyEmpty_ShouldReturnTrue()
		{
			var spot = new ParkingSpot();

			bool isEmpty = spot.IsEmpty();

			Assert.IsTrue(isEmpty, "En nyskapad parkeringsplats borde vara tom.");
			Assert.AreEqual(0, spot.ParkedVehicles.Count, "Antalet fordon borde vara 0.");
		}

		// --- TEST 2: Kontrollera att platsen inte är tom efter att ha lagt till ett fordon ---
		[TestMethod]
		public void ParkingSpot_AfterAddingVehicle_ShouldNotBeEmptyAndCountIsOne()
		{
			var spot = new ParkingSpot();
			var car = new Car { RegNum = "TEST123", ArrivalTime = DateTime.Now };

			spot.ParkedVehicles.Add(car);
			bool isEmpty = spot.IsEmpty();
			int vehicleCount = spot.ParkedVehicles.Count;

			Assert.IsFalse(isEmpty, "Platsen borde inte vara tom efter att ett fordon lagts till.");
			Assert.AreEqual(1, vehicleCount, "Antalet fordon borde vara 1 efter att ett lagts till.");
			Assert.IsTrue(spot.ParkedVehicles.Contains(car), "Listan borde innehålla det tillagda fordonet.");
		}
	}
}