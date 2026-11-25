# Mountain Adventure

Existing Unity 2D game, extended through Assignments 10–12. Unity 6000.3.24f1.
Open `Assets/Scenes/MainMenu.unity` and press Play.
Campaign: Level1 → Level2 → Level3 → EnemyTrial → WinScene.
EnemyTrial also has an independent practice entry. All authored levels, mobile controls,
enemy FSMs and JSON progress from Assignment 11 remain in use.

## Assignment 12 — Sound Pass

`Assets/Resources/SoundPass/SoundBank.asset` contains all active clips, mixer groups,
snapshots, pitch range, gains and crossfade duration. `Adventure.mixer` contains:

```
Master                 exposed MasterVolume
├── Music              exposed MusicVolume
│   └── Music Tone     snapshot attenuation + Lowpass
└── SFX                exposed SFXVolume
```

Normal: Music Tone at 0 dB, lowpass cutoff 22000 Hz.
Paused: Music Tone at -12 dB, lowpass cutoff 1000 Hz.
Snapshots transition over 0.25 seconds using unscaled time. Keeping snapshot
attenuation on a child group preserves the user's saved Music slider value.
Settings displays dB using `20 * Log10(value)`; zero maps safely to -80 dB.
Master, Music and SFX are saved with the existing JSON SaveStore, including pause/quit.

`GameSession` creates `AdventureAudio` once. It survives scene changes with
DontDestroyOnLoad, uses two looping music decks and a one-second smooth crossfade.
Changing from one level to another or retrying a level keeps the level track's position.
There are 16 reusable SFX voices, with pitch variation from 0.92 to 1.08 for repeated
effects. Win/lose cues retain their composed pitch.

### Script map

- PlayerController → AdventureAudio: jump; Shooter → AdventureAudio: player shot.
- PlayerHealth / EnemyHealth2D / BossHealth → AdventureAudio: hit and death.
- Coin / HealthPickup → AdventureAudio: pickup.
- EnemyAgent2D.Enter → AdventureAudio: different Guard WindUp, Shooter Aim and
  Charger WindUp warnings immediately on state entry, before their attack states;
  Attack/Shoot/Dash entry also has an attack sound. Senses and attack fairness remain unchanged.
- AudioButton → AdventureAudio: pointer click and keyboard/controller submit.
  ButtonSound remains only as a legacy serialized callback, avoiding double clicks.
- GameSession timeout → lose; loading WinScene → win.
- AdventureUI / GameSession.Settings → AdventureAudio.Apply → exposed mixer dB parameters.
- SaveStore → saved volume values; AdventureAudio.Update → Normal/Paused snapshots.

### Audio sources and licenses

**All active Assignment 12 audio is original procedural synthesis, created for this
project with Codex assistance.** Source/generator for every row:
[`Tools/GenerateSoundPass.py`](Tools/GenerateSoundPass.py). No third-party samples,
recordings or compositions were used. Generated audio and the generator are dedicated
under [CC0-1.0](https://creativecommons.org/publicdomain/zero/1.0/).
Run `python Tools/GenerateSoundPass.py` to regenerate the deterministic WAV files.

All files below are in `Assets/Resources/SoundPass/`:

| File | Use | Source | License |
|---|---|---|---|
| Jump.wav | Player jump | Original generated rising tone | CC0-1.0 |
| Shot.wav | Player shot | Original generated tone/noise transient | CC0-1.0 |
| Hit.wav | Damage feedback | Original generated low tone/noise | CC0-1.0 |
| Pickup.wav | Coin and health pickup | Original generated bright rising tone | CC0-1.0 |
| GuardWindup.wav | Patroller attack preparation | Original generated rough rising tone | CC0-1.0 |
| ShooterWindup.wav | Shooter aiming warning | Original generated high rising tone | CC0-1.0 |
| ChargerWindup.wav | Charger dash warning | Original generated low rising tone | CC0-1.0 |
| EnemyAttack.wav | Attack / shot / dash release | Original generated short sweep | CC0-1.0 |
| Death.wav | Player, enemy and boss death | Original generated descending tone | CC0-1.0 |
| Click.wav | UI activation | Original generated short tone | CC0-1.0 |
| Win.wav | Victory | Original composed four-note ascending phrase | CC0-1.0 |
| Lose.wav | Death/timeout defeat cue | Original composed four-note descending phrase | CC0-1.0 |
| menu-music.wav | Menu loop, 90 BPM | Original composed 16-bar melody and accompaniment | CC0-1.0 |
| level-music.wav | Gameplay loop, 112 BPM | Original composed 16-bar melody, bass and percussion | CC0-1.0 |

Old Assignment 7/10 files in `Assets/Audio` are retained as unused historical assets.
Their provenance has not been established here. They are not used by this sound pass:
old scene clip fields are cleared, legacy AudioSources are stopped and routed to SFX,
and active playback uses only the documented SoundBank above.

### Verification and hand-in

Editor menu: Mountain Adventure → Assignment 12 → Prepare sound pass.
Batch verification: `-executeMethod Assignment12Verification.Run`.
It checks the mixer, all events, actual jump/shot hooks, FSM warning entry, saved dB
settings, crossfade overlap, continuous track position, pause and resume; then runs the
existing Assignment 11 campaign/save/FSM checks. Tests restore the pre-test save.
Android builds retain `com.DefaultCompany.urp_2d` and use version 1.1 / code 2.

Automated checks do not replace listening or a physical phone test. For Canvas:
repository link, APK, actual AudioMixer screenshot showing groups and Paused snapshot,
and a 2–3 minute MP4 with game sound and the student's own English narration.
Demonstrate all eight required event categories, audible warning before attack,
music crossfade, softened pause music, saved volume after complete phone restart,
and the script map. Keep real decisions in DECISIONS.md.
