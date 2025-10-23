### Loggbok - Prague Parking V2

**Datum:** 2025-10-15

**Aktivitet:** Forskning och planering för omstrukturering till Objektorienterad Programmering (OOP).

**Problem/Fundering:**
Projekt V2 kräver en total övergång från den sträng-baserade logiken i V1 till en objektorienterad modell. Jag behöver förstå de grundläggande principerna innan jag kan börja koda. Mina initiala frågeställningar är:
* Vad är den praktiska skillnaden mellan en klass och ett objekt? Hur relaterar de till varandra?
* Vad betyder syntaxen `public string Egenskap { get; set; }` och varför används den istället för en vanlig variabel?
* Hur fungerar `List<T>`? Specifikt, vad är syftet med `<T>` (t.ex. `<ParkingSpot>`) och hur skiljer det sig från en vanlig array?
* Hur skapas och kopplas de olika objekten (`ParkingGarage`, `ParkingSpot`) samman i praktiken? Jag förstår inte hur en behållarklass kan "innehålla" andra objekt.

**Lösning/Slutsats (Min undersökning):**
Efter att ha undersökt C# och OOP-principer har jag kommit fram till följande plan och förståelse:

1.  **Klasser vs. Objekt:** En **klass** är enbart en ritning som definierar hur något ska se ut. Ett **objekt** är den faktiska, konkreta saken som byggs från ritningen. Jag kan skapa många unika objekt från en och samma klass.
2.  **Properties (`get; set;`):** Detta är det moderna sättet att hantera en klass variabler. `get` är mekanismen för att läsa värdet, och `set` är för att ändra det. Detta ger mig kontroll och möjlighet att lägga till valideringslogik på ett centralt ställe i framtiden. Uttrycket `... = new List<Vehicle>();` är ett modernt sätt att både deklarera och ge ett startvärde, vilket garanterar att listan aldrig är `null`. `.Count` är en inbyggd egenskap i `List`.
3.  **Projektstruktur:** Anledningen till att separera projekt är **"Separation of Concerns"**.
    * **Klassbiblioteket (`.Core`)** blir min "verktygslåda" med all återanvändbar logik och alla klassdefinitioner.
    * **Konsolappen (`.UI`)** blir min "verkstad" som använder verktygen för att bygga det specifika programmet.
    * Detta gör att jag kan återanvända min `.Core`-logik för VG-versionen utan att kopiera kod.
4.  **`List<T>`:** Detta är en dynamisk samling. `<T>` (den generiska typparametern) ger **typsäkerhet** genom att tala om för listan exakt vilken typ av objekt den får innehålla, vilket förhindrar buggar. Jag kommer att använda `.Count` för att se antalet objekt och `.Add()` för att lägga till nya.
5.  **Objekt och Referenser:** Ett objekt (t.ex. ett `ParkingGarage`) innehåller inte andra objekt direkt. Istället innehåller dess lista (`List<ParkingSpot>`) **referenser** (pekare) till de andra objekten, som finns på andra platser i minnet. Att specificera `newSpot.SpotNumber` är nödvändigt för att tala om **vilket specifikt objekt** jag vill ändra på.

Med denna grundförståelse är jag nu redo att börja implementera den grundläggande strukturen.

---

**Datum:** 2025-10-15

**Aktivitet:** Implementation av grundläggande OOP-struktur och initiering av parkeringshuset.

**Problem/Fundering:**
Hur omsätter jag den teoretiska planen till fungerande kod? Vad är den korrekta syntaxen för att skapa objekt och koppla dem till varandra i en loop?

