### Loggbok - Prague Parking V2

**Datum:** 2025-10-15

**Aktivitet:** Forskning och planering f�r omstrukturering till Objektorienterad Programmering (OOP).

**Problem/Fundering:**
Projekt V2 kr�ver en total �verg�ng fr�n den str�ng-baserade logiken i V1 till en objektorienterad modell. Jag beh�ver f�rst� de grundl�ggande principerna innan jag kan b�rja koda. Mina initiala fr�gest�llningar �r:
* Vad �r den praktiska skillnaden mellan en klass och ett objekt? Hur relaterar de till varandra?
* Vad betyder syntaxen `public string Egenskap { get; set; }` och varf�r anv�nds den ist�llet f�r en vanlig variabel?
* Hur fungerar `List<T>`? Specifikt, vad �r syftet med `<T>` (t.ex. `<ParkingSpot>`) och hur skiljer det sig fr�n en vanlig array?
* Hur skapas och kopplas de olika objekten (`ParkingGarage`, `ParkingSpot`) samman i praktiken? Jag f�rst�r inte hur en beh�llarklass kan "inneh�lla" andra objekt.

**L�sning/Slutsats (Min unders�kning):**
Efter att ha unders�kt C# och OOP-principer har jag kommit fram till f�ljande plan och f�rst�else:

1.  **Klasser vs. Objekt:** En **klass** �r enbart en ritning som definierar hur n�got ska se ut. Ett **objekt** �r den faktiska, konkreta saken som byggs fr�n ritningen. Jag kan skapa m�nga unika objekt fr�n en och samma klass.
2.  **Properties (`get; set;`):** Detta �r det moderna s�ttet att hantera en klass variabler. `get` �r mekanismen f�r att l�sa v�rdet, och `set` �r f�r att �ndra det. Detta ger mig kontroll och m�jlighet att l�gga till valideringslogik p� ett centralt st�lle i framtiden. Uttrycket `... = new List<Vehicle>();` �r ett modernt s�tt att b�de deklarera och ge ett startv�rde, vilket garanterar att listan aldrig �r `null`. `.Count` �r en inbyggd egenskap i `List`.
3.  **Projektstruktur:** Anledningen till att separera projekt �r **"Separation of Concerns"**.
    * **Klassbiblioteket (`.Core`)** blir min "verktygsl�da" med all �teranv�ndbar logik och alla klassdefinitioner.
    * **Konsolappen (`.UI`)** blir min "verkstad" som anv�nder verktygen f�r att bygga det specifika programmet.
    * Detta g�r att jag kan �teranv�nda min `.Core`-logik f�r VG-versionen utan att kopiera kod.
4.  **`List<T>`:** Detta �r en dynamisk samling. `<T>` (den generiska typparametern) ger **typs�kerhet** genom att tala om f�r listan exakt vilken typ av objekt den f�r inneh�lla, vilket f�rhindrar buggar. Jag kommer att anv�nda `.Count` f�r att se antalet objekt och `.Add()` f�r att l�gga till nya.
5.  **Objekt och Referenser:** Ett objekt (t.ex. ett `ParkingGarage`) inneh�ller inte andra objekt direkt. Ist�llet inneh�ller dess lista (`List<ParkingSpot>`) **referenser** (pekare) till de andra objekten, som finns p� andra platser i minnet. Att specificera `newSpot.SpotNumber` �r n�dv�ndigt f�r att tala om **vilket specifikt objekt** jag vill �ndra p�.

Med denna grundf�rst�else �r jag nu redo att b�rja implementera den grundl�ggande strukturen.

---

**Datum:** 2025-10-15

**Aktivitet:** Implementation av grundl�ggande OOP-struktur och initiering av parkeringshuset.

**Problem/Fundering:**
Hur oms�tter jag den teoretiska planen till fungerande kod? Vad �r den korrekta syntaxen f�r att skapa objekt och koppla dem till varandra i en loop?

