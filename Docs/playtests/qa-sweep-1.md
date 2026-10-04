# QA sweep 1

**Build:** `http://localhost:8765/` ("dev build Sun 4 Oct 08:44"). Browser-pane tab `seed`, native pane size (800x600 frame; the game column is about x 232-568). Music and SFX were already OFF.
**When:** Sun 4 Oct 2026, 08:46-08:59 IST. About 68 pane actions.
**Scope:** the six checks in the brief, in order. Bug hunting only, no scoring.

## What passed

- **Title:** the Stickers button opens the sheet (9/25) and Close works. The gear opens Options (Sound FX / Music) and Close works.
- **Normal run:** Skip >> works. Meet the Astrologer leads to BLESSED (Kadak Chai) and the "Bablu learned KADAK CHAI!" box appears. FIZZLED appeared twice (Pressure Horn, N95 Mask). The free Mystery Box revealed "COMMON! Chappal" with **Equip** / **Return for ₹50** (the buttons say "Equip", not "Keep it", for a weapon). After Equip, money stayed at ₹200 and the box price went to ₹200. Start Day 1, the "Brew it KADAK!" meter, the fight (won in round 2) and Lv 2 / ₹280 afterwards all worked. No sticker pop-in on Day 1, which is correct because Auto Bhaiya was already on the sheet.
- **Phone apps:** Skills, Profile (the ID card has the photo, EXP/HP/ATK/DEF, ₹, and Stickers 9/25, which opens the sheet), Map ("Tap the map to zoom"), Settings and JhatPat all open and look right, apart from the bugs below.
- **Day 4:** Face the Boss opens "Fill the Broker's documents!" ("The clock starts on your first tap"). The result line showed in Messages: "Come back tomorrow." No boost. The fight was won in round 7.
- **Day 6:** Lalaji's form is "Fill Lalaji's appraisal form!". The result line was "Form accepted (21 taps): +40% ATK for the first 3 rounds." The CFO (Bunty, Margin 50/50) and CTO (Pappu, Battery 40/40) summon happened with a "CTO!" burst. Won in round 9. New stickers: Lalaji, Bunty (CFO), Pappu (CTO), bringing the sheet to 13/25.
- **Day 11:** if you never tap, the form times out by itself ("Come back tomorrow"), so there is no softlock. Bablu fainted in round 7. Amma's Nursing Home scene played, then Zombie Bablu (green, "Zombie 1 fight", statuses cured) and "Back to Day 11 >", which restarts Day 11 with the Babu at 120/120. The sheet now shows **Zombie Bablu** collected with a NEW tag (14/25).

## Confirmed bugs

1. **P2: JhatPat, the Health Insurance card's expiry line is clipped.**
   Steps: start a run, then open JhatPat before Day 1, or open JhatPat on the phone after Day 1.
   Saw: the description wraps to three lines ("If Bablu faints, AIIMS ... policy expires in 3 days."), which pushes the red "Expires in 3 days" line out of the card. The next card (Hockey Stick, later Cutting Chai) covers it, so only the top half of the letters shows.
   Expected: the whole expiry line visible inside the card (grow the card or shorten the copy).
   Moment: the first JhatPat screenshot, and again in the phone JhatPat after Day 1.

2. **P2: Amma's Nursing Home scene plays on top of the frozen Sarkari Daftar fight.**
   Steps: `?day=11&status=...&patience=60`, Face the Boss, don't tap, lose.
   Saw: the VN box says "Amma's Nursing Home. The ceiling fan is doing its best. So is Amma." Behind it is still the Daftar: the Babu standing, his "Patience 74/120" bar, "Round 7/15" at the top, and Bablu's faded body. The HUD still says "Sarkari Daftar".
   Expected: a nursing-home backdrop, or at least the fight cleared away (no boss, no HP bar, no round counter) while Amma's scene plays.
   Moment: the first Amma's line, right after HP 0/168.

3. **P3: Amma's scene, Bablu's portrait is wrong for his state.**
   Saw: Bablu came in with the Shaved status and his battle sprite is bald, but his VN portrait has full hair. The portrait also doesn't turn green on "Bablu feels strong. Bablu feels green." Vaidya Baba talks with a nameplate but no portrait.
   Expected: a bald portrait while Shaved, a green tint on the zombie line, and ideally a Vaidya Baba portrait.
   Moment: "What is in it?" / "Bablu feels green."

