# Battle Arena

`Assets/_Project/Scenes/BattleArena.unity` is the single-fight arena loaded by the Garage Hub's **Go to Mission** button. The scene now uses side-view armored-car artwork and a battle HUD while keeping physics roots, presentation prefabs, and audio/effect resources as replaceable assets.

## Scene structure

```text
BattleArena
├── Main Camera
├── Background
├── ForegroundRoad
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

The controller initializes player Engine, Weapon, and Armor from `GameSession.GarageProgress`; challenger values remain serialized tuning fields. Cars drive toward one another, apply contact damage on timed intervals, and push in alternating surges. End boundaries are solid: wall contact rebounds a car and applies a small impact, while health depletion or the 60-second time limit resolves the fight once. A time-limit result awards the win to the car with the higher remaining health ratio. Both outcomes show a result briefly and then return to `GarageHub`.

Health bars are rectangular, with player health filling from the left and challenger health from the right. Keep their names, colors, and fill images on the HUD so the presentation can be adjusted without changing combat logic.

Current battle presentation includes fast wheel rotation, short body recoil/sway during contact, translucent warm road-dust clouds emitted from wheel anchors, and camera plus subtle HUD shake on impacts. The losing car stops and emits dark engine smoke during the result display. Dust and smoke use `Resources/BattleDustPuff.png` with `Resources/BattleDustPuff.mat`; particle colors and alpha are separate so the dust stays see-through while defeat smoke is dark. Replace or tune these separately from the car sprites. The configured loop asset is `Audio/Music/Locked_At_Redline_BattleLoop.wav`, sourced from `Locked_At_Redline.mp3`. It is a 60-second stereo loop with a crossfade; the selected source window begins at approximately 14 seconds and its 2.5-second tail/head overlap crossfades to reduce the loop seam. `BattleArenaController` starts the looping 2D AudioSource when the fight begins.

## Responsibilities

- `BattleArenaController` owns the fight state, initializes stats, starts the countdown and music, applies damage and push surges, and resolves time, health, and wall outcomes.
- `CarMotor2D` drives and rebounds physics roots while keeping horizontal movement and no-roll behavior.
- `CrashReporter` reports car contact and wall impacts without applying repeated wall damage every physics frame.
- `BattleArenaCameraRig` applies short camera impulses at impacts.
- `BattleCarPresentation` animates wheels and body response and emits wheel dust.
- `BattleResolver` maps the defeat condition to a winner; it does not update the UI or load scenes.
- `BattleHudView` and `BattleResultView` present health, countdown, and outcome.

## Verification record

On 2026-10-02, Unity compiled the updated battle scripts without errors. Play Mode showed the reduced-alpha dust while both cars remained visible. Direct effect activation reported the defeat-smoke system playing with live particles, and triggering the HUD shake moved its Canvas RectTransform by 1.61 × 0.58 UI units. A later Play Mode run was observed returning from `BattleArena` to `GarageHub` after the battle resolved. The result view was not captured at the moment of that transition. A dedicated full 60-second fight, both result paths, loop-seam listening check, and build validation remain open.

## Remaining validation

- Play through a complete fight to verify contact timing, timer resolution, and return to `GarageHub` for both outcomes.
- Listen across the generated music-loop seam and confirm the blend sounds natural in the game mix.
- Review dust density and visibility at the intended game resolution and during the full crash sequence.
- Validate supported aspect ratios and input devices, add planned EditMode/PlayMode tests, and produce a Windows x86_64 development build.

See [ImplementationChecklist.md](ImplementationChecklist.md) for project-wide status and [ImplementationPlan.md](ImplementationPlan.md) for broader architecture.