**L�sning/Slutsats (Genomf�rande):**
Baserat p� min forskning har jag nu implementerat grundstommen f�r programmet:
1.  En Solution har skapats med ett klassbibliotek (`PragueParking2.Core`) och en konsolapp (`PragueParking2.UI`). En referens har lagts till fr�n UI till Core.
2.  De grundl�ggande klassfilerna (`Vehicle.cs`, `Car.cs`, `MC.cs`, `ParkingSpot.cs`, `ParkingGarage.cs`) har skapats i `.Core`-projektet och fyllts med sina grundl�ggande egenskaper.
3.  I `Program.cs` har jag skapat den centrala datastrukturen. Processen var:
    * F�rst skapades ett huvudobjekt: `ParkingGarage garage = new ParkingGarage();`. Detta skapar beh�llaren f�r hela parkeringshuset.
    * D�refter implementerades en `for`-loop som k�r 100 g�nger f�r att fylla beh�llaren.
    * Inuti loopen skapas ett nytt, unikt objekt f�r varje plats: `ParkingSpot newSpot = new ParkingSpot();`.
    * Varje nytt `ParkingSpot`-objekt tilldelas sitt korrekta, anv�ndarv�nliga nummer: `newSpot.SpotNumber = i + 1;`.
    * Slutligen kopplades objekten samman genom att l�gga till det nya `newSpot`-objektet i huvudobjektets lista: `garage.Spots.Add(newSpot);`.

Resultatet �r en komplett, objektorienterad representation av ett tomt parkeringshus. Hela den gamla `string[]`-arrayen �r nu ersatt av detta enda, kraftfulla `garage`-objekt, och grunden f�r V2 �r lagd.

---

**Datum:** 2025-10-15

**Aktivitet:** Planering av implementationen f�r `AddVehicle`-metoden med den nya OOP-modellen.

**Problem/Fundering:**
Nu n�r grundstrukturen med klasser och ett tomt `garage`-objekt �r p� plats, hur �vers�tter jag den gamla, str�ng-baserade `AddVehicle`-logiken till det nya objektorienterade t�nket? Hur hittar jag en tom plats, skapar ett fordonsobjekt och placerar det i r�tt `ParkingSpot`? Hur hanterar jag specialfallet f�r motorcyklar?

**L�sning/Slutsats (Plan):**
Logiken kommer att �vers�ttas steg f�r steg. En ny metod `AddVehicle` ska skapas i `Program.cs`.
1.  **Hitta platsen:** Ist�llet f�r att leta efter en tom str�ng, ska jag nu loopa igenom `garage.Spots` och leta efter ett `ParkingSpot`-objekt d�r `ParkedVehicles.Count == 0` (f�r tom plats) eller d�r `Count == 1` och fordonet `is MC` (f�r delbar MC-plats). En `foreach`-loop verkar l�mplig.
2.  **Skapa fordonet:** Ist�llet f�r att bygga en str�ng, ska jag skapa ett faktiskt objekt (`Car newCar = new Car();` eller `MC newMC = new MC();`).
3.  **Fyll i information:** Egenskaperna `RegNum` och `ArrivalTime = DateTime.Now;` ska s�ttas p� det nya objektet.
4.  **Parkera fordonet:** Det nya fordonsobjektet ska l�ggas till i den hittade platsens `ParkedVehicles`-lista med `.Add()`.
5.  **MC-Logik:** F�r MC ska jag f�rst s�ka efter en delbar plats. Om ingen s�dan hittas, faller logiken tillbaka till att s�ka efter en helt tom plats. En `bool`-flagga kan anv�ndas f�r att h�lla reda p� om MC:n redan har parkerats.

---

**Datum:** 2025-10-16

**Aktivitet:** Planering av implementationen f�r en �teranv�ndbar s�kfunktion f�r att hitta fordon.

**Problem/Fundering:**
F�r att kunna implementera funktionerna f�r att ta bort, flytta och s�ka efter fordon, beh�ver jag ett p�litligt s�tt att hitta var ett specifikt fordon �r parkerat. I V1 returnerade min hj�lpmetod ett `int` (index). Hur b�r en modern, objektorienterad s�kmetod fungera?

**L�sning/Slutsats (Plan):**
Ist�llet f�r att bara returnera ett indexnummer, �r det mycket kraftfullare att skapa en hj�lpmetod som returnerar **hela `ParkingSpot`-objektet** d�r fordonet hittades.

