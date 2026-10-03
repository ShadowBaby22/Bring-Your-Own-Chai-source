"""JhatPat shop icons (item_*.svg, mystery_box.svg), enemy projectiles (shot_*.svg) and the NCR map (map.svg).
Same sticker look as the characters: thick dark outline + white sticker border.
Items are drawn for the HERO side, so text reads normally. Projectiles fly nose-first to the RIGHT."""
import math, os, sys

OUT = '#1d1b22'

def attrs(line, k, sw=7):
    d = {'stroke': OUT, 'stroke-width': sw, 'stroke-linejoin': 'round', 'stroke-linecap': 'round'} if line else {}
    for x, v in k.items(): d[x.replace('_', '-')] = v
    return ' '.join(f'{x}="{v}"' for x, v in d.items())
def path(d, fill, line=True, **k): return f'<path d="{d}" fill="{fill}" {attrs(line, k)}/>'
def ellipse(cx, cy, rx, ry, fill, line=True, **k): return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" {attrs(line, k)}/>'
def circle(cx, cy, r, fill, line=True, **k): return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {attrs(line, k)}/>'
def rect(x, y, w, h, rx, fill, line=True, **k): return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" {attrs(line, k)}/>'
def line(x1, y1, x2, y2, w=6, color=OUT, **k): return f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{w}" stroke-linecap="round" {attrs(False, k)}/>'
def text(x, y, s, size, fill, weight='900', anchor='middle', rot=0, family='Arial Black, Arial, sans-serif'):
    return (f'<text x="{x}" y="{y}" font-family="{family}" font-weight="{weight}" font-size="{size}" fill="{fill}" '
            f'text-anchor="{anchor}" transform="rotate({rot} {x} {y})">{s}</text>')
def g(inner, rot=0, cx=128, cy=128, sx=1, sy=1): return f'<g transform="rotate({rot} {cx} {cy}) translate({cx} {cy}) scale({sx} {sy}) translate({-cx} {-cy})">{inner}</g>'

def sticker(inner, w=256, h=256, glow=None):
    gl = ''
    if glow:
        gl = (f'<defs><radialGradient id="gl"><stop offset="0" stop-color="{glow}" stop-opacity="0.85"/>'
              f'<stop offset="1" stop-color="{glow}" stop-opacity="0"/></radialGradient></defs>'
              f'<circle cx="{w/2}" cy="{h/2}" r="{w/2}" fill="url(#gl)"/>')
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{gl}'
            '<defs><filter id="st" x="-12%" y="-12%" width="124%" height="124%">'
            '<feMorphology in="SourceAlpha" operator="dilate" radius="6" result="d"/>'
            '<feFlood flood-color="#ffffff"/><feComposite in2="d" operator="in" result="w"/>'
            '<feGaussianBlur in="d" stdDeviation="4" result="b"/><feOffset in="b" dx="0" dy="4" result="o"/>'
            '<feFlood flood-color="#000000" flood-opacity="0.28"/><feComposite in2="o" operator="in" result="s"/>'
            '<feMerge><feMergeNode in="s"/><feMergeNode in="w"/><feMergeNode in="SourceGraphic"/></feMerge></filter></defs>'
            f'<g filter="url(#st)">{inner}</g></svg>')

I = {}

# ------------------------------------------------------------------ items
def chappal():
    sole = path('M 128 30 Q 176 30 180 92 Q 184 150 168 196 Q 150 236 118 232 Q 84 228 80 186 Q 76 140 84 92 Q 90 30 128 30 Z', '#f5f5f5')
    top = path('M 128 44 Q 166 44 168 94 Q 170 148 156 190 Q 142 220 120 218 Q 94 214 92 184 Q 90 140 96 94 Q 100 44 128 44 Z', '#42a5f5', line=False)
    strap = path('M 96 120 Q 128 70 160 120', 'none', stroke='#1565c0', stroke_width='16') + path('M 96 120 Q 128 70 160 120', 'none', stroke=OUT, stroke_width='4', fill_opacity='0')
    s = sole + top + line(128, 82, 128, 104, 10, '#1565c0') + strap + circle(128, 82, 9, '#1565c0')
    return g(s, rot=28)

def cork_ball():
    s = circle(128, 128, 92, '#c62828')
    def q(p0, p1, p2, t): return ((1-t)**2*p0[0] + 2*(1-t)*t*p1[0] + t*t*p2[0], (1-t)**2*p0[1] + 2*(1-t)*t*p1[1] + t*t*p2[1])
    for side in (1, -1):
        p0, p1, p2 = (128 - 64 * side, 62), (128 - 0 * side, 128), (128 - 64 * side, 196)
        s += path(f'M {p0[0]} {p0[1]} Q {p1[0]} {p1[1]} {p2[0]} {p2[1]}', 'none', line=False, stroke='#f5f5f5', stroke_width='7')
        for k in range(8):
            x, y = q(p0, p1, p2, 0.08 + k * 0.12)
            s += line(x - 9, y - 3, x + 9, y + 3, 4, '#ffcdd2') if side > 0 else line(x - 9, y + 3, x + 9, y - 3, 4, '#ffcdd2')
    s += ellipse(98, 92, 22, 14, '#ffffff', line=False, opacity='0.45', transform='rotate(-30 98 92)')
    return s

