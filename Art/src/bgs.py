"""Backgrounds for KADAK Chai. 1320x820 (wider than the 1080 battle area so it can slide). Ground line ~y 580."""
import os, sys, random

OUT = '#1d1b22'
W, H = 1320, 820
random.seed(11)

def attrs(line, k, sw=4):
    d = {'stroke': OUT, 'stroke-width': sw, 'stroke-linejoin': 'round', 'stroke-linecap': 'round'} if line else {}
    for x, v in k.items(): d[x.replace('_', '-')] = v
    return ' '.join(f'{x}="{v}"' for x, v in d.items())
def rect(x, y, w, h, fill, line=True, rx=0, **k): return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" {attrs(line, k)}/>'
def circle(cx, cy, r, fill, line=True, **k): return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {attrs(line, k)}/>'
def path(d, fill, line=True, **k): return f'<path d="{d}" fill="{fill}" {attrs(line, k)}/>'
def line(x1, y1, x2, y2, w=4, color=OUT, **k): return f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{w}" stroke-linecap="round" {attrs(False, k)}/>'
def text(x, y, s, size, fill, anchor='middle', weight='900', rot=0, family='Arial Black, Arial, sans-serif'):
    return f'<text x="{x}" y="{y}" font-family="{family}" font-weight="{weight}" font-size="{size}" fill="{fill}" text-anchor="{anchor}" transform="rotate({rot} {x} {y})">{s}</text>'
def sky(top, bottom):
    return (f'<defs><linearGradient id="sky" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="{top}"/><stop offset="1" stop-color="{bottom}"/></linearGradient></defs>'
            f'<rect width="{W}" height="{H}" fill="url(#sky)"/>')
def svg(inner): return f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">{inner}</svg>'

def skyline(color, base, hmin, hmax, opacity):
    s, x = '', -20
    while x < W:
        w = random.randint(60, 140); h = random.randint(hmin, hmax)
        s += rect(x, base - h, w, h + 10, color, line=False, opacity=opacity)
        x += w - 4
    return s

def windows(x, y, w, h, cols, rows, color, lit='#fff3c4'):
    s = ''
    cw, ch = w / cols, h / rows
    for i in range(cols):
        for j in range(rows):
            c = lit if random.random() < 0.25 else color
            s += rect(x + i * cw + cw * 0.2, y + j * ch + ch * 0.2, cw * 0.6, ch * 0.55, c, line=False)
    return s

def stripes(x, y, w, h, n=8, a='#ffd600', b='#212121'):
    s = rect(x, y, w, h, a)
    sw = w / n
    for i in range(0, n, 2):
        s += path(f'M {x+i*sw} {y+h} L {x+i*sw+sw} {y+h} L {x+i*sw+sw*1.6} {y} L {x+i*sw+sw*0.6} {y} Z', b, line=False)
    return s + rect(x, y, w, h, 'none')

