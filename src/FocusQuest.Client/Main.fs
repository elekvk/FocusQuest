module FocusQuest.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/focus">] Focus
    | [<EndPoint "/stats">] Stats
    | [<EndPoint "/settings">] Settings

type Difficulty =
    | Easy
    | Medium
    | Hard

type Quest =
    {
        title: string
        duration: int
        difficulty: Difficulty
        completed: bool
    }

type Player =
    {
        name: string
        xp: int
        level: int
    }

type Achievement =
    {
        title: string
        description: string
        unlocked: bool
    }

type QuestHistory =
    {
        title: string
        xpEarned: int
        completedAt: DateTime
    }

type ProgressService =
    {
        saveQuestHistory: string * int -> Async<unit>
    }

    interface Bolero.Remoting.IRemoteService with
        member this.BasePath = "/progress"

type Model =
    {
        page: Page
        focusMinutes: int
        player: Player
        quests: Quest list
        achievements: Achievement list
        streak: int
        dailyChallengeCompleted: bool
        dailyChallengeRewardXp: int
        questHistory: QuestHistory list
        timerRunning: bool
        secondsLeft: int
        focusSessionCompleted: bool
    }

let initModel =
    {
        page = Home
        focusMinutes = 25
        player = { name = "Player1"; xp = 0; level = 1 }
        quests =
            [
                { title = "Study for 30 minutes"; duration = 30; difficulty = Easy; completed = false }
                { title = "Practice coding for 60 minutes"; duration = 60; difficulty = Medium; completed = false }
                { title = "Work on project for 45 minutes"; duration = 45; difficulty = Hard; completed = false }
            ]
        achievements =
            [
                { title = "First Quest"; description = "Complete your first quest."; unlocked = false }
                { title = "Level Up"; description = "Reach level 2."; unlocked = false }
                { title = "Focused Hero"; description = "Complete all daily quests."; unlocked = false }
            ]
        streak = 0
        dailyChallengeCompleted = false
        dailyChallengeRewardXp = 40
        questHistory = []
        timerRunning = false
        secondsLeft = 25 * 60
        focusSessionCompleted = false
    }

type Message =
    | SetPage of Page
    | IncreaseFocusTime
    | DecreaseFocusTime
    | CompleteQuest of string
    | CompleteDailyChallenge
    | CompleteFocusSession
    | StartFocusTimer
    | StopFocusTimer
    | Tick
    | ResetFocusTimer
    | ResetProgress

let timerCmd =
    Cmd.OfAsync.perform
        (fun () ->
            async {
                do! Async.Sleep 1000
            })
        ()
        (fun _ -> Tick)

let xpForDifficulty difficulty =
    match difficulty with
    | Easy -> 25
    | Medium -> 50
    | Hard -> 80

let updateAchievements player quests achievements =
    let completedCount =
        quests |> List.filter (fun q -> q.completed) |> List.length

    let allCompleted =
        completedCount = quests.Length

    achievements
    |> List.map (fun (a: Achievement) ->
        if a.title = "First Quest" && completedCount >= 1 then
            { a with unlocked = true }
        elif a.title = "Level Up" && player.level >= 2 then
            { a with unlocked = true }
        elif a.title = "Focused Hero" && allCompleted then
            { a with unlocked = true }
        else
            a)

let calculateLevel xp currentLevel =
    if xp >= currentLevel * 100 then currentLevel + 1
    else currentLevel

