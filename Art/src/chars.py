"""Sticker-style chibi characters for GRAP Stage 4. Everyone faces RIGHT (enemies are flipped in-game).
Canvas 512x640. Outputs SVG files; render.sh turns them into transparent PNGs."""
import os, sys

OUT = '#1d1b22'
SW = 8
HURT = False  # when True, faces render in the pained 'hit' pose (> < eyes, open mouth)
W, H = 512, 640

def attrs(line, k):
    d = {'stroke': OUT, 'stroke-width': SW, 'stroke-linejoin': 'round', 'stroke-linecap': 'round'} if line else {}
    for x, v in k.items(): d[x.replace('_', '-')] = v
    return ' '.join(f'{x}="{v}"' for x, v in d.items())
def circle(cx, cy, r, fill, line=True, **k): return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {attrs(line, k)}/>'
def ellipse(cx, cy, rx, ry, fill, line=True, **k): return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" {attrs(line, k)}/>'
def rect(x, y, w, h, rx, fill, line=True, **k): return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" {attrs(line, k)}/>'
def path(d, fill, line=True, **k): return f'<path d="{d}" fill="{fill}" {attrs(line, k)}/>'
def line(x1, y1, x2, y2, w=SW, color=OUT): return f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{w}" stroke-linecap="round"/>'
def capsule(x1, y1, x2, y2, w, color):
    return line(x1, y1, x2, y2, w + 2 * SW, OUT) + line(x1, y1, x2, y2, w, color)
def text(x, y, s, size, fill, weight='900', anchor='middle', rot=0, mirror=False):
    t = (f'<text x="{x}" y="{y}" font-family="Arial Black, Arial, Helvetica, sans-serif" font-weight="{weight}" '
         f'font-size="{size}" fill="{fill}" text-anchor="{anchor}" transform="rotate({rot} {x} {y})">{s}</text>')
    # Enemies are flipped in-game to face left, so their text is pre-mirrored here to read correctly on screen.
    return f'<g transform="translate({2 * x} 0) scale(-1 1)">{t}</g>' if mirror else t

# ---------------------------------------------------------------- body parts
def legs(pants, shoes, spread=34, top=520, bottom=596):
    l, r = 262 - spread, 262 + spread
    return (capsule(l, top, l, bottom, 46, pants) + capsule(r, top, r, bottom, 46, pants) +
            ellipse(l + 10, bottom + 12, 36, 19, shoes) + ellipse(r + 10, bottom + 12, 36, 19, shoes))

def torso(color, belly=False):
    if belly:
        return ellipse(262, 470, 118, 108, color)
    return rect(168, 362, 190, 196, 62, color)

def head(skin, cy=232, r=138, ear=True):
    s = ''
    if ear: s += circle(136, cy + 14, 28, skin)
    s += circle(262, cy, r, skin)
    return s

def eye(x, y, look=6, kind='open', lid=0):
    if HURT:  # squeezed shut: '>' on the left eye, '<' on the right
        d = 1 if x < 309 else -1
        return path(f'M {x-14*d} {y-15} L {x+12*d} {y} L {x-14*d} {y+15}', 'none', stroke_width='10')
    if kind == 'closed':
        return path(f'M {x-18} {y} Q {x} {y+14} {x+18} {y}', 'none')
    s = ellipse(x, y, 20, 25, '#ffffff')
    s += circle(x + look, y + 3, 12, OUT, line=False) + circle(x + look + 4, y - 3, 4, '#ffffff', line=False)
    if lid:  # half-lidded (bored / smug / sleepy)
        s += path(f'M {x-24} {y-27} L {x+24} {y-27} L {x+24} {y-27+lid} Q {x} {y-27+lid+6} {x-24} {y-27+lid} Z', 'currentColor', line=False)
        s += line(x - 24, y - 27 + lid, x + 24, y - 27 + lid, 6)
    return s

def eyes(skin, kind='open', look=6, lid=0, y=242, x1=274, x2=344):
    return f'<g color="{skin}">' + eye(x1, y, look, kind, lid) + eye(x2, y, look, kind, lid) + '</g>'

def brows(kind='flat', y=200, x1=274, x2=344, color=OUT, w=10):
    if HURT: kind = 'worried'
    if kind == 'worried':  return line(x1 - 18, y + 4, x1 + 16, y - 6, w, color) + line(x2 - 16, y - 6, x2 + 18, y + 4, w, color)
    if kind == 'angry':    return line(x1 - 18, y - 6, x1 + 16, y + 6, w, color) + line(x2 - 16, y + 6, x2 + 18, y - 6, w, color)
    if kind == 'smug':     return line(x1 - 18, y, x1 + 16, y - 2, w, color) + line(x2 - 16, y - 14, x2 + 18, y - 8, w, color)
    if kind == 'raised':   return line(x1 - 18, y - 10, x1 + 16, y - 14, w, color) + line(x2 - 16, y - 14, x2 + 18, y - 10, w, color)
    return line(x1 - 18, y, x1 + 16, y, w, color) + line(x2 - 16, y, x2 + 18, y, w, color)

def blush(): return ellipse(250, 290, 22, 12, '#ff8a80', line=False, opacity='0.45') + ellipse(372, 290, 18, 11, '#ff8a80', line=False, opacity='0.45')

def mouth(kind='smile', x=318, y=306):
    if HURT:
        return path(f'M {x-30} {y-4} Q {x-15} {y-14} {x} {y-4} Q {x+15} {y-14} {x+30} {y-4} Q {x+22} {y+36} {x} {y+36} Q {x-22} {y+36} {x-30} {y-4} Z', '#7b2d26')
    if kind == 'smile':   return path(f'M {x-26} {y} Q {x} {y+24} {x+26} {y}', 'none')
    if kind == 'grin':    return path(f'M {x-34} {y-6} Q {x} {y+40} {x+34} {y-6} Z', '#ffffff') + line(x - 30, y + 2, x + 30, y + 2, 4)
    if kind == 'open':    return path(f'M {x-26} {y-8} Q {x} {y+44} {x+26} {y-8} Z', '#7b2d26') + ellipse(x, y + 16, 12, 7, '#ff8a80', line=False)
    if kind == 'wavy':    return path(f'M {x-24} {y+4} q 8 -10 16 0 q 8 10 16 0 q 8 -10 16 0', 'none')
    if kind == 'smirk':   return path(f'M {x-20} {y+4} Q {x+6} {y+14} {x+28} {y-8}', 'none')
    if kind == 'frown':   return path(f'M {x-22} {y+10} Q {x} {y-8} {x+22} {y+10}', 'none')
    if kind == 'flat':    return line(x - 20, y + 4, x + 20, y + 4)
    return ''

def hand(x, y, skin, r=22): return circle(x, y, r, skin)

def mustache(x=318, y=284, color='#2b2024', big=False):
    s = 1.3 if big else 1
    return path(f'M {x} {y} Q {x-30*s} {y-8} {x-48*s} {y+16} Q {x-22*s} {y+8} {x} {y+10} Q {x+22*s} {y+8} {x+48*s} {y+16} Q {x+30*s} {y-8} {x} {y} Z', color)

def sticker(inner, shadow=True):
    sh = ellipse(262, 618, 150, 16, '#000000', line=False, opacity='0.18') if shadow else ''
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">'
            '<defs><filter id="st" x="-10%" y="-10%" width="120%" height="120%">'
            '<feMorphology in="SourceAlpha" operator="dilate" radius="9" result="d"/>'
            '<feFlood flood-color="#ffffff"/><feComposite in2="d" operator="in" result="w"/>'
            '<feGaussianBlur in="d" stdDeviation="6" result="b"/><feOffset in="b" dx="0" dy="6" result="o"/>'
            '<feFlood flood-color="#000000" flood-opacity="0.25"/><feComposite in2="o" operator="in" result="s"/>'
            '<feMerge><feMergeNode in="s"/><feMergeNode in="w"/><feMergeNode in="SourceGraphic"/></feMerge></filter></defs>'
            f'{sh}<g filter="url(#st)">{inner}</g></svg>')

# ---------------------------------------------------------------- characters
C = {}

def new_kid():
    skin = '#D9A066'
    s = legs('#3A5A99', '#f5f5f5')
    s += path('M 150 372 Q 262 330 374 372 L 360 400 Q 262 372 164 400 Z', '#1f8c80')        # hood behind
    s += torso('#2BB3A3')
    s += rect(196, 372, 22, 170, 8, '#5d4037') + rect(306, 372, 22, 170, 8, '#5d4037')         # backpack straps
    s += rect(228, 470, 70, 40, 14, '#24a093')                                                  # hoodie pocket
    s += capsule(178, 400, 150, 488, 44, '#2BB3A3') + hand(148, 496, skin)                     # back arm
    s += head(skin)
    s += path('M 126 236 Q 112 92 262 86 Q 406 84 404 214 L 384 168 L 362 206 L 340 156 L 312 200 L 286 148 L 256 196 L 226 150 L 200 204 L 170 164 L 150 236 Z', '#2b2024')
    s += eyes(skin) + brows('worried') + mouth('wavy') + blush()
    s += path('M 404 150 Q 418 176 404 190 Q 390 176 404 150 Z', '#7fd3ff')                   # sweat drop
    s += capsule(350, 400, 394, 470, 44, '#2BB3A3')
    s += f'<g transform="rotate(-18 420 452)">' + rect(370, 432, 110, 40, 12, '#fff8e1') + rect(418, 428, 14, 48, 4, '#e53935') + '</g>'
    s += hand(398, 474, skin)
    return s

def pinky_aunty():
    skin = '#C68B59'
    s = legs('#fce4ec', '#c2185b', spread=30)
    s += torso('#F06292')
    s += path('M 172 372 Q 230 460 352 540 L 352 500 Q 250 430 210 364 Z', '#FFD54F')           # dupatta
    s += ''.join(circle(x, y, 6, '#ad1457', line=False) for x, y in [(220, 420), (300, 470), (250, 520), (330, 420)])
    s += capsule(178, 400, 150, 480, 42, '#F06292') + hand(148, 488, skin)
    s += circle(200, 104, 56, '#3e2723')                                                       # bun
    s += head(skin)
    s += path('M 124 250 Q 118 96 262 94 Q 400 96 400 230 Q 376 150 300 140 Q 230 136 180 170 Q 140 200 150 260 Z', '#3e2723')
    s += path('M 262 96 Q 250 120 256 140', 'none', stroke_width='5')
    s += circle(136, 300, 15, '#FFC107')                                                       # earring
    s += circle(310, 186, 8, '#e53935', line=False)                                            # bindi
    s += eyes(skin, lid=14) + brows('raised') + mouth('smirk') + blush()
    s += capsule(350, 400, 386, 440, 42, '#F06292')
    s += rect(372, 360, 74, 124, 14, '#263238') + rect(382, 372, 54, 98, 6, '#25D366', line=False)
    s += rect(388, 380, 34, 14, 6, '#ffffff', line=False) + rect(398, 402, 32, 14, 6, '#dcf8c6', line=False) + rect(388, 424, 38, 14, 6, '#ffffff', line=False)
    s += hand(392, 460, skin)
    return s

def scooty_uncle():
    skin = '#A86B3C'
    s = legs('#f5f5f5', '#6d4c41')
    s += torso('#FAFAFA', belly=True)
    s += line(262, 378, 262, 450, 5) + ''.join(circle(262, y, 5, OUT, line=False) for y in (400, 425))
    s += capsule(170, 410, 150, 500, 44, '#FAFAFA') + hand(150, 508, skin)
    s += path('M 96 470 A 62 62 0 0 1 220 470 L 220 486 L 96 486 Z', '#e53935') + rect(150, 452, 66, 22, 8, '#90caf9')  # helmet on elbow
    s += head(skin)
    s += path('M 128 260 Q 122 196 160 170 L 168 214 Q 150 230 150 262 Z', '#5d4037') + path('M 378 196 Q 400 220 398 250 L 384 246 Z', '#5d4037')
    s += path('M 236 228 Q 274 214 300 228 Q 300 262 266 262 Q 236 260 236 228 Z', '#212121') + path('M 312 228 Q 346 214 376 228 Q 376 262 344 262 Q 312 260 312 228 Z', '#212121')
    s += line(300, 230, 312, 230, 6) + line(236, 230, 150, 236, 6)
    s += brows('flat', y=206) + mustache(318, 288, big=True) + mouth('smile', y=314)
    s += capsule(354, 410, 398, 470, 44, '#FAFAFA') + hand(400, 476, skin)
    s += circle(420, 500, 14, 'none', stroke_width='6') + rect(424, 506, 10, 30, 3, '#ffca28')
    return s

def auto_bhaiya():
    skin = '#8D5524'
    s = legs('#a1887f', '#3e2723')
    s += torso('#C8B273')
    s += rect(296, 400, 44, 40, 6, '#b39d5f') + line(306, 392, 306, 420, 6, '#1565c0') + line(318, 392, 318, 418, 6, '#e53935')
    s += path('M 168 372 Q 200 360 232 380 L 220 520 Q 196 520 178 506 Z', '#f5f5f5')           # gamcha
    s += ''.join(line(180, y, 228, y + 6, 7, '#e53935') for y in (400, 430, 460, 490))
    s += capsule(174, 410, 154, 494, 44, '#C8B273') + hand(154, 502, skin)
    s += head(skin)
    s += path('M 126 240 Q 120 100 262 94 Q 396 92 402 200 Q 340 132 250 150 Q 190 160 150 240 Z', '#1b1b1b')
    s += eyes(skin, kind='closed', y=246) + brows('smug') + mustache(318, 286) + mouth('smirk', y=312)
    s += capsule(352, 400, 400, 320, 44, '#C8B273') + hand(404, 312, skin) + capsule(404, 292, 404, 256, 14, skin)  # finger wag
    s += text(444, 236, 'NO!', 34, '#e53935', rot=-12, mirror=True)
    return s

def south_delhi_girl():
    skin = '#E0AC69'
    s = path('M 128 230 Q 110 420 150 470 L 380 470 Q 414 400 398 230 Z', '#4a2c20')            # long hair behind
    s += legs('#bbdefb', '#ffffff', spread=28)
    s += torso('#ffffff')
    s += path('M 168 380 Q 168 362 200 362 L 236 362 L 250 470 L 168 470 Z', '#7FA7D9') + path('M 358 380 Q 358 362 326 362 L 290 362 L 276 470 L 358 470 Z', '#7FA7D9')
    s += capsule(176, 400, 156, 486, 40, '#7FA7D9') + hand(156, 494, skin)
    s += path('M 104 420 L 190 400 L 200 520 L 112 534 Z', '#f8bbd0') + text(152, 478, 'HKV', 24, '#ad1457', rot=8, mirror=True)   # tote
    s += head(skin)
    s += path('M 126 250 Q 118 90 262 90 Q 404 92 400 250 Q 380 160 320 140 Q 250 150 200 150 Q 150 170 140 260 Z', '#4a2c20')
    s += path('M 200 130 Q 240 112 280 126', 'none', stroke='#8a5a3c', stroke_width='10')
    s += rect(196, 96, 80, 34, 16, '#212121') + rect(288, 96, 80, 34, 16, '#212121') + line(276, 110, 288, 110, 6)  # shades on head
    s += eyes(skin, lid=20, look=8) + line(358, 216, 372, 206, 5) + line(290, 216, 282, 204, 5)
    s += brows('raised', y=196, w=7) + path('M 300 306 Q 322 316 342 300', '#f06292')
    s += circle(140, 306, 18, 'none', stroke='#FFC107', stroke_width='6')
    s += capsule(350, 400, 392, 452, 40, '#7FA7D9')
    s += path('M 370 380 L 430 380 L 420 490 L 380 490 Z', '#e3f2fd') + path('M 376 420 L 424 420 L 418 488 L 382 488 Z', '#8d6e63', line=False)
    s += rect(364, 368, 72, 18, 8, '#ffffff') + line(408, 370, 424, 316, 9, '#43a047')
    s += hand(392, 456, skin)
    return s

def broker():
    skin = '#C68B59'
    s = legs('#37474f', '#212121')
    s += torso('#8E2C3B', belly=True)
    s += path('M 226 370 L 262 420 L 298 370 Z', skin)
    s += path('M 214 374 Q 262 470 310 374', 'none', stroke='#FFC107', stroke_width='14') + path('M 214 374 Q 262 470 310 374', 'none', stroke=OUT, stroke_width='3')
    s += rect(300, 440, 26, 8, 3, '#1565c0', line=False)
    s += capsule(176, 404, 150, 318, 44, '#8E2C3B') + hand(150, 300, skin)                     # arm up to ear
    s += head(skin)
    s += rect(112, 230, 40, 76, 10, '#212121')                                                  # phone at ear
    s += path('M 124 230 Q 120 96 262 92 Q 400 94 404 210 Q 360 150 290 136 Q 210 128 150 190 Z', '#1b1b1b') + path('M 220 112 Q 270 100 320 116', 'none', stroke='#9e9e9e', stroke_width='8')
    s += eyes(skin, lid=10, look=9) + brows('smug') + mouth('grin', y=304)
    s += capsule(356, 410, 400, 470, 44, '#8E2C3B') + rect(392, 430, 46, 78, 10, '#212121') + rect(398, 438, 34, 56, 4, '#64b5f6', line=False) + hand(404, 486, skin)
    return s

def monkey():
    fur, face = '#8D6E63', '#E6C3A5'
    s = path('M 150 520 Q 60 520 80 430 Q 96 380 130 400', 'none', stroke=OUT, stroke_width='34') + path('M 150 520 Q 60 520 80 430 Q 96 380 130 400', 'none', stroke=fur, stroke_width='18')
    s += legs(fur, '#6d4c41', spread=40)
    s += torso(fur)
    s += ellipse(262, 470, 66, 70, face, line=False)
    s += rect(232, 410, 70, 86, 8, '#ffffff') + rect(240, 420, 54, 24, 4, '#1565c0', line=False) + line(220, 372, 250, 410, 6, '#1565c0') + line(304, 372, 284, 410, 6, '#1565c0')
    s += capsule(178, 400, 150, 486, 40, fur) + hand(150, 494, face)
    s += circle(130, 240, 44, fur) + circle(130, 240, 22, face, line=False) + circle(394, 214, 40, fur) + circle(394, 214, 20, face, line=False)
    s += circle(262, 232, 136, fur, False) + head(fur, ear=False)
    s += path('M 200 210 Q 230 160 290 196 Q 350 160 380 210 Q 400 300 300 336 Q 200 320 200 210 Z', face)
    s += rect(204, 120, 82, 36, 16, '#212121') + rect(296, 120, 82, 36, 16, '#212121') + line(286, 136, 296, 136, 6)
    s += eyes(face, y=240, x1=268, x2=336) + brows('raised', y=200, x1=268, x2=336) + mouth('grin', y=294, x=306)
    s += capsule(350, 400, 396, 456, 40, fur) + hand(398, 462, face)
    return s

def firecracker():
    skin = '#A86B3C'
    s = legs('#fff3e0', '#5d4037')
    s += torso('#FFC107')
    s += ''.join(path(f'M {x} {y-12} L {x+4} {y-4} L {x+12} {y} L {x+4} {y+4} L {x} {y+12} L {x-4} {y+4} L {x-12} {y} L {x-4} {y-4} Z', '#fff59d', line=False) for x, y in [(210, 420), (300, 460), (240, 510), (320, 400)])
    s += line(262, 372, 262, 450, 5, '#e65100')
    s += capsule(176, 400, 140, 330, 44, '#FFC107') + hand(138, 320, skin)
    s += f'<g transform="rotate(-20 130 260)">' + line(130, 250, 130, 380, 8, '#8d6e63') + rect(108, 160, 44, 110, 10, '#e53935') + path('M 108 164 L 130 116 L 152 164 Z', '#ffeb3b') + '</g>'
    s += head(skin)
    s += path('M 124 250 Q 104 110 200 96 Q 230 70 290 84 Q 380 80 404 180 Q 410 220 396 240 Q 380 160 320 150 Q 220 150 160 200 Q 140 220 146 260 Z', '#1b1b1b')
    s += eyes(skin, look=4) + brows('raised') + mouth('open') + blush()
    s += capsule(352, 400, 398, 450, 44, '#FFC107') + hand(400, 456, skin)
    s += line(404, 450, 444, 380, 6, '#9e9e9e')
    s += ''.join(line(444, 380, 444 + dx, 380 + dy, 5, '#ffeb3b') for dx, dy in [(30, 0), (-30, 0), (0, 30), (0, -30), (22, 22), (-22, -22), (22, -22), (-22, 22)])
    return s

def guard():
    skin = '#8D5524'
    s = legs('#263238', '#111111')
    s += torso('#34495E')
    s += rect(168, 372, 50, 16, 6, '#c9a227') + rect(308, 372, 50, 16, 6, '#c9a227')
    s += path('M 230 380 Q 262 450 296 380', 'none', stroke='#bdbdbd', stroke_width='5') + rect(282, 440, 32, 16, 6, '#cfd8dc')
    s += rect(112, 420, 80, 104, 8, '#8e2c3b') + line(124, 440, 180, 440, 4, '#f5f5f5')                    # register
    s += capsule(178, 400, 166, 484, 44, '#34495E') + hand(166, 492, skin)
    s += head(skin)
    s += path('M 124 180 Q 130 100 262 96 Q 394 100 400 180 Z', '#2c3e50') + path('M 280 176 Q 380 170 430 196 Q 380 204 280 196 Z', '#111111') + circle(262, 140, 16, '#c9a227')
    s += eyes(skin, lid=18, look=10) + brows('angry', y=212) + mustache(318, 288) + mouth('flat', y=312)
    s += capsule(352, 400, 398, 460, 44, '#34495E') + hand(400, 466, skin)
    s += line(370, 560, 450, 330, 18, OUT) + line(370, 560, 450, 330, 10, '#8d6e63')
    return s

def ceo():
    skin = '#E0AC69'
    s = legs('#455a64', '#ffffff')
    s += torso('#64B5F6')
    s += path('M 168 400 Q 168 362 210 362 L 240 362 L 240 556 L 200 556 Q 168 556 168 520 Z', '#9E9E9E') + path('M 358 400 Q 358 362 316 362 L 286 362 L 286 556 L 326 556 Q 358 556 358 520 Z', '#9E9E9E')
    s += line(262, 380, 262, 556, 4, '#e0e0e0')
    s += f'<g transform="rotate(-12 150 470)">' + rect(96, 420, 120, 84, 10, '#cfd8dc') + circle(156, 462, 14, '#ff7043', line=False) + '</g>'   # laptop under arm
    s += capsule(178, 400, 168, 484, 42, '#9E9E9E') + hand(168, 492, skin)
    s += head(skin)
    s += path('M 126 230 Q 116 110 220 90 Q 300 60 380 110 Q 420 150 400 200 Q 360 130 300 140 Q 240 150 200 140 Q 150 160 150 236 Z', '#8d5a3b')
    s += rect(130, 252, 14, 26, 6, '#ffffff')                                                               # AirPod
    s += eyes(skin, lid=8) + brows('smug') + mouth('smirk')
    s += capsule(352, 400, 396, 456, 42, '#64B5F6') + rect(378, 400, 52, 76, 10, '#ffffff') + rect(378, 424, 52, 26, 0, '#8d6e63') + hand(400, 462, skin)
    return s

def babu():
    skin = '#C68B59'
    s = legs('#5d4037', '#3e2723')
    s += torso('#FFF8E1')
    s += path('M 168 400 Q 168 362 200 362 L 262 450 L 324 362 Q 358 362 358 400 L 358 556 L 168 556 Z', '#8D6E63')
    s += ''.join(line(180, y, 346, y, 3, '#6d4c41') for y in (480, 510, 540))
    s += rect(96, 420, 110, 30, 4, '#d7ccc8') + rect(96, 446, 110, 30, 4, '#bcaaa4') + rect(96, 472, 110, 30, 4, '#d7ccc8') + line(150, 416, 150, 504, 6, '#e53935')  # files
    s += capsule(178, 400, 168, 470, 42, '#FFF8E1') + hand(168, 478, skin)
    s += head(skin)
    s += path('M 126 270 Q 118 200 146 168 L 160 214 Q 146 236 150 272 Z', '#bdbdbd') + path('M 380 186 Q 402 220 400 258 L 386 252 Z', '#bdbdbd')
    s += line(146, 176, 192, 120, 9, '#1565c0')                                                             # pen behind ear
    s += eyes(skin, lid=22, y=250) + circle(274, 254, 30, 'none') + circle(344, 254, 30, 'none') + line(304, 254, 314, 254, 6)
    s += brows('flat', y=208, color='#9e9e9e') + mouth('frown', y=314)
    s += capsule(352, 400, 392, 340, 42, '#FFF8E1') + hand(396, 330, skin)
    s += rect(372, 236, 48, 86, 14, '#c62828') + rect(352, 220, 88, 26, 8, '#3e2723') + rect(348, 196, 96, 24, 6, '#212121')
    return s

def sector_cabbie():
    skin = '#A86B3C'
    s = legs('#424242', '#212121')
    s += torso('#ECEFF1')
    s += capsule(178, 400, 164, 484, 44, '#ECEFF1') + hand(164, 492, skin)
    s += head(skin)
    s += path('M 124 196 Q 130 110 262 104 Q 394 110 400 196 Z', '#455a64') + path('M 300 190 Q 380 186 422 204 Q 380 214 300 206 Z', '#263238')
    s += eye(274, 242, 2) + eye(344, 242, 10).replace('r="12"', 'r="7"')
    s += brows('worried') + mouth('wavy') + mustache(318, 286)
    s += text(150, 120, '?', 64, '#ff9800', rot=14, mirror=True) + text(420, 110, '?', 52, '#ff9800', rot=-12, mirror=True) + text(450, 190, '?', 40, '#ff9800', mirror=True)
    s += capsule(352, 400, 396, 440, 44, '#ECEFF1') + rect(378, 380, 62, 104, 12, '#263238') + rect(386, 390, 46, 80, 4, '#a5d6a7', line=False)
    s += path('M 409 404 Q 422 404 422 418 Q 422 430 409 446 Q 396 430 396 418 Q 396 404 409 404 Z', '#e53935', line=False) + hand(400, 466, skin)
    return s

def techbro():
    skin = '#E8B98A'
    s = legs('#c8b68e', '#ffffff')
    s += torso('#90CAF9')
    s += ''.join(line(176, y, 350, y, 3, '#64b5f6') for y in range(388, 556, 24)) + ''.join(line(x, 370, x, 552, 3, '#64b5f6') for x in range(192, 346, 24))
    s += path('M 168 400 Q 168 362 204 362 L 238 362 L 238 556 L 200 556 Q 168 556 168 520 Z', '#37474f') + path('M 358 400 Q 358 362 322 362 L 288 362 L 288 556 L 326 556 Q 358 556 358 520 Z', '#37474f')
    s += line(214, 370, 262, 452, 5, '#1565c0') + line(312, 370, 262, 452, 5, '#1565c0') + rect(242, 448, 42, 54, 6, '#ffffff') + rect(247, 455, 32, 14, 2, '#1565c0', line=False)
    s += capsule(178, 400, 166, 476, 42, '#90CAF9') + hand(166, 484, skin)
    s += rect(132, 430, 46, 62, 8, '#ffffff') + rect(128, 422, 54, 14, 6, '#5d4037') + circle(155, 462, 9, '#00704a', line=False)   # cold brew
    s += head(skin)
    s += path('M 124 236 Q 112 120 210 98 Q 300 70 372 110 Q 420 140 404 196 Q 380 160 330 150 Q 300 120 250 130 Q 190 140 160 180 Q 146 200 148 236 Z', '#4e342e')
    s += path('M 210 98 Q 280 40 360 80 Q 330 96 300 100 Z', '#4e342e')                                    # quiff
    s += rect(176, 120, 150, 30, 14, '#212121') + rect(200, 124, 40, 20, 8, '#455a64', line=False) + rect(262, 124, 40, 20, 8, '#455a64', line=False)  # shades on head
    s += rect(130, 252, 14, 26, 6, '#ffffff')                                                               # AirPod
    s += eyes(skin, lid=6) + brows('raised') + mouth('grin')
    s += capsule(352, 400, 392, 330, 42, '#90CAF9') + rect(366, 232, 64, 104, 12, '#263238') + rect(372, 240, 52, 86, 4, '#0a66c2', line=False)
    s += text(398, 296, 'in', 34, '#ffffff', mirror=True) + hand(394, 334, skin)
    return s

SWEAT = (path('M 410 150 Q 426 178 410 194 Q 394 178 410 150 Z', '#7fd3ff') +
         path('M 120 170 Q 132 192 120 204 Q 108 192 120 170 Z', '#7fd3ff') +
         line(420, 250, 450, 240, 6) + line(424, 280, 456, 284, 6))  # pain marks

for fn in [new_kid, pinky_aunty, scooty_uncle, auto_bhaiya, south_delhi_girl, broker, monkey, firecracker, guard, ceo, babu, sector_cabbie, techbro]:
    C[fn.__name__] = fn()
    HURT = True
    C[fn.__name__ + '_hit'] = fn() + SWEAT
    HURT = False

out = sys.argv[1]
os.makedirs(out, exist_ok=True)
for name, inner in C.items():
    open(os.path.join(out, name + '.svg'), 'w').write(sticker(inner))
print(len(C), 'characters')
