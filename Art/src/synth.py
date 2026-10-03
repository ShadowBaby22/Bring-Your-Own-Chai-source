"""Placeholder chiptune SFX + music for GRAP Stage 4. Pure Python (no numpy).
Usage: python3 synth.py <Assets/_Game/Resources/Audio>   -> writes SFX/*.wav, Music/*.wav
Replace any file with your own recording of the same name whenever you like."""
import math, os, random, struct, sys, wave

SR = 22050
random.seed(4)

def tone(freq, dur, wave_='square', vol=0.5, attack=0.005, decay=None, sweep_to=None, vibrato=0.0):
    n = int(SR * dur); out = []; ph = 0.0
    for i in range(n):
        t = i / SR
        f = freq if sweep_to is None else freq + (sweep_to - freq) * (i / n)
        if vibrato: f *= 1 + vibrato * math.sin(2 * math.pi * 6 * t)
        ph += f / SR
        x = ph % 1.0
        if wave_ == 'square': v = 1.0 if x < 0.5 else -1.0
        elif wave_ == 'saw': v = 2 * x - 1
        elif wave_ == 'tri': v = 4 * abs(x - 0.5) - 1
        else: v = math.sin(2 * math.pi * x)
        env = min(1.0, t / attack) if attack else 1.0
        if decay: env *= math.exp(-t / decay)
        else: env *= max(0.0, 1 - i / n) ** 0.3
        out.append(v * env * vol)
    return out

def noise(dur, vol=0.5, decay=0.05, lowpass=0.0):
    out, prev = [], 0.0
    for i in range(int(SR * dur)):
        v = random.uniform(-1, 1)
        prev = prev * lowpass + v * (1 - lowpass)
        out.append(prev * vol * math.exp(-(i / SR) / decay))
    return out

def mix(*tracks):
    n = max(len(t) for t in tracks)
    return [sum(t[i] for t in tracks if i < len(t)) for i in range(n)]

def seq(*parts):
    out = []
    for p in parts: out += p
    return out

def silence(dur): return [0.0] * int(SR * dur)

def save(path, samples, gain=0.9):
    peak = max(1e-6, max(abs(s) for s in samples))
    k = gain / peak if peak > gain else 1.0
    with wave.open(path, 'w') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(b''.join(struct.pack('<h', int(max(-1, min(1, s * k)) * 32767)) for s in samples))

def note(name):
    names = {'C': -9, 'Db': -8, 'D': -7, 'Eb': -6, 'E': -5, 'F': -4, 'Gb': -3, 'G': -2, 'Ab': -1, 'A': 0, 'Bb': 1, 'B': 2}
    return 440 * 2 ** ((names[name[:-1]] + 12 * (int(name[-1]) - 4)) / 12)

# ------------------------------------------------------------------ SFX
def punch(body=150, low=50, crack=0.9, color=0.15, length=0.16):
    """Layered cartoon punch: noise crack + bandlimited smack + pitched-down body thump."""
    return mix(noise(0.012, crack, decay=0.004),
               noise(length * 0.6, 0.55, decay=0.03, lowpass=color),
               tone(body, length, 'sine', 1.0, sweep_to=low, decay=0.045),
               tone(body * 2.1, 0.05, 'tri', 0.25, decay=0.015))

def sfx():
    return {
        'click':   tone(1400, 0.05, 'square', 0.35, decay=0.015),
        'card':    noise(0.05, 0.25, decay=0.015, lowpass=0.2),
        'hit':     punch(),
        'hit1':    punch(150, 50, 0.9, 0.15),
        'hit2':    punch(185, 60, 1.0, 0.35, 0.14),
        'hit3':    punch(125, 42, 0.8, 0.55, 0.18),
        'slap':    mix(noise(0.06, 1.0, decay=0.012, lowpass=0.05), tone(1100, 0.04, 'sine', 0.3, decay=0.01)),
        'crit':    mix(punch(170, 45, 1.0, 0.2, 0.2), tone(85, 0.45, 'sine', 1.0, sweep_to=32, decay=0.14),
                       noise(0.5, 0.35, decay=0.16, lowpass=0.75), tone(1760, 0.12, 'tri', 0.2, decay=0.04)),
        'dhishoom': seq(noise(0.16, 0.5, decay=0.5, lowpass=0.9)[::-1],
                        mix(punch(160, 40, 1.0, 0.25, 0.22), tone(70, 0.5, 'sine', 1.0, sweep_to=30, decay=0.16), noise(0.45, 0.3, decay=0.15, lowpass=0.8))),
        'boom':    mix(tone(60, 0.7, 'sine', 1.0, sweep_to=28, decay=0.22), noise(0.7, 0.6, decay=0.2, lowpass=0.85), noise(0.02, 0.9, decay=0.006)),
        'spray':   noise(0.45, 0.35, decay=0.25, lowpass=0.02),
        'ping':    seq(tone(1568, 0.08, 'sine', 0.45, decay=0.05), tone(2093, 0.18, 'sine', 0.45, decay=0.08)),
        'coin':    seq(tone(988, 0.06, 'square', 0.3, decay=0.03), tone(1319, 0.22, 'square', 0.3, decay=0.09)),
        'heal':    seq(tone(note('C5'), 0.07, 'tri', 0.5), tone(note('E5'), 0.07, 'tri', 0.5), tone(note('G5'), 0.07, 'tri', 0.5), tone(note('C6'), 0.18, 'tri', 0.5, decay=0.08)),
        'ko':      tone(620, 0.45, 'square', 0.45, sweep_to=70, decay=0.25),
        'win':     seq(*[tone(note(n), d, 'square', 0.4) for n, d in [('C5', .09), ('E5', .09), ('G5', .09), ('C6', .3)]]),
        'lose':    seq(tone(note('G4'), .25, 'saw', .4), tone(note('Gb4'), .25, 'saw', .4), tone(note('F4'), .25, 'saw', .4), tone(note('E4'), .7, 'saw', .4, vibrato=0.02)),
        'stamp':   mix(tone(90, 0.3, 'sine', 1.0, sweep_to=45, decay=0.09), noise(0.08, 0.7, decay=0.015)),
        'levelup': seq(*[tone(note(n), .055, 'square', .35) for n in ['C5', 'E5', 'G5', 'C6', 'E6', 'G6']], tone(note('C7'), .2, 'square', .3, decay=.08)),
        'whoosh':  noise(0.35, 0.5, decay=0.2, lowpass=0.85),
        'expire':  mix(noise(0.3, 0.4, decay=0.08, lowpass=0.6), tone(note('E5'), 0.4, 'tri', 0.3, sweep_to=note('E4'), decay=0.15)),
        'honk':    mix(tone(410, 0.32, 'square', 0.3), tone(515, 0.32, 'square', 0.3)),
        'shout':   tone(240, 0.12, 'square', 0.3, sweep_to=480, decay=0.05),
        'blip_lo': tone(220, 0.035, 'square', 0.25, decay=0.012),
        'blip_mid': tone(330, 0.035, 'square', 0.25, decay=0.012),
        'blip_hi': tone(520, 0.035, 'square', 0.25, decay=0.012),
    }

# ------------------------------------------------------------------ music (short loops, Bhairav-ish scale for flavour)
def drum_tabla(dur):  # pitched-down "dhol" thump
    return tone(140, dur, 'sine', 0.7, sweep_to=70, decay=0.08)

def drum_na(dur):     # bright "na"
    return mix(tone(660, dur, 'sine', 0.25, decay=0.04), noise(dur, 0.15, decay=0.02))

def loop(bpm, melody, bass, drums, lead='square', lead_vol=0.16):
    beat = 60 / bpm / 2  # eighth notes
    tracks = []
    m = []
    for n in melody: m += silence(beat) if n == '-' else tone(note(n), beat, lead, lead_vol, decay=beat * 0.9)
    b = []
    for n in bass: b += silence(beat * 2) if n == '-' else tone(note(n), beat * 2, 'tri', 0.35, decay=beat * 1.6)
    d = []
    for h in drums: d += drum_tabla(beat) if h == 'D' else drum_na(beat) if h == 'n' else silence(beat)
    L = len(m) * 2  # trim everything to the melody so the loop is seamless
    return mix(m * 2, (b * 2)[:L], (d * (L // max(1, len(d)) + 1))[:L])

def music():
    day = loop(96,
               melody=['C5', '-', 'Db5', 'E5', 'F5', '-', 'E5', 'Db5', 'C5', '-', '-', 'G4', 'Ab4', 'B4', 'C5', '-',
                       'E5', '-', 'F5', 'G5', 'Ab5', '-', 'G5', 'F5', 'E5', 'Db5', 'C5', '-', 'B4', '-', 'C5', '-'],
               bass=['C3', 'C3', 'F3', 'F3', 'Ab2', 'G2', 'C3', 'C3', 'E3', 'F3', 'G3', 'G3', 'F3', 'E3', 'Db3', 'C3'],
               drums=list('D-n-D-nn'), lead='tri', lead_vol=0.2)
    battle = loop(150,
                  melody=['C5', 'C5', 'Db5', 'C5', 'B4', 'C5', 'E5', '-', 'F5', 'E5', 'Db5', 'C5', 'B4', 'Ab4', 'G4', '-',
                          'G4', 'Ab4', 'B4', 'C5', 'Db5', 'E5', 'F5', 'G5', 'Ab5', 'G5', 'F5', 'E5', 'Db5', 'C5', 'B4', 'C5'],
                  bass=['C2', 'C3', 'C2', 'C3', 'F2', 'F3', 'G2', 'G3', 'Ab2', 'Ab3', 'G2', 'G3', 'F2', 'E2', 'Db2', 'C2'],
                  drums=list('DnDnD-nn'), lead='square', lead_vol=0.12)
    return {'day': day, 'battle': battle}

if __name__ == '__main__':
    root = sys.argv[1]
    os.makedirs(os.path.join(root, 'SFX'), exist_ok=True)
    os.makedirs(os.path.join(root, 'Music'), exist_ok=True)
    for name, s in sfx().items(): save(os.path.join(root, 'SFX', name + '.wav'), s)
    for name, s in music().items(): save(os.path.join(root, 'Music', name + '.wav'), s, gain=0.7)
    print('done')
