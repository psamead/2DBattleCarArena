# 2D Battle Car Arena — Implementation Checklist

This checklist tracks verified work from [ImplementationPlan.md](ImplementationPlan.md). Check an item only after its implementation has been validated in Unity.

Last updated: 2026-10-03

## Project organization

- [x] Keep project-owned scenes under `Assets/_Project/Scenes`.
- [x] Keep source fonts under `Assets/_Project/Art/Fonts`.
- [x] Keep music under `Assets/_Project/Audio/Music`.
- [x] Keep Start Menu artwork under `Assets/_Project/Art/UI/StartMenu`.
- [x] Keep Start Menu and Garage Hub theme assets in their respective `Assets/_Project/Data/UI` subfolders.
- [x] Keep Garage Door transition art and theme in feature-named folders.
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
- [x] Display credits, score, and rank.
- [x] Implement engine, weapon, and armor upgrade values and purchase controls.
- [x] Display current value, next value, and upgrade cost.
- [x] Disable purchases when credits are insufficient.
- [x] Connect the Go to Mission button to the BattleArena placeholder scene.
- [x] Connect the Back to Menu button to return to the Start Menu.
- [x] Refresh the UI immediately after purchases.
- [x] Verify upgrade buttons receive UI raycasts and a Play Mode purchase updates Credits, Score, and the matching stat.

## Phase 3 — Battle Arena prototype

- [x] Create an empty `BattleArena` placeholder scene and add it to Build Settings.
- [x] Document the planned arena structure and battle flow in [BattleArenaTemplate.md](BattleArenaTemplate.md).
- [x] Build the plain blockout background, road, opposing cars, end walls, and health/name HUD.
- [x] Show a `GET READY` cue before the single fight begins.
- [x] Drive the cars toward each other and keep them from rolling.
- [x] Apply contact damage and end the fight when either car's health reaches zero.
- [x] End the fight when a car is pushed into an end wall.
- [x] Show the battle winner and result confirmation once; keep the scene to one fight.
- [x] Feed Engine, Weapon, and Armor upgrade values into movement, attacks, and defense.
- [x] Link player health to the starting Garage Score of 500 and randomize challenger health and each power within ±20% of the player.
- [x] Apply Engine power on car hits and Weapon power every 5 seconds, with separate 3-hit/3-shot Armor counters.
- [x] Deduct 20% Energy per completed round, restore 20% after every third round, and award 5% Score on victory.
- [ ] Verify the new battle rules, Garage Energy readout, and win reward in Play Mode.
- [x] Create separate fixed garage-frame and moving door-panel sprites in the Garage Door theme folder.
- [x] Build and enable the `GarageTransit` scene with opening and closing door animations.
- [x] Keep all garage routes direct before the first victory; route garage entry and exit through the door scene afterward, with animated GarageHub UI arrival and departure.
- [x] Show the car during mission departure and StartMenu return; hide it during the BattleArena-to-GarageHub door opening.
- [x] Crossfade the transit background and car into the GarageHub background and car on arrival.
- [ ] Verify first-win opening, GarageHub reveal, and both GarageHub closing routes in Play Mode.
- [x] Return to `GarageHub` after the player confirms either result.
- [x] Replace the blockout cars with player and challenger side-view visual prefabs.
- [x] Align both vehicle bodies and expose separate wheel transforms and dust anchors.
- [x] Use rectangular health bars with the challenger fill progressing from the right.
- [x] Add alternating push surges and wall rebounds.
- [x] Add fast wheel rotation, contact recoil, wheel dust texture/material, and impact camera shake; smoke-checked in Play Mode.
- [x] Reduce wheel-dust opacity and density so car sprites remain visible through the clouds.
- [x] Emit dark engine smoke from the losing car during the results cue and stop its combat presentation.
- [x] Add a small HUD shake alongside the camera shake on car impacts.
- [x] Show a themed, clickable result panel and return to `GarageHub` when the player confirms.
- [x] Style the player/challenger labels and health values with the 04B_21 UI font and shadows.
- [x] Use the rusted-steel result frame, framed cyan/purple health bars, and hide arena boundary sprites while retaining their colliders.
- [x] Clear tire-dust particles immediately when the battle completes.
- [x] Add four dark smoke plumes across the losing car's engine/body for its result display.
- [x] Show low-rate engine smoke during the battle and stronger smoke on the losing car after resolution.
- [x] Remove the `ForegroundRoad` overlay and add soft contact shadows beneath all four wheels.
- [x] Reduce camera shake and add short HUD motion trails for visible impact feedback.
- [x] Add and start a looping 60-second battle music asset derived from `Locked_At_Redline.mp3`.
- [x] Record battle art slots, effect assets, loop construction, and current verification limits in [BattleArenaTemplate.md](BattleArenaTemplate.md) and [ArmoredCarArt.md](ArmoredCarArt.md).
- [x] Observe one resolved Play Mode fight return from `BattleArena` to `GarageHub`.
- [ ] Confirm the increased loser-smoke opacity and volume in a fresh result preview.
- [ ] Verify both result paths, Score-linked health, three-hit/three-shot armor depletion, and battles continuing until health depletion or a boundary loss.
- [ ] Listen to the battle loop seam in-game and tune dust presentation after reviewing the full battle.

## Phase 4 — Results and rank progression

- [ ] Apply and save battle rewards before leaving results.
- [ ] Implement configured rank tiers and one-time rank-up handling.
- [ ] Add rank-up handling to the Garage to Battle to Garage loop.

## Phase 5 — Polish and validation

- [ ] Route music and sound effects through mixer groups.
- [ ] Add button feedback and route music and sound effects through mixer groups.
- [ ] Validate common aspect ratios and supported input devices.
- [ ] Add the planned EditMode and PlayMode test coverage.
- [ ] Produce and launch a Windows x86_64 development build.
