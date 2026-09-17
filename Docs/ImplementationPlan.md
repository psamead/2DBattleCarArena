# 2D Battle Car Arena — Implementation Plan

Source brief: [Google design document](https://docs.google.com/document/d/12c7xQsV6o9KRW7UOS7EU85JhDNZQFtx2R8d-PCJijdw/edit?usp=sharing)

## Project baseline

- Unity 6000.3.24f1 using URP 2D.
- New Input System, uGUI, and Unity Test Framework are installed.
- `Assets/Scenes/StartMenu.unity` is currently the only enabled build scene.
- The project does not yet contain gameplay code, prefabs, tests, or an established application architecture.

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
  Audio/
  Data/
  Prefabs/
  Scenes/
  Scripts/
    Core/
    Progression/
    Garage/
    Battle/
    UI/
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

- Credits, score, and rank display.
- Engine, weapon, and armor upgrade cards.
- Current value, next value, and upgrade cost.
- Disabled purchase state when credits are insufficient.
- Go to Mission action.

Acceptance criteria:

- Start opens the Garage once the Garage scene is added to Build Settings.
- Purchases update the UI immediately and cannot create negative credits.
- Progress survives scene changes and application restarts.

## Phase 3 — Battle Arena prototype

Implement the battle flow as `Preparing → Approaching → Collided → Resolving → Results`.

Key components:

- `BattleController`
- `CarMotor2D`
- `CrashReporter`
- `BattleResolver` as testable plain C#
- `EnemyFactory`
- `BattleResultView`

Use `Rigidbody2D` velocity in `FixedUpdate`, continuous collision detection, and a one-shot collision guard. Start with the brief's formula: `Horsepower + Damage + Armor`.

Acceptance criteria:

- Cars approach and collide automatically.
- Each battle resolves and awards rewards exactly once.
- Identical inputs produce identical resolver results.
- Result UI displays both sides and the outcome.

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

