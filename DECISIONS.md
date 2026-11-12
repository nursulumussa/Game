# Assignment 10 — decisions

- 2026-11-12 — Asked Codex to implement the revised assignment, which explicitly permits agents. Kept Mountain Adventure as a 2D platformer and preserved the previous project in a separate backup.
- 2026-11-12 — The earlier guard hit immediately and moved without ground checks. Replaced it with waypoint movement, wall/ledge probes, a visible 0.6 s wind-up and a recovery state.
- 2026-11-12 — Chose three different FSMs: pursuing melee guard, stationary shooter with locked aim, and charger with a committed dash. Shared only sensing, movement safety and presentation.
- 2026-11-12 — Agent background Unity launch failed during license-client connection. Runtime compilation and serialized-reference checks are recorded separately from unverified Unity/mobile gameplay; an APK is not claimed without a successful build.
- 2026-11-12 — Real phone playtests and post-playtest tuning are still pending. Record both testers' actual tries and at least two measured old/new settings after testing; do not invent attempts, wins, video evidence or past commits.
