# Script punch-up: head writer's before/after

A read-only review of KADAK Chai's player-facing text. Three editors each covered one source (dialogue.csv, scenes.csv, JSON and C# UI strings), and a head writer accepted, revised or rejected every proposal. Nothing in the game has been changed.

- Coverage: events: 85 lines reviewed, 36 flagged, scenes: 65 lines reviewed, 19 flagged, ui: 294 lines reviewed, 25 flagged
- Head writer: 68 accepted, 12 revised, 0 rejected

## Top 15 (highest impact first)

### #27 · `dialogue.csv` · lose_3.text (line 68) · typo · accept

- **Before** (17w): *Bablu* is defeated. The society WhatsApp group posts "Om Shanti" and moves on to the parking dispute.
- **After**: *Bablu* is defeated. The society WhatsApp group posts 'Om Shanti' and moves on to the parking dispute.
- **Why**: Real bug, confirmed against GameData.Csv.Parse: the raw inner quotes toggle quote mode and are silently dropped. Single quotes fix it.

### #0 · `dialogue.csv` · intro.text (line 7) · verbose · revise

- **Before** (57w): *Bablu*, 21, from Kanpur. B.Tech third year, four backlogs. His Gurugram internship starts in *11 days*, but every society in NCR says *No Bachelors*. The RWAs (residents' committees) want *police verification*, stamped at the *Sarkari Daftar* (the government office). One flat. One stamp. 11 days. If he faints, Amma's Nursing Home patches him up, one day back.
- **After**: *Bablu* has *11 days* to get *police verification* stamped at the *Sarkari Daftar* (government office). Without it, every society says *No Bachelors*. If he faints, Amma's Nursing Home patches him up, one day back.
- **Why**: Right to cut the retold VN, but 'No stamp, no flat' muddles cause and effect. 'Without it' reads clearly, and the Daftar gloss stays because this card is the line players can scroll back to. 57 to 33 words.

### #64 · `GameController.cs` · GameController.cs:344 (skill-expired banner tail) · joke-doesnt-land · accept

- **Before** (4w): " *expired*. Everything is temporary."
- **After**: " *expired*."
- **Why**: Fires on most day changes, right next to an expire_ card that already makes the joke. Removing it protects the ending's closer.

### #54 · `scenes.csv` · babu_intro, line 74 · joke-doesnt-land · accept

- **Before** (17w): Original, three photocopies, self-attested, attested by gazetted officer... and your *soul*. Come. Let us begin the procedure.
- **After**: Let us begin the procedure. Original, three photocopies, self-attested, attested by gazetted officer... and your *soul*.
- **Why**: 'Soul' becomes the last word before the final boss. Biggest single punchline fix in the scenes.

### #4 · `dialogue.csv` · d4_win.text (line 33) · verbose · accept

- **Before** (23w): The Broker drops his phone. He was on a call with another tenant the whole time. He hands you a *temporary address proof*.
- **After**: The Broker drops his phone. He was on a call with another tenant *the whole time*.
- **Why**: Confirmed in GameController: the win card is logged before broker_outro plays, so the original gave away the outro's address-proof punch.

### #6 · `dialogue.csv` · d8_win.text (line 35) · inconsistent-voice · accept

- **Before** (18w): Lalaji shuts the bahi-khata. ""Profit is temporary, beta. Family is forever."" Bunty writes you down as a loss.
- **After**: Lalaji shuts the bahi-khata. ""*Profit* is temporary, beta. Family is forever.""
- **Why**: Cutting the Bunty line removes the clash with ceo_outro's 'Bunty, write: intern passed'. The new *Profit* highlight is a nod to his HP bar label.

### #46 · `scenes.csv` · nursing_home, line 45 · joke-doesnt-land · accept

- **Before** (13w): Ashwagandha. Triphala. One secret ingredient. And a little bit of the Yamuna. *Jadi-booti.*
- **After**: *Jadi-booti.* Ashwagandha, triphala, and a little bit of the Yamuna.
- **Why**: Yamuna becomes the last word. This scene replays on every faint, so the fix pays off repeatedly.

### #49 · `scenes.csv` · broker_intro, line 50 · verbose · accept

- **Before** (18w): No problem, sir! For bachelors, rent is double and brokerage is triple. Walking distance for a fit person.
- **After**: No problem, sir! Walking distance. For a fit person.
- **Why**: Removes the 'brokerage is triple' contradiction with the very next line's 'One month brokerage'. 'Fit person' is the real comeback.

### #50 · `scenes.csv` · broker_intro, line 53 · joke-doesnt-land · accept

- **Before** (14w): Sir, the owner is *very nice*. Very nice. Let's settle this the Delhi way.
- **After**: Sir, the owner is *very nice*. Very nice. I am not.
- **Why**: His catchphrase turns into the threat that starts the boss fight. A generic 'Delhi way' becomes a sharp three-word punch.

### #2 · `dialogue.csv` · d11.text (line 37) · joke-doesnt-land · accept

- **Before** (22w): The *Babu*, Keeper of the Stamp, looks up. ""Original + 3 photocopies + self-attested + attested by gazetted officer + your soul.""
- **After**: The *Babu*, Keeper of the Stamp, looks up. Then at the clock. It is almost *lunch*.
- **Why**: The new beat is a setup that pays off a few seconds later in the Babu's 'Lunch Break! Come back in 2 rounds.' opener. The soul list now lands only in the VN.

### #66 · `GameController.cs` · GameController.cs:432 (gear-broke banner) · joke-doesnt-land · accept

- **Before** (5w): $"*{it.name}* broke. Everything is temporary."
- **After**: $"*{it.name}* broke!"
- **Why**: The item's breakText card is the joke. The tagline stacked a staler second joke on top of it.

### #28 · `dialogue.csv` · expire_2.text (line 75) · joke-doesnt-land · accept

- **Before** (7w): *{skill}* was only temporary. Like everything here.
- **After**: *{skill}* was only temporary. Renew at *Window 7*. Closed since 2009.
- **Why**: Plants Window 7 before day 11, so the Babu's line becomes a callback. It also stops repeating the expiry banner's tagline. {skill} is kept.

### #22 · `dialogue.csv` · rent_day.text (line 93) · joke-doesnt-land · accept

- **Before** (13w): *Rent day.* The landlord takes *₹{rs}*. He does not care about the baraat.
- **After**: *Rent day.* The landlord takes *₹{rs}*. The agreement is temporary. The rent is *permanent*.
- **Why**: Confirmed: rent is logged before day 8's random event, so the baraat reference usually pointed at nothing. This is the core theme gag, and {rs} is kept.

### #65 · `GameController.cs` · GameController.cs:413 (status-inflicted log line) · typo · revise

- **Before** (12w): $"*{GameData.HeroName}* is now *{Conditions.Name(c)}*: {Conditions.All[c].desc} It lasts until you drink *Pudin Hara* (JhatPat)."
- **After**: $"*{GameData.HeroName}*'s new status: *{Conditions.Name(c)}*. {Conditions.All[c].desc} Cure: *Pudin Hara* (JhatPat)."
- **Why**: The frame fixes the grammar for all 12 status names. 'Cure: Pudin Hara' matches the banner at :417 and avoids the clunky 'Lasts until Pudin Hara'.

### #30 · `dialogue.csv` · delivery_end.text (line 104) · inconsistent-voice · accept

- **Before** (14w): *Delivery Wale Bhaiya* finally arrives. ""Sir, your order."" You placed it 11 months ago.
- **After**: *Delivery Wale Bhaiya* finally arrives with your Day 1 order. ""Sorry sir, *2 minute* late.""
- **Why**: Follows straight on from the ending VN's 'Sir, 2 minute.', closing the running gag on its own catchphrase and fixing the 11-months timeline.

## Script-wide notes

- Where the text lives. VN dialogue (one row per tap): Assets/_Game/Resources/Data/scenes.csv. Phone Messages cards, choice buttons, result lines, fight intros and wins, news ticker, border/rent/join/leave cards and the challan credits: Assets/_Game/Resources/Data/dialogue.csv. Item desc/breakText: Resources/Data/items.json. Skill desc/name/shortName: Resources/Data/skills.json. Status names and descs: Assets/_Game/Scripts/Core/Conditions.cs. Hard-coded log, banner and title strings: Scripts/View/GameController.cs. Battle shouts and pet popups: Scripts/Battle/AutoBattle.cs. Phone UI strings: Scripts/View/PhoneView.cs. Mystery Box log: Scripts/View/ShopView.cs. Hit words: Scripts/View/BattleView.cs:680. In combatants.json, most 'flavor' and many 'quip' fields are never shown, so don't polish them.
- Biggest structural cause of 'too verbose': each VN scene is followed by a Messages card that retells it (GameController.PlayDay plays the scene, then logs the card). Rule going forward: the scene carries the joke, and the card adds one new beat or just names the stakes or choice. After this pass, the remaining overlap is the d2 card repeating Pinky Aunty's forward request. That's acceptable because it frames the choice, but you could trim it to: *Pinky Aunty* corners you at the gate. ""Forward this to 10 people.""
- Ship these as sets. Aunty scene: 39, 41 and 42 together. Broker objection and answer: 48 with 49. Gloss placement: 0, 36 and 38 together (the Daftar gloss stays at the first VN mention and in the intro card; the backlog gloss goes, and the ID card still says '4 backlogs'). 'Everything is temporary' tagline: 25, 64, 66 and 71 together, which leaves it in text only at the ending's 'Everything is temporary. Except Delhi.' 'No refunds': 29 and 68 together, so the fizzle keeps that punch.
- Rule of thumb that fixed most lines: the last word on screen before the tap is the joke. Watch for trailing tags ('Or was it?', 'Nobody is surprised.', 'Rules are rules', 'Come. Let us begin the procedure') and for result lines repeating the notes the game already prints ('+Max HP', '+₹100', 'Pinky Aunty joins you!').
- Running gags after this pass. Window 7 is now planted early (expire_2). The '2 minute' delivery gag closes on its own catchphrase (delivery_end). The DND 'out of respect' joke is now an explicit callback. '47' is becoming a running number (RWA vote 47 to 1, then the 47-paragraph forward), so keep both. 'Posts it / Xk likes' now appears only in the Tech Bro win; South Delhi Girl gets 'cropped out'.
- Needs a code change, not new text. A KADAK brew is announced 3-4 times within a second: meter result, banner, log card and the fight opener shout (GameController.cs:568-573, AutoBattle.cs:181-182). Drop one of them. Separately, the 'Kadak Chai' skill shouts its name right after the KADAK opener. Renaming only its name/shortName in skills.json (e.g. 'Adrak Chai' / 'Adrak') ends the triple-Kadak, and the id stays the same.
- Dead content, so don't spend polish time unless you wire it in. No day uses the ev_dilli_ pool, so all six ev_dilli_* rows never appear, including the excellent Maamu '2011' joke (worth reviving). win_1 to win_4 never fire because every fight has its own *_win row. gameover_1 to gameover_4 are used only by GameController.GameOver(), which nothing calls. Fix the stale comments: '# Noida event pool (day 6)' should say day 8, '# Dilli event pool (day 3)' is wrong, and ev_daftar_chai sits under 'Fixed day cards' but is really one of day 10's random events.
- Small consistency nits: news_4 says 'Rs 900' where everything else uses '₹900'. The time is '6 PM' in fight_jaanu/metro_intro but '6pm' in ev_dilli_rajiv_chowk. fight_rwa_aunty_win's 'Your head is cold' assumes the 50% shave landed, which is almost always but not guaranteed.
- Possible fact-check, worth verifying before relying on it: I believe the Sirhaul toll plaza on the Delhi-Gurugram expressway was shut down around 2014. Kherki Daula is the toll Gurugram drivers complain about, and it has reportedly been 'about to be shifted' for years, which would make a ready-made 'temporary is permanent' joke for border_dilli_gurugram.
- Already great, don't touch: d11_win ('Bablu is officially a person. For 11 months.'); astro_intro ('Your stars are aligned, beta. Temporarily.'); fight_auto_bhaiya_win ('broken since 2004'); fight_techbro_win ('endorsed for Violence'); fight_firecracker_win ('You were trying to leave.'); ev_daftar_forms (both results); ev_daftar_token.result_a ('Now serving: 13.'); ev_noida_sector18.result_a ('In Sector 19.'); ev_noida_sectors.result_b ('his son's JEE rank. Twice.'); expire_1 ('The landlord kept the deposit.'); leave_sheru ('it is the smog'); leave_daaru_uncle ('friends with the boom barrier'); lose_1 ('Not even to AIIMS.'); news_6 ('Flyover completed on schedule. Experts investigate.'); hockey_stick and steel_tiffin breakText. In scenes.csv: aunty_betrayal '47 to 1. The 1 was you.', nursing_home 'The ceiling fan is doing its best. So is Amma.', daaru_uncle_join 'That is the business model.', ceo_intro 'We are a startup. Since 1972.', babu_intro 'Window 7 is closed since 2009.', broker_outro 'Valid for 11 months, sir. Like everything.', and the whole ending scene ('I am 21.', 'Everything is temporary. Except Delhi.'). Also the e-challan credits.

