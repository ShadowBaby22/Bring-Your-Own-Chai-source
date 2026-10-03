# Playtest scoreboard

BYOG 2026 is judged **by a jury only** (no public voting), on the five criteria BYOG has used since at least 2021, with no published weights, so they count equally here:

| Code | Criterion | What the jury asks |
|---|---|---|
| `theme` | Relevance to Theme | How well does it fit *Everything Is Temporary*? |
| `innovation` | Innovation | Did it do something new? |
| `gameplay` | Gameplay | How engaging and fun is it? |
| `aesthetics` | Aesthetics | Narrative, visual and audio |
| `overall` | Overall Experience | The whole thing |

**The loop (the "node balancing"):**
1. The `playtester` agent (`.claude/agents/playtester.md`) plays the latest build in the browser pane as each persona.
2. Each persona gives jury scores (1-10, with evidence) and an audience score.
3. Average each criterion across personas. The criterion furthest below target is the weakest node.
4. Make the fix that moves the weakest node most for the least risk, then re-run the personas.

Target before submission: every criterion at 7 or higher, and an audience score of 7 or higher.

## Round 0: Claude's own juror estimate (before any persona played)

| Criterion | Score | Why | What would add +1 |
|---|---|---|---|
| theme | 8 | Temporariness runs through the mechanics (leased skills, breaking weapons, 11-month rent, statuses, rolling back a day) and the jokes (NCR's "temporary" diversions). | Say it in the first 30 seconds of play and on the submission page. |
| innovation | 6 | An auto-battler with a roguelite shop is a known shape (Capybara Go). The fresh parts are the setting, statuses as social satire, the phone as the whole UI, AQI rules and the nursing-home rollback. | Frame the new parts on the page; one mechanic nobody else has (for example, AQI you can push). |
| gameplay | 6 | The choices between fights matter, but you watch every fight without touching it. A run is about 15 minutes. | One input during fights (for example, a Honk button on a cooldown). |
| aesthetics | 7 | Consistent paper-sticker art, VN portraits, comic popups, the phone UI, chiptune audio, sharp writing for an Indian jury. | Juicier hits, a real sadhu portrait and nursing-home background, readable text sizes. |
| overall | 7 | Polished and full of content for a jam; text-heavy in the first minute. | A faster hook: the first fight within 30 seconds. |

**Risk outside the criteria:** BYOG's rules say generative AI is "not encouraged", though it isn't banned, and a jury may ask about it. This game was built with heavy help from Claude (the code, and the scripts that draw the SVG art). Decide as a team how to describe that on the submission page before 18:00.

## Round 1 (dry run)

| Persona | theme | innovation | gameplay | aesthetics | overall | audience | keep playing | recommend | furthest |
|---|---|---|---|---|---|---|---|---|---|
| juror | 8 | 6 | 5 | 7 | 6 | 6 | no | yes | Day 4 (beat the Broker) |
| ncr_student | | | | | | | | | |

Averages and the weakest node are filled in after the round.

### Juror notes (round 1, played the PR #3 + blessing build, music off)
- **Frictions:** the hook is slow: about 10 intro text beats (60-75 seconds) before the first input, and the first real choice comes at about 1:20. Fights are watch-only and low-stakes, and event choices don't hint at what they do. Baba's 10-second timer starts during the victory confetti (it lost the Day 1 blessing). Nothing warns that tomorrow is a boss before the shop closes. The phone stayed on the Map, hiding the boss intro.
- **Delights:** the writing ("Valid for 11 months, sir. Like everything."); HP bars renamed per enemy (Followers, Love, Deposit); a shipped-mobile-game feel (phone as HUD, Mystery Box pull, VS splash, speed toggle).
- **Bug:** the phone header's HP doesn't update during a fight (it stayed at 85/144 while Bablu's bar went to 45).
