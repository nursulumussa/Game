# Real-device playtest — complete before submission

Status: NOT RUN. No claim is made that the 2–4 attempt target has been reached.

1. Build and install the APK on the Android device. Confirm the main menu, left/right, jump and fire work.
2. Record tester A, who has never seen this level. Press PLAY: the try counter resets to 1. Collect all three coins and reach the portal. Shooting defeats enemies; jumping dodges attacks. Do not explain enemy timing in advance.
3. Each death/fall reload increments the TRY counter. Record the attempt that reaches YOU WIN. Count any deliberate restart as a try in the written record too.
4. Return to the menu and press PLAY for tester B. Record the same evidence. The counter is per application session, not permanent telemetry.
5. Note unfair hits, unclear signals, missed controls and the first successful attempt. Keep the original recordings.
6. Change at least TWO serialized numbers in EnemyTrial after observing problems. Record actual old/new values and the reason in TUNING.csv and DECISIONS.md. Save the scene and commit the changes.
7. Rebuild the APK after tuning. Retest; a trained tester's second run is not an unseen first experience. If possible use two additional fresh testers for final first-time difficulty confirmation.
8. Produce a 1–2 min clip containing both testers, identifiable attempt counts, real failures/success and no fabricated evidence.

## Baseline settings (implemented before any playtest)

| Enemy | Wind-up | Cooldown | Other setting |
|---|---:|---:|---|
| Guard | 0.6 s | 1.5 s | Chase speed 1.6 units/s |
| Shooter | 0.6 s | 1.5 s | Projectile speed 3.2 units/s |
| Charger | 0.7 s | 1.5 s | Dash speed 4.2 units/s; duration 0.5 s |

These are baseline design choices, NOT changes after testing.

## Functional checks — do not mark PASS until observed

- All three types are present together in EnemyTrial; PLAY opens this scene.
- Guard patrols waypoints, detects inside the configured angle, chases, warns, hits once, recovers and returns.
- Shooter locks the red aim line, warns, fires a dodgeable projectile, then cools down.
- Charger warns, commits to its direction, cannot turn during the dash, stops at walls/edges and recovers.
- Cover blocks sight and both enemy/player bullets. Remove or jump over cover to compare detection.
- Every hit follows a visible warning of 0.3–0.8 s. Breaking line of sight cancels the shooter's shot.
- No enemy starts or resolves an attack when it or the player is outside the gameplay camera viewport.
- The charger damages at most once per dash. HP invulnerability prevents rapid repeated damage.
- FIRE can defeat every type; death removes its label and stops its behaviour.
- Collect all three coins and enter the portal: WinScene loads. Re-entering a portal without all coins does not win.
- Repeated deaths reload cleanly; no exceptions or missing references in Console.
- Android phone/tablet in landscape: controls are reachable and HUD does not overlap; check 16:9 and 16:10.

## Current limitations

- Enemies move horizontally on their platform; they do not jump or compute paths around obstacles.
- A pursuing guard stops at cover, then can be bypassed or defeated. It cannot teleport through it.
- Cached query buffers handle this small level. Dense scenes with many overlapping colliders require larger buffers.
- Player jump/ground detection is inherited from the original project; verify it on the target device.
