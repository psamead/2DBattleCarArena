# 2D Battle Car Arena — Implementation Checklist

This checklist tracks verified work from [ImplementationPlan.md](ImplementationPlan.md). Check an item only after its implementation has been validated in Unity.

Last updated: 2026-09-18

## Phase 1 — Foundation and progression

### Application foundation

- [x] Add one automatically bootstrapped `GameSession` composition root.
- [x] Keep `GameSession` alive across scene loads with `DontDestroyOnLoad`.
- [x] Reject duplicate `GameSession` instances.
- [x] Add shared scene navigation owned by `GameSession`.
- [x] Route the Start Menu scene transition through `GameSession`.

### Progression and persistence

- [ ] Create the authored `GameBalance` configuration.
- [ ] Create the serializable `PlayerProgress` runtime model.
- [ ] Create `PlayerProgressService` as the authoritative progress owner.
- [ ] Implement deterministic `UpgradeService` prices and derived stats.
- [ ] Reject invalid purchases without mutating progress.
- [ ] Implement `JsonPlayerProgressStore` under `Application.persistentDataPath`.
- [ ] Recover safely from missing or corrupt save data.
- [ ] Add EditMode tests for defaults, upgrades, purchases, and save recovery.

## Phase 2 — Start Menu and Garage

### Start Menu

- [x] Create a replaceable full-screen background slot.
- [x] Create Start Game and Quit Game selection bars.
- [x] Support pointer, keyboard, and controller navigation.
- [x] Use a responsive 1920x1080 reference layout.
- [x] Center the menu panel.
- [x] Wire Start Game to the future `GarageHub` scene.
- [x] Wire Quit Game for Editor and player builds.

### Garage

- [ ] Create `GarageHub` and add it to Build Settings.
- [ ] Display credits, score, and rank.
- [ ] Create engine, weapon, and armor upgrade cards.
- [ ] Display current value, next value, and upgrade cost.
- [ ] Disable purchases when credits are insufficient.
- [ ] Add the Go to Mission action.
- [ ] Refresh the UI immediately after purchases.

## Phase 3 — Battle Arena prototype

- [ ] Create the `BattleArena` scene and battle-state flow.
- [ ] Implement `BattleController`, `CarMotor2D`, and `CrashReporter`.
- [ ] Implement deterministic `BattleResolver` logic.
- [ ] Implement `EnemyFactory` and `BattleResultView`.
- [ ] Resolve and reward each battle exactly once.

## Phase 4 — Results and rank progression

- [ ] Apply and save battle rewards before leaving results.
- [ ] Implement configured rank tiers and one-time rank-up handling.
- [ ] Complete the Garage to Battle to Results return loop.

## Phase 5 — Polish and validation

- [ ] Route music and sound effects through mixer groups.
- [ ] Add collision feedback, particles, camera shake, and button feedback.
- [ ] Validate common aspect ratios and supported input devices.
- [ ] Add the planned EditMode and PlayMode test coverage.
- [ ] Produce and launch a Windows x86_64 development build.
