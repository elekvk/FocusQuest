module FocusQuest.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/focus">] Focus
    | [<EndPoint "/stats">] Stats
    | [<EndPoint "/shop">] Shop
    | [<EndPoint "/inventory">] Inventory
    | [<EndPoint "/rewards">] Rewards
    | [<EndPoint "/boss">] Boss
    | [<EndPoint "/skills">] Skills
    | [<EndPoint "/profile">] Profile
    | [<EndPoint "/settings">] Settings

type Difficulty =
    | Easy
    | Medium
    | Hard

type Rarity =
    | Common
    | Rare
    | Epic
    | Legendary

type SkillCategory =
    | FocusPower
    | QuestMastery
    | LootLuck
    | BossDamage

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

type ShopItem =
    {
        name: string
        description: string
        cost: int
        rarity: Rarity
        purchased: bool
    }

type Skill =
    {
        name: string
        description: string
        category: SkillCategory
        level: int
        maxLevel: int
        baseCost: int
    }

type PlayerTitle =
    {
        name: string
        description: string
        requirement: string
        unlocked: bool
    }

type QuestHistory =
    {
        title: string
        xpEarned: int
        completedAt: DateTime
    }

type RewardHistory =
    {
        rewardName: string
        rarity: Rarity
        unlockedAt: DateTime
    }

