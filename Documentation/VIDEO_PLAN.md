# English video — 2–3 minutes

Only record after the build and real playtests. Replace brackets with actual results.

## 0:00–0:20 — phone gameplay, all three enemies

Hello, my name is Musa Nursulu. This is Mountain Adventure. For Assignment Ten, I added three enemy types to one level: a guard, a shooter and a charger. The goal is to collect three coins and reach the exit. The player can jump to dodge or fire to defeat enemies.

## 0:20–1:10 — demonstrate each FSM and warning

The guard follows waypoints. It checks distance, viewing angle and a two-dimensional raycast. The first object hit must be the player, so cover blocks its sight. When it sees me, it chases. Before a melee hit, it flashes red for zero point six seconds. I can move away during this warning. Then it recovers and returns.

The blue shooter stays in place. Its states are Scan, Aim, Shoot and Cooldown. It locks its direction during the red warning line, so I can dodge the shot.

The orange charger patrols, warns, dashes, recovers and returns. Its warning lasts zero point seven seconds. It cannot turn during the dash. Ground and wall checks stop it at unsafe edges. Attacks are blocked when the enemy or player is outside the camera view.

## 1:10–1:45 — open SCRIPT_MAP.svg and Inspector

This is my script map. The three enemies share sensing and safety helpers, but each has its own state machine. The projectile checks swept collisions. PlayerHealth handles damage and invulnerability. EnemyHealth lets my bullets defeat enemies. The tuning values are exposed in the Inspector.

## 1:45–2:30 — real clips and actual changes

I tested the level on [device] with two people who had not seen it. [Name] needed [number] tries, and [name] needed [number]. They struggled with [real observation]. I changed [parameter] from [old] to [new], and [parameter] from [old] to [new], because [reason]. [Describe the real retest result; do not claim the target unless reached.]

I used an AI agent under the revised assignment rules. I checked its changes and recorded my decisions and remaining limitations in DECISIONS.md. I can explain the state transitions, raycast, attack timing and movement checks.
