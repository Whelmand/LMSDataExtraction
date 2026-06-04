# LMSDataExtraction API — Technische Documentatie

> **Versie:** 1.0  
> **Datum:** Juni 2025  
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

---

## 1. Projectoverzicht

**LMSDataExtraction** is een .NET 8 Web API die data uit het Canvas LMS (Learning Management System) van Fontys Hogescholen extraheert en opslaat in een lokale PostgreSQL-database. De API dient als tussenlaag tussen Canvas en externe systemen zoals Portflow, FeedPulse en een competentiebeheersysteem.

### Doel

- Cursusgegevens, modules, opdrachten en gebruikersactiviteiten ophalen via de Canvas REST API
- Geëxtraheerde data persisteren in een eigen PostgreSQL-database
- Data beschikbaar stellen aan externe systemen (Portflow, FeedPulse, Competenties)
- Eenvoudige, veilige toegang via een Bearer-token (Canvas API Token)

### Technologiestack

| Onderdeel | Technologie |
|---|---|
| Backend framework | ASP.NET Core 8 (Web API) |
| Programmeertaal | C# 12 |
| Database | PostgreSQL (via Entity Framework Core + Npgsql) |
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
│                   API (Presentatielaag)             │
│  Controllers · Middleware · Swagger                 │
├─────────────────────────────────────────────────────┤
│                Application (Businesslaag)           │
│  Interfaces · DTOs · Mapping                        │
├─────────────────────────────────────────────────────┤
│                Domain (Kernlaag)                    │
│  Entities (Course, Module, Assignment, User, ...)   │
├─────────────────────────────────────────────────────┤
│              Infrastructure (Datalaag)              │
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
  │       │
  │       ├──► CanvasService ──► Canvas REST API (ophalen data)
  │       │
  │       └──► Repository ──► PostgreSQL (opslaan & raadplegen)
  │
  └──► JSON response terug naar gebruiker
```

---

## 3. Projectstructuur

```
LMSDataExtraction/
│
├── LMSDataExtraction.Api/              # Web API project (presentatielaag)
│   ├── Controllers/
│   │   ├── ActivityController.cs       # Activiteiten/inleveringen per cursus
│   │   ├── AssignmentController.cs     # Opdrachten per cursus
│   │   ├── CourseController.cs         # Cursusoverzicht
│   │   ├── ModuleController.cs         # Modules per cursus
│   │   └── PortflowController.cs       # Leerdoelen (Portflow mock)
│   ├── Middleware/
│   │   └── BearerTokenMiddleware.cs    # Authenticatie middleware
│   ├── Properties/
│   ├── Program.cs                      # App configuratie & DI registratie
│   ├── appsettings.json                # Productie-instellingen
│   └── appsettings.Development.json    # Ontwikkel-instellingen
│
├── LMSDataExtraction.Application/      # Businesslaag
│   ├── Interfaces/                     # Repository & service interfaces
│   ├── Dtos/                           # Data Transfer Objects (Canvas responses)
│   └── Mapping/                        # Object-naar-DTO mappers
│
├── LMSDataExtraction.Domain/           # Domeinlaag (kernentiteiten)
│   └── Entities/
│       ├── Course.cs
│       ├── Module.cs
│       ├── Assignment.cs
│       ├── User.cs
│       ├── Activity.cs
│       └── LearningGoal.cs
│
├── LMSDataExtraction.Infrastructure/   # Infrastructuurlaag
│   ├── Canvas/
│   │   └── CanvasService.cs            # HTTP-client voor Canvas API
│   ├── Persistence/
│   │   ├── AppDbContext.cs             # Entity Framework DbContext
│   │   └── Repositories/              # Concrete repository implementaties
│   └── Sources/
│       ├── PortflowMockSource.cs       # Mock implementatie Portflow
│       ├── FeedPulseMockSource.cs      # Mock implementatie FeedPulse
│       └── CompetenceMockSource.cs     # Mock implementatie Competenties
│
├── LMSDataExtraction.Tests/            # Unit- en integratietests
│
├── docker/                             # Docker-gerelateerde bestanden
├── .github/
│   └── workflows/
│       ├── ci.yml                      # Build & test workflow
│       └── docker-publish.yml          # Docker build & push workflow
├── Dockerfile                          # Multi-stage Docker build
├── .dockerignore                       # Bestanden uitgesloten van Docker build
└── docker-compose.yml                  # Lokale PostgreSQL database
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