type BossEnemy =
    {
        name: string
        maxHp: int
        currentHp: int
        level: int
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
        xpMultiplier: float
        dailyChallengeCompleted: bool
        dailyChallengeRewardXp: int
        questHistory: QuestHistory list
        timerRunning: bool
        secondsLeft: int
        focusSessionCompleted: bool
        shopItems: ShopItem list
        equippedItem: string option
        lootMessage: string option
        rewardHistory: RewardHistory list
        boss: BossEnemy
        skills: Skill list
        playerTitles: PlayerTitle list
        equippedTitle: string option
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
        xpMultiplier = 1.0
        dailyChallengeCompleted = false
        dailyChallengeRewardXp = 40
        questHistory = []
        timerRunning = false
        secondsLeft = 25 * 60
        focusSessionCompleted = false
        shopItems =
            [
                {
                    name = "Starter Cloak"
                    description = "A common reward for beginning your focus journey."
                    cost = 50
                    rarity = Common
                    purchased = false
                }
                {
                    name = "Golden Sword"
                    description = "A cosmetic reward for focused warriors."
                    cost = 100
                    rarity = Rare
                    purchased = false
                }
                {
                    name = "Crystal Timer"
                    description = "A rare timer skin for disciplined players."
                    cost = 120
                    rarity = Rare
                    purchased = false
                }
                {
                    name = "Focus Crown"
                    description = "Shows that you are serious about your focus journey."
                    cost = 150
                    rarity = Epic
                    purchased = false
                }
                {
                    name = "XP Booster Badge"
                    description = "A badge for players who consistently complete quests."
                    cost = 200
                    rarity = Legendary
                    purchased = false
                }
                {
                    name = "Dragon Focus Emblem"
                    description = "A legendary symbol of deep work mastery."
                    cost = 250
                    rarity = Legendary
                    purchased = false
                }
            ]
        equippedItem = None
        lootMessage = None
        rewardHistory = []
        boss =
            {
                name = "Shadow Dragon"
                maxHp = 500
                currentHp = 500
                level = 1
            }
        skills =
            [
                {
                    name = "Focus Power"
                    description = "Increase XP earned from completed focus sessions."
                    category = FocusPower
                    level = 0
                    maxLevel = 5
                    baseCost = 80
                }
                {
                    name = "Quest Mastery"
                    description = "Increase XP earned from completed quests."
                    category = QuestMastery
                    level = 0
                    maxLevel = 5
                    baseCost = 90
                }
                {
                    name = "Loot Luck"
                    description = "Improve your chance of earning higher-rarity loot."
                    category = LootLuck
                    level = 0
                    maxLevel = 5
                    baseCost = 120
                }
                {
                    name = "Boss Damage"
                    description = "Increase damage dealt during boss battles."
                    category = BossDamage
                    level = 0
                    maxLevel = 5
                    baseCost = 100
                }
            ]
        playerTitles =
            [
                {
                    name = "Novice Adventurer"
                    description = "A starting title for new FocusQuest players."
                    requirement = "Available from the beginning."
                    unlocked = true
                }
                {
                    name = "Focused Apprentice"
                    description = "Awarded for completing your first quest."
                    requirement = "Complete at least 1 quest."
                    unlocked = false
                }
                {
                    name = "Level Climber"
                    description = "Awarded for reaching level 2."
                    requirement = "Reach level 2."
                    unlocked = false
                }
                {
                    name = "Treasure Hunter"
                    description = "Awarded for unlocking at least 3 rewards."
                    requirement = "Unlock 3 inventory rewards."
                    unlocked = false
                }
                {
                    name = "Dragon Challenger"
                    description = "Awarded for progressing beyond the first boss."
                    requirement = "Defeat the first boss."
                    unlocked = false
                }
                {
                    name = "Skill Strategist"
                    description = "Awarded for investing in the skill tree."
                    requirement = "Reach 3 total skill levels."
                    unlocked = false
                }
            ]
        equippedTitle = Some "Novice Adventurer"
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
    | BuyShopItem of string
    | EquipItem of string
    | ClearLootMessage
    | AttackBoss
    | UpgradeSkill of string
    | EquipTitle of string
    | ResetProgress
    | RefreshDailyQuests

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

let updateAchievements (player: Player) (quests: Quest list) (achievements: Achievement list) =
    let completedCount =
        quests |> List.filter (fun quest -> quest.completed) |> List.length

    let allCompleted =
        completedCount = quests.Length

    achievements
    |> List.map (fun achievement ->
        if achievement.title = "First Quest" && completedCount >= 1 then
            { achievement with unlocked = true }
        elif achievement.title = "Level Up" && player.level >= 2 then
            { achievement with unlocked = true }
        elif achievement.title = "Focused Hero" && allCompleted then
            { achievement with unlocked = true }
        else
            achievement)

let calculateLevel xp currentLevel =
    if xp >= currentLevel * 100 then currentLevel + 1
    else currentLevel

let calculateMultiplier streak =
    if streak >= 14 then 1.5
    elif streak >= 7 then 1.25
    elif streak >= 3 then 1.1
    else 1.0

let rarityColor rarity =
    match rarity with
    | Common -> "#94a3b8"
    | Rare -> "#38bdf8"
    | Epic -> "#c084fc"
    | Legendary -> "#facc15"

let rarityText rarity =
    match rarity with
    | Common -> "Common"
    | Rare -> "Rare"
    | Epic -> "Epic"
    | Legendary -> "Legendary"

let skillCategoryText category =
    match category with
    | FocusPower -> "Focus Power"
    | QuestMastery -> "Quest Mastery"
    | LootLuck -> "Loot Luck"
    | BossDamage -> "Boss Damage"

let skillCost skill =
    skill.baseCost + (skill.level * 60)

let skillBonusText skill =
    match skill.category with
    | FocusPower -> "+" + string (skill.level * 10) + "% focus XP"
    | QuestMastery -> "+" + string (skill.level * 10) + "% quest XP"
    | LootLuck -> "+" + string (skill.level * 5) + "% loot luck"
    | BossDamage -> "+" + string (skill.level * 10) + " boss damage"

let skillLevel (category: SkillCategory) (skills: Skill list) =
    skills
    |> List.tryFind (fun skill -> skill.category = category)
    |> Option.map (fun skill -> skill.level)
    |> Option.defaultValue 0

let updatePlayerTitles (model: Model) =
    let completedQuestCount =
        model.quests |> List.filter (fun quest -> quest.completed) |> List.length

    let purchasedRewardCount =
        model.shopItems |> List.filter (fun item -> item.purchased) |> List.length

    let totalSkillLevels =
        model.skills |> List.sumBy (fun skill -> skill.level)

    model.playerTitles
    |> List.map (fun title ->
        if title.name = "Novice Adventurer" then
            { title with unlocked = true }
        elif title.name = "Focused Apprentice" && completedQuestCount >= 1 then
            { title with unlocked = true }
        elif title.name = "Level Climber" && model.player.level >= 2 then
            { title with unlocked = true }
        elif title.name = "Treasure Hunter" && purchasedRewardCount >= 3 then
            { title with unlocked = true }
        elif title.name = "Dragon Challenger" && model.boss.level >= 2 then
            { title with unlocked = true }
        elif title.name = "Skill Strategist" && totalSkillLevels >= 3 then
            { title with unlocked = true }
        else
            title)

let refreshTitles model =
    { model with playerTitles = updatePlayerTitles model }

let random = Random()

let generateLoot (skills: Skill list) (shopItems: ShopItem list) =
    let availableItems =
        shopItems |> List.filter (fun item -> not item.purchased)

    if List.isEmpty availableItems then
        None
    else
        let lootLuckLevel =
            skillLevel LootLuck skills

        let roll =
            random.Next(100) + (lootLuckLevel * 5)

        let desiredRarity =
            if roll < 50 then Common
            elif roll < 80 then Rare
            elif roll < 95 then Epic
            else Legendary

        let matchingItems =
            availableItems |> List.filter (fun item -> item.rarity = desiredRarity)

        let finalPool =
            if List.isEmpty matchingItems then availableItems
            else matchingItems

        Some finalPool.[random.Next(finalPool.Length)]

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
        let newMinutes =
            max 5 (model.focusMinutes - 5)

        { model with
            focusMinutes = newMinutes
            secondsLeft = newMinutes * 60 },
        Cmd.none

    | CompleteQuest title ->
        let selectedQuest =
            model.quests |> List.tryFind (fun quest -> quest.title = title)

        let baseXp =
            match selectedQuest with
            | Some quest when not quest.completed -> xpForDifficulty quest.difficulty
            | _ -> 0

        let questMasteryLevel =
            skillLevel QuestMastery model.skills

        let gainedXp =
            int (float baseXp * (1.0 + float questMasteryLevel * 0.1))

        let updatedQuests =
            model.quests
            |> List.map (fun quest ->
                if quest.title = title then { quest with completed = true }
                else quest)

        let newXp =
            model.player.xp + gainedXp

        let newLevel =
            calculateLevel newXp model.player.level

        let updatedPlayer =
            { model.player with
                xp = newXp
                level = newLevel }

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

        let newMultiplier =
            calculateMultiplier newStreak

        { model with
            quests = updatedQuests
            player = updatedPlayer
            achievements = updatedAchievements
            streak = newStreak
            xpMultiplier = newMultiplier
            questHistory = updatedQuestHistory }
        |> refreshTitles,
        Cmd.none

    | CompleteDailyChallenge ->
        let hasCompletedQuest =
            model.quests |> List.exists (fun quest -> quest.completed)

        if model.dailyChallengeCompleted || not hasCompletedQuest then
            model, Cmd.none
        else
            let newXp =
                model.player.xp + model.dailyChallengeRewardXp

            let newLevel =
                calculateLevel newXp model.player.level

            let updatedPlayer =
                { model.player with
                    xp = newXp
                    level = newLevel }

            let updatedAchievements =
                updateAchievements updatedPlayer model.quests model.achievements

            { model with
                player = updatedPlayer
                achievements = updatedAchievements
                dailyChallengeCompleted = true }
            |> refreshTitles,
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
        let baseXp = 50

        let focusPowerLevel =
            skillLevel FocusPower model.skills

        let gainedXp =
            int (float baseXp * model.xpMultiplier * (1.0 + float focusPowerLevel * 0.1))

        let newXp =
            model.player.xp + gainedXp

        let newLevel =
            calculateLevel newXp model.player.level

        let updatedPlayer =
            { model.player with
                xp = newXp
                level = newLevel }

        let updatedHistory =
            {
                title = "Focus Session"
                xpEarned = gainedXp
                completedAt = DateTime.Now
            } :: model.questHistory

        let lootReward =
            generateLoot model.skills model.shopItems

        let finalShopItems =
            match lootReward with
            | Some loot ->
                model.shopItems
                |> List.map (fun item ->
                    if item.name = loot.name then
                        { item with purchased = true }
                    else
                        item)
            | None ->
                model.shopItems

        let updatedRewardHistory =
            match lootReward with
            | Some loot ->
                {
                    rewardName = loot.name
                    rarity = loot.rarity
                    unlockedAt = DateTime.Now
                } :: model.rewardHistory
            | None ->
                model.rewardHistory

        let lootMessage =
            match lootReward with
            | Some loot ->
                Some ("Loot unlocked: " + loot.name + " (" + rarityText loot.rarity + ")")
            | None ->
                Some "Focus session completed. No new loot available."

        { model with
            player = updatedPlayer
            questHistory = updatedHistory
            focusSessionCompleted = true
            shopItems = finalShopItems
            rewardHistory = updatedRewardHistory
            lootMessage = lootMessage }
        |> refreshTitles,
        Cmd.none

    | ResetFocusTimer ->
        { model with
            timerRunning = false
            secondsLeft = model.focusMinutes * 60
            focusSessionCompleted = false },
        Cmd.none

    | BuyShopItem itemName ->
        let selectedItem =
            model.shopItems |> List.tryFind (fun item -> item.name = itemName)

        match selectedItem with
        | Some item when not item.purchased && model.player.xp >= item.cost ->
            let updatedItems =
                model.shopItems
                |> List.map (fun shopItem ->
                    if shopItem.name = itemName then
                        { shopItem with purchased = true }
                    else
                        shopItem)

            let updatedPlayer =
                { model.player with xp = model.player.xp - item.cost }

            { model with
                player = updatedPlayer
                shopItems = updatedItems }
            |> refreshTitles,
            Cmd.none

        | _ ->
            model, Cmd.none

    | EquipItem itemName ->
        let ownsItem =
            model.shopItems
            |> List.exists (fun item -> item.name = itemName && item.purchased)

        if ownsItem then
            { model with equippedItem = Some itemName }, Cmd.none
        else
            model, Cmd.none

    | EquipTitle titleName ->
        let ownsTitle =
            model.playerTitles
            |> List.exists (fun title -> title.name = titleName && title.unlocked)

        if ownsTitle then
            { model with equippedTitle = Some titleName }, Cmd.none
        else
            model, Cmd.none

    | ClearLootMessage ->
        { model with lootMessage = None }, Cmd.none

    | AttackBoss ->
        let bossDamageLevel =
            skillLevel BossDamage model.skills

        let damage =
            random.Next(20, 80) + (bossDamageLevel * 10)

        let newHp =
            max 0 (model.boss.currentHp - damage)

        if newHp <= 0 then
            let newBossLevel =
                model.boss.level + 1

            let scaledHp =
                500 + (newBossLevel * 200)

            let newBoss =
                {
                    name = "Ancient Titan Lv." + string newBossLevel
                    maxHp = scaledHp
                    currentHp = scaledHp
                    level = newBossLevel
                }

            let rewardXp =
                300

            let newXp =
                model.player.xp + rewardXp

            let newLevel =
                calculateLevel newXp model.player.level

            let updatedPlayer =
                {
                    model.player with
                        xp = newXp
                        level = newLevel
                }

            {
                model with
                    player = updatedPlayer
                    boss = newBoss
                    lootMessage = Some ("Boss defeated! +" + string rewardXp + " XP earned ⚔️")
            }
            |> refreshTitles,
            Cmd.none
        else
            {
                model with
                    boss =
                        {
                            model.boss with
                                currentHp = newHp
                        }
                    lootMessage = Some ("Boss hit for " + string damage + " damage!")
            },
            Cmd.none

    | UpgradeSkill skillName ->
        let selectedSkill =
            model.skills |> List.tryFind (fun skill -> skill.name = skillName)

        match selectedSkill with
        | Some skill when skill.level < skill.maxLevel && model.player.xp >= skillCost skill ->
            let cost =
                skillCost skill

            let updatedSkills =
                model.skills
                |> List.map (fun currentSkill ->
                    if currentSkill.name = skillName then
                        { currentSkill with level = currentSkill.level + 1 }
                    else
                        currentSkill)

            let updatedPlayer =
                { model.player with xp = model.player.xp - cost }

            {
                model with
                    player = updatedPlayer
                    skills = updatedSkills
                    lootMessage = Some ("Skill upgraded: " + skill.name)
            }
            |> refreshTitles,
            Cmd.none

        | _ ->
            model, Cmd.none

    | RefreshDailyQuests ->
        let refreshedQuests =
            [
                { title = "Read for 20 minutes"; duration = 20; difficulty = Easy; completed = false }
                { title = "Deep work session"; duration = 45; difficulty = Medium; completed = false }
                { title = "Complete a hard project task"; duration = 60; difficulty = Hard; completed = false }
            ]

        { model with
            quests = refreshedQuests
            dailyChallengeCompleted = false
            focusSessionCompleted = false
            lootMessage = Some "Daily quests refreshed!" },
        Cmd.none

    | ResetProgress ->
        initModel, Cmd.none

let router =
    Router.infer SetPage (fun model -> model.page)

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

let rarityLabel (item: ShopItem) =
    p {
        attr.style ("font-weight:800; color:" + rarityColor item.rarity)
        text (rarityText item.rarity)
    }

let homePage model dispatch =
    let requiredXp =
        model.player.level * 100

    let hasCompletedQuest =
        model.quests |> List.exists (fun quest -> quest.completed)

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
            attr.style "display:grid; grid-template-columns:repeat(4, minmax(0, 1fr)); gap:18px; margin-bottom:24px;"

            statBox "Player" model.player.name
            statBox "Title" (match model.equippedTitle with | Some title -> title | None -> "None")
            statBox "Level" (string model.player.level)
            statBox "XP" (string model.player.xp + " / " + string requiredXp)
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(2, minmax(0, 1fr)); gap:18px; margin-bottom:24px;"

            statBox
                "Equipped Reward"
                (match model.equippedItem with
                 | Some item -> item
                 | None -> "None")

            statBox "Streak Multiplier" (string model.xpMultiplier + "x")
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

        match model.lootMessage with
        | Some message ->
            div {
                attr.style "margin-bottom:24px; padding:18px; border-radius:14px; background:#14532d; color:#dcfce7; font-weight:800;"

                text message

                button {
                    attr.style "margin-left:20px; padding:8px 14px; border:none; border-radius:8px; cursor:pointer;"
                    on.click (fun _ -> dispatch ClearLootMessage)
                    text "Close"
                }
            }
        | None ->
            empty()

        h3 {
            attr.style "font-size:26px; color:#facc15;"
            text "Today’s Quests"
        }

        button {
            attr.style "margin-bottom:20px; padding:10px 18px; border-radius:10px; border:none; background:#475569; color:white; cursor:pointer; font-weight:700;"
            on.click (fun _ -> dispatch RefreshDailyQuests)
            text "Refresh Daily Quests"
        }
        
        for quest in model.quests do
            div {
                attr.style "background:#0f172a; border:1px solid #334155; border-radius:16px; padding:22px; margin-top:18px; box-shadow:0 8px 20px rgba(0,0,0,0.35);"

                h4 {
                    attr.style "font-size:22px; color:#38bdf8; margin-top:0;"
                    text quest.title
                }

                p {
                    text ("Duration: " + string quest.duration + " minutes")
                }

                p {
                    text ("Difficulty: " + string quest.difficulty)
                }

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
    let minutesLeft =
        model.secondsLeft / 60

    let remainingSeconds =
        model.secondsLeft % 60

    let timerText =
        sprintf "%02d:%02d" minutesLeft remainingSeconds

    let focusReward =
        int (float 50 * model.xpMultiplier * (1.0 + float (skillLevel FocusPower model.skills) * 0.1))

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
                    text ("Focus session completed! +" + string focusReward + " XP earned ⚔️")
                }
        }
    }