let update message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | IncreaseFocusTime ->
        { model with
            focusMinutes = model.focusMinutes + 5
            secondsLeft = (model.focusMinutes + 5) * 60 },
        Cmd.none

    | DecreaseFocusTime ->
        let newMinutes = max 5 (model.focusMinutes - 5)

        { model with
            focusMinutes = newMinutes
            secondsLeft = newMinutes * 60 },
        Cmd.none

    | CompleteQuest title ->
        let selectedQuest =
            model.quests |> List.tryFind (fun q -> q.title = title)

        let gainedXp =
            match selectedQuest with
            | Some q when not q.completed -> xpForDifficulty q.difficulty
            | _ -> 0

        let updatedQuests =
            model.quests
            |> List.map (fun q ->
                if q.title = title then { q with completed = true }
                else q)

        let newXp = model.player.xp + gainedXp
        let newLevel = calculateLevel newXp model.player.level

        let updatedPlayer =
            { model.player with xp = newXp; level = newLevel }

        let updatedAchievements =
            updateAchievements updatedPlayer updatedQuests model.achievements

        let updatedQuestHistory =
            if gainedXp > 0 then
                {
                    title = title
                    xpEarned = gainedXp
                    completedAt = DateTime.Now
                } :: model.questHistory
            else
                model.questHistory

        let newStreak =
            if gainedXp > 0 then model.streak + 1
            else model.streak

        { model with
            quests = updatedQuests
            player = updatedPlayer
            achievements = updatedAchievements
            streak = newStreak
            questHistory = updatedQuestHistory },
        Cmd.none

    | CompleteDailyChallenge ->
        let hasCompletedQuest =
            model.quests |> List.exists (fun q -> q.completed)

        if model.dailyChallengeCompleted || not hasCompletedQuest then
            model, Cmd.none
        else
            let newXp = model.player.xp + model.dailyChallengeRewardXp
            let newLevel = calculateLevel newXp model.player.level

            let updatedPlayer =
                { model.player with xp = newXp; level = newLevel }

            let updatedAchievements =
                updateAchievements updatedPlayer model.quests model.achievements

            { model with
                player = updatedPlayer
                achievements = updatedAchievements
                dailyChallengeCompleted = true },
            Cmd.none

    | StartFocusTimer ->
        let updatedSeconds =
            if model.secondsLeft <= 0 then model.focusMinutes * 60
            else model.secondsLeft

        { model with
            timerRunning = true
            focusSessionCompleted = false
            secondsLeft = updatedSeconds },
        timerCmd

    | StopFocusTimer ->
        { model with timerRunning = false }, Cmd.none

    | Tick ->
        if model.timerRunning && model.secondsLeft > 1 then
            { model with secondsLeft = model.secondsLeft - 1 },
            timerCmd
        elif model.timerRunning && model.secondsLeft <= 1 then
            { model with
                timerRunning = false
                secondsLeft = 0 },
            Cmd.ofMsg CompleteFocusSession
        else
            model, Cmd.none

    | CompleteFocusSession ->
        let gainedXp = 50
        let newXp = model.player.xp + gainedXp
        let newLevel = calculateLevel newXp model.player.level

        let updatedPlayer =
            { model.player with xp = newXp; level = newLevel }

        let updatedHistory =
            {
                title = "Focus Session"
                xpEarned = gainedXp
                completedAt = DateTime.Now
            } :: model.questHistory

        { model with
            player = updatedPlayer
            questHistory = updatedHistory
            focusSessionCompleted = true },
        Cmd.none

    | ResetFocusTimer ->
        { model with
            timerRunning = false
            secondsLeft = model.focusMinutes * 60
            focusSessionCompleted = false },
        Cmd.none

    | ResetProgress ->
        initModel, Cmd.none

let router = Router.infer SetPage (fun model -> model.page)

let buttonStyle active =
    if active then
        "margin-right:10px; padding:10px 16px; border-radius:10px; border:none; background:#2563eb; color:white; cursor:pointer; font-weight:700;"
    else
        "margin-right:10px; padding:10px 16px; border-radius:10px; border:none; background:#1e293b; color:white; cursor:pointer; font-weight:700;"

let statBox title value =
    div {
        attr.style "background:#1e293b; border:1px solid #334155; border-radius:16px; padding:20px; box-shadow:0 10px 30px rgba(0,0,0,0.35);"

        h3 {
            attr.style "color:#38bdf8; margin-top:0;"
            text title
        }

        p {
            attr.style "font-size:26px; font-weight:800; margin-bottom:0;"
            text value
        }
    }

