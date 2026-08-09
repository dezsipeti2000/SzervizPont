-- SzervizPont adatbázis szerkezete
-- Az alkalmazás az adatbázist az Entity Framework Core segítségével automatikusan létrehozza.

CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" TEXT NOT NULL
);

CREATE TABLE "Services" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Services" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Price" INTEGER NOT NULL,
    "EstimatedMinutes" INTEGER NOT NULL
);

CREATE TABLE "Cars" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Cars" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "PlateNumber" TEXT NOT NULL,
    "Brand" TEXT NOT NULL,
    "Model" TEXT NOT NULL,
    "Year" INTEGER NOT NULL,
    CONSTRAINT "FK_Cars_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Appointments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Appointments" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "CarId" INTEGER NOT NULL,
    "ServiceItemId" INTEGER NOT NULL,
    "StartAt" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "Note" TEXT NULL,
    CONSTRAINT "FK_Appointments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Appointments_Cars_CarId" FOREIGN KEY ("CarId") REFERENCES "Cars" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Appointments_Services_ServiceItemId" FOREIGN KEY ("ServiceItemId") REFERENCES "Services" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX "IX_Cars_UserId" ON "Cars" ("UserId");
CREATE UNIQUE INDEX "IX_Cars_PlateNumber" ON "Cars" ("PlateNumber");
CREATE INDEX "IX_Appointments_UserId" ON "Appointments" ("UserId");
CREATE INDEX "IX_Appointments_CarId" ON "Appointments" ("CarId");
CREATE INDEX "IX_Appointments_ServiceItemId" ON "Appointments" ("ServiceItemId");