let statsPage model =
    let completed =
        model.quests |> List.filter (fun quest -> quest.completed) |> List.length

    let total =
        model.quests.Length

    let progress =
        if total = 0 then 0
        else completed * 100 / total

    let purchasedCount =
        model.shopItems |> List.filter (fun item -> item.purchased) |> List.length

    let totalSkillLevels =
        model.skills |> List.sumBy (fun skill -> skill.level)

    let unlockedTitleCount =
        model.playerTitles |> List.filter (fun title -> title.unlocked) |> List.length

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Player Statistics"
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(6, minmax(0, 1fr)); gap:18px; margin-bottom:24px;"

            statBox "Completed quests" (string completed + " / " + string total)
            statBox "Progress" (string progress + "%")
            statBox "Focus minutes" (string model.focusMinutes)
            statBox "Current level" (string model.player.level)
            statBox "Skill Levels" (string totalSkillLevels)
            statBox "Titles" (string unlockedTitleCount + " / " + string model.playerTitles.Length)
        }

        div {
            attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px;"

            h2 {
                attr.style "color:#facc15; margin-top:0;"
                text "Project Summary"
            }

            p { text ("Player: " + model.player.name) }
            p { text ("Equipped title: " + (match model.equippedTitle with | Some title -> title | None -> "None")) }
            p { text ("Total XP: " + string model.player.xp) }
            p { text ("Quest completion: " + string progress + "%") }
            p { text ("Current streak: " + string model.streak + " days") }
            p { text ("Current multiplier: " + string model.xpMultiplier + "x") }
            p { text ("Purchased rewards: " + string purchasedCount + " / " + string model.shopItems.Length) }
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

                    p {
                        text achievement.description
                    }

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

let shopPage model dispatch =
    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Reward Shop"
        }

        p {
            attr.style "font-size:18px; color:#cbd5e1;"
            text "Spend your earned XP on cosmetic rewards and focus achievements."
        }

        div {
            attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:22px; margin-bottom:24px;"

            h2 {
                attr.style "color:#facc15; margin-top:0;"
                text ("Available XP: " + string model.player.xp)
            }
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(3, minmax(0, 1fr)); gap:18px;"

            for item in model.shopItems do
                div {
                    attr.style (
                        if item.purchased then
                            "background:linear-gradient(135deg,#14532d,#166534); border:1px solid #22c55e; border-radius:16px; padding:22px;"
                        else
                            "background:#0f172a; border:1px solid #334155; border-radius:16px; padding:22px;"
                    )

                    h3 {
                        attr.style "color:#38bdf8; margin-top:0;"
                        text item.name
                    }

                    p {
                        text item.description
                    }

                    rarityLabel item

                    p {
                        attr.style "color:#facc15; font-weight:700;"
                        text ("Cost: " + string item.cost + " XP")
                    }

                    if item.purchased then
                        p {
                            attr.style "color:#22c55e; font-weight:800;"
                            text "Purchased"
                        }
                    elif model.player.xp >= item.cost then
                        button {
                            attr.style "padding:10px 18px; border-radius:10px; border:none; background:linear-gradient(90deg,#22c55e,#38bdf8); color:#082f49; cursor:pointer; font-weight:800;"
                            on.click (fun _ -> dispatch (BuyShopItem item.name))
                            text "Buy Item"
                        }
                    else
                        p {
                            attr.style "color:#94a3b8;"
                            text "Not enough XP"
                        }
                }
        }
    }

