# SzervizPont

A SzervizPont egy autószervizhez készített időpontfoglaló webalkalmazás, amely a szoftverfejlesztő és tesztelő képzés vizsgaprojektjeként készült.

## Csapat

**Csapatnév:** SzervizPont

- Dézsi Péter – dezsipeti2000@gmail.com
- Dézsi Richárd – dezsirichard29@gmail.com

## A repository felépítése

```text
SzervizPont/
├── Frontend/    HTML, CSS és JavaScript
├── Backend/     ASP.NET Core Web API és C# kód
├── Database/    adatbázisséma és kapcsolati diagram
├── Documents/   projekt- és felhasználói dokumentáció
├── start.bat
├── reset-database.bat
└── README.md
```

## Fő funkciók

- regisztráció és belépés;
- ügyfél- és adminisztrátori szerepkör;
- autók felvétele, módosítása és törlése;
- szolgáltatások kezelése adminisztrátorként;
- időpontok foglalása, módosítása és törlése;
- időpont státuszának kezelése adminisztrátorként;
- reszponzív, mobilon is használható felület.

## Használt technológiák

- C#
- ASP.NET Core 8 Web API
- Entity Framework Core
- SQLite
- HTML5
- CSS3
- JavaScript
- Swagger

## Indítás

A programhoz .NET 8 SDK szükséges.

1. Csomagold ki vagy klónozd a repository-t.
2. A fő mappában indítsd el a `start.bat` fájlt.
3. A backend elindul, és a `Frontend` mappa weboldalait szolgálja ki.
4. Nyisd meg a böngészőben a megjelenő címet, például `https://localhost:7092`.

Parancssorból:

```powershell
cd Backend
dotnet restore
dotnet run
```

## Adminisztrátori tesztfiók

- E-mail: `admin@szervizpont.hu`
- Jelszó: `Admin123!`

Az adminisztrátor és az alap szolgáltatások az első indításkor automatikusan létrejönnek.

## Adatbázis visszaállítása

## Csapattagok

- Dézsi Péter
- Dézsi Richárd

A projekt szoftverfejlesztő vizsgaremekként készült.

A `reset-database.bat` törli a futás közben létrejött SQLite adatbázist. A következő indításkor a program automatikusan létrehozza újra.

## Dokumentáció

A `Documents` mappában található a projektdokumentáció és a felhasználói dokumentáció.