# ------------------------------------------------------------------ DILLI
def dilli():
    s = sky('#f6d2a6', '#d9a47a')
    s += circle(1060, 150, 62, '#fff4d6', line=False, opacity='0.55')
    s += skyline('#c08d6c', 470, 120, 260, '0.55')
    s += rect(0, 0, W, H, '#ffffff', line=False, opacity='0.10')
    # buildings with shops
    x = -10
    for w, h, col in [(260, 330, '#e8b04b'), (230, 280, '#d9725c'), (280, 360, '#8fb3a9'), (240, 300, '#e9c46a'), (330, 340, '#c97b63')]:
        s += rect(x, 580 - h, w, h, col) + windows(x + 10, 580 - h + 20, w - 20, h - 150, 4, 4, '#5d4037')
        x += w
    signs = [(232, 236, 'CHOLE BHATURE', '#c62828', '#fff59d'), (482, 108, 'TO-LET', '#ffffff', '#c62828'), (694, 200, 'PHOTOSTAT', '#1565c0', '#ffffff'),
             (906, 144, 'SWEETS', '#2e7d32', '#ffffff'), (1152, 160, 'MOBILE', '#6a1b9a', '#ffeb3b'), (-6, 140, 'DHABA', '#ef6c00', '#ffffff')]
    # tangled wires
    for i in range(7):
        y0 = 300 + i * 9
        s += path(f'M 0 {y0} Q 330 {y0+70+i*6} 660 {y0+10} Q 990 {y0+80-i*5} 1320 {y0+20}', 'none', stroke='#212121', stroke_width='3')
    # metro viaduct + train
    s += rect(0, 210, W, 46, '#bdbdbd') + rect(0, 250, W, 12, '#9e9e9e')
    for px in (180, 640, 1100):
        s += path(f'M {px-40} 262 L {px+40} 262 L {px+26} 300 L {px+26} 580 L {px-26} 580 L {px-26} 300 Z', '#cfcfcf')
    s += rect(260, 140, 760, 70, '#eceff1', rx=18) + rect(260, 186, 760, 10, '#e53935', line=False)
    s += ''.join(rect(290 + i * 92, 152, 64, 28, '#455a64', line=False, rx=6) for i in range(8))
    for x0, w, label, bg, fg in signs:   # drawn after the pillars so nothing hides them
        s += rect(x0, 452, w, 50, bg, rx=4) + text(x0 + w / 2, 487, label, 24, fg)
    # footpath, curb, road
    s += rect(0, 576, W, 44, '#bdb4a8') + stripes(0, 618, W, 16, n=44)
    s += rect(0, 634, W, 200, '#8f8b85', line=False)
    s += ''.join(rect(60 + i * 260, 726, 140, 10, '#f5f5f5', line=False, opacity='0.7') for i in range(6))
    # barricade (the running gag)
    s += rect(1010, 520, 14, 70, '#9e9e9e') + rect(1210, 520, 14, 70, '#9e9e9e') + stripes(990, 500, 250, 40, n=10)
    return svg(s)

# ------------------------------------------------------------------ NOIDA
def noida():
    s = sky('#b7c6d4', '#e2e6ea')
    s += skyline('#a7b4c0', 500, 100, 220, '0.6')
    # glass IT towers
    for x0, w, h in [(40, 200, 420), (270, 160, 340), (1040, 240, 460)]:
        s += rect(x0, 580 - h, w, h, '#7fb3c8') + windows(x0 + 10, 600 - h, w - 20, h - 60, 5, 10, '#5a8ca3', lit='#d6eef7')
    s += rect(1060, 98, 200, 40, '#263238', rx=6) + text(1160, 128, 'IT PARK 62', 26, '#80deea')
    # half-built flyover, rebar sticking out
    s += rect(380, 300, 470, 46, '#c9c2b8') + rect(380, 340, 470, 12, '#a8a197')
    for px in (450, 720):
        s += rect(px - 24, 352, 48, 228, '#d6d0c6')
    s += ''.join(line(850, 306 + i * 9, 880 + (i % 2) * 14, 300 + i * 9, 4, '#6d4c41') for i in range(5))
    s += rect(930, 352, 48, 228, '#d6d0c6') + ''.join(line(940 + i * 9, 352, 940 + i * 9, 320 - i * 4, 4, '#6d4c41') for i in range(4))
    # crane
    s += rect(960, 60, 24, 300, 'none', stroke='#f9a825', stroke_width='8') + line(972, 70, 1280, 70, 10, '#f9a825') + line(972, 70, 760, 70, 10, '#f9a825') + line(1180, 70, 1180, 170, 3, '#424242') + rect(1164, 170, 32, 26, '#757575')
    # billboard
    s += rect(560, 150, 300, 110, '#fff8e1') + text(710, 190, 'NOIDA EXTENSION', 26, '#bf360c') + text(710, 228, 'Possession: 2031*', 24, '#37474f', family='Arial, sans-serif', weight='700') + text(840, 252, '*T&amp;C apply', 12, '#78909c', anchor='end', family='Arial, sans-serif', weight='400')
    s += line(620, 260, 620, 300, 8, '#757575') + line(800, 260, 800, 300, 8, '#757575')
    # confusing sector gantry
    s += line(250, 360, 250, 580, 10, '#9e9e9e') + line(760, 360, 760, 580, 10, '#9e9e9e')
    s += rect(250, 366, 510, 120, '#2e7d32', rx=10)
    s += text(272, 404, 'Sector 18  →', 28, '#ffffff', anchor='start', family='Arial, sans-serif', weight='700')
    s += text(272, 438, 'Sector 62  ←', 28, '#ffffff', anchor='start', family='Arial, sans-serif', weight='700')
    s += text(272, 472, 'Sector 63  ↑  (also 62?)', 28, '#ffffff', anchor='start', family='Arial, sans-serif', weight='700')
    # expressway
    s += rect(0, 580, W, 30, '#9e9e9e') + rect(0, 606, W, 14, '#66bb6a', line=False)
    s += rect(0, 620, W, 210, '#7d7a76', line=False)
    s += ''.join(rect(30 + i * 220, 712, 120, 10, '#ffffff', line=False, opacity='0.8') for i in range(7))
    # stray dog, sitting
    s += path('M 1180 600 Q 1176 560 1200 548 L 1206 530 L 1216 546 Q 1240 548 1238 576 L 1244 600 Z', '#a1887f') + circle(1218, 556, 3, OUT, line=False)
    return svg(s)