1.  **Ny Hj�lpmetod:** Jag ska skapa en ny metod, `ParkingSpot FindSpotByRegNum(string regNum)`.
2.  **S�klogik (N�stlade loopar):** Metoden m�ste anv�nda tv� `foreach`-loopar: en yttre som g�r igenom varje `ParkingSpot` och en inre som g�r igenom varje `Vehicle` p� den platsen.
3.  **J�mf�relse:** Inuti den inre loopen ska jag g�ra en exakt, skiftl�gesok�nslig j�mf�relse av `vehicle.RegNum` mot `regNum`.
4.  **Returv�rde:** Om en matchning hittas, returneras `ParkingSpot`-objektet (`return spot;`). Om ingen matchning hittas efter att alla platser och fordon har kontrollerats, returneras `null`.

Denna design ger direkt tillg�ng till det relevanta `ParkingSpot`-objektet, vilket f�renklar implementationen av `RemoveVehicle` och `MoveVehicle`.

---

**Datum:** 2025-10-16

**Aktivitet:** Implementation av den anv�ndarv�nda s�kfunktionen (`SearchVehicleUI`).

**Problem/Fundering:**
Nu n�r jag har en kraftfull hj�lpmetod, `FindSpotByRegNum`, hur anv�nder jag den f�r att bygga den kompletta s�kfunktionen som anropas fr�n menyn? Hur ska jag hantera resultatet, som antingen �r ett `ParkingSpot`-objekt eller `null`?

**L�sning/Slutsats (Plan):**
En ny metod `void SearchVehicleUI(ParkingGarage garage)` skapas. Dess ansvar �r att:
1.  Fr�ga anv�ndaren efter ett registreringsnummer.
2.  Anropa `FindSpotByRegNum` med det inmatade numret.
3.  Anv�nda en `if`-sats f�r att kontrollera resultatet:
    * Om `!= null`: Skriv ut platsnumret (`foundSpot.SpotNumber`) och information om fordonen p� platsen.
    * Om `== null`: Skriv ut att fordonet inte hittades.
4.  Denna metod ska sedan kopplas till ett menyval.

---

**Datum:** 2025-10-16

**Aktivitet:** Planering av implementationen f�r funktionen "Ta bort fordon" (`RemoveVehicle`).

**Problem/Fundering:**
Hur �vers�tter jag den gamla `RemoveVehicle`-logiken till den nya OOP-modellen, s�rskilt hanteringen av delade MC-platser? Hur kan jag utnyttja objektstrukturen och `FindSpotByRegNum`?

**L�sning/Slutsats (Plan):**
Ansvaret delas igen: en UI-metod och en logik-metod.
1.  **`void RemoveVehicleUI(...)`:** Fr�gar efter reg-nr.
2.  **`bool RemoveVehicle(...)`:** Ska inneh�lla logiken:
    * Anropa `FindSpotByRegNum`. Om `null`, returnera `false`.
    * Om plats hittas, loopa igenom `spot.ParkedVehicles` f�r att hitta det exakta `Vehicle`-objektet med matchande `RegNum`.
    * Om objektet hittas, anv�nd `spot.ParkedVehicles.Remove(vehicleToRemove);`. Listans inbyggda `Remove` hanterar automatiskt b�de enskilda och delade fall. Skriv ut bekr�ftelse och returnera `true`.
    * Om objektet av n�gon anledning inte hittas i listan (b�r inte h�nda), returnera `false`.

---

**Datum:** 2025-10-16

**Aktivitet:** Planering av implementationen f�r funktionen "Flytta fordon" (`MoveVehicle`).

**Problem/Fundering:**
Hur ska `MoveVehicle`-funktionen implementeras i OOP-modellen? Den gamla metoden var komplex p� grund av str�nghanteringen f�r delade platser.

**L�sning/Slutsats (Plan):**
Samma uppdelning som f�r `Remove`.
1.  **`void MoveVehicleUI(...)`:** Fr�gar efter reg-nr och ny plats (`toSpotNumber`).
2.  **`bool MoveVehicle(...)`:** Ska inneh�lla logiken:
    * Anropa `FindSpotByRegNum` f�r att f� `fromSpot`. Validera (`!= null`).
    * Validera `toSpotNumber` (1-100). Hitta motsvarande `toSpot`-objekt. Validera att `toSpot` �r tom (`Count == 0`). Validera att `fromSpot != toSpot`. Om n�gon validering misslyckas, skriv meddelande och returnera `false`.
    * Om allt �r OK: Hitta det exakta `vehicleToMove`-objektet i `fromSpot.ParkedVehicles`.
    * Utf�r flytten: `fromSpot.ParkedVehicles.Remove(vehicleToMove);` f�ljt av `toSpot.ParkedVehicles.Add(vehicleToMove);`.
    * Skriv bekr�ftelse och returnera `true`.