let inventoryPage model dispatch =
    let purchasedItems =
        model.shopItems |> List.filter (fun item -> item.purchased)

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Inventory"
        }

        p {
            attr.style "font-size:18px; color:#cbd5e1;"
            text "Your unlocked rewards and focus trophies."
        }

        if List.isEmpty purchasedItems then
            div {
                attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px; margin-top:24px;"

                h2 {
                    attr.style "color:#facc15; margin-top:0;"
                    text "No rewards unlocked yet"
                }

                p {
                    attr.style "color:#94a3b8;"
                    text "Complete quests and focus sessions to earn XP, then buy rewards in the Shop."
                }
            }
        else
            div {
                attr.style "display:grid; grid-template-columns:repeat(3, minmax(0, 1fr)); gap:18px; margin-top:24px;"

                for item in purchasedItems do
                    div {
                        attr.style "background:linear-gradient(135deg,#14532d,#166534); border:1px solid #22c55e; border-radius:16px; padding:22px;"

                        h3 {
                            attr.style "color:#38bdf8; margin-top:0;"
                            text item.name
                        }

                        p {
                            text item.description
                        }

                        rarityLabel item

                        p {
                            attr.style "color:#dcfce7; font-weight:800;"
                            text "Unlocked reward"
                        }

                        if model.equippedItem = Some item.name then
                            p {
                                attr.style "color:#facc15; font-weight:800;"
                                text "Equipped"
                            }
                        else
                            button {
                                attr.style "margin-top:12px; padding:10px 18px; border-radius:10px; border:none; background:#38bdf8; color:#082f49; cursor:pointer; font-weight:800;"
                                on.click (fun _ -> dispatch (EquipItem item.name))
                                text "Equip Item"
                            }
                    }
            }
    }

let rewardsPage model =
    let legendaryCount =
        model.rewardHistory |> List.filter (fun reward -> reward.rarity = Legendary) |> List.length

    let epicCount =
        model.rewardHistory |> List.filter (fun reward -> reward.rarity = Epic) |> List.length

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Reward History"
        }

        p {
            attr.style "font-size:18px; color:#cbd5e1;"
            text "Track the random loot rewards unlocked from completed focus sessions."
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(4, minmax(0, 1fr)); gap:18px; margin-bottom:24px;"

            statBox "Unlocked Rewards" (string model.rewardHistory.Length)
            statBox "Legendary Rewards" (string legendaryCount)
            statBox "Epic Rewards" (string epicCount)

            statBox
                "Equipped Item"
                (match model.equippedItem with
                 | Some item -> item
                 | None -> "None")
        }

        if List.isEmpty model.rewardHistory then
            div {
                attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px;"

                h2 {
                    attr.style "color:#facc15;"
                    text "No rewards unlocked yet"
                }

                p {
                    attr.style "color:#94a3b8;"
                    text "Complete focus sessions to discover random loot rewards."
                }
            }
        else
            for reward in model.rewardHistory do
                div {
                    attr.style "background:#0f172a; border:1px solid #334155; border-radius:16px; padding:22px; margin-top:18px;"

                    h2 {
                        attr.style ("color:" + rarityColor reward.rarity)
                        text reward.rewardName
                    }

                    p {
                        attr.style ("font-weight:800; color:" + rarityColor reward.rarity)
                        text (rarityText reward.rarity)
                    }

                    p {
                        text ("Unlocked at: " + reward.unlockedAt.ToString("g"))
                    }
                }
    }

let bossPage model dispatch =
    let hpPercent =
        (float model.boss.currentHp / float model.boss.maxHp) * 100.0

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:48px; color:#ef4444;"
            text "Boss Battle"
        }

        div {
            attr.style "background:#1e293b; border:2px solid #7f1d1d; border-radius:20px; padding:32px; max-width:800px;"

            h2 {
                attr.style "font-size:36px; color:#f87171;"
                text model.boss.name
            }

            p {
                attr.style "font-size:20px;"
                text ("Level " + string model.boss.level)
            }

            p {
                attr.style "font-size:20px; font-weight:700;"
                text ("HP: " + string model.boss.currentHp + " / " + string model.boss.maxHp)
            }

            div {
                attr.style "height:24px; background:#334155; border-radius:999px; overflow:hidden; margin-top:20px;"

                div {
                    attr.style ("height:100%; width:" + string hpPercent + "%; background:linear-gradient(90deg,#dc2626,#f87171);")
                }
            }

            button {
                attr.style "margin-top:30px; padding:14px 26px; border:none; border-radius:12px; background:linear-gradient(90deg,#dc2626,#f97316); color:white; font-size:18px; font-weight:800; cursor:pointer;"
                on.click (fun _ -> dispatch AttackBoss)
                text "Attack Boss ⚔️"
            }

            p {
                attr.style "margin-top:20px; color:#94a3b8;"
                text "Each attack deals random damage. Defeating a boss grants bonus XP and summons a stronger boss."
            }
        }
    }

let skillsPage model dispatch =
    let totalSkillLevels =
        model.skills |> List.sumBy (fun skill -> skill.level)

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Skill Tree"
        }

        p {
            attr.style "font-size:18px; color:#cbd5e1;"
            text "Spend XP to unlock permanent upgrades for focus sessions, quests, loot, and boss battles."
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(3, minmax(0, 1fr)); gap:18px; margin-bottom:24px;"

            statBox "Available XP" (string model.player.xp)
            statBox "Total Skill Levels" (string totalSkillLevels)
            statBox "Focus Bonus" (string (skillLevel FocusPower model.skills * 10) + "%")
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(2, minmax(0, 1fr)); gap:18px;"

            for skill in model.skills do
                let cost =
                    skillCost skill

                let isMaxed =
                    skill.level >= skill.maxLevel

                div {
                    attr.style "background:#0f172a; border:1px solid #334155; border-radius:18px; padding:24px;"

                    h2 {
                        attr.style "color:#38bdf8; margin-top:0;"
                        text skill.name
                    }

                    p {
                        attr.style "color:#cbd5e1;"
                        text skill.description
                    }

                    p {
                        attr.style "color:#facc15; font-weight:800;"
                        text ("Category: " + skillCategoryText skill.category)
                    }

                    p {
                        attr.style "color:#22c55e; font-weight:800;"
                        text ("Current bonus: " + skillBonusText skill)
                    }

                    p {
                        text ("Level: " + string skill.level + " / " + string skill.maxLevel)
                    }

                    div {
                        attr.style "height:14px; background:#334155; border-radius:999px; overflow:hidden; margin:12px 0;"

                        div {
                            let percent =
                                (float skill.level / float skill.maxLevel) * 100.0

                            attr.style ("height:100%; width:" + string percent + "%; background:linear-gradient(90deg,#7c3aed,#38bdf8);")
                        }
                    }

                    if isMaxed then
                        p {
                            attr.style "color:#22c55e; font-weight:800;"
                            text "Max level reached"
                        }
                    elif model.player.xp >= cost then
                        button {
                            attr.style "margin-top:12px; padding:10px 18px; border-radius:10px; border:none; background:linear-gradient(90deg,#22c55e,#38bdf8); color:#082f49; cursor:pointer; font-weight:800;"
                            on.click (fun _ -> dispatch (UpgradeSkill skill.name))
                            text ("Upgrade for " + string cost + " XP")
                        }
                    else
                        p {
                            attr.style "color:#94a3b8; font-weight:700;"
                            text ("Need " + string cost + " XP to upgrade")
                        }
                }
        }
    }