> ⚠️ **Let op:** Roep eerst `GET /api/v1/Course` aan voordat je modules ophaalt. De cursus moet bekend zijn in de lokale database.

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
  },
  {
    "id": 55002,
    "name": "Opdracht 2: Docker implementatie",
    "dueDate": "2025-04-01T23:59:00Z",
    "maxScore": 10.0
  }
]
```

**Response 404 Not Found**
```json
"Course not found. Fetch courses first via GET /api/v1/Course."
```

---

### 4.4 Activity — Activiteiten per cursus

#### `GET /api/v1/Activity/{courseCanvasId}`

Haalt inleveringen (submissions) op van de ingelogde gebruiker voor een specifieke cursus en registreert deze als activiteiten.

**Parameters**

| Parameter | Type | Beschrijving |
|---|---|---|
| courseCanvasId | integer | Het Canvas-ID van de cursus |

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
  },
  {
    "assignmentId": 55002,
    "score": null,
    "submittedAt": null
  }
]
```

**Response 404 Not Found**
```json
"Course not found. Fetch courses first via GET /api/v1/Course."
```

**Logica:**
- Bepaalt de huidige gebruiker via Canvas API (`/api/v1/users/self`)
- Slaat de gebruiker op in de database als deze nog niet bestaat
- Haalt alle inleveringen (submissions) op van de gebruiker voor de opgegeven cursus
- Slaat elke nieuwe activiteit op (deduplicatie op gebruiker + opdracht + type)

---

### 4.5 Portflow — Leerdoelen

#### `GET /api/v1/Portflow/learninggoals`

Haalt leerdoelen op vanuit het Portflow-systeem. Momenteel een mock-implementatie.

**Request**
```http
GET /api/v1/Portflow/learninggoals
Authorization: Bearer <canvas_api_token>
```

**Response 200 OK**
```json
[
  {
    "id": 1,
    "title": "Leerdoel: CI/CD begrijpen en toepassen",
    "description": "De student kan een CI/CD pipeline opzetten met GitHub Actions"
  }
]
```

> ℹ️ Dit endpoint gebruikt momenteel een **mock-implementatie** (`PortflowMockSource`). De daadwerkelijke Portflow-integratie is nog in ontwikkeling.

---

### Aanbevolen aanroepvolgorde

Voor een correcte werking van de API is de volgende volgorde van aanroepen aanbevolen:

```
1. GET /api/v1/Course               → Cursussen ophalen & opslaan
2. GET /api/v1/Module/{id}          → Modules ophalen per cursus
3. GET /api/v1/Assignment/{id}      → Opdrachten ophalen per cursus
4. GET /api/v1/Activity/{id}        → Activiteiten registreren per cursus
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
3. Voer je Canvas API token in het veld in (zonder "Bearer " prefix — Swagger voegt dit automatisch toe)
4. Klik **Authorize** en dan **Close**
5. Alle endpoints zijn nu toegankelijk via de **Try it out** functie

---

## 6. Database

### Overzicht

De applicatie gebruikt **PostgreSQL** als database, beheerd via **Entity Framework Core** met de Npgsql provider.

### Entiteiten

#### Course
```
Course
├── Id           (int, primary key, auto-increment)
├── CanvasId     (int, uniek Canvas-ID)
├── Name         (string)
└── Description  (string, bevat de CourseCode)
```

#### Module
```
Module
├── Id           (int, primary key)
├── CanvasId     (int, uniek Canvas-ID)
├── Name         (string)
├── Position     (int, volgorde in cursus)
└── CourseId     (int, foreign key → Course)
```

#### Assignment
```
Assignment
├── Id           (int, primary key)
├── CanvasId     (int, uniek Canvas-ID)
├── Name         (string)
├── DueDate      (DateTime?, inleverdatum)
├── MaxScore     (decimal?, maximale score)
└── CourseId     (int, foreign key → Course)
```

#### User
```
User
├── Id           (int, primary key)
├── CanvasId     (int, uniek Canvas-ID)
├── Name         (string)
├── Email        (string)
└── Role         (string, bijv. "StudentEnrollment")
```

#### Activity
```
Activity
├── Id           (int, primary key)
├── UserId       (int, foreign key → User)
├── CourseId     (int, foreign key → Course)
├── SourceType   (string, bijv. "Submission")
├── SourceId     (int, Canvas assignment ID)
├── Score        (decimal?, behaalde score)
└── CompletedAt  (DateTime?, inlevertijdstip)
```

### Verbindingsstring

De database-verbindingsstring wordt geconfigureerd via `appsettings.json` of een omgevingsvariabele:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lmsdata;Username=postgres;Password=yourpassword"
  }
}
```

