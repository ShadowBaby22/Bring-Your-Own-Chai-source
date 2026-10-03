"""Live GDD update #2 (shop numbers, Mystery Box, Map + Toll Tax, new day order). Builds one guarded batch from a fresh read.
python3 update2.py <read_doc.json> > batch.json"""
import json, subprocess, sys, tempfile
DI = "/Users/siddharthgupta/Library/Application Support/Claude/local-agent-mode-sessions/skills-plugin/039f7e42-79ab-4ede-9baf-110fa3df243a/db92beba-2067-43ae-99bb-9b3772125643/skills/google-workspace/scripts/docs_index.py"
TAB = 't.b10lkliin7su'
def u16(s): return len(s.encode('utf-16-le')) // 2

doc = json.load(open(sys.argv[1]))['content']
rev = doc['revisionId']
body = next(t for t in doc['tabs'] if t['tabProperties']['tabId'] == TAB)['documentTab']['body']['content']
def ptext(el): return ''.join(r.get('textRun', {}).get('content', '') for r in el['paragraph']['elements'])
def para(prefix):
    for el in body:
        if 'paragraph' in el and ptext(el).startswith(prefix): return el
    raise SystemExit('no paragraph ' + prefix)
def table_after(prefix):
    els = [e for e in body]
    for i, el in enumerate(els):
        if 'paragraph' in el and ptext(el).startswith(prefix):
            for e in els[i + 1:]:
                if 'table' in e: return e
    raise SystemExit('no table after ' + prefix)

def new_table(at, rows, widths):
    f = tempfile.NamedTemporaryFile('w', suffix='.json', delete=False); json.dump(rows, f); f.close()
    out = json.loads(subprocess.check_output(['python3', DI, 'new-table', '--at', str(at), '--data', f.name, '--tab', TAB, '--bold-header']))
    reqs = out['requests']
    loc = {'index': at + 1, 'tabId': TAB}
    for i, w in enumerate(widths):
        reqs.append({'updateTableColumnProperties': {'tableStartLocation': loc, 'columnIndices': [i], 'tableColumnProperties': {'widthType': 'FIXED_WIDTH', 'width': {'magnitude': w, 'unit': 'PT'}}, 'fields': 'width,widthType'}})
    reqs.append({'updateTableRowStyle': {'tableStartLocation': loc, 'tableRowStyle': {'preventOverflow': True}, 'fields': 'preventOverflow'}})
    return reqs

def replace_table(prefix, rows, widths):
    t = table_after(prefix)
    p = para(prefix)
    end = t['endIndex']
    nxt = body[body.index(t) + 1]
    if 'paragraph' in nxt and ptext(nxt) == '\n': end = nxt['endIndex']   # the blank line the new table brings back
    return [{'deleteContentRange': {'range': {'startIndex': t['startIndex'], 'endIndex': end, 'tabId': TAB}}}] + new_table(p['endIndex'] - 1, rows, widths)

def rng(a, b): return {'startIndex': a, 'endIndex': b, 'tabId': TAB}

def section(at, paras, then_table=None):
    """Insert paragraphs [(style, text, bullet?)] at the start of the paragraph at index `at`. ** marks a bold prefix."""
    reqs, styles, bullets, bolds = [], [], [], []
    text, idx = '', at
    for style, t, bullet in paras:
        bold = None
        if t.startswith('**'):
            end = t.index('**', 2); bold = (idx, idx + u16(t[2:end])); t = t[2:end] + t[end + 2:]
        styles.append((idx, idx + u16(t) + 1, style))
        if bullet: bullets.append((idx, idx + u16(t)))
        if bold: bolds.append(bold)
        text += t + '\n'; idx += u16(t) + 1
    reqs.append({'insertText': {'location': {'index': at, 'tabId': TAB}, 'text': text}})
    for a, b, st in styles:
        reqs.append({'updateParagraphStyle': {'range': rng(a, b), 'paragraphStyle': {'namedStyleType': st}, 'fields': 'namedStyleType'}})
    reqs.append({'deleteParagraphBullets': {'range': rng(at, idx)}})
    runs = []   # contiguous bullet paragraphs only (headings between bullets must stay headings)
    for bs, be in bullets:
        if runs and bs == runs[-1][1] + 1: runs[-1][1] = be
        else: runs.append([bs, be])
    for bs, be in runs:
        reqs.append({'createParagraphBullets': {'range': rng(bs, be), 'bulletPreset': 'BULLET_DISC_CIRCLE_SQUARE'}})
    reqs.append({'updateTextStyle': {'range': rng(at, idx), 'textStyle': {'bold': False}, 'fields': 'bold'}})
    for a, b in bolds:
        reqs.append({'updateTextStyle': {'range': rng(a, b), 'textStyle': {'bold': True}, 'fields': 'bold'}})
    if then_table:
        rows, widths = then_table
        reqs += new_table(idx - 1, rows, widths)   # after the last inserted paragraph
    return reqs

