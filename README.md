
# 🍽️ Gastro Leinefelde Menu API

Eine REST‑API zum automatischen Abrufen, Parsen und Speichern der täglichen Speiseangebote von [essen‑auf‑raedern‑eichsfeld.de](https://essen-auf-raedern-eichsfeld.de/tagesangebot).  
Die Anwendung ist in **ASP.NET Core 9** geschrieben und verwendet **PostgreSQL** als Datenbank.  
Sie ist vollständig containerisiert und wird mit **Docker Compose** betrieben.

---

## 🚀 Schnellstart (lokal / Entwicklung)

### Voraussetzungen

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker & Docker Compose](https://www.docker.com/products/docker-desktop/)
- PostgreSQL (oder der Docker‑Container aus dem Compose‑File)

### Mit Docker Compose (Entwicklung)

```bash
git clone <your-repo-url>
cd GastroMenuParser
docker compose -f docker-compose.dev.yml up -d
```

Die API ist dann erreichbar unter:  
`http://localhost:8080`  
Swagger UI: `http://localhost:8080/swagger`  
pgAdmin (optional): `http://localhost:5050` (admin@admin.com / admin)

### Lokale Entwicklung ohne Docker

```bash
cd src/GastroLeinefeldeAPI
dotnet restore
dotnet ef database update
dotnet run
```

Die API läuft dann auf `http://localhost:5193` (Swagger unter `/swagger`).

---

## 📡 API‑Endpunkte

| Methode | Endpoint | Beschreibung |
|---------|----------|--------------|
| `POST` | `/api/menu/import?url=...` | Importiert das Menü von der Website (URL optional) |
| `GET` | `/api/menu` | Alle gespeicherten Gerichte |
| `GET` | `/api/menu/{id}` | Einzelnes Gericht |
| `GET` | `/api/menu/active` | Nur aktive Gerichte |
| `GET` | `/api/menu/category/{category}` | Nach Kategorie filtern |
| `GET` | `/health` | Health‑Check für Monitoring |
| `GET` | `/metrics` | **Prometheus‑Metriken** (für Monitoring) |

> **Hinweis:** Der `/metrics`‑Endpunkt wird im Produktionsbetrieb für das Monitoring verwendet (siehe Abschnitt [Monitoring](#-monitoring)).

---

## 🏗️ Architektur im Überblick

Die Anwendung folgt einem **Schichtenmodell** (Controller → Service → Repository → Datenbank).  
Für das Parsen der Website wird **HtmlAgilityPack** verwendet, die Datenbankanbindung erfolgt über **Entity Framework Core** mit **PostgreSQL**.  

Ein **Hash‑Feld** pro Gericht dient der **Änderungserkennung** – nur neue oder geänderte Einträge werden gespeichert.

---

## 🐳 Docker‑Services (Entwicklung vs. Produktion)

### Entwicklung (`docker-compose.dev.yml`)

| Service | Beschreibung | Port |
|---------|--------------|------|
| `api`   | ASP.NET Core API (Build aus Source) | `8080` |
| `postgres` | PostgreSQL | `5432` (intern) |

### Produktion (`docker-compose.yml`)

In der Produktion wird die API **nicht mehr lokal gebaut**, sondern das Image wird aus **GitHub Container Registry (GHCR)** bezogen.  
Der Tag wird über die Umgebungsvariable `VERSION` gesteuert (standardmässig `latest`).  

| Service | Beschreibung | Port |
|---------|--------------|------|
| `api`   | Image: `ghcr.io/your-org/gastro-api:${VERSION}` | intern `8080` |
| `postgres` | PostgreSQL mit persistentem Volume | intern |
| `caddy` | Reverse‑Proxy mit automatischem HTTPS (Let's Encrypt) | `80`, `443` |

Weitere Services (Monitoring) werden über eine separate Compose‑Datei (`docker-compose.monitoring.yml`) betrieben.

---

## 🚀 Produktions‑Deployment (vollständige Infrastruktur)

Das Production‑Setup besteht aus:

- **Terraform** – verwaltet die gesamte Cloud‑Infrastruktur (Hetzner):
  - VPS‑Instanzen
  - Firewall‑Regeln
  - Private Netzwerke
  - Persistentes Volume für PostgreSQL
- **Ansible** – konfiguriert die Betriebssysteme (Docker, Benutzer, Fail2Ban, UFW, automatische Updates)
- **GitHub Actions** – drei unabhängige Pipelines:
  1. **Infrastructure** – führt `terraform plan/apply` bei Änderungen im `terraform/`‑Verzeichnis aus.
  2. **Build** – baut das Docker‑Image und pusht es mit zwei Tags (`${{ github.sha }}` und `latest`) nach GHCR.
  3. **Deploy** – zieht das neue Image auf den/die Zielserver und startet die Container mit `docker compose up -d` neu.

Der **Deploy‑Workflow** verwendet **kein `latest`‑Tag**, sondern den konkreten Commit‑SHA, um reproduzierbare und rollback‑fähige Deployments zu gewährleisten.

### Serveranforderungen

- Ubuntu 26.04 LTS
- Docker Engine & Docker Compose Plugin
- Git
- Domain (z.B. `gastro.example.com`)

Die gesamte Einrichtung (inkl. Docker, Benutzer, Firewall, etc.) wird automatisch durch **Ansible** beim ersten Start des Servers durchgeführt – gesteuert über **Cloud‑Init** (minimaler Bootstrap, der nur Ansible installiert und den Playbook‑Run startet).

---

## 🔐 GitHub Secrets (für CI/CD)

Folgende Secrets müssen im Repository hinterlegt werden:

| Secret | Beschreibung |
|--------|--------------|
| `HCLOUD_TOKEN` | Hetzner Cloud API‑Token (Read/Write) |
| `OBJ_ACCESS_KEY` | Access Key für Object Storage (Terraform State & Backups) |
| `OBJ_SECRET_KEY` | Secret Key für Object Storage |
| `PROD_SERVER_IP` | IP‑Adresse des Production‑Servers |
| `STAGE_SERVER_IP` | IP‑Adresse des Stage‑Servers |
| `PROD_SSH_KEY` | Privater SSH‑Key für Production |
| `STAGE_SSH_KEY` | Privater SSH‑Key für Stage |

---

## 📈 Monitoring

Das Monitoring besteht aus:

- **Prometheus** – Sammlung von Metriken (API‑Metriken, Node‑Exporter, cAdvisor)
- **Grafana** – Visualisierung mit vordefinierten Dashboards
- **Loki + Promtail** – zentrale Log‑Aggregation

Die Metriken der API werden unter `/metrics` im Prometheus‑Format exponiert.  
Das Monitoring‑Stack wird über `docker-compose.monitoring.yml` gestartet und nutzt dasselbe interne Netzwerk wie die Anwendung.

**Start:**
```bash
docker compose -f docker-compose.monitoring.yml up -d
```

Grafana ist dann unter `http://<server-ip>:3000` erreichbar (Standard‑Login: admin / $GRAFANA_PASSWORD).

---

## 💾 Backup & Restore

- **Datenbank‑Backup** (PostgreSQL) – täglicher Dump, der automatisch in den Object Storage hochgeladen wird (Script: `ops/backups/backup.sh`).
- **Volume‑Snapshots** – wöchentliche Snapshots des persistenten Volumes (über Hetzner API).

**Manuelles Restore:**
```bash
./ops/backups/restore.sh <backup-file.sql.gz>
```

---

## 🔧 Wichtige technische Entscheidungen (Zusammenfassung)

- **Infrastructure as Code** – alle Cloud‑Ressourcen sind in Terraform deklariert.
- **Separation of Concerns** – Trennung zwischen Infrastruktur (Terraform), System‑Konfiguration (Ansible) und Anwendungs‑Deployment (GitHub Actions).
- **Container‑Registry** – GHCR mit SHA‑Tags für reproduzierbare Deployments.
- **Monitoring und Logging** – von Anfang an integriert.
- **Sicherheit** – Firewall, Fail2Ban, automatisierte Updates, keine Secrets im Code.
- **Skalierbarkeit** – durch modulare Terraform‑Konfiguration können jederzeit weitere Server hinzugefügt werden.

---

## 📁 Projektstruktur (auszugsweise)

```
GastroMenuParser/
├── .github/
│   └── workflows/
│       ├── infrastructure.yml
│       ├── build.yml
│       └── deploy.yml
├── terraform/
│   ├── modules/
│   │   ├── server/
│   │   ├── firewall/
│   │   ├── network/
│   │   └── volume/
│   ├── live/
│   │   ├── production/
│   │   ├── stage/
│   │   └── dev/
│   ├── .tflint.hcl
│   └── Makefile
├── ops/
│   ├── ansible/
│   │   ├── playbooks/
│   │   └── inventory/
│   ├── monitoring/
│   │   ├── prometheus.yml
│   │   ├── loki-config.yml
│   │   └── promtail-config.yml
│   └── backups/
│       ├── backup.sh
│       └── restore.sh
├── src/
│   ├── GastroLeinefeldeAPI/
│   └── GastroLeinefeldeAPI.Tests/
├── docker-compose.yml
├── docker-compose.monitoring.yml
├── .env.example
└── README.md
```

---

## 📝 Hinweise für Entwickler

### Metriken hinzufügen / erweitern

Die API exponiert bereits Metriken unter `/metrics`.  
Verwendet wird die Bibliothek **`prometheus-net.AspNetCore`**.  
In `Program.cs` wird die Middleware mit `app.UseHttpMetrics()` und `app.UseMetricServer()` aktiviert.

Weitere benutzerdefinierte Metriken (z.B. Anzahl importierter Gerichte, Fehlerraten) können über das `Metrics`-Objekt von `prometheus-net` definiert werden.

### Lokales Testen der Metriken

```bash
curl http://localhost:5193/metrics
```

---

## 🐛 Fehlerbehebung (häufige Probleme)

### PostgreSQL‑Authentifizierung schlägt fehl

Wenn ein persistentes Volume wiederverwendet wird, muss das Passwort in der Datenbank mit dem in der Connection‑String übereinstimmen.  
Abhilfe:

```sql
ALTER USER postgres PASSWORD 'postgres';
```

Dann die API neu starten: `docker compose restart api`.

### Caddy kann kein Zertifikat bekommen

Stelle sicher, dass die Domain öffentlich erreichbar ist und die Ports 80/443 vom Internet aus offen sind.  
Prüfe die Firewall‑Regeln (Terraform / Hetzner).

---

## 📄 Lizenz

MIT


---

