# TripPlanner

Webová aplikace pro plánování výletů a cest, která umožňuje spravovat itinerář, místa k návštěvě, ubytování, checklisty, sdílené výdaje a spolupracovníky v rámci jednoho tripu.

Projekt je postavený jako vícevrstvá **ASP.NET Core MVC** aplikace s oddělenou **Domain**, **Application**, **Infrastructure** a **Web** vrstvou. Součástí je autentizace přes **ASP.NET Core Identity**, perzistence přes **Entity Framework Core** a databázi **MySQL**.

---

## Hlavní funkce

- registrace, přihlášení a správa uživatelského účtu
- vytváření a správa tripů
- detail výletu s itinerářem rozděleným po dnech
- přidávání a přesouvání míst mezi jednotlivými dny
- změna pořadí míst v rámci dne
- správa ubytování
- checklisty a checklist položky
- sdílený budget a evidence výdajů
- rozdělování výdajů mezi více uživatelů
- přidávání collaboratorů ke konkrétnímu tripu
- načítání předpovědi počasí pro destinaci
- práce s lokací a integrace s Google API

---

## Použité technologie

- **C# / .NET 8**
- **ASP.NET Core MVC**
- **Entity Framework Core**
- **ASP.NET Core Identity**
- **MySQL**
- **HTML / CSS / JavaScript**
- **Google API**
- **Open-Meteo API**

---

## Architektura projektu

Projekt je rozdělen do čtyř hlavních vrstev:

### `TripPlanner.Web`
Prezentační vrstva aplikace.

Obsahuje:
- MVC controllery
- views
- area pro `Security`
- area pro `User`
- konfiguraci aplikace
- registraci služeb a middleware pipeline

### `TripPlanner.Application`
Aplikační vrstva.

Obsahuje:
- rozhraní služeb
- implementace aplikační logiky
- DTO objekty
- ViewModely

Tato vrstva řeší hlavní use-cases aplikace, například:
- správu tripů
- práci s itinerary
- budget logiku
- checklisty
- collaborateory
- počasí
- komunikaci s Google API

### `TripPlanner.Domain`
Doménová vrstva.

Obsahuje hlavní entity projektu, například:
- `Trip` - základní informace o výletu
- `TripDay` - jednotlivé dny výletu
- `Place` - místa přiřazená ke konkrétním dnům
- `Destination` - cílová destinace tripu
- `Location` - geografická data a souřadnice
- `Accomodation` - ubytování přiřazené k tripu
- `Checklist` - seznamy úkolů a věcí s možností odškrtávání
- `ChecklistItem` - položky seznamu úkolů a věcí
- `Expense` - výdaje úživatelů
- `ExpenseShare` - rozdělení výdajů mezi uživatele
- `TripUsers` - vazba mezi tripem a spolupracovníky

### `TripPlanner.Infrastructure`
Infrastrukturní vrstva.

Obsahuje:
- `ApplicationDbContext`
- Identity uživatele
- EF Core migrace
- databázovou konfiguraci a mapování relací

---

## Hlavní části aplikace

### Autentizace a účet

Uživatel se může:

- registrovat
- přihlásit
- odhlásit
- upravit své základní údaje v nastavení

---

### Správa tripů

Uživatel může:

- vytvořit nový trip
- zobrazit přehled svých tripů
- otevřít detail tripu
- upravit název, popis a datumy
- smazat trip

---

### Itinerář

Po vytvoření tripu se generují jednotlivé dny výletu podle rozsahu dat.  
Do těchto dnů je možné:

- přidávat místa
- přesouvat místa mezi dny
- měnit jejich pořadí v rámci dne

---

### Budget

Trip může obsahovat výdaje, které se rozdělí mezi více uživatelů.

Aplikace počítá:

- seznam výdajů
- zůstatky uživatelů
- celkové částky za uživatele
- návrh settlementů

---

### Checklisty

Každý trip může obsahovat jeden nebo více checklistů a jejich položek.

---

### Collaboratoři

K tripu lze přidat další uživatele, kteří se na plánování podílí.

---

### Počasí

Aplikace umí načíst předpověď počasí pro souřadnice destinace a promítnout ji do jednotlivých dnů itineráře.
