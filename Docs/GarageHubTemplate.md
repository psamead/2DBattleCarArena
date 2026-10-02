# Garage Hub Template

`Assets/_Project/Scenes/GarageHub.unity` is the prototype Garage Hub reached from Start Game. It provides a layout for the future garage screen while keeping its artwork and font replaceable through one theme asset.

## Current scene setup

- The background starts with a neutral garage image and crossfades to the alternate warmer image, then back. The current theme asset uses an 8-second full cycle.
- A pale blue-white overlay makes a brief, subtle flicker occasionally.
- Background motion uses unscaled time, so it continues during pause or slow motion.
- `Under_The_Chassis.mp3` is played as looping 2D Garage Hub music.
- The Main Camera has an AudioListener so Unity can hear the Garage Hub AudioSource.
- The energy panel is named `EnergyPanel`, its label reads `ENERGY`, and its value displays the current session energy percentage.
- The title currently uses the ordinary `LiberationSans SDF` font.
- The Machine panel shows the front-view pixel-art car (see Car preview below).
- In Play Mode, the neutral and warm car sprites crossfade with the background images using the same breathing cycle and blend value. The warm variant is toned down by its theme tint. Separate matching car overlays follow the background flicker's pulse timing and strength.
- Engine, Weapon, and Armor purchases are handled by `GarageHubController` and the session-owned `GarageProgress`. A purchase spends the card's cost, adds its `+UP` amount to both that vehicle stat and Score, then refreshes Credits, Score, the stat value, and purchase availability. Starting values are 1000 Credits, 500 Score/health, Engine 100 (+145 for 250 Credits), Weapon 100 (+55 for 300 Credits), and Armor 100 (+82 for 275 Credits). Energy starts at 100%; each completed battle round removes 20%, and every third completed round restores 20% after that round's cost. A victory adds 5% of the current Score, rounded up, and grants 200 Credits. These values last while `GameSession` remains alive; they are not saved between application launches.
- The Engine, Weapon, and Armor `Upgrade` buttons receive pointer clicks after the theme is applied. In Play Mode, clicking a purchase button was verified to deduct Credits, add the upgrade amount to Score and the matching stat, and refresh the displayed values. The card art and labels outside the `Upgrade` button strip are decorative and are not separate purchase targets.
- The Engine, Weapon, Armor, Energy, Credits, Score, and Rank panels use the shared sliced rusted-steel frame at `Assets/_Project/Art/UI/GarageHub/GarageHubPanelFrame_RustedSteel.png`. Replace the `Slot Frame` field on `DefaultGarageHubTheme` to swap it for another frame; its nine-slice border preserves the corners when each panel scales.
- The `GoToMissionButton` uses the replaceable `Go To Mission Button Frame` theme slot, currently assigned to `Assets/_Project/Art/UI/GarageHub/GoToMissionFrame_RustedSteel.png`; the transparent padding was trimmed to fit the current 470×86 button. `BackToMenuButton` keeps the shared action-button frame. Both buttons use the special 04B pixel font. The frame and font are theme fields, so they can be replaced without changing the presentation code. The sprites are rendered as simple stretched images (not nine-sliced); adjust each button's RectTransform to change its size.
- Confirming `GoToMissionButton` plays the Garage Hub theme's shared low-metal UI confirmation cue through the persistent `GameSession` audio source, so the cue continues across the scene change.
- After the first battle victory, confirming the result loads `GarageTransit` and opens the door without a car in the transit interior, then returns to this scene where the car is shown. The transit garage background and car stay rendered above GarageHub while both scenes crossfade for one second; the GarageHub title, upgrade cards, stat panels, and buttons fade in during that crossfade. Initial Start Game still enters GarageHub directly before the player has won a battle; later Start Game entry uses an opening-door transit with the car visible and the same background/car crossfade.
- Before the player's first victory, GarageHub exits directly to `BattleArena` or `StartMenu` without loading `GarageTransit`. After the first victory, Go to Mission fades out GarageHub UI, opens the transit door with the car visible, then loads `BattleArena`. Back to Menu fades out the GarageHub UI while keeping its car preview fully visible; the transit scene keeps the car in view while the door closes, then loads `StartMenu`. The door frame and moving panel are separate sprites and can be replaced through `DefaultGarageDoorTransitionTheme`.
- Returning from battle takes 3 seconds total: 2 seconds for the shutter and interior lighting, followed by a 1-second GarageHub reveal. Both GarageHub exits take 3 seconds total: a 0.45-second UI fade and a 2.55-second door animation (opening for Go to Mission, closing for Back to Menu). The transit interior darkens from 85% to 20% as the door opens; the GarageHub reveal then fades that remaining darkness out. Its car uses the GarageHub preview slot's 813×455 layout and position. `GarageTransit` has a dedicated Main Camera and AudioListener.
- Go to Mission routes to `BattleArena`; Back to Menu routes to `StartMenu`. See [BattleArenaTemplate.md](BattleArenaTemplate.md) for battle rules. The session remains alive across scene changes, so garage Credits, Score, Energy, and upgraded stats are retained during the same play session.
- The first-winner garage entrance and door transition paths are implemented, but their timing and visual alignment still need Play Mode verification.

## Swap assets

Select `Assets/_Project/Data/UI/GarageHub/DefaultGarageHubTheme.asset`. Its replaceable fields are:

- Neutral Background and Alternate Background
- Title Logo
- Car Preview and the separate Engine Upgrade Icon, Weapon Upgrade Icon, and Armor Upgrade Icon fields
- Panel and Button sprites
- Action Button Frame (shared by Back to Menu and other action buttons)
- Go To Mission Button Frame (the dedicated rusted-steel frame for Go to Mission; falls back to Action Button Frame when empty)
- Title Font (applied to the Garage Hub TextMeshPro elements)
- Action Button Font (applied to the Go to Mission and Back to Menu labels)
- Background Music

The scene's `GarageHubPresentation` component references that theme. Leave an optional sprite field empty to keep the corresponding prototype placeholder. Assign another TMP font asset to the Title Font or Action Button Font field to change those text styles independently.

## Upgrade icons

The three transparent PNGs are imported as single sprites and assigned separately in `DefaultGarageHubTheme`:

- Engine: `Assets/_Project/Art/UI/GarageHub/Upgrades/Engine_Supercharger.png`
- Weapon: `Assets/_Project/Art/UI/GarageHub/Upgrades/Weapon_HiddenMachineGun.png`
- Armor: `Assets/_Project/Art/UI/GarageHub/Upgrades/Armor_FrontBumper.png`

Replace the corresponding Engine, Weapon, or Armor Upgrade Icon field to swap one card's art. The machine gun is a standalone removable item; it does not alter the car preview. The legacy shared Upgrade Icon field remains available as a fallback when a dedicated icon is empty.

## Car preview

- Sprites: `Assets/_Project/Art/UI/GarageHub/GarageCarPreview_Front.png` (neutral base) and `Assets/_Project/Art/UI/GarageHub/GarageCarPreview_OrangeGlow.png` (warm alternate, toned down through the theme tint; transparent background).
- The neutral sprite is assigned to the theme's `Car Preview` field and directly to `GarageHubTemplate/Canvas/Content/CarPreviewPanel/CarPreviewArtSlot` so it is visible in Edit Mode. The warm sprite is assigned to `Alternate Car Preview` and to the sibling `CarPreviewAlternateArtSlot` Image for direct Canvas editing.
- `CarPreviewArtSlot` is 813×498 (anchored position y = -128.46) with Preserve Aspect enabled. `CarPreviewAlternateArtSlot` uses the same anchors and size, with a small scale and position correction to align the visible car art. Select it in the Canvas hierarchy to adjust it further; in Play Mode the presentation script controls its alpha for the crossfade.
- Import settings match the Start Menu car sprite (Sprite, no mipmaps, same filter mode and compression).
- The `ArtSlotLabel` child ("CAR ART SLOT") is still active and draws over the grille; disable it when the art is final.
- To swap the car, import the new sprite into `Art/UI/GarageHub`, then assign it to both the theme field and the slot's Image. Keep the art near a 1.63:1 aspect ratio or rely on Preserve Aspect.

## Adjust background motion

On the same theme asset, tune the Background Motion fields:

- `Breathing Cycle Seconds`: time for one full transition from neutral to alternate and back (currently 8 seconds).
- `Flicker Interval Minimum` and `Flicker Interval Maximum`: randomized time between flicker bursts (currently 4–8 seconds).
- `Flicker Duration`: length of each burst (currently 0.3 seconds).
- `Flicker Frequency Hz`: how quickly the light pulses within a burst (currently 50 Hz).
- `Flicker Strength`: maximum overlay opacity (currently 0.02).
- `Random Seed`: zero chooses a different sequence each Play session; a nonzero seed makes the randomized intervals repeatable (currently 10).

The current short duration and low strength are subtle starting values. Increase them in the theme asset if you want the flicker to be more visible; the effect code does not need to change.

## Audio

`Background Music` is set to `Assets/_Project/Audio/Music/Under_The_Chassis.mp3`. The Garage Hub AudioSource loops the clip, uses 2D playback, and takes its volume from the theme's `Music Volume` field. To replace the track, assign a different AudioClip in the theme asset.

## Project folders

```text
Assets/_Project/
  Art/UI/StartMenu/       Start Menu logo, button, and foreground sprites
  Art/UI/GarageHub/       Garage Hub car preview, frame, and upgrade sprites
    Upgrades/             Separate Engine, Weapon, and Armor icons
  Art/UI/GarageDoor/      Separate fixed garage frame and moving shutter sprites
  Data/UI/StartMenu/      Start Menu theme asset
  Data/UI/GarageHub/      Garage Hub theme asset
  Data/UI/GarageDoor/     Replaceable garage door transition theme
  Scripts/UI/StartMenu/   Start Menu behavior and template builder
  Scripts/UI/GarageHub/   Garage Hub theme and presentation behavior
  Scripts/UI/GarageDoor/  Transit behavior and editor scene builder
```

## Porting to another laptop

Open the project with Unity 6000.3.24f1. Clone the GitHub repository (or copy the whole project), retaining `.meta` files, `Packages`, `ProjectSettings`, `Docs`, `.gitattributes`, and the actual Git LFS art/audio content. After cloning, run `git lfs install` and `git lfs pull` so the image and audio files are downloaded rather than left as LFS pointer files.

Install and sign in to Codex on that laptop, open the project folder as the workspace, and open the same project in Unity. Let Unity finish importing before using Codex with the project. Select GPT-6 in Codex for continued work. If this task is not available on the other laptop, start a new one with this document, `ImplementationPlan.md`, and `ImplementationChecklist.md` as the handoff context. The local Unity `Library` folder can be regenerated.
