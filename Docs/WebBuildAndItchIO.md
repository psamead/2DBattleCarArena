# Web Build and itch.io Handoff

Last updated: 2026-10-04

## Current delivery

- Unity version: 6000.3.24f1, with Web Build Support installed.
- The four enabled scenes are `StartMenu`, `GarageHub`, `BattleArena`, and `GarageTransit`.
- A Web player build completed successfully. Its output is kept outside the Unity repository at `C:\Users\Administrator\Documents\UnityBuilds\2DBattleCarArena-WebGL\index.html\`.
- A second Web build completed successfully for GitHub Pages at `C:\Users\Administrator\Documents\UnityBuilds\2DBattleCarArena-WebGL-GitHubPages\index.html\`. It uses Unity's decompression fallback, and its runtime files use the `.unityweb` extension so static hosting does not need custom Brotli response headers.
- Keep `PlayerSettings > Web > Decompression Fallback` enabled when rebuilding for static hosting. This setting is saved in `ProjectSettings/ProjectSettings.asset`.
- The GitHub Pages deployment uses a separate `gh-pages` branch containing only the generated static Web build. It is a deployment artifact branch, not a second copy of the Unity source project. GitHub Pages is configured to publish `gh-pages` from `/(root)`, and the site is live at `https://psamead.github.io/2DBattleCarArena/` (HTTP 200 verified 2026-10-04).
- The output contains `index.html`, the `Build` runtime files, and `TemplateData`; keep this folder structure intact when hosting the build.
- Butler uploaded the first build to `psamead/battle-car-arena:html` (build `#2054722`, version 1). The itch.io project page is `https://psamead.itch.io/battle-car-arena`.
- The page is still a draft. The draft Secret URL is available from the page toolbar and should not be committed to this repository. Publish the page in itch.io settings to make the ordinary project URL available to everyone.
- The user confirmed the game starts and runs in Android Chrome. A desktop embedded screenshot showed Garage Hub side panels partly clipped; the cause has not been isolated, so check embedded and fullscreen layouts before treating desktop presentation as verified.

## Rebuilding and uploading

1. Open the project in Unity 6000.3.24f1 and confirm the four project scenes are enabled in Build Settings.
2. Select the Web platform and build to an output folder outside the repository. The output must include `index.html` and its sibling `Build` and `TemplateData` folders.
3. Install Butler, complete `butler login` in a browser, and verify the itch.io account email before the first upload.
4. Ensure the existing itch.io project is set to **HTML**. Push the entire Web output directory to the `html` channel:

   ```powershell
   C:\butler\butler.exe push "<Web output folder>" "psamead/battle-car-arena:html"
   ```

5. Check the result with:

   ```powershell
   C:\butler\butler.exe status "psamead/battle-car-arena:html"
   ```

Uploading the channel updates the playable files; it does not publish the itch.io page or change its visibility.

## GitHub Pages deployment

1. Build the Web player to `C:\Users\Administrator\Documents\UnityBuilds\2DBattleCarArena-WebGL-GitHubPages\index.html\` with **Decompression Fallback** enabled.
2. Publish the contents of that output directory (the `index.html`, `Build`, and `TemplateData` together) to the repository's `gh-pages` branch root. Do not commit the output into the Unity source tree.
3. GitHub repository Pages settings are configured for **Deploy from a branch**, `gh-pages`, `/(root)`.
4. Open `https://psamead.github.io/2DBattleCarArena/`. Rebuild and republish the `gh-pages` contents whenever the game changes.

## Browser layout follow-up

- The Start Menu and Garage Hub Canvas Scalers use a 1920×1080 reference resolution (16:9).
- `ProjectSettings/ProjectSettings.asset` currently sets the Web default size to 960×600 (16:10). This differs from the Canvas reference and should be compared against the itch.io embed dimensions if side UI clipping recurs. It is a possible contributor, not a confirmed cause.
- Validate the Start Menu and Garage Hub at the itch.io embedded size, fullscreen, and on desktop and mobile browsers before calling responsive layout complete.

## Build log notes

The successful build log also contained a TextMesh Pro `m_AtlasTextures` unassigned-reference exception and a warning that no `RuntimePipelineConfig` asset was found. Android Chrome startup succeeded, but inspect TMP text and any Pipeline-dependent features in the browser build before a public release. A failed `FailedDownload.wav` import came from the Unity AI Assistant package and was not shown to be a game dependency.
