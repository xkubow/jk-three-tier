CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS "Configuration" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Key" varchar(200) NOT NULL,
    "Value" varchar(2000) NOT NULL
);

INSERT INTO "Configuration" ("Key", "Value")
SELECT 'HelloWorld', 'Hello from DB'
WHERE NOT EXISTS (
    SELECT 1 FROM "Configuration" WHERE "Key" = 'HelloWorld'
);