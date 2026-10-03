# Regression checks

Two layers. Run both before anything goes to `staging`.

## 1. Automated: `bash Tools/test.sh` (about 5 seconds)

No browser. It compiles every script against Unity's DLLs, then runs the game's own C# (`Tools/tests/Tests.cs`).
`Tools/deploy_pages.sh staging` runs it first and refuses to deploy on a failure (`SKIP_TESTS=1` overrides).

| Group | Guards |
|---|---|
| `data` | days.json, combatants, scenes, dialogue cards, statuses and items all point at things that exist; KHATRA 1 to 8; gentler Days 1-4 |
| `art` | every fighter has a sprite and a hurt sprite, bald Bablu, backgrounds, skill icons. Missing item icons are a warning |
| `shop` | free first Mystery Box then ₹200, Laathi box-only at about 5%, box odds, shelf contents, Pudin Hara, chai, gear wear, selling |
| `days` | rent, Robbed, companions join and stay at the border, Toll Tax and FASTag, skill leases, Baba's 75/15/10 odds, level-ups |
| `nurse` | faint instead of game over, insurance revive, back to yesterday's morning minus ₹100, zombie for one fight, lifesteal, one un-death |
| `battle` | same seed gives the same fight, every fight ends cleanly, Lalaji summons once, KHATRA scaling, status bleed, rewards |
| `balance` | 400 simulated runs: 100% reach the stamp, clean win 30-60%, at most 1.5 faints per run, Days 1-4 under 15% faints, the Babu at least 5% |

Run one group: `bash Tools/test.sh nurse`. Skip the compile step: `--no-compile`.
When a test fails because the design changed on purpose, update the test in the same commit and say so in the PR.

## 2. Visible: play it in the browser pane (about 5 minutes)

Open the build (staging, or the `kadak-chai-webgl` localhost preview), set a phone-sized viewport, and **turn the music off with the gear first**.

| # | URL | Do | Expect |
|---|---|---|---|
| 1 | `/` | Load | KADAK Chai title: logo pops in, sunburst spins, chai steams, Bablu and the Babu bob, the button pulses; build stamp at the bottom |
| 2 | `/` | Gear, then Music OFF, then Close | Options panel opens and closes; music stops |
| 3 | `/` | Sign the Agreement, Skip | Day 1 intro, phone Messages, "Meet the Astrologer" |
| 4 | `/` | Astrologer | Baba's 3 cards with the countdown; picking one closes it (waiting gives "No blessing") |
| 5 | `/` | Fight Day 1 | The **Brew it KADAK!** meter appears; a tap (or 6 s) closes it, the result shows and lands in Messages; then the VS card, the fight, rupees and EXP; JhatPat shows the **FREE** Mystery Box |
| 5b | `/` | Finish Day 3 | Before Day 4: the boss warning card and a red "Next Day: BOSS" button; the phone HP moves during fights |
| 6 | `/?day=3` | Fight | Metro background, couple fight, Pinky Aunty fighting beside Bablu |
| 7 | `/?day=6` | Face the Boss | Lalaji; below half his Profit, the CFO and CTO walk in |
| 8 | `/?day=9` | Fight | Pinky Aunty betrayal scene; a shave makes Bablu bald |
| 8b | `/?day=9&status=shaved,confused,robbed,bitten,lost` | Open the Skills app | The list scrolls with "swipe up for more" and never covers the dock; the Messages chips show "5 statuses" |
| 9 | `/?day=11` | Face the Boss, win | "VALID FOR 11 MONTHS" stamp, ending scene |
| 10 | `/?day=11&status=bitten,grilled,cringe,shaved,cancelled,confused,lost` | Face the Boss, lose | Amma's Nursing Home scene, green zombie Bablu at full HP, ₹100 less, "Back to Day N"; the retry starts with the zombie still green |

Close the pane when done.

## Known issues the checks have shown

- Missing `item_health_insurance.png`: the shop shows a dot for Health Insurance.
- A player who never opens JhatPat finishes only about a third of runs within 10 faints (the walls are Lalaji, Day 7, Pinky Aunty and the Babu). A faint keeps no progress and costs ₹100.
