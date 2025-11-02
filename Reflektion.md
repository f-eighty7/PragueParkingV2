### Reflektion – Prague Parking V2

#### 1. Inledning

Detta dokument är en personlig reflektion över mitt arbete med inlämningsuppgift 2, "Prague Parking V2". Syftet är att analysera den tekniska och konceptuella resan från det första, proceduriella V1-projektet, genom refaktoreringen till en grundläggande objektorienterad struktur (V2.0, G-nivå), och slutligen till den avancerade, polymorfiska och utbyggbara lösningen för V2.1 (VG-nivå).

Reflektionen kommer att täcka de designval, problem och lösningar som uppstod under projektets gång, samt hur de lärdomar jag drog från min V1-reflektion direkt påverkade mitt angreppssätt i denna uppgift.

#### 2. Bakgrund

Min utgångspunkt för V2 var min egen slutsats från V1-reflektionen: den ursprungliga lösningen, som byggde på en `string[100]`-array och komplex strängmanipulation, var "bräcklig", "naiv" och "en dålig grund att bygga vidare på". Jag var smärtsamt medveten om att systemet var en återvändsgränd.

När jag påbörjade V2 var jag dock helt ny inför Objektorienterad Programmering (OOP). Min första prioritet var därför att hantera denna osäkerhet.

Beslutet landade i en tvåstegsstrategi:
1.  **G-nivå (V2.0):** Jag valde att göra en "direkt översättning" av V1-logiken till OOP, snarare än att designa om systemet från grunden. Syftet var att lära mig *mekaniken* i OOP (klasser, objekt, listor) utan att samtidigt behöva lösa *ny* komplex logik. Jag skapade klasser som `Car`, `MC`, `ParkingSpot` och `ParkingGarage`. Parkeringslogiken var dock identisk med V1: en `ParkingSpot` kunde hålla 1 Bil eller 2 MC, vilket styrdes av en `MaxPerSpot`-egenskap i `config.json`. Den stora vinsten här var den arkitektoniska uppdelningen i tre projekt (Core, Data, UI), vilket omedelbart löste V1:s "allt-i-en-fil"-problem.

2.  **VG-nivå (V2.1):** Först efter att G-nivån var stabil och jag kände mig bekväm med OOP, påbörjade jag VG-kraven. Detta blev inte en *utökning* av G-koden, utan en *total refaktorering*. G-logiken revs ut och ersattes helt.

Detta tvåstegstänk blev avgörande. Det separerade inlärningskurvan för OOP-syntax från inlärningskurvan för komplex, abstrakt design.

#### 3. Genomförande

Genomförandet följde den plan som lades i bakgrunden.

**Fas 1 (G-nivå):** Fokuserade på att etablera strukturen. `PragueParkingV2.Core` fylldes med de grundläggande klasserna. `PragueParkingV2.Data` byggdes för att hantera serialisering till/från `garage.json` och inläsning av `config.json` och `pricelist.txt`. `PragueParkingV2.UI` innehöll all användarinteraktion (med Spectre.Console) och anropen till logikmetoderna. Denna fas resulterade i en fungerande G-version som uppfyllde alla V1-krav i en ny, objektorienterad skrud.

**Fas 2 (VG-nivå):** Här skedde den verkliga omvandlingen, en total refaktorering.
1.  **"Size":** Jag började med att implementera `Size`-egenskapen. `MaxPerSpot` raderades helt från `VehicleTypeConfig.cs`. Istället fick `Config.cs` en ny egenskap, `ParkingSpotSize` (satt till 4).
2.  **Interfaces och Arv:** Jag skapade `IVehicle` och `IParkingSpot` enligt VG-kravet. Basklassen `Vehicle` gjordes `abstract` och fick implementera `IVehicle`. Alla fordonsklasser (`Car`, `MC`) uppdaterades för att ärva från `Vehicle` och definiera sin `Size` (4 respektive 2). Därefter skapades de nya klasserna `Bike` (Size 1) och `Bus` (Size 16).
3.  **Logik-omskrivning:** Kärnan i arbetet. Metoderna `AddVehicle`, `RemoveVehicle` och `MoveVehicle` i `Program.cs` kastades och skrevs om från grunden. De byggde nu på att kontrollera `spot.OccupiedSpace` (en ny egenskap i `ParkingSpot`) mot `vehicle.Size`.
4.  **UI-uppdatering:** Alla metoder som visade data (`PrintGarageStatus`, `DisplayGarageMapUI`, `ShowSpotContent`) skrevs om för att hantera de nya klasserna, `OccupiedSpace` och dynamiskt visa `BIKE` och `BUS`.
5.  **Återstående VG-krav:** Slutligen implementerades de sista kraven: automatisk generering av testdata i `DataAccess.cs` om `garage.json` saknas, och en ny meny för att dynamiskt hantera konfigurationen (ladda om fil, eller ändra och spara priser/storlek).

