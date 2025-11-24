
CREATE TABLE "ToDoUser" (
    "UserId" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL,
    "TelegramUserName" VARCHAR(255) NOT NULL,
    "RegisteredAt" TIMESTAMP NOT NULL
);

CREATE TABLE "ToDoList" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(10) NOT NULL,
    "UserId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL
);

CREATE TABLE "ToDoItem" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "State" INT NOT NULL,
    "StateChangedAt" TIMESTAMP,
    "Deadline" TIMESTAMP NOT NULL,
    "ListId" UUID
);

ALTER TABLE "ToDoList" 
ADD CONSTRAINT "FK_ToDoList_ToDoUser" 
FOREIGN KEY ("UserId") REFERENCES "ToDoUser"("UserId");

ALTER TABLE "ToDoItem" 
ADD CONSTRAINT "FK_ToDoItem_ToDoUser" 
FOREIGN KEY ("UserId") REFERENCES "ToDoUser"("UserId");

ALTER TABLE "ToDoItem" 
ADD CONSTRAINT "FK_ToDoItem_ToDoList" 
FOREIGN KEY ("ListId") REFERENCES "ToDoList"("Id");

CREATE INDEX "IX_ToDoList_UserId" ON "ToDoList" ("UserId");
CREATE INDEX "IX_ToDoItem_UserId" ON "ToDoItem" ("UserId");
CREATE INDEX "IX_ToDoItem_ListId" ON "ToDoItem" ("ListId");

CREATE UNIQUE INDEX "IX_ToDoUser_TelegramUserId" ON "ToDoUser" ("TelegramUserId");

CREATE INDEX "IX_ToDoItem_State" ON "ToDoItem" ("State");
CREATE INDEX "IX_ToDoItem_Deadline" ON "ToDoItem" ("Deadline");