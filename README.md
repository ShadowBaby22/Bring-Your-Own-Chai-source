# Kadak Chai: "GRAP Stage 4" (jam build)

Design doc: [GDD.pdf](GDD.pdf) / [GDD.md](GDD.md)

## Run it
1. Open the project in Unity 6000.5.9f1.
2. Menu **Kadak Chai > Build Main Scene** (once). This creates `Assets/_Game/Scenes/Main.unity` and applies the WebGL settings.
3. In the Game view, pick a **portrait** size (add 1080x1920 if it isn't listed), then press **Play**.

The whole UI is built in code by `GameController`, so the scene only holds a camera and one GameObject.

## Change content (no code needed)
All files are in `Assets/_Game/Resources/Data/`:

| File | What it holds |
|---|---|
| `days.json` | The 11-day script: zone, type (fight / event / chai / boss), card id, enemies |
| `combatants.json` | Enemy and boss stats, AI gimmick, placeholder colour, HP label ("Runway") |
| `skills.json` | Astrologer Baba's skills: effect, trigger chance, **lease in days** |
| `dialogue.csv` | Every log card. Wrap a word in `*stars*` to make it orange. Choices and effects are in the same row. |
| `scenes.csv` | Visual-novel scenes (boss intros/outros, Pinky Aunty, intro, ending): `scene,speaker,side,text` |

**Variety:** event days in `days.json` have a `pool` (e.g. `ev_dilli_`). Any `dialogue.csv` row whose id starts with it can show up, without repeats in a run. To add variety, add rows. Fight days can list `enemyGroups` (e.g. `"monkey+monkey"`). One is rolled each run, and its intro card is `fight_<first enemy id>` (plus `_win`).

Effect syntax for event choices: `hp:-15%;atk:+5;def:+6;xp:+30;aqi:-80;heal:100%;flag:aunty;skill:random`

After changing numbers, run **Kadak Chai > Simulate 500 Runs** and read the Console. It shows the first-try win % per day.
Current targets: normal fights ~95–100%, Broker ~94%, CEO ~80%, Babu ~45% (you're *meant* to "Come Tomorrow" once).

## Art and audio sources
All current art was drawn as code (SVG), so it's easy to tweak and re-export:
- `Art/src/chars.py` (12 characters) and `Art/src/bgs.py` (3 backgrounds): edit colours or props, then run `python3 Art/src/chars.py out && bash Art/src/render.sh out <pngdir> 512 640`.
- `Art/src/synth.py` generates every SFX and both music loops: `python3 Art/src/synth.py Assets/_Game/Resources/Audio`.

Replace any PNG or WAV with your own file of the same name whenever you like.

## Drop in art (no code or inspector changes)
Put PNGs into `Assets/_Game/Resources/Art/` with these exact names. Anything missing stays a coloured blob.

**Characters/** — draw everyone **facing right**; enemies get flipped automatically. Use a transparent background, about 512 px tall.
- `new_kid.png`, `pinky_aunty.png`
- `scooty_uncle.png`, `auto_bhaiya.png`, `south_delhi_girl.png`, `broker.png`
- `monkey.png`, `sector_cabbie.png`, `firecracker.png`, `guard.png`, `ceo.png`
- `babu.png`
- (stretch) `techbro.png`, `builder.png`
- Optional poses: `<id>_attack.png` and `<id>_hit.png` are swapped in during attacks and hits if present.

**Backgrounds/** — at least 1320×820. Wider than the screen so it can slide while walking.
- `dilli.png`, `noida.png`, `daftar.png`, (stretch) `gurugram.png`

**Font** — drop any `.ttf` into `Resources/Fonts/` (e.g. Baloo 2 from Google Fonts) and it's used everywhere.

## Drop in audio (optional)
Put files in `Assets/_Game/Resources/Audio/`:
- `SFX/`: `click`, `card`, `hit`, `crit`, `heal`, `ko`, `win`, `lose`, `stamp`, `levelup`, `whoosh`, `expire`, `shout`, `honk`, `blip_lo/mid/hi` (VN voices)
- `Music/`: `day`, `battle`
- `Barks/`: named after a combatant id (e.g. `babu.wav`). It plays when that enemy appears.

## Ship it
**Live playtest link: https://shadowbaby22.github.io/Bring-Your-Own-Chai/** (repo `ShadowBaby22/Bring-Your-Own-Chai` holds only the built game).
To update it after a build: `bash Builds/deploy_pages.sh`. It pushes `Builds/WebGL` and goes live in about a minute.

Step-by-step itch.io upload (for the jam submission) (secret link for teammates, mobile-friendly): **[Itch/UPLOAD.md](Itch/UPLOAD.md)**.
The web build uses a custom mobile-first page template (`Assets/WebGLTemplates/KadakChai`) and shows a "dev build <date>" stamp on the title screen.

1. **Kadak Chai > Build WebGL** writes to `Builds/WebGL/`.
2. Zip the **contents** of `Builds/WebGL` (`index.html` must be at the top level of the zip).
3. On itch.io: Kind of project = HTML, upload the zip, tick "This file will be played in the browser", and set the viewport to **540 × 960**.
4. Decompression Fallback is already on, which itch needs.

## Code map
- `Scripts/Core/`: engine-free rules (`Run`, `RunState`, `Effects`, `Aqi`, `GameData`, `RunSimulator`)
- `Scripts/Battle/`: `AutoBattle` simulates a whole fight instantly into a list of events
- `Scripts/View/`: `GameController` (flow + HUD), `BattleView` (replays the events with juice: shake, hit-stop, flashes, particles, VS card, slow-mo KO), `VNPlayer` (portrait dialogue), `StoryLog`, `Puppet`, `TextFx` (typewriter/wave/shake), `Fx` (particles), `Art`, `AudioKit`, `UiKit`
- `Scripts/Editor/`: the **Kadak Chai** menu
