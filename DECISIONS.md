# Assignment 10 — decisions

- 2026-11-12 — Asked Codex to implement the revised assignment, which explicitly permits agents. Kept Mountain Adventure as a 2D platformer and preserved the previous project in a separate backup.
- 2026-11-12 — The earlier guard hit immediately and moved without ground checks. Replaced it with waypoint movement, wall/ledge probes, a visible 0.6 s wind-up and a recovery state.
- 2026-11-12 — Chose three different FSMs: pursuing melee guard, stationary shooter with locked aim, and charger with a committed dash. Shared only sensing, movement safety and presentation.
- 2026-11-12 — Background Unity launch failed during license-client connection; separate C# compilation/reference checks passed. A later user screenshot confirmed EnemyTrial starts, but Coins overlapped the title: moved Coins left, HP right and the objective to its own center region. Full phone gameplay and APK remain unverified.
- 2026-11-12 — Real phone playtests and post-playtest tuning are still pending. Record both testers' actual tries and at least two measured old/new settings after testing; do not invent attempts, wins, video evidence or past commits.
