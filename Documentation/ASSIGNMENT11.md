# Assignment 11 integrated into Mountain Adventure

Project: `C:/workspace/MountainAdventure`, Unity 6000.3.24f1.

Open `Assets/Scenes/MainMenu.unity`. The original menu artwork and Quit button remain. Play opens the added level select. The campaign order is **Level1 → Level2 → Level3 → EnemyTrial → WinScene**. Level1, Level2 and Level3 remain separate authored scenes; their original objects, ground artwork, platforms, colliders and input components are retained. There is no replacement Gameplay scene. EnemyTrial is the campaign finale and is also available independently through **EnemyTrial / Practice**.

## How to play

- Level 1: collect three coins and enter the original portal.
- Level 2: collect three coins and enter the original portal.
- Level 3: defeat the original boss. After the boss, EnemyTrial starts automatically as the finale; coins are optional for score.
- Enemy Trial: the original Patroller, Shooter and Charger with their FSMs, warnings, state labels and projectiles. Collect three coins and enter the exit. The finale completes the campaign only after its exit. Practice remains independent and does not unlock or complete campaign levels.

Campaign progression is configured by LevelData.nextLevelId for levels 1–3 and GameData.trialAfterCampaign for the EnemyTrial finale: 1 → 2 → 3 → EnemyTrial → WinScene. This intentionally supersedes the legacy Level1 portal's EnemyTrial link and the legacy Level3 portal's Level2 link. The original portal objects and serialized legacy fields remain; edit `nextLevelId` in LevelData for campaign routing.

## Data assets

`Assets/Resources/GameData.asset` contains the campaign catalog, EnemyTrial entry, global player/item references, projectile radius and audio defaults.

`Assets/Resources/Assignment11/` contains:

- PlayerStats: movement, jump, HP, invincibility, blink count, weapon speed/cooldown/damage/lifetime and muzzle offset.
- ItemValues: coin value, heal amount, score and coin sound volume.
- Per-enemy EnemyStats assets: original senses, wall/ledge safety, wind-up, attack interval, damage, HP, patroller speeds/ranges, shooter projectile values and charger dash/recovery values. Serialized scene references, waypoints, sight/solid masks, state labels and visual warning styling stay on the existing components.
- Level_01, Level_02, Level_03: existing scene name, next level ID, goal, time, score and spawn bindings.
- EnemyTrial_Data: data for the retained practice scene.

A spawn row targets an existing authored object by `sceneObjectPath`, preserving references and components. Position, rotation and active state are applied by the common loader. Optional `extraPrefab` rows can add data-driven spawns. If renaming a bound object, update its path in LevelData. Camera/UI presentation constants remain presentation settings, not combat balance.

Do not run Prepare data again to rebalance. It refuses to overwrite an initialized catalog. Edit assets in the Inspector. The original values are recorded in `Assets/Editor/Assignment11OriginalBalance.json` for audit/migration.

## Save system

SaveStore writes `save.json` in `Application.persistentDataPath`. It stores format/version, best score, last selected campaign level, unlocked/completed campaign lists and master/music/SFX settings, campaignTrialPending and campaignCompleted. Completing a level, selecting a campaign level, changing settings, application pause, focus loss and quit flush the save. A fresh level run starts from its data spawn configuration; in-level position is not persisted. An unfinished finale is saved, so Continue finale / EnemyTrial resumes that stage from its beginning after reopening. Practice does not consume a pending finale. Existing version-1 saves without the new flags remain compatible.

Missing, malformed, oversized or schema-invalid JSON restores defaults. Unknown/duplicate IDs and invalid volumes are rejected. Writes flush a temporary file before replacement; unsupported File.Replace platforms use a copy fallback. I/O errors are reported without crashing. Reset asks for confirmation; Cancel preserves progress. Best score is the highest single-run score, including practice.

The Android application identifier, company and product identity are retained from this project's PlayerSettings. No credentials, publishing, Canvas upload or remote repository changes are performed.

The original **Mountain Adventure / Build Android APK** menu delegates to the Assignment 11 builder when the data catalog exists. This prevents the old Assignment 10 build helper from changing the application's package identity or using an outdated output name.

## Script map

GameData → GameSession → LevelData → the existing scene and original spawn objects.
PlayerStats → PlayerController / PlayerHealth / Shooter / BulletDamage.
EnemyStats → EnemyAgent2D + EnemyPatrol / EnemyShooter2D / EnemyCharger2D + EnemyHealth2D / BossHealth.
ItemValues → Coin / HealthPickup. CoinCounter / Portal / BossHealth → GameSession → SaveStore → save.json.
AdventureUI → GameSession for level select, settings, reset and retry. EnemyTrialSession still owns the original trial HUD, attempts and camera framing.

## Verification and build

`Assignment11Integration.Verify` runs in batch mode, restores the pre-test save byte for byte, and closes Unity. The menu entry is for controlled verification: save scenes and stop Play first. It uses synthetic movement/completion/damage and manual Update calls for deterministic FSM checks; these are integration checks, not a human phone playtest.

Set `MOUNTAIN_ADVENTURE_REVIEW` to a folder to render actual Unity camera/canvas previews. Render requests temporarily target a 1280×720 texture; previews are not phone screenshots. Set `MOUNTAIN_ADVENTURE_APK` to an output path to build Android after successful verification. `Assignment11Integration.BuildAndroid` can also build directly; it retains scene identities and builds ARM64 + ARMv7.

The source preservation audit compared original geometry/renderer/collider/component blocks, normalizing Unity's default editor fields and float formatting. Three world-space state labels had inconsistent old LocalPosition/AnchoredPosition serialization; their anchoring was corrected to retain the intended original placement.

## Student submission

Install the new APK and actually check touch controls, all three enemy types, level progression and a complete app close/reopen. Then demonstrate manual corruption of the actual save.json and safe default loading. Do not uninstall or clear app data during the restart test.

A live PlayerStats Inspector screenshot and the actual Editor save JSON are included in Documentation. Update the screenshot if changing balance, and show the real phone save for the phone demonstration. Record a 2–3 minute MP4 with your voice in English: live asset balance change, phone restart, broken save and script map. Publish this correct repository yourself and submit its URL, APK, screenshot/JSON and MP4 to Canvas. Automated checks and Unity renders do not replace phone/video evidence.
