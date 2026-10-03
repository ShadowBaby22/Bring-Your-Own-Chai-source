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
    if bullets:
        reqs.append({'createParagraphBullets': {'range': rng(bullets[0][0], bullets[-1][1]), 'bulletPreset': 'BULLET_DISC_CIRCLE_SQUARE'}})
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
# --- descending document order ---
R += append_bullets('Sat 3 Oct: added the JhatPat shop', [
    '**Sat 3 Oct, evening:** built the Mystery Box (Laathi is the 5% ultra rare), the Map and Toll Tax from the note above, '
    'Gurugram as a zone (Tech Bro fight, CEO boss) and close vs ranged combat with thrown projectiles. Weapon ATK raised '
    'and retry fatigue added after 500 simulated runs; no run gets stuck now.'])
R += replace_table('Tasks', [
    ['Task', 'Owner', 'Status'],
    ['Rupees: earning, rent day, balance on screen', 'Siddharth (with Claude)', 'Done'],
    ['JhatPat shop, daily stock and Mystery Box', 'Siddharth (with Claude)', 'Done'],
    ['Close vs ranged combat and weapon effects', 'Siddharth (with Claude)', 'Done'],
    ['Map and Toll Tax at every border', 'Siddharth (with Claude)', 'Done'],
    ['Gurugram: Tech Bro, background, CEO moved home', 'Siddharth (with Claude)', 'Done'],
    ['Weapon icons and in-hand sprites', 'Claude', 'First pass done'],
    ['Phone UI redesign (Edits tab)', 'Siddharth (with Claude)', 'To do'],
    ['Playtest a full run on a phone and note problems in this doc', 'Open', 'To do'],
    ['Itch.io page: cover, screenshots, GIF, description', 'Open', 'To do'],
    ['More event cards and jokes (Gurugram has none yet)', 'Open', 'To do'],
], [232, 140, 96])
R += append_bullets('A mobile web build at the link above.', [
    'The JhatPat shop, rupees, rent and the Mystery Box, with an icon for every item.',
    'Close vs ranged combat: thrown projectiles, weapon perks and the weapon in the hero\'s hand.',
    'A map of NCR at every border, with the Toll Tax stamp.'])
