# GDD: **"GRAP Stage 4"** (working title)
*Game jam entry. Theme: EVERYTHING IS TEMPORARY. Solo dev. Deadline Sunday 4 Oct 2026, 18:00 IST.*

Other title ideas: *"11 Mahine Ka Agreement"*, *"Temporary Diversion"*, *"Work In Progress Since 2009"*

**Design rule: ship something. If a feature isn't needed to finish a run, it's a stretch goal.**

---

## 0. Context and constraints
- **Time:** Friday is out. Saturday is about 11 focused hours and Sunday about 7, with a hard upload by **16:30 IST**.
- **Engine:** Unity 6 (6000.5.9f1), URP 2D project **"Kadak Chai"**, exported to **WebGL** for itch.io.
- **Format:** **portrait**, 9:16 at a 1080×1920 reference resolution, embedded at 540×960 on itch. It plays with just a mouse or a tap.
- **Reference:** *Capybara Go* (iOS). We copy its structure: a day-by-day story log, auto-battles, and choices between fights.

---

## 1. High concept
You're the **New Kid from Out of Town**, and you've just signed an **11-month rent agreement** in Delhi NCR. In this game an 11-month lease lasts **11 days**. Each day brings one event: a fight, a ridiculous situation with a choice, or a chai break. Survive all 11 days and get the **one stamp** from the **Babu** at the Sarkari Daftar before the lease runs out.

**Theme hook:** in NCR, "temporary" things are permanent: barricades, diversions, a flyover under construction since forever. But everything *you* have is temporary: your powers, your buffs, the air quality, the lease itself.

**Tone:** affectionate roast of situations and archetypes (broker, RWA aunty, tech bro, babu, Maamu). **No real named politicians, religions, or communities.**

---

## 2. Core loop (one "day")
```
Day card appears in the log  →  press the big button
   ├─ FIGHT:  auto-battle plays out (~10–20 sec)  →  Victory card + EXP
   ├─ EVENT:  joke text + 2 choice buttons        →  stat/skill/item outcome
   └─ CHAI BREAK: heal + a joke
Level up?  →  "Astrologer Baba" offers 3 skills, pick 1
Press "Next Day"  →  hero walks a few steps, next card
```
- **Run:** 11 days, about **10–15 minutes**.
- **Losing** a fight shows a *"Come Tomorrow"* card and **restarts that day** at full HP. Judges must be able to see the ending.

---

## 3. Theme mechanics: "Everything is Temporary"

### 3.1 Skills on a lease (the core hook)
- Each skill you pick lasts a set number of days, shown as a chip with the days left, for example *"Pressure Horn: 3 days"*.
- When it expires, a card says *"Your Pressure Horn's lease has ended. The landlord kept the deposit."*
- So the build keeps changing, and nothing you own lasts. That one rule is the theme.

### 3.2 AQI of the day
- A dial at the top shows the AQI, which changes **once per day** (random, pushed up or down by events). Each band applies one rule to that day's fight:

