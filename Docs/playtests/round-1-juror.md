# Playtest: round 1, persona `juror`

**Persona:** industry jury member (mobile and casual designer, 12 years at studios like Zynga and Miniclip), desktop, one of 200 entries tonight.
**Build:** `http://localhost:8765/`, title screen says "dev build Sun 4 Oct 01:29" (includes Amma's blessing after a faint and the swipeable phone chip row).
**Setup:** used the existing browser-pane tab `seed` (per run instructions), viewport 450x800, reloaded once. Music and Sound FX were both already OFF from a saved setting, so **audio was not assessed**.
**Play time:** about 7 minutes of game time, about 40 actions. **Furthest:** Day 4, beat the first boss (The Broker).
**Console:** no errors logged.

## Play log

| Time | What happened |
|---|---|
| 0:00 | Title: "GRAP STAGE 4. Bablu. 11 days. One stamp. No Bachelors. *Everything is temporary.*" Strong, clear pitch. Opened the gear: Music and SFX already OFF. Closed it and tapped **Sign the Agreement**. |
| 0:20 | Intro visual novel, about 10 beats (Kanpur hostel, Gurugram internship "Stipend: ₹8,000. Accommodation: *Figure it out*", "Delhi NCR. Population: yes. Air quality: also yes.", NO BACHELORS boards, RWA police verification, 11 days, "In NCR everything is temporary. Except the rules for bachelors."). Funny, but I was only reading until about 1:20. |
| 1:20 | A recap card on the phone, then **Meet the Astrologer**. Baba offered 3 leased blessings with a 10 s timer ("Pick fast. Blessings are temporary. Some are fake."). Took **Anti-Smog Gun** (lease 3 days). |
| 1:50 | Opened **JhatPat** ("delivered in 10 minutes*"). It lists today's enemies and says ranged weapons hit first. The free Mystery Box gave an **EPIC Hockey Stick** (+7 ATK, 4 fights), with a choice of Equip or "Return for ₹200". Equipped it. |
| 2:40 | **Day 1** fight against South Delhi Girl (her HP bar is labelled "Followers"). Won in 2 rounds, with confetti. Baba's blessing popped up straight away and the countdown ran out before my tap landed: "Astrologer Baba got bored and left... No blessing." Reached Lv 2. |
| 3:20 | **Day 2** event with Pinky Aunty. Skipped the scene. The choice was **Forward it** / **Leave her on read**, with no hint of what each does. Picked "leave on read": she "joins anyway" as a companion, -13 HP. |
| 4:00 | **Day 3** in the Delhi Metro (Blue Line, next stop Rajiv Chowk): the PDA couple Jaanu and Shona (HP labelled "Love"). The header showed KHATRA 2/8 ("Thoda risky, enemies hit harder") and AQI 263 / GRAP 1 (+10% enemy ATK). Set the speed to 1.5x. Shona heals ("Jaanu, don't die!") and I picked up a **Cringe** status. Won. Took **N95 Mask** (4 days). Lv 3. Looked at the Map ("NCR. Not to scale. Nothing is."). |
| 5:10 | **Day 4**: The Broker arrives with a VS splash. He is a **BOSS** with HP labelled "Deposit 190/190". I wanted to heal first (I was at 85/144), but JhatPat was closed: "Your Delivery Wale Bhaiya is running late. JhatPat opens after today's event." In Messages: "Anti-Smog Gun lease has ended. The landlord kept the deposit." |
| 5:45 | **Face the Boss**. He put a **Brokerage** status on me. Won in round 7 with HP 45/144. His lines on losing: "half brokerage. Final. Okay, quarter... a free temporary address proof", then "Valid for 11 months, sir. Like everything." |
| 6:40 | Post-boss blessing: took **Duct-Tape Fix** (I passed on "Do You Know Who My Father Is?"). Lv 4, ₹615. Stopped on the Next Day screen. I had reached a boss and used my time on this entry. |

## Scores

**BYOG jury criteria (1-10)**

| Criterion | Score | Evidence |
|---|---|---|
| Theme (Everything Is Temporary) | **8** | Every system is on a lease: skills expire in days, weapons show fights left, the game reports "lease has ended. The landlord kept the deposit", and the boss says "Valid for 11 months, sir. Like everything." Expiring buffs and weapon durability are a common jam take on this theme. The writing is what makes it fit this well. |
| Innovation | **6** | Under the skin it is a Capybara-Go-style auto-battler with roguelite picks and a durability shop. What is fresh: the phone works as the HUD, each enemy's HP is renamed (Followers, Love, Deposit), and blessings run on a timer. Faint-to-yesterday-as-zombie sounds new, but I never saw it. |
| Gameplay | **5** | I watched far more than I played. Day 1 ended in 2 rounds, Days 1-3 never threatened me, and the event choice gave no hint of stakes (her joining anyway made the choice feel moot). It was pleasant, but nothing in 4 days made me think hard. |
| Aesthetics (narrative, visual, audio) | **7** | The flat vector art is clean and consistent and the portraits are expressive. Juice is good: damage starbursts, confetti, a VS splash, two distinct locations (street and Metro). The writing is the standout. Minus: the phone panel fills the bottom half with log text and leaves the battlefield small. Audio not assessed. |
| Overall | **6** | Top-third polish and voice for a jam, and I reached a boss in about 6 minutes with no bugs. The first minute is all reading and the core loop is passive, so in a pile of 200 it stands out for its jokes more than its game. |

**Audience score:** 6/10. I smiled several times. **Keep playing past 10 min:** no (I have 199 more to play, and the fights never asked anything of me). **Recommend to a friend:** yes, especially to anyone who has tried to rent in NCR.

## Top 3 frictions

1. **Slow hook.** About 10 text beats (60-75 s) before the first input, and the first real decision (Baba's blessing) comes at about 1:20. A juror's first minute is all reading. Suggest cutting the intro to 3-4 beats and putting a fight or choice inside the first 30 s. The recap card already covers the rest.
2. **Passive, low-stakes core.** Fights are watch-only, and on Days 1-3 they were never in doubt. Blessing cards ask me to compare "15% / 20% / 25% chance" in 10 seconds. The event choice ("Forward it" / "Leave her on read") gives no hint of stakes, and the result felt the same either way. I wanted one meaningful lever per day: a visible risk/reward on event cards, or an in-fight tap ability.
3. **Timing and warning gaps around blessings and the boss.**
   - Baba's 10 s countdown starts while the victory confetti is still playing, and I lost my Day 1 blessing to it.
   - Nothing told me Day 4 was a boss before JhatPat closed for the day, so I fought at 85/144 HP with no chance to heal.
   - The phone stayed on the Map tab when the boss arrived, so his intro message sat hidden behind a "3" badge.

## Top 3 delights

1. **The writing.** "Population: yes. Air quality: also yes." / "The landlord kept the deposit." / "Valid for 11 months, sir. Like everything." The theme lands as jokes, not just as numbers.
2. **Characterful enemies at a glance.** HP is renamed per enemy (Followers, Love, Deposit), Shona heals her boyfriend with "Jaanu, don't die!", and statuses are named **Cringe** and **Brokerage**. I could read who each enemy is without opening a menu.
3. **It feels like a shipped mobile game.** The phone-as-HUD (bitterChal messages, a JhatPat shop "delivered in 10 minutes*", the map "Not to scale. Nothing is."), a free EPIC Mystery Box pull with an Equip vs "Return for ₹200" choice, the VS boss splash, a speed toggle and a tidy chip row for buffs and statuses.

## Bugs

- **Phone header HP is stale during battle (minor).** Steps: start the Day 4 boss fight (any fight shows it) and compare the HP bar under Bablu with "HP" in the phone header. Seen: the battlefield showed 64/144 and then 45/144 while the header stayed at "HP 85/144" for the whole fight. It updated after the fight (57/156 after the level-up).
- *Unconfirmed:* the Day 1 recap card on the phone ended in "...One flat. One stamp. 1". It may have been truncated, or may still have been typing out when I took the screenshot.
- *Unconfirmed:* in the title-screen Options dialog, my first tap on **Close** (sent right before another tap) did not register; a second tap closed it. This was probably my input timing.

## Quotes (juror voice)

- "The writing is carrying this, and it is genuinely funny. 'The landlord kept the deposit' says the theme in six words."
- "I read for a full minute before I touched anything. In a stack of 200, that's a minute you don't get."
- "It looks like a soft-launched mobile game. I just wish I were doing something during the fights instead of watching them."
