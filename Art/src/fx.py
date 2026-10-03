"""Generic overlays that line up with every character (they all share the same 512x640 chibi template):
dmg1 / dmg2 = progressive battle damage, burst = comic impact starburst for damage numbers."""
import math, os, sys

OUT = '#1d1b22'

def attrs(line, k, sw=6):
    d = {'stroke': OUT, 'stroke-width': sw, 'stroke-linejoin': 'round', 'stroke-linecap': 'round'} if line else {}
    for x, v in k.items(): d[x.replace('_', '-')] = v
    return ' '.join(f'{x}="{v}"' for x, v in d.items())
def path(d, fill, line=True, **k): return f'<path d="{d}" fill="{fill}" {attrs(line, k)}/>'
def ellipse(cx, cy, rx, ry, fill, line=True, **k): return f'<ellipse cx="{cx}" cy="{cy}" rx="{rx}" ry="{ry}" fill="{fill}" {attrs(line, k)}/>'
def circle(cx, cy, r, fill, line=True, **k): return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}" {attrs(line, k)}/>'
def rect(x, y, w, h, rx, fill, line=True, **k): return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" {attrs(line, k)}/>'
def line(x1, y1, x2, y2, w=5, color=OUT): return f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="{w}" stroke-linecap="round"/>'
def svg(inner, w=512, h=640): return f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{inner}</svg>'

def bandaid(cx, cy, rot, scale=1.0):
    w, h = 96 * scale, 34 * scale
    return (f'<g transform="rotate({rot} {cx} {cy})">' + rect(cx - w / 2, cy - h / 2, w, h, h / 2, '#f4c79a') +
            rect(cx - w * 0.17, cy - h / 2 + 3, w * 0.34, h - 6, 4, '#e8b07e', line=False) +
            ''.join(circle(cx + dx, cy + dy, 2.5, '#c98e5c', line=False) for dx in (-w * 0.34, -w * 0.27, w * 0.27, w * 0.34) for dy in (-5, 5)) + '</g>')

def bruise(cx, cy, rx, ry, a=0.55):
    return ellipse(cx, cy, rx, ry, '#6a3d9a', line=False, opacity=str(a)) + ellipse(cx + 4, cy + 2, rx * 0.55, ry * 0.55, '#4a2370', line=False, opacity=str(a))

def smudge(cx, cy, r, a=0.35):
    return ''.join(circle(cx + dx, cy + dy, r * k, '#5d4037', line=False, opacity=str(a)) for dx, dy, k in [(0, 0, 1), (r * 0.8, r * 0.3, 0.7), (-r * 0.7, r * 0.4, 0.6), (r * 0.2, -r * 0.6, 0.5)])

def scratch(x, y, n=3):
    return ''.join(line(x + i * 12, y + i * 4, x + 26 + i * 12, y - 18 + i * 4, 4, '#c62828') for i in range(n))

def dmg1():
    s = bandaid(352, 168, 22, 0.75)           # forehead, enemy side
    s += bruise(206, 176, 26, 18)        # forehead
    s += smudge(226, 470, 18) + smudge(304, 528, 14)
    s += scratch(176, 440)
    return svg(s)

def dmg2():
    s = bruise(206, 176, 30, 22, 0.65)
    s += ellipse(344, 242, 36, 38, 'none', line=False, stroke='#4a2370', stroke_width='14', opacity='0.6')   # black eye
    s += bandaid(352, 168, 22, 0.75) + bandaid(228, 160, 30, 0.85) + bandaid(228, 160, -30, 0.85)                 # crossed on forehead
    s += circle(306, 98, 30, '#ef9a9a') + circle(298, 90, 9, '#ffffff', line=False)                          # bump on the head
    s += smudge(226, 470, 22, 0.45) + smudge(306, 520, 18, 0.45) + smudge(258, 410, 12, 0.4)
    s += scratch(176, 440, 4) + scratch(300, 470, 3)
    s += path('M 238 548 L 252 520 L 262 546 L 276 516 L 290 548', 'none', stroke='#3e2723', stroke_width='6')  # torn hem
    return svg(s)

def burst():
    # Jagged comic starburst, white with a thick outline; tinted in-game.
    cx = cy = 128; pts = []
    for i in range(24):
        r = 118 if i % 2 == 0 else 78 + (i * 7 % 13)
        a = math.pi * 2 * i / 24 - math.pi / 2
        pts.append(f'{cx + r * math.cos(a):.1f},{cy + r * math.sin(a):.1f}')
    return svg(f'<polygon points="{" ".join(pts)}" fill="#ffffff" stroke="{OUT}" stroke-width="9" stroke-linejoin="round"/>', 256, 256)

out = sys.argv[1]
os.makedirs(out, exist_ok=True)
for name, fn in [('dmg1', dmg1), ('dmg2', dmg2), ('burst', burst)]:
    open(os.path.join(out, name + '.svg'), 'w').write(fn())
print('fx done')
