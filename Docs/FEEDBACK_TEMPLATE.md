# Feedback log template

Paste lines like these into the Change log or the chat. **One change per line.** P1 = must fix, P2 = should, P3 = nice to have.

```
[TYPE] WHERE | NOW | WANT | P1/P2/P3 | screenshot? Y/N
```

## Types (pick one)

| Type | Use it for | What to write in NOW and WANT |
|---|---|---|
| `[DIALOGUE]` | Any text in the game | NOW = the exact current words (I search for them). WANT = the new words. |
| `[TEXT]` | Font size, colour, boldness, wrapping | NOW = "too small" or "cut off". WANT = "+20%", "bold", a colour like #FFD23F. |
| `[UI]` | Buttons, panels, layout, icons, the phone | WANT = move it, resize it, remove it, add one. |
| `[ANIM]` | Animations, shakes, popups, speed, effects | WANT = faster, slower, bigger, remove, "like Paper Mario". |
| `[BALANCE]` | Too hard or too easy | WHERE = day or enemy. WANT = easier or harder, and by how much. |
| `[BUG]` | Something broke | NOW = what you did and what happened. WANT = what should happen. |
| `[ART]` | Replace or redraw an image | WHERE = the filename from ASSET_LIST.md. WANT = "new file attached" or a description. |
| `[AUDIO]` | Music, sound effects | WANT = louder, quieter, a different vibe, remove. |

## WHERE: names I recognise

- **Title**, **Intro scene**, **Skill pick** (Astrologer Baba), **Mystery Box**, **Map** (border crossing), **Game Over**, **Credits**
- **Battle**, plus a part of it: top bar, KHATRA meter, AQI box, HP bar, damage numbers, comic words, status pills, speed button, gear, news ticker
- **Phone**, plus an app: Messages, JhatPat, Skills, Profile, Map, Settings (and: status bar, dock, chips)
- **VN scene: <name>**, for example: intro, aunty_join, metro_intro, broker_intro, ceo_intro, aunty_betrayal, babu_intro, ending
- **Day N**, for example Day 3 (metro) or Day 9 (Pinky Aunty betrayal)

## Examples

```
[DIALOGUE] VN scene: metro_intro | "Bro. Can't you see we're having a moment?" | "Bhai, dikh nahi raha? Moment chal raha hai." | P2 | N
[TEXT] Phone > Messages | bubbles too small on my phone | +20% font size | P1 | Y
[UI] Battle > gear | overlaps Bablu's name | move it down 40px | P2 | Y
[ANIM] Battle > comic words | too slow, blocks the fight | 2x faster | P3 | N
[BALANCE] Day 6 Lalaji | died 4 times in a row | a bit easier | P1 | N
[BUG] Mystery Box | tapped Equip, weapon didn't show in his hand | should show the bat | P1 | Y
[ART] cto_pappu.png | looks too old | redrawn file attached (512x640) | P2 | N
[AUDIO] Battle music | too loud vs SFX | -30% | P3 | N
```

## For art files

- Keep the **same filename** and **canvas size** as in `ASSET_LIST.md`. Export a **transparent PNG**.
- Characters face **right**; enemies are flipped in-game, so mirror any text on enemies.
- Put the files in one folder (Drive or a zip) and send the link with the matching `[ART]` lines.
