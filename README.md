# LMSDataExtraction — LMS Dataontsluiting

> Dataontsluitingslaag voor de challenge **"Bouw je eigen LMS"** (Fontys ICT).
> Een ASP.NET Core 8 Web API die data uit **Canvas LMS** (en aanvullende, deels gemockte bronnen) ophaalt en via één uniforme REST API aanbiedt aan de afnemende componenten (Frontend, Chatbot Service en Activiteitenplanning).

[![CI](https://github.com/Whelmand/LMSDataExtraction/actions/workflows/ci.yml/badge.svg)](https://github.com/Whelmand/LMSDataExtraction/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)
![Docker](https://img.shields.io/badge/Docker-multi--stage-2496ED)

- **Live API:** https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net
- **Swagger UI:** https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger
- **Repository:** https://github.com/Whelmand/LMSDataExtraction

---

## Inhoudsopgave

1. [Wat is dit?](#1-wat-is-dit)
2. [Projectcontext](#2-projectcontext)
3. [Eisen](#3-eisen)
4. [Architectuur](#4-architectuur)
5. [Technologiestack](#5-technologiestack)
6. [Projectstructuur](#6-projectstructuur)
7. [Snel starten (lokaal)](#7-snel-starten-lokaal)
8. [Datamodel](#8-datamodel)
9. [API-endpoints](#9-api-endpoints)
10. [Authenticatie](#10-authenticatie)
11. [Caching](#11-caching)
12. [Database & migraties](#12-database--migraties)
13. [Databronnen en status](#13-databronnen-en-status)
14. [Testen](#14-testen)
15. [Docker](#15-docker)
16. [CI/CD-pipeline](#16-cicd-pipeline)
17. [Omgevingen & deployment](#17-omgevingen--deployment)
18. [Configuratie & omgevingsvariabelen](#18-configuratie--omgevingsvariabelen)
19. [Bekende beperkingen & aanbevelingen](#19-bekende-beperkingen--aanbevelingen)
20. [Projectteam](#20-projectteam)
21. [Verdere documentatie](#21-verdere-documentatie)

---

## 1. Wat is dit?

**LMSDataExtraction** is de *ontsluitingslaag* van het project: het centrale punt dat data ophaalt bij bronsystemen en beschikbaar stelt aan aanvragende componenten. De laag werkt **on demand** (geen achtergrond-synchronisatie) en bewaart een kopie van opgehaalde Canvas-data in een eigen **PostgreSQL**-database.

In één zin: *een veilige, gelaagde REST API die Canvas-data normaliseert en aanvult met (voorlopig gemockte) data uit Portflow, FeedPulse en de Competence Tool.*

---

## 2. Projectcontext

FHICT gebruikt **Canvas LMS** als centraal onderwijsplatform. Aan Canvas zijn via LTI externe modules gekoppeld: **Portflow** (portfolio), **FeedPulse** (peer feedback) en de **Competence Tool** (competentievoortgang). De challenge *"Bouw je eigen LMS"* bouwt een alternatieve omgeving die data uit al deze bronnen samenvoegt en via een uniforme API aanbiedt.

```
                 ┌──────────────────────────────────────────┐
  Frontend ─────►│                                          │──► Canvas LMS (live)
  Chatbot  ─────►│   LMSDataExtraction (ontsluitingslaag)   │──► Portflow      (mock)
  Planning ─────►│   ASP.NET Core 8  +  PostgreSQL          │──► FeedPulse     (mock)
                 │                                          │──► Competence    (mock)
                 └──────────────────────────────────────────┘
```

---

## 3. Eisen

> De volledige, formele requirements staan in het **Functioneel Ontwerp (FO_Dataontsluiting)**. Onderstaande lijst vat de eisen samen die de huidige implementatie afdekt.

### 3.1 Functionele eisen

| # | Eis | Status |
|---|---|---|
| FR-1 | Cursussen, modules, opdrachten en activiteiten ophalen uit Canvas. | ✅ |
| FR-2 | Inleveringen, cijfers, aankondigingen, leerdoelen (outcomes) en peer reviews ophalen uit Canvas. | ✅ |
| FR-3 | Opgehaalde Canvas-data persisteren in een eigen relationele database (deduplicatie op `CanvasId`). | ✅ |
| FR-4 | Alle data ontsluiten via één uniforme REST API onder `/api/v1/`. | ✅ |
| FR-5 | Endpoints aanbieden voor de externe modules Portflow, FeedPulse en Competence Tool. | ✅ (in-memory mock) |
| FR-6 | Authenticatie per gebruiker via een Canvas Bearer Token. | ✅ |
| FR-7 | Zelf-documenterende API via Swagger/OpenAPI. | ✅ |

### 3.2 Niet-functionele eisen

| # | Eis | Invulling |
|---|---|---|
| NFR-1 | **Onderhoudbaarheid** — duidelijke scheiding van verantwoordelijkheden. | Clean Architecture (5 projecten). |
| NFR-2 | **Vervangbaarheid** — bronnen omwisselbaar zonder andere lagen te raken. | Alles achter interfaces (`I*Source`, `I*Repository`, `ICanvasService`). |
| NFR-3 | **Testbaarheid / kwaliteit** — geautomatiseerde tests. | xUnit + CI op elke push/PR. |
| NFR-4 | **Betrouwbare deployment** — geen falende build naar productie. | CI/CD met test-gate vóór deploy. |
| NFR-5 | **Performance / rate-limiting Canvas** — onnodige externe calls vermijden. | In-memory caching (30s TTL) per token. |
| NFR-6 | **Veiligheid** — tokens nooit hardcoded; per request aangeleverd. | Bearer-token middleware; geen token in de repo. |
| NFR-7 | **Reproduceerbare omgeving** — iedereen draait dezelfde stack. | Docker + docker-compose + multi-stage image. |

---

## 4. Architectuur

De applicatie volgt **Clean Architecture**. Afhankelijkheden wijzen altijd naar binnen: Domain weet niets van database of API, Application weet niets van Infrastructure.

```
┌─────────────────────────────────────────────────────────┐
│  Api (presentatielaag)                                   │  Controllers · Middleware · Swagger · DI
├─────────────────────────────────────────────────────────┤
│  Application (businesslaag)                              │  Interfaces · DTO's · Mapping
├─────────────────────────────────────────────────────────┤
│  Domain (kern)                                           │  Entities (pure C#)
├─────────────────────────────────────────────────────────┤
│  Infrastructure (datalaag)                              │  CanvasService (HttpClient) · EF Core · Mocks
└─────────────────────────────────────────────────────────┘

Afhankelijkheidsrichting:   Api ──► Application ──► Domain ◄── Infrastructure
```

**Request-flow** (bijv. `GET /api/v1/Course`):

```
Client ──(Bearer token)──► BearerTokenMiddleware ──► Controller
                                                        │
                                 ┌──────────────────────┴───────────────┐
                                 ▼                                       ▼
                         CanvasService (cache → Canvas REST)     Repository (EF → PostgreSQL)
                                 │                                       │
                                 └──────────────► Mapping ──► JSON-response (DTO)
```

Voor de architectuurdocumentatie is het **C4-model** gebruikt (Context- en Container-niveau); de diagrammen (Word + draw.io-bron) staan in de repository. Zie ook [`ANALYSE.md`](ANALYSE.md) voor een laag-voor-laag uitleg.

---

## 5. Technologiestack

| Onderdeel | Keuze |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Taal | C# 12 (.NET 8, `net8.0`) |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core 9 + Npgsql (`Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4) |
| Caching | `Microsoft.Extensions.Caching.Memory` (IMemoryCache) |
| API-docs | Swagger / OpenAPI 3 (Swashbuckle 6.4.0) |
| Tests | xUnit 2.6 |
| Container | Docker (multi-stage build) |
| Registry | GitHub Container Registry (ghcr.io) |
| Hosting | Azure App Service (Web App for Containers) |
| Database (prod) | Azure Database for PostgreSQL Flexible Server |
| CI/CD | GitHub Actions |

---

## 6. Projectstructuur

```
LMSDataExtraction/
├── LMSDataExtraction.Api/            # Presentatielaag
│   ├── Controllers/                  # 12 controllers (Canvas + mock)
│   ├── Middleware/BearerTokenMiddleware.cs
│   ├── Program.cs                    # DI, pipeline, EnsureCreated()
│   ├── appsettings.json              # Canvas:BaseUrl + lokale connection string
│   └── appsettings.Development.json
├── LMSDataExtraction.Application/    # Businesslaag
│   ├── Interfaces/                   # ICanvasService, I*Repository, I*Source, I*Store
│   ├── Dtos/                         # Canvas*Dto, *ResponseDto, Competences/, FeedPulse/
│   └── Mapping/                      # Canvas-DTO → response-DTO
├── LMSDataExtraction.Domain/         # Kern (pure entities)
│   └── Entities/                     # Course, Module, Assignment, User, Activity, + mock-entiteiten
├── LMSDataExtraction.Infrastructure/ # Datalaag
│   ├── Canvas/CanvasService.cs       # HttpClient + caching naar Canvas REST API
│   ├── Persistence/                  # AppDbContext + repositories
│   ├── Migrations/                   # EF Core migraties
│   └── Sources/                      # Portflow/FeedPulse/Competence mocks
├── LMSDataExtraction.Tests/          # xUnit tests
├── .github/workflows/                # ci.yml, docker-publish.yml
├── docker/                           # init-scripts voor lokale Postgres
├── Dockerfile                        # Multi-stage build
├── docker-compose.yml                # Lokale PostgreSQL
├── DOCUMENTATIE.md                   # Uitgebreide technische documentatie
├── ANALYSE.md                        # Laag-voor-laag analyse
└── DATABASE.md                       # Database-snelstart
```

---

## 7. Snel starten (lokaal)

### Vereisten
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Een **Canvas API Token** (zie [Authenticatie](#10-authenticatie))

### Stappen

```bash
# 1. Clone
git clone https://github.com/Whelmand/LMSDataExtraction.git
cd LMSDataExtraction

# 2. Start de PostgreSQL-database (Docker)
docker-compose up -d

# 3. Start de API
dotnet run --project LMSDataExtraction.Api
```

De standaard connection string in `appsettings.json` wijst al naar de Docker-database (`localhost:5433`), dus je hoeft niets aan te passen. De API start op **http://localhost:5106** en opent Swagger op **http://localhost:5106/swagger**.

> 💡 Bij het opstarten maakt `EnsureCreated()` de databasetabellen automatisch aan als ze nog niet bestaan.

---

## 8. Datamodel

### Canvas-entiteiten

| Entiteit | Belangrijkste velden |
|---|---|
| User | Id, CanvasId, Name, Email, Role |
| Course | Id, CanvasId, Name, Description |
| Module | Id, CanvasId, CourseId, Name, Position |
| Assignment | Id, CanvasId, CourseId, Name, DueDate, MaxScore, ModuleId |
| Activity | Id, UserId, CourseId, SourceType, SourceId, Score, CompletedAt, AssignmentId |

Elke entiteit heeft twee ID-velden: **`Id`** (primaire sleutel in de eigen database) en **`CanvasId`** (het nummer dat Canvas zelf gebruikt). `Id` voor interne relaties, `CanvasId` voor communicatie met de Canvas API en deduplicatie.

### Mock-entiteiten

| Entiteit | Bron |
|---|---|
| Competence | Competence Tool (mock) |
| Feedback | FeedPulse (mock) |
| LearningGoal | Portflow (mock) |
| Review | Portflow (mock) |
| Snapshot | Portflow (mock) |

---

## 9. API-endpoints

Basispad: **`/api/v1/`**. Alle endpoints vereisen een geldig Canvas Bearer Token (behalve `/swagger`). Routes zijn hoofdletterongevoelig. Volledige request/response-schema's staan in **Swagger**.

### Canvas-endpoints (live data uit Canvas)

| Methode | Endpoint | Omschrijving |
|---|---|---|
| GET | `/api/v1/Course` | Alle cursussen van de ingelogde gebruiker (en opslaan in DB). |
| GET | `/api/v1/Module/{courseCanvasId}` | Modules van een cursus. |
| GET | `/api/v1/Assignment/{courseCanvasId}` | Opdrachten van een cursus. |
| GET | `/api/v1/Activity/{courseCanvasId}` | Activiteiten (submissions) gekoppeld aan User en Course. |
| GET | `/api/v1/Submission/{courseCanvasId}` | Alle eigen submissions van een cursus. |
| GET | `/api/v1/Submission/{courseCanvasId}/{assignmentCanvasId}` | Submissions voor één opdracht. |
| GET | `/api/v1/Announcement/{courseCanvasId}` | Aankondigingen van een cursus. |
| GET | `/api/v1/Grading/{courseCanvasId}` | Cijfers van de eigen submissions. |
| GET | `/api/v1/Outcome/{courseCanvasId}` | Leerdoelen (outcomes) van een cursus. |
| GET | `/api/v1/Outcome/{courseCanvasId}/groups` | Outcome-groepen (indeling van leerdoelen). |
| GET | `/api/v1/PeerReview/{courseCanvasId}/{assignmentCanvasId}` | Peer reviews voor een opdracht. |

### Mock-endpoints (in-memory, geen Canvas-token nodig, reset bij herstart)

| Methode | Endpoint | Omschrijving |
|---|---|---|
| GET | `/api/v1/Portflow/learninggoals` | Gemockte leerdoelen uit Portflow. |
| GET | `/api/v1/Competences` | Huidige HBO-i competentievoortgang. |
| GET | `/api/v1/Competences/framework` | Statische beschrijving van het HBO-i raamwerk. |
| PUT | `/api/v1/Competences` | Voortgang van één HBO-i cel bijwerken (upsert). |
| GET | `/api/v1/FeedPulse/me` | Volledig overzicht: student + alle checkpoints. |
| GET | `/api/v1/FeedPulse/checkpoints` | Lijst van alle checkpoints. |
| GET | `/api/v1/FeedPulse/checkpoints/{id}` | Eén specifiek checkpoint. |
| POST | `/api/v1/FeedPulse/checkpoints/{id}/response` | Mock-writeback: student plaatst reactie op coachfeedback. |

### Aanbevolen aanroepvolgorde
`Course` → daarna `Module` / `Assignment` / `Activity` / `Submission` / … met de `CanvasId` uit de cursuslijst.

---

## 10. Authenticatie

De API gebruikt **Canvas API Tokens** via een custom `BearerTokenMiddleware`. Bij elk request controleert de middleware op aanwezigheid en vorm van de header:

```
Authorization: Bearer <jouw_canvas_api_token>
```

- De middleware controleert alleen de **vorm** van het token, niet de geldigheid bij Canvas. De inhoud wordt impliciet gevalideerd zodra `CanvasService` ermee naar Canvas belt.
- `/swagger` en `/health` zijn vrijgesteld van de tokencheck.
- Het token wordt **nooit** opgeslagen in de codebase — het wordt per request meegestuurd.

| Situatie | Status | Response |
|---|---|---|
| Geen Authorization-header | 401 | `Authorization header ontbreekt.` |
| Begint niet met `Bearer ` | 401 | `Authorization header moet beginnen met 'Bearer '.` |
| Leeg token na `Bearer ` | 401 | `Bearer token is leeg.` |

### Canvas API Token aanmaken
1. Log in op Canvas (`https://fhict.instructure.com`).
2. **Account → Instellingen**.
3. Onder **Goedgekeurde integraties** → **+ Nieuw toegangstoken**.
4. Kopieer het token (eenmalig zichtbaar).

### Gebruik in Swagger
Klik op **Authorize 🔒**, plak het token (zonder `Bearer `-prefix), **Authorize → Close**, en gebruik **Try it out**.

---

## 11. Caching

`CanvasService` cachet elke succesvolle Canvas-respons in het geheugen (`IMemoryCache`):

- **TTL:** 30 seconden · **HTTP-timeout:** 10 seconden.
- **Cache-sleutel bevat het Bearer Token**, zodat data nooit tussen gebruikers wordt gedeeld (sleutels zoals `canvas:courses:<token>`, `canvas:modules:<token>:<courseId>`).
- Doel: Canvas-rate-limits ontzien en herhaalde calls (bijv. tijdens een Swagger-demo) versnellen.

---

## 12. Database & migraties

### Lokaal (Docker)
`docker-compose up -d` start een PostgreSQL 16-container:

| Instelling | Waarde |
|---|---|
| Image | `postgres:16` |
| Containernaam | `lms_database` |
| Database | `lmsdataextraction` |
| Host-poort | **5433** (5432 is vaak al in gebruik door een lokale PostgreSQL) |
| Gebruiker / wachtwoord | `postgres` / `postgres` |
| Volume | `lms_data` (persistente opslag) |

### Schema aanmaken: EnsureCreated() vs. migraties
- **Runtime:** `Program.cs` roept bij het opstarten `db.Database.EnsureCreated()` aan. Dat maakt ontbrekende tabellen aan, maar past **geen** schemawijzigingen toe op een bestaande database.
- **In de repo** staan daarnaast **3 EF Core-migraties** (voor lokale schema-evolutie):
  1. `InitialCreate` — de vijf Canvas-tabellen (Users, Courses, Modules, Assignments, Activities).
  2. `AddMockDataSources` — de mock-tabellen (Competences, Feedback, LearningGoals, Reviews, Snapshots).
  3. `AddModuleIdToAssignment_AddAssignmentIdToActivity` — kolommen `ModuleId` (Assignment) en `AssignmentId` (Activity).

> ⚠️ `EnsureCreated()` en migraties gaan niet goed samen. Voor structurele schema-updates is overstappen op `Database.Migrate()` aanbevolen (zie [beperkingen](#19-bekende-beperkingen--aanbevelingen)).

Migraties aanmaken/toepassen:
```bash
dotnet ef migrations add <naam> --project LMSDataExtraction.Infrastructure --startup-project LMSDataExtraction.Api
dotnet ef database update          --project LMSDataExtraction.Infrastructure --startup-project LMSDataExtraction.Api
```

---

## 13. Databronnen en status

| Bron | Koppeling | Status | Aantekening |
|---|---|---|---|
| Canvas LMS | REST API | **Live** | Per-gebruiker Bearer Token. Base URL: `https://fhict.instructure.com/api/v1`. |
| Portflow | REST API (OAuth 2.0) | Mock | API bestaat, maar er is momenteel geen geldig token van Drieam. In-memory mock. |
| FeedPulse | Geen publieke API | Mock | In-memory mockdata; reset bij herstart van de Web App. |
| Competence Tool | Geen publieke API | Mock | In-memory mockdata; reset bij herstart van de Web App. |

> De mock-tabellen bestaan wél in de database (via migratie `AddMockDataSources`), maar de actieve mock-endpoints leveren **in-memory** data en gebruiken die tabellen niet.

---

## 14. Testen

```bash
dotnet test
```

- **16 unit tests** (xUnit) in `LMSDataExtraction.Tests`.
- Dekking: entiteiten (Course, Assignment, User, Module, Feedback), `CanvasCourseDto`-mapping en `FeedPulseFeedbackMockStore`-gedrag (checkpoints, vergrendelde vs. open responses).
- Tests draaien automatisch in CI bij elke push/PR.

---

## 15. Docker

Multi-stage build (`Dockerfile`):
- **Stage 1 (`sdk:8.0`)** — restore + publish (Release).
- **Stage 2 (`aspnet:8.0`)** — alleen de runtime; exposeert poort **8080**.

```bash
# Image bouwen
docker build -t lmsdataextraction:local .

# Container draaien (verbind met lokale Postgres via host.docker.internal)
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5433;Database=lmsdataextraction;Username=postgres;Password=postgres" \
  lmsdataextraction:local
```

Gepubliceerde images: `ghcr.io/whelmand/lmsdataextraction:{latest|main|sha-<hash>}`.

---

## 16. CI/CD-pipeline

Twee GitHub Actions-workflows:

### `ci.yml` — Build & Test
- **Trigger:** push én pull request naar `main`.
- Stappen: checkout → setup .NET 8 → restore → build (Release) → test → testrapport (`.trx`) als artifact uploaden.

### `docker-publish.yml` — Build, Publish & Deploy
- **Trigger:** push naar `main`.
- **Job `test`:** restore → build → test.
- **Job `build-and-push`** (`needs: test`, draait dus alleen als tests slagen):
  1. Login bij GHCR (automatische `GITHUB_TOKEN`).
  2. Docker-metadata/tags (`latest`, `main`, `sha-<hash>`).
  3. Image bouwen + pushen naar GHCR.
  4. Login bij Azure (`AZURE_CREDENTIALS` service principal).
  5. `az webapp restart` → de Web App haalt het nieuwe `:latest` image op.

> ✅ De deploy is **gegate op tests**: een versie met falende tests komt niet in productie. Doorlooptijd ≈ 1–2 minuten.

**Benodigde GitHub Secret:** `AZURE_CREDENTIALS` (service principal JSON met contributor-rechten op de resource group).

---

## 17. Omgevingen & deployment

| Omgeving | Beschrijving |
|---|---|
| **Lokaal (dev)** | Visual Studio / Rider / `dotnet run`; PostgreSQL via Docker (poort 5433). |
| **Docker (team)** | `docker-compose up -d` voor een gedeelde, identieke database. |
| **Productie (Azure)** | Web App for Containers, image via GHCR, deploy via de CI/CD-pipeline. |

**Azure-infrastructuur** (zie [`DOCUMENTATIE.md`](DOCUMENTATIE.md) voor details):

| Resource | Waarde |
|---|---|
| Web App | `lmsdataextraction-api` (Spain Central, Linux, poort 8080) |
| Container image | `ghcr.io/whelmand/lmsdataextraction:latest` |
| PostgreSQL | `lmsdataextraction-db` (Flexible Server, B1ms, PostgreSQL 16) |
| Database (prod) | `lmsdata` |

Bij elke deploy controleert `EnsureCreated()` of de tabellen bestaan en maakt ze aan indien nodig.

---

## 18. Configuratie & omgevingsvariabelen

| Variabele | Lokaal | Productie (Azure) | Beschrijving |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Development | Production | Omgeving |
| `ASPNETCORE_URLS` | http://localhost:5106 | http://+:8080 | Luisteradres/poort |
| `ConnectionStrings__DefaultConnection` | localhost:5433 | Azure PostgreSQL | PostgreSQL-verbindingsstring |
| `Canvas__BaseUrl` | `https://fhict.instructure.com/api/v1` | (zelfde) | Canvas API base-URL |

> 💡 In Docker-containers op Azure is een **App-instelling** `ConnectionStrings__DefaultConnection` (dubbele underscore) de betrouwbaarste manier om de connection string door te geven; .NET vertaalt dit naar `ConnectionStrings:DefaultConnection` en overschrijft `appsettings.json`.

---

## 19. Bekende beperkingen & aanbevelingen

| Onderwerp | Toelichting / aanbeveling |
|---|---|
| **Echte koppelingen** | Portflow, FeedPulse en Competence Tool zijn mock. Bouw echte adapters achter dezelfde interfaces zodra koppelingen/tokens beschikbaar zijn. |
| **Token-beheer** | Elke gebruiker voert handmatig een Bearer Token in. Overweeg OAuth 2.0 Authorization Code Flow met centrale tokenopslag + refresh. |
| **`EnsureCreated()` vs. migraties** | Runtime gebruikt `EnsureCreated()` (geen schema-updates). Stap over op `Database.Migrate()` voor structurele wijzigingen. |
| **Mock-tabellen opschonen** | De mock-endpoints zijn in-memory; de DB-tabellen uit `AddMockDataSources` worden niet gebruikt. Koppel de DB-bronnen of verwijder de ongebruikte tabellen. |
| **Geen globale exception handler** | Onverwachte fouten geven een kale 500. Voeg `UseExceptionHandler()` / ProblemDetails toe. |
| **Geen rate-limiting** | Een geldig token kan Canvas onbedoeld overbelasten. Overweeg `Microsoft.AspNetCore.RateLimiting`. |
| **Health endpoint** | `/health` is vrijgesteld in de middleware, maar er is nog geen `MapHealthChecks("/health")`. |
| **Testdekking** | Integratietests voor de Canvas-koppeling en repository-laag ontbreken nog. |
| **Swagger-documentatie** | Request/response-voorbeelden en foutcodes nog niet volledig beschreven. |

Zie [`ANALYSE.md` §13](ANALYSE.md) voor de uitgebreide risico-analyse.

---

## 20. Projectteam

| Naam | Rol |
|---|---|
| Wares Helmand | Dataontsluiting (projectstructuur, middleware, caching, mocks, tests, CI/CD) |
| Tamara Lemmens | Dataontsluiting (entiteiten, DBContext, Canvas-endpoints, persistentie) |
| Jan (chatbot-team) | Chatbot Service + frontend |
| Tijn (chatbot-team) | Chatbot Service + frontend |

---

## 21. Verdere documentatie

| Document | Inhoud |
|---|---|
| [`DOCUMENTATIE.md`](DOCUMENTATIE.md) | Uitgebreide technische documentatie (endpoints, Azure, CI/CD, changelog). |
| [`ANALYSE.md`](ANALYSE.md) | Laag-voor-laag analyse van code, DTO's, interfaces, middleware en pipeline. |
| [`DATABASE.md`](DATABASE.md) | Snelstart database & tabeloverzicht. |
| Overdrachtsdocument LMS Dataontsluiting | Formele overdracht: architectuur, keuzes, openstaande punten. |
| FO_Dataontsluiting | Functioneel ontwerp incl. eisen, aannames en risico's. |
| C4 Context- & Container-diagram | Architectuurdiagrammen (Word + draw.io-bron). |

---

| Resource | Link |
|---|---|
| Repository | https://github.com/Whelmand/LMSDataExtraction |
| Live API | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net |
| Swagger UI | https://lmsdataextraction-api-bbfehtfvhvd9e8a4.spaincentral-01.azurewebsites.net/swagger |
| Docker image | https://github.com/Whelmand/LMSDataExtraction/pkgs/container/lmsdataextraction |
| GitHub Actions | https://github.com/Whelmand/LMSDataExtraction/actions |
| Canvas API-docs | https://canvas.instructure.com/doc/api/ |