**Lösning/Slutsats (Genomförande):**
Baserat på min forskning har jag nu implementerat grundstommen för programmet:
1.  En Solution har skapats med ett klassbibliotek (`PragueParking2.Core`) och en konsolapp (`PragueParking2.UI`). En referens har lagts till från UI till Core.
2.  De grundläggande klassfilerna (`Vehicle.cs`, `Car.cs`, `MC.cs`, `ParkingSpot.cs`, `ParkingGarage.cs`) har skapats i `.Core`-projektet och fyllts med sina grundläggande egenskaper.
3.  I `Program.cs` har jag skapat den centrala datastrukturen. Processen var:
    * Först skapades ett huvudobjekt: `ParkingGarage garage = new ParkingGarage();`. Detta skapar behållaren för hela parkeringshuset.
    * Därefter implementerades en `for`-loop som kör 100 gånger för att fylla behållaren.
    * Inuti loopen skapas ett nytt, unikt objekt för varje plats: `ParkingSpot newSpot = new ParkingSpot();`.
    * Varje nytt `ParkingSpot`-objekt tilldelas sitt korrekta, användarvänliga nummer: `newSpot.SpotNumber = i + 1;`.
    * Slutligen kopplades objekten samman genom att lägga till det nya `newSpot`-objektet i huvudobjektets lista: `garage.Spots.Add(newSpot);`.

Resultatet är en komplett, objektorienterad representation av ett tomt parkeringshus. Hela den gamla `string[]`-arrayen är nu ersatt av detta enda, kraftfulla `garage`-objekt, och grunden för V2 är lagd.

---

**Datum:** 2025-10-15

**Aktivitet:** Planering av implementationen för `AddVehicle`-metoden med den nya OOP-modellen.

**Problem/Fundering:**
Nu när grundstrukturen med klasser och ett tomt `garage`-objekt är på plats, hur översätter jag den gamla, sträng-baserade `AddVehicle`-logiken till det nya objektorienterade tänket? Hur hittar jag en tom plats, skapar ett fordonsobjekt och placerar det i rätt `ParkingSpot`? Hur hanterar jag specialfallet för motorcyklar?

**Lösning/Slutsats (Plan):**
Logiken kommer att översättas steg för steg. En ny metod `AddVehicle` ska skapas i `Program.cs`.
1.  **Hitta platsen:** Istället för att leta efter en tom sträng, ska jag nu loopa igenom `garage.Spots` och leta efter ett `ParkingSpot`-objekt där `ParkedVehicles.Count == 0` (för tom plats) eller där `Count == 1` och fordonet `is MC` (för delbar MC-plats). En `foreach`-loop verkar lämplig.
2.  **Skapa fordonet:** Istället för att bygga en sträng, ska jag skapa ett faktiskt objekt (`Car newCar = new Car();` eller `MC newMC = new MC();`).
3.  **Fyll i information:** Egenskaperna `RegNum` och `ArrivalTime = DateTime.Now;` ska sättas på det nya objektet.
4.  **Parkera fordonet:** Det nya fordonsobjektet ska läggas till i den hittade platsens `ParkedVehicles`-lista med `.Add()`.
5.  **MC-Logik:** För MC ska jag först söka efter en delbar plats. Om ingen sådan hittas, faller logiken tillbaka till att söka efter en helt tom plats. En `bool`-flagga kan användas för att hålla reda på om MC:n redan har parkerats.

---

**Datum:** 2025-10-16

**Aktivitet:** Planering av implementationen för en återanvändbar sökfunktion för att hitta fordon.

**Problem/Fundering:**
För att kunna implementera funktionerna för att ta bort, flytta och söka efter fordon, behöver jag ett pålitligt sätt att hitta var ett specifikt fordon är parkerat. I V1 returnerade min hjälpmetod ett `int` (index). Hur bör en modern, objektorienterad sökmetod fungera?

**Lösning/Slutsats (Plan):**
Istället för att bara returnera ett indexnummer, är det mycket kraftfullare att skapa en hjälpmetod som returnerar **hela `ParkingSpot`-objektet** där fordonet hittades.

1.  **Ny Hjälpmetod:** Jag ska skapa en ny metod, `ParkingSpot FindSpotByRegNum(string regNum)`.
2.  **Söklogik (Nästlade loopar):** Metoden måste använda två `foreach`-loopar: en yttre som går igenom varje `ParkingSpot` och en inre som går igenom varje `Vehicle` på den platsen.
3.  **Jämförelse:** Inuti den inre loopen ska jag göra en exakt, skiftlägesokänslig jämförelse av `vehicle.RegNum` mot `regNum`.
4.  **Returvärde:** Om en matchning hittas, returneras `ParkingSpot`-objektet (`return spot;`). Om ingen matchning hittas efter att alla platser och fordon har kontrollerats, returneras `null`.

