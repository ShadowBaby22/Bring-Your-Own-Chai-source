import json,sys
d=json.load(open(sys.argv[1]))['content']
print('REV',d['revisionId'])
def para(el): return ''.join(r.get('textRun',{}).get('content','') for r in el['paragraph']['elements'])
for t in d['tabs']:
    tid=t['tabProperties']['tabId']
    if len(sys.argv)>2 and tid!=sys.argv[2]: continue
    print('=== TAB',tid,t['tabProperties'].get('title'))
    for el in t['documentTab']['body']['content']:
        if 'table' in el:
            print('TABLE @',el['startIndex'])
            for row in el['table']['tableRows']:
                print('   |',' | '.join(''.join(para(c) for c in cell['content'] if 'paragraph' in c).strip().replace('\n',' / ') for cell in row['tableCells']))
        elif 'paragraph' in el:
            s=para(el).rstrip('\n')
            st=el['paragraph'].get('paragraphStyle',{}).get('namedStyleType','')
            if s: print(f"{el['startIndex']:5} {st[:9]:9} {s}")
