"""Genera los mapas de los 3 niveles (LevelData.cs) y una vista previa PNG.
y = 0 es la fila de abajo. El suelo normal ocupa y=0..1 (el jugador pisa y=2)."""
import sys
from PIL import Image, ImageDraw

H = 15

class Map:
    def __init__(self, w):
        self.w = w
        self.g = [[' '] * w for _ in range(H)]
    def put(self, x, y, ch):
        if 0 <= x < self.w and 0 <= y < H:
            self.g[y][x] = ch
    def ground(self, x0, x1, h=2):
        for x in range(x0, x1 + 1):
            for y in range(h):
                self.put(x, y, '#')
    def row(self, x0, x1, y, ch):
        for x in range(x0, x1 + 1):
            self.put(x, y, ch)
    def text(self, x, y, s):
        for i, ch in enumerate(s):
            if ch != '.':
                self.put(x + i, y, ch)
    def stairs(self, x0, n, base=2, up=True, ch='S'):
        for i in range(n):
            hgt = i + 1 if up else n - i
            for k in range(hgt):
                self.put(x0 + i, base + k, ch)
    def rows(self):
        return ["".join(self.g[y]) for y in range(H - 1, -1, -1)]

# ------------------------------------------------------------- NIVEL 1: Aldea del Bambu
def level1():
    m = Map(200)
    m.ground(0, 30)
    m.put(3, 2, 'P')
    m.put(1, 2, 'L')
    m.text(8, 2, 'ooo')
    m.text(12, 5, '?B?B?')
    m.put(14, 5, 'h')
    m.text(13, 9, '.?.')
    m.put(22, 2, 'R')
    # foso 1
    m.ground(34, 60)
    m.text(31, 5, 'ooo')
    m.stairs(38, 3)
    m.put(42, 4, 'o'); m.put(43, 5, 'o'); m.put(44, 4, 'o')
    m.put(46, 6, 'Y')
    m.text(50, 2, '^^^')
    m.text(49, 6, 'BB?BB')
    m.put(57, 2, 'R')
    # foso 2 con plataformas
    m.text(62, 4, 'SS')
    m.ground(66, 95)
    m.text(62, 6, 'oo')
    m.put(68, 2, 'L')
    m.stairs(72, 2)
    m.put(73, 4, '<')            # torreta en lo alto de la escalera
    m.text(78, 5, '??')
    m.put(83, 2, 'R'); m.put(88, 2, 'R')
    m.text(84, 6, 'ooooo')
    m.put(92, 2, 'K')
    # puente que se cae
    m.row(96, 103, 1, '=')
    m.text(97, 4, 'oooooo')
    m.ground(104, 140)
    m.put(108, 5, 'Y'); m.put(116, 7, 'Y')
    m.text(112, 2, '^^')
    m.text(118, 5, 'B?B')
    m.text(117, 9, 'SSSSS')
    m.text(118, 10, 'ooo')
    m.put(124, 2, 'R')
    m.text(128, 2, '^^^')
    m.stairs(132, 4)
    m.put(135, 6, 'o')
    m.put(139, 2, 'R')
    # foso con plataforma movil
    m.put(143, 3, 'M')
    m.ground(150, 199)
    m.put(152, 2, 'L')
    m.put(155, 2, '>')
    m.text(158, 5, '?h?')
    m.put(162, 2, 'R'); m.put(166, 6, 'Y')
    m.text(169, 2, '^^')
    m.stairs(174, 6)
    m.row(180, 181, 2, 'S'); m.row(180, 181, 3, 'S'); m.row(180, 181, 4, 'S'); m.row(180, 181, 5, 'S'); m.row(180, 181, 6, 'S'); m.row(180, 181, 7, 'S')
    m.text(176, 10, 'ooooo')
    m.put(190, 2, 'G')
    m.put(196, 2, 'L')
    return m

# ------------------------------------------------------------- NIVEL 2: Paso de la Montana Kaze
def level2():
    m = Map(215)
    m.ground(0, 22)
    m.put(3, 2, 'P')
    m.text(9, 5, '?B?')
    m.put(17, 2, 'N')
    m.ground(27, 45, 3)
    m.text(24, 6, 'ooo')
    m.put(31, 6, 'F')            # barra de fuego
    m.put(38, 3, 'R')
    m.text(41, 3, '^^')
    # gran foso con plataformas moviles
    m.put(48, 4, 'M')
    m.put(56, 5, 'V')
    m.put(62, 4, 'M')
    m.ground(68, 92)
    m.text(50, 8, 'ooo'); m.text(60, 9, 'ooo')
    m.put(70, 2, 'N')
    m.stairs(74, 3)
    m.put(76, 5, '<')
    m.text(80, 6, 'B?B?B')
    m.put(84, 9, 'Y')
    m.text(86, 2, '^^^')
    m.put(90, 2, 'K')
    m.row(93, 99, 1, '=')
    m.ground(100, 130)
    m.put(104, 2, 'N'); m.put(110, 2, 'R')
    m.put(113, 6, 'F')
    m.text(117, 2, '^^^^')
    m.text(116, 6, 'oooooo')
    m.put(124, 2, 'N')
    m.stairs(126, 4)
    # zona alta: escalones flotantes
    m.text(133, 5, 'SS'); m.text(137, 7, 'SS'); m.text(141, 9, 'SSS')
    m.text(141, 10, 'oo')
    m.put(146, 6, 'Y')
    m.text(147, 7, 'SS'); m.text(151, 5, 'SS')
    m.ground(155, 175)
    m.put(157, 2, '>')
    m.put(162, 2, 'N'); m.put(165, 2, 'R')
    m.text(162, 5, '?h?')
    m.put(170, 7, 'Y')
    m.row(176, 182, 1, '=')
    m.ground(183, 214)
    m.put(186, 6, 'F')
    m.text(190, 2, '^^')
    m.put(194, 2, 'N')
    m.stairs(197, 4)
    m.text(203, 2, '.')
    m.put(207, 2, 'G')
    return m