---

**Datum:** 2025-10-17

**Aktivitet:** Integrering av `DataAccess`-komponenten med huvudapplikationen (`Program.cs`).

**Problem/Fundering:**
Nu n�r `DataAccess`-klassen med `SaveGarage` och `LoadGarage` finns i ett separat projekt, hur kopplar jag ihop den med mitt UI-projekt s� att programmet faktiskt laddar data vid start och sparar vid �ndringar?

**L�sning/Slutsats (Plan):**
Modifiera `Program.cs`:
1.  **Ladda vid Start:** Skapa ett `DataAccess`-objekt. Anropa `dataAccess.LoadGarage()` ist�llet f�r att skapa ett nytt, tomt garage manuellt. Detta blir huvud-`garage`-objektet.
2.  **Spara vid �ndring:** Efter varje anrop till `AddVehicleUI`, `RemoveVehicleUI`, och `MoveVehicleUI`, anropa `dataAccess.SaveGarage(garage)` f�r att spara det nya tillst�ndet till `garage.json`.

---

**Datum:** 2025-10-17

**Aktivitet:** Refaktorering och f�rb�ttring av befintlig OOP-kod f�r �kad robusthet och konsekvens.

**Problem/Fundering:**
Efter att ha implementerat grundfunktionerna och persistens identifierades f�rb�ttringsomr�den:
1.  **Null-s�kerhet:** Koden beh�ver hantera potentiella `null`-v�rden s�krare.
2.  **Skiftl�gesk�nslighet:** J�mf�relser av reg-nr �r k�nsliga f�r skiftl�ge.
3.  **Tomma str�ngar vid input:** Ogiltig input (tomma str�ngar) hanteras inte explicit.
4.  **Duplicerad logik:** Kontroll f�r om fordon redan finns g�rs inte konsekvent.

**L�sning/Slutsats (Refaktorering):**
F�ljande f�rb�ttringar implementerades:
1.  **Null-s�kerhet:** Inf�rande av null-f�rl�tande (`?.`) och null-sammanfogande (`??`) operatorer samt nullable referenstyper (`?`).
2.  **Skiftl�gesok�nslig j�mf�relse:** Anv�nder nu `string.Equals(..., StringComparison.OrdinalIgnoreCase)` f�r reg-nr j�mf�relser.
3.  **Validering av Input:** Kontroller med `string.IsNullOrWhiteSpace(regNum)` lades till.
4.  **Kontrollera om fordon finns:** `AddVehicle` anropar nu alltid `FindSpotByRegNum` f�rst.

---

**Datum:** 2025-10-21

**Aktivitet:** Planering f�r att slutf�ra G-niv�kraven g�llande konfigurationsfilen.

**Problem/Fundering:**
Konfigurationen hanterar bara `GarageSize`. Den m�ste �ven definiera fordonstyper och deras kapacitet per plats. `AddVehicle`-logiken �r h�rdkodad f�r Bil/MC.

**L�sning/Slutsats (Plan):**
1.  Skapa `VehicleTypeConfig`-klass (`TypeName`, `MaxPerSpot`).
2.  Ut�ka `Config.cs` med `List<VehicleTypeConfig> AllowedVehicleTypes`.
3.  Uppdatera `config.json` med motsvarande data.
4.  Uppdatera `DataAccess.LoadConfig()` att hantera den nya listan (med standardv�rden som fallback).
5.  Refaktorera `AddVehicleUI` att dynamiskt visa typer fr�n config.
6.  Refaktorera `AddVehicle` logik att anv�nda `MaxPerSpot` fr�n config f�r att hitta platser.

---

**Datum:** 2025-10-21

**Aktivitet:** Planering av implementationen f�r prislista och kostnadsber�kning.

**Problem/Fundering:**
Programmet m�ste ber�kna och visa kostnad vid utcheckning, baserat p� en extern prislista och parkeringstid.

