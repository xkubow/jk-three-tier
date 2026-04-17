CREATE TABLE IF NOT EXISTS "ApiMessageTask" (
    "Id" VARCHAR(200) PRIMARY KEY,
    "TaskName" VARCHAR(200) NOT NULL,
    "TargetUrl" VARCHAR(2000) NOT NULL,
    "State" VARCHAR(50) NOT NULL,
    "Attempts" INT NOT NULL DEFAULT 0,
    "MaxAttempts" INT NOT NULL DEFAULT 5,
    "LastError" TEXT NULL,
    "CreatedOn" TIMESTAMP WITH TIME ZONE NOT NULL,
    "StartOn" TIMESTAMP WITH TIME ZONE NULL,
    "FinishOn" TIMESTAMP WITH TIME ZONE NULL,
    "NextRetryOn" TIMESTAMP WITH TIME ZONE NULL
);

CREATE INDEX IF NOT EXISTS "IX_ApiMessageTask_State" ON "ApiMessageTask" ("State");
CREATE INDEX IF NOT EXISTS "IX_ApiMessageTask_StartOn" ON "ApiMessageTask" ("StartOn");
CREATE INDEX IF NOT EXISTS "IX_ApiMessageTask_NextRetryOn" ON "ApiMessageTask" ("NextRetryOn");