let homePage model dispatch =
    let requiredXp = model.player.level * 100

    let hasCompletedQuest =
        model.quests |> List.exists (fun q -> q.completed)

    let xpPercent =
        (float model.player.xp / float requiredXp) * 100.0
        |> min 100.0

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:48px; font-weight:900; color:#38bdf8; margin-bottom:5px;"
            text "FocusQuest"
        }

        h2 {
            attr.style "font-size:22px; color:#c084fc; margin-top:0; margin-bottom:30px;"
            text "Level Up Your Focus"
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(3, minmax(0, 1fr)); gap:18px; margin-bottom:24px;"
            statBox "Player" model.player.name
            statBox "Level" (string model.player.level)
            statBox "XP" (string model.player.xp + " / " + string requiredXp)
        }

        div {
            attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px; margin-bottom:24px;"

            h3 {
                attr.style "margin-top:0; color:#facc15;"
                text "XP Progress"
            }

            div {
                attr.style "height:16px; background:#334155; border-radius:999px; overflow:hidden;"

                div {
                    attr.style ("height:100%; width:" + string xpPercent + "%; background:linear-gradient(90deg,#22c55e,#38bdf8);")
                }
            }
        }

        div {
            attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px; margin-bottom:24px;"

            h3 {
                attr.style "margin-top:0; color:#facc15;"
                text "Daily Focus Challenge"
            }

            p {
                text "Complete at least one quest today to earn bonus XP."
            }

            p {
                text ("Reward: " + string model.dailyChallengeRewardXp + " XP")
            }

            if model.dailyChallengeCompleted then
                p {
                    attr.style "color:#22c55e; font-weight:700;"
                    text "Daily challenge completed"
                }
            elif not hasCompletedQuest then
                p {
                    attr.style "color:#94a3b8; font-weight:600;"
                    text "Complete at least one quest before claiming the reward."
                }
            else
                button {
                    attr.style "background:linear-gradient(90deg,#f97316,#facc15); color:#111827; border:none; border-radius:10px; padding:10px 18px; font-weight:700; cursor:pointer;"
                    on.click (fun _ -> dispatch CompleteDailyChallenge)
                    text "Claim Daily Challenge"
                }
        }

        h3 {
            attr.style "font-size:26px; color:#facc15;"
            text "Today’s Quests"
        }

        for quest in model.quests do
            div {
                attr.style "background:#0f172a; border:1px solid #334155; border-radius:16px; padding:22px; margin-top:18px; box-shadow:0 8px 20px rgba(0,0,0,0.35);"

                h4 {
                    attr.style "font-size:22px; color:#38bdf8; margin-top:0;"
                    text quest.title
                }

                p { text ("Duration: " + string quest.duration + " minutes") }
                p { text ("Difficulty: " + string quest.difficulty) }

                p {
                    if quest.completed then text "Status: Completed"
                    else text "Status: Open"
                }

                if not quest.completed then
                    button {
                        attr.style "background:linear-gradient(90deg,#7c3aed,#06b6d4); color:white; border:none; border-radius:10px; padding:10px 18px; font-weight:700; cursor:pointer;"
                        on.click (fun _ -> dispatch (CompleteQuest quest.title))
                        text "Complete Quest"
                    }
            }
    }

let focusPage model dispatch =
    let minutesLeft = model.secondsLeft / 60
    let remainingSeconds = model.secondsLeft % 60
    let timerText = sprintf "%02d:%02d" minutesLeft remainingSeconds

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Focus Session"
        }

        div {
            attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px; max-width:520px;"

            h2 {
                attr.style "color:#facc15;"
                text "Boss Fight Mode"
            }

            p {
                text "Choose how long your next focus session should be."
            }

            div {
                attr.style "font-size:48px; font-weight:900; margin:20px 0;"
                text (string model.focusMinutes + " min")
            }

            div {
                attr.style "font-size:36px; font-weight:900; color:#22c55e; margin:16px 0;"
                text timerText
            }

            button {
                attr.style "margin-right:10px; padding:10px 18px; border-radius:10px; border:none; background:#334155; color:white; cursor:pointer;"
                on.click (fun _ -> dispatch DecreaseFocusTime)
                text "-5 min"
            }

            button {
                attr.style "padding:10px 18px; border-radius:10px; border:none; background:linear-gradient(90deg,#7c3aed,#06b6d4); color:white; cursor:pointer; font-weight:700;"
                on.click (fun _ -> dispatch IncreaseFocusTime)
                text "+5 min"
            }

            button {
                attr.style "margin-top:20px; margin-right:10px; padding:12px 22px; border-radius:12px; border:none; background:#22c55e; color:#052e16; cursor:pointer; font-weight:800;"
                on.click (fun _ -> dispatch StartFocusTimer)
                text "Start Timer"
            }

            button {
                attr.style "margin-top:20px; margin-right:10px; padding:12px 22px; border-radius:12px; border:none; background:#475569; color:white; cursor:pointer; font-weight:800;"
                on.click (fun _ -> dispatch StopFocusTimer)
                text "Stop Timer"
            }

            button {
                attr.style "margin-top:20px; padding:12px 22px; border:none; border-radius:10px; background:#2563eb; color:white; font-weight:700; cursor:pointer;"
                on.click (fun _ -> dispatch ResetFocusTimer)
                text "Reset Timer"
            }

            if model.focusSessionCompleted then
                div {
                    attr.style "margin-top:20px; padding:18px; border-radius:14px; background:#14532d; color:#dcfce7; font-weight:700;"
                    text "Focus session completed! +50 XP earned ⚔️"
                }
        }
    }

let statsPage model =
    let completed =
        model.quests |> List.filter (fun q -> q.completed) |> List.length

    let total = model.quests.Length

    let progress =
        if total = 0 then 0
        else completed * 100 / total

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Player Statistics"
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(5, minmax(0, 1fr)); gap:18px; margin-bottom:24px;"
            statBox "Completed quests" (string completed + " / " + string total)
            statBox "Progress" (string progress + "%")
            statBox "Focus minutes" (string model.focusMinutes)
            statBox "Current level" (string model.player.level)
            statBox "Current streak" (string model.streak + " days 🔥")
        }

        div {
            attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px;"

            h2 {
                attr.style "color:#facc15; margin-top:0;"
                text "Project Summary"
            }

            p { text ("Player: " + model.player.name) }
            p { text ("Total XP: " + string model.player.xp) }
            p { text ("Quest completion: " + string progress + "%") }
            p { text ("Current streak: " + string model.streak + " days") }
        }

        div {
            attr.style "margin-top:30px;"

            h2 {
                attr.style "font-size:28px; color:#facc15;"
                text "Achievements"
            }

            for achievement in model.achievements do
                div {
                    attr.style "background:#0f172a; border:1px solid #334155; border-radius:14px; padding:18px; margin-top:14px;"

                    h3 {
                        attr.style "margin-top:0; color:#38bdf8;"
                        text achievement.title
                    }

                    p { text achievement.description }

                    if achievement.unlocked then
                        p {
                            attr.style "color:#22c55e; font-weight:700;"
                            text "Unlocked"
                        }
                    else
                        p {
                            attr.style "color:#94a3b8;"
                            text "Locked"
                        }
                }
        }

        div {
            attr.style "margin-top:30px;"

            h2 {
                attr.style "font-size:28px; color:#facc15;"
                text "Recent Quest History"
            }

            if List.isEmpty model.questHistory then
                p {
                    attr.style "color:#94a3b8;"
                    text "Complete your first quest to start building your legend."
                }
            else
                for history in model.questHistory do
                    div {
                        attr.style "background:#0f172a; border:1px solid #334155; border-radius:14px; padding:18px; margin-top:14px;"

                        h3 {
                            attr.style "margin-top:0; color:#38bdf8;"
                            text history.title
                        }

                        p {
                            attr.style "color:#22c55e; font-weight:700;"
                            text ("XP Earned: +" + string history.xpEarned)
                        }

                        p {
                            text ("Completed at: " + history.completedAt.ToString("g"))
                        }
                    }
        }
    }

let settingsPage model dispatch =
    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Settings"
        }

        div {
            attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px; max-width:600px;"

            h2 {
                attr.style "color:#facc15;"
                text "Player Settings"
            }

            p {
                text ("Current player: " + model.player.name)
            }

            p {
                text ("Current level: " + string model.player.level)
            }

            p {
                text ("Current streak: " + string model.streak + " days 🔥")
            }

            button {
                attr.style "margin-top:16px; padding:10px 18px; border-radius:10px; border:none; background:#dc2626; color:white; font-weight:700; cursor:pointer;"
                on.click (fun _ -> dispatch ResetProgress)
                text "Reset Progress"
            }
        }
    }

let view model dispatch =
    div {
        attr.style "min-height:100vh; background:linear-gradient(135deg,#020617,#111827); color:#e5e7eb; font-family:Segoe UI,Arial,sans-serif;"

        div {
            attr.style "background:#020617; padding:16px 40px; border-bottom:1px solid #334155;"

            button {
                attr.style (buttonStyle (model.page = Home))
                on.click (fun _ -> dispatch (SetPage Home))
                text "Dashboard"
            }

            button {
                attr.style (buttonStyle (model.page = Focus))
                on.click (fun _ -> dispatch (SetPage Focus))
                text "Focus Session"
            }

            button {
                attr.style (buttonStyle (model.page = Stats))
                on.click (fun _ -> dispatch (SetPage Stats))
                text "Stats"
            }

            button {
                attr.style (buttonStyle (model.page = Settings))
                on.click (fun _ -> dispatch (SetPage Settings))
                text "Settings"
            }
        }

        match model.page with
        | Home -> homePage model dispatch
        | Focus -> focusPage model dispatch
        | Stats -> statsPage model
        | Settings -> settingsPage model dispatch
    }

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.MyApp

    override this.Program =
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