| AQI | Stage | Rule for today's fight |
|---|---|---|
| 0–200 | "Rare Good Day" | Hero +20% ATK. NPCs are suspicious. |
| 201–300 | GRAP 1 | Enemies +10% ATK |
| 301–400 | GRAP 2 | Everyone has a 15% miss chance (can't see anything) |
| 401–450 | GRAP 3, "Construction Ban" | Skills trigger half as often |
| 450+ | GRAP 4, "Odd-Even" | Hero attacks only on odd rounds, enemies only on even |

- Every stage change also shows a fake news ticker line, e.g. *"BREAKING: Schools shut, offices open, logic suspended."*

### 3.3 Running gags as event cards
- **The Barricade** blocks the road in every zone, more decorated each time (garland, then a poster, then a birthday banner). You can't fight it, only choose a detour.
- **Delivery Wale Bhaiya** is "2 min away" in one card per zone. He arrives in the credits.

---

## 4. Auto-battle
- **Layout:** hero (and Pinky Aunty once she joins) on the left, 1–3 enemies on the right.
- **Rounds:** the hero acts, then each enemy acts, up to **15 rounds**. Not winning by round 15 counts as a loss.
- **Every attack** is a basic hit plus a chance that each owned skill triggers. A triggered skill shows its name in floating text and a big damage number.
- **Damage:** `ATK × skill power × random(0.9–1.1) − DEF/2`, minimum 1.
- **Speed:** a 2× speed toggle.
- **No input during fights.** All player decisions happen between fights.

### 4.1 Skills (pool of about 10, picked from Astrologer Baba)
| Skill | Effect | Lease |
|---|---|---|
| Pressure Horn | 30% chance: extra hit ×1.5 | 3 days |
| Anti-Smog Gun | 25%: hit all enemies ×0.8 | 3 days |
| Duct-Tape Fix | every 4 rounds: heal 20% | 4 days |
| "Do You Know Who My Father Is?" | 15%: ×3 hit, 20% chance to hit yourself | 2 days |
| Kadak Chai | +25% ATK for 3 rounds at fight start | 3 days |
| N95 Mask | ignore AQI penalties | 4 days |
| Jugaad Shield | first 3 hits each fight do half damage | 3 days |
| Wrong-Side Shortcut | always act first, +15% dodge | 3 days |
| Forward This Message | 20%: enemy is confused for 1 round | 3 days |
| Haggling | 20%: steal 10% of the enemy's ATK | 3 days |

### 4.2 Companion: Pinky Aunty from the RWA WhatsApp group
- She joins through an event on Day 2 (*"Beta, I will help, but first forward this to 10 people."*).
- Like the falcon in Capybara Go, she acts on her own every 3 rounds: a *Good Morning Flower Image* heals, or a *Complaint to Society Secretary* hits hard.

---

## 5. The 11 days (hand-authored, not procedural)
Zones are just a background and a group of days. The sequence is a fixed list in JSON, so it's easy to tune and the jokes land in order.

| Day | Zone | Type | Content |
|---|---|---|---|
| 1 | Dilli | Fight | Wrong-Side Scooty Uncle |
| 2 | Dilli | Event | Pinky Aunty joins |
| 3 | Dilli | Event | Barricade #1 / Maamu: *"Closed temporarily." "Since when?" "2011."* |
| 4 | Dilli | **Boss** | **The Broker**: heals every 3 rounds (*"Owner is very nice"*) and drains HP as brokerage |
| 5 | Noida | Fight | Monkey Gang (steals your shortest-lease skill for the fight) |
| 6 | Noida | Event | A baraat blocks the expressway. Dance? (+ATK, −HP) |
| 7 | Noida | Fight | Firecracker Ban Violator (pushes AQI up) + Society Guard |
| 8 | Noida | **Boss** | **The Startup CEO**: *"PIVOT!"* every 2 rounds, switching between B2C mode (high ATK) and B2B mode (high DEF). HP label is "Runway". |
| 9 | Daftar | Chai break | Heal, plus Delivery Bhaiya "2 min away" |
| 10 | Daftar | Event | The photocopy shop: choose which document to bring (decides one buff for the boss) |
| 11 | Daftar | **Final boss** | **The Babu**: *"Lunch Break"* (invulnerable for rounds 1–2), then *"Come Tomorrow"* (revives once at full HP) |

**Ending:** the stamp reads **"VALID FOR 11 MONTHS."** Your lease ends that same moment. The credits roll as a traffic challan.

**Stretch goal:** a Gurugram zone (Days 5–8 → Gurugram, with Noida after it) with a flooded road and a Builder boss whose "Possession Date" keeps moving.

---

## 6. Writing and humour
- **The log cards do the comedy:** short text, one or two orange keywords, at most 2 choice buttons.
- **Language:** English with Hinglish punchlines.
- **Budget:** about **40 cards** (11 day intros + outcomes + victory/skill-expiry/level-up flavour). They live in `dialogue.csv` and can be written on your phone.
- **Sample lines:**
  - Tech bro enemy intro: *"I'm not stuck in traffic, I'm doing async commuting."*
  - Victory over Scooty Uncle: *"He reverses into oncoming traffic. Nobody is surprised."*
  - Babu: *"Original + 3 photocopies + self-attested + attested by gazetted officer + your soul."*

---

## 7. Art plan (the biggest risk, so the job is made small)

### 7.1 What's needed (about 25 images total)
| Asset | Count | Notes |
|---|---|---|
| Characters | 10 | New Kid, Pinky Aunty, 5 enemies, 3 bosses. **One static image each.** No parts, no rigging, no animation frames. |
| Backgrounds | 3 | Dilli, Noida, Daftar. One wide strip each, which scrolls a bit while the hero walks. |
| UI | ~6 | Log card panel, big button, stat strip, day-track icons (fight / event / chai / boss), AQI dial |
| Skill icons | ~10 | Small and simple. Could be emoji-style. |
| Font | 1 | **Baloo 2** (Google Fonts, OFL): round, friendly, and has a Hindi feel |

### 7.2 Style: "sticker" chibi (like Capybara Go)
- Big head, tiny body, **flat colours, a thick dark outline, and a white sticker border**. Max 3–4 colours per character.
- Build them from **circles and rounded rectangles** in Figma, Inkscape, or Photopea. You don't need drawing skill: a broker is a circle head, a gold chain, and a phone. About 15 minutes per character.
- **Identity comes from one prop:** the Broker's phone, the CEO's hoodie and laptop, the Babu's rubber stamp, Maamu's whistle, the Scooty Uncle's helmet on the elbow.

### 7.3 All animation is code (no frames)
- **Idle:** a slow squash-and-stretch bob.
- **Attack:** lunge forward and back.
- **Hit:** flash white and shake.
- **Death:** tip over, then fade.
- **Walk to next day:** bob faster while the background scrolls.

This is exactly how Capybara Go's sprites read in the video, and it costs **zero art time**.

### 7.4 Fast sources
- **Backgrounds:** take phone photos of real NCR scenes (barricades, metro pillars, a sarkari office corridor), posterize them, and lay them behind the stickers. It looks authentic and takes minutes. Use only your own photos.
- **UI:** the free **Kenney UI packs** (CC0). Just recolour them.
- **Icons:** **game-icons.net** (CC BY; credit it on the itch page).
- **AI-generated art:** check the jam rules first. Many jams ban it or require disclosure.

### 7.5 Order of work
- **Saturday: placeholders only.** Coloured capsules with a name label. Gameplay must never wait on art.
- **Sunday morning: art block.** Swap in the stickers. If time runs out, the placeholders are flat-coloured blobs with faces, and that still fits the style.

---

## 8. Audio (stretch goal)
- **Music:** 1 loop for the day log and 1 for battle (BeepBox or royalty-free, with a dhol/shehnai flavour).
- **SFX:** mostly mouth-made: honk, stamp thunk, "Arre!".
- **Barks:** 2–3 short voice clips per boss that play when they appear. That's all the voice acting.

---

## 9. Implementation (Unity 6), already scaffolded
```
Assets/_Game/
  Resources/Data/   days.json (the 11-day script), combatants.json, skills.json, dialogue.csv
  Resources/Art/    Characters/<id>.png, Backgrounds/<zone>.png   (drop-in, auto-loaded)
  Resources/Audio/  SFX/, Music/, Barks/                          (drop-in, optional)
  Scripts/Core/     Run + RunState (day loop, leases), Effects, Aqi, GameData, RunSimulator
  Scripts/Battle/   AutoBattle (simulates a fight into events), Combatant, StatusEffect
  Scripts/View/     GameController (UI + flow), BattleView (replays events), StoryLog, Puppet, Art, AudioKit, UiKit
  Scripts/Editor/   "Kadak Chai" menu: Build Main Scene, Build WebGL, Simulate 500 Runs
```
- **Everything is data-driven:** days, enemies, skills and card text come from JSON/CSV.
- **One scene,** with the UI built in code. Art and audio are swapped by dropping files into place.
- **The rules are engine-free C#,** so the balance simulator runs 500 games in seconds.
- **Skills** have `daysLeft`. Buffs have `roundsLeft`. Those two counters are the whole "temporary" system.
- **Debug:** set `Debug Start Day` on the Game object, or add `?day=11` to a WebGL URL, to jump straight to a boss.

---

## 10. Schedule

| When | Hours | Goal |
|---|---|---|
| **Fri (tonight)** | ~1 | Write day cards on your phone. Scaffold is set up. |
| **Sat 09–12** | 3 | Auto-battle with placeholder capsules: round loop, damage popups, win/lose |
| **Sat 12–15** | 3 | Day loop: story log, big button, event choices, day track, JSON day script |
| **Sat 15–18** | 3 | Skills (pick 1 of 3, triggers, lease expiry), AQI of the day, Pinky Aunty, 3 boss gimmicks |
| **Sat 18–20** | 2 | Full run playable start to credits with placeholders. **This is the shippable build.** |
| **Sat 20–21** | 1 | WebGL build uploaded to itch (as a private draft) |
| **Sun 09–12** | 3 | Art: 10 stickers, 3 backgrounds, UI reskin |
| **Sun 12–14** | 2 | Finish all cards, balance, juice (screen shake, ticker, 2× speed) |
| **Sun 14–15:30** | 1.5 | Music/SFX, boss barks |
| **Sun 15:30–16:30** | 1 | Playtest with 2 friends, fix, **upload** |
| **16:30–18:00** | buffer | Itch page, screenshots, GIF, submit |

### Cut list (cut from the top first)
1. Gurugram zone (already a stretch goal)
2. Barks and music (keep 3–4 SFX)
3. Pinky Aunty companion
4. AQI rules (keep the dial and ticker as flavour)
5. Boss gimmicks beyond one each

**Never cut:** the 11-day lease, skills that expire, the story log jokes, the Babu finale.

---

## 11. Done criteria
- The WebGL build runs on itch in Chrome and Safari, and a full run reaches the credits in under 15 minutes with no softlock.
- At least 2 skills expire visibly during a normal run.
- Two friends can finish it without help.
- The itch page has a 3-bullet "How this fits the theme" blurb (§3).
