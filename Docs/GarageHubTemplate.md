# Garage Hub Template

`Assets/_Project/Scenes/GarageHub.unity` is the prototype Garage Hub reached from Start Game. It provides a layout for the future garage screen while keeping its artwork and font replaceable through one theme asset.

## Current scene setup

- The background starts with a neutral garage image and crossfades to the alternate warmer image, then back. The current theme asset uses an 8-second full cycle.
- A pale blue-white overlay makes a brief, subtle flicker occasionally.
- Background motion uses unscaled time, so it continues during pause or slow motion.
- `Under_The_Chassis.mp3` is played as looping 2D Garage Hub music.
- The Main Camera has an AudioListener so Unity can hear the Garage Hub AudioSource.
- The energy panel is named `EnergyPanel` and its label reads `ENERGY`.
- The title currently uses the ordinary `LiberationSans SDF` font.
- Placeholder panels and art slots remain in place until final sprites are ready.

## Swap assets

Select `Assets/_Project/Data/UI/GarageHub/DefaultGarageHubTheme.asset`. Its replaceable fields are:

- Neutral Background and Alternate Background
- Title Logo
- Car Preview and Upgrade Icon
- Panel and Button sprites
- Title Font (applied to the Garage Hub TextMeshPro elements)
- Background Music

The scene's `GarageHubPresentation` component references that theme. Leave an optional sprite field empty to keep the corresponding prototype placeholder. The default title font is the ordinary Liberation Sans SDF; assign another TMP font asset later to change the Garage Hub typography.

## Adjust background motion

On the same theme asset, tune the Background Motion fields:

- `Breathing Cycle Seconds`: time for one full transition from neutral to alternate and back (currently 8 seconds).
- `Flicker Interval Minimum` and `Flicker Interval Maximum`: randomized time between flicker bursts (currently 4–8 seconds).
- `Flicker Duration`: length of each burst (currently 0.25 seconds).
- `Flicker Frequency Hz`: how quickly the light pulses within a burst (currently 4 Hz).
- `Flicker Strength`: maximum overlay opacity (currently 0.02).
- `Random Seed`: zero chooses a different sequence each Play session; a nonzero seed makes the randomized intervals repeatable (currently 10).

The current short duration and low strength are subtle starting values. Increase them in the theme asset if you want the flicker to be more visible; the effect code does not need to change.

## Audio

`Background Music` is set to `Assets/_Project/Audio/Music/Under_The_Chassis.mp3`. The Garage Hub AudioSource loops the clip, uses 2D playback, and takes its volume from the theme's `Music Volume` field. To replace the track, assign a different AudioClip in the theme asset.

## Project folders

```text
Assets/_Project/
  Art/UI/StartMenu/       Start Menu logo, button, and foreground sprites
  Data/UI/StartMenu/      Start Menu theme asset
  Data/UI/GarageHub/      Garage Hub theme asset
  Scripts/UI/StartMenu/   Start Menu behavior and template builder
  Scripts/UI/GarageHub/   Garage Hub theme and presentation behavior
```

## Porting to another laptop

Open the project with Unity 6000.3.24f1. Clone the GitHub repository (or copy the whole project), retaining `.meta` files, `Packages`, `ProjectSettings`, `Docs`, `.gitattributes`, and the actual Git LFS art/audio content. After cloning, run `git lfs install` and `git lfs pull` so the image and audio files are downloaded rather than left as LFS pointer files.

Install and sign in to Codex on that laptop, open the project folder as the workspace, and open the same project in Unity. Let Unity finish importing before using Codex with the project. Select GPT-6 in Codex for continued work. If this task is not available on the other laptop, start a new one with this document, `ImplementationPlan.md`, and `ImplementationChecklist.md` as the handoff context. The local Unity `Library` folder can be regenerated.