Denna design ger direkt tillgång till det relevanta `ParkingSpot`-objektet, vilket förenklar implementationen av `RemoveVehicle` och `MoveVehicle`.

---

**Datum:** 2025-10-16

**Aktivitet:** Implementation av den användarvända sökfunktionen (`SearchVehicleUI`).

**Problem/Fundering:**
Nu när jag har en kraftfull hjälpmetod, `FindSpotByRegNum`, hur använder jag den för att bygga den kompletta sökfunktionen som anropas från menyn? Hur ska jag hantera resultatet, som antingen är ett `ParkingSpot`-objekt eller `null`?

**Lösning/Slutsats (Plan):**
En ny metod `void SearchVehicleUI(ParkingGarage garage)` skapas. Dess ansvar är att:
1.  Fråga användaren efter ett registreringsnummer.
2.  Anropa `FindSpotByRegNum` med det inmatade numret.
3.  Använda en `if`-sats för att kontrollera resultatet:
    * Om `!= null`: Skriv ut platsnumret (`foundSpot.SpotNumber`) och information om fordonen på platsen.
    * Om `== null`: Skriv ut att fordonet inte hittades.
4.  Denna metod ska sedan kopplas till ett menyval.

---

**Datum:** 2025-10-16

**Aktivitet:** Planering av implementationen för funktionen "Ta bort fordon" (`RemoveVehicle`).

**Problem/Fundering:**
Hur översätter jag den gamla `RemoveVehicle`-logiken till den nya OOP-modellen, särskilt hanteringen av delade MC-platser? Hur kan jag utnyttja objektstrukturen och `FindSpotByRegNum`?

**Lösning/Slutsats (Plan):**
Ansvaret delas igen: en UI-metod och en logik-metod.
1.  **`void RemoveVehicleUI(...)`:** Frågar efter reg-nr.
2.  **`bool RemoveVehicle(...)`:** Ska innehålla logiken:
    * Anropa `FindSpotByRegNum`. Om `null`, returnera `false`.
    * Om plats hittas, loopa igenom `spot.ParkedVehicles` för att hitta det exakta `Vehicle`-objektet med matchande `RegNum`.
    * Om objektet hittas, använd `spot.ParkedVehicles.Remove(vehicleToRemove);`. Listans inbyggda `Remove` hanterar automatiskt både enskilda och delade fall. Skriv ut bekräftelse och returnera `true`.
    * Om objektet av någon anledning inte hittas i listan (bör inte hända), returnera `false`.

---

**Datum:** 2025-10-16

**Aktivitet:** Planering av implementationen för funktionen "Flytta fordon" (`MoveVehicle`).

**Problem/Fundering:**
Hur ska `MoveVehicle`-funktionen implementeras i OOP-modellen? Den gamla metoden var komplex på grund av stränghanteringen för delade platser.

**Lösning/Slutsats (Plan):**
Samma uppdelning som för `Remove`.
1.  **`void MoveVehicleUI(...)`:** Frågar efter reg-nr och ny plats (`toSpotNumber`).
2.  **`bool MoveVehicle(...)`:** Ska innehålla logiken:
    * Anropa `FindSpotByRegNum` för att få `fromSpot`. Validera (`!= null`).
    * Validera `toSpotNumber` (1-100). Hitta motsvarande `toSpot`-objekt. Validera att `toSpot` är tom (`Count == 0`). Validera att `fromSpot != toSpot`. Om någon validering misslyckas, skriv meddelande och returnera `false`.
    * Om allt är OK: Hitta det exakta `vehicleToMove`-objektet i `fromSpot.ParkedVehicles`.
    * Utför flytten: `fromSpot.ParkedVehicles.Remove(vehicleToMove);` följt av `toSpot.ParkedVehicles.Add(vehicleToMove);`.
    * Skriv bekräftelse och returnera `true`.