4. **P3: Messages, the top line of the oldest visible bubble is always cut off.**
   Steps: watch Messages any time new lines arrive, for example after the intro, before Day 1, or after Back to Day 11.
   Saw: the first line of the top bubble sits under the "Messages" header and status-chip row, with only the bottom half of the letters showing. Examples: "Bablu, 21, from Kanpur", "JhatPat is installed on your phone", "JhatPat Mystery Box: Chappal", "Nursing Home. Amma's bill".
   Expected: the viewport mask starts below the chip row, or the list stops on a bubble edge.
   Moment: the intro recap, pre-Day-1 Messages, Day 1 start, Day 11 restart.

5. **P3: the Skills dock badge "!" doesn't clear after you open Skills.**
   Steps: after Day 1 (Baba's pick fizzled), tap Skills, then Profile, Map, Settings.
   Saw: the red "!" on the Skills icon was still there in every later screenshot.
   Expected: the badge clears once Skills has been viewed, the way JhatPat's badge cleared after opening the shop.

6. **P3: no "New sticker" line in Messages for Zombie Bablu.**
   Steps: lose on Day 11 and finish Amma's scene.
   Saw: Messages shows "Knocked out by The Babu..." and "Amma's blessing...", then, after Back to Day 11, the Day 11 header. There is no "New sticker: Zombie Bablu (14/25)" line, although the sheet does award it (NEW tag, 14/25). Every other sticker today got a line ("New sticker: Daaru Uncle (10/25). Your sheet is in Profile > Stickers.", "New stickers: Lalaji, Bunty (CFO), Pappu (C...").
   Expected: the same Messages line for the zombie sticker. (For the pop-in itself, see M3.)

7. **P3: the NEW STICKER pop-in covers Skip >> and the enemy's name label.**
   Steps: `?day=6`, Sign the Agreement. It happens again at the end of the Lalaji fight.
   Saw: the pop-in card (top right, about x 485-530, y 80-190) sits over Skip >> and over Lalaji, and its caption "Daaru Uncle" lands right under Lalaji, so for a second the boss looks like he is named Daaru Uncle.
   Expected: put the pop-in where it doesn't cover Skip >> or a fighter, for example top left under the HUD, or above the phone.

8. **P3: sticker-sheet hints are hard or impossible to read at desktop size.**
   Steps: title, then Stickers, or Profile, then Stickers.
   Saw: the white italic hints on the light-blue sky (rows 1-3) are low contrast. On the street art (row 4: "Beat the lost cab driver" and the middle hint) the TO-LET and shop signs run through them. In an 800x600 frame none of the hints could actually be read. The footer "Screenshot this sheet to keep it!" also sits over the title's "Sign the Agreement" button text, which bleeds through the dim layer.
   Expected: a dark outline or a pill behind each hint. (Commit a7cebf0 covered phones; this is the desktop jury view.)

9. **P3: text wraps badly in a few places.**
   - The "Bablu learned KADAK CHAI!" box wraps "· lease 3 / days" onto a second line.
   - The day header "Day 6 of 11 · Gurugram Corporate Emirates · Khatra" pushes "4/8" onto its own line.

10. **P3: labels in the battle view are clipped or overlapping.**
    - Day 11: the companion label "Kush (Rival Uni)" is cut off at the left edge of the battle view, so it reads "ush (Rival Uni)".
    - Day 4 / Day 6: the companion labels "Pinky Aunty" and "Daaru Uncle" sit on top of the DHABA sign and the SURGE 3.2x billboard.
    - Day 11: the status chips "Monkey Bite / Interrogated / +5" and the "Bablu" label sit on the notice-board art, which makes them hard to read.
    - Day 11: "Round N/15" sits on top of the ceiling-fan art.

11. **P3: "Round N/15" stays on screen through the post-fight VN scenes.**
    Saw: "Round 7/15" during the Broker outro, "Round 9/15" during the Lalaji outro, and "Round 7/15" during Amma's scene.
    Expected: hide the round counter once the fight ends.

12. **P3: the "CTO!" summon burst is cut off at the right edge of the battle view.**
    Moment: Day 6, round 4, when Pappu (CTO) arrives.

13. **P3: on desktop, VN portraits slide in over the dark letterbox.**
    Saw: Lalaji's portrait was drawn at about x 575-710, outside the game column, while it slid in from the right. Bablu's portrait slid in from the left at about x 100-240. On a phone this happens off-screen, but on the desktop jury build it shows up in the side bars.
    Expected: clip the slide to the game column, or start it from just inside the column.

14. **P3: the Duct-Tape Fix skill icon looks like a magnifying glass.**
    Moment: Baba's second offer on Day 1.

15. **P3: the Mystery Box reveal text has no backing.**
    Saw: "+3 ATK. Ranged. Never misses, even in smog..." is white text over the dimmed shop list, right on top of the Cutting Chai card. "COMMON!" overlaps the shop's "Today:" hint text.
    Expected: a solid panel behind the reveal.

## Maybe test-harness artifacts (need a check on a real device)

- **M1: the boss tap-tap drops clicks and is too slow in the pane.** Day 4: I sent 22 clicks in one batch and got "Come back tomorrow" (fail). Day 6: 22 clicks registered as 18, with the readout frozen at "18 taps · 4.0s left". After 3 more clicks it said "Form accepted (21 taps)". So roughly 1 click in 5 gets lost and the pane can't fit 20 clicks into 4 s reliably. Probably the harness, not the game. A debug switch such as `?formtaps=` would make boss checks repeatable.
- **M2: Messages wouldn't scroll back.** Neither the mouse wheel nor a drag on the list moved it after Day 1, and the fight-result and reward lines weren't visible after Baba. The code (`StoryLog.cs`: cleared only at run start in `GameController.RunLoop`, capped at 40 cards, `scrollSensitivity` 30) says the history should be there, so wheel and drag events probably aren't reaching the Unity canvas in this pane. Try a touch scroll on a phone and a trackpad scroll in desktop Chrome. If it doesn't scroll there either, this becomes a P2.
- **M3: I didn't see the Zombie Bablu pop-in.** I took screenshots about 2-3 s apart after Amma's scene and never caught the pop-in, although the sticker was awarded. It may have shown during a gap. Together with bug 6 (no Messages line), check it on a slower watch.
- **M4: Pappu (CTO) vanished while his label and "Battery 19/40" bar stayed.** On Day 6 round 8 his sprite was gone but his name and HP bar were still on screen. By the next screenshot (round 9) everything was gone and the fight was won. It may be a death or knock-back caught mid-animation.
- **M5: Bablu got "Brokerage" during the Lalaji fight.** The chip appeared mid-fight on Day 6 ("Fights pay 15% less. Cure: Pudin Hara"). That is the Broker's debuff name. If Lalaji is meant to inflict it, fine; if not, a status was reused by mistake. Bablu also didn't have it at the Day 6 start.
- **M6: AQI and GRAP jump around after fainting.** On Day 11 it read AQI 301 / GRAP 2. The Amma screen showed AQI 260 / GRAP 1, then Back to Day 11 showed 332 / GRAP 2. The `?day=` starts also show low values (Day 4: 250 / GRAP 1). It's likely a re-roll caused by the debug skip.
- **M7: "SUPER EFFECTIVE!" never appeared.** On `?day=6` Bablu had no weapon (fists), so the close-vs-ranged rule probably never applied. This was untested, not seen failing.
- **M8: one stale screenshot.** After tapping Profile on Day 11 the screenshot still showed Messages, but the next tap opened the sticker sheet from Profile, so the Profile tap did land.
- **M9: fights ran slowly.** One round took about 3 s at 2x, and Day 11 took about 40 s to reach round 7. The pane was hidden during the run, so it was likely throttled.

## Notes (not bugs)

- The first Mystery Box is free but can be "Return for ₹50", so the welcome offer can be turned into ₹50 cash. Check that this is intended.
- The phone clock went 11:10 back to 09:01 when Day 1 started. I read that as the day starting at 9 AM.
