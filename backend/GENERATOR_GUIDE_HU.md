# Edzésterv-generátor – rövid útmutató

## Mi készült el?

A generátor a projekt jelenlegi frontend-szerződéséhez igazodik: a `POST /api/plans/generate` egy heti tervet állít elő a cél, az otthoni/termi helyszín, a rendelkezésre álló eszközök, a napok, az edzésidő és a nehézség alapján. A kiválasztás egyszerű szabályokra és véletlen sorrendre épül; nincs benne gépi tanulás.

A szabályok külön `WorkoutGenerator` osztályban vannak, ezért az osztály adatbázis nélkül, kézzel összeállított gyakorlatlistákkal is tesztelhető. Az API vezérlő menti az elkészült tervet, visszaadja a hét mind a hét napját, és támogatja a gyakorlatok kipipálását.

## Visual Studio lépések

1. Klónozd a csapat GitHub-repóját, majd hozz létre egy külön ágat: `feat/workout-generator`.
2. Nyisd meg a `backend/EdzestervGenerator.sln` fájlt Visual Studioban. A projekt .NET 10-et használ, a szükséges SDK legyen telepítve.
3. A generátor logikáját a `src/EdzestervGenerator.Api/Services/WorkoutGenerator.cs` fájlban találod. Ez szűri az eszközöket és nehézséget, napokra bont, cél szerint állítja be az ismétlést, és kiválasztja a gyakorlatokat.
4. Az `ExerciseSeed.cs` a jelenlegi, kisméretű magyar példakatalógus. Az eszközazonosítók a projekt `EquipmentSeed` fájljához igazodnak. Amikor a másik backendes véglegesíti a wger-importot, itt vagy külön importleképezésben cseréljétek a gyakorlatokat a valódi adatokra.
5. Az `Program.cs` regisztrálja a generátort, és első induláskor feltölti az üres gyakorlatlistát és a demo felhasználót. A már létező adatbázis-migrációkhoz nem kell új oszlop.
6. Futtasd a teszteket a Solution Explorerben a **Test > Run All Tests** menüponttal, vagy terminálból a `cd backend` után: `dotnet test`.
7. Indítsd a backend projektet Visual Studioból. A frontend által használt végpontok: `POST /api/plans/generate`, `GET /api/plans/current`, `PATCH /api/plans/exercises/{id}`.
8. Commitolj angol Conventional Commit üzenettel, pushold az ágat, majd nyiss PR-t a csapat README-jének folyamata szerint. A repó README-je PR-jóváhagyást kér összeolvasztás előtt.

## Kérésminta

```json
{
  "goal": "Hypertrophy",
  "location": "Home",
  "equipmentIds": [1, 4],
  "days": ["Monday", "Thursday"],
  "sessionMinutes": 60,
  "difficulty": "Medium"
}
```

A `sessionMinutes` 45/60/75/90 perc esetén 4/5/6/7 gyakorlatot kér. Ha az otthoni eszközök és a nehézség mellett kevesebb gyakorlat marad, a terv rövidebb lesz. A kézisúlyzós példagyakorlatok kezdeti súlya a katalógus `MinWeightKg` értéke; a kérés jelenlegi frontend-sémája csak eszközazonosítókat küld, felhasználói súlyokat nem.

A hét hétfőtől vasárnapig jelenik meg, Budapest helyi hete szerint. Legfeljebb három edzésnapnál teljes testes nap készül, négy vagy több napnál felső és alsótest váltja egymást. Ha elérhető a katalógusban fekvőtámasz, a heti generálás egyik felsőtestes napjára bekerül, így van egy követhető alapgyakorlat.

## Fontos egyeztetés a csapattal

A mellékelt backend-terv régebbi változata napi, fixen öt gyakorlatot ír le, és az elmúlt 48 óra aktivitását is figyelembe venné. A GitHub-repó és a frontend aktuális API-terve viszont heti tervet kér, cél/helyszín/nap/session beállításokkal, és nem küld aktivitási előzményt. Ez a megvalósítás a repó aktuális heti API-tervét követi. Mielőtt PR-t nyitsz, egyeztesd a másik backendessel, hogy ez legyen-e a végleges szerződés.

A katalógus csak bemutató seed. A wger gyakorlatok izomcsoporthoz rendelését nem a képek címkézésével, hanem az API mezőinek ellenőrzésével és egy átlátható lokális leképezéssel érdemes elvégezni; a seedet a wger-import felelőse tudja majd lecserélni.
