IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [Movies] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [DurationMinutes] int NOT NULL,
        [IsPremium] bit NOT NULL,
        CONSTRAINT [PK_Movies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [Screens] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Capacity] int NOT NULL,
        CONSTRAINT [PK_Screens] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [Showings] (
        [Id] int NOT NULL IDENTITY,
        [MovieId] int NOT NULL,
        [ScreenId] int NOT NULL,
        [StartTime] datetime2 NOT NULL,
        [EndTime] datetime2 NOT NULL,
        [BasePrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Showings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Showings_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Showings_Screens_ScreenId] FOREIGN KEY ([ScreenId]) REFERENCES [Screens] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [Bookings] (
        [Id] int NOT NULL IDENTITY,
        [ShowingId] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Bookings_Showings_ShowingId] FOREIGN KEY ([ShowingId]) REFERENCES [Showings] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE TABLE [BookedTickets] (
        [Id] uniqueidentifier NOT NULL,
        [BookingId] int NOT NULL,
        [TicketType] nvarchar(20) NOT NULL,
        [PricePaid] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_BookedTickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BookedTickets_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_BookedTickets_BookingId] ON [BookedTickets] ([BookingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_Bookings_ShowingId] ON [Bookings] ([ShowingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_Showings_MovieId] ON [Showings] ([MovieId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    CREATE INDEX [IX_Showings_ScreenId_StartTime] ON [Showings] ([ScreenId], [StartTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123195124_InitialSqlServerSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123195124_InitialSqlServerSchema', N'10.0.10');
END;

COMMIT;
GO

