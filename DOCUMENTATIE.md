# LMSDataExtraction API — Technische Documentatie

> **Versie:** 1.2
> **Datum:** Juni 2026
> **Auteur:** Whelmand
> **Repository:** https://github.com/Whelmand/LMSDataExtraction
> **Live API:** https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net
> **Swagger UI:** https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger

---

## Inhoudsopgave

1. [Projectoverzicht](#1-projectoverzicht)
2. [Architectuur](#2-architectuur)
3. [Projectstructuur](#3-projectstructuur)
4. [API Endpoints](#4-api-endpoints)
5. [Authenticatie](#5-authenticatie)
6. [Database](#6-database)
7. [Docker & Containerisatie](#7-docker--containerisatie)
8. [CI/CD Pipeline](#8-cicd-pipeline)
9. [Azure Deployment](#9-azure-deployment)
10. [Lokaal draaien](#10-lokaal-draaien)
11. [Omgevingsvariabelen](#11-omgevingsvariabelen)
12. [Bekende beperkingen & toekomstige verbeteringen](#12-bekende-beperkingen--toekomstige-verbeteringen)
13. [Changelog & opgeloste problemen](#13-changelog--opgeloste-problemen)

---

## 1. Projectoverzicht

**LMSDataExtraction** is een .NET 8 Web API die data uit het Canvas LMS (Learning Management System) van Fontys Hogescholen extraheert en opslaat in een PostgreSQL-database in Azure. De API dient als tussenlaag tussen Canvas en externe systemen zoals Portflow, FeedPulse en een competentiebeheersysteem.

### Doel

- Cursusgegevens, modules, opdrachten en gebruikersactiviteiten ophalen via de Canvas REST API
- Geëxtraheerde data persisteren in een eigen Azure PostgreSQL-database
- Data beschikbaar stellen aan externe systemen (Portflow, FeedPulse, Competenties)
- Eenvoudige, veilige toegang via een Bearer-token (Canvas API Token)

### Technologiestack

| Onderdeel | Technologie |
|---|---|
| Backend framework | ASP.NET Core 8 (Web API) |
| Programmeertaal | C# 12 |
| Database | Azure Database for PostgreSQL Flexible Server |
| ORM | Entity Framework Core + Npgsql |
| Containerisatie | Docker (multi-stage build) |
| Container Registry | GitHub Container Registry (ghcr.io) |
| Cloud hosting | Azure App Service (Web App for Containers) |
| CI/CD | GitHub Actions |
| API Documentatie | Swagger / OpenAPI 3 (Swashbuckle) |

---

## 2. Architectuur

De applicatie volgt **Clean Architecture** met een strikte scheiding van verantwoordelijkheden:

```
┌─────────────────────────────────────────────────────┐
│  API (Presentatielaag)                              │
│  Controllers · Middleware · Swagger                 │
├─────────────────────────────────────────────────────┤
│  Application (Businesslaag)                         │
│  Interfaces · DTOs · Mapping                        │
├─────────────────────────────────────────────────────┤
│  Domain (Kernlaag)                                  │
│  Entities (Course, Module, Assignment, User, ...)   │
├─────────────────────────────────────────────────────┤
│  Infrastructure (Datalaag)                          │
│  Canvas HTTP-client · Repositories · DbContext      │
└─────────────────────────────────────────────────────┘
```

### Dataflow

```
Gebruiker
│  Bearer Token (Canvas API Token)
▼
Azure Web App (lmsdataextraction-api)
│
├──► BearerTokenMiddleware (valideert token aanwezigheid)
│
├──► Controller (bijv. CourseController)
│      │
│      ├──► CanvasService ──► Canvas REST API (ophalen data)
│      │
│      └──► Repository ──► Azure PostgreSQL (opslaan & raadplegen)
│
└──► JSON response terug naar gebruiker
```

---

## 3. Projectstructuur

```
LMSDataExtraction/
│
├── LMSDataExtraction.Api/          # Web API project (presentatielaag)
│   ├── Controllers/
│   │   ├── ActivityController.cs   # Activiteiten/inleveringen per cursus
│   │   ├── AssignmentController.cs # Opdrachten per cursus
│   │   ├── CourseController.cs     # Cursusoverzicht
│   │   ├── ModuleController.cs     # Modules per cursus
│   │   └── PortflowController.cs   # Leerdoelen (Portflow mock)
│   ├── Middleware/
│   │   └── BearerTokenMiddleware.cs # Authenticatie middleware
│   ├── Properties/
│   ├── Program.cs                  # App configuratie & DI registratie
│   ├── appsettings.json            # Productie-instellingen
│   └── appsettings.Development.json # Ontwikkel-instellingen
│
├── LMSDataExtraction.Application/  # Businesslaag
│   ├── Interfaces/                 # Repository & service interfaces
│   ├── Dtos/                       # Data Transfer Objects (Canvas responses)
│   └── Mapping/                    # Object-naar-DTO mappers
│
├── LMSDataExtraction.Domain/       # Domeinlaag (kernentiteiten)
│   └── Entities/
│       ├── Course.cs
│       ├── Module.cs
│       ├── Assignment.cs
│       ├── User.cs
│       ├── Activity.cs
│       └── LearningGoal.cs
│
├── LMSDataExtraction.Infrastructure/ # Infrastructuurlaag
│   ├── Canvas/
│   │   └── CanvasService.cs        # HTTP-client voor Canvas API
│   ├── Persistence/
│   │   ├── AppDbContext.cs         # Entity Framework DbContext
│   │   └── Repositories/          # Concrete repository implementaties
│   └── Sources/
│       ├── PortflowMockSource.cs   # Mock implementatie Portflow
│       ├── FeedPulseMockSource.cs  # Mock implementatie FeedPulse
│       └── CompetenceMockSource.cs # Mock implementatie Competenties
│
├── LMSDataExtraction.Tests/        # Unit- en integratietests
│
├── docker/                         # Docker-gerelateerde bestanden
├── .github/
│   └── workflows/
│       ├── ci.yml                  # Build & test workflow
│       └── docker-publish.yml      # Docker build & push workflow
├── Dockerfile                      # Multi-stage Docker build
├── .dockerignore                   # Bestanden uitgesloten van Docker build
└── docker-compose.yml              # Lokale PostgreSQL database
```

---

## 4. API Endpoints

### Basispad
```
https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/api/v1
```

Alle endpoints vereisen een **Authorization header** met een geldig Canvas API Token (zie [Authenticatie](#5-authenticatie)).

---

### 4.1 Course — Cursussen

#### `GET /api/v1/Course`

Haalt alle cursussen op van de ingelogde Canvas-gebruiker en slaat nieuwe cursussen op in de database.

**Request**
```http
GET /api/v1/Course
Authorization: Bearer <canvas_api_token>
```

**Response 200 OK**
```json
[
  {
    "id": 12345,
    "name": "Software Engineering 2B",
    "courseCode": "SE2B-2025"
  },
  {
    "id": 12346,
    "name": "Cloud & DevOps",
    "courseCode": "CLOUD-2025"
  }
]
```

**Logica:**
- Roept Canvas API aan om cursussen op te halen
- Controleert per cursus of deze al bestaat in de database (op basis van Canvas ID)
- Slaat nieuwe cursussen op (deduplicatie)
- Geeft alle opgehaalde Canvas-cursussen terug als response

---

### 4.2 Module — Modules per cursus

#### `GET /api/v1/Module/{courseCanvasId}`

Haalt alle modules op van een specifieke cursus.

**Parameters**

| Parameter | Type | Beschrijving |
|---|---|---|
| courseCanvasId | integer | Het Canvas-ID van de cursus (te verkrijgen via GET /Course) |

**Request**
```http
GET /api/v1/Module/12345
Authorization: Bearer <canvas_api_token>
```

**Response 200 OK**
```json
[
  {
    "id": 98765,
    "name": "Week 1 — Introductie",
    "position": 1
  },
  {
    "id": 98766,
    "name": "Week 2 — Docker basics",
    "position": 2
  }
]
```

**Response 404 Not Found**
```json
"Course not found. Fetch courses first via GET /api/v1/Course."
```

> ⚠️ **Let op:** Roep eerst `GET /api/v1/Course` aan voordat je modules ophaalt.

---

### 4.3 Assignment — Opdrachten per cursus

#### `GET /api/v1/Assignment/{courseCanvasId}`

Haalt alle opdrachten op van een specifieke cursus.

**Parameters**

| Parameter | Type | Beschrijving |
|---|---|---|
| courseCanvasId | integer | Het Canvas-ID van de cursus |

**Request**
```http
GET /api/v1/Assignment/12345
Authorization: Bearer <canvas_api_token>
```

**Response 200 OK**
```json
[
  {
    "id": 55001,
    "name": "Opdracht 1: REST API ontwerpen",
    "dueDate": "2025-03-15T23:59:00Z",
    "maxScore": 10.0
  }
]
```

---

### 4.4 Activity — Activiteiten per cursus

#### `GET /api/v1/Activity/{courseCanvasId}`

Haalt inleveringen (submissions) op van de ingelogde gebruiker voor een specifieke cursus.

**Request**
```http
GET /api/v1/Activity/12345
Authorization: Bearer <canvas_api_token>
```

**Response 200 OK**
```json
[
  {
    "assignmentId": 55001,
    "score": 8.5,
    "submittedAt": "2025-03-14T20:30:00Z"
  }
]
```

---

### 4.5 Portflow — Leerdoelen

#### `GET /api/v1/Portflow/learninggoals`

Haalt leerdoelen op vanuit het Portflow-systeem. Momenteel een mock-implementatie.

> ℹ️ Dit endpoint gebruikt momenteel een **mock-implementatie** (`PortflowMockSource`).

---

### Aanbevolen aanroepvolgorde

```
1. GET /api/v1/Course                → Cursussen ophalen & opslaan
2. GET /api/v1/Module/{id}           → Modules ophalen per cursus
3. GET /api/v1/Assignment/{id}       → Opdrachten ophalen per cursus
4. GET /api/v1/Activity/{id}         → Activiteiten registreren per cursus
5. GET /api/v1/Portflow/learninggoals → Leerdoelen ophalen (mock)
```

---

## 5. Authenticatie

De API gebruikt **Canvas API Tokens** als authenticatiemechanisme via een custom Bearer Token middleware.

### Hoe werkt het?

Bij elk inkomend request controleert de `BearerTokenMiddleware` of een geldige Authorization-header aanwezig is:

```
Authorization: Bearer <jouw_canvas_api_token>
```

De middleware laat requests naar `/swagger` en `/health` altijd door zonder token.

### Canvas API Token aanmaken

1. Log in op je Canvas-omgeving (bijv. https://canvas.fontys.nl)
2. Ga naar **Account → Instellingen**
3. Scroll naar beneden naar **Goedgekeurde integraties**
4. Klik op **+ Nieuw toegangstoken**
5. Geef een beschrijving op en stel eventueel een verloopdatum in
6. Kopieer het gegenereerde token — dit is maar één keer zichtbaar

### Foutmeldingen authenticatie

| Situatie | HTTP Status | Response |
|---|---|---|
| Geen Authorization header | 401 | `{"error":"Authorization header ontbreekt."}` |
| Header begint niet met "Bearer " | 401 | `{"error":"Authorization header moet beginnen met 'Bearer '."}` |
| Token is leeg na "Bearer " | 401 | `{"error":"Bearer token is leeg."}` |

### Gebruik in Swagger UI

1. Ga naar https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger
2. Klik op de groene **Authorize 🔒** knop rechtsboven
3. Voer je Canvas API token in het veld in (zonder "Bearer " prefix)
4. Klik **Authorize** en dan **Close**
5. Alle endpoints zijn nu toegankelijk via de **Try it out** functie

### Gebruik in Postman

1. Open Postman en maak een nieuwe request aan
2. Stel de URL in op bijv. `GET https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/api/v1/Course`
3. Ga naar het tabblad **Authorization**
4. Kies type **Bearer Token**
5. Plak je Canvas API token in het Token-veld
6. Klik **Send**

---

## 6. Database

### Overzicht

De applicatie gebruikt **Azure Database for PostgreSQL Flexible Server** als productiedatabase, beheerd via **Entity Framework Core** met de Npgsql provider.

### Azure PostgreSQL — Productie-infrastructuur

| Instelling | Waarde |
|---|---|
| Server naam | lmsdataextraction-db |
| Volledig adres | lmsdataextraction-db.postgres.database.azure.com |
| Database naam | lmsdata |
| Gebruikersnaam | lmsadmin |
| Regio | Spain Central |
| Compute tier | Burstable B1ms (1 vCore, 2 GiB RAM) |
| PostgreSQL versie | 16 |
| SSL vereist | Ja (`SSL Mode=Require`) |
| Authenticatie | PostgreSQL-only (username/password) |

### Verbindingsstring (productie)

De verbindingsstring wordt ingesteld als **App-instelling** in de Azure Web App:

```
Naam:  ConnectionStrings__DefaultConnection
Waarde: Host=lmsdataextraction-db.postgres.database.azure.com;Port=5432;Database=lmsdata;Username=lmsadmin;Password=<wachtwoord>;SSL Mode=Require;Trust Server Certificate=true
```

> ⚠️ De App-instelling `ConnectionStrings__DefaultConnection` (met dubbele underscore) is de meest betrouwbare methode om connection strings door te geven aan .NET apps in Docker containers op Azure. De waarde overschrijft automatisch `ConnectionStrings:DefaultConnection` uit `appsettings.json`.

### Tabellen automatisch aanmaken

Bij het opstarten van de applicatie worden ontbrekende databasetabellen **automatisch aangemaakt** via `EnsureCreated()` in `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
```

Dit zorgt ervoor dat de database bij de eerste deployment direct klaar is voor gebruik, zonder handmatige migratiestappen.

### Entiteiten

#### Course
```
Course
├── Id          (int, primary key, auto-increment)
├── CanvasId    (int, uniek Canvas-ID)
├── Name        (string)
└── Description (string, bevat de CourseCode)
```

#### Module
```
Module
├── Id       (int, primary key)
├── CanvasId (int, uniek Canvas-ID)
├── Name     (string)
├── Position (int, volgorde in cursus)
└── CourseId (int, foreign key → Course)
```

#### Assignment
```
Assignment
├── Id       (int, primary key)
├── CanvasId (int, uniek Canvas-ID)
├── Name     (string)
├── DueDate  (DateTime?, inleverdatum)
├── MaxScore (decimal?, maximale score)
└── CourseId (int, foreign key → Course)
```

#### User
```
User
├── Id       (int, primary key)
├── CanvasId (int, uniek Canvas-ID)
├── Name     (string)
├── Email    (string)
└── Role     (string, bijv. "StudentEnrollment")
```

#### Activity
```
Activity
├── Id          (int, primary key)
├── UserId      (int, foreign key → User)
├── CourseId    (int, foreign key → Course)
├── SourceType  (string, bijv. "Submission")
├── SourceId    (int, Canvas assignment ID)
├── Score       (decimal?, behaalde score)
└── CompletedAt (DateTime?, inlevertijdstip)
```

### Lokale database (ontwikkeling)

Voor lokaal ontwikkelen wordt PostgreSQL gestart via Docker Compose:

```bash
docker-compose up -d
```

Lokale verbindingsstring in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lmsdata;Username=postgres;Password=postgres"
  }
}
```

---

## 7. Docker & Containerisatie

### Dockerfile

De applicatie gebruikt een **multi-stage Docker build**:

**Stage 1 — Build (dotnet/sdk:8.0)**
- Kopieert projectbestanden en `.sln`
- Voert `dotnet restore` uit (NuGet packages)
- Compileert en publiceert de applicatie naar `/app/publish`

**Stage 2 — Runtime (dotnet/aspnet:8.0)**
- Gebruikt alleen de lichtgewicht ASP.NET runtime image (geen SDK)
- Kopieert de gepubliceerde output van Stage 1
- Exposeert poort **8080**
- Start de applicatie met `dotnet LMSDataExtraction.Api.dll`

### Docker image locatie

```
ghcr.io/whelmand/lmsdataextraction:latest
ghcr.io/whelmand/lmsdataextraction:main
ghcr.io/whelmand/lmsdataextraction:sha-<commit-hash>
```

---

## 8. CI/CD Pipeline

De CI/CD pipeline is volledig geautomatiseerd via **GitHub Actions** en bestaat uit twee workflows.

### Workflow 1: CI — Build & Test (`ci.yml`)

**Trigger:** Push of pull request naar `main`

**Stappen:**
1. Checkout repository
2. Setup .NET 8 SDK
3. Restore NuGet packages
4. Build de solution
5. Voer unit tests uit
6. Upload test results als artifact

### Workflow 2: Docker Build, Publish & Deploy (`docker-publish.yml`)

**Trigger:** Push naar `main` (alleen — niet bij pull requests)

**Stappen:**

```
Push naar main
│
▼
1. Checkout repository
│
▼
2. Login bij GitHub Container Registry
   (via automatisch GITHUB_TOKEN — geen secrets nodig)
│
▼
3. Extraheer Docker metadata
   (tags: latest, main, sha-<hash>)
│
▼
4. Build Docker image (multi-stage)
│
▼
5. Push naar ghcr.io/whelmand/lmsdataextraction
│
▼
6. Login bij Azure
   (via AZURE_CREDENTIALS secret — service principal)
│
▼
7. Restart Azure Web App (lmsdataextraction-api)
   → Web App trekt automatisch het nieuwe :latest image op
```

**Gemiddelde doorlooptijd:** ~1-2 minuten

### Vereiste GitHub Secrets

| Secret naam | Beschrijving |
|---|---|
| `AZURE_CREDENTIALS` | Azure service principal JSON (clientId, clientSecret, subscriptionId, tenantId) |

De `AZURE_CREDENTIALS` secret is aangemaakt via Azure Cloud Shell:
```bash
az ad sp create-for-rbac \
  --name "lmsdataextraction-github-actions" \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/rg-lmsdataextraction \
  --sdk-auth
```

### CI/CD testen

De volledige pipeline testen doe je door een kleine wijziging naar `main` te pushen:

```bash
# Optie 1 — lege commit (geen code wijziging nodig)
git commit --allow-empty -m "ci: test pipeline"
git push origin main
```

Daarna controleer je in volgorde:
1. **GitHub Actions** → beide workflows groen? (Actions-tabblad in de repo)
2. **GHCR** → nieuw image gepusht? (Packages-sectie op je GitHub profiel)
3. **Azure Web App** → herstart gelukt? (Activity Log in Azure Portal)
4. **Live API** → reageert de API nog correct? (Swagger UI of Postman)

**Gemiddelde doorlooptijd:** ~1-2 minuten

---

## 9. Azure Deployment

### Infrastructuur overzicht

| Resource | Details |
|---|---|
| **Web App** | lmsdataextraction-api |
| App Service Plan | ASP-rglmsdataextraction-b0e5 (Basic B1) |
| Resource Group | rg-lmsdataextraction |
| Regio | Spain Central |
| OS | Linux |
| Container image | ghcr.io/whelmand/lmsdataextraction:latest |
| Poort | 8080 |
| Abonnement | Azure for Students |
| **PostgreSQL Server** | lmsdataextraction-db |
| DB adres | lmsdataextraction-db.postgres.database.azure.com |
| Database | lmsdata |
| Compute tier | Burstable B1ms |

### Live URL's

| Omgeving | URL |
|---|---|
| API root | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net |
| Swagger UI | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger |
| OpenAPI JSON | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger/v1/swagger.json |

### Deployment aanpak

Bij elke push naar `main`:
1. GitHub Actions bouwt en pusht een nieuw image naar `ghcr.io/...lmsdataextraction:latest`
2. Azure Web App herstart en trekt het nieuwe `:latest` image op
3. `EnsureCreated()` controleert of alle tabellen bestaan en maakt ze aan indien nodig

### Omgevingsvariabelen in Azure

De volgende instellingen zijn geconfigureerd in de Azure Web App onder **Omgevingsvariabelen**:

| Type | Naam | Beschrijving |
|---|---|---|
| App-instelling | `ConnectionStrings__DefaultConnection` | PostgreSQL verbindingsstring (overschrijft appsettings.json) |
| App-instelling | `WEBSITES_ENABLE_APP_SERVICE_STORAGE` | Azure standaardinstelling |

> 💡 **Technische toelichting:** Azure App Service geeft connection strings door als omgevingsvariabelen. Voor Docker containers is de meest betrouwbare methode om dit te doen via een App-instelling met de naam `ConnectionStrings__DefaultConnection` (dubbele underscore = .NET configuratie-hiërarchie). Dit overschrijft automatisch de waarde in `appsettings.json`.

### Handmatig herstarten in Azure Portal

1. Ga naar https://portal.azure.com
2. Zoek de resource **lmsdataextraction-api**
3. Klik op **Opnieuw starten** in de toolbar bovenaan
4. Bevestig met **Ja**
5. De melding "Web-app is opnieuw gestart" verschijnt rechtsbovenin

---

## 10. Lokaal draaien

### Vereisten

- .NET 8 SDK
- Docker Desktop
- PostgreSQL (of via Docker)

### Stap 1: Repository klonen

```bash
git clone https://github.com/Whelmand/LMSDataExtraction.git
cd LMSDataExtraction
```

### Stap 2: Database starten

```bash
docker-compose up -d
```

### Stap 3: Verbindingsstring instellen

Bewerk `LMSDataExtraction.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lmsdata;Username=postgres;Password=postgres"
  }
}
```

### Stap 4: Applicatie starten

```bash
dotnet run --project LMSDataExtraction.Api
```

De API is nu beschikbaar op:
- http://localhost:5000
- Swagger: http://localhost:5000/swagger

### Stap 5: Tests uitvoeren

```bash
dotnet test
```

### Lokaal via Docker draaien

```bash
# Image bouwen
docker build -t lmsdataextraction:local .

# Container starten
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=lmsdata;Username=postgres;Password=postgres" \
  lmsdataextraction:local
```

---

## 11. Omgevingsvariabelen

| Variabele | Standaard (lokaal) | Productie (Azure) | Beschrijving |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Development | Production | Omgeving |
| `ASPNETCORE_URLS` | http://+:5000 | http://+:8080 | Luisteradres en poort |
| `ConnectionStrings__DefaultConnection` | localhost:5432 | Azure PostgreSQL | Volledige PostgreSQL verbindingsstring |
| `Canvas:BaseUrl` | https://fhict.instructure.com/api/v1 | (zelfde) | Canvas API basis-URL |

---

## 12. Bekende beperkingen & toekomstige verbeteringen

### Huidige beperkingen

- **Mock-implementaties:** Portflow, FeedPulse en Competentie-bronnen zijn momenteel mock-implementaties.
- **Azure for Students quota:** Azure Container Registry kon niet worden aangemaakt door policy-beperkingen; GitHub Container Registry wordt als alternatief gebruikt.
- **Geen HTTPS-redirect in container:** De container luistert alleen op HTTP (poort 8080). HTTPS-terminatie vindt plaats op Azure App Service niveau.

### Toekomstige verbeteringen

- [ ] Implementatie van echte Portflow API-integratie
- [ ] Implementatie van FeedPulse API-integratie
- [ ] Implementatie van Competentie API-integratie
- [x] ~~Automatische redeploy via GitHub Actions — Azure Web App herstart nu automatisch na elke push naar main~~ ✅ Opgelost in v1.2
- [ ] Health check endpoint configureren in Azure
- [ ] Logging met Azure Application Insights
- [ ] Rate limiting voor Canvas API-aanroepen
- [ ] Caching uitbreiden voor Canvas-responses
- [ ] EF Core Migrations invoeren ter vervanging van EnsureCreated (voor schema-updates)

---

## 13. Changelog & opgeloste problemen

### Versie 1.2 — Juni 2026

#### ✅ Volledige CI/CD pipeline met automatische Azure deployment
**Wat:** De `docker-publish.yml` workflow uitgebreid met automatische deployment naar Azure.
**Oplossing:** Twee extra stappen toegevoegd na de Docker push:
1. `azure/login@v2` — logt in bij Azure via een service principal (`AZURE_CREDENTIALS` secret)
2. `azure/CLI@v2` — voert `az webapp restart` uit, waardoor de Web App het nieuwe `:latest` image ophaalt

De `AZURE_CREDENTIALS` secret is aangemaakt via Azure Cloud Shell met `az ad sp create-for-rbac` en opgeslagen in GitHub Settings → Secrets and variables → Actions.

**Resultaat:** Elke push naar `main` triggert nu de volledige flow: build → test → push naar GHCR → deploy naar Azure. Handmatig herstarten is niet meer nodig.

---

### Versie 1.1 — Juni 2025

#### ✅ Swagger ingeschakeld in productie
**Probleem:** Swagger was omgeven door `if (app.Environment.IsDevelopment())`, waardoor het niet beschikbaar was in productie.
**Oplossing:** De if-conditie verwijderd zodat Swagger altijd ingeschakeld is.

#### ✅ Azure PostgreSQL database opgezet
**Wat:** Azure Database for PostgreSQL Flexible Server aangemaakt in Spain Central op de Burstable B1ms tier (goedkoopste optie, valt onder de 750 gratis uren van Azure for Students).
**Details:** Server `lmsdataextraction-db`, database `lmsdata`, gebruiker `lmsadmin`.

#### ✅ Database verbinding geconfigureerd voor Docker containers op Azure
**Probleem:** De applicatie verbond nog met `localhost:5433` in plaats van de Azure PostgreSQL server. De connection string in de "Verbindingsreeksen" sectie van Azure werd niet opgepikt door de Docker container.
**Oorzaak:** Voor .NET apps in Docker containers op Azure App Service werkt de verbindingsreeks-instelling soms niet betrouwbaar. De omgevingsvariabele wordt op een andere manier doorgegeven dan verwacht.
**Oplossing:** Een **App-instelling** toegevoegd met de naam `ConnectionStrings__DefaultConnection` (dubbele underscore). In .NET's configuratiesysteem werkt dubbele underscore als scheidingsteken voor geneste sleutels, wat equivalent is aan `ConnectionStrings:DefaultConnection` in appsettings.json. Dit is de meest betrouwbare methode voor Docker containers.

#### ✅ Databasetabellen automatisch aangemaakt
**Probleem:** De Azure PostgreSQL database was leeg — er bestonden nog geen tabellen. De app crashte met een 500-fout bij het eerste verzoek.
**Oplossing:** `db.Database.EnsureCreated()` toegevoegd aan het opstartproces in `Program.cs`. Bij elke start controleert de applicatie of de tabellen bestaan en maakt ze aan indien nodig.

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
```

#### Resultaat
Na deze wijzigingen geeft de API bij aanroep zonder token een correcte **401 Unauthorized** terug (in plaats van een 500-fout). De volledige flow van code-push tot werkende API in Azure is operationeel.

---

## Contactinformatie & Links

| Resource | Link |
|---|---|
| GitHub Repository | https://github.com/Whelmand/LMSDataExtraction |
| Live API | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net |
| Swagger UI | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger |
| Docker Image | https://github.com/Whelmand/LMSDataExtraction/pkgs/container/lmsdataextraction |
| GitHub Actions | https://github.com/Whelmand/LMSDataExtraction/actions |
| Azure Portal | https://portal.azure.com |

---

*Documentatie bijgewerkt op basis van de volledige Azure deployment inclusief PostgreSQL database configuratie.*
