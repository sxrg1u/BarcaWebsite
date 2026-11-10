-- Tabellen fuer die Aufstellungen (einmal in der Datenbank barcadata ausfuehren)
-- Jeder Benutzer kann mehrere Aufstellungen mit Namen speichern

CREATE TABLE IF NOT EXISTS Lineups (
    Id        INT         NOT NULL AUTO_INCREMENT PRIMARY KEY,
    UserId    INT         NOT NULL,
    Name      VARCHAR(40) NOT NULL,
    Formation VARCHAR(10) NOT NULL,
    CaptainId INT         NULL,
    UpdatedAt DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    KEY IX_Lineups_User (UserId),
    CONSTRAINT FK_Lineups_User    FOREIGN KEY (UserId)    REFERENCES Users (Id)   ON DELETE CASCADE,
    CONSTRAINT FK_Lineups_Captain FOREIGN KEY (CaptainId) REFERENCES Players (Id) ON DELETE SET NULL
);

-- Slot 0-10 = Startelf (Reihenfolge wie im System), Slot 11-19 = Ersatzbank
CREATE TABLE IF NOT EXISTS LineupPlayers (
    LineupId INT     NOT NULL,
    Slot     TINYINT NOT NULL,
    PlayerId INT     NOT NULL,
    PRIMARY KEY (LineupId, Slot),
    UNIQUE KEY UQ_LineupPlayers_Player (LineupId, PlayerId),
    CONSTRAINT FK_LineupPlayers_Lineup FOREIGN KEY (LineupId) REFERENCES Lineups (Id) ON DELETE CASCADE,
    CONSTRAINT FK_LineupPlayers_Player FOREIGN KEY (PlayerId) REFERENCES Players (Id) ON DELETE CASCADE
);