def badminton_racket():
    head = ellipse(150, 86, 62, 76, 'none', stroke=OUT, stroke_width='14')
    face = ellipse(150, 86, 60, 74, '#eceff1', line=False) + ellipse(150, 86, 62, 76, 'none', line=False, stroke='#e53935', stroke_width='8')
    strings = ''.join(line(150 + dx, 86 - 70 * math.sqrt(max(0, 1 - (dx / 58) ** 2)), 150 + dx, 86 + 70 * math.sqrt(max(0, 1 - (dx / 58) ** 2)), 2, '#ffffff') for dx in range(-48, 50, 12)).replace('#ffffff', '#90a4ae')
    strings += ''.join(line(150 - 58 * math.sqrt(max(0, 1 - (dy / 70) ** 2)), 86 + dy, 150 + 58 * math.sqrt(max(0, 1 - (dy / 70) ** 2)), 86 + dy, 2, '#ffffff') for dy in range(-60, 64, 12)).replace('#ffffff', '#90a4ae')
    shaft = line(120, 156, 66, 226, 14, OUT) + line(120, 156, 66, 226, 7, '#9e9e9e')
    grip = line(84, 202, 52, 244, 26, OUT) + line(84, 202, 52, 244, 18, '#212121')
    cock = path('M 196 196 L 236 168 L 242 206 Z', '#ffffff') + circle(198, 198, 12, '#fff176')
    return face + strings + head + shaft + grip + cock

def cricket_bat():
    blade = path('M 108 120 L 196 32 Q 218 18 232 32 Q 246 46 232 68 L 144 156 Q 130 166 118 156 L 102 140 Q 96 128 108 120 Z', '#f0d9a7')
    sticker_ = path('M 150 92 L 206 36 L 222 52 L 166 108 Z', '#e53935', line=False) + text(186, 78, 'MRF', 20, '#ffffff', rot=-45)
    handle = line(108, 150, 40, 218, 24, OUT) + line(108, 150, 40, 218, 16, '#1e88e5')
    grip = ''.join(line(96 - i * 10, 160 + i * 10, 88 - i * 10, 156 + i * 10, 3, '#0d47a1') for i in range(6))
    return handle + grip + blade + sticker_ + line(120, 140, 210, 50, 3, '#d4b77a')

def hockey_stick():
    s = path('M 70 24 L 92 24 L 132 196 Q 140 222 168 222 L 214 222 Q 232 222 232 238 Q 232 248 214 248 L 156 248 Q 112 248 104 204 Z', '#8d6e63')
    s += path('M 76 24 L 88 24 L 100 76 L 86 76 Z', '#212121', line=False)
    s += ''.join(line(78 + i * 2.2, 30 + i * 9, 92 + i * 2.2, 30 + i * 9, 3, '#616161') for i in range(5))
    s += path('M 140 222 L 214 222 Q 232 222 232 238 Q 232 248 214 248 L 156 248 Z', '#fdd835', line=False, opacity='0.85')
    return s

def steel_tiffin():
    s = ''
    for i, y in enumerate((150, 104, 58)):
        s += rect(62, y, 132, 54, 14, '#cfd8dc') + rect(70, y + 8, 116, 10, 4, '#ffffff', line=False, opacity='0.6')
    s += path('M 66 58 Q 128 18 190 58', 'none', stroke=OUT, stroke_width='14') + path('M 66 58 Q 128 18 190 58', 'none', line=False, stroke='#90a4ae', stroke_width='7')
    s += rect(44, 70, 20, 130, 8, '#90a4ae') + rect(192, 70, 20, 130, 8, '#90a4ae')
    s += circle(54, 132, 6, '#546e7a', line=False) + circle(202, 132, 6, '#546e7a', line=False)
    return s

def fastag():
    s = g(rect(30, 64, 196, 128, 18, '#ffffff') + rect(30, 64, 196, 40, 18, '#2e7d32') + rect(30, 90, 196, 14, 0, '#2e7d32', line=False)
          + text(128, 96, 'FASTag', 30, '#ffffff')
          + ''.join(rect(46 + i * 9, 120, 5 if i % 3 else 3, 52, 0, OUT, line=False) for i in range(12))
          + rect(160, 118, 52, 56, 6, '#e8f5e9') + path('M 172 146 L 182 158 L 202 132', 'none', stroke='#2e7d32', stroke_width='8'), rot=-8)
    return s

def cutting_chai():
    s = path('M 70 92 L 186 92 L 172 222 Q 170 232 160 232 L 96 232 Q 86 232 84 222 Z', '#e3f2fd', opacity='0.95')
    s += path('M 78 126 L 178 126 L 170 220 Q 168 226 160 226 L 96 226 Q 88 226 86 220 Z', '#c47a35', line=False)
    s += ''.join(line(80 + i * 16, 96, 88 + i * 13, 222, 3, '#ffffff', opacity='0.55') for i in range(7))
    s += path('M 104 70 Q 92 52 106 36 Q 118 22 106 6', 'none', stroke='#b0bec5', stroke_width='7', opacity='0.8')
    s += path('M 144 74 Q 132 56 146 40 Q 158 26 146 10', 'none', stroke='#b0bec5', stroke_width='7', opacity='0.8')
    return s

