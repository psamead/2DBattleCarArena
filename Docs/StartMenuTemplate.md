# Start Menu Template

The Start Menu is intentionally asset-independent. It creates a complete uGUI layout from placeholder colors and TextMeshPro text, so art can be swapped without changing navigation code.

## Current behavior

- The template generates automatically when `StartMenu` runs and no baked `StartMenuController` exists.
- Start loads `GarageHub` when that scene is present in Build Settings.
- Exit quits a player build and stops Play Mode in the Editor.
- The first button is selected for keyboard/controller navigation.
- Moving the pointer over either option plays the theme's hover sound. Clicking an option or activating it with keyboard/controller plays its confirmation sound.
- Canvas scaling uses a 1920×1080 reference resolution with a balanced width/height match.

## Bake the editable hierarchy into the scene

With `Assets/_Project/Scenes/StartMenu.unity` open, use:

`Tools > 2D Battle Car Arena > Build Start Menu Template`

This creates `StartMenuTemplate` in the scene and saves it. The default theme asset is stored at:

`Assets/_Project/Data/UI/StartMenu/DefaultStartMenuTheme.asset`

The runtime generator detects the baked controller and will not create a duplicate.

## Swap assets later

Select `DefaultStartMenuTheme.asset` and assign:

- Background Sprite
- Button Sprite
- Font Asset
- Menu Music
- Option Hover Sound
- Option Confirm Sound
- Background, panel, accent, text, muted text, and button colors

The current Start Menu sprites are grouped under `Assets/_Project/Art/UI/StartMenu`, and the procedural UI sounds are under `Assets/_Project/Audio/SFX/UI`. The theme asset is the main place to swap the background, button, logo, font, music, and menu sound effects. The title and both selection-bar labels remain editable TextMeshPro objects.

For generation without a baked hierarchy, create a `StartMenuTheme` asset at `Assets/Resources/UI/StartMenuTheme.asset`; the runtime bootstrap loads that conventional path automatically.

## Porting

Use Unity 6000.3.24f1 on another laptop. Copy or clone the whole project while preserving every `.meta` file, `Packages`, `ProjectSettings`, `Docs`, and the Git LFS content for art and audio. Unity can regenerate the local `Library` folder after opening the project.
