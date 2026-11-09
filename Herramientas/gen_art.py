"""Genera el arte original (tiles, objetos, fondos) del juego Kage no Michi.
Todo es pixel art procedural creado para este proyecto."""
import math, random, os, sys
from PIL import Image, ImageDraw

OUT = sys.argv[1]
os.makedirs(OUT + "/Tiles", exist_ok=True)
os.makedirs(OUT + "/Props", exist_ok=True)
os.makedirs(OUT + "/Backgrounds", exist_ok=True)

def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)

def shade(c, f):
    return tuple(max(0, min(255, int(v * f))) for v in c[:3]) + (c[3],)

def mix(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3)) + (255,)

def new(w, h):
    return Image.new("RGBA", (w, h), (0, 0, 0, 0))

def save(im, path):
    im.save(OUT + "/" + path)

R = random.Random(7)

# ---------------------------------------------------------------- TILES
def ground_tile(base, dark, light, top=None, top_dark=None, top_light=None, seed=1, kind="dirt"):
    r = random.Random(seed)
    im = new(32, 32); p = im.load()
    for y in range(32):
        for x in range(32):
            n = r.random()
            c = base
            if n < 0.12: c = dark
            elif n > 0.93: c = light
            p[x, y] = c
    # piedras
    for _ in range(4):
        cx, cy = r.randrange(3, 29), r.randrange(10 if top else 3, 29)
        for dy in range(-1, 2):
            for dx in range(-2, 2):
                if 0 <= cx+dx < 32 and 0 <= cy+dy < 32:
                    p[cx+dx, cy+dy] = light if dy < 0 else dark
    if top:
        for x in range(32):
            h = 7 + (1 if (x * 7 + seed) % 5 == 0 else 0) + (2 if x % 6 == 2 else 0)
            for y in range(h):
                c = top
                if y == 0 or y == 1: c = top_light
                elif y >= h - 2: c = top_dark
                p[x, y] = c
        for x in range(0, 32, 4):
            p[x, 1] = shade(top_light, 1.15)
    return im

def brick_tile(base, mortar, light, dark, seed=3, top=False, cap=None):
    im = new(32, 32); p = im.load(); r = random.Random(seed)
    for y in range(32):
        for x in range(32):
            p[x, y] = mortar
    rows = [(0, 8), (8, 16), (16, 24), (24, 32)]
    for i, (y0, y1) in enumerate(rows):
        off = 0 if i % 2 == 0 else 8
        for bx in range(-16, 32, 16):
            x0 = bx + off
            for y in range(y0 + 1, y1):
                for x in range(x0 + 1, x0 + 16):
                    if 0 <= x < 32:
                        c = base
                        if y == y0 + 1 or x == x0 + 1: c = light
                        elif y == y1 - 1 or x == x0 + 15: c = dark
                        elif r.random() < 0.08: c = shade(base, 0.9)
                        p[x, y] = c
    if top and cap:
        for y in range(6):
            for x in range(32):
                p[x, y] = cap[0] if y < 2 else (cap[1] if y < 5 else cap[2])
        for x in range(0, 32, 8):
            for y in range(2, 5):
                p[x, y] = cap[2]
    return im

def stone_block(base=hexc('#8a8f99'), light=hexc('#c3c8d0'), dark=hexc('#4b4f57')):
    im = new(32, 32); p = im.load(); r = random.Random(11)
    for y in range(32):
        for x in range(32):
            c = base
            if x < 3 or y < 3: c = light
            if x > 28 or y > 28: c = dark
            if r.random() < 0.06: c = shade(c, 0.88)
            p[x, y] = c
    for (x, y) in [(6, 6), (25, 6), (6, 25), (25, 25)]:
        p[x, y] = dark; p[x+1, y] = dark; p[x, y+1] = dark; p[x+1, y+1] = light
    return im

def crate():
    im = new(32, 32); d = ImageDraw.Draw(im)
    wood, wl, wd, line = hexc('#b07a3c'), hexc('#d6a25a'), hexc('#7a4f22'), hexc('#4a2e12')
    d.rectangle([0, 0, 31, 31], fill=wood)
    for y in range(4, 28, 6):
        d.line([3, y, 28, y], fill=shade(wood, 0.88))
    d.line([3, 3, 28, 28], fill=wd, width=3)
    d.line([28, 3, 3, 28], fill=wd, width=3)
    d.rectangle([0, 0, 31, 31], outline=line)
    d.rectangle([1, 1, 30, 30], outline=wl)
    d.rectangle([2, 2, 29, 29], outline=wd)
    for (x, y) in [(4, 4), (27, 4), (4, 27), (27, 27)]:
        d.point((x, y), fill=hexc('#e0e0e0'))
    return im

QMARK = ["0111110", "1100011", "0000011", "0000110", "0001100", "0001100", "0000000", "0001100", "0001100"]

def question(frame):
    im = new(32, 32); p = im.load()
    gold, gl, gd, edge = hexc('#f2b632'), hexc('#ffe28a'), hexc('#b0741a'), hexc('#5a3608')
    if frame == 1: gold, gl = hexc('#f7c84c'), hexc('#fff3c0')
    for y in range(32):
        for x in range(32):
            c = gold
            if x < 2 or y < 2: c = gl
            if x > 29 or y > 29: c = gd
            if x == 0 or y == 0 or x == 31 or y == 31: c = edge
            p[x, y] = c
    for (x, y) in [(4, 4), (27, 4), (4, 27), (27, 27)]:
        p[x, y] = gd; p[x+1, y] = gd; p[x, y+1] = gd; p[x+1, y+1] = gd
    for j, row in enumerate(QMARK):
        for i, ch in enumerate(row):
            if ch == '1':
                for sy in range(2):
                    for sx in range(2):
                        X, Y = 9 + i*2 + sx, 7 + j*2 + sy
                        p[X+1, Y+1] = gd
                        p[X, Y] = hexc('#fffaf0')
    return im

def used_block():
    im = stone_block(hexc('#8a5a32'), hexc('#a77445'), hexc('#4d2f16'))
    return im

def spikes():
    im = new(32, 32); d = ImageDraw.Draw(im)
    for i, x in enumerate([2, 10, 18, 26]):
        h = 22 if i % 2 == 0 else 26
        top = 32 - h
        d.polygon([(x, 32), (x + 2, top), (x + 4, 32)], fill=hexc('#6fa83a'))
        d.line([x + 2, top + 2, x + 2, 31], fill=hexc('#a9d86a'))
        d.line([x + 4, top + 6, x + 4, 31], fill=hexc('#3f6b1e'))
        d.polygon([(x + 1, top + 6), (x + 2, top), (x + 3, top + 6)], fill=hexc('#e8e2c0'))
    d.rectangle([0, 27, 31, 31], fill=hexc('#5a3a1c'))
    d.line([0, 27, 31, 27], fill=hexc('#8a5a2c'))
    return im

def plank():
    im = new(32, 32); d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 31, 9], fill=hexc('#a0692e'))
    d.line([0, 0, 31, 0], fill=hexc('#d39b55'))
    d.line([0, 9, 31, 9], fill=hexc('#5a3510'))
    for x in (7, 19, 27):
        d.line([x, 2, x + 3, 2], fill=hexc('#7a4a1c'))
    d.line([1, 5, 30, 5], fill=hexc('#8c5a26'))
    d.line([0, 1, 0, 9], fill=hexc('#5a3510')); d.line([31, 1, 31, 9], fill=hexc('#5a3510'))
    # cuerdas
    for x in (4, 27):
        d.line([x, 10, x, 13], fill=hexc('#d8c58c'))
    return im

def raft():
    im = new(32, 16); d = ImageDraw.Draw(im)
    for i in range(4):
        x0 = i * 8
        d.rectangle([x0, 0, x0 + 7, 11], fill=hexc('#c28b47'))
        d.line([x0, 0, x0 + 7, 0], fill=hexc('#ecc07c'))
        d.line([x0 + 7, 0, x0 + 7, 11], fill=hexc('#7a4a1c'))
    d.rectangle([0, 3, 31, 5], fill=hexc('#6b4220'))
    d.rectangle([0, 12, 31, 13], fill=hexc('#4a2a10'))
    return im

def ground_sets():
    # Nivel 1: hierba / tierra
    g = dict(base=hexc('#8a5a2e'), dark=hexc('#5c3a1c'), light=hexc('#ad7a44'))
    save(ground_tile(top=hexc('#5cb043'), top_dark=hexc('#3a7a2a'), top_light=hexc('#9ae065'), seed=1, **g), "Tiles/l1_top.png")
    save(ground_tile(seed=2, **g), "Tiles/l1_fill.png")
    # Nivel 2: roca de montaña
    r = dict(base=hexc('#7a6a5e'), dark=hexc('#4e423a'), light=hexc('#9e8e80'))
    save(ground_tile(top=hexc('#c9a86a'), top_dark=hexc('#8a6e3e'), top_light=hexc('#ead39a'), seed=3, **r), "Tiles/l2_top.png")
    save(ground_tile(seed=4, **r), "Tiles/l2_fill.png")
    # Nivel 3: castillo
    cap = (hexc('#c9b98f'), hexc('#a8986e'), hexc('#6e6046'))
    save(brick_tile(hexc('#4a4e63'), hexc('#24263a'), hexc('#6b7090'), hexc('#33364a'), top=True, cap=cap), "Tiles/l3_top.png")
    save(brick_tile(hexc('#4a4e63'), hexc('#24263a'), hexc('#6b7090'), hexc('#33364a'), seed=5), "Tiles/l3_fill.png")

ground_sets()
save(stone_block(), "Tiles/hard.png")
save(crate(), "Tiles/brick.png")
qs = new(64, 32); qs.paste(question(0), (0, 0)); qs.paste(question(1), (32, 0)); save(qs, "Tiles/question.png")
save(used_block(), "Tiles/used.png")
save(spikes(), "Tiles/spikes.png")
save(plank(), "Tiles/plank.png")
save(raft(), "Tiles/raft.png")

# ---------------------------------------------------------------- PROPS
def coin_sheet():
    im = new(64, 16)
    widths = [12, 8, 2, 8]
    for f, w in enumerate(widths):
        d = ImageDraw.Draw(im)
        cx = f * 16 + 8
        x0, x1 = cx - w // 2, cx + w // 2 - 1 if w > 2 else cx
        d.ellipse([x0 - 1, 1, x1 + 1, 14], fill=hexc('#8a5a08'))
        d.ellipse([x0, 2, x1, 13], fill=hexc('#f4c430'))
        if w >= 8:
            d.ellipse([x0 + 1, 3, x1 - 1, 12], outline=hexc('#ffe58a'))
            # agujero cuadrado (moneda mon)
            hw = 1 if w == 8 else 2
            d.rectangle([cx - hw, 6, cx + hw - 1, 9], fill=hexc('#8a5a08'))
        d.point((x0 + 1, 4), fill=hexc('#ffffff'))
    return im

HEART = ["0110110", "1111111", "1111111", "1111111", "0111110", "0011100", "0001000"]
def heart():
    im = new(16, 16); p = im.load()
    for j, row in enumerate(HEART):
        for i, ch in enumerate(row):
            if ch == '1':
                for s in range(2):
                    for t in range(2):
                        p[1 + i*2 + s, 1 + j*2 + t] = hexc('#e8283c')
    # contorno
    base = im.copy(); bp = base.load()
    for y in range(16):
        for x in range(16):
            if bp[x, y][3] == 0:
                for dx, dy in ((1,0),(-1,0),(0,1),(0,-1)):
                    X, Y = x+dx, y+dy
                    if 0 <= X < 16 and 0 <= Y < 16 and bp[X, Y][3] > 0:
                        p[x, y] = hexc('#5a0a14'); break
    p[3, 3] = p[4, 3] = p[3, 4] = hexc('#ffb0b8')
    for x in range(9, 13): p[x, 9 - (x - 9)] = hexc('#a8101f') if p[x, 9-(x-9)][3] else p[x, 9-(x-9)]
    return im

def shuriken():
    im = new(32, 16)
    for f in range(2):
        d = ImageDraw.Draw(im)
        cx, cy = f * 16 + 8, 8
        ang0 = 0 if f == 0 else math.pi / 4
        for k in range(4):
            a = ang0 + k * math.pi / 2
            tip = (cx + math.cos(a) * 7, cy + math.sin(a) * 7)
            l = (cx + math.cos(a + 0.9) * 2.5, cy + math.sin(a + 0.9) * 2.5)
            rr = (cx + math.cos(a - 0.4) * 2, cy + math.sin(a - 0.4) * 2)
            d.polygon([l, tip, rr], fill=hexc('#b8c0cc'), outline=hexc('#3a404a'))
        d.ellipse([cx - 2, cy - 2, cx + 1, cy + 1], fill=hexc('#2a2e36'))
    return im

def onibi(color=('#3a8cff', '#9fd0ff', '#ffffff')):
    im = new(32, 16)
    for f in range(2):
        d = ImageDraw.Draw(im)
        ox = f * 16
        sway = 1 if f == 0 else -1
        d.polygon([(ox + 3, 12), (ox + 8 + sway, 0), (ox + 13, 12)], fill=hexc(color[0]))
        d.ellipse([ox + 2, 5, ox + 13, 15], fill=hexc(color[0]))
        d.ellipse([ox + 4, 7, ox + 11, 14], fill=hexc(color[1]))
        d.ellipse([ox + 6, 9, ox + 9, 13], fill=hexc(color[2]))
    return im

def turret():
    # linterna de piedra (toro) que dispara shuriken
    im = new(32, 32); d = ImageDraw.Draw(im)
    st, sl, sd = hexc('#9a9a8a'), hexc('#c8c8b4'), hexc('#5a5a4e')
    d.polygon([(2, 9), (16, 1), (30, 9)], fill=sd)
    d.polygon([(4, 8), (16, 2), (28, 8)], fill=st)
    d.line([4, 8, 16, 2], fill=sl)
    d.rectangle([6, 10, 25, 20], fill=st)
    d.rectangle([6, 10, 25, 10], fill=sl)
    d.rectangle([10, 12, 21, 18], fill=hexc('#1a1410'))
    d.rectangle([12, 13, 19, 17], fill=hexc('#ff8a1a'))
    d.rectangle([14, 14, 17, 16], fill=hexc('#ffe070'))
    d.rectangle([11, 21, 20, 26], fill=sd)
    d.rectangle([12, 21, 19, 25], fill=st)
    d.rectangle([5, 27, 26, 31], fill=st)
    d.line([5, 27, 26, 27], fill=sl)
    d.line([5, 31, 26, 31], fill=sd)
    return im

def yokai():
    # chochin-obake: farol de papel fantasma
    im = new(128, 32)
    for f in range(4):
        d = ImageDraw.Draw(im)
        ox = f * 32
        bob = [0, 1, 2, 1][f]
        y0 = 4 + bob
        d.rectangle([ox + 10, y0 - 3, ox + 21, y0], fill=hexc('#2a1a12'))
        d.ellipse([ox + 5, y0, ox + 26, y0 + 22], fill=hexc('#e8452c'), outline=hexc('#5a120a'))
        for k in range(5):
            yy = y0 + 4 + k * 4
            d.line([ox + 7, yy, ox + 24, yy], fill=hexc('#b02a18'))
        d.ellipse([ox + 8, y0 + 2, ox + 14, y0 + 8], fill=hexc('#ff9a7a'))
        d.rectangle([ox + 10, y0 + 21, ox + 21, y0 + 24], fill=hexc('#2a1a12'))
        # ojo
        d.ellipse([ox + 13, y0 + 6, ox + 22, y0 + 13], fill=hexc('#fff8e0'), outline=hexc('#2a0a06'))
        px = ox + 17 + (1 if f in (1, 2) else 0)
        d.rectangle([px, y0 + 8, px + 2, y0 + 11], fill=hexc('#1a0a06'))
        # boca y lengua
        d.line([ox + 11, y0 + 16, ox + 22, y0 + 16], fill=hexc('#2a0a06'), width=2)
        tl = 3 + (f % 2) * 2
        d.rectangle([ox + 14, y0 + 17, ox + 17, y0 + 17 + tl], fill=hexc('#ff6fa0'))
        # brillo amarillo interior
        d.point((ox + 9, y0 + 12), fill=hexc('#ffd040'))
    return im

def torii():
    im = new(96, 96); d = ImageDraw.Draw(im)
    red, rl, rd, blk = hexc('#d8321e'), hexc('#ff6a48'), hexc('#8a1a0e'), hexc('#1e1a1a')
    # pilares
    for x in (18, 70):
        d.rectangle([x, 22, x + 7, 95], fill=red)
        d.line([x, 22, x, 95], fill=rl)
        d.line([x + 7, 22, x + 7, 95], fill=rd)
        d.rectangle([x - 2, 88, x + 9, 95], fill=blk)
    # kasagi (viga superior curva)
    for x in range(2, 94):
        t = (x - 48) / 46.0
        lift = int(5 * t * t * t * t * 2) if abs(t) > 0.5 else 0
        d.line([x, 6 - lift, x, 12 - lift], fill=blk)
        d.line([x, 12 - lift, x, 17 - lift], fill=red)
        d.point((x, 12 - lift), fill=rl)
    # nuki (viga inferior)
    d.rectangle([10, 30, 85, 36], fill=red)
    d.line([10, 30, 85, 30], fill=rl)
    d.line([10, 36, 85, 36], fill=rd)
    # placa central
    d.rectangle([43, 15, 52, 30], fill=blk)
    d.rectangle([44, 17, 51, 28], fill=hexc('#e8c070'))
    d.line([46, 19, 49, 19], fill=blk); d.line([46, 22, 49, 22], fill=blk); d.line([47, 24, 47, 27], fill=blk)
    return im

def checkpoint():
    im = new(64, 64)
    for f in range(2):
        d = ImageDraw.Draw(im)
        ox = f * 32
        d.rectangle([ox + 6, 4, ox + 8, 63], fill=hexc('#3a2a1a'))
        d.line([ox + 6, 4, ox + 6, 63], fill=hexc('#7a5a3a'))
        d.ellipse([ox + 4, 1, ox + 10, 6], fill=hexc('#e8c040'))
        if f == 0:
            d.rectangle([ox + 9, 8, ox + 26, 34], fill=hexc('#ece6d6'))
            d.line([ox + 9, 34, ox + 26, 34], fill=hexc('#b8b0a0'))
            d.ellipse([ox + 13, 15, ox + 22, 25], outline=hexc('#b8b0a0'))
        else:
            d.rectangle([ox + 9, 8, ox + 26, 34], fill=hexc('#f4efe2'))
            d.ellipse([ox + 12, 14, ox + 23, 26], fill=hexc('#d8201c'))
            d.line([ox + 9, 34, ox + 26, 34], fill=hexc('#b8b0a0'))
        d.rectangle([ox + 3, 60, ox + 12, 63], fill=hexc('#5a5a50'))
    return im

def deco_lantern():
    im = new(32, 64); d = ImageDraw.Draw(im)
    st, sl, sd = hexc('#8e8e80'), hexc('#bdbdac'), hexc('#55554a')
    d.rectangle([4, 58, 27, 63], fill=st); d.line([4, 58, 27, 58], fill=sl)
    d.rectangle([12, 34, 19, 57], fill=st); d.line([12, 34, 12, 57], fill=sl); d.line([19, 34, 19, 57], fill=sd)
    d.rectangle([6, 28, 25, 33], fill=st)
    d.rectangle([8, 16, 23, 27], fill=st)
    d.rectangle([11, 18, 20, 25], fill=hexc('#ffb040'))
    d.rectangle([13, 19, 18, 24], fill=hexc('#ffe9a0'))
    d.polygon([(2, 16), (16, 6), (29, 16)], fill=sd)
    d.polygon([(5, 15), (16, 8), (26, 15)], fill=st)
    d.rectangle([14, 2, 17, 7], fill=st)
    return im

def debris():
    im = new(8, 8); d = ImageDraw.Draw(im)
    d.rectangle([0, 0, 7, 7], fill=hexc('#b07a3c'), outline=hexc('#4a2e12'))
    d.line([1, 1, 6, 1], fill=hexc('#d6a25a'))
    return im

def white():
    im = Image.new("RGBA", (4, 4), (255, 255, 255, 255)); return im

save(coin_sheet(), "Props/coin.png")
save(heart(), "Props/heart.png")
save(shuriken(), "Props/shuriken.png")
save(onibi(), "Props/onibi.png")
save(onibi(('#ff4a1a', '#ffb040', '#fff4c0')), "Props/fire.png")
save(turret(), "Props/turret.png")
save(yokai(), "Props/yokai.png")
save(torii(), "Props/torii.png")
save(checkpoint(), "Props/checkpoint.png")
save(deco_lantern(), "Props/lantern.png")
save(debris(), "Props/debris.png")
save(white(), "Props/white.png")

# ---------------------------------------------------------------- FONDOS (480x270, tileables en X)
W, H = 480, 270

def gradient(top, bottom, h=H, w=W):
    im = Image.new("RGBA", (w, h)); p = im.load()
    for y in range(h):
        t = y / (h - 1)
        # banding estilo pixel art
        t = round(t * 12) / 12
        c = mix(top, bottom, t)
        for x in range(w): p[x, y] = c
    return im

def ridge(seed, base, amp, harmonics):
    r = random.Random(seed)
    comps = [(k, r.uniform(0, 6.28), r.uniform(0.3, 1.0) / k ** 0.7) for k in harmonics]
    def f(x):
        return base - amp * sum(a * math.sin(2 * math.pi * k * x / W + ph) for k, ph, a in comps)
    return f

def fill_ridge(im, f, color, light=None, snow=None):
    p = im.load()
    for x in range(W):
        top = int(f(x))
        for y in range(max(0, top), H):
            c = color
            if snow and y < top + 6 and top < snow[0]: c = snow[1]
            p[x, y] = c
        if light and 0 <= top < H:
            p[x, top] = light

def clouds(im, seed, color, n=7, ymin=20, ymax=110):
    d = ImageDraw.Draw(im); r = random.Random(seed)
    for _ in range(n):
        cx, cy = r.randrange(0, W), r.randrange(ymin, ymax)
        w = r.randrange(40, 90)
        for k in range(5):
            ox = r.randrange(-w // 2, w // 2)
            rad = r.randrange(8, 16)
            for dx in (-W, 0, W):
                d.ellipse([cx + ox - rad + dx, cy - rad // 2, cx + ox + rad + dx, cy + rad // 2], fill=color)

def bamboo_layer(seed):
    im = new(W, H); d = ImageDraw.Draw(im); r = random.Random(seed)
    for i in range(16):
        x = i * 30 + r.randrange(0, 14)
        w = r.choice([4, 5, 6])
        col = r.choice([hexc('#3d7a55'), hexc('#4a8a5e'), hexc('#356b4a')])
        y0 = r.randrange(0, 60)
        step = r.randrange(26, 34)
        for dx in (0, -W):
            X = x + dx
            d.rectangle([X, y0, X + w - 1, H], fill=col)
            d.line([X, y0, X, H], fill=shade(col, 1.2))
            for y in range(y0 + 20, H, step):
                d.line([X, y, X + w - 1, y], fill=shade(col, 0.7))
            rr = random.Random(i)
            for _ in range(5):
                ly = rr.randrange(y0 + 10, 170)
                if rr.random() < 0.5:
                    d.polygon([(X + w, ly), (X + w + 14, ly + 4), (X + w, ly + 6)], fill=hexc('#5a9e66'))
                else:
                    d.polygon([(X, ly), (X - 14, ly + 4), (X, ly + 6)], fill=hexc('#5a9e66'))
    # arbustos al pie
    for x in range(0, W, 3):
        hgt = 20 + int(8 * math.sin(x / W * 2 * math.pi * 6)) + int(5 * math.sin(x / W * 2 * math.pi * 15))
        d.line([x, H - hgt, x, H], fill=hexc('#1f5226'))
        d.line([x + 1, H - hgt + 2, x + 1, H], fill=hexc('#1f5226'))
        d.line([x + 2, H - hgt + 1, x + 2, H], fill=hexc('#1f5226'))
    return im

def pines_layer(seed, col, col2):
    im = new(W, H); d = ImageDraw.Draw(im); r = random.Random(seed)
    f = ridge(seed, 215, 18, [2, 5, 9])
    for x in range(W):
        d.line([x, int(f(x)), x, H], fill=col)
    for i in range(16):
        x = i * 30 + r.randrange(0, 12)
        base = int(f(x % W)) + 4
        h = r.randrange(50, 85)
        for k in range(5):
            yy = base - h + k * (h // 5)
            ww = 6 + k * 4
            for dx in (0, -W, W):
                d.polygon([(x + dx, yy), (x + dx - ww, yy + h // 4), (x + dx + ww, yy + h // 4)], fill=col2)
        d.rectangle([x - 1, base - 6, x + 1, base + 2], fill=col2)
    return im

def castle_silhouette(im, x0, base, col, win):
    d = ImageDraw.Draw(im)
    tiers = [(70, 26), (54, 22), (40, 20), (26, 18)]
    y = base
    for i, (w, h) in enumerate(tiers):
        d.rectangle([x0 - w // 2, y - h, x0 + w // 2, y], fill=col)
        # techo curvo
        d.polygon([(x0 - w // 2 - 10, y - h + 2), (x0 - w // 2 + 4, y - h - 8), (x0 + w // 2 - 4, y - h - 8), (x0 + w // 2 + 10, y - h + 2)], fill=col)
        for k in range(-w // 2 + 8, w // 2 - 6, 10):
            d.rectangle([x0 + k, y - h + 8, x0 + k + 3, y - h + 13], fill=win)
        y -= h + 6
    d.rectangle([x0 - 2, y - 6, x0 + 2, y + 6], fill=col)
    d.rectangle([x0 - 50, base, x0 + 50, base + 30], fill=col)

def stars(im, seed, n=120):
    p = im.load(); r = random.Random(seed)
    for _ in range(n):
        x, y = r.randrange(W), r.randrange(0, 170)
        p[x, y] = (255, 255, 230, 255) if r.random() < 0.3 else (190, 200, 255, 255)

def moon(im, cx, cy, rad, col, glow):
    d = ImageDraw.Draw(im)
    d.ellipse([cx - rad - 8, cy - rad - 8, cx + rad + 8, cy + rad + 8], fill=glow)
    d.ellipse([cx - rad, cy - rad, cx + rad, cy + rad], fill=col)
    d.ellipse([cx - rad // 3, cy - rad // 4, cx, cy + rad // 6], fill=shade(col, 0.9))
    d.ellipse([cx + rad // 4, cy + rad // 3, cx + rad // 2, cy + rad // 2], fill=shade(col, 0.9))

# Nivel 1 - Aldea del Bambu (dia)
sky = gradient(hexc('#5ab0f0'), hexc('#cdeefa'))
d = ImageDraw.Draw(sky); d.ellipse([360, 30, 400, 70], fill=hexc('#fff6c8')); d.ellipse([366, 36, 394, 64], fill=hexc('#fffbe8'))
clouds(sky, 1, hexc('#ffffff'))
save(sky, "Backgrounds/l1_sky.png")
far = new(W, H)
fill_ridge(far, ridge(2, 160, 30, [1, 3, 4]), hexc('#7a9cc8'), hexc('#a8c4e8'))
# monte Fuji
dd = ImageDraw.Draw(far)
for dx in (0,):
    dd.polygon([(150, 200), (230, 60), (250, 60), (330, 200)], fill=hexc('#5f7fb0'))
    dd.polygon([(205, 105), (230, 60), (250, 60), (275, 105), (262, 98), (250, 110), (238, 96), (222, 108)], fill=hexc('#f4f8ff'))
fill_ridge(far, ridge(3, 205, 14, [2, 5, 7]), hexc('#6b9a7a'), hexc('#8cbf9a'))
save(far, "Backgrounds/l1_far.png")
save(bamboo_layer(4), "Backgrounds/l1_near.png")

# Nivel 2 - Montana Kaze (atardecer)
sky = gradient(hexc('#3a2a6a'), hexc('#ff9a52'))
d = ImageDraw.Draw(sky); d.ellipse([190, 150, 290, 250], fill=hexc('#ffd27a')); d.ellipse([200, 160, 280, 240], fill=hexc('#fff0b0'))
clouds(sky, 5, hexc('#e87a6a'), n=6, ymin=60, ymax=140)
save(sky, "Backgrounds/l2_sky.png")
far = new(W, H)
fill_ridge(far, ridge(6, 140, 55, [1, 2, 5, 9]), hexc('#5a3a6a'), hexc('#8a5a8a'), snow=(115, hexc('#e8d0e0')))
fill_ridge(far, ridge(7, 200, 25, [2, 3, 7]), hexc('#3e2a52'), hexc('#5e4072'))
save(far, "Backgrounds/l2_far.png")
save(pines_layer(8, hexc('#2a1a36'), hexc('#1e1228')), "Backgrounds/l2_near.png")

# Nivel 3 - Castillo del Shogun (noche)
sky = gradient(hexc('#070a1e'), hexc('#2a2050'))
stars(sky, 9)
moon(sky, 380, 70, 34, hexc('#f4e8c8'), hexc('#3a3468'))
save(sky, "Backgrounds/l3_sky.png")
far = new(W, H)
fill_ridge(far, ridge(10, 190, 30, [1, 3, 4]), hexc('#1a1838'), hexc('#2c2a58'))
castle_silhouette(far, 140, 190, hexc('#121028'), hexc('#ffb040'))
save(far, "Backgrounds/l3_far.png")
near = new(W, H); d = ImageDraw.Draw(near)
for x in range(W):
    d.line([x, 205, x, H], fill=hexc('#0e0c1e'))
for i in range(8):
    x = i * 60 + 10
    d.polygon([(x - 8, 205), (x + 4, 190), (x + 44, 190), (x + 56, 205)], fill=hexc('#0e0c1e'))
    d.rectangle([x + 22, 206, x + 27, 216], fill=hexc('#ff4a2a'))
    d.rectangle([x + 23, 207, x + 26, 215], fill=hexc('#ffa040'))
save(near, "Backgrounds/l3_near.png")
print("ok")
