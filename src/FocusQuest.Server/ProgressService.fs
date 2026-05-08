module FocusQuest.Server.ProgressService

open System
open Microsoft.Data.Sqlite
open FocusQuest.Server.Database

let saveQuestHistory title xpEarned =
    use connection = new SqliteConnection(connectionString)
    connection.Open()

    use command = connection.CreateCommand()
    command.CommandText <- """
        INSERT INTO QuestHistory (Title, XpEarned, CompletedAt)
        VALUES ($title, $xpEarned, $completedAt)
    """

    command.Parameters.AddWithValue("$title", title) |> ignore
    command.Parameters.AddWithValue("$xpEarned", xpEarned) |> ignore
    command.Parameters.AddWithValue("$completedAt", DateTime.UtcNow.ToString("o")) |> ignore

    command.ExecuteNonQuery() |> ignore