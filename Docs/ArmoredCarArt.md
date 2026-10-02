# Battle car artwork

The player uses the cream armored muscle car. The challenger uses the blue armored van from the side-view reference.

## Player

- Body: `Assets/_Project/Art/BattleArena/Cars/ArmoredCar_SideBody.png`
- Wheel: `Assets/_Project/Art/BattleArena/Cars/ArmoredCar_Wheel.png`
- Visual prefab: `Assets/_Project/Prefabs/Battle/ArmoredCarVisual.prefab`

## Challenger

- Body: `Assets/_Project/Art/BattleArena/Cars/ChallengerCar_SideBody.png`
- Wheel: `Assets/_Project/Art/BattleArena/Cars/ChallengerCar_Wheel.png`
- Visual prefab: `Assets/_Project/Prefabs/Battle/ChallengerCarVisual.prefab`

The challenger has its own dark gunmetal and rust wheel because the player's cream-rimmed wheel did not fit the blue van's darker palette. Each prefab has separate FrontWheel and RearWheel transforms behind the body, with dust anchors beneath them. The challenger prefab is mirrored on X to face left. `BattleCarPresentation` rotates the wheel transforms around local Z, emits warm road dust while driving/contacting, clears it at battle completion, and creates soft wheel-contact shadows. The losing car emits dark smoke from four engine/body points during the 10-second result hold. Impacts also add short body recoil, restrained camera shake, and faint HUD motion trails. There is no wheel motion-blur sprite yet; the current fast wheel rotation provides the spinning effect.

`BattleArena.unity` uses these visual prefabs under its existing player and challenger physics roots. The gameplay roots retain their Rigidbody2D, CarMotor2D, CrashReporter, and BoxCollider2D; only the placeholder blockout sprites were removed. Battle dust art is replaceable at `Assets/_Project/Resources/BattleDustPuff.png` and its particle material at `Assets/_Project/Resources/BattleDustPuff.mat`. The looping fight track is `Assets/_Project/Audio/Music/Locked_At_Redline_BattleLoop.wav`; the user-provided source is retained as `Locked_At_Redline.mp3`. See [BattleArenaTemplate.md](BattleArenaTemplate.md) for runtime and validation notes.
