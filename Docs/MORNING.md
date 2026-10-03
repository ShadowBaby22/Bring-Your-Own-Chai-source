# Morning briefing (Sun 4 Oct)

Deadline **18:00 IST, no extensions.** Aim to upload by 16:30.

## Play it first
Dev link (everything below): https://shadowbaby22.github.io/Bring-Your-Own-Chai/dev/
Staging (what the team saw yesterday): https://shadowbaby22.github.io/Bring-Your-Own-Chai/

## What changed overnight (all on branches, nothing merged to main)
| Branch (stacked, in order) | What it does |
|---|---|
| `feature/regression-suite` (PR #4) | `bash Tools/test.sh`: compile check + logic tests in about 5 s; staging deploys refuse to go out if it fails |
| `feature/amma-blessing` | Each faint makes that wall 10% easier (floor 60%); the nursing home cures statuses. Fixes the never-shopper wall (34% to 100% finish) |
| `feature/chip-row` | Phone chips scroll sideways; 3+ statuses collapse into one chip |
| `feature/box-odds` | The Laathi is exactly 5% again (it had drifted to 4.5%) |
| `feature/kadak-chai-title` | Renamed to **KADAK Chai**; a loud, animated title screen |
| `feature/playtest-agent` | The playtester agent, scoreboard, persona reports, submission draft |
| `feature/juror-fixes` | Shorter intro, boss warning the day before, live phone HP in fights, Baba reading grace |
| `feature/kadak-opener` | **Brew it KADAK!**: one tap before every fight for an ATK boost; Days 6-11 retuned to keep 42% clean wins |
| `feature/skills-scroll` | The Skills app list scrolls instead of running over the dock (the P1 from the Feedback Log) |
| `feature/round2-fixes` | The brew result is visible and logged; Baba never gives two duds in a row or repeats an offer |

`feature/round2-fixes` contains every branch above, so **one merge ships all of it.**

## Ship it (about 10 minutes)
1. Play the dev link on your phone for 3 minutes. If anything is wrong, tell Claude before merging.
2. Merge: open a PR from `feature/round2-fixes` to `main` (or ask Claude to), merge it, and close PR #4 as included.
3. Ask Claude to "push to staging": it fast-forwards `staging`, runs the test suite, and deploys. `Builds/WebGL` already holds this exact build.

## Scores so far (Docs/playtests/SCOREBOARD.md)
| Round | Persona | Theme | Innovation | Gameplay | Aesthetics | Overall | Audience |
|---|---|---|---|---|---|---|---|
| 1 | juror (old build) | 8 | 6 | 5 | 7 | 6 | 6 |
| 2 | ncr_student (new build) | 8 | 7 | 6 | 8 | 7 | 7 |

Gameplay is still the weakest node; the Kadak opener and the round 2 fixes target it. Round 3 (juror again, on the newest build) is in the scoreboard if it finished.

## People
- **Siddharth:** play the dev link, merge, record 8-10 voice barks on your phone, watch 2-3 real people play, own the IndieConnect page (draft in `Docs/SUBMISSION.md`, needs the Discord username).
- **Jaideep:** art in this order: logo + cover, Bablu, the Day 1-4 cast, Health Insurance icon, sadhu + nursing home. Same filenames and sizes as `Art/ASSET_LIST.md`, by about 13:00.
- **Dhivesh:** play on a phone, P1/P2 lines in the Feedback Log, then 3 Gurugram event cards.
- **Amolika (if free):** screenshots + a 10-second GIF (shot list in `Docs/SUBMISSION.md`).

## Still open
- Missing `item_health_insurance.png` (the shop shows a dot).
- Fights may feel slow at 2x for impatient players (option: add a 3x speed).
- Check on a real phone that Skip >> always responds on the first tap (the test browser sometimes dropped clicks).
