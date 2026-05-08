module FocusQuest.Server.Database

open Microsoft.Data.Sqlite

let connectionString = "Data Source=focusquest.db"

let initializeDatabase () =
    use connection = new SqliteConnection(connectionString)
    connection.Open()

    let command = connection.CreateCommand()

    command.CommandText <- """
        CREATE TABLE IF NOT EXISTS PlayerProgress (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            PlayerName TEXT NOT NULL,
            XP INTEGER NOT NULL,
            Level INTEGER NOT NULL,
            Streak INTEGER NOT NULL,
            DailyChallengeCompleted INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS QuestHistory (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Title TEXT NOT NULL,
            XpEarned INTEGER NOT NULL,
            CompletedAt TEXT NOT NULL
        );
    """

    command.ExecuteNonQuery() |> ignore