#### 4. Problem

Under denna tvåfas-process uppstod flera betydande utmaningar.

* **Problem 1: Det konceptuella "dubbelarbetet"**
    Det största problemet var egentligen ett designval jag gjorde från början. Genom att implementera G-nivån som en "direkt översättning" av V1-logiken (`MaxPerSpot`), skapade jag en lösning som jag *visste* var en återvändsgränd. Hela den logiken var värdelös för VG-nivån. Detta innebar att jag fick skriva kärnfunktioner som `AddVehicle` och `RemoveVehicle` två gånger – först för G, sedan en total omskrivning för VG. Detta var ineffektivt, men som nybörjare inom OOP kändes det som ett nödvändigt steg för att hantera inlärningskurvan.

* **Problem 2: Den algoritmiska utmaningen med "Bussar"**
    Detta var den i särklass svåraste *tekniska* utmaningen. Kraven för en buss (storlek 16) var komplexa: den måste uppta 4 sammanhängande platser (16 / 4 = 4), och den fick bara använda plats 1-50.
    * **Parkeringslogiken:** Att hitta *en* plats med ledig kapacitet var enkelt (`spot.OccupiedSpace + vehicle.Size <= spot.Capacity`). Men att hitta *fyra helt tomma, sammanhängande platser* krävde en helt ny algoritm. Lösningen blev en loop som itererade från `i = 0` till `maxSpotIndex - spotsNeeded`, och för varje `i` använde LINQ (`garage.Spots.Skip(i).Take(spotsNeeded)`) för att hämta ett "fönster" av platser, följt av en `All(spot => spot.OccupiedSpace == 0)`-kontroll.
    * **Datamodellering och borttagning:** Hur representerar man en buss som upptar fyra platser? Lösningen blev att lägga till *samma* buss-objektreferens till `ParkedVehicles`-listan för *alla fyra* `ParkingSpot`-objekt (t.ex. plats 10, 11, 12 och 13). Detta fick `OccupiedSpace` att korrekt rapportera `16` för alla dessa platser. Detta skapade dock ett nytt problem: när `RemoveVehicle` anropades, var det tvunget att hitta och ta bort *alla fyra* referenser, inte bara den första. Lösningen blev att `RemoveVehicle` loopar igenom *hela* garaget (alla 100 platser) och kör `spot.ParkedVehicles.Remove(vehicleToRemove)` på varje, för att garantera att alla instanser rensas.

* **Problem 3: Paradoxen med Interfaces, Arv och JSON-Persistens**
    VG-kraven specificerade interfaces (`IVehicle`, `IParkingSpot`) och min implementation använde en `List<Vehicle>` i `ParkingSpot.cs` för att lagra polymorfiska objekt (Cars, MCs, etc.). Detta visade sig skapa en tyst, katastrofal bugg.
    * **Symptomet:** Varje gång jag startade om programmet var garaget tomt. Mina sparade bilar och testdatan var borta.
    * **Rotorsaken:** `System.Text.Json` (som används i `DataAccess.cs`) kan inte deserialisera (ladda) en lista av abstrakta basklasser (`List<Vehicle>`) som standard. Den läser JSON-datan men vet inte om den ska skapa ett `Car`, `MC`, `Bus` eller `Bike`. Detta kastade en `JsonException`, som min `LoadGarage`-metod tolkade som en korrupt fil, vilket ledde till att den skapade ett tomt garage (`CreateEmptyGarage`).
    * **Lösningen:** Den eleganta lösningen var inte att sluta använda arv, utan att *lära* JSON-serialiseraren hur man hanterar det. Genom att dekorera den abstrakta `Vehicle`-klassen med `[JsonDerivedType]`-attribut för varje underklass (`Car`, `MC`, `Bus`, `Bike`), kunde serialiseraren korrekt spara och ladda den polymorfiska listan.