def laathi():
    s = line(44, 228, 214, 26, 30, OUT) + line(44, 228, 214, 26, 20, '#c8a165')
    for t in (0.12, 0.42, 0.72, 0.94):  # bamboo knots + brass bands
        x, y = 44 + (214 - 44) * t, 228 + (26 - 228) * t
        s += line(x - 10, y - 8, x + 10, y + 8, 8, '#8d6e63') if t not in (0.12, 0.94) else line(x - 13, y - 11, x + 13, y + 11, 12, '#ffca28')
    s += line(70, 190, 200, 40, 4, '#e6c48a')
    for a, r in ((20, 26), (140, 30), (250, 24), (320, 22)):  # sparkles
        x, y = 128 + 92 * math.cos(math.radians(a)), 128 + 92 * math.sin(math.radians(a))
        s += path(f'M {x} {y-r} Q {x} {y} {x+r} {y} Q {x} {y} {x} {y+r} Q {x} {y} {x-r} {y} Q {x} {y} {x} {y-r} Z', '#fff59d', line=False)
    return s

def bubble_wrap():
    s = g(rect(40, 40, 176, 176, 14, '#e1f5fe', opacity='0.9'), rot=8)
    for i in range(4):
        for j in range(4):
            x, y = 66 + i * 42, 66 + j * 42
            s += circle(x, y, 15, '#b3e5fc', stroke_width='4') + circle(x - 5, y - 5, 4, '#ffffff', line=False)
    return s

def mystery_box():
    s = path('M 40 98 L 128 132 L 216 98 L 216 208 L 128 244 L 40 208 Z', '#c58b4c')
    s += path('M 40 98 L 128 132 L 128 244 L 40 208 Z', '#b07a3e')
    s += path('M 40 98 L 128 64 L 216 98 L 128 132 Z', '#dba567')
    s += path('M 40 98 L 14 62 L 102 30 L 128 64 Z', '#d29a5c') + path('M 216 98 L 242 62 L 154 30 L 128 64 Z', '#d29a5c')
    s += path('M 84 81 L 172 115 L 172 226 L 160 231 L 160 121 L 72 86 Z', '#FFD23F', line=False)
    s += text(150, 196, '?', 72, '#4a148c', rot=-8) + text(84, 200, '?', 44, '#4a148c', rot=10)
    return s

# ------------------------------------------------------------------ enemy projectiles (fly right)
def iced_coffee():
    s = path('M 76 70 L 180 70 L 166 228 Q 164 236 156 236 L 100 236 Q 92 236 90 228 Z', '#f3e5d0', opacity='0.95')
    s += path('M 82 120 L 174 120 L 166 226 L 90 226 Z', '#8d5a3b', line=False)
    s += rect(100, 132, 26, 26, 6, '#e1f5fe', line=False, opacity='0.8') + rect(134, 150, 24, 24, 6, '#e1f5fe', line=False, opacity='0.8')
    s += rect(66, 56, 124, 22, 10, '#ffffff') + line(150, 60, 178, 10, 12, OUT) + line(150, 60, 178, 10, 6, '#ec407a')
    s += rect(98, 166, 60, 34, 6, '#ffffff', line=False) + circle(128, 183, 12, '#00704a', line=False)
    return g(s, rot=20)

def rocket():
    s = path('M 40 112 L 170 112 L 222 128 L 170 144 L 40 144 Z', '#e53935')
    s += path('M 170 112 L 222 128 L 170 144 Z', '#ffd54f')
    s += path('M 56 112 L 34 90 L 80 112 Z', '#ffd54f') + path('M 56 144 L 34 166 L 80 144 Z', '#ffd54f')
    s += ''.join(line(80 + i * 26, 114, 92 + i * 26, 142, 5, '#ffeb3b') for i in range(3))
    s += line(40, 128, 6, 128, 6, '#8d6e63')
    s += path('M 30 116 Q 4 128 30 140 Q 18 128 30 116 Z', '#ff9800', line=False)
    return s

def honk():
    s = path('M 30 96 L 70 96 L 120 56 L 120 200 L 70 160 L 30 160 Z', '#ffca28')
    for i, r in enumerate((40, 70, 100)):
        s += path(f'M {140 + i * 10} {128 - r * 0.8} Q {140 + i * 10 + r * 0.55} 128 {140 + i * 10} {128 + r * 0.8}', 'none', stroke='#ff7043', stroke_width='12')
    return s

def pitch_deck():
    s = g(rect(26, 50, 204, 150, 12, '#ffffff') + rect(26, 50, 204, 34, 12, '#26a69a') + rect(26, 70, 204, 14, 0, '#26a69a', line=False)
          + text(128, 78, 'SERIES A', 24, '#ffffff')
          + path('M 50 180 L 90 150 L 120 162 L 170 110 L 206 96', 'none', stroke='#e53935', stroke_width='10')
          + path('M 190 92 L 210 94 L 204 114', 'none', stroke='#e53935', stroke_width='10')
          + text(70, 120, '10x', 30, '#26a69a'), rot=-10)
    return s

