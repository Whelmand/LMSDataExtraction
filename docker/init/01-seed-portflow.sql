-- Mockdata voor de integratielaag.
-- Dit bestand wordt door de Postgres-container automatisch uitgevoerd
-- bij de eerste start (als het data-volume leeg is).
--
-- LET OP: tabellen bestaan op dit moment nog NIET. EF Core maakt ze pas
-- aan via de migrations zodra de API voor het eerst start. Dit script
-- wacht daarop met een DO-block dat alleen invoegt als de tabel bestaat.

-- ──────────────────────────────────────────────────────────────────────
-- Portflow: LearningGoals
-- ──────────────────────────────────────────────────────────────────────
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_name = 'LearningGoals'
    ) THEN
        INSERT INTO "LearningGoals" ("PortflowUuid", "Name", "Nickname", "SourcedId", "CreatedAt")
        VALUES
            (gen_random_uuid(), 'Software ontwerpen',          'Ontwerp',   'sis-101', NOW()),
            (gen_random_uuid(), 'Testautomatisering opzetten', 'Testen',    'sis-102', NOW()),
            (gen_random_uuid(), 'Stakeholders interviewen',    'Interview', 'sis-103', NOW());
    END IF;
END
$$;

-- ──────────────────────────────────────────────────────────────────────
-- Portflow: Reviews
-- ──────────────────────────────────────────────────────────────────────
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_name = 'Reviews'
    ) THEN
        INSERT INTO "Reviews" ("ReviewRequestId", "ReviewerId", "Score", "ReviewerRole", "SubmittedAt")
        VALUES
            (gen_random_uuid(), gen_random_uuid(), 7.5, 'student', NOW()),
            (gen_random_uuid(), gen_random_uuid(), 8.0, 'docent',  NOW());
    END IF;
END
$$;

-- ──────────────────────────────────────────────────────────────────────
-- Portflow: Snapshots
-- ──────────────────────────────────────────────────────────────────────
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_name = 'Snapshots'
    ) THEN
        INSERT INTO "Snapshots" ("PortflowUuid", "UserUuid", "AssessmentUrl", "AssessmentPassword", "LtiSubmissionCount", "CreatedAt")
        VALUES
            (gen_random_uuid(), gen_random_uuid(), 'https://assess.example/abc', 'pw-abc', 2, NOW()),
            (gen_random_uuid(), gen_random_uuid(), 'https://assess.example/xyz', 'pw-xyz', 5, NOW());
    END IF;
END
$$;
