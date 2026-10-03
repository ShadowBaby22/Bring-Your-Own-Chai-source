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

**Disclosure (not a criterion):** BYOG's FAQ says generative AI is "not encouraged" and asks teams to keep to the spirit of building the game in 72 hours. Ours: the concept, design, story, humour, references and every feature decision come from the team, with original art by Jaideep; the code was written with an AI assistant under the team's direction. Say it plainly on the page (wording in `Docs/SUBMISSION.md`).

## Round 1 (dry run)

| Persona | theme | innovation | gameplay | aesthetics | overall | audience | keep playing | recommend | furthest |
|---|---|---|---|---|---|---|---|---|---|
| juror | 8 | 6 | 5 | 7 | 6 | 6 | no | yes | Day 4 (beat the Broker) |

Averages and the weakest node are filled in after the round.

### Juror notes (round 1, played the PR #3 + blessing build, music off)
- **Frictions:** the hook is slow: about 10 intro text beats (60-75 seconds) before the first input, and the first real choice comes at about 1:20. Fights are watch-only and low-stakes, and event choices don't hint at what they do. Baba's 10-second timer starts during the victory confetti (it lost the Day 1 blessing). Nothing warns that tomorrow is a boss before the shop closes. The phone stayed on the Map, hiding the boss intro.
- **Delights:** the writing ("Valid for 11 months, sir. Like everything."); HP bars renamed per enemy (Followers, Love, Deposit); a shipped-mobile-game feel (phone as HUD, Mystery Box pull, VS splash, speed toggle).
- **Bug:** the phone header's HP doesn't update during a fight (it stayed at 85/144 while Bablu's bar went to 45).

## Round 2 (build: KADAK Chai title, Kadak opener, juror fixes)

| Persona | theme | innovation | gameplay | aesthetics | overall | audience | keep playing | recommend | furthest |
|---|---|---|---|---|---|---|---|---|---|
| ncr_student | 8 | 7 | 6 | 8 | 7 | 7 | yes | yes | Day 4 (beat the Broker) |

**Weakest node: gameplay (6).** The round 1 fixes landed: the intro is faster, the boss warning is clear, the phone HP is live, and the title "looks shipped".

### NCR student notes
- **The meter is the best new idea, but you can't see its payoff:** nothing showed what was brewed after the tap. Fixed after the round: the result stays up longer, a banner shows for theek too, and a line goes into Messages.
- **Baba felt rigged:** 2 of 3 blessings fizzled, and the post-boss offer repeated Day 1's exact cards. Fixed after the round: never two duds in a row (overall 80/12/8), and never the same offer twice in a row.
- **To check on a real phone:** Skip >> on the Broker's outro ignored two taps (likely the test browser's synthetic clicks).
- **Still open:** fights feel slow at 2x for an impatient player.
- **Delights:** the NCR writing ("Meter se nahi jayenge", "Leave her on read" still lets Pinky Aunty join), the Broker's Deposit bar and "temporary address proof", the faster start.

## Round 3 (build: round 2 fixes)

| Persona | theme | innovation | gameplay | aesthetics | overall | audience | keep playing | recommend | furthest |
|---|---|---|---|---|---|---|---|---|---|
| juror | 9 | 6 | 6 | 8 | 7 | 7 | no | yes | Day 4 (beat the Broker) |

**Same juror, round 1 to round 3:** theme 8 to 9, gameplay 5 to 6, aesthetics 7 to 8, overall 6 to 7, audience 6 to 7; innovation flat at 6.
**Weakest nodes: innovation and gameplay (6).** Innovation is mostly a pitch problem now ("a known auto-battler formula"): the IndieConnect page should lead with what is new (a faint rolls the day back as a zombie; the phone is the whole world; the AQI changes the rules; brew your chai before each fight).

### Juror notes (round 3)
- **Meter:** clear zones and big result words, but it didn't say what KADAK gives before the tap. Fixed after the round: the card now says "KADAK = +40% ATK for 3 rounds, theek = +15% for 2".
- **Key results hid in the log:** the Day 2 event's -13 HP and Pinky Aunty joining were only seen after scrolling. Fixed after the round: event results also show as a banner over the street.
- **Unexplained labels:** a status sat on Bablu for days without explanation; the speed button's "2x" read like the next speed. Fixed after the round: a new status gets a banner ("CANCELLED! -15% ATK. Cure: Pudin Hara"), and the button reads "SPEED 2x".
- **Delights:** the writing (Pinky Aunty's son "in Canada. And never calls."), the theme built into the systems, a clear first minute with the red "Next Day: BOSS" button.
- **Test caveat:** the browser pane was hidden, so Unity ran throttled and dropped first taps; pacing and timers were not judged.