---

**Datum:** 2025-10-17

**Aktivitet:** Integrering av `DataAccess`-komponenten med huvudapplikationen (`Program.cs`).

**Problem/Fundering:**
Nu när `DataAccess`-klassen med `SaveGarage` och `LoadGarage` finns i ett separat projekt, hur kopplar jag ihop den med mitt UI-projekt så att programmet faktiskt laddar data vid start och sparar vid ändringar?

**Lösning/Slutsats (Plan):**
Modifiera `Program.cs`:
1.  **Ladda vid Start:** Skapa ett `DataAccess`-objekt. Anropa `dataAccess.LoadGarage()` istället för att skapa ett nytt, tomt garage manuellt. Detta blir huvud-`garage`-objektet.
2.  **Spara vid Ändring:** Efter varje anrop till `AddVehicleUI`, `RemoveVehicleUI`, och `MoveVehicleUI`, anropa `dataAccess.SaveGarage(garage)` för att spara det nya tillståndet till `garage.json`.

---

**Datum:** 2025-10-17

**Aktivitet:** Refaktorering och förbättring av befintlig OOP-kod för ökad robusthet och konsekvens.

**Problem/Fundering:**
Efter att ha implementerat grundfunktionerna och persistens identifierades förbättringsområden:
1.  **Null-säkerhet:** Koden behöver hantera potentiella `null`-värden säkrare.
2.  **Skiftlägeskänslighet:** Jämförelser av reg-nr är känsliga för skiftläge.
3.  **Tomma strängar vid input:** Ogiltig input (tomma strängar) hanteras inte explicit.
4.  **Duplicerad logik:** Kontroll för om fordon redan finns görs inte konsekvent.

**Lösning/Slutsats (Refaktorering):**
Följande förbättringar implementerades:
1.  **Null-säkerhet:** Införande av null-förlåtande (`?.`) och null-sammanfogande (`??`) operatorer samt nullable referenstyper (`?`).
2.  **Skiftlägesokänslig jämförelse:** Använder nu `string.Equals(..., StringComparison.OrdinalIgnoreCase)` för reg-nr jämförelser.
3.  **Validering av Input:** Kontroller med `string.IsNullOrWhiteSpace(regNum)` lades till.
4.  **Kontrollera om fordon finns:** `AddVehicle` anropar nu alltid `FindSpotByRegNum` först.

---

**Datum:** 2025-10-21

**Aktivitet:** Planering för att slutföra G-nivåkraven gällande konfigurationsfilen.

**Problem/Fundering:**
Konfigurationen hanterar bara `GarageSize`. Den måste även definiera fordonstyper och deras kapacitet per plats. `AddVehicle`-logiken är hårdkodad för Bil/MC.

**Lösning/Slutsats (Plan):**
1.  Skapa `VehicleTypeConfig`-klass (`TypeName`, `MaxPerSpot`).
2.  Utöka `Config.cs` med `List<VehicleTypeConfig> AllowedVehicleTypes`.
3.  Uppdatera `config.json` med motsvarande data.
4.  Uppdatera `DataAccess.LoadConfig()` att hantera den nya listan (med standardvärden som fallback).
5.  Refaktorera `AddVehicleUI` att dynamiskt visa typer från config.
6.  Refaktorera `AddVehicle` logik att använda `MaxPerSpot` från config för att hitta platser.

---

**Datum:** 2025-10-21

**Aktivitet:** Planering av implementationen för prislista och kostnadsberäkning.

**Problem/Fundering:**
Programmet måste beräkna och visa kostnad vid utcheckning, baserat på en extern prislista och parkeringstid.

