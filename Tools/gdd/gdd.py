"""Turn spec.txt into Google Docs batchUpdate requests for an EMPTY tab (text inserted at index 1).
Phase 1 (text + styles): python3 gdd.py phase1 TAB_ID > phase1.json
Tables are emitted to tables.json with the text of the paragraph each one follows (phase 2 uses a fresh read)."""
import json, re, sys

def u16(s): return len(s.encode('utf-16-le')) // 2

STYLE = {'TITLE': 'TITLE', 'H1': 'HEADING_1', 'H2': 'HEADING_2', 'P': 'NORMAL_TEXT', 'B': 'NORMAL_TEXT'}

def parse(path):
    paras, tables = [], []
    for line in open(path, encoding='utf-8'):
        line = line.rstrip('\n')
        if not line: continue
        kind, body = line.split('|', 1)
        if kind == 'TABLE':
            tables.append({'after': paras[-1]['text'], 'rows': json.loads(body)})
            continue
        bolds, plain, pos = [], '', 0
        for part in re.split(r'(\*\*.+?\*\*)', body):
            if part.startswith('**') and part.endswith('**'):
                t = part[2:-2]; bolds.append((u16(plain), u16(plain) + u16(t))); plain += t
            else:
                plain += part
        paras.append({'kind': kind, 'text': plain, 'bolds': bolds})
    return paras, tables

def phase1(tab):
    paras, tables = parse(sys.argv[3] if len(sys.argv) > 3 else 'spec.txt')
    text = '\n'.join(p['text'] for p in paras)
    reqs = [{'insertText': {'location': {'index': 1, 'tabId': tab}, 'text': text}}]
    idx, run = 1, None
    styles, bullets, bolds, links = [], [], [], []
    for p in paras:
        start, end = idx, idx + u16(p['text'])
        styles.append({'updateParagraphStyle': {'range': {'startIndex': start, 'endIndex': end, 'tabId': tab},
                       'paragraphStyle': {'namedStyleType': STYLE[p['kind']]}, 'fields': 'namedStyleType'}})
        if p['kind'] == 'B':
            run = [run[0], end] if run else [start, end]
        elif run:
            bullets.append(run); run = None
        for a, b in p['bolds']:
            bolds.append({'updateTextStyle': {'range': {'startIndex': start + a, 'endIndex': start + b, 'tabId': tab},
                          'textStyle': {'bold': True}, 'fields': 'bold'}})
        for m in re.finditer(r'https?://\S+?(?=[\s)]|$)', p['text']):
            a, b = u16(p['text'][:m.start()]), u16(p['text'][:m.end()])
            links.append({'updateTextStyle': {'range': {'startIndex': start + a, 'endIndex': start + b, 'tabId': tab},
                          'textStyle': {'link': {'url': m.group(0)}}, 'fields': 'link'}})
        idx = end + 1
    if run: bullets.append(run)
    for s, e in bullets:
        reqs.append({'createParagraphBullets': {'range': {'startIndex': s, 'endIndex': e, 'tabId': tab},
                     'bulletPreset': 'BULLET_DISC_CIRCLE_SQUARE'}})
    reqs += styles + bolds + links
    json.dump(tables, open('tables.json', 'w'), ensure_ascii=False)
    print(json.dumps(reqs, ensure_ascii=False))

if __name__ == '__main__':
    if sys.argv[1] == 'phase1': phase1(sys.argv[2])
