# 2D Battle Car Arena — Implementation Checklist

This checklist tracks verified work from [ImplementationPlan.md](ImplementationPlan.md). Check an item only after its implementation has been validated in Unity.

Last updated: 2026-09-30

## Project organization

- [x] Keep project-owned scenes under `Assets/_Project/Scenes`.
- [x] Keep source fonts under `Assets/_Project/Art/Fonts`.
- [x] Keep music under `Assets/_Project/Audio/Music`.
- [x] Keep Start Menu artwork under `Assets/_Project/Art/UI/StartMenu`.
- [x] Keep Start Menu and Garage Hub theme assets in their respective `Assets/_Project/Data/UI` subfolders.
- [x] Keep feature scripts in `Assets/_Project/Scripts/UI/StartMenu` and `Assets/_Project/Scripts/UI/GarageHub`.

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
- [x] Store replaceable Start Menu art, font, and music in `Data/UI/StartMenu/DefaultStartMenuTheme.asset`.

### Garage

- [x] Create the `GarageHub` prototype layout scene and add it to Build Settings.
- [x] Add a Garage Hub theme asset with replaceable background, logo, car preview, upgrade-card art, panel, button, font, and music slots.
- [x] Crossfade the two Garage Hub background sprites with adjustable cycle time.
- [x] Add a subtle flicker overlay with adjustable frequency, interval range, duration, strength, and random seed.
- [ ] Visually verify the neutral and warm car crossfade and flicker sync with the backgrounds in Play Mode.
- [x] Add a reusable sliced Garage Hub frame for the Engine, Weapon, Armor, Energy, Credits, Score, and Rank panels.
- [x] Style the Go to Mission and Back to Menu buttons with the Start Menu frame sprite and special font through Garage Hub theme fields.
- [x] Create and assign separate replaceable Engine, Weapon, and Armor upgrade icons.
- [x] Loop `Under_The_Chassis.mp3` as Garage Hub background music.
- [x] Add an AudioListener to the Garage Hub Main Camera.
- [x] Rename `BatteryPanel` to `EnergyPanel` and its displayed label to `ENERGY`.
- [x] Verify the Start Game action loads `GarageHub` in Play Mode.
- [x] Document Garage Hub setup and asset swapping in [GarageHubTemplate.md](GarageHubTemplate.md).
- [ ] Display credits, score, and rank.
- [ ] Implement engine, weapon, and armor upgrade card values and purchase controls.
- [ ] Display current value, next value, and upgrade cost.
- [ ] Disable purchases when credits are insufficient.
- [ ] Connect the Go to Mission button to a mission/battle flow.
- [ ] Connect the Back to Menu button to return to the Start Menu.
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