**L�sning/Slutsats (Plan):**
1.  **Filformat:** Skapa `pricelist.txt` med format `TYP:PRIS` och st�d f�r `#`-kommentarer.
2.  **Ut�ka `DataAccess`:** L�gg till `Dictionary<string, decimal> LoadPriceList()` som l�ser och parsar filen (med standardv�rden som fallback).
3.  **Ladda vid Start:** Anropa `LoadPriceList()` i `Program.cs`.
4.  **Refaktorera `RemoveVehicle`:**
    * Ber�kna `TimeSpan parkedDuration`.
    * Implementera 10 min gratistid.
    * H�mta pris fr�n dictionary baserat p� fordonstyp.
    * Ber�kna antal p�b�rjade timmar (`Math.Ceiling`).
    * Ber�kna kostnad.
    * Uppdatera utskrift till anv�ndaren med tid och kostnad.

---

**Datum:** 2025-10-21

**Aktivitet:** Fels�kning av felaktig fordonstyp ("Ok�nd") efter laddning fr�n JSON-fil.

**Problem/Fundering:**
Vid omladdning fr�n `garage.json` identifieras alla fordon som "Ok�nd". Typinformationen (Car/MC) verkar f�rloras.

**L�sning/Slutsats (Analys):**
Problemet �r standard JSON-serialiseringens hantering av arv (polymorfism). Den sparar bara basklassens (`Vehicle`) egenskaper. L�sningen �r att instruera serialiseraren att inkludera typinformation. Detta g�rs genom att l�gga till `[JsonDerivedType(typeof(Car), ...)]` och `[JsonDerivedType(typeof(MC), ...)]` attribut ovanf�r `Vehicle`-klassen i `Vehicle.cs` (kr�ver `using System.Text.Json.Serialization;`). `DataAccess`-koden beh�ver ingen �ndring. Den gamla `garage.json` m�ste raderas.

---

**Datum:** 2025-10-21

**Aktivitet:** Fels�kning och f�rb�ttring av anv�ndarfl�det i undermenyerna.

**Problem/Fundering:**
Vid "Avbryt" i en undermeny kr�vs en extra tangenttryckning f�r att �terg� till huvudmenyn p� grund av en generell paus i huvudloopen.

**L�sning/Slutsats (Implementation):**
Den generella pausen (`Console.ReadKey()`) i slutet av huvudmenyns `while`-loop tas bort. Pausen placeras ist�llet *inuti* varje UI-metod och k�rs endast efter en slutf�rd �tg�rd. "Avbryt"-valen (via `SelectionPrompt`, tom input i `TextPrompt<string>`, eller `null` fr�n `TextPrompt<int?>`) implementeras med en direkt `return;` i UI-metoderna f�r att omedelbart �terg� till huvudmenyn.

---

**Datum:** 2025-10-22

**Aktivitet:** Planering av implementationen f�r den visuella kartan �ver parkeringshuset.

**Problem/Fundering:**
G-kravet specificerar en visuell karta f�r att se bel�ggningen. Hur skapas detta med `Spectre.Console`?

**L�sning/Slutsats (Plan):**
1.  **Design:** En grid-layout med f�rgkodning (Gr�n/Gul/R�d) baserat p� platsens status (ledig/halvfull/full).
2.  **Ny Metod:** `void DisplayGarageMapUI(ParkingGarage garage, Config config)` skapas.
3.  **Logik:** Loopa igenom `garage.Spots`. Avg�r status baserat p� `Count` och `config.MaxPerSpot`. Anv�nd `AnsiConsole.Markup` f�r att skriva ut f�rgade block med platsnummer. Implementera radbrytning (`%`). L�gg till f�rklaring.
4.  **Menyval:** L�gg till "Visa Karta" i huvudmenyn som anropar den nya metoden.

---

**Datum:** 2025-10-22

**Aktivitet:** Planering f�r implementation av enhetstester.

**Problem/Fundering:**
G-kravet kr�ver minst tv� enhetstester med MSTest. Hur skapas projektet och vad ska testas?

**L�sning/Slutsats (Plan):**
1.  **Skapa Testprojekt:** Ett MSTest-projekt (`PragueParking2.Tests`) l�ggs till i Solution.
2.  **Referens:** Referens fr�n `Tests` till `Core` l�ggs till.
3.  **Implementera Tester:** Minst tv� `[TestMethod]` implementeras f�r att testa `ParkingSpot`-klassen:
    * Testa att `IsEmpty()` �r `true` f�r en ny plats.
    * Testa att `IsEmpty()` �r `false` och `Count` �r 1 efter att ett fordon lagts till.
    * Anv�nd `Assert`-metoder f�r verifiering.