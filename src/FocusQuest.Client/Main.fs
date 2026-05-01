module FocusQuest.Client.Main

open Elmish
open Bolero
open Bolero.Html
open Bolero.Templating.Client

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/data">] Data

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
type Book =
    {
        title: string
        author: string
        publishDate: System.DateTime
        isbn: string
    }

type BookService =
    {
        getBooks: unit -> Async<Book[]>
        addBook: Book -> Async<unit>
        removeBookByIsbn: string -> Async<unit>
        signIn: string * string -> Async<option<string>>
        getUsername: unit -> Async<string>
        signOut: unit -> Async<unit>
    }

    interface Bolero.Remoting.IRemoteService with
        member this.BasePath = "/books"
        
type Model =
    {
        page: Page
        focusMinutes: int
        player: Player
        quests: Quest list
        error: string option
    }

let initModel =
    {
        page = Home
        focusMinutes = 25
        player = { name = "Player1"; xp = 0; level = 1 }
        quests =
            [
                { title = "Tanulás 30 perc"; duration = 30; difficulty = Easy; completed = false }
                { title = "Gyakorlás 60 perc"; duration = 60; difficulty = Medium; completed = false }
            ]
        error = None
    }

type Message =
    | SetPage of Page
    | IncreaseFocusTime
    | DecreaseFocusTime
    | SetFocusTime of int
    | CompleteQuest of string
    | ClearError

let xpForDifficulty difficulty =
    match difficulty with
    | Easy -> 25
    | Medium -> 50
    | Hard -> 80

let update message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | IncreaseFocusTime ->
        { model with focusMinutes = model.focusMinutes + 5 }, Cmd.none

    | DecreaseFocusTime ->
        let newValue = max 5 (model.focusMinutes - 5)
        { model with focusMinutes = newValue }, Cmd.none

    | SetFocusTime value ->
        { model with focusMinutes = max 5 value }, Cmd.none

    | CompleteQuest title ->
        let quest =
            model.quests
            |> List.tryFind (fun q -> q.title = title)

        let gainedXp =
            match quest with
            | Some q when not q.completed -> xpForDifficulty q.difficulty
            | _ -> 0

        let updatedQuests =
            model.quests
            |> List.map (fun q ->
                if q.title = title then { q with completed = true }
                else q)

        let newXp = model.player.xp + gainedXp

        let newLevel =
            if newXp >= model.player.level * 100 then model.player.level + 1
            else model.player.level

        { model with
            quests = updatedQuests
            player = { model.player with xp = newXp; level = newLevel } },
        Cmd.none

    | ClearError ->
        { model with error = None }, Cmd.none

let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main.Home().Elt()

let counterPage model dispatch =
    Main.Counter()
        .Decrement(fun _ -> dispatch DecreaseFocusTime)
        .Increment(fun _ -> dispatch IncreaseFocusTime)
        .Value(model.focusMinutes, fun v -> dispatch (SetFocusTime v))
        .Elt()

let dataPage model dispatch =
    Main.Data()
        .Reload(fun _ -> ())
        .Username(model.player.name)
        .SignOut(fun _ -> ())
        .Rows(
            concat {
                for quest in model.quests do
                    tr {
                        td { quest.title }
                        td { string quest.duration + " perc" }
                        td { string quest.difficulty }
                        td {
                            if quest.completed then "Completed"
                            else "Open"
                        }
                    }
            }
        )
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat {
            menuItem model Home "Dashboard"
            menuItem model Counter "Focus Session"
            menuItem model Data "Stats"
        })
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Counter -> counterPage model dispatch
            | Data -> dataPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty()
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.MyApp

    override this.Program =
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif