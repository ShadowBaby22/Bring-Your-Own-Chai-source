# Bring Your Own Chai: art asset list

Every in-game image, with its size and what it is for. **To replace one: keep the same filename and the same canvas size, export a transparent PNG, and drop it into `Assets/_Game/Resources/Art/<folder>/`.** The game picks it up automatically, no code changes needed.

- PNGs here are the exact in-game files. SVGs are the editable vector sources (open them in Illustrator/Inkscape/Affinity, or import into Procreate/Photoshop as a starting layer).
- Characters share one 512x640 frame: feet at the bottom, head centred near the top. Keep that frame so damage overlays and the bald head line up.
- Style: flat colours, thick dark outline (#1d1b22), white sticker border with a soft drop shadow.

## Characters

Character sprite. Facing RIGHT; enemies are mirrored in-game (pre-mirror any text). `_hit` = hurt pose.

| File | Size (px) | Use |
|---|---|---|
| `auto_bhaiya.png` | 512x640 |  |
| `auto_bhaiya_hit.png` | 512x640 | hurt pose |
| `babu.png` | 512x640 |  |
| `babu_hit.png` | 512x640 | hurt pose |
| `broker.png` | 512x640 |  |
| `broker_hit.png` | 512x640 | hurt pose |
| `ceo.png` | 512x640 |  |
| `ceo_hit.png` | 512x640 | hurt pose |
| `cfo_bunty.png` | 512x640 |  |
| `cfo_bunty_hit.png` | 512x640 | hurt pose |
| `cto_pappu.png` | 512x640 |  |
| `cto_pappu_hit.png` | 512x640 | hurt pose |
| `daaru_uncle.png` | 512x640 |  |
| `daaru_uncle_hit.png` | 512x640 | hurt pose |
| `firecracker.png` | 512x640 |  |
| `firecracker_hit.png` | 512x640 | hurt pose |
| `guard.png` | 512x640 |  |
| `guard_hit.png` | 512x640 | hurt pose |
| `jaanu.png` | 512x640 |  |
| `jaanu_hit.png` | 512x640 | hurt pose |
| `monkey.png` | 512x640 |  |
| `monkey_hit.png` | 512x640 | hurt pose |
| `new_kid.png` | 512x640 |  |
| `new_kid_hit.png` | 512x640 | hurt pose |
| `new_kid_shaved.png` | 512x640 |  |
| `new_kid_shaved_hit.png` | 512x640 | hurt pose |
| `pinky_aunty.png` | 512x640 |  |
| `pinky_aunty_hit.png` | 512x640 | hurt pose |
| `rival_student.png` | 512x640 |  |
| `rival_student_hit.png` | 512x640 | hurt pose |
| `rwa_aunty.png` | 512x640 |  |
| `rwa_aunty_hit.png` | 512x640 | hurt pose |
| `scooty_uncle.png` | 512x640 |  |
| `scooty_uncle_hit.png` | 512x640 | hurt pose |
| `sector_cabbie.png` | 512x640 |  |
| `sector_cabbie_hit.png` | 512x640 | hurt pose |
| `sheru.png` | 512x640 |  |
| `sheru_hit.png` | 512x640 | hurt pose |
| `shona.png` | 512x640 |  |
| `shona_hit.png` | 512x640 | hurt pose |
| `south_delhi_girl.png` | 512x640 |  |
| `south_delhi_girl_hit.png` | 512x640 | hurt pose |
| `techbro.png` | 512x640 |  |
| `techbro_hit.png` | 512x640 | hurt pose |

## Backgrounds

Battle background (wider than the screen so it can slide).

| File | Size (px) | Use |
|---|---|---|
| `daftar.png` | 1320x820 |  |
| `dilli.png` | 1320x820 |  |
| `gurugram.png` | 1320x820 |  |
| `metro.png` | 1320x820 |  |
| `noida.png` | 1320x820 |  |

## Ui

Icon / UI art (see prefix below).

| File | Size (px) | Use |
|---|---|---|
| `app_jhatpat.png` | 256x256 | bitterChai phone dock app icon |
| `app_map.png` | 256x256 | bitterChai phone dock app icon |
| `app_messages.png` | 256x256 | bitterChai phone dock app icon |
| `app_profile.png` | 256x256 | bitterChai phone dock app icon |
| `app_settings.png` | 256x256 | bitterChai phone dock app icon |
| `app_skills.png` | 256x256 | bitterChai phone dock app icon |
| `burst.png` | 256x256 | Comic impact starburst (tinted in-game) |
| `chili.png` | 256x256 | (unused, old Theeka icon) |
| `cond_confused.png` | 512x640 | Status overlay drawn over Bablu (same 512x640 frame) |
| `cond_shaved.png` | 512x640 | Status overlay drawn over Bablu (same 512x640 frame) |
| `dmg1.png` | 512x640 | Damage overlay drawn over characters (512x640) |
| `dmg2.png` | 512x640 | Damage overlay drawn over characters (512x640) |
| `gear.png` | 256x256 | Options button |
| `item_badminton_racket.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_bubble_wrap.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_chappal.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_cork_ball.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_cricket_bat.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_cutting_chai.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_fastag.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_hockey_stick.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_laathi.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_pudin_hara.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `item_steel_tiffin.png` | 256x256 | JhatPat shop item icon (also thrown projectile for Chappal/Cork Ball) |
| `khatra.png` | 256x256 | KHATRA meter sign |
| `map.png` | 1000x1000 | NCR map (Map app, border crossings) |
| `mystery_box.png` | 256x256 | Mystery Box icon |
| `scooty.png` | 256x256 | JhatPat closed screen |
| `shot_babu.png` | 256x256 | Enemy ranged projectile (flies right) |
| `shot_ceo.png` | 256x256 | Enemy ranged projectile (flies right) |
| `shot_firecracker.png` | 256x256 | Enemy ranged projectile (flies right) |
| `shot_sector_cabbie.png` | 256x256 | Enemy ranged projectile (flies right) |
| `shot_south_delhi_girl.png` | 256x256 | Enemy ranged projectile (flies right) |
| `shot_techbro.png` | 256x256 | Enemy ranged projectile (flies right) |
| `skill_anti_smog_gun.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_duct_tape.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_forward_msg.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_haggling.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_jugaad_shield.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_kadak_chai.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_n95_mask.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_papa_ka_naam.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_pressure_horn.png` | 256x256 | Astrologer Baba skill card icon |
| `skill_wrong_side.png` | 256x256 | Astrologer Baba skill card icon |
| `star.png` | 256x256 | Dizzy star (orbits Bablu) |
| `zone_daftar.png` | 256x256 | Zone icon (top bar + map) |
| `zone_dilli.png` | 256x256 | Zone icon (top bar + map) |
| `zone_gurugram.png` | 256x256 | Zone icon (top bar + map) |
| `zone_noida.png` | 256x256 | Zone icon (top bar + map) |