# ------------------------------------------------------------------ GURUGRAM
def gurugram():
    s = sky('#a9c4dc', '#e4ecf2')
    s += skyline('#9fb3c6', 470, 160, 300, '0.55')
    # glass towers of Cyber City, with diagonal reflections
    for x0, w, h, col in [(-20, 230, 500, '#4f86b3'), (220, 180, 420, '#6fa3c9'), (880, 210, 470, '#5b8fbb'), (1100, 240, 540, '#3f77a6')]:
        s += rect(x0, 580 - h, w, h, col) + windows(x0 + 12, 600 - h, w - 24, h - 60, 5, 12, '#3a6c94', lit='#d8ecf9')
        s += path(f'M {x0+20} {580-h+40} L {x0+w*0.55} {580-h+40} L {x0+20} {580-h+40+w*0.9} Z', '#ffffff', line=False, opacity='0.18')
    s += rect(240, 130, 140, 40, '#0d47a1', rx=6) + text(310, 158, 'DLF', 26, '#ffffff')
    s += rect(1120, 70, 200, 44, '#212121', rx=6) + text(1220, 101, 'CYBER HUB', 24, '#ffd54f')
    # rapid metro on a viaduct
    s += rect(0, 236, W, 36, '#cfd8dc') + rect(0, 268, W, 10, '#b0bec5')
    for px in (120, 560, 1000):
        s += rect(px - 22, 278, 44, 302, '#dfe5e8')
    s += rect(430, 176, 600, 60, '#e0f7fa', rx=18) + rect(430, 214, 600, 10, '#00838f', line=False)
    s += ''.join(rect(456 + i * 90, 188, 60, 24, '#37474f', line=False, rx=6) for i in range(6))
    # "coming soon" mall + surge billboard
    s += rect(420, 330, 420, 250, '#eceff1') + stripes(420, 330, 420, 30, n=14, a='#ff7043', b='#ffffff')
    s += rect(456, 380, 348, 90, '#fff8e1', rx=8) + text(630, 418, 'MALL OF EVERYTHING', 26, '#bf360c') + text(630, 454, 'Coming Soon* (since 2012)', 22, '#37474f', family='Arial, sans-serif', weight='700')
    s += ''.join(line(440 + i * 46, 480, 440 + i * 46, 580, 4, '#b0bec5') for i in range(9))   # scaffolding
    s += rect(30, 330, 170, 120, '#263238', rx=8) + text(115, 372, 'SURGE', 26, '#ffeb3b') + text(115, 420, '3.2x', 40, '#ffffff')
    s += line(115, 450, 115, 580, 8, '#757575')
    # palm trees in planters
    for px in (330, 880):
        s += rect(px - 10, 470, 20, 110, '#8d6e63') + rect(px - 34, 560, 68, 22, '#bcaaa4')
        for a in (-170, -135, -100, -65, -30, 5):
            s += f'<g transform="rotate({a} {px} 466)"><ellipse cx="{px + 58}" cy="466" rx="62" ry="15" fill="#43a047" stroke="{OUT}" stroke-width="4"/><line x1="{px}" y1="466" x2="{px + 112}" y2="466" stroke="#2e7d32" stroke-width="3"/></g>'
        s += circle(px, 466, 12, '#6d4c41')
    # golf course road sign
    s += line(1250, 400, 1250, 580, 8, '#9e9e9e') + rect(1150, 400, 160, 70, '#2e7d32', rx=8) + text(1230, 430, 'Golf Course', 20, '#ffffff', family='Arial, sans-serif', weight='700') + text(1230, 456, 'Road  →', 20, '#ffffff', family='Arial, sans-serif', weight='700')
    # flooded road (every monsoon, always)
    s += rect(0, 576, W, 40, '#b0a99f') + rect(0, 616, W, 210, '#6f6d69', line=False)
    s += path('M 0 640 Q 330 626 660 640 Q 990 654 1320 640 L 1320 820 L 0 820 Z', '#5c8fb3', line=False, opacity='0.85')
    s += ''.join(path(f'M {60 + i * 170} {680 + (i % 3) * 30} q 30 -10 60 0 q 30 10 60 0', 'none', stroke='#d6ecf8', stroke_width='4', opacity='0.8') for i in range(8))
    # half-submerged car
    s += path('M 700 660 L 730 618 L 860 618 L 900 660 Z', '#e53935') + rect(742, 626, 50, 28, '#b3e5fc', rx=4) + rect(804, 626, 50, 28, '#b3e5fc', rx=4)
    s += path('M 680 662 Q 790 652 920 662', 'none', stroke='#d6ecf8', stroke_width='5')
    s += text(980, 700, 'blub', 22, '#d6ecf8', family='Arial, sans-serif', weight='700', rot=-8)
    return svg(s)