def append_bullets(prefix, items):
    """Add bullet paragraphs after an existing bullet paragraph (they inherit its bullet)."""
    p = para(prefix)
    at = p['endIndex'] - 1
    reqs, idx, text, bolds = [], at, '', []
    for t in items:
        idx += 1  # the newline that starts the new paragraph
        if t.startswith('**'):
            end = t.index('**', 2); bolds.append((idx, idx + u16(t[2:end]))); t = t[2:end] + t[end + 2:]
        text += '\n' + t; idx += u16(t)
    reqs.append({'insertText': {'location': {'index': at, 'tabId': TAB}, 'text': text}})
    reqs.append({'updateTextStyle': {'range': rng(at, idx), 'textStyle': {'bold': False}, 'fields': 'bold'}})
    for a, b in bolds:
        reqs.append({'updateTextStyle': {'range': rng(a, b), 'textStyle': {'bold': True}, 'fields': 'bold'}})
    return reqs

R = []
last = [e for e in body if 'paragraph' in e and ptext(e).strip()][-1]
R += append_bullets(ptext(last)[:40], [
    '**Sat 3 Oct, 20:55:** built from the 19:21 notes: Bablu dies for real (Lease Terminated, back to Day 1; Health Insurance revives once); '
    'KHATRA meter 1-8; lasting statuses with Pudin Hara; a companion per zone (Daaru Uncle, Sheru, Kush); Pinky Aunty\'s betrayal shaves Bablu bald; '
    'Lalaji the Lala CEO calls his board (Bunty, Pappu); metro PDA couple fight; Astrologer Baba timer with fake and backfiring wishes, skill icons; '
    'phone-died Game Over card; bigger text, zoomable map, grey smog, Delhi techno under 50% HP. Dev link: https://shadowbaby22.github.io/Bring-Your-Own-Chai/dev/'])
R += replace_table('Tasks', [
    ['Task', 'Owner', 'Status'],
    ['Death, KHATRA meter, Health Insurance', 'Siddharth (with Claude)', 'Done'],
    ['Lasting statuses + Pudin Hara antidote', 'Siddharth (with Claude)', 'Done'],
    ['Zone companions + Pinky Aunty betrayal', 'Siddharth (with Claude)', 'Done'],
    ['Lalaji (Lala CEO) + board meeting', 'Siddharth (with Claude)', 'Done'],
    ['Metro fight: PDA couple (1v2)', 'Siddharth (with Claude)', 'Done'],
    ['Bablu story (see Story section)', 'Team', 'Draft done, needs a team pass'],
    ['Phone UI (bitterChai)', 'Siddharth (with Claude)', 'Done, feedback welcome'],
    ['Playtest on phones, note deaths and balance here', 'Team', 'To do'],
    ['Balance pass after playtests', 'Siddharth (with Claude)', 'To do'],
    ['Itch.io page: cover, screenshots, GIF, description', 'Open', 'To do'],
    ['More event cards (Gurugram has none yet)', 'Open', 'To do'],
], [232, 140, 96])
R += append_bullets('A map of NCR at every border, with the Toll Tax stamp.', [
    'Real death with a phone-died Game Over card, the KHATRA meter and Health Insurance.',
    'Lasting statuses with paper-flip and comic popups; bald Bablu, dizzy stars; Pudin Hara cures all.',
    'Zone companions, Pinky Aunty\'s betrayal, Lalaji\'s board meeting, the metro PDA couple.'])
