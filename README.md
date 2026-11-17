# FC Barcelona Website
Eine Fan-Website über den FC Barcelona, entwickelt mit ASP.NET Core MVC und einer MySQL-Datenbank.

---

## Über das Projekt
Dieses Projekt ist eine Website rund um den FC Barcelona (Saison 2026/27), entwickelt mit ASP.NET Core MVC (.NET 10) und MySQL. Ziel war es, das MVC-Muster sauber umzusetzen: Controller nehmen Anfragen entgegen, Repositories greifen per ADO.NET auf die Datenbank zu und Razor-Views zeigen die Daten an.

Besucher sehen den Kader und die gewonnenen Titel. Wer sich registriert, kann Lieblingsspieler als Favoriten markieren und daraus ein Bild speichern. Admins pflegen Spieler, Titel und Benutzer direkt auf der Website.

## Funktionen
• Startseite – Übersicht mit Titelbild und den wichtigsten Wettbewerben (La Liga, Copa del Rey, Supercopa)
• Kader – Alle Spieler mit Rückennummer, Land, Position und Alter (wird aus dem Geburtsdatum berechnet), mit Live-Suche nach Namen und seitlich scrollbarer Tabelle auf dem Handy
• Titel – Alle gewonnenen Titel nach Jahr sortiert, getrennt nach National und International
• Registrierung & Login – Benutzer registrieren sich selbst und sind danach direkt eingeloggt, Weiterleitung zurück auf die vorherige Seite
• Favoriten – Per Stern-Klick Spieler markieren, ohne dass die Seite neu lädt (fetch + JSON)
• Favoriten als Bild – Der Knopf "Save as image" speichert die Favoriten als Story-Bild
• Admin-Bereich – Spieler, Titel und Benutzer erstellen, bearbeiten und löschen, inklusive Foto-Upload (PNG, JPG, WebP, max. 2 MB)
• Benutzerverwaltung – Admin-Rolle vergeben oder entziehen, Passwörter zurücksetzen, Benutzer löschen
• Fehlerseiten – Eigene 404-Seite und Fehlerseite

## Sicherheit
• Passwort-Hashing – PBKDF2 mit SHA-256 und Salt, alte Klartext-Passwörter werden beim nächsten Login automatisch ersetzt
• SQL-Injection-Schutz – Alle Abfragen laufen über parametrisierte ADO.NET-Befehle
• CSRF-Schutz – Jedes POST-Formular braucht ein Anti-Forgery-Token
• Rollenprüfung – Der Admin-Bereich prüft die Rolle bei jeder Anfrage in der Datenbank, ein entzogenes Admin-Recht gilt sofort
• Sichere Weiterleitung – Nach dem Login sind nur lokale Rücksprung-URLs erlaubt
• Eingabevalidierung – Benutzername, Passwort, Geburtsdatum und Rückennummer werden geprüft

## Was ich gelernt habe
• MVC-Architektur mit Controllern, Models, ViewModels und Razor-Views
• Datenzugriff mit ADO.NET und MySqlConnector, ein Repository pro Tabelle
• Dependency Injection (Singleton für die Datenbank, Scoped für die Repositories)
• Session-basierte Anmeldung und Rollen mit einem eigenen ActionFilter
• Passwörter sicher speichern mit Salt und Hash
• Formulare mit Validierung, Anti-Forgery und Datei-Upload
• Asynchrone Klicks mit fetch und JSON statt Seiten-Reload
• Responsives Design mit eigenen Fonts (Anton, Barlow Condensed) und langem Caching

## Projektstruktur
```
MVC/
├── Controllers/   HomeController, AdminController
├── Data/          Database, PlayerRepository, TrophyRepository,
│                  UserRepository, FavouriteRepository
├── Filters/       AdminOnlyAttribute
├── Models/        Player, Trophy, User, ViewModels
├── Services/      PasswordHasher
├── Views/         Home, Admin, Shared
└── wwwroot/       CSS, Fonts, Bilder
```

## Aufgabenstellung
Die Aufgabe bestand darin, eine dynamische Webanwendung nach dem MVC-Muster mit Datenbankanbindung umzusetzen. Dazu gehören Benutzerverwaltung mit Login, Daten aus einer Datenbank anzeigen und verwalten (CRUD) sowie eine saubere Trennung von Logik, Datenzugriff und Darstellung.

## Autor
**sxrg1u** – Schulprojekt / MVC (IMS-T Informatik)

---

## Download & Starten
**Schritt 1 – Repository holen:** `git clone https://github.com/sxrg1u/BarcaWebsite.git` (oder als ZIP herunterladen).

**Schritt 2 – Datenbank einrichten:** In MySQL eine Datenbank `barcadata` mit den Tabellen `Users`, `Players`, `Trophies` und `Favourites` anlegen.

**Schritt 3 – Verbindung anpassen:** In `MVC/appsettings.json` den Connection String `BarcaData` mit deinen eigenen MySQL-Zugangsdaten ersetzen.

**Schritt 4 – Starten:** Die Datei `MVC.slnx` mit Visual Studio öffnen und mit F5 ausführen, oder im Ordner `MVC` per Terminal:

```bash
dotnet run
```

**Voraussetzungen:** .NET 10 SDK, MySQL Server (Port 3306), Visual Studio 2022 oder neuer (optional).

**Admin werden:** Registriere einen Benutzer und setze in der Tabelle `Users` die Spalte `Role` auf `Admin`. Danach können weitere Admins im Admin-Bereich vergeben werden.
