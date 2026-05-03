-- Athenaeum Dynamic Seed Data Script (PostgreSQL)
-- IMPORTANT: Register your user in the UI first, then run this script.

DO $$ 
DECLARE 
    v_user_id UUID;
    v_email TEXT := 'akash@gmail.com'; 
BEGIN
    -- 1. Identify the Target User
    SELECT "Id" INTO v_user_id FROM "Users" WHERE "Email" = v_email;

    IF v_user_id IS NULL THEN
        RAISE NOTICE 'User % not found. Please register in the UI first.', v_email;
    ELSE
        -- Clear existing sample data for this user to avoid duplicates
        DELETE FROM "Books" WHERE "UserId" = v_user_id;
        DELETE FROM "ReadingGoals" WHERE "UserId" = v_user_id;

        -- 2. Catalog the Curated Collection (Books)
        -- Currently Reading (Status: 1)
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "Genre", "PublicationYear", "Status", "CurrentPage", "TotalPages", "Rating", "Review", "CoverImageUrl", "UserId", "CreatedAt", "UpdatedAt")
        VALUES (gen_random_uuid(), 'The Architecture of Silence', 'Elena Thorne', '978-0143125471', 'Psychology', 2021, 1, 142, 350, NULL, 'A profound exploration of monastic spaces and the psychological impact of intentional quietude in the modern era.', '/covers/architecture.png', v_user_id, NOW(), NOW());

        -- To Read Next (Status: 0)
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "Genre", "PublicationYear", "Status", "CurrentPage", "TotalPages", "Rating", "Review", "CoverImageUrl", "UserId", "CreatedAt", "UpdatedAt")
        VALUES 
        (gen_random_uuid(), 'Urban Solitude', 'Julian Vane', '978-0374280598', 'Sociology', 2023, 0, 0, 280, NULL, NULL, '/covers/solitude.png', v_user_id, NOW(), NOW()),
        (gen_random_uuid(), 'The Ethics of Aesthetics', 'Marcus Thorne', '978-0199673988', 'Philosophy', 2019, 0, 0, 420, NULL, NULL, '/covers/ethics.png', v_user_id, NOW(), NOW());

        -- Recently Finished (Status: 2)
        INSERT INTO "Books" ("Id", "Title", "Author", "ISBN", "Genre", "PublicationYear", "Status", "CurrentPage", "TotalPages", "Rating", "Review", "CoverImageUrl", "UserId", "CreatedAt", "UpdatedAt")
        VALUES 
        (gen_random_uuid(), 'Midnight in the Garden', 'Sarah Moore', '978-0679751526', 'Memoir', 1994, 2, 388, 388, 5, 'A masterpiece of atmospheric storytelling. The descriptive prose regarding the archival vaults is unparalleled.', '/covers/midnight.png', v_user_id, NOW() - INTERVAL '10 days', NOW() - INTERVAL '10 days'),
        (gen_random_uuid(), 'The Paper Trail', 'Oliver Reed', '978-1501166304', 'History', 2017, 2, 320, 320, 4, 'Excellent historical context on the evolution of private libraries.', '/covers/paper.png', v_user_id, NOW() - INTERVAL '20 days', NOW() - INTERVAL '20 days'),
        (gen_random_uuid(), 'Typography as Art', 'Inter Script', '978-0300222044', 'Design', 2015, 2, 210, 210, 5, 'A visual feast for anyone interested in the intersection of print and digital archives.', '/covers/typography.png', v_user_id, NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days');

        -- 3. Set the 2024 Reading Goal
        INSERT INTO "ReadingGoals" ("TargetYear", "GoalCount", "UserId")
        VALUES (2024, 24, v_user_id);
        
        RAISE NOTICE 'Success: Seed data mapped to User %', v_email;
    END IF;
END $$;
