# 🚗 Prague Parking V2 🅿️

Welcome to Prague Parking V2, a complete, object-oriented (OOP) refactor of the original Prague Parking V1 project.

This project transforms the original single-file, string-array-based application into a robust, multi-layered C# console application. It manages a parking garage, handling different vehicle types, parking logic, persistence, and dynamic configuration.

## ✨ Core Features

* **Vehicle Management:** Park, remove, and move vehicles in the garage.
* **Cost Calculation:** Automatically calculates parking costs based on time parked and vehicle type from a price list.
* **Search Functionality:** Find any vehicle by its registration number or query a specific parking spot.
* **Visual Map:** Displays a color-coded map of the garage for a quick overview of occupancy.
* **Data Persistence:** Garage state is saved to `garage.json` on exit and reloaded on start.
* **External Configuration:** Garage size, rules, and prices are loaded from external `.json` and `.txt` files.
* **Rich UI:** Built with the `Spectre.Console` library for a modern, user-friendly interface.

## 🏗️ Architecture

The solution is split into three distinct projects, following the principle of Separation of Concerns:

* **`PragueParkingV2.Core`**: The domain layer. Contains all POCO classes and models, such as `Vehicle`, `Car`, `ParkingSpot`, and `Config`.
* **`PragueParkingV2.Data`**: The data access layer. Responsible for all file I/O, including serializing/deserializing `garage.json` and reading the config files.
* **`PragueParkingV2.UI`**: The presentation layer. This is the runnable console application that handles all user interaction and application logic.

## 🛠️ Tech Stack

* C# & .NET
* Spectre.Console (for the rich UI)
* System.Text.Json (for data persistence)

## 🚀 Branch Information

This repository contains two primary branches representing different feature levels.

### `main` (G-Level Features)

This branch contains the stable, foundational (G-level) version of the project.

* **Handles `Car` and `MC`** vehicles.
* **Simple Logic:** Uses a `MaxPerSpot` logic (e.g., 1 Car or 2 MCs can share a single spot).
* **Basic Persistence:** Saves and loads the garage state from `garage.json`.

### `vg-requirements` (VG-Level Features)

This branch contains the advanced implementation for the VG grade. It represents a significant refactor of the core logic for maximum flexibility.

* **Advanced Size-Based Logic:** The `MaxPerSpot` logic is completely replaced by a flexible `Size` system.
    * Parking spots have a `ParkingSpotSize` (e.g., capacity 4).
    * All vehicles have a `Size` (e.g., Car: 4, MC: 2, Bike: 1, Bus: 16).
* **New Vehicle Types:** Adds full support for `Bus` and `Bike`.
* **Complex Parking:** Can park large vehicles (like a `Bus`) across multiple consecutive, empty parking spots.
* **Interface-Driven Design:** Uses `IParkingSpot` and inheritance from the `Vehicle` base class (which implements `IVehicle`) to allow for polymorphic handling of all vehicles.
* **Dynamic Configuration:** Features a "Manage Configuration" menu where the user can:
    * Change the garage size and p-spot capacity in real-time.
    * Edit vehicle prices.
    * Save these changes back to `config.json` and `pricelist.txt`.
    * Reload configuration from files, with validation to prevent data loss (e.g., shrinking the garage over parked cars).
* **Automatic Test Data:** If `garage.json` is missing on first launch, it is automatically created and populated with a set of sample vehicles (including Cars, MCs, a Bus, and Bikes).
* **Robust Persistence:** Implements the critical `[JsonDerivedType]` attributes on the `Vehicle` base class, which allows `System.Text.Json` to correctly save and load the polymorphic `List<Vehicle>`. This fixes the bug that caused the garage to be empty on restart.

***

### 🏃 How to Run

1.  Clone the repository.
2.  Open the solution file (`PragueParkingV2.sln`) in Visual Studio.
3.  Set `PragueParkingV2.UI` as the startup project.
4.  **(Optional) To run the advanced version, switch to the `vg-requirements` branch (see guide below).**
5.  Run the project (F5).

### 👇 How to Switch Branches in Visual Studio

To test the advanced VG-level features, you need to switch from the `main` branch to the `vg-requirements` branch.

1.  Open the `PragueParkingV2.sln` in Visual Studio.
2.  Look in the **bottom-right corner** of the Visual Studio window. You will see the name of the current branch (e.g., `main`) next to a branch icon (🍴).
3.  Click on the branch name.
4.  A Git menu will pop up (similar to the screenshot `image_d8d89e.png`).
5.  In this menu, find **`vg-requirements`** in the list (it might be under "Local" or "Remotes/origin").
6.  Click on `vg-requirements` to check it out.
7.  Visual Studio will reload the files in the Solution Explorer, and you will now be running the advanced version of the project.