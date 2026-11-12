# Mountain Adventure — three enemies

Open this project: C:\workspace\MountainAdventure

## Open and verify

1. Stop Play Mode. If Unity has an unsaved version of an edited scene open, do not save that stale version over the updated file. Close/reopen the project or reload the scene from disk first.
2. Wait for scripts to import. Open Assets/Scenes/EnemyTrial.unity, or use Mountain Adventure → Open Three Enemy Level.
3. PLAY in MainMenu now opens EnemyTrial directly. Level1 is retained as an optional tutorial and its portal also leads to EnemyTrial. All old scenes remain in the build list.
4. Inspect the three enemies' references, masks, waypoints and parameters. Select each enemy in Scene view to see its sight cone and waypoint markers.
5. Run the functional checks in PLAYTEST.md. Compilation alone does not prove gameplay.

## Android

Use Mountain Adventure → Build Android APK. It saves scenes, sets landscape, configures ARM64/ARMv7 IL2CPP and builds Builds/MountainAdventure_ThreeEnemies.apk. Android Build Support, SDK/NDK and a valid Unity license are required. A successful build still needs installation/testing on the device.

The agent's background Unity launch failed at license-client connection in this session. No APK or runtime success is claimed until verified.

## Submission

- Repository URL with this week's real commits and DECISIONS.md. This local repository is not automatically uploaded to GitHub.
- Working APK, a real 1–2 minute playtest clip with both testers and tries, and a 2–3 minute English narrated MP4.
- Complete PLAYTEST.csv and two rows of TUNING.csv only from real observations; save and commit the tuned scene, then rebuild the submitted APK.
- Show SCRIPT_MAP.svg in the main video and explain every script yourself.

Backup before implementation: C:\workspace\MountainAdventure_BeforeThreeEnemies