**Lösning/Slutsats (Plan):**
1.  **Filformat:** Skapa `pricelist.txt` med format `TYP:PRIS` och stöd för `#`-kommentarer.
2.  **Utöka `DataAccess`:** Lägg till `Dictionary<string, decimal> LoadPriceList()` som läser och parsar filen (med standardvärden som fallback).
3.  **Ladda vid Start:** Anropa `LoadPriceList()` i `Program.cs`.
4.  **Refaktorera `RemoveVehicle`:**
    * Beräkna `TimeSpan parkedDuration`.
    * Implementera 10 min gratistid.
    * Hämta pris från dictionary baserat på fordonstyp.
    * Beräkna antal påbörjade timmar (`Math.Ceiling`).
    * Beräkna kostnad.
    * Uppdatera utskrift till användaren med tid och kostnad.

---

**Datum:** 2025-10-21

**Aktivitet:** Felsökning av felaktig fordonstyp ("Okänd") efter laddning från JSON-fil.

**Problem/Fundering:**
Vid omladdning från `garage.json` identifieras alla fordon som "Okänd". Typinformationen (Car/MC) verkar förloras.

**Lösning/Slutsats (Analys):**
Problemet är standard JSON-serialiseringens hantering av arv (polymorfism). Den sparar bara basklassens (`Vehicle`) egenskaper. Lösningen är att instruera serialiseraren att inkludera typinformation. Detta görs genom att lägga till `[JsonDerivedType(typeof(Car), ...)]` och `[JsonDerivedType(typeof(MC), ...)]` attribut ovanför `Vehicle`-klassen i `Vehicle.cs` (kräver `using System.Text.Json.Serialization;`). `DataAccess`-koden behöver ingen ändring. Den gamla `garage.json` måste raderas.

---

**Datum:** 2025-10-21

**Aktivitet:** Felsökning och förbättring av användarflödet i undermenyerna.

**Problem/Fundering:**
Vid "Avbryt" i en undermeny krävs en extra tangenttryckning för att återgå till huvudmenyn på grund av en generell paus i huvudloopen.

**Lösning/Slutsats (Implementation):**
Den generella pausen (`Console.ReadKey()`) i slutet av huvudmenyns `while`-loop tas bort. Pausen placeras istället *inuti* varje UI-metod och körs endast efter en slutförd åtgärd. "Avbryt"-valen (via `SelectionPrompt`, tom input i `TextPrompt<string>`, eller `null` från `TextPrompt<int?>`) implementeras med en direkt `return;` i UI-metoderna för att omedelbart återgå till huvudmenyn.

---

**Datum:** 2025-10-22

**Aktivitet:** Planering av implementationen för den visuella kartan över parkeringshuset.

**Problem/Fundering:**
G-kravet specificerar en visuell karta för att se beläggningen. Hur skapas detta med `Spectre.Console`?

**Lösning/Slutsats (Plan):**
1.  **Design:** En grid-layout med färgkodning (Grön/Gul/Röd) baserat på platsens status (ledig/halvfull/full).
2.  **Ny Metod:** `void DisplayGarageMapUI(ParkingGarage garage, Config config)` skapas.
3.  **Logik:** Loopa igenom `garage.Spots`. Avgör status baserat på `Count` och `config.MaxPerSpot`. Använd `AnsiConsole.Markup` för att skriva ut färgade block med platsnummer. Implementera radbrytning (`%`). Lägg till förklaring.
4.  **Menyval:** Lägg till "Visa Karta" i huvudmenyn som anropar den nya metoden.

---

**Datum:** 2025-10-22

**Aktivitet:** Planering för implementation av enhetstester.

**Problem/Fundering:**
G-kravet kräver minst två enhetstester med MSTest. Hur skapas projektet och vad ska testas?

**Lösning/Slutsats (Plan):**
1.  **Skapa Testprojekt:** Ett MSTest-projekt (`PragueParking2.Tests`) läggs till i Solution.
2.  **Referens:** Referens från `Tests` till `Core` läggs till.
3.  **Implementera Tester:** Minst två `[TestMethod]` implementeras för att testa `ParkingSpot`-klassen:
    * Testa att `IsEmpty()` är `true` för en ny plats.
    * Testa att `IsEmpty()` är `false` och `Count` är 1 efter att ett fordon lagts till.
    * Använd `Assert`-metoder för verifiering.