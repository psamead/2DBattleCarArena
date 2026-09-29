# Battle Arena Template

`Assets/_Project/Scenes/BattleArena.unity` is currently an empty placeholder loaded by the Garage Hub's **Go to Mission** button. This document defines the next implementation phase. It is a design and handoff document; the battle scene and gameplay systems described here have not been implemented yet.

## Prototype goal

Build one complete, deterministic car-versus-enemy battle using the current Garage upgrades. The first pass should use replaceable placeholder visuals and resolve automatically after the cars approach and collide.

The planned state flow is:

```text
Preparing → Approaching → Collided → Resolving → Results
```

Each battle must transition forward once. Collision callbacks, animation callbacks, and UI input must not resolve or award the same battle more than once.

## Planned scene structure

Keep scene-owned objects under a clear root so a future arena presentation can be replaced without changing the battle rules:

```text
BattleArena
├── Main Camera
├── BattleController
├── ArenaPresentation
│   ├── BackgroundSlot
│   ├── PlayerCarSlot
│   ├── EnemyCarSlot
│   └── ResultPanel
└── EventSystem
```

Use the existing `BattleArena` placeholder scene and keep it enabled in Build Settings. Add the smallest set of scene objects needed for the first playable battle. Keep backgrounds, car sprites, result-panel art, and fonts in named replaceable theme or Inspector slots where practical.

## Planned responsibilities

- `BattleController` owns the state flow, creates a snapshot of the battle inputs, starts the approach, accepts the first collision, invokes resolution once, and presents the result.
- `CarMotor2D` moves a car with `Rigidbody2D` velocity from `FixedUpdate`. Configure continuous collision detection for the moving cars.
- `CrashReporter` reports a collision to the controller. A one-shot guard prevents duplicate resolution if multiple colliders or callbacks fire.
- `BattleResolver` is plain C# with no scene or Unity object dependencies. Given the same input, it returns the same outcome and combat values.
- `EnemyFactory` creates an enemy from an explicit battle configuration or deterministic input. Avoid hidden random state in the resolver.
- `BattleResultView` presents the player and enemy values, the outcome, and any awarded Score or Credits.

Keep scene behavior in the controller and motors; keep combat calculations and result decisions in the resolver. Feed the resolver snapshots of the Garage stats and enemy configuration so mid-battle scene changes cannot change an in-progress calculation.

## Initial combat rule

The current implementation plan starts from the source brief's power comparison:

```text
combat power = Horsepower + Damage + Armor
```

Use the player's current Engine, Weapon, and Armor stats from `GameSession.GarageProgress`. Define the enemy's values in explicit configuration rather than scattering constants through scene scripts. Keep the inputs and returned result visible to tests and logs.

The original sample treats a tie as a player win. Preserve that rule for the first prototype unless the game design is updated. Keep the exact enemy setup and battle reward amounts configurable; this template does not assign values that have not been decided.

## Result and reward flow

`BattleResolver` should return a result without directly editing UI or loading scenes. `BattleController` applies the result once through the progression owner, then asks `BattleResultView` to display it. The result should contain enough information to show both cars' combat power and the winner.

The first arena phase may present the outcome in place. The return and rank-up loop belongs to the Results and rank progression phase. When that phase is implemented, apply and save rewards before leaving the result screen, do not deduct Score on a loss unless the design changes, and trigger each newly crossed rank only once.

## Acceptance criteria

- Go to Mission opens the `BattleArena` scene from the Garage Hub.
- The battle enters `Preparing`, then both cars approach and collide automatically.
- The first valid collision advances the battle once; duplicate collision notifications do not duplicate resolution or rewards.
- `BattleResolver` is deterministic for identical player and enemy inputs.
- The power comparison follows the configured rule, including the tie behavior.
- The result view shows both sides and the outcome.
- The player stats used by the battle come from the current Garage session.
- Placeholder visuals can be swapped without changing battle calculations.

## Deferred work

- Configure concrete enemy stats and the battle reward values.
- Implement rank thresholds, save persistence, and return-to-Garage/rank-up routing.
- Add collision feedback, audio routing, camera shake, and effects.
- Add EditMode coverage for deterministic battle calculations and PlayMode coverage for collision guards and result flow.
- Validate controller/keyboard navigation, aspect ratios, and a Windows development build.

See [ImplementationPlan.md](ImplementationPlan.md) for the broader architecture and [ImplementationChecklist.md](ImplementationChecklist.md) for implementation status.
