# Start Menu Template

The Start Menu is intentionally asset-independent. It creates a complete uGUI layout from placeholder colors and TextMeshPro text, so art can be swapped without changing navigation code.

## Current behavior

- The template generates automatically when `StartMenu` runs and no baked `StartMenuController` exists.
- Start loads `GarageHub` when that scene is present in Build Settings.
- Exit quits a player build and stops Play Mode in the Editor.
- The first button is selected for keyboard/controller navigation.
- Canvas scaling uses a 1920×1080 reference resolution with a balanced width/height match.

## Bake the editable hierarchy into the scene

With `StartMenu.unity` open, use:

`Tools > 2D Battle Car Arena > Build Start Menu Template`

This creates `StartMenuTemplate` in the scene, saves the scene, and creates a default theme asset at:

`Assets/_Project/Data/UI/DefaultStartMenuTheme.asset`

The runtime generator detects the baked controller and will not create a duplicate.

## Swap assets later

Select `DefaultStartMenuTheme.asset` and assign:

- Background Sprite
- Button Sprite
- Font Asset
- Menu Music
- Background, panel, accent, text, muted text, and button colors

The background is the `BackgroundPanel` Image in the scene and can also be swapped directly in the Inspector. The title and both selection-bar labels remain editable TextMeshPro objects.

For generation without a baked hierarchy, create a `StartMenuTheme` asset at `Assets/Resources/UI/StartMenuTheme.asset`; the runtime bootstrap loads that conventional path automatically.