# ------------------------------------------------------------------ DAFTAR
def daftar():
    s = rect(0, 0, W, H, '#cfe3c4', line=False)
    s += rect(0, 470, W, 110, '#9fc394', line=False) + line(0, 470, W, 470, 4)
    # ceiling fan, tube light
    s += line(660, 0, 660, 40, 8) + circle(660, 46, 18, '#5d4037') + path('M 660 46 L 520 30 L 520 44 Z', '#6d4c41') + path('M 660 46 L 800 60 L 800 46 Z', '#6d4c41')
    s += rect(150, 70, 260, 20, '#fffde7', rx=10) + rect(130, 64, 300, 8, '#bdbdbd', line=False)
    # notice board
    s += rect(90, 150, 330, 230, '#c49a6c', rx=6)
    for x0, y0, c, rot in [(110, 170, '#ffffff', -4), (230, 166, '#fff59d', 3), (330, 180, '#ffffff', -2), (130, 270, '#ffccbc', 2), (260, 270, '#ffffff', -3)]:
        s += f'<g transform="rotate({rot} {x0+40} {y0+45})">' + rect(x0, y0, 80, 90, c) + ''.join(line(x0 + 10, y0 + 22 + i * 14, x0 + 70, y0 + 22 + i * 14, 2, '#9e9e9e') for i in range(4)) + circle(x0 + 40, y0 + 6, 5, '#e53935') + '</g>'
    s += text(255, 400, 'NOTICE: Office closed temporarily', 18, '#33691e', family='Arial, sans-serif', weight='700')
    # clock at 1:00, lunch sign
    s += circle(560, 150, 52, '#ffffff') + line(560, 150, 560, 112, 6) + line(560, 150, 578, 132, 6) + circle(560, 150, 5, OUT, line=False)
    s += rect(640, 116, 300, 84, '#fff8e1', rx=8) + text(790, 152, 'LUNCH 1:00 – 2:00', 26, '#c62828') + text(790, 184, '(approx.)', 20, '#6d4c41', family='Arial, sans-serif', weight='700')
    # window no. 4
    s += rect(980, 200, 290, 270, '#8d6e63', rx=6) + rect(1000, 220, 250, 200, '#455a64')
    s += ''.join(line(1000 + i * 25, 220, 1000 + i * 25, 420, 3, '#bdbdbd') for i in range(11))
    s += rect(1040, 150, 210, 44, '#1565c0', rx=6) + text(1145, 182, 'WINDOW No. 4', 24, '#ffffff')
    s += f'<g transform="rotate(-6 1125 300)">' + rect(1060, 280, 130, 50, '#ffffff') + text(1125, 314, 'CLOSED', 28, '#c62828') + '</g>'
    # file mountains
    for bx, n in [(440, 8), (1010, 6), (1150, 10)]:
        for i in range(n):
            c = '#d7ccc8' if i % 2 else '#bcaaa4'
            s += rect(bx + random.randint(-6, 6), 580 - (i + 1) * 22, 130, 22, c)
        s += line(bx + 64, 580, bx + 64, 580 - n * 22, 4, '#e53935')
    # counter + terrazzo floor
    s += rect(0, 580, W, 250, '#d8cfc4', line=False)
    s += ''.join(circle(random.randint(0, W), random.randint(590, 820), random.randint(2, 5), random.choice(['#a1887f', '#8d6e63', '#bcaaa4', '#ffffff']), line=False) for _ in range(260))
    s += line(0, 580, W, 580, 4)
    s += rect(640, 540, 40, 40, '#ffffff', opacity='0.8') + rect(646, 556, 28, 22, '#a1663a', line=False)  # chai glass
    return svg(s)

