# Sunday 4 Oct: what changed (all in PR #6, live on the dev link)

Dev link: https://shadowbaby22.github.io/Bring-Your-Own-Chai/dev/
PR: https://github.com/ShadowBaby22/Bring-Your-Own-Chai-source/pull/6 (the one to merge; close #4 and #5 after)
Tests: `bash Tools/test.sh`, 74 passing, no warnings.

## Morning (07:30-08:35)
- **Sticker album:** 25 stickers collected across runs. A NEW STICKER pop-in during the run, and a full-screen sheet with white cut-outs and hints plus a "Screenshot this sheet" prompt. It opens from the title (top-left), from Profile > Stickers, and automatically after the end-of-run challan. `Tools/make_stickers.py` rebuilds the sticker art from the game art.
- **Boss tap-tap, "Fill the form!":** on boss days it replaces the brew meter. The Broker's documents, Lalaji's appraisal form and the Babu's triplicate each have their own form. 20 taps in 4 s = FORM ACCEPTED (+40% ATK), 10+ = +15%.
- **Tap counting:** both meters read taps from the Input System, so very fast taps never merge, and phone touches aren't double-counted.
- **Pokémon feel:** "SUPER EFFECTIVE!" popups for close hits on ranged enemies, a "Bablu learned PRESSURE HORN!" box, and tomorrow's enemy types in Messages.
- **Fix:** the title's street no longer spills outside the game on landscape desktops.
- **Test-only switch:** `?patience=60` lengthens Baba's timer. Testers and agents need it; players keep 10 s.

## While you were away (08:35-09:15)
1. **Profile is now Bablu's college ID card**, from Jaideep's mock. It has a tilted paper card, a paper-clipped sepia photo (hurt and bald variants), a purple signature, a LV stamp, EXP / HP / ATK / DEF with gear bonuses in green and penalties in red, ₹, Party, Student / B. Tech, and a Stickers button. Drop `Resources/Art/UI/id_card.png` in to replace the drawn card with Jaideep's final art.
2. **A QA sweep** (`Docs/playtests/qa-sweep-1.md`) found 15 confirmed bugs (2 P2, 13 P3), plus "maybe a test artifact" notes. Fixed:
   - P2: shop card text was cut off (Health Insurance lost "Expires in 3 days"). Long text now shrinks to fit, and the glosses are shorter.
   - P2: Amma's Nursing Home played over the frozen boss fight. The street, party and HUD now reset first.
   - **Mouse-wheel scrolling in Messages** never worked on desktop (it snapped back every frame). It does now.
   - Nothing draws outside the portrait game on desktop anymore (scene portraits, summon bursts).
   - Status pills over Bablu overlapped with 3+ statuses. Now two pills plus "+N".
   - "Round N/15" lingered after fights.
   - The NEW STICKER pop-in covered Skip >> and enemy names. It moved to the phone's corner.
   - The zombie sticker popped up under the nursing-home dialogue. It comes after now.
   - Scenes now show bald Bablu while he's shaved.
   - Bad wraps fixed: the day header and the "learned" box.
   - The Skills "!" badge clears once opened that day.
   - The Mystery Box reveal now hides the shop behind it.
3. **The challan said "48 hours".** BYOG is 72; it now also names the game and BYOG 2026.
4. **Sticker hints** were too small to read on a phone. Bigger now.
5. **Checked on screen:**
   - the ID card
   - the Skills scroll (the overnight P1 fix)
   - the full ending: a Day 11 win, the stamp, the challan, and the sticker sheet with NEW badges
   - the triplicate form counting every tap
   - Amma's scene on a clean street
   - mouse-wheel scrolling
6. **Docs:**
   - The playtester agent uses `?patience=60`.
   - The pitch mentions the boss form and the stickers.
   - The briefing points at PR #6.
   - The Live GDD has a 09:00 Change log entry.

## Still open (from the QA sweep, P3)
- Battle labels sit on background signs, and "Kush (Rival Uni)" is cut at the left edge.
- The "CTO!" burst is cut at the right edge.
- The Duct-Tape Fix icon looks like a magnifier (art).
- No Vaidya Baba portrait (art).
- The top message bubble sits half under the chip row while scrolled (normal scrolling).
- Design question: the free first Mystery Box can be returned for ₹50 (free cash). Keep or change?