def rubber_stamp():
    s = path('M 100 40 Q 100 20 128 20 Q 156 20 156 40 L 150 120 L 106 120 Z', '#795548')
    s += ellipse(128, 26, 30, 12, '#8d6e63')
    s += rect(62, 120, 132, 34, 8, '#5d4037') + rect(56, 150, 144, 26, 6, '#c62828')
    s += g(rect(54, 186, 148, 52, 6, 'none', stroke='#c62828', stroke_width='6') + text(128, 224, 'REJECTED', 26, '#c62828'), rot=-6)
    return s

def cold_email():
    s = g(rect(30, 64, 196, 132, 14, '#ffffff') + path('M 34 72 L 128 140 L 222 72', 'none', stroke=OUT, stroke_width='8')
          + rect(166, 40, 66, 66, 14, '#0a66c2') + text(199, 88, 'in', 40, '#ffffff', family='Arial, sans-serif'), rot=-8)
    return s

# ------------------------------------------------------------------ zone icons + gear (top bar, map)
def india_gate():
    s = rect(46, 206, 164, 26, 4, '#d7b48a')
    s += path('M 62 206 L 62 92 L 194 92 L 194 206 L 150 206 L 150 150 Q 128 118 106 150 L 106 206 Z', '#e9c89c')
    s += rect(52, 70, 152, 24, 4, '#d7b48a') + rect(70, 50, 116, 22, 4, '#e9c89c') + rect(96, 34, 64, 18, 4, '#d7b48a')
    s += line(74, 110, 182, 110, 4, '#c49a6c') + line(74, 124, 182, 124, 4, '#c49a6c')
    s += path('M 118 206 L 118 176 Q 128 164 138 176 L 138 206 Z', '#ff9800', line=False, opacity='0.85')  # eternal flame glow
    return s

def beer():
    s = path('M 60 70 L 168 70 L 160 226 Q 158 236 148 236 L 80 236 Q 70 236 68 226 Z', '#ffb300')
    s += path('M 168 104 Q 214 104 214 146 Q 214 188 164 188', 'none', stroke=OUT, stroke_width='22') + path('M 168 104 Q 214 104 214 146 Q 214 188 164 188', 'none', line=False, stroke='#ffe082', stroke_width='10')
    s += ''.join(circle(x, y, r, '#ffffff', line=False, opacity='0.7') for x, y, r in ((96, 150, 7), (130, 190, 5), (112, 210, 6), (140, 130, 4)))
    s += path('M 50 78 Q 50 38 86 42 Q 100 20 128 30 Q 156 18 170 44 Q 196 44 186 78 Z', '#ffffff')
    return s

def katta():
    s = path('M 28 92 L 214 92 Q 228 92 228 106 L 228 128 L 120 128 L 112 138 L 84 138 L 74 128 L 28 128 Z', '#78909c')
    s += path('M 74 128 L 112 128 L 98 224 Q 96 236 84 236 L 54 236 Q 42 236 46 222 Z', '#6d4c41')
    s += path('M 104 140 Q 128 140 128 166 Q 128 182 112 184', 'none', stroke=OUT, stroke_width='9')
    s += rect(28, 80, 26, 14, 3, '#546e7a') + line(150, 104, 216, 104, 5, '#546e7a') + ''.join(line(60 + i * 10, 166 + i * 4, 84 + i * 6, 166 + i * 4, 4, '#4e342e') for i in range(4))
    return g(s, rot=-14)

def stamp_file():
    s = g(rect(40, 96, 176, 128, 10, '#ffe0b2') + rect(40, 84, 80, 24, 8, '#ffcc80') + line(64, 150, 190, 150, 6, '#bcaaa4') + line(64, 176, 160, 176, 6, '#bcaaa4'), rot=-6)
    s += path('M 150 28 Q 150 12 172 12 Q 194 12 194 28 L 188 82 L 156 82 Z', '#795548') + rect(130, 82, 84, 24, 6, '#c62828')
    s += g(circle(110, 186, 40, 'none', stroke='#c62828', stroke_width='8') + text(110, 196, 'OK', 30, '#c62828'), rot=-12)
    return s

def gear():
    cx = cy = 128; teeth = 8; pts = []
    for i in range(teeth * 2):
        for j in (0, 1):
            a = math.pi * 2 * (i + j * 0.9) / (teeth * 2) - math.pi / teeth / 2
            r = 104 if i % 2 == 0 else 78
            pts.append(f'{cx + r * math.cos(a):.1f},{cy + r * math.sin(a):.1f}')
    s = f'<polygon points="{" ".join(pts)}" fill="#eceff1" stroke="{OUT}" stroke-width="9" stroke-linejoin="round"/>'
    s += circle(cx, cy, 34, '#455a64')
    return s

zones = {'dilli': india_gate, 'gurugram': beer, 'noida': katta, 'daftar': stamp_file}

def chili():
    s = path('M 70 70 Q 40 140 90 196 Q 140 240 206 226 Q 150 210 128 168 Q 110 128 118 84 Z', '#e53935')
    s += path('M 84 92 Q 74 140 100 176', 'none', line=False, stroke='#ff8a80', stroke_width='10', opacity='0.8')
    s += path('M 70 70 Q 82 44 112 48 Q 124 60 118 84 Q 96 66 70 70 Z', '#43a047') + path('M 100 50 Q 96 28 114 16', 'none', stroke='#2e7d32', stroke_width='10')
    return s

def scooty():
    """Delivery Wale Bhaiya's scooty with the JhatPat box, running late."""
    s = circle(64, 196, 30, '#37474f') + circle(64, 196, 12, '#b0bec5') + circle(196, 196, 30, '#37474f') + circle(196, 196, 12, '#b0bec5')
    s += path('M 40 170 Q 60 130 112 132 L 150 132 L 170 104 L 186 104 L 196 168 Q 196 182 182 182 L 96 182 Q 80 162 40 170 Z', '#ffca28')
    s += line(172, 104, 190, 70, 8) + line(178, 70, 206, 70, 8)                                   # handlebar
    s += rect(24, 64, 82, 70, 8, '#FFD23F') + path('M 70 76 L 54 100 L 66 100 L 58 122 L 80 94 L 68 94 Z', '#e8590c', stroke_width='4')  # JhatPat box
    s += circle(136, 84, 24, '#d9a066') + path('M 112 82 Q 112 52 136 52 Q 162 52 162 82 Z', '#e53935')  # bhaiya + helmet
    s += path('M 120 106 Q 136 98 152 106 L 160 140 L 116 140 Z', '#43a047')
    s += ''.join(line(10, y, 30, y, 6, '#90a4ae') for y in (150, 170, 190))                       # speed lines
    s += text(214, 46, '2 min', 26, '#e53935', rot=-8)
    return s

def pudin_hara():
    s = path('M 96 70 L 160 70 L 160 96 Q 196 112 196 150 L 196 214 Q 196 232 178 232 L 78 232 Q 60 232 60 214 L 60 150 Q 60 112 96 96 Z', '#66bb6a', opacity='0.95')
    s += rect(100, 36, 56, 38, 8, '#eceff1') + rect(70, 140, 116, 64, 8, '#ffffff') + text(128, 168, 'PUDIN', 22, '#1b5e20') + text(128, 194, 'HARA', 22, '#1b5e20')
    s += path('M 176 60 Q 210 40 226 66 Q 206 84 176 60 Z', '#43a047') + path('M 186 92 Q 222 84 230 112 Q 204 120 186 92 Z', '#43a047')
    return s

def cond_overlay_svg(inner):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="512" height="640" viewBox="0 0 512 640">{inner}</svg>')

def cond_shaved():
    # Bald dome over Bablu's hair (the chibi template's head is a circle at 262,232 r138).
    s = path('M 120 240 Q 108 86 262 80 Q 414 80 406 224 Q 330 196 262 200 Q 190 200 120 240 Z', '#D9A066', stroke_width='8')
    s += ellipse(220, 130, 46, 22, '#ffffff', line=False, opacity='0.45', transform='rotate(-18 220 130)')
    s += ''.join(line(x, y, x + 10, y + 4, 4, '#c62828') for x, y in ((300, 120), (330, 150), (196, 170)))   # razor nicks
    return s

def cond_confused():
    s = ellipse(262, 70, 120, 26, 'none', line=False, stroke='#ffffff', stroke_width='5', stroke_dasharray='14 10', opacity='0.8')
    for x, y, r in ((160, 72, 26), (262, 46, 30), (364, 76, 24)):
        pts = []
        for i in range(10):
            rr = r if i % 2 == 0 else r * 0.45
            a = math.pi * 2 * i / 10 - math.pi / 2
            pts.append(f'{x + rr * math.cos(a):.1f},{y + rr * math.sin(a):.1f}')
        s += f'<polygon points="{" ".join(pts)}" fill="#ffeb3b" stroke="{OUT}" stroke-width="5" stroke-linejoin="round"/>'
    return s

def khatra():
    """KHATRA 440 VOLT: the skull-and-lightning danger sign on every Delhi electric pole."""
    s = path('M 128 18 L 244 222 L 12 222 Z', '#ffd600', stroke_width='10')
    s += circle(128, 118, 40, '#ffffff') + rect(108, 146, 40, 26, 6, '#ffffff')
    s += circle(112, 116, 11, OUT, line=False) + circle(144, 116, 11, OUT, line=False) + path('M 128 128 L 120 142 L 136 142 Z', OUT, line=False)
    s += line(114, 160, 114, 172, 4) + line(128, 160, 128, 172, 4) + line(142, 160, 142, 172, 4)
    s += path('M 200 120 L 176 168 L 194 168 L 182 206 L 218 150 L 198 150 L 212 120 Z', '#e53935', stroke_width='4')
    s += text(84, 208, '440V', 26, '#e53935')
    return s

def star():
    pts = []
    for i in range(10):
        r = 110 if i % 2 == 0 else 48
        a = math.pi * 2 * i / 10 - math.pi / 2
        pts.append(f'{128 + r * math.cos(a):.1f},{132 + r * math.sin(a):.1f}')
    return f'<polygon points="{" ".join(pts)}" fill="#ffeb3b" stroke="{OUT}" stroke-width="12" stroke-linejoin="round"/>'

# ------------------------------------------------------------------ bitterChai phone app icons (glossy dark squares, bitterSweet style)
def app_base(inner):
    return ('<defs><linearGradient id="gl2" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#4a5056"/><stop offset="1" stop-color="#1c1f22"/></linearGradient></defs>'
            + rect(18, 18, 220, 220, 48, 'url(#gl2)', stroke_width='8') + inner
            + path('M 30 66 Q 30 30 66 30 L 190 30 Q 226 30 226 66 L 226 104 Q 128 130 30 104 Z', '#ffffff', line=False, opacity='0.13'))

def app_messages():
    return app_base(path('M 60 82 Q 60 60 84 60 L 172 60 Q 196 60 196 82 L 196 142 Q 196 164 172 164 L 112 164 L 76 196 L 84 164 Q 60 164 60 142 Z', '#66bb6a')
                    + ''.join(circle(x, 112, 10, '#ffffff', line=False) for x in (96, 128, 160)))

def app_jhatpat():
    return app_base(path('M 70 98 L 186 98 L 176 206 L 80 206 Z', '#FFD23F') + path('M 98 98 Q 98 58 128 58 Q 158 58 158 98', 'none', stroke=OUT, stroke_width='10')
                    + path('M 136 112 L 106 160 L 128 160 L 116 196 L 152 142 L 130 142 Z', '#e8590c', stroke_width='5'))

def app_skills():
    return app_base(ellipse(128, 202, 60, 14, '#6d4c41') + circle(128, 124, 64, '#7e57c2')
                    + circle(108, 104, 18, '#ffffff', line=False, opacity='0.45') + path('M 128 92 l 8 20 l 22 2 l -17 14 l 6 21 l -19 -12 l -19 12 l 6 -21 l -17 -14 l 22 -2 Z', '#ffe082', stroke_width='4'))

def app_profile():
    return app_base(rect(54, 70, 148, 118, 14, '#eceff1') + rect(54, 70, 148, 26, 14, '#26a69a') + circle(98, 136, 24, '#f2a65a')
                    + path('M 70 186 Q 98 150 126 186 Z', '#2bb3a3', stroke_width='5') + line(140, 124, 186, 124, 7, '#90a4ae') + line(140, 148, 178, 148, 7, '#90a4ae'))

def app_map():
    return app_base(path('M 52 76 L 100 60 L 156 80 L 204 64 L 204 182 L 156 198 L 100 178 L 52 194 Z', '#fff3c4')
                    + line(100, 60, 100, 178, 5) + line(156, 80, 156, 198, 5)
                    + path('M 70 150 Q 100 120 128 140 Q 150 156 184 110', 'none', stroke='#e8590c', stroke_width='7', stroke_dasharray='4 12')
                    + path('M 184 86 Q 200 86 200 102 Q 200 116 184 134 Q 168 116 168 102 Q 168 86 184 86 Z', '#e53935', stroke_width='5'))

def app_settings():
    return app_base(g(gear(), sx=0.62, sy=0.62))

phone_apps = {'messages': app_messages, 'jhatpat': app_jhatpat, 'skills': app_skills, 'profile': app_profile, 'map': app_map, 'settings': app_settings}

# ------------------------------------------------------------------ Astrologer Baba skill icons (skill_<id>)
def sk_horn():
    s = path('M 40 110 L 90 110 L 196 50 L 196 206 L 90 146 L 40 146 Z', '#ffca28') + rect(28, 104, 22, 48, 6, '#8d6e63')
    s += ''.join(path(f'M {206 + i * 10} {128 - 30 - i * 14} Q {226 + i * 14} 128 {206 + i * 10} {128 + 30 + i * 14}', 'none', stroke='#ff7043', stroke_width='9') for i in range(2))
    return s
def sk_smog_gun():
    s = rect(50, 110, 120, 50, 12, '#26a69a') + rect(70, 156, 34, 60, 8, '#00796b') + rect(166, 122, 40, 26, 6, '#80cbc4')
    s += ''.join(circle(210 + i * 12, 120 + (i % 2) * 18 - i * 6, 10 + i * 3, '#e0f7fa', stroke_width='4') for i in range(3))
    return s
def sk_duct_tape():
    s = circle(128, 128, 84, '#b0bec5') + circle(128, 128, 38, '#eceff1') + path('M 184 168 L 236 214 L 214 232 L 166 186 Z', '#b0bec5')
    s += path('M 60 100 Q 128 60 196 100', 'none', line=False, stroke='#90a4ae', stroke_width='6')
    return s
def sk_papa():
    s = rect(46, 64, 164, 120, 14, '#263238') + rect(58, 76, 140, 96, 6, '#ffd54f')
    s += text(128, 120, 'PAPA', 34, '#263238') + text(128, 158, 'calling...', 22, '#263238', family='Arial, sans-serif')
    s += path('M 104 196 L 152 196 L 128 226 Z', '#e53935')
    return s
def sk_mask():
    s = path('M 46 104 Q 128 72 210 104 L 200 170 Q 128 210 56 170 Z', '#eceff1')
    s += ''.join(line(70, 116 + i * 18, 186, 116 + i * 18, 4, '#b0bec5') for i in range(3))
    s += line(46, 104, 20, 90, 6, '#90a4ae') + line(210, 104, 236, 90, 6, '#90a4ae') + text(128, 196, 'N95', 26, '#1565c0')
    return s
def sk_shield():
    s = circle(128, 128, 90, '#cfd8dc') + circle(128, 128, 66, '#eceff1') + rect(116, 30, 24, 30, 6, '#424242')
    s += line(70, 90, 186, 166, 10, '#ffca28') + line(186, 90, 70, 166, 10, '#ffca28')                       # duct-taped cooker lid
    return s
def sk_wrong_side():
    s = g(scooty().replace("2 min", ""), sx=-0.8, sy=0.8, cy=110)                 # facing the wrong way
    s += path('M 216 206 L 90 206 L 90 186 L 40 222 L 90 256 L 90 236 L 216 236 Z', '#e53935', stroke_width='5')
    return s
def sk_forward():
    s = rect(36, 50, 184, 140, 26, '#43a047') + path('M 128 80 L 182 120 L 128 160 L 128 136 Q 84 136 66 168 Q 70 108 128 104 Z', '#ffffff', stroke_width='5')
    s += text(128, 230, 'x10', 30, '#43a047')
    return s
def sk_haggle():
    s = g(rect(40, 80, 176, 96, 10, '#a5d6a7') + circle(128, 128, 30, '#81c784') + text(128, 140, '500', 30, '#1b5e20'), rot=-10)
    s += path('M 150 196 L 210 140 M 150 140 L 210 196', 'none', stroke='#455a64', stroke_width='10') + circle(144, 204, 14, 'none', stroke='#455a64', stroke_width='8') + circle(144, 132, 14, 'none', stroke='#455a64', stroke_width='8')
    return s
skill_icons = {'pressure_horn': sk_horn, 'anti_smog_gun': sk_smog_gun, 'duct_tape': sk_duct_tape, 'papa_ka_naam': sk_papa,
               'kadak_chai': cutting_chai, 'n95_mask': sk_mask, 'jugaad_shield': sk_shield, 'wrong_side': sk_wrong_side,
               'forward_msg': sk_forward, 'haggling': sk_haggle}

# ------------------------------------------------------------------ the NCR map (1000x1000)
def ncr_map():
    W = H = 1000
    s = (f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">'
         '<defs><pattern id="dots" width="22" height="22" patternUnits="userSpaceOnUse"><circle cx="2" cy="2" r="2" fill="#d8c9a3"/></pattern></defs>')
    s += ('<defs><filter id="st" x="-12%" y="-12%" width="124%" height="124%">'
          '<feMorphology in="SourceAlpha" operator="dilate" radius="6" result="d"/>'
          '<feFlood flood-color="#ffffff"/><feComposite in2="d" operator="in" result="w"/>'
          '<feMerge><feMergeNode in="w"/><feMergeNode in="SourceGraphic"/></feMerge></filter></defs>')
    s += rect(10, 10, 980, 980, 40, '#f3e7c9', stroke_width='10') + rect(10, 10, 980, 980, 40, 'url(#dots)', line=False)
    # regions
    s += path('M 300 120 Q 520 70 660 160 Q 720 260 680 420 Q 600 500 470 480 Q 340 470 300 380 Q 250 250 300 120 Z', '#ffcc80', opacity='0.9')   # Delhi
    s += path('M 40 440 Q 200 420 320 420 Q 420 480 430 560 Q 420 720 340 880 Q 200 960 60 900 Z', '#c5e1a5', opacity='0.9')       # Haryana / GGN
    s += path('M 700 300 Q 860 320 960 400 L 960 900 Q 800 920 640 820 Q 560 700 600 560 Q 660 470 700 300 Z', '#b3e5fc', opacity='0.9')  # UP / Noida
    # Yamuna
    s += path('M 610 20 Q 700 200 650 330 Q 600 450 660 560 Q 720 680 640 980', 'none', line=False, stroke='#64b5f6', stroke_width='26', opacity='0.9')
    s += text(700, 110, 'Yamuna', 26, '#1e88e5', rot=62, family='Arial, sans-serif', weight='700')
    # route: dilli > sirhaul > gurugram > dnd > noida > border > daftar
    pts = [(470, 330), (340, 525), (230, 700), (620, 600), (790, 610), (700, 380), (560, 175)]
    d = 'M ' + ' L '.join(f'{x} {y}' for x, y in pts)
    s += path(d, 'none', line=False, stroke='#ffffff', stroke_width='22', stroke_linejoin='round', stroke_linecap='round')
    s += path(d, 'none', line=False, stroke='#e8590c', stroke_width='10', stroke_dasharray='4 22', stroke_linejoin='round', stroke_linecap='round')
    # toll booths
    for (x, y), name, dy in [((340, 525), 'Sirhaul Toll', 66), ((620, 600), 'DND Flyway', 70), ((700, 380), 'Delhi Border', -62)]:
        s += rect(x - 34, y - 30, 68, 44, 6, '#fff8e1') + rect(x - 40, y - 42, 80, 16, 4, '#e53935')
        s += line(x + 34, y + 6, x + 90, y + 6, 8, OUT) + line(x + 34, y + 6, x + 90, y + 6, 4, '#ffffff')
        s += text(x, y + dy, name, 26, '#b71c1c', family='Arial, sans-serif', weight='900')
    # cities
    def city(x, y, name, color, label_dy=70):
        return circle(x, y, 30, color, stroke_width='8') + circle(x, y, 11, '#ffffff', line=False) + \
               text(x, y + label_dy, name, 34, OUT, family='Arial Black, Arial, sans-serif')
    s += city(470, 330, 'Republic of Dilli', '#ef6c00', 72)
    s += city(230, 700, 'Gurugram', '#558b2f', 74) + text(230, 812, 'Corporate Emirates', 26, '#33691e', family='Arial, sans-serif', weight='700')
    s += city(790, 610, 'Noida', '#0277bd', 74) + text(790, 722, 'Free Trade Zone', 26, '#01579b', family='Arial, sans-serif', weight='700')
    # the Daftar: a little government building
    x, y = 560, 175
    s += rect(x - 60, y - 40, 120, 70, 4, '#eeeeee') + path(f'M {x-70} {y-40} L {x} {y-80} L {x+70} {y-40} Z', '#bdbdbd')
    s += ''.join(rect(x - 48 + i * 26, y - 30, 12, 60, 2, '#ffffff', stroke_width='4') for i in range(4))
    s += text(x + 4, y + 66, 'Sarkari Daftar', 30, OUT)
    # flavour
    s += text(200, 240, 'smog', 30, '#a1887f', rot=-8, family='Arial, sans-serif', weight='700') + text(860, 220, 'more smog', 26, '#a1887f', rot=8, family='Arial, sans-serif', weight='700')
    s += text(130, 470, 'waterlogged', 24, '#1e88e5', rot=-12, family='Arial, sans-serif', weight='700')
    s += text(860, 860, 'Sector 62 (also 63?)', 24, '#01579b', rot=-6, family='Arial, sans-serif', weight='700')
    s += rect(36, 920, 470, 54, 12, '#fff8e1', stroke_width='5') + text(271, 957, 'NCR. Not to scale. Nothing is.', 26, OUT, family='Arial, sans-serif', weight='900')
    for k, (x, y) in {'dilli': (355, 250), 'gurugram': (105, 650), 'noida': (885, 520), 'daftar': (410, 95)}.items():
        s += f'<g filter="url(#st)" transform="translate({x - 64} {y - 64}) scale(0.5)">{zones[k]()}</g>'
    # compass
    s += circle(900, 100, 44, '#fff8e1', stroke_width='5') + path('M 900 66 L 912 100 L 900 134 L 888 100 Z', '#e53935', stroke_width='4') + text(900, 60, 'N', 26, OUT)
    return s + '</svg>'

items = {'chappal': chappal, 'cork_ball': cork_ball, 'badminton_racket': badminton_racket, 'cricket_bat': cricket_bat,
         'hockey_stick': hockey_stick, 'steel_tiffin': steel_tiffin, 'fastag': fastag, 'cutting_chai': cutting_chai,
         'laathi': laathi, 'bubble_wrap': bubble_wrap}
shots = {'south_delhi_girl': iced_coffee, 'firecracker': rocket, 'sector_cabbie': honk, 'ceo': pitch_deck, 'babu': rubber_stamp, 'techbro': cold_email}

out = sys.argv[1]
os.makedirs(out, exist_ok=True)
for k, fn in items.items():
    open(os.path.join(out, f'item_{k}.svg'), 'w').write(sticker(fn(), glow='#ffd54f' if k == 'laathi' else None))
open(os.path.join(out, 'mystery_box.svg'), 'w').write(sticker(mystery_box()))
for k, fn in shots.items():
    open(os.path.join(out, f'shot_{k}.svg'), 'w').write(sticker(fn()))
for k, fn in zones.items():
    open(os.path.join(out, f'zone_{k}.svg'), 'w').write(sticker(fn()))
open(os.path.join(out, 'gear.svg'), 'w').write(sticker(gear()))
open(os.path.join(out, 'chili.svg'), 'w').write(sticker(chili()))
open(os.path.join(out, 'scooty.svg'), 'w').write(sticker(scooty()))
open(os.path.join(out, 'khatra.svg'), 'w').write(sticker(khatra()))
open(os.path.join(out, 'star.svg'), 'w').write(sticker(star()))
open(os.path.join(out, 'item_pudin_hara.svg'), 'w').write(sticker(pudin_hara()))
os.makedirs(os.path.join(out, 'overlays'), exist_ok=True)   # 512x640, drawn over the hero sprite
open(os.path.join(out, 'overlays', 'cond_shaved.svg'), 'w').write(cond_overlay_svg(cond_shaved()))
open(os.path.join(out, 'overlays', 'cond_confused.svg'), 'w').write(cond_overlay_svg(cond_confused()))
for k, fn in skill_icons.items():
    open(os.path.join(out, f'skill_{k}.svg'), 'w').write(sticker(fn()))
for k, fn in phone_apps.items():
    open(os.path.join(out, f'app_{k}.svg'), 'w').write(sticker(fn()))
open(os.path.join(out, 'map.svg'), 'w').write(ncr_map())
print(len(items) + 1 + len(shots), 'icons + map')
