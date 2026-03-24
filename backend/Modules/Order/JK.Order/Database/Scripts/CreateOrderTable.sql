-- Create the "Order" table based on OrderEntity.cs
-- This script is compatible with PostgreSQL

CREATE TABLE IF NOT EXISTS "Order" (
    "Id" UUID PRIMARY KEY,
    "Number" VARCHAR(100) NOT NULL,
    "TotalAmount" DECIMAL(18, 2) NOT NULL DEFAULT 0.0,
    "Status" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Optional: Add an index on the "Number" column since it's likely used for searches
CREATE INDEX IF NOT EXISTS "IX_Order_Number" ON "Order" ("Number");

-- Optional: Add a comment to the table
COMMENT ON TABLE "Order" IS 'Stores order information for the Order module';