#### 5. Lärdomar

1.  **Lita på min V1-Reflektion:** Min största lärdom från V1 var att den bräckliga stränglösningen var fel. Jag borde ha tillämpat den lärdomen hårdare på V2. Jag *borde* ha insett att G-nivåns `MaxPerSpot`-logik var en exakt kopia av V1:s problem, bara i OOP-kläder. Den var lika oflexibel och dömde mig till dubbelarbete. Hade jag läst VG-kraven *först* hade jag byggt G-nivån på `Size`-principen direkt, vilket hade sparat enormt med tid.

2.  **Test-Driven Development (TDD) för komplex logik:** Buss-logiken var svår att felsöka genom att köra applikationen om och om igen. Jag hade ett test-projekt men använde det inte proaktivt. Jag borde ha skrivit enhetstester *först* (t.ex. `Test_AddBus_FindsFourConsecutiveSpots`, `Test_AddBus_Fails_IfSpot49_IsOccupied`, `Test_RemoveBus_ClearsAllFourSpots`). Att skriva testerna hade tvingat mig att tänka igenom alla kantfall innan jag skrev en enda rad implementering.

3.  **Persistens Först! (Lärdomen från JSON-buggen):** Min viktigaste lärdom var att **arkitekturkrav krockar**. VG-kravet "koda mot interfaces/abstraktioner" krockade direkt med G/VG-kravet "data skall sparas". Min implementation av `List<Vehicle>` var "korrekt" ur ett OOP-perspektiv men "totalt fel" ur ett persistens-perspektiv. Lärdomen är att *alltid* testa att en datamodell kan sparas och laddas *direkt* efter att man har skapat eller ändrat den. Att hitta den eleganta lösningen (`[JsonDerivedType]`-attributen) visade att man oftast *kan* uppfylla båda kraven, men det kräver djupare kunskap om verktygen (i detta fall, `System.Text.Json`).

#### 6. Slutsats

Att gå från V1 till V2.1 var inte en uppgradering; det var ett paradigmskifte. Projektet tvingade mig att konfrontera den tekniska skulden från V1 och betala av den genom att helt byta arkitektur. Den största utmaningen var inte att skriva kod, utan att *designa* kod. Att gå från den konkreta `MaxPerSpot`-logiken till den abstrakta `Size`-logiken var ett stort steg.

Slutprodukten är ett system som inte bara uppfyller alla krav i V2-PDF:en, utan som också är robust, underhållsvänligt och genuint flexibelt för framtida förändringar. Att hitta och åtgärda den kritiska persistens-buggen var frustrerande men oerhört lärorikt, och slutprodukten är nu stabil tack vare det. Jag har framgångsrikt byggt det system som min V1-reflektion önskade att jag hade byggt från början.

Det här projektet, i kombination med det föregående, var den enskilt viktigaste lärdomen i kursen. Det var här all teoretisk kunskap (OOP, Arv, Polymorfism, Interfaces, Separation of Concerns, JSON-serialisering, LINQ) slutade vara isolerade koncept och tvingades samarbeta för att lösa ett verkligt problem.

Att tvingas genomgå "smärtan" i V1 var avgörande. Utan att först ha upplevt *varför* V1-metoden var så fruktansvärt dålig, hade jag aldrig fullt ut uppskattat *varför* V2-metoden är så bra. Jag känner att jag inte bara har lärt mig att *skriva* C#-kod; jag har börjat lära mig att *designa* mjukvarusystem. Denna uppgift har gett mig ett fundamentalt nytt och mer kraftfullt sätt att tänka kring problemlösning.