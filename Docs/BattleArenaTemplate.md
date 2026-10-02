# Battle Arena

`Assets/_Project/Scenes/BattleArena.unity` is the single-fight arena loaded by the Garage Hub's **Go to Mission** button. The scene now uses side-view armored-car artwork and a battle HUD while keeping physics roots, presentation prefabs, and audio/effect resources as replaceable assets.

## Scene structure

```text
BattleArena
├── Main Camera
├── Background
├── RoadCenterMarking
├── LeftBoundary
├── RightBoundary
├── PlayerCar (physics root + ArmoredCarVisual prefab)
├── ChallengerCar (physics root + ChallengerCarVisual prefab)
├── BattleArenaController
└── BattleHUD
    ├── PlayerName
    ├── ChallengerName
    ├── PlayerHealthBar
    ├── ChallengerHealthBar
    ├── StartCue
    └── ResultPanel
```

`BattleCarVisual` prefabs contain the body, separate rotating front/rear wheel transforms, and wheel-dust anchors. They are children of the existing physics roots so presentation changes do not change collision geometry or combat calculations. See [ArmoredCarArt.md](ArmoredCarArt.md) for source sprite and prefab paths.

## Fight flow and tuning

```text
Preparing → Approaching ⇄ Fighting → Results → GarageHub
```

The controller initializes player Engine, Weapon, and Armor from `GameSession.GarageProgress`; challenger values remain serialized tuning fields. Cars drive toward one another, apply contact damage on timed intervals, and push in alternating surges. End boundaries are solid: wall contact rebounds a car and applies a small impact, while health depletion or the 60-second time limit resolves the fight once. A time-limit result awards the win to the car with the higher remaining health ratio. Both outcomes show the winner for 10 seconds, then return to `GarageHub`.

Health bars are rectangular, with player health filling from the left and challenger health from the right. Keep their names, colors, and fill images on the HUD so the presentation can be adjusted without changing combat logic.

Current battle presentation includes fast wheel rotation, short body recoil/sway during contact, translucent warm road-dust clouds from wheel anchors, soft shadows beneath all four wheels, reduced camera shake, and a brief HUD shake with faint motion trails on impact. At battle completion, both cars' tire dust is cleared immediately; the loser stops and emits dark smoke from four points around the engine/body throughout the 10-second result display. The separate smoke plumes use the dust puff texture with darker color and independent alpha. The scene no longer includes the `ForegroundRoad` block, leaving the arena background's road visible. The configured loop asset is `Audio/Music/Locked_At_Redline_BattleLoop.wav`, sourced from `Locked_At_Redline.mp3`. It is a 60-second stereo loop with a crossfade; the selected source window begins at approximately 14 seconds and its 2.5-second tail/head overlap crossfades to reduce the loop seam. `BattleArenaController` starts the looping 2D AudioSource when the fight begins.

## Responsibilities

- `BattleArenaController` owns the fight state, initializes stats, starts the countdown and music, applies damage and push surges, and resolves time, health, and wall outcomes.
- `CarMotor2D` drives and rebounds physics roots while keeping horizontal movement and no-roll behavior.
- `CrashReporter` reports car contact and wall impacts without applying repeated wall damage every physics frame.
- `BattleArenaCameraRig` applies short camera impulses at impacts.
- `BattleCarPresentation` animates wheels and body response, emits wheel dust and defeat smoke, and creates the wheel-contact shadows.
- `BattleResolver` maps the defeat condition to a winner; it does not update the UI or load scenes.
- `BattleHudView` and `BattleResultView` present health, countdown, and outcome.

## Verification record

On 2026-10-02, Unity compiled the battle scripts without errors. A Play Mode result preview confirmed four losing-car smoke emitters, four wheel shadows, seven HUD motion trails, and zero remaining tire-dust particles under the winner overlay. The preview showed the smoke too faintly, so smoke opacity, size, and emission were increased afterward; the new tuning still needs visual confirmation. The scene stores a 10-second results delay and no longer contains `ForegroundRoad`. A prior Play Mode run returned from `BattleArena` to `GarageHub` after resolution. A complete timed 10-second display, both winner paths, updated smoke visibility, loop-seam listening check, and build validation remain open.

## Remaining validation

- Play through a result for the full 10 seconds and verify both winner paths, contact timing, timeout resolution, and return to `GarageHub`.
- Listen across the generated music-loop seam and confirm the blend sounds natural in the game mix.
- Review dust density and visibility at the intended game resolution and during the full crash sequence.
- Validate supported aspect ratios and input devices, add planned EditMode/PlayMode tests, and produce a Windows x86_64 development build.

See [ImplementationChecklist.md](ImplementationChecklist.md) for project-wide status and [ImplementationPlan.md](ImplementationPlan.md) for broader architecture.