let profilePage model dispatch =
    let unlockedTitles =
        model.playerTitles |> List.filter (fun title -> title.unlocked) |> List.length

    div {
        attr.style "padding:40px;"

        h1 {
            attr.style "font-size:42px; color:#38bdf8;"
            text "Player Profile"
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(4, minmax(0, 1fr)); gap:18px; margin-bottom:24px;"

            statBox "Player" model.player.name
            statBox "Level" (string model.player.level)
            statBox "XP" (string model.player.xp)
            statBox "Titles" (string unlockedTitles + " / " + string model.playerTitles.Length)
        }

        div {
            attr.style "background:#1e293b; border:1px solid #334155; border-radius:18px; padding:24px; margin-bottom:24px;"

            h2 {
                attr.style "color:#facc15; margin-top:0;"
                text "Active Identity"
            }

            p {
                attr.style "font-size:20px; font-weight:800; color:#e5e7eb;"
                text ("Equipped title: " + (match model.equippedTitle with | Some title -> title | None -> "None"))
            }

            p {
                text ("Equipped reward: " + (match model.equippedItem with | Some item -> item | None -> "None"))
            }
        }

        h2 {
            attr.style "color:#facc15;"
            text "Player Titles"
        }

        div {
            attr.style "display:grid; grid-template-columns:repeat(2, minmax(0, 1fr)); gap:18px;"

            for title in model.playerTitles do
                div {
                    attr.style (
                        if title.unlocked then
                            "background:#0f172a; border:1px solid #22c55e; border-radius:16px; padding:22px;"
                        else
                            "background:#0f172a; border:1px solid #334155; border-radius:16px; padding:22px; opacity:0.75;"
                    )

                    h3 {
                        attr.style "color:#38bdf8; margin-top:0;"
                        text title.name
                    }

                    p {
                        text title.description
                    }

                    p {
                        attr.style "color:#94a3b8;"
                        text ("Requirement: " + title.requirement)
                    }

                    if model.equippedTitle = Some title.name then
                        p {
                            attr.style "color:#facc15; font-weight:800;"
                            text "Equipped"
                        }
                    elif title.unlocked then
                        button {
                            attr.style "margin-top:12px; padding:10px 18px; border-radius:10px; border:none; background:#38bdf8; color:#082f49; cursor:pointer; font-weight:800;"
                            on.click (fun _ -> dispatch (EquipTitle title.name))
                            text "Equip Title"
                        }
                    else
                        p {
                            attr.style "color:#64748b; font-weight:700;"
                            text "Locked"
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
                attr.style (buttonStyle (model.page = Shop))
                on.click (fun _ -> dispatch (SetPage Shop))
                text "Shop"
            }

            button {
                attr.style (buttonStyle (model.page = Inventory))
                on.click (fun _ -> dispatch (SetPage Inventory))
                text "Inventory"
            }

            button {
                attr.style (buttonStyle (model.page = Rewards))
                on.click (fun _ -> dispatch (SetPage Rewards))
                text "Rewards"
            }

            button {
                attr.style (buttonStyle (model.page = Boss))
                on.click (fun _ -> dispatch (SetPage Boss))
                text "Boss"
            }

            button {
                attr.style (buttonStyle (model.page = Skills))
                on.click (fun _ -> dispatch (SetPage Skills))
                text "Skills"
            }

            button {
                attr.style (buttonStyle (model.page = Profile))
                on.click (fun _ -> dispatch (SetPage Profile))
                text "Profile"
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
        | Shop -> shopPage model dispatch
        | Inventory -> inventoryPage model dispatch
        | Rewards -> rewardsPage model
        | Boss -> bossPage model dispatch
        | Skills -> skillsPage model dispatch
        | Profile -> profilePage model dispatch
        | Settings -> settingsPage model dispatch
    }

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.MyApp

    override this.Program =
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