# ------------------------------------------------------------- NIVEL 3: Castillo del Shogun + jefe
def level3():
    m = Map(190)
    m.ground(0, 24)
    m.put(3, 2, 'P'); m.put(1, 2, 'L')
    m.text(8, 5, '?h?')
    m.put(14, 2, 'R'); m.put(19, 2, 'N')
    m.put(22, 6, 'F')
    m.ground(28, 50)
    m.text(25, 6, 'ooo')
    m.text(31, 2, '^^^')
    m.put(36, 2, '<')            # dispara hacia el inicio
    m.put(36, 3, 'S')
    m.put(41, 6, 'Y'); m.put(46, 2, 'N')
    m.stairs(47, 3)
    m.put(54, 4, 'M')
    m.ground(61, 92)
    m.put(63, 2, 'R')
    m.put(68, 6, 'F'); m.put(76, 6, 'F')
    m.text(71, 2, '^^^^')
    m.text(70, 9, 'ooooo')
    m.put(82, 2, 'N'); m.put(86, 7, 'Y')
    m.put(90, 2, 'K')
    m.row(93, 100, 1, '=')
    m.ground(101, 150)
    m.put(104, 2, '>')
    m.text(109, 5, 'B?B')
    m.put(113, 2, 'R'); m.put(117, 2, 'N'); m.put(120, 6, 'Y')
    m.text(124, 2, '^^^')
    m.put(130, 6, 'F')
    m.text(134, 5, 'h')
    m.put(138, 2, 'N')
    m.put(144, 2, 'L')
    m.put(147, 2, 'K')
    # arena del jefe
    m.ground(151, 189)
    m.put(155, 2, 'A')
    for y in range(2, H):
        m.put(189, y, 'S')
    m.text(164, 5, 'WWW'); m.text(177, 5, 'WWW')   # plataformas de un solo sentido
    m.text(171, 6, 'h')   # corazon de ayuda en la arena (se alcanza desde el suelo)
    m.put(182, 2, 'X')
    return m

levels = [level1(), level2(), level3()]

# ---- C#
out = sys.argv[1]
names = ["Aldea del Bambú", "Paso de la Montaña Kaze", "Castillo del Shogun"]
with open(out, "w", encoding="utf-8") as f:
    f.write("// Archivo generado: mapas de los niveles (cada caracter es un tile de 1x1).\n")
    f.write("// Leyenda: # suelo  S bloque de piedra  B caja rompible  ? bloque moneda  h bloque corazon\n")
    f.write("//  o moneda  H corazon  ^ pinchos  R ronin  N shinobi  Y yokai-farol  < > torreta de shuriken\n")
    f.write("//  F barra de fuego  = puente que se cae  M plataforma movil horizontal  V vertical\n")
    f.write("//  W plataforma de madera (un solo sentido)  K checkpoint  P jugador  G meta (torii)  A inicio de arena del jefe  X jefe  L farol decorativo\n")
    f.write("public static class LevelData\n{\n")
    f.write("    public static readonly string[] Names = {\n")
    for n in names:
        f.write(f'        "{n}",\n')
    f.write("    };\n\n    public static readonly int[] TimeLimit = { 240, 260, 300 };\n\n")
    f.write("    public static readonly string[][] Maps = {\n")
    for m in levels:
        f.write("        new string[] {\n")
        for r in m.rows():
            f.write(f'            "{r}",\n')
        f.write("        },\n")
    f.write("    };\n}\n")

# ---- preview
cols = {'#': (120, 80, 40), 'S': (140, 140, 150), 'B': (190, 130, 60), '?': (240, 190, 40), 'h': (240, 90, 120),
        'o': (255, 230, 0), '^': (60, 200, 60), 'R': (220, 40, 40), 'N': (120, 40, 200), 'Y': (255, 120, 0),
        '<': (90, 90, 90), '>': (90, 90, 90), 'F': (40, 120, 255), '=': (150, 100, 50), 'M': (200, 160, 100),
        'V': (200, 160, 100), 'K': (255, 255, 255), 'P': (0, 255, 255), 'G': (255, 0, 0), 'A': (255, 0, 255),
        'X': (100, 0, 100), 'L': (180, 180, 160), 'H': (255, 0, 80)}
for i, m in enumerate(levels):
    s = 6
    im = Image.new("RGB", (m.w * s, H * s), (150, 200, 240))
    d = ImageDraw.Draw(im)
    for y in range(H):
        for x in range(m.w):
            ch = m.g[y][x]
            if ch in cols:
                yy = (H - 1 - y) * s
                d.rectangle([x * s, yy, x * s + s - 1, yy + s - 1], fill=cols[ch])
    for x in range(0, m.w, 10):
        d.line([x * s, 0, x * s, 3], fill=(0, 0, 0))
    im.save(f"/home/claude/work/level{i+1}.png")
print("ok")
