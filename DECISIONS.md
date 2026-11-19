# Assignment 10 — decisions

- 2026-11-12 — Asked Codex to implement the revised assignment, which explicitly permits agents. Kept Mountain Adventure as a 2D platformer and preserved the previous project in a separate backup.
- 2026-11-12 — The earlier guard hit immediately and moved without ground checks. Replaced it with waypoint movement, wall/ledge probes, a visible 0.6 s wind-up and a recovery state.
- 2026-11-12 — Chose three different FSMs: pursuing melee guard, stationary shooter with locked aim, and charger with a committed dash. Shared only sensing, movement safety and presentation.
- 2026-11-12 - Background Unity launch failed during license-client connection; separate C# compilation/reference checks passed. A user screenshot confirmed EnemyTrial starts but showed overlapping HUD text: moved Coins left, HP right and the objective to the center. The student later reported the Android game opening EnemyTrial; the agent has not independently tested the APK.
- 2026-11-12 - Assisted with recording plans and converted two student videos to MP4. Actual tester attempt counts and two post-playtest old/new settings have not been supplied here; add those real observations before claiming the playtest criteria are complete. Detailed assistance is recorded in AI_LOG.txt.


# Assignment 11 — decisions

- 2026-11-19 — Corrected the integration base to C:/workspace/MountainAdventure after the student identified the actual Assignment 10 project. The first implementation in Assignment7_InputSystem was an older version and is not the submission build.
- 2026-11-19 — Retained Level1, Level2, Level3 and EnemyTrial, their authored ground, platforms, references and mobile controls. LevelData binds existing objects by hierarchy path; no replacement Gameplay scene or enemy rewrite is used.
- 2026-11-19 — Moved the existing player, weapon, item and all three enemy FSM tuning values into ScriptableObjects. Original FSM Update bodies remain unchanged except for a pause guard. Restored world-label anchoring during Unity serialization.
- 2026-11-19 — Campaign progression now comes from LevelData.nextLevelId: Level1 → Level2 → Level3 → WinScene. The old Level1 portal pointed to EnemyTrial, which remains accessible separately from level select.
- 2026-11-19 — Added validated JSON saves, settings, pause/completion saving and confirmed reset. Automated checks cover original scenes, three actual FSM cycles, wall occlusion, off-screen fairness and save failure cases; real phone testing and the student video are still required.

- 2026-11-19 — At the student's request, connected the complete campaign as Level1 → Level2 → Level3 → EnemyTrial → WinScene. Kept a separate practice entry, added persistent unfinished/completed finale flags without breaking existing version-1 saves, and added transition/resume/retry/practice-isolation checks.