R += replace_table('The 11 days', [
    ['Day', 'Zone', 'Khatra', 'Type', 'What happens'],
    ['1', 'Dilli', '1/8', 'Fight', 'Scooty Uncle, Auto Bhaiya or South Delhi Girl'],
    ['2', 'Dilli', '1/8', 'Event', 'Pinky Aunty joins (Bablu fixes her WiFi)'],
    ['3', 'Dilli', '2/8', 'Fight', 'Metro, Rajiv Chowk: PDA couple Jaanu and Shona (1v2)'],
    ['4', 'Dilli', '2/8', 'Boss', 'The Broker'],
    ['5', 'Gurugram', '3/8', 'Fight', 'Tech Bro (sometimes with South Delhi Girl). Daaru Uncle joins. Toll Tax.'],
    ['6', 'Gurugram', '4/8', 'Boss', 'Lalaji (CEO); at half HP his board joins: Bunty (CFO), Pappu (CTO). Toll Tax.'],
    ['7', 'Noida', '5/8', 'Fight', 'Monkey Gang, Lost Cab Driver or Firecracker. Sheru joins. Toll Tax.'],
    ['8', 'Noida', '5/8', 'Event', 'Random Noida card, plus rent day (-₹150)'],
    ['9', 'Noida', '6/8', 'Fight', 'Pinky Aunty (RWA) turns on Bablu and shaves his head'],
    ['10', 'Daftar', '7/8', 'Event', 'Random Daftar card. Kush (rival uni) joins. Toll Tax.'],
    ['11', 'Daftar', '8/8', 'Final boss', 'The Babu, Keeper of the Stamp. Toll Tax.'],
], [30, 62, 46, 62, 268])
R += replace_table('The full catalogue:', [
    ['Item', 'Type', 'Price', 'Effect', 'Lasts'],
    ['Chappal', 'Ranged', '₹100', '+3 ATK. Never misses, even in smog. 30% extra damage against uncles.', '2 fights'],
    ['Cork Ball', 'Ranged', '₹250', '+6 ATK. The opening shot is a Yorker: 150% damage and a 25% chance to stun for a round.', '3 fights'],
    ['Badminton Racket', 'Close', '₹300', '+4 ATK. Attacks twice per turn at 60% damage each.', '3 fights'],
    ['Cricket Bat', 'Close', '₹350', '+8 ATK. 20% chance of a Cover Drive that hits for 200%.', '4 fights'],
    ['Hockey Stick', 'Close', '₹400', '+7 ATK. 15% chance to stun.', '4 fights'],
    ['Laathi (ultra rare)', 'Close', 'Mystery Box only', '+12 ATK. 25% chance of a Lathicharge that hits every enemy.', '5 fights'],
    ['Steel Tiffin (armour)', 'None', '₹200', '+6 DEF.', '3 fights'],
    ['FASTag', 'None', '₹150', 'Skips the Toll Tax at the next border.', 'One border'],
    ['Health Insurance', 'None', '₹300', 'If Bablu dies, AIIMS revives him once at 50% HP.', '3 days'],
    ['Pudin Hara', 'None', '₹80', 'Cures every lasting status. Always in stock.', 'One use'],
    ['Cutting Chai', 'None', '₹60', 'Heals 40% HP right away.', 'One use'],
], [92, 52, 54, 180, 90])
core = para('Core loop')['startIndex']
R += section(core, [
    ('HEADING_1', 'Theory of fun: real stakes', False),
    ('NORMAL_TEXT', 'Fun comes from decisions that matter, and choices only matter if Bablu can lose: skills, shop spending, close vs ranged, FASTag, antidote.', False),
    ('NORMAL_TEXT', '**Death is real:** HP 0 means Lease Terminated and back to Day 1. Health Insurance (₹300, lasts 3 days) revives once at 50% HP.', True),
    ('NORMAL_TEXT', '**KHATRA meter, 1/8 to 8/8:** a Balatro-style ante. It rises through the run, and every level gives enemies +8% HP and +10% ATK.', True),
    ('NORMAL_TEXT', '**Lasting statuses (Pokémon style):** each enemy can inflict one. Every status bleeds 1.5% max HP each fight round, on top of its own effect, until Pudin Hara.', True),
    ('NORMAL_TEXT', '**No stalling:** after round 15 it is AQI 999 sudden death until someone drops.', True),
    ('NORMAL_TEXT', '**Astrologer Baba:** pick within 10 seconds or he leaves; 15% of blessings fizzle and 10% backfire.', True),
    ('NORMAL_TEXT', '**Balance now (1,000 simulated runs):** a good shopper clears about 1 in 8 runs, and deaths climb through the run: 2% on Day 3, 8% at the Broker, 16% on Day 5, 38% at Lalaji, 26% at Pinky Aunty, 61% at the Babu. Never shopping clears 0%.', True),
    ('HEADING_2', 'Companions (one per zone, they leave at the border)', False),
    ('NORMAL_TEXT', '**Pinky Aunty (Dilli):** Good Morning Flower Image heals; Complaint to Society Secretary hits.', True),
    ('NORMAL_TEXT', '**Daaru Uncle (Gurugram):** throws bottles, shares his desi tonic, sometimes hits Bablu by mistake.', True),
    ('NORMAL_TEXT', '**Sheru the street dog (Noida):** bites every 2 rounds and can chase enemies off (stun).', True),
    ('NORMAL_TEXT', '**Kush, rival-uni topper (Daftar):** shares notes (+25% ATK), a glucose biscuit when Bablu is low, and takes the credit.', True),
    ('HEADING_2', 'Lasting statuses', False),
    ('NORMAL_TEXT', 'Every status also bleeds every round until Bablu drinks Pudin Hara (₹80, always in stock at JhatPat).', False),
], then_table=([
    ['Status', 'From', 'Effect'],
    ['Robbed', 'Auto Bhaiya', 'Lose ₹30 every morning'],
    ['Confused', 'Tech Bro, Pappu', '15% chance each turn to hit yourself; dizzy stars'],
    ['Head Shaved', 'Pinky Aunty (RWA)', '-30% DEF; bald Bablu'],
    ['Cancelled', 'South Delhi Girl', '-15% ATK'],
    ['Tyre Marks', 'Scooty Uncle', 'Skip your first turn in every fight'],
    ['Cringe', 'PDA couple', 'All healing halved'],
    ['Brokerage', 'The Broker, Bunty', 'Fights pay 15% less'],
    ['Unpaid Intern', 'Lalaji', '25% less EXP'],
    ['Lost in Sectors', 'Lost Cab Driver', '15% of your attacks miss'],
    ['Monkey Bite', 'Monkey', 'Bleeds double'],
    ['Smoked Out', 'Firecracker', '10% chance to cough instead of attacking'],
    ['Interrogated', 'Society Guard', 'Lose 8% HP at the start of every fight'],
], [100, 130, 238]))
R += section(core, [
    ('HEADING_1', 'Story: Bablu', False),
    ('NORMAL_TEXT', 'Bablu, 21, from Kanpur. B.Tech third year, four backlogs. His Gurugram internship starts in 11 days and the hostel wants his room back. Every society in NCR says No Bachelors, and the RWAs want police verification, stamped at the Sarkari Daftar. One flat, one stamp, 11 days.', False),
    ('NORMAL_TEXT', '**Dilli:** Pinky Aunty wrote the No Bachelors board, but Bablu fixes her WiFi so she adopts him. Metro fight at Rajiv Chowk. The Broker wants four months up front.', True),
    ('NORMAL_TEXT', '**Gurugram:** Daaru Uncle shows him Cyber Hub. His internship boss, Lalaji of Lalaji & Sons (a startup since 1972), pays in "family".', True),
    ('NORMAL_TEXT', '**Noida:** Sheru the street dog follows him home. Pinky Aunty turns on him at the RWA meeting and shaves his head.', True),
    ('NORMAL_TEXT', '**Daftar:** Kush from the rival uni tags along. The Babu: bachelor verification is Window 7, closed since 2009.', True),
    ('NORMAL_TEXT', '**Ending:** the stamp says valid for 11 months, and the RWA changes the rule to no bachelors under 30.', True),
    ('NORMAL_TEXT', 'Open for the team: more NCR moments, names and jokes. Add them right here.', False),
])
def rep(a, b): return {'replaceAllText': {'containsText': {'text': a, 'matchCase': True}, 'replaceText': b, 'tabsCriteria': {'tabIds': [TAB]}}}
R += [
    rep('"Come Tomorrow" restarts the day at full HP. Each retry adds 15% ATK and takes 8% off the enemies\' HP, so every run can reach the ending.',
        'if Bablu\'s HP hits 0 he dies: Lease Terminated, back to Day 1. Health Insurance revives him once.'),
    rep('The Startup CEO (pitch decks)', 'Lalaji the CEO (calculators; calls his relatives)'),
    rep('13 sticker-style characters with hit poses and damage states, and 4 backgrounds.', '20 sticker-style characters with hit poses and damage states, and 5 backgrounds.'),
]
print(json.dumps({'requests': R, 'writeControl': {'requiredRevisionId': rev}}, ensure_ascii=False))
