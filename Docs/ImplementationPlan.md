# 2D Battle Car Arena — Implementation Plan

Source brief: [Google design document](https://docs.google.com/document/d/12c7xQsV6o9KRW7UOS7EU85JhDNZQFtx2R8d-PCJijdw/edit?usp=sharing)

## Project baseline

- Unity 6000.3.24f1 using URP 2D.
- New Input System, uGUI, and Unity Test Framework are installed.
- `Assets/_Project/Scenes/StartMenu.unity`, `Assets/_Project/Scenes/GarageHub.unity`, `Assets/_Project/Scenes/BattleArena.unity`, and `Assets/_Project/Scenes/GarageTransit.unity` are enabled build scenes.
- The original planning baseline had no gameplay code, prefabs, tests, or established application architecture. The current prototype includes the Start Menu, Garage Hub, session-owned Garage upgrades, and a battle scene; save persistence and planned automated test coverage remain future work.

## Prototype status (2026-10-03)

- The Start Menu to Garage Hub route and both scene layout prototypes exist. Their art and theme assets are kept in feature-named folders.
- `GameSession` owns runtime Garage progress across scene changes. Upgrade purchasing, the Garage-to-menu/BattleArena navigation, and the battle loop are implemented; save persistence remains future work.
- Garage Hub now has a `GarageHubTheme` asset for its replaceable backgrounds, logo, car preview, upgrade art, panels, buttons, font, and music. The two current backgrounds crossfade slowly, with occasional adjustable flicker.
- Garage Hub loops `Under_The_Chassis.mp3`; the persistent `GameSession` owns the sole runtime AudioListener, and scene camera listeners are disabled on load.
- Garage Hub crossfades the neutral and warm car sprites using the same breathing cycle and blend value as the two backgrounds. The warm car sprite is toned down in the theme; a matching flicker overlay follows the background pulse.
- A reusable rusted-steel panel frame is assigned to the three upgrade cards and four stat panels. Its nine-slice border keeps the riveted corners and edges consistent across both panel sizes.
- The Go to Mission button uses its dedicated rusted-steel frame and the Back to Menu button uses the shared action-button frame. Both use the special pixel font through replaceable Garage Hub theme fields. Go to Mission loads `BattleArena`; Back to Menu returns to `StartMenu`.
- Engine, Weapon, and Armor start at 100. Purchases spend their displayed costs and add their displayed upgrade amount to that stat and Score; the cards disable when remaining Credits cannot cover the cost. Progress currently resets on a new application run because save persistence is still future work.
- Upgrade-button raycasts and the engine purchase callback were previously verified in Play Mode. Those older observations used Engine 120 and are superseded by the current 100 baseline; the current baseline and Energy display need a fresh Play Mode check.
- Garage Hub includes separate transparent Engine, Weapon, and Armor icon sprites under `Art/UI/GarageHub/Upgrades/`, each assigned through its own replaceable field on `DefaultGarageHubTheme`.
- The Energy display is named `EnergyPanel`, reads `ENERGY`, and shows the current session energy percentage. Completed battle rounds spend 20%; after every third round, 20% is restored after the cost. A victory adds 5% of the current Score, rounded up, and grants 200 Credits.
- Garage Hub shows the front-view pixel-art car (`Art/UI/GarageHub/GarageCarPreview_Front.png`) in a resized `CarPreviewArtSlot` (813×498, Preserve Aspect).
- See [ImplementationChecklist.md](ImplementationChecklist.md), [StartMenuTemplate.md](StartMenuTemplate.md), and [GarageHubTemplate.md](GarageHubTemplate.md) for verified setup and swap instructions.
- BattleArena uses Garage Score (starting at 500) as the player's maximum health; challenger health and Engine, Weapon, and Armor power vary independently within ±20% of the player's corresponding values. Car hits use Engine power every 5.5 seconds during contact; both cars fire every 5 seconds using Weapon power. Armor reduces either damage type for three charges each, then stops protecting against that type. Battles have no time limit and end on health depletion or a boundary loss. Victory grants 5% of current Score and 200 Credits, and every completed battle spends 20% Energy with 20% restored every third round. Unity reports no Console errors after script refresh; these combat rules and the updated Energy display still need Play Mode validation.
- The battle result panel waits for a player click before returning to GarageHub. A previous Play Mode run returned to GarageHub after a resolved fight. Both winner paths, armor depletion, loop-seam listening check, and development build remain open.
- Before the first victory, GarageHub entry, exit, and battle-result return load their destinations directly. Once it is recorded, mission departure crossfades GarageHub into an additive GarageTransit scene while the shutter opens; its car stays visible until the shutter is fully open, then fades over 0.5 seconds before BattleArena loads. Leaving for StartMenu keeps the car visible while the shutter closes; returning from BattleArena opens the shutter with the car hidden until GarageHub loads. The open and close routes take 3 seconds total, including GarageHub UI/reveal animation where applicable. The scripts, sprites, theme, and built scene are in place; route variants and timing still need Play Mode verification.
- See [BattleArenaTemplate.md](BattleArenaTemplate.md) and [ArmoredCarArt.md](ArmoredCarArt.md) for current battle setup, replaceable art/effect/audio assets, and validation limits.

## Architecture

Use authored ScriptableObjects for immutable configuration, but keep mutable player progression in a serializable runtime model.

- `GameBalance`: upgrade curves, base stats, rewards, rank tiers, and enemy scaling.
- `PlayerProgress`: credits, score, rank, and upgrade levels.
- `PlayerProgressService`: authoritative runtime owner of progress.
- `JsonPlayerProgressStore`: persistence under `Application.persistentDataPath`.
- `GameSession`: the single persistent composition root.
- Scene controllers: narrowly scoped adapters between UI/physics and domain services.

Suggested project layout:

```text
Assets/_Project/
  Art/
    UI/StartMenu/
  Audio/Music/
  Data/UI/
    StartMenu/
    GarageHub/
  Prefabs/
  Scenes/
  Scripts/
    Core/
    Progression/
    Garage/
    Battle/
    UI/
      StartMenu/
      GarageHub/
  Tests/
    EditMode/
    PlayMode/
```

## Phase 1 — Foundation and progression

Create `GameBalance`, `PlayerProgress`, `UpgradeService`, persistence, `GameSession`, and scene navigation.

Acceptance criteria:

- A new profile starts with configured defaults.
- Upgrade prices and derived stats are deterministic and tested.
- Invalid purchases never mutate progress.
- Progress saves and reloads, including a safe fallback for missing or corrupt saves.

## Phase 2 — Start Menu and Garage

Complete the first playable route: `StartMenu → GarageHub`.

Start Menu:

- Replaceable background, logo, font, colors, and music.
- Start and Exit actions.
- Keyboard, controller, and pointer navigation.
- Responsive 1920×1080 reference layout.

Garage:

- Start with a responsive placeholder layout and named art slots so the car preview, background, upgrade-card art, and button sprites can be replaced independently.
- Credits, score, and rank display.
- Engine, weapon, and armor upgrade cards.
- Current value, next value, and upgrade cost.
- Disabled purchase state when credits are insufficient.
- Go to Mission action.

Acceptance criteria:

- Start opens the Garage once the Garage scene is added to Build Settings.
- The prototype Garage layout is present in `GarageHub.unity` before progression logic and final art are added.
- Purchases update the UI immediately and cannot create negative credits.
- Progress survives scene changes and application restarts.

## Phase 3 — Battle Arena prototype

The implemented blockout uses `Preparing → Approaching ⇄ Fighting → Results` for one fight.

Key components:

- `BattleArenaController`
- `CarMotor2D`
- `CrashReporter`
- `BattleResolver` for mapping the defeat condition to the winner
- `BattleHudView`
- `BattleResultView`

Cars use `Rigidbody2D` force in `FixedUpdate`, continuous collision detection, and frozen rotation. Armor sets health, weapon stats apply contact damage, and engine stats drive the push. Reaching zero health or touching an end wall loses the single fight. The result is shown briefly before both outcomes return to GarageHub; rewards and rank progression belong to Phase 4. See [BattleArenaTemplate.md](BattleArenaTemplate.md) for blockout tuning values and detailed acceptance criteria.

Acceptance criteria:

- [x] Two named block cars, health bars, road, background, and end walls use replaceable blockout visuals.
- [x] A `GET READY` cue precedes the cars driving toward each other.
- [x] Cars collide and push horizontally without rolling.
- [x] Health depletion and end-wall contact each finish the fight once.
- [x] The result overlay shows the winner and defeat reason.
- [x] Both outcomes return to GarageHub after the result display.
- [ ] Apply and save rewards exactly once as part of Phase 4.

## Phase 4 — Results and rank progression

Complete the loop: `GarageHub → BattleArena → GarageHub/RankUp → GarageHub`.

- Store rank thresholds as configured tiers rather than one mutable threshold.
- Apply and save battle rewards before leaving the result screen.
- Trigger Rank Up exactly once for every newly crossed tier.
- Do not deduct score on a loss unless the design changes.
- Treat ties as a player win, matching the original sample.

## Phase 5 — Polish and validation

- Route music and effects through mixer groups.
- Add collision sound, camera shake, particles, and button feedback.
- Validate controller/keyboard navigation and common aspect ratios.
- Add EditMode tests for progression, purchases, battle math, rewards, ranks, and save recovery.
- Add PlayMode tests for scene flow, collision one-shot behavior, results, and rank routing.
- Produce and launch a Windows x86_64 development build.

## Recommended delivery order

Finish a placeholder-art vertical slice first:

```text
Start → buy upgrade → battle → result → return/rank up → save
```

Replace placeholder visuals only after the complete loop is reliable. Estimated effort is 4–6 development days for the functional slice and 6–9 days for a polished first pass, depending on asset readiness.
