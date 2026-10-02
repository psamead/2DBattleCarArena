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

The player's maximum battle health comes from Garage Score, which starts at 500 and is displayed as the player's health value in the BattleArena HUD. Engine, Weapon, and Armor power come from `GameSession.GarageProgress`; upgrades and the 5% victory reward increase Score and therefore raise the player's next battle health maximum. Each victory also grants 200 Credits. Challenger health and each power value are independently randomized within ±20% of the player's corresponding value at battle start. A car hit applies the attacker's Engine power; while armor charges remain, the defender takes `max(0, engine power - armor power)`. Gun shots fire for both cars every 5 seconds and use the same formula with Weapon power. Each car can use armor for 3 car hits and 3 gun shots independently; after that attack-type counter reaches zero, that armor no longer reduces that type of damage. Car hits occur at the existing 5.5-second contact interval. There is no battle time limit: the fight ends when a car's health reaches zero or it loses at a boundary. Both outcomes show a themed result panel. Before the first victory, confirmation returns to GarageHub directly; after a victory, it plays the shared low-metal UI cue and routes through `GarageTransit` to return, opening the door without the car in the transit interior. See [GarageHubTemplate.md](GarageHubTemplate.md) for all garage door routes.

Health bars are rectangular, with player health filling from the left and challenger health from the right. Keep their names, colors, and fill images on the HUD so the presentation can be adjusted without changing combat logic.

Current battle presentation includes fast wheel rotation, short body recoil/sway during contact, translucent warm road-dust clouds from wheel anchors, soft shadows beneath all four wheels, reduced camera shake, and a brief HUD shake with faint motion trails on impact. Player and challenger labels and health values use the 04B_21 UI font with drop shadows. Health bars use the GarageHub rusted-steel panel art as a frame with cyan and purple fills. The arena boundaries are invisible but retain their colliders. At battle completion, both cars' tire dust is cleared immediately; the loser stops and emits dark smoke from four points around the engine/body while the result panel is shown. Smoke plumes use the dust puff texture with darker color; low-rate smoke also runs during the battle. The result panel uses `Art/UI/BattleArena/BattleResultFrame_RustedSteel.png` and acts as the **CLICK TO RETURN** button. The scene no longer includes the `ForegroundRoad` block, leaving the arena background's road visible. The configured loop asset is `Audio/Music/Locked_At_Redline_BattleLoop.wav`, sourced from `Locked_At_Redline.mp3`. It is a 60-second stereo loop with a crossfade; the selected source window begins at approximately 14 seconds and its 2.5-second tail/head overlap crossfades to reduce the loop seam. `BattleArenaController` starts the looping 2D AudioSource when the fight begins. `Audio/SFX/Battle/ManshaOfficialDieselEngine.mp3` layers a second looped engine bed over the battle music; it fades in at **FIGHT!** and fades to a quieter level on the result screen. `Audio/SFX/Battle/DragonStudioCarEngine.mp3` plays once during the pre-fight countdown as the engine startup cue. Each crash pulse restarts the clip and cuts it off after a random 0.25–0.85-second impact window, rather than looping or playing the full 3.08-second clip. The impact sound stops when cars separate or the battle ends; startup and impact volumes are adjustable on the controller.

## Responsibilities

- `BattleArenaController` owns the fight state, initializes stats, starts the countdown and music, applies damage and push surges, resolves time, health, and wall outcomes, and routes result confirmation back to the Garage Hub.
- `CarMotor2D` drives and rebounds physics roots while keeping horizontal movement and no-roll behavior.
- `CrashReporter` reports car contact and wall impacts without applying repeated wall damage every physics frame.
- `BattleArenaCameraRig` applies short camera impulses at impacts.
- `BattleCarPresentation` animates wheels and body response, emits wheel dust and defeat smoke, and creates the wheel-contact shadows.
- `BattleResolver` maps the defeat condition to a winner; it does not update the UI or load scenes.
- `BattleHudView` and `BattleResultView` present health, countdown, and outcome.

## Verification record

On 2026-10-02, Unity compiled the battle scripts without errors. A Play Mode result preview confirmed four losing-car smoke emitters, four wheel shadows, seven HUD motion trails, and zero remaining tire-dust particles under the winner overlay. The smoke opacity, size, and emission were subsequently increased; this tuning still needs visual confirmation. The result flow was changed to wait for a player click before returning to `GarageHub`; both result paths and the confirmation interaction need fresh Play Mode verification. The scene no longer contains `ForegroundRoad`. The loop-seam listening check and build validation remain open.

On 2026-10-03, the mechanic implementation compiled after a Unity asset refresh, and the Unity Console reported no errors. The combat timing, armor charge depletion, challenger randomization, energy changes, victory reward, and Garage Energy display have not yet been observed in Play Mode. Battles have no time limit, player health follows Garage Score, and armor protects against three hits and three gunshots per vehicle.

## Remaining validation

- Play through both result paths, verify click-to-return to `GarageHub`, Score-linked health, three-hit/three-shot armor depletion, and that battles continue without a time limit.
- Listen across the generated music-loop seam and confirm the blend sounds natural in the game mix.
- Review dust density and visibility at the intended game resolution and during the full crash sequence.
- Validate supported aspect ratios and input devices, add planned EditMode/PlayMode tests, and produce a Windows x86_64 development build.

See [ImplementationChecklist.md](ImplementationChecklist.md) for project-wide status and [ImplementationPlan.md](ImplementationPlan.md) for broader architecture.