R += replace_table('The 11 days', [
    ['Day', 'Zone', 'Type', 'What happens'],
    ['1', 'Dilli', 'Fight', 'Scooty Uncle, Auto Bhaiya or South Delhi Girl'],
    ['2', 'Dilli', 'Event', 'Pinky Aunty joins the party'],
    ['3', 'Dilli', 'Event', 'Random Dilli card (Sarojini, Rajiv Chowk, chole bhature and more)'],
    ['4', 'Dilli', 'Boss', 'The Broker'],
    ['5', 'Gurugram', 'Fight', 'LinkedIn Tech Bro, sometimes with a South Delhi Girl. Toll Tax.'],
    ['6', 'Gurugram', 'Boss', 'The Startup CEO, at home in Cyber Hub. Toll Tax.'],
    ['7', 'Noida', 'Fight', 'Monkey Gang or Lost Cab Driver. Toll Tax.'],
    ['8', 'Noida', 'Event', 'Random Noida card, plus rent day (-₹150)'],
    ['9', 'Noida', 'Fight', 'Firecracker Ban Violator or Lost Cab Driver, with the Society Guard'],
    ['10', 'Daftar', 'Event', 'Random Daftar card, including the chai tapri. Toll Tax.'],
    ['11', 'Daftar', 'Final boss', 'The Babu, Keeper of the Stamp. Toll Tax.'],
], [30, 66, 66, 306])
R += section(para('The 11 days')['startIndex'], [
    ('HEADING_1', 'Map and Toll Tax', False),
    ('NORMAL_TEXT', 'From the change log note. The route is Dilli, then Gurugram, then Noida, then the Daftar, so a run crosses three borders. '
                    'At each one a map of NCR shows the hero walking to the toll plaza.', False),
    ('NORMAL_TEXT', '**Toll Tax:** ATK, DEF and max HP drop 15% for 2 days. Everything is temporary, including the debuff.', True),
    ('NORMAL_TEXT', '**Borders:** Sirhaul toll (Dilli to Gurugram, after Day 4), DND Flyway (Gurugram to Noida, after Day 6: toll-free, '
                    'but the tax applies out of respect) and the Delhi border (Noida to the Daftar, after Day 9).', True),
    ('NORMAL_TEXT', '**FASTag:** a ₹150 shop item that skips the Toll Tax at the next border.', True),
])
R += replace_table('Rupees are the shop currency', [
    ['Source', 'Rupees'],
    ['Starting pocket money', '+₹200'],
    ['Normal fight win', '+₹60 to ₹150 (twice the enemies\' EXP)'],
    ['Boss win', '+₹300'],
    ['Event choices', 'Some pay or cost rupees: haggle at Sarojini (-₹80) or pay ₹500, a ₹100 challan from Maamu, ₹150 for a Film City role, and more.'],
    ['Rent day (Day 8)', '-₹150. The landlord does not care about the baraat.'],
    ['Mystery Box', '-₹200 to open. Return an unwanted item for half its price (at least ₹50).'],
    ['Losing a fight', 'No change. You keep your rupees.'],
], [150, 318])
R += section(para('Rupees')['startIndex'] if False else next(e for e in body if 'paragraph' in e and ptext(e) == 'Rupees\n')['startIndex'], [
    ('HEADING_2', 'Mystery Box (loot box)', False),
    ('NORMAL_TEXT', 'Always in stock for ₹200. It gives one random item with a rarity reveal, and the Laathi only comes from the box. '
                    'Keep the item (it is equipped straight away, replacing the old one) or return it for half price. The odds are shown in the shop.', False),
], then_table=([
    ['Rarity', 'Items', 'Chance'],
    ['Ultra rare', 'Laathi', '5%'],
    ['Epic', 'Cricket Bat, Hockey Stick', '20%'],
    ['Rare', 'Cork Ball, Badminton Racket, Steel Tiffin, FASTag', '41%'],
    ['Common', 'Chappal, Cutting Chai, or just bubble wrap', '34%'],
], [90, 318, 60]))
R += replace_table('The full catalogue:', [
    ['Item', 'Type', 'Price', 'Effect', 'Lasts'],
    ['Chappal', 'Ranged', '₹100', '+3 ATK. Never misses, even in smog. 30% extra damage against uncles.', '2 fights'],
    ['Cork Ball', 'Ranged', '₹250', '+6 ATK. The opening shot is a Yorker: 150% damage and a 25% chance to stun for a round.', '3 fights (balls go over the neighbour\'s wall)'],
    ['Badminton Racket', 'Close', '₹300', '+4 ATK. Attacks twice per turn at 60% damage each.', '3 fights'],
    ['Cricket Bat', 'Close', '₹350', '+8 ATK. 20% chance of a Cover Drive that hits for 200%.', '4 fights'],
    ['Hockey Stick', 'Close', '₹400', '+7 ATK. 15% chance to stun.', '4 fights'],
    ['Laathi (ultra rare)', 'Close', 'Mystery Box only', '+12 ATK. 25% chance of a Lathicharge that hits every enemy.', '5 fights'],
    ['Steel Tiffin (armour)', 'None', '₹200', '+6 DEF.', '3 fights'],
    ['FASTag', 'None', '₹150', 'Skips the Toll Tax at the next border.', 'One border'],
    ['Cutting Chai', 'None', '₹60', 'Heals 40% HP right away.', 'One use'],
], [92, 52, 54, 180, 90])
# --- text-only changes last (no indexes) ---
def rep(a, b): return {'replaceAllText': {'containsText': {'text': a, 'matchCase': True}, 'replaceText': b, 'tabsCriteria': {'tabIds': [TAB]}}}
R += [
    rep('The Babu (rubber stamps).', 'The Babu (rubber stamps), LinkedIn Tech Bro (cold emails).'),
    rep('"Come Tomorrow" restarts the day at full HP with a small attack bonus, so every run can reach the ending.',
        '"Come Tomorrow" restarts the day at full HP. Each retry adds 15% ATK and takes 8% off the enemies\' HP, so every run can reach the ending.'),
    rep('the player can open the shop on their phone before tapping Next Day.', 'the player can open JhatPat, the shop app, before tapping Next Day (and before Day 1).'),
    rep('12 sticker-style characters with hit poses and damage states, and 3 backgrounds.', '13 sticker-style characters with hit poses and damage states, and 4 backgrounds.'),
    rep('before the CEO on Day 8', 'before the CEO on Day 6'),
    rep('Target: about ₹1,500 earned per run.', 'Target: about ₹1,500 earned per run (500 simulated runs average ₹1,375).'),
    rep('Map (the 11-day track)', 'Map (the NCR route and the 11-day track)'),
]
print(json.dumps({'requests': R, 'writeControl': {'requiredRevisionId': rev}}, ensure_ascii=False))
