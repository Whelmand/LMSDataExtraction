# Database — LMS Dataontsluiting

## Verbinding maken

Zorg dat Docker Desktop geinstalleerd is en draai het volgende commando in de root van de repository:

    docker-compose up -d

Verbindingsgegevens:

    Host:     localhost
    Port:     5433
    Database: lmsdataextraction
    Username: postgres
    Password: postgres

## Tabellen

| Tabel       | Belangrijkste velden                                           |
|-------------|----------------------------------------------------------------|
| Users       | Id, CanvasId, Name, Email, Role                                |
| Courses     | Id, CanvasId, Name, Description                                |
| Modules     | Id, CanvasId, CourseId, Name, Position                         |
| Assignments | Id, CanvasId, CourseId, Name, DueDate, MaxScore                |
| Activities  | Id, UserId, CourseId, SourceType, SourceId, Score, CompletedAt |

## Opmerking

Elke tabel heeft een eigen Id (primary key) en een CanvasId.
Het CanvasId is het nummer dat Canvas LMS gebruikt voor hetzelfde object.
