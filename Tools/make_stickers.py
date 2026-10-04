#!/usr/bin/env python3
"""Builds the sticker-sheet art from the game art: a die-cut sticker (thick white border) and a white blank
(the cut-out shape, white fill, thin grey edge) for every sticker in Album.cs.

    python3 Tools/make_stickers.py

Re-run it after replacing character or icon art (e.g. Jaideep's originals) so the stickers match.
Output: Assets/_Game/Resources/Art/Stickers/{id}.png and {id}_blank.png (+ .meta for new files).
"""
import os, re, uuid
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, 'Assets/_Game/Resources/Art')
OUT = os.path.join(ART, 'Stickers')
FONT = os.path.join(ROOT, 'Assets/_Game/Resources/Fonts/Baloo2.ttf')
META_TEMPLATE = os.path.join(ART, 'UI/item_chappal.png.meta')
SIZE = 300      # longest side of the art before the border
BORDER = 10     # white die-cut border, px

# sticker id -> (source art, kind). Ids must match Album.cs.
CHAR = lambda n: os.path.join(ART, 'Characters', n + '.png')
UI = lambda n: os.path.join(ART, 'UI', n + '.png')
STICKERS = {
    'bablu': (CHAR('new_kid'), 'art'), 'scooty_uncle': (CHAR('scooty_uncle'), 'art'),
    'auto_bhaiya': (CHAR('auto_bhaiya'), 'art'), 'south_delhi_girl': (CHAR('south_delhi_girl'), 'art'),
    'pinky_aunty': (CHAR('pinky_aunty'), 'art'), 'jaanu': (CHAR('jaanu'), 'art'), 'shona': (CHAR('shona'), 'art'),
    'broker': (CHAR('broker'), 'art'), 'kadak': (UI('item_cutting_chai'), 'art'), 'laathi': (UI('item_laathi'), 'art'),
    'techbro': (CHAR('techbro'), 'art'), 'daaru_uncle': (CHAR('daaru_uncle'), 'art'), 'ceo': (CHAR('ceo'), 'art'),
    'cfo_bunty': (CHAR('cfo_bunty'), 'art'), 'cto_pappu': (CHAR('cto_pappu'), 'art'), 'monkey': (CHAR('monkey'), 'art'),
    'sector_cabbie': (CHAR('sector_cabbie'), 'art'), 'firecracker': (CHAR('firecracker'), 'art'),
    'sheru': (CHAR('sheru'), 'art'), 'rwa_aunty': (CHAR('rwa_aunty'), 'art'), 'bald_bablu': (CHAR('new_kid_shaved'), 'art'),
    'rival_student': (CHAR('rival_student'), 'art'), 'babu': (CHAR('babu'), 'art'),
    'zombie_bablu': (CHAR('new_kid'), 'zombie'), 'stamp': (None, 'stamp'),
}


def fit(im, size=SIZE):
    im = im.crop(im.getbbox())
    k = size / max(im.size)
    return im.resize((max(1, round(im.width * k)), max(1, round(im.height * k))), Image.LANCZOS)


def die_cut(art):
    """Pads the art and puts a thick white sticker border around its silhouette."""
    pad = BORDER + 6
    canvas = Image.new('RGBA', (art.width + 2 * pad, art.height + 2 * pad), (0, 0, 0, 0))
    canvas.paste(art, (pad, pad), art)
    alpha = canvas.split()[3].point(lambda a: 255 if a > 40 else 0)
    grown = alpha.filter(ImageFilter.MaxFilter(2 * BORDER + 1)).filter(ImageFilter.GaussianBlur(1.2))
    sticker = Image.new('RGBA', canvas.size, (255, 255, 255, 0))
    sticker.putalpha(grown)
    sticker.alpha_composite(canvas)
    return sticker


def blank(sticker):
    """The missing-sticker cut-out: the same shape in white, with a thin grey edge."""
    alpha = sticker.split()[3]
    inner = alpha.filter(ImageFilter.MinFilter(5))
    edge = ImageChops.subtract(alpha, inner)
    out = Image.new('RGBA', sticker.size, (250, 250, 250, 0))
    out.putalpha(alpha)
    grey = Image.new('RGBA', sticker.size, (190, 190, 190, 0))
    grey.putalpha(edge)
    out.alpha_composite(grey)
    return out


def zombie(art):
    """Green skin and clothes; the white outline and highlights stay white."""
    r, g, b, a = art.split()
    tinted = Image.merge('RGBA', (r.point(lambda v: int(v * 0.55)), g.point(lambda v: min(255, int(v * 1.05 + 30))), b.point(lambda v: int(v * 0.5)), a))
    darkest = ImageChops.darker(ImageChops.darker(r, g), b)        # min channel: high only for near-white pixels
    keep_white = darkest.point(lambda v: 255 if v > 225 else 0)
    return Image.composite(art, tinted, keep_white)


def stamp():
    w, h = 340, 220
    im = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    red = (211, 47, 47, 255)
    d.rounded_rectangle((8, 8, w - 8, h - 8), radius=22, outline=red, width=10, fill=(255, 244, 220, 255))
    big = ImageFont.truetype(FONT, 46)
    small = ImageFont.truetype(FONT, 30)
    for y, text, f in ((40, 'VALID FOR', small), (78, '11 MONTHS', big), (140, 'SARKARI DAFTAR', small)):
        tw = d.textlength(text, font=f)
        d.text(((w - tw) / 2, y), text, font=f, fill=red)
    return im.rotate(-8, expand=True, resample=Image.BICUBIC)


def write_meta(png):
    meta = png + '.meta'
    if os.path.exists(meta) or not os.path.exists(META_TEMPLATE):
        return
    text = open(META_TEMPLATE).read()
    open(meta, 'w').write(re.sub(r'^guid: .*$', 'guid: ' + uuid.uuid4().hex, text, count=1, flags=re.M))


def main():
    os.makedirs(OUT, exist_ok=True)
    for sid, (src, kind) in STICKERS.items():
        if kind == 'stamp':
            art = fit(stamp())
        else:
            art = fit(Image.open(src).convert('RGBA'))
            if kind == 'zombie':
                art = zombie(art)
        s = die_cut(art)
        for name, img in ((sid, s), (sid + '_blank', blank(s))):
            path = os.path.join(OUT, name + '.png')
            img.save(path, optimize=True)
            write_meta(path)
    print(len(STICKERS), 'stickers ->', os.path.relpath(OUT, ROOT))


if __name__ == '__main__':
    main()
