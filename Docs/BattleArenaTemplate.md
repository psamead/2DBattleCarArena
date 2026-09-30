# Battle Arena Template

`Assets/_Project/Scenes/BattleArena.unity` is the one-fight blockout loaded by the Garage Hub's **Go to Mission** button. It uses plain colored blocks and TextMeshPro UI so the scene layout and mechanics can be reviewed before final assets or a visual style are added. The named scene objects and serialized slots are the replacement points for later art.

## Prototype goal

Build one complete car-versus-challenger fight using the current Garage upgrades. Start with a plain blockout scene: a background, foreground road, two opposing block cars, and a wall at each end. Keep the visuals replaceable and leave the scene without a finished art style.

The planned state flow is:

```text
Preparing → Approaching ⇄ Fighting → Results
```

Show a short `GET READY` cue before the cars start moving toward each other. This prototype contains exactly one fight and does not loop into another round. Collision and wall callbacks must end the fight only once.

## Blockout scene structure

The saved blockout currently uses this scene hierarchy. Replace the presentation objects with final art as it becomes available, while keeping the controller and HUD references intact:

```text
BattleArena
├── Main Camera
├── Background
├── ForegroundRoad
├── RoadCenterMarking
├── LeftBoundary
├── RightBoundary
├── PlayerCar
├── ChallengerCar
├── BattleArenaController
└── BattleHUD
    ├── PlayerName
    ├── ChallengerName
    ├── PlayerHealthBar
    ├── ChallengerHealthBar
    ├── StartCue
    └── ResultPanel
```

Use the existing `BattleArena` placeholder scene and keep it enabled in Build Settings. Add the smallest set of scene objects needed for the first playable battle. Keep backgrounds, car sprites, result-panel art, and fonts in named replaceable theme or Inspector slots where practical.

## Planned responsibilities

- `BattleArenaController` owns the single-fight state, initializes player stats from `GameSession.GarageProgress`, starts the countdown, applies contact damage, and stops both cars when the fight ends.
- `CarMotor2D` pushes its car horizontally using engine power. Use `Rigidbody2D` and freeze rotation so the cars collide and push without rolling or flipping.
- `CrashReporter` reports car contact and end-wall contact to the controller. A one-shot end guard prevents duplicate result presentation.
- `BattleResolver` is plain C# and maps the defeat condition to the winner. It does not compare a single sum of car stats to decide the fight.
- `BattleHudView` shows `PLAYER` and `CHALLENGER` names, current/max health bars, and the start cue. `BattleResultView` shows the winner and whether health or a wall ended the fight.

Keep scene behavior in the controller and motors; keep the final outcome mapping in the resolver. Challenger stats are serialized blockout values on the controller so they can be tuned without changing code. Replace the blockout sprites and background through the scene's named visual slots when art is ready.

## Initial combat rule

For this scene prototype, use a short health-based fight:

```text
maximum health = Armor
damage per contact tick = opponent's Weapon damage
push force = Engine power × configurable force scale
```

Use the player's current Engine, Weapon, and Armor stats from `GameSession.GarageProgress`. Apply damage at a configurable interval while the cars remain in contact. Use continuous collision detection and horizontal movement only. The current challenger blockout defaults are Engine 220, Weapon 15, and Armor 140; contact damage ticks every 0.65 seconds, and each engine point supplies 0.18 units of push force with a 2.75 units/second speed cap. These values demonstrate the mechanic and remain tunable; they are not final balance.

Each end wall is a loss condition for the car that touches it. A health bar reaching zero is also a loss. Preserve the original sample's player-win tie rule if both health values reach zero on the same tick. There is one fight and no next round. This phase presents the outcome but does not award battle rewards or implement rank progression.

## Result and reward flow

`BattleResolver` returns a result without editing UI or loading scenes. `BattleArenaController` stops movement and asks `BattleResultView` to display the winner and the reason for the result.

The result overlay remains visible briefly, then the game returns to `GarageHub` after either a win or a loss. The return delay is configurable on `BattleArenaController`. Rank-up routing belongs to the Results and rank progression phase. When that phase is implemented, apply and save rewards before leaving the result screen, do not deduct Score on a loss unless the design changes, and trigger each newly crossed rank only once.

## Acceptance criteria

- Go to Mission opens the `BattleArena` scene from the Garage Hub.
- The scene shows two named cars, two health bars, a background, a foreground road, and walls at both ends using blockout visuals.
- `GET READY` appears before the cars begin driving toward each other.
- Both cars collide and push horizontally; their bodies do not roll or flip.
- Contact damage lowers each health bar, and a car at zero health loses.
- A car touching either end wall loses.
- The first defeat condition ends the fight once, stops both cars, and shows the winner and defeat reason.
- After showing the result briefly, both wins and losses return to `GarageHub`.
- The fight does not restart or automatically advance to a second round. The result overlay shows the winner and `HEALTH DEPLETED` or `PUSHED INTO THE WALL`.
- The player stats used by the battle come from the current Garage session.
- Placeholder visuals can be swapped without changing battle calculations.

## Deferred work

- Implement rank thresholds, save persistence, and rank-up routing.
- Tune challenger stats and contact-damage timing after playtesting the blockout.
- Add collision feedback, audio routing, camera shake, and effects.
- Add EditMode coverage for deterministic battle calculations and PlayMode coverage for collision guards and result flow.
- Validate controller/keyboard navigation, aspect ratios, and a Windows development build.

See [ImplementationPlan.md](ImplementationPlan.md) for the broader architecture and [ImplementationChecklist.md](ImplementationChecklist.md) for implementation status.