## Every proposal, by file

## dialogue.csv

### #1 · `dialogue.csv` · jhatpat_intro.text (line 92) · buried-mechanics · accept

- **Before** (38w): *JhatPat* is installed on your phone: weapons, chai and a *Mystery Box*, delivered in 10 minutes (T&C apply). Mummy sent *₹200* pocket money. *Welcome offer: your first Mystery Box is FREE.* Tap the JhatPat app after any day.
- **After**: *JhatPat* is on your phone: open it after any day for weapons, chai and a *Mystery Box*, delivered in 10 minutes (T&C apply). Mummy sent *₹200*. *First Mystery Box FREE.*
- **Why**: Puts the instruction first and ends on FREE. Every fact stays (PhoneView confirms the shop opens after each day's event).

### #3 · `dialogue.csv` · d4.text (line 32) · joke-doesnt-land · accept

- **Before** (17w): The *Broker* appears. ""Sir, small matter only. One month brokerage, one month deposit, one month *documentation charges*.""
- **After**: The *Broker* appears. ""Sir, small matter only. Just *four months* in advance.""
- **Why**: The understatement is the joke, and it no longer recites the month list the VN gives in full.

### #5 · `dialogue.csv` · d8.text (line 34) · verbose · accept

- **Before** (22w): *Lalaji*, CEO of *Lalaji & Sons Pvt. Ltd.* (a startup since 1972), blocks your path with his bahi-khata. ""Intern! Culture-fit round. Now.""
- **After**: *Lalaji* blocks your path with his bahi-khata. ""Intern! *Culture-fit round*. Now.""
- **Why**: ceo_intro has just used 'startup. Since 1972' as its punch. Hearing it again in a mid-sentence parenthetical adds nothing.

### #7 · `dialogue.csv` · fight_rwa_aunty.text (line 27) · explains-joke · accept

- **Before** (26w): *The Noida society gate.* The RWA has called an emergency meeting about *you*. Chairing it: *Pinky Aunty*. Her WiFi works now. She no longer needs you.
- **After**: *The Noida society gate.* The RWA has called an emergency meeting about *you*. Chairing it: *Pinky Aunty*. Her WiFi works now.
- **Why**: 'Her WiFi works now' is the dry punch. The sentence after it explained the betrayal the VN had just shown.

### #8 · `dialogue.csv` · fight_rwa_aunty_win.text (line 28) · verbose · accept

- **Before** (20w): Pinky Aunty retreats to draft a 47-paragraph WhatsApp forward about this. The guard lets you through. Your head is cold.
- **After**: Pinky Aunty retreats to draft a 47-paragraph WhatsApp forward about you. Your head is cold.
- **Why**: Drops the filler guard beat, which also duplicated fight_firecracker_win's guard. Caveat: 'head is cold' assumes the 50% shave landed.

### #9 · `dialogue.csv` · fight_scooty_uncle_win.text (line 12) · joke-doesnt-land · accept

- **Before** (9w): He reverses into oncoming traffic, unbothered. Nobody is surprised.
- **After**: He reverses into oncoming traffic. Oncoming traffic apologises.
- **Why**: Real wrong-side Delhi dynamic, with the punch last. Repeating 'oncoming traffic' is a deliberate echo.

### #10 · `dialogue.csv` · fight_south_delhi_girl_win.text (line 16) · joke-doesnt-land · accept

- **Before** (16w): She posts your fight with the caption ""humble beginnings"". It gets 2.3k likes. You get none.
- **After**: She posts your fight, captioned ""humble beginnings"". 2.3k likes. You are cropped out.
- **Why**: 'Cropped out' is a specific Instagram burn and breaks the 'Xk likes, you get...' shape the Tech Bro win uses better.

### #11 · `dialogue.csv` · fight_monkey_win.text (line 18) · verbose · accept

- **Before** (17w): The monkeys retreat to a balcony with your sunglasses. They will be back. They always come back.
- **After**: The monkeys retreat to a balcony with your sunglasses. Ransom: one Frooti.
- **Why**: The Frooti ransom is a known North India monkey detail. It replaces two sentences that said the same thing.

### #12 · `dialogue.csv` · fight_sector_cabbie_win.text (line 20) · joke-doesnt-land · accept

- **Before** (13w): You reach your destination. It was *Sector 62* all along. Or was it?
- **After**: You reach your destination. It was *Sector 62* all along. The driver rates you 1 star.
- **Why**: Pays off the intro's 'Somehow it's your fault' where 'Or was it?' was an empty tag.

### #13 · `dialogue.csv` · fight_firecracker.text (line 21) · unclear-hinglish · accept

- **Before** (19w): A *Firecracker Ban Violator* is launching rockets ""for the culture"". The *Society Guard* wants to know: ""Kahan jaana hai?""
- **After**: A *Firecracker Ban Violator* is launching rockets ""for the culture"". The *Society Guard* ignores him and stops *you*: ""Kahan jaana hai?""
- **Why**: Context now makes the Hindi readable and sets up the win card's 'You were trying to leave'. Questioning the wrong person is a joke in its own right.

### #14 · `dialogue.csv` · fight_techbro.text (line 23) · verbose · accept

- **Before** (25w): Welcome to the *Gurugram Corporate Emirates*. A *LinkedIn Tech Bro* blocks the Cyber Hub entrance. ""Let's take this offline. Also, are you open to work?""
- **After**: Welcome to the *Gurugram Corporate Emirates*. A *LinkedIn Tech Bro* blocks the Cyber Hub entrance. ""Bro, let's take this *offline*.""
- **Why**: One LinkedIn joke instead of two. 'Offline' doubles as the challenge to fight.

### #15 · `dialogue.csv` · fight_jaanu.text (line 25) · joke-doesnt-land · accept

- **Before** (24w): *Rajiv Chowk*, 6 PM. The doors open and you are *transferred* inside. A *PDA couple* has claimed the only door. Your stop is next.
- **After**: *Rajiv Chowk*, 6 PM. You don't board the train. You are *transferred* onto it. A *PDA couple* blocks the door. Your stop is next.
- **Why**: Brings the best 'transferred' phrasing in from the dead ev_dilli_rajiv_chowk row, with the punch word last, and fixes the 'only door' error.

### #16 · `dialogue.csv` · fight_jaanu_win.text (line 26) · verbose · accept

- **Before** (20w): The couple breaks up mid-fight, gets back together at Mandi House, and posts about it. You make your stop. Barely.
- **After**: The couple breaks up mid-fight and gets back together by Mandi House. You make your stop. Barely.
- **Why**: Cuts the third 'posts about it' social-media beat across the fight wins.

### #17 · `dialogue.csv` · d2.result_b (line 31) · verbose · accept

- **Before** (16w): She joins anyway, and tells the whole society you are ""very rude"". *Pinky Aunty* joins you!
- **After**: *Pinky Aunty* joins you anyway, then tells the whole society you are ""very rude"".
- **Why**: Says she joins once, ends on the punch, and lets the notes line print 'Pinky Aunty joined!'.

### #18 · `dialogue.csv` · ev_daftar_token.result_b (line 58) · typo · accept

- **Before** (19w): ₹100 for chai-paani. A peon takes you to the front for ""chai-paani"". You feel powerful, and a little guilty.
- **After**: A peon walks you to the front for ₹100 ""chai-paani"". You feel powerful, and a little guilty.
- **Why**: Fixes the doubled 'chai-paani'. Keeping ₹100 inline is right because it tells players what 'chai-paani' means (a bribe).

### #19 · `dialogue.csv` · ev_noida_dnd.text (line 50) · joke-doesnt-land · revise

- **Before** (22w): The *DND Flyway*. It's been toll-free for years, but every car still slows down at the old toll booth. Out of respect.
- **After**: The *DND Flyway* again. Toll-free for years. Every car still slows down at the empty booth. Out of respect.
- **Why**: 'Again' makes it a deliberate callback, and this version keeps the 'toll-free' fact. Players who used a FASTag never read border_gurugram_noida, so they need it. The editor's draft also had two 'still's.

### #20 · `dialogue.csv` · ev_noida_extension.result_a (line 53) · verbose · revise

- **Before** (15w): ₹150 token amount. Possession date: *2031*. But the brochure is very glossy. You feel invested.
- **After**: ₹150 token amount. Possession date: *2031*. You feel invested.
- **Why**: Cutting the brochure beat is right, but 'very' softened the pun. The original 'You feel invested' was already the punch.

### #21 · `dialogue.csv` · ev_noida_filmcity.choice_b (line 54) · verbose · accept

- **Before** (5w): Ask for a speaking part
- **After**: Ask for a line
- **Why**: Within the button budget, and it sets up result_b's 'Your line:'.

### #23 · `dialogue.csv` · border_dilli_gurugram.text (line 94) · joke-doesnt-land · accept

- **Before** (24w): *Sirhaul toll plaza.* Welcome to Gurugram. The toll is 30 seconds long and 40 minutes deep. *Toll Tax: all stats -15% for 2 days.*
- **After**: *Sirhaul toll plaza.* Welcome to Gurugram. Paying takes 30 seconds. The queue takes *40 minutes*. *Toll Tax: all stats -15% for 2 days.*
- **Why**: Readable at a glance. The mechanic text is unchanged. See global notes for a possible Sirhaul fact-check.

### #24 · `dialogue.csv` · border_fastag.text (line 97) · verbose · revise

- **Before** (22w): Your *FASTag* beeps. The boom barrier opens. For one moment, you understand what it feels like to be rich. *No Toll Tax.*
- **After**: Your *FASTag* beeps on the first try. The boom barrier opens. For one moment, you are *rich*. *No Toll Tax.*
- **Why**: Good idea ('first try' is the real FASTag detail). The only fix is the stray double space before *No Toll Tax.*

### #25 · `dialogue.csv` · revive.text (line 69) · explains-joke · accept

- **Before** (19w): *Health Insurance* claimed. *Bablu* wakes up in AIIMS at 50% HP. The policy has now expired. Everything is temporary.
- **After**: *Health Insurance* claimed. *Bablu* wakes up in AIIMS at 50% HP. The policy has now *expired*.
- **Why**: 'Expired' is the punch. Together with 64, 66 and 71 this saves 'Everything is temporary' for the ending.

### #26 · `dialogue.csv` · lose_2.text (line 67) · joke-doesnt-land · accept

- **Before** (11w): *Bablu* goes down. Somewhere, Mummy's phone rings three times and stops.
- **After**: *Bablu* goes down. His phone lights up: *3 missed calls from Mummy*.
- **Why**: A specific, universal Indian dread, where the original read as a vague mood line.

### #29 · `dialogue.csv` · baba_2.text (line 78) · joke-doesnt-land · revise

- **Before** (9w): *Astrologer Baba*: ""Mercury is in retrograde, beta. Pick quickly.""
- **After**: *Astrologer Baba*: ""Mercury is in retrograde, beta. Choose anyway.""
- **Why**: Right to drop 'Pick quickly'. But 'Choose wisely' is generic, and 'No refunds' is the fizzle line's punch (GameController:776), so using it here would steal that. 'Choose anyway' is the con-man beat and still hints at risk.

### #31 · `dialogue.csv` · news_3.text (line 82) · joke-doesnt-land · accept

- **Before** (16w): BREAKING: Delhi Metro adds announcement asking passengers not to sit on the floor, in 12 languages.
- **After**: BREAKING: Delhi Metro asks passengers not to sit on the floor. Floor fully booked.
- **Why**: Shorter for the ticker. Ending on 'fully booked' gives a real punch, where 'in 12 languages' was flat.

### #32 · `dialogue.csv` · news_7.text (line 86) · joke-doesnt-land · accept

- **Before** (12w): BREAKING: Auto driver agrees to go by meter. Meter files police complaint.
- **After**: BREAKING: Auto driver agrees to go by meter. Meter refuses.
- **Why**: Flips 'Meter se nahi jayenge' and echoes the 'meter broken since 2004' win line. Tighter, and the logic holds.

### #33 · `dialogue.csv` · news_8.text (line 87) · joke-doesnt-land · accept

- **Before** (11w): BREAKING: Rare Good Day spotted. Residents suspicious, refuse to go outside.
- **After**: BREAKING: Delhi AQI hits 'Good'. Residents suspicious, refuse to go outside.
- **Why**: Ties the line to the AQI/GRAP system and the smog gag. Single quotes keep the CSV safe.

### #34 · `dialogue.csv` · ev_dilli_maamu.text (line 41) [currently unused row] · unclear-hinglish · accept

- **Before** (17w): A *barricade* blocks the road. *Maamu* stands next to it. ""Road closed temporarily, sir."" ""Since when?"" ""2011.""
- **After**: A *barricade* blocks the road. *Maamu*, the traffic cop, stands beside it. ""Road closed temporarily, sir."" ""Since when?"" ""2011.""
- **Why**: Right fix, but this row is dead content (no day uses ev_dilli_). Apply it only if the Dilli pool is wired back in.

### #35 · `dialogue.csv` · ev_dilli_gk_party.result_a (line 45) [currently unused row] · verbose · accept

- **Before** (21w): Someone hires you as a consultant on the spot. +₹100. Everyone nods. Nobody asks a follow-up question. You blend in perfectly.
- **After**: Everyone nods. Nobody asks a follow-up. Someone hires you as a consultant.
- **Why**: Under budget, with the punch last. Also dead content, so it's low priority.

## scenes.csv

### #36 · `scenes.csv` · intro, line 5 · explains-joke · accept

- **Before** (18w): Kanpur Shatabdi. Six hours late. Meet *Bablu*: 21, B.Tech third year, four backlogs (exams to retake), one suitcase.
- **After**: Kanpur Shatabdi. Six hours late. Meet *Bablu*: 21, B.Tech third year, four backlogs, one suitcase.
- **Why**: The gloss explains the backlog joke and breaks the list before 'one suitcase'. The phone ID card still says '4 backlogs'.

### #37 · `scenes.csv` · intro, line 7 · joke-doesnt-land · accept

- **Before** (10w): Agla station: *New Delhi*. Population: yes. Air quality: also yes.
- **After**: Agla station: *New Delhi*. Population: yes. Air: visible.
- **Why**: One concrete smog word, placed last. 'Air quality: also yes' read ambiguously.

### #38 · `scenes.csv` · intro, line 9 · verbose · revise

- **Before** (20w): No bachelor rents without *police verification*, stamped at the *Sarkari Daftar* (the government office). The internship starts in *11 days*.
- **After**: The only way in: *police verification*, stamped at the *Sarkari Daftar* (government office). Internship starts in *11 days*.
- **Why**: 'Bachelors need' read like a general rule. 'The only way in' answers the previous line's 'every society gate says NO BACHELORS' directly. All facts and the first-mention gloss stay. 17 words.

### #39 · `scenes.csv` · aunty_join, line 13 · verbose · accept

- **Before** (14w): Aunty, I am just an engineering student. I need a room for my internship—
- **After**: Aunty, I'm just an engineering student—
- **Why**: Cutting off at 'engineering' hands Aunty her trigger word. It works as a set with 41 and 42.

### #40 · `scenes.csv` · aunty_join, line 14 · joke-doesnt-land · accept

- **Before** (20w): *Engineer?!* Beta, my WiFi is not working since Diwali. Also the TV says ""No Signal"". Also my phone is full.
- **After**: *Engineer?!* Beta, my WiFi is not working since Diwali. TV says 'No Signal'. Phone says 'Storage Full'.
- **Why**: Three parallel error screens, ending on the specific 'Storage Full' (her forwards). Single quotes are CSV-safe.

### #41 · `scenes.csv` · aunty_join, line 16 · verbose · accept

- **Before** (19w): WAH. Genius. Okay, I will help you. You are like my son. Who is in Canada. And never calls.
- **After**: WAH. Genius! You are like my son. Who is in Canada. And never calls.
- **Why**: The Canada punch gets the line to itself. The 'help' beat moves to line 17.

### #42 · `scenes.csv` · aunty_join, line 17 · verbose · revise

- **Before** (17w): But first, forward this to 10 people. Sharma ji didn't forward, and now his flat has *seepage*.
- **After**: I will help you. But first, forward this to 10 people. Sharma ji didn't. Now his flat has *seepage*.
- **Why**: The idea is right, but the clipped 'I'll help. First,' isn't Pinky Aunty's voice. Her 'I will help you. But first' rhythm also matches the d2 card. Still 19 words, ending on *seepage*.

### #43 · `scenes.csv` · metro_intro, line 23 · joke-doesnt-land · accept

- **Before** (16w): The doors are blocked. The viewing is in 30 minutes. There is only one way out.
- **After**: *Doors will open on the right.* The couple is on the right.
- **Why**: The real metro announcement replaces a stock action line and a repeated time. The VS screen that follows makes the fight clear.

### #44 · `scenes.csv` · rival_student_join, line 33 · joke-doesnt-land · accept

- **Before** (13w): Bablu? From *that* college? I'm Kush. Tier-1. I'm also here for police verification.
- **After**: Bablu? From *that* college? Here for police verification? Same. I'm Kush. Tier-1.
- **Why**: Ends on the brag, which sets up join_rival_student's 'remind you he is Tier-1'.

### #45 · `scenes.csv` · aunty_betrayal, line 39 · verbose · accept

- **Before** (14w): And now it works, so I don't need you. Rules are rules. *No bachelors.*
- **After**: And now it works. So I don't need you. *No bachelors.*
- **Why**: 'Rules are rules' was filler between two strong beats.

### #47 · `scenes.csv` · nursing_home, line 46 · buried-mechanics · accept

- **Before** (13w): Bablu feels strong. Bablu feels green. Bablu feels like going back to *yesterday*.
- **After**: Bablu feels strong. Bablu feels green. Bablu wakes up *yesterday*.
- **Why**: States the rewind mechanic and turns a nostalgic phrase into a surreal punch.

### #48 · `scenes.csv` · broker_intro, line 49 · verbose · accept

- **Before** (11w): The metro is 4 km away. Also, I am a bachelor.
- **After**: The metro is 4 km away.
- **Why**: One objection, so the broker answers one joke. Must ship together with 49.

### #51 · `scenes.csv` · ceo_intro, line 62 · buried-mechanics · revise

- **Before** (18w): First task: our *culture-fit round*. And if you win, I call the *board*. The board is my relatives.
- **After**: First, the *culture-fit round*. Cut my profit in half and I call the *board*. The board is my relatives.
- **Why**: Correct trigger (below 50% HP, and his bar is 'Profit'), but 'Cut my profit in half' reads at a glance where 'Halve my profit' makes the reader stop.

### #52 · `scenes.csv` · ceo_outro, line 64 · inconsistent-voice · revise

- **Before** (12w): Okay okay! Board meeting adjourned. Bunty, write in the bahi-khata: *intern passed*.
- **After**: Bas, beta, bas! Board meeting adjourned. Bunty, write in the bahi-khata: *intern passed*.
- **Why**: Right to drop the broker's 'Okay okay!' tic. 'Enough!' was flat, though. 'Bas, beta, bas!' gives Lalaji his own patriarch voice, and context makes 'bas' (enough) clear.

### #53 · `scenes.csv` · ceo_outro, line 66 · verbose · accept

- **Before** (19w): Beta, it is an *unpaid* internship. But you can work from your flat. You do have a flat, right?
- **After**: Beta, it is *unpaid*. Work from your flat. You do have a flat, right?
- **Why**: Shrinks the repeated 'unpaid' setup so the flat punch, which ties back to the main quest, lands faster. The *unpaid* callback stays.

## JSON and C# UI strings

### #55 · `items.json` · chappal.breakText (line 4) · joke-doesnt-land · accept

- **Before** (12w): Your *Chappal* has been confiscated by Mummy. She needs it for something.
- **After**: Mummy has confiscated your *Chappal*. She needs it for you.
- **Why**: Every Indian kid knows what that chappal is for, and 'you' as the last word is the punch. 'For something' was vague.

### #56 · `items.json` · cork_ball.breakText (line 6) · joke-doesnt-land · accept

- **Before** (15w): Your last *Cork Ball* goes over the neighbour's wall. One tip, one hand: you're out.
- **After**: Your last *Cork Ball* goes over the neighbour's wall. Gully rules: you're out.
- **Why**: Fixes the wrong rule (one tip one hand is a catch rule). Over the wall is out.

### #57 · `items.json` · cricket_bat.breakText (line 10) · joke-doesnt-land · accept

- **Before** (11w): Your *Cricket Bat*'s lease has ended. The neighbour wants it back.
- **After**: Your *Cricket Bat*'s lease is up. Its owner got out and took it home.
- **Why**: Keeps the lease callback and adds the universal gully truth, with 'got out' working both ways.

### #58 · `items.json` · laathi.breakText (line 24) · joke-doesnt-land · accept

- **Before** (14w): Your *Laathi* is requisitioned by the local thana. They promise to return it. Temporarily.
- **After**: The local thana has borrowed your *Laathi*. Temporarily.
- **Why**: Half the words, and 'Temporarily' (meaning forever) lands cleanly.

### #59 · `items.json` · laathi.desc (line 24) · buried-mechanics · accept

- **Before** (12w): Police bamboo stick. +12 ATK, close. 25%: a Lathicharge hits every enemy.
- **After**: +12 ATK. Close. 25% chance: a Lathicharge hits every enemy. Police-issue bamboo.
- **Why**: Matches the house '+X ATK. Close.' format, mechanic first. All numbers kept.

### #60 · `items.json` · health_insurance.desc (line 18) · buried-mechanics · revise

- **Before** (12w): Faint? AIIMS (the big government hospital) revives you once at 50% HP.
- **After**: Faint? Revive once at 50% HP. AIIMS will even find you a bed.
- **Why**: Good joke, but 'even had' is past tense in a shop description for something that hasn't happened yet. Future tense reads right and still ends on 'bed'.

### #61 · `items.json` · pudin_hara.desc (line 20) · buried-mechanics · accept

- **Before** (9w): Mint tummy drops. Cures every status. Tastes like regret.
- **After**: Cures every status. Mint tummy drops. Aunties prescribe it for everything.
- **Why**: Mechanic first, and the joke justifies the cure-all. 'Tastes like regret' lives on in Run.cs's 'Nothing to cure' note, so it isn't lost.

### #62 · `items.json` · bubble_wrap.desc (line 26) · joke-doesnt-land · accept

- **Before** (11w): The box was empty. The bubble wrap is very satisfying, though.
- **After**: Two metres of bubble wrap, carefully packed around nothing.
- **Why**: Still tells you the box was empty, through the specific over-packing joke, and no longer repeats the keep-it log line.

### #63 · `GameController.cs` · GameController.cs:253 (title-screen metro line) · joke-doesnt-land · accept

- **Before** (10w): "\"Platform 16. Mind the gap, don't forget to mask up.\""
- **After**: "\"Platform 16. Please mind the gap. And the AQI.\""
- **Why**: The first-screen line ends on AQI and plants the smog gag. The original was a safety notice.

### #67 · `GameController.cs` · GameController.cs:572 (pheeka brew log line) · verbose · accept

- **Before** (16w): "*Pheeka* (weak) brew. No boost this fight. Tap when the glass is in the dark middle."
- **After**: "*Pheeka* (weak) brew. No boost this fight. Aim for the dark middle."
- **Why**: Matches the 'theek' line above it and drops an instruction the meter already shows.

### #68 · `GameController.cs` · GameController.cs:776 (blessing fizzled) · joke-doesnt-land · accept

- **Before** (11w): $"The *{skill.name}* blessing fizzles. Baba: \"Stars are cloudy today. No refunds.\""
- **After**: $"The *{skill.name}* blessing fizzles. Baba: \"Smog is hiding your stars. No refunds.\""
- **Why**: Smog replaces the generic 'cloudy' and ties into the AQI gag. 'No refunds' stays unique to this line, since baba_2 was revised away from it.

### #69 · `GameController.cs` · GameController.cs:961 (Nursing Home result card) · verbose · revise

- **Before** (29w): $"Knocked out by *{killer}*. *{GameData.HeroName}* wakes up at *Amma's Nursing Home*. Amma's bill: *₹{Run.NursingHomeBill}*. The Vaidya's jadi-booti cures every status and makes him a *Zombie* for the next fight."
- **After**: $"Knocked out by *{killer}*. *Amma's Nursing Home* bill: *₹{Run.NursingHomeBill}*. The jadi-booti cures every status and makes *{GameData.HeroName}* a *Zombie* for the next fight."
- **Why**: The trim is right, but the editor's colon made the Zombie buff sound like a result of the cure. 'And makes' states both effects plainly. 29 to 23 words.

### #70 · `GameController.cs` · GameController.cs:962 (Amma's blessing, closing quote) · unclear-hinglish · accept

- **Before** (17w): $"*Amma's blessing:* enemies are *{run.MercyPercent}% weaker* until {GameData.HeroName} gets past Day {run.S.furthestDay + 1}. \"Kha le beta, phir ladna.\""
- **After**: $"*Amma's blessing:* enemies are *{run.MercyPercent}% weaker* until {GameData.HeroName} gets past Day {run.S.furthestDay + 1}. \"Eat first, beta. Then fight.\""
- **Why**: A full Hindi sentence with no context loses non-Hindi players. 'Beta' keeps Amma's voice. All placeholders kept.

### #71 · `PhoneView.cs` · PhoneView.cs:300 (empty chip row in Messages) · joke-doesnt-land · accept

- **Before** (5w): "No skills. Everything is temporary."
- **After**: "No skills, no gear. Just 4 backlogs."
- **Why**: Verified that the chip row shows gear, passes, statuses and skills, so the empty state really means no skills and no gear. Bablu-specific, punch last.

### #72 · `PhoneView.cs` · PhoneView.cs:347 (status row suffix in Skills app) · verbose · accept

- **Before** (5w): " Cure: Pudin Hara at JhatPat."
- **After**: " Cure: Pudin Hara."
- **Why**: Stops the 46px sub-row from wrapping. JhatPat is the only place it's sold anyway.

### #73 · `PhoneView.cs` · PhoneView.cs:595 (JhatPat closed headline) · explains-joke · accept

- **Before** (7w): "Your Delivery Wale Bhaiya is running late."
- **After**: "Your Delivery Wale Bhaiya is on his way."
- **Why**: 'On his way' is what he always claims, which lets the next row's '2 minute, sir.' be the punch.

### #74 · `ShopView.cs` · ShopView.cs:347 (ultra-rare Laathi log) · joke-doesnt-land · accept

- **Before** (15w): "JhatPat Mystery Box: *ULTRA RARE Laathi!* The whole society WhatsApp group is talking about it."
- **After**: "JhatPat Mystery Box: *ULTRA RARE Laathi!* Pinky Aunty has already forwarded it to the society group."
- **Why**: A character callback beats 'is talking about it'. Caveat: the free day-1 box has about a 5% Laathi chance, which would name her before day 2. Acceptable, because the name alone reads as a WhatsApp aunty.

### #75 · `AutoBattle.cs` · AutoBattle.cs:116 (GRAP 4 hero-skip shout) · joke-doesnt-land · accept

- **Before** (7w): "Odd-Even! Your car stays home this round."
- **After**: "Odd-Even! You skip even rounds. You don't even own a car."
- **Why**: States the rule, which the banner shows only once, and the joke lands. Two lines at 40pt fit the 980x140 banner.

### #76 · `AutoBattle.cs` · AutoBattle.cs:123 (GRAP 4 enemy-skip shout) · buried-mechanics · accept

- **Before** (5w): "Odd-Even! Enemies wait their turn."
- **After**: "Odd-Even! Enemies skip odd rounds."
- **Why**: Same length, and it now tells the player which rounds. Pairs with 75.

### #77 · `AutoBattle.cs` · AutoBattle.cs:194 (monkey steals skill) · buried-mechanics · accept

- **Before** (5w): $"A monkey stole your {stolen.name}!"
- **After**: $"A monkey stole your {stolen.name}! Back after this fight."
- **Why**: Confirmed in AutoBattle: only this fight's active list loses the skill. In a game about expiring skills, players would otherwise read the theft as permanent.

### #78 · `AutoBattle.cs` · AutoBattle.cs:347 (Daaru Uncle heal popup) · joke-doesnt-land · accept

- **Before** (4w): "Desi tonic! Smells terrible."
- **After**: "Desi tonic! Purely medicinal."
- **Why**: The classic uncle excuse, same length, punch last. 'Smells terrible' was generic.

### #79 · `BattleView.cs` · BattleView.cs:680 (ComicWords array) · inconsistent-voice · accept

- **Before** (6w): { "DHISHOOM!", "PATAAK!", "DHADAM!", "THWACK!", "DHAAP!", "KAPOW!" }
- **After**: { "DHISHOOM!", "PATAAK!", "DHADAM!", "DHISHKYAON!", "DHAAP!", "TADAAK!" }
- **Why**: Keeps the hit words desi and on-brand. Low priority, as it's voice polish, not a verbosity fix.

## Lines the editors said to keep as they are

### events

- fight_scooty_uncle.text: ...On the footpath. Against traffic. Honking at *you*. (A clean three-step escalation with the punch word last. It's the model for every fight intro.)
- fight_auto_bhaiya.text + fight_auto_bhaiya_win.text: ...the four most powerful words in Delhi: *Meter se nahi jayenge.* / The meter has been broken since 2004. (The setup glosses the Hinglish for non-Delhi players, and the win pays it off with a specific detail.)
- fight_techbro_win.text: It gets 9k likes. You get endorsed for Violence. (A specific LinkedIn mechanic and a perfect last word. Keep this one and change the South Delhi Girl version.)
- fight_firecracker_win.text: The guard finally lets you through the gate. You were trying to leave. (A short reversal with the punch last. The fight_firecracker intro tweak only makes this land harder.)
- astro_intro.text: "Your stars are aligned, beta. Temporarily." (Puts the theme into the punch word without explaining it.)
- ev_daftar_chai (whole row): He said that on Day 1. / Kadak chai. Full HP. For one moment, everything is fine. / Now you are furious, which helps. (A real callback to the delivery gag, tight results, and the mechanic is clear.)
- ev_daftar_token.text + result_a: "Now serving: 12" ... Full HP. Now serving: 13. (The best callback-within-a-row in the file, and it states the mechanic in two words.)
- ev_daftar_forms (whole row): Form 16-B ... only issued after you submit Form 16-B. / Nothing is solved, but everything is waterproof now. (A specific sarkari loop. Both results are short and land.)
- ev_noida_sectors.result_b: He walks you there himself, and tells you his son's JEE rank. Twice. (Specific uncle behaviour, with 'Twice.' as a perfect one-word tag.)
- ev_noida_sector18.result_a: Forty minutes later you find a spot. In Sector 19. (Short and specific, with the punch last.)
- ev_noida_extension.text: "Possession in 6 months. Guaranteed." The brochure is from 2014. (A real NCR detail with the date as the punch.)
- ev_noida_dnd.result_b: You slow down. You don't know why. You are becoming one of them. (The best character beat in the event pool.)
- lose_1.text: A passing auto refuses to take him. Not even to AIIMS. (Calls back the auto-refusal gag where it hurts most.)
- d11_win.text: Bablu is officially a person. For 11 months. (The title payoff. Don't touch it.)
- join_sheru.text + leave_sheru.text: ...forever, or until the border. / You are not crying, it is the smog. (The setup and payoff are planted days apart, and the smog gag lands emotionally.)
- baba_3.text: *Astrologer Baba* accepts UPI now. "...Terms and conditions apply." (Specific, short, and it hints honestly at the fizzle/backfire odds.)
- news_6.text + news_5.text: Flyover completed on schedule. Experts investigate. / Gurgaon receives 10 minutes of rain. Residents switch to boats. (Ideal ticker length, and both are real NCR jokes.)
- credits.text: Fine: one kind word from the jury / Status: Temporary (The challan format carries it, and the last line closes the theme.)

### scenes

- intro, line 6: Gurugram internship letter! Stipend: ₹8,000. Accommodation: *""figure it out""*. (Short, specific, punch last, and it sets up the whole game in nine words.)
- intro, line 10: One flat. One stamp. 11 days. How hard can it be? (This is the game's tagline and the goal stated as a rhythm. The intro card echoes it, so leave it alone.)
- aunty_join, line 15: (turns it off and on again) (A six-word stage direction that pays off the whole 'Engineer?!' setup. Perfect.)
- metro_intro, lines 21-22: *Bro.* Can't you see we're having a *moment*? / Babe, he's staring. Babe. *BABE.* Do something. (Two real archetypes, each in under 10 words, and the escalation lands on the capitals.)
- daaru_uncle_join, line 27: Know it? I *sleep* in it. Come. I will protect you. I have bottles. (It does character and mechanic at once (he throws bottles), and the punch is the last two words.)
- sheru_join, lines 30-31: *Bhow.* / Okay. You can come. Your name is Sheru now. (Sweet and short. It is the emotional setup that leave_sheru pays off later.)
- rival_student_join, line 35: Sure. I'll share my notes. You do the fighting. I'll put it on my LinkedIn. (A specific archetype, rule of three, and the punch is the last word.)
- aunty_betrayal, lines 37-38: Beta. The RWA has voted. 47 to 1. The 1 was you. / Aunty! I fixed your WiFi! (The vote number is the joke, and the WiFi line calls back to aunty_join in five words.)
- aunty_betrayal, line 40: And for disrespecting the society notice board... *the head will be shaved.* (Notice-board officialese, a callback to 'I wrote that board myself', and it signals her shave status attack.)
- nursing_home, lines 42-43: *Amma's Nursing Home*. The ceiling fan is doing its best. So is Amma. / Beta, you were dead for four minutes. I have seen worse. Drink this. (Warm, specific, short. The best narrator line in the file.)
- broker_intro, lines 51-52: One month brokerage. One month deposit. One month ""documentation"". One month ""my cousin's wedding"". / That's four months! I get ₹8,000 a month! (A textbook escalating list with the absurd item last, plus a stipend callback in the reply.)
- broker_outro, line 57: Valid for 11 months, sir. Like everything. (The theme delivered in seven words, and it sets up the ending stamp.)
- ceo_intro, lines 59 and 61: We are a *startup*. Since 1972. / Family does not get paid. (The sharpest archetype roast in the script. Each punch is the last word.)
- babu_intro, lines 69-73: Hmm. / ...Bachelor? / ...Yes. / Window 7 is closed since 2009. (The pacing is the joke: one-word lines, then the running gag's origin. Don't add a word.)
- ending, lines 76, 78-79, 81-82: ...Fine. Stamped. Now go. / New rule: *no bachelors under 30*. / I am 21. / ...I love this city. / Everything is temporary. Except Delhi. (Tight, earned callbacks and a deadpan reply. A perfect closer.)

### ui

- GameController.cs:244 (title button): Sign the Agreement (The start button doubles as the premise; three words.)
- GameController.cs:223 (title tagline): Start playing before the chai gets cold. (Sets up the meter's 'Chai got cold.' timeout result.)
- GameController.cs:835 (ending stamp): VALID FOR 11 MONTHS (The whole theme in four words.)
- GameController.cs:612-614 (form-meter fields): Salary slip (you're an intern) / Four months' deposit / Salary: blessings / Self-attested, with your soul (Specific NCR paperwork, short, punch last; the triplicate gag works well here.)
- GameController.cs:688 (Baba skill pick): Pick fast. Blessings are temporary. Some are fake. (Teaches the timer and the fizzle/backfire mechanic, and ends on 'fake'.)
- AutoBattle.cs:290: Backfired! Nobody knows your father. (A perfect payoff for the 'Do You Know Who My Father Is?' skill name.)
- AutoBattle.cs:490: Come Tomorrow! Form rejected: wrong ink colour. (A very specific babu detail that also explains the boss revive.)
- AutoBattle.cs:199: Lunch Break! Come back in 2 rounds. (Running gag and mechanic (2 rounds invulnerable) in one line.)
- AutoBattle.cs:343 / 380 / 415: Good Morning Flower Image! / Owner is very nice! / Bunty! Pappu! BOARD MEETING! (Short, specific archetype lines that need no explanation.)
- combatants.json hpLabel (broker, south_delhi_girl, ceo, babu/rwa_aunty): Deposit / Followers / Profit / Patience (The HP bar is the joke; zero extra words.)
- combatants.json sector_cabbie.quip: Sector 62 ya 63? (Real Noida detail, readable without Hindi, matches the Lost in Sectors status.)
- items.json steel_tiffin.breakText: Your *Steel Tiffin* has a fist-shaped dent. Mummy wants it back anyway. (Specific, short, ends on 'anyway'.)
- items.json badminton_racket.breakText: The *Badminton Racket*'s strings snap. The society uncles blame the wind. (An accurate society-badminton detail with the punch last.)
- ShopView.cs:98 / 255: delivered in 10 minutes* / Delivered in 9 min {40-59} sec! (A quick-commerce parody that pays off on every purchase.)
- ShopView.cs:159 / 379: Might be bubble wrap. / You pop every single bubble. Worth it. (Sets expectations for the box, then a satisfying payoff.)
- PhoneView.cs:583 / 453: bitterChai OS 11.0 · valid for 11 months / B. Tech (4 backlogs) (Small, specific callbacks hidden where curious players find them.)
- skills.json (all desc fields): e.g. 30% chance: an extra hit for 150% damage. (Mechanics-only and scannable mid-fight. The skill names carry the jokes; don't add flavour here.)
