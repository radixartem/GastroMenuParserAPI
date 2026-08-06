# 🍽️ Gastro Leinefelde Menu API

Eine REST-API zum automatischen Abrufen, Parsen und Speichern der täglichen Speiseangebote von [essen-auf-raedern-eichsfeld.de](https://essen-auf-raedern-eichsfeld.de/tagesangebot). Die Anwendung ist in ASP.NET Core 9 geschrieben und verwendet PostgreSQL als Datenbank.

---

## 🚀 Schnellstart

### Voraussetzungen

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker & Docker Compose](https://www.docker.com/products/docker-desktop/) (für Container-Betrieb)
- PostgreSQL (oder Docker-Container)

### Start mit Docker Compose (empfohlen)

```bash
# 1. Repository klonen
git clone <your-repo-url>
cd GastroMenuParser

# 2. Container starten (API + PostgreSQL + pgAdmin)
docker-compose up -d

# 3. API ist erreichbar unter:
#    http://localhost:8080
#    Swagger UI: http://localhost:8080/swagger
#    pgAdmin: http://localhost:5050 (admin@admin.com / admin)
```

### Lokale Entwicklung

```bash
# 0. Pakete installieren
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package HtmlAgilityPack
dotnet add package Serilog.AspNetCore
dotnet add package Swashbuckle.AspNetCore


# 1. In das Projektverzeichnis wechseln
cd src/GastroLeinefeldeAPI

# 2. Abhängigkeiten wiederherstellen
dotnet restore

# 3. Datenbank-Migration anwenden (PostgreSQL muss laufen)
dotnet ef database update

# 4. API starten
dotnet run

# Die API läuft nun unter:
#    http://localhost:5193
#    Swagger UI: http://localhost:5193/swagger
```

---

## 📡 API-Endpunkte

| Methode | Endpoint | Beschreibung |
|---------|----------|--------------|
| `POST` | `/api/menu/import?url=...` | Importiert das Menü von der Website (URL optional) |
| `GET` | `/api/menu` | Gibt alle gespeicherten Gerichte zurück |
| `GET` | `/api/menu/{id}` | Gibt ein einzelnes Gericht zurück |
| `GET` | `/api/menu/active` | Gibt nur aktive Gerichte zurück |
| `GET` | `/api/menu/category/{category}` | Filtert nach Kategorie |
| `GET` | `/health` | Health-Check für Monitoring |

### Beispiel: Menü importieren

**PowerShell:**
```powershell
Invoke-RestMethod -Method POST -Uri "http://localhost:5193/api/menu/import"
```

**curl (Linux/macOS):**
```bash
curl -X POST "http://localhost:5193/api/menu/import"
```

### Beispiel: Alle Gerichte abrufen

**PowerShell:**
```powershell
Invoke-RestMethod -Method GET -Uri "http://localhost:5193/api/menu"
```

**curl:**
```bash
curl "http://localhost:5193/api/menu"
```

---

## 🧠 Wichtigste technische Entscheidungen

### 1. Architektur – Schichtenmodell

Die Anwendung ist in klare Schichten unterteilt (Controller → Service → Repository → Datenbank). Dies fördert die Wartbarkeit, Testbarkeit und macht die Codebasis übersichtlich.

- **Controller** – Nur für HTTP-Kommunikation zuständig
- **Service** – Enthält die gesamte Geschäftslogik
- **Repository** – Abstrahiert den Datenbankzugriff
- **Models/DTOs** – Trennung von Datenbank- und API-Modellen

### 2. Parsing mit `HtmlAgilityPack`

Die Website liefert kein maschinenlesbares JSON/XML, sondern nur HTML. `HtmlAgilityPack` ist die etablierte Bibliothek in .NET, um auch "schmutziges" HTML zuverlässig zu parsen und Textknoten zu extrahieren. Die Verwendung von regulären Ausdrücken für die Extraktion von Preisen, Zeiten und Status macht die Logik flexibel gegenüber kleinen Änderungen im Text.

### 3. Datenbank & Entity Framework Core

- **PostgreSQL** als relationale Datenbank – robust, Open Source und gut in Docker-Umgebungen integrierbar.
- **Entity Framework Core** als ORM – reduziert den Boilerplate-Code für Datenbankzugriffe und ermöglicht einfache Migrationen.
- Ein `Hash`-Feld pro Gericht dient der **Detektion von Änderungen**: So werden nur wirklich neue oder veränderte Gerichte in der Datenbank aktualisiert, ohne doppelte Einträge zu erzeugen.

### 4. Containerisierung mit Docker Compose

Die gesamte Anwendung inklusive Datenbank wird über `docker-compose.yml` bereitgestellt. Das stellt eine konsistente Entwicklungsumgebung sicher und erleichtert das Deployment. Über `pgAdmin` kann die Datenbank komfortabel verwaltet werden.

### 5. Fehlerbehandlung und Logging

- Einheitliche **try-catch-Blöcke** in den Services mit aussagekräftigen HTTP-Statuscodes (z.B. 404, 500).
- **Structured Logging** mit `ILogger` – erleichtert das Debugging und Monitoring.

---

## 📦 Docker Compose Services

Die `docker-compose.yml` startet drei Container:

| Service | Beschreibung | Port |
|---------|--------------|------|
| **`api`** | ASP.NET Core API | `8080` |
| **`postgres`** | PostgreSQL-Datenbank | `5432` |

Alle Services sind über ein internes Netzwerk verbunden.

---

## 🧪 Tests

Die Tests können mit dem Befehl `dotnet test` im Hauptverzeichnis ausgeführt werden.

```bash
dotnet test
```

---

## 📁 Projektstruktur (Auszug)

```
GastroMenuParser/
├── src/
│   ├── GastroLeinefeldeAPI/          # Hauptprojekt
│   │   ├── Controllers/              # API-Controller
│   │   ├── Models/                   # Daten- und DTO-Modelle
│   │   ├── Data/                     # DbContext & Migrationen
│   │   ├── Services/                 # Business-Logik (Parser, Repository, Service)
│   │   ├── Program.cs                # Einstiegspunkt
│   │   └── appsettings.json          # Konfiguration
│   └── GastroLeinefeldeAPI.Tests/    # Unit- und Integrationstests
├── docker-compose.yml                # Multi-Container Setup
├── .env                              # Umgebungsvariablen
└── README.md                         # Diese Datei
```

---

## 🔧 Konfiguration

Die wichtigsten Einstellungen können über Umgebungsvariablen oder `appsettings.json` angepasst werden:

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=gastro_menu;Username=postgres;Password=postgres"
  }
}
```

In Docker können diese Werte über die `environment`-Sektion in der `docker-compose.yml` überschrieben werden.


!!!  Wenn die Anwendung keine Verbindung zur Datenbank herstellen kann, 
     muss man ein Benutzerpasswort in der Datenbank festlegen. Z.b.: ALTER USER postgres PASSWORD 'postgres';
---

## 📄 Lizenz

MIT
```

---