### Lokale database starten

```bash
docker-compose up -d
```

Dit start een PostgreSQL 16 container op poort 5432.

---

## 7. Docker & Containerisatie

### Dockerfile

De applicatie gebruikt een **multi-stage Docker build** om de image zo klein en veilig mogelijk te houden:

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

Het Docker image wordt gepubliceerd naar GitHub Container Registry:

```
ghcr.io/whelmand/lmsdataextraction:latest
ghcr.io/whelmand/lmsdataextraction:main
ghcr.io/whelmand/lmsdataextraction:sha-<commit-hash>
```

### .dockerignore

Uitgesloten van de Docker build context:

```
bin/, obj/, .git/, TestResults/
appsettings.Development.json
*.user, .vs/, .idea/
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

### Workflow 2: Docker Build & Publish (`docker-publish.yml`)

**Trigger:** Push naar `main` (of pull request voor build-only)

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
Azure Web App pikt het nieuwe :latest image op
```

**Gebruikte GitHub Actions:**
- `actions/checkout@v4`
- `docker/login-action@v3`
- `docker/metadata-action@v5`
- `docker/build-push-action@v5`

**Gemiddelde doorlooptijd:** ~2 minuten

---

## 9. Azure Deployment

### Infrastructuur

| Instelling | Waarde |
|---|---|
| Service | Azure App Service (Web App for Containers) |
| Resource Group | rg-lmsdataextraction |
| App naam | lmsdataextraction-api |
| Regio | Spain Central |
| App Service Plan | ASP-rglmsdataextraction-b0e5 (Basic B1) |
| OS | Linux |
| Container image | ghcr.io/whelmand/lmsdataextraction:latest |
| Poort | 8080 |
| Abonnement | Azure for Students |

### Live URL's

| Omgeving | URL |
|---|---|
| API root | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net |
| Swagger UI | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger |
| OpenAPI JSON | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger/v1/swagger.json |

### Deployment aanpak

Azure App Service for Containers haalt het image op uit GitHub Container Registry. Het registry is publiek toegankelijk, waardoor geen registry-credentials in Azure nodig zijn.

Bij elke push naar `main`:
1. GitHub Actions bouwt en pusht een nieuw image naar `ghcr.io/...lmsdataextraction:latest`
2. Azure Web App herstart automatisch en trekt het nieuwe `:latest` image op

### Handmatig herstarten in Azure Portal

1. Ga naar https://portal.azure.com
2. Zoek de resource **lmsdataextraction-api**
3. Klik op **Opnieuw starten** in de toolbar
4. Bevestig met **Ja**

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
- https://localhost:5001
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

| Variabele | Standaard | Beschrijving |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Production | Omgeving (Development/Production) |
| `ASPNETCORE_URLS` | http://+:8080 | Luisteradres en poort |
| `ConnectionStrings__DefaultConnection` | — | PostgreSQL verbindingsstring |

---

## 12. Bekende beperkingen & toekomstige verbeteringen

### Huidige beperkingen

- **Mock-implementaties:** Portflow, FeedPulse en Competentie-bronnen zijn momenteel mock-implementaties. De daadwerkelijke API-integraties zijn nog niet gerealiseerd.
- **Geen automatische database migraties:** Entity Framework migraties worden niet automatisch uitgevoerd bij opstarten.
- **Azure for Students quota:** Het Azure for Students abonnement heeft beperkte resources. Azure Container Registry kon niet worden aangemaakt door policy-beperkingen; GitHub Container Registry wordt als alternatief gebruikt.
- **Geen HTTPS-redirect in container:** De container luistert alleen op HTTP (poort 8080). HTTPS-terminatie vindt plaats op Azure App Service niveau.

### Toekomstige verbeteringen

- [ ] Implementatie van echte Portflow API-integratie
- [ ] Implementatie van FeedPulse API-integratie
- [ ] Implementatie van Competentie API-integratie
- [ ] Automatische database migraties bij startup
- [ ] Toevoegen van een deploy-stap in de GitHub Actions workflow (Azure Web App automatisch herstarten na push)
- [ ] Health check endpoint configureren in Azure
- [ ] Logging met Azure Application Insights
- [ ] Rate limiting voor Canvas API-aanroepen
- [ ] Caching uitbreiden voor Canvas-responses

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

*Documentatie gegenereerd op basis van de broncode en infrastructuur van het LMSDataExtraction project.*
