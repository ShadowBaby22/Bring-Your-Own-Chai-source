"""Cover / box art from the game's own art: Jaideep's Dilli background, Bablu and the enemies, the KADAK Chai logo.
Writes Builds/cover/cover_<W>x<H>.png for each size below."""
import os
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageEnhance

ROOT = os.path.join(os.path.dirname(__file__), "..")
ART = os.path.join(ROOT, "Assets/_Game/Resources/Art")
FONT = os.path.join(ROOT, "Assets/_Game/Resources/Fonts/Baloo2.ttf")
OUT = os.path.join(ROOT, "Builds/cover")
YELLOW, RED = (255, 225, 77), (229, 57, 53)

def font(px):
    f = ImageFont.truetype(FONT, px)
    try: f.set_variation_by_name("ExtraBold")
    except Exception: pass
    return f

def img(path): return Image.open(os.path.join(ART, path)).convert("RGBA")

def cover(im, W, H):
    s = max(W / im.width, H / im.height)
    im = im.resize((round(im.width * s), round(im.height * s)), Image.LANCZOS)
    x, y = (im.width - W) // 2, im.height - H
    return im.crop((x, y, x + W, y + H))

def put(canvas, im, cx, bottom, h, dim=1.0, flip=False, shadow=True):
    s = h / im.height
    im = im.resize((round(im.width * s), round(h)), Image.LANCZOS)
    if flip: im = im.transpose(Image.FLIP_LEFT_RIGHT)
    if dim < 1: im = Image.merge("RGBA", (*ImageEnhance.Brightness(im.convert("RGB")).enhance(dim).split(), im.getchannel("A")))
    x, y = round(cx - im.width / 2), round(bottom - im.height)
    if shadow:
        sh = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
        ImageDraw.Draw(sh).ellipse((x + im.width * .2, bottom - h * .04, x + im.width * .8, bottom + h * .03), fill=(0, 0, 0, 110))
        canvas.alpha_composite(sh.filter(ImageFilter.GaussianBlur(h * .02)))
    canvas.alpha_composite(im, (x, y))

def text(canvas, xy, s, px, fill, stroke, anchor="mm", angle=0):
    layer = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    d.text(xy, s, font=font(px), fill=fill, stroke_width=stroke, stroke_fill=(0, 0, 0), anchor=anchor)
    if angle: layer = layer.rotate(angle, center=xy, resample=Image.BICUBIC)
    canvas.alpha_composite(layer)

def make(W, H):
    u = min(W, H)
    wide = W / H > 1.4
    tall = H / W > 1.3
    c = cover(img("Backgrounds/dilli.png"), W, H)
    smog = img("UI/smog.png").resize((W, H), Image.LANCZOS)
    smog.putalpha(smog.getchannel("A").point(lambda a: a * 0.55))
    c.alpha_composite(smog)
    shade = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    sd = ImageDraw.Draw(shade)
    for i in range(H):   # darken top (logo) and bottom (tagline) a little
        t = i / H
        a = int(120 * max(0, 0.30 - t) / 0.30 + 150 * max(0, t - 0.72) / 0.28)
        sd.line((0, i, W, i), fill=(10, 8, 20, a))
    c.alpha_composite(shade)

    floor = H * (0.93 if wide else 0.90)
    side = H * (0.40 if wide else 0.22 if tall else 0.34)
    gap = W * (0.17 if wide else 0.23 if tall else 0.20)
    put(c, img("Characters/scooty_uncle.png"), W / 2 - gap * 1.9, floor - H * .03, side * .92, dim=.8)
    put(c, img("Characters/babu.png"), W / 2 + gap * 1.9, floor - H * .03, side * .92, dim=.8, flip=True)
    put(c, img("Characters/pinky_aunty.png"), W / 2 - gap, floor, side)
    put(c, img("Characters/auto_bhaiya.png"), W / 2 + gap, floor, side, flip=True)
    put(c, img("Characters/new_kid_armed.png"), W / 2, floor + H * .02, H * (0.60 if wide else 0.40 if tall else 0.52))

    ly = H * (0.17 if wide else 0.20 if tall else 0.16)
    px = round(u * (0.26 if wide else 0.27 if tall else 0.20))
    text(c, (W / 2, ly), "KADAK", px, YELLOW, max(4, px // 11), angle=4)
    text(c, (W / 2 + (px * .9 if tall else px * 1.35), ly + px * (.75 if tall else .55)), "Chai", round(px * .62), (255, 255, 255), max(3, px // 14), angle=4)
    cup = img("UI/item_cutting_chai.png")
    if tall: put(c, cup, W / 2, ly - px * .55, px * .9, shadow=False)
    else: put(c, cup, W / 2 - px * 1.85, ly + px * .55, px * .9, shadow=False)

    tp = round(u * (0.045 if wide else 0.05))
    tag = "Start playing before the chai gets cold."
    tw = font(tp).getlength(tag)
    pill = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    ty = H * 0.955 if wide else H * 0.95
    ImageDraw.Draw(pill).rounded_rectangle((W / 2 - tw / 2 - tp, ty - tp * .85, W / 2 + tw / 2 + tp, ty + tp * .75), radius=tp, fill=(0, 0, 0, 170))
    c.alpha_composite(pill)
    text(c, (W / 2, ty), tag, tp, (255, 255, 255), 0)
    os.makedirs(OUT, exist_ok=True)
    path = os.path.join(OUT, f"cover_{W}x{H}.png")
    c.convert("RGB").save(path)
    return path

if __name__ == "__main__":
    import sys
    sizes = [tuple(map(int, a.split("x"))) for a in sys.argv[1:]] or [(1920, 1080), (630, 500), (1080, 1080), (1080, 1920)]
    for size in sizes:
        print(make(*size))