# ------------------------------------------------------------------ METRO (Rajiv Chowk coach)
def metro():
    s = rect(0, 0, W, H, '#dfe3e6', line=False)
    s += rect(0, 0, W, 70, '#c9ced2', line=False) + line(0, 70, W, 70, 4)
    s += rect(80, 16, 520, 40, '#111111', rx=6) + text(340, 46, 'Next: RAJIV CHOWK', 24, '#ff7043', family='Courier New, monospace', weight='900')
    s += rect(660, 14, 600, 44, '#1565c0', rx=6) + text(960, 44, 'BLUE LINE  Dwarka - Noida Electronic City', 20, '#ffffff', family='Arial, sans-serif', weight='700')
    # windows with the dark tunnel rushing past
    for x0 in (60, 470, 880):
        s += rect(x0, 110, 360, 210, '#263238', rx=24) + ''.join(line(x0 + 20 + i * 70, 130, x0 + 60 + i * 70, 300, 6, '#37474f') for i in range(5))
        s += rect(x0, 110, 360, 210, 'none', rx=24, stroke='#90a4ae', stroke_width='10')
    # grab handles and rails
    s += line(0, 92, W, 92, 8, '#9e9e9e')
    for x0 in range(40, W, 90):
        s += line(x0, 92, x0, 128, 4, '#9e9e9e') + rect(x0 - 14, 128, 28, 30, '#fafafa', rx=12, stroke_width='4')
    # signs
    s += rect(60, 344, 330, 56, '#ffffff', rx=6) + text(225, 380, 'Please do not sit on the floor', 18, '#c62828', family='Arial, sans-serif', weight='700')
    s += rect(920, 344, 300, 56, '#f8bbd0', rx=6) + text(1070, 380, 'Reserved for Ladies', 22, '#ad1457', family='Arial, sans-serif', weight='700')
    # sliding door with yellow edge
    s += rect(560, 330, 200, 250, '#b0bec5') + line(660, 330, 660, 580, 5) + rect(560, 330, 200, 250, 'none', stroke='#fdd835', stroke_width='10')
    s += rect(586, 360, 60, 120, '#263238', rx=8) + rect(674, 360, 60, 120, '#263238', rx=8)
    # long seats + poles
    s += rect(0, 470, 540, 70, '#1e88e5', rx=10) + rect(780, 470, 500, 70, '#1e88e5', rx=10)
    s += rect(0, 520, 540, 20, '#1565c0', line=False) + rect(780, 520, 500, 20, '#1565c0', line=False)
    for px in (300, 1040):
        s += line(px, 70, px, 580, 12, '#bdbdbd') + line(px - 3, 70, px - 3, 580, 3, '#ffffff', opacity='0.7')
    # floor
    s += rect(0, 580, W, 250, '#9ea7ad', line=False) + line(0, 580, W, 580, 4)
    s += ''.join(line(0, 620 + i * 50, W, 620 + i * 50, 2, '#8a9399') for i in range(4))
    s += rect(560, 580, 200, 18, '#fdd835', line=False)
    return svg(s)

out = sys.argv[1]
os.makedirs(out, exist_ok=True)
for name, fn in [('dilli', dilli), ('gurugram', gurugram), ('noida', noida), ('daftar', daftar), ('metro', metro)]:
    open(os.path.join(out, name + '.svg'), 'w').write(fn())
print('5 backgrounds